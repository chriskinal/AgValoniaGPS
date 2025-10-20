using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.Services.Interfaces;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance;

/// <summary>
/// Unit tests for SteeringCoordinatorService:
/// - Algorithm routing (Stanley vs Pure Pursuit selection)
/// - PGN 254 message format verification
/// - Real-time algorithm switching
/// - Integral reset when switching algorithms
/// - Event publishing (SteeringUpdated)
/// - Performance requirements (<5ms for Update())
/// </summary>
public class SteeringCoordinatorServiceTests
{
    /// <summary>
    /// Mock implementation of IStanleySteeringService for testing
    /// </summary>
    private class MockStanleySteeringService : IStanleySteeringService
    {
        public double LastCalculatedAngle { get; private set; }
        public int CalculateCallCount { get; private set; }
        public int ResetCallCount { get; private set; }
        public double IntegralValue { get; private set; }

        public double CalculateSteeringAngle(
            double crossTrackError,
            double headingError,
            double speed,
            double pivotDistanceError,
            bool isReverse)
        {
            CalculateCallCount++;
            // Simple mock calculation: combine XTE and heading error
            LastCalculatedAngle = headingError + (crossTrackError * 0.5);
            return LastCalculatedAngle;
        }

        public void ResetIntegral()
        {
            ResetCallCount++;
            IntegralValue = 0.0;
        }
    }

    /// <summary>
    /// Mock implementation of IPurePursuitService for testing
    /// </summary>
    private class MockPurePursuitService : IPurePursuitService
    {
        public double LastCalculatedAngle { get; private set; }
        public int CalculateCallCount { get; private set; }
        public int ResetCallCount { get; private set; }
        public double IntegralValue { get; private set; }

        public double CalculateSteeringAngle(
            Position3D steerAxlePosition,
            Position3D goalPoint,
            double speed,
            double pivotDistanceError)
        {
            CalculateCallCount++;
            // Simple mock calculation based on goal point offset
            double dx = goalPoint.Easting - steerAxlePosition.Easting;
            LastCalculatedAngle = Math.Atan2(dx, 10.0) * (180.0 / Math.PI);
            return LastCalculatedAngle;
        }

        public void ResetIntegral()
        {
            ResetCallCount++;
            IntegralValue = 0.0;
        }
    }

    /// <summary>
    /// Mock implementation of ILookAheadDistanceService for testing
    /// </summary>
    private class MockLookAheadDistanceService : ILookAheadDistanceService
    {
        public double FixedDistance { get; set; } = 5.0;
        public LookAheadMode Mode { get; set; } = LookAheadMode.ToolWidthMultiplier;

        public double CalculateLookAheadDistance(
            double speed,
            double crossTrackError,
            double guidanceLineCurvature,
            VehicleType vehicleType,
            bool isAutoSteerActive)
        {
            return FixedDistance;
        }
    }

    /// <summary>
    /// Mock implementation of IAutoSteerCommunicationService for testing
    /// </summary>
    private class MockAutoSteerCommunicationService : IAutoSteerCommunicationService
    {
        public List<object[]> SentCommands { get; } = new();
        public AutoSteerFeedback? CurrentFeedback => null;
        public double ActualWheelAngle => 0.0;
        public byte[] SwitchStates => Array.Empty<byte>();

        public event EventHandler<AutoSteerFeedbackEventArgs>? FeedbackReceived;
        public event EventHandler<AutoSteerSwitchStateChangedEventArgs>? SwitchStateChanged;

        public void SendSteeringCommand(double speedMph, double steerAngle, int xteErrorMm, bool isActive)
        {
            SentCommands.Add(new object[] { speedMph, steerAngle, xteErrorMm, isActive });
        }

        public void SendSettings(byte pwmDrive, byte minPwm, float proportionalGain, byte highPwm, float lowSpeedPwm)
        {
            // Not needed for these tests
        }

        public void SendConfiguration(byte configFlags)
        {
            // Not needed for these tests
        }
    }

    [Fact]
    public void Update_WithStanleyAlgorithm_CallsStanleyService()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockAutoSteer = new MockAutoSteerCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockAutoSteer,
            vehicleConfig
        );

        coordinator.ActiveAlgorithm = SteeringAlgorithm.Stanley;

        var pivotPos = new Position3D(100.0, 200.0, 0.0);
        var steerPos = new Position3D(102.0, 200.0, 0.0);
        var guidanceResult = new GuidanceLineResult
        {
            CrossTrackError = 0.5,
            HeadingError = 0.1,
            ClosestPoint = new Position { Easting = 100.0, Northing = 200.0 }
        };

        // Act
        coordinator.Update(pivotPos, steerPos, guidanceResult, 5.0, 0.0, true);

        // Assert
        Assert.Equal(1, mockStanley.CalculateCallCount);
        Assert.Equal(0, mockPurePursuit.CalculateCallCount);
        Assert.True(coordinator.CurrentSteeringAngle != 0.0);
    }

    [Fact]
    public void Update_WithPurePursuitAlgorithm_CallsPurePursuitService()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockAutoSteer = new MockAutoSteerCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockAutoSteer,
            vehicleConfig
        );

        coordinator.ActiveAlgorithm = SteeringAlgorithm.PurePursuit;

        var pivotPos = new Position3D(100.0, 200.0, 0.0);
        var steerPos = new Position3D(102.0, 200.0, 0.0);
        var guidanceResult = new GuidanceLineResult
        {
            CrossTrackError = 0.5,
            HeadingError = 0.1,
            ClosestPoint = new Position { Easting = 100.0, Northing = 200.0 }
        };

        // Act
        coordinator.Update(pivotPos, steerPos, guidanceResult, 5.0, 0.0, true);

        // Assert
        Assert.Equal(0, mockStanley.CalculateCallCount);
        Assert.Equal(1, mockPurePursuit.CalculateCallCount);
        Assert.True(coordinator.CurrentSteeringAngle != 0.0);
    }

    [Fact]
    public void AlgorithmSwitch_ResetsIntegralsInBothServices()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockAutoSteer = new MockAutoSteerCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockAutoSteer,
            vehicleConfig
        );

        coordinator.ActiveAlgorithm = SteeringAlgorithm.Stanley;

        // Act - Switch algorithm
        coordinator.ActiveAlgorithm = SteeringAlgorithm.PurePursuit;

        // Assert - Both services should have their integrals reset
        Assert.Equal(1, mockStanley.ResetCallCount);
        Assert.Equal(1, mockPurePursuit.ResetCallCount);
    }

    [Fact]
    public void Update_SendsSteeringCommand_WithCorrectParameters()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockAutoSteer = new MockAutoSteerCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockAutoSteer,
            vehicleConfig
        );

        coordinator.ActiveAlgorithm = SteeringAlgorithm.Stanley;

        var pivotPos = new Position3D(100.0, 200.0, 0.0);
        var steerPos = new Position3D(102.0, 200.0, 0.0);
        var guidanceResult = new GuidanceLineResult
        {
            CrossTrackError = 0.5,
            HeadingError = 0.1,
            ClosestPoint = new Position { Easting = 100.0, Northing = 200.0 }
        };

        // Act
        coordinator.Update(pivotPos, steerPos, guidanceResult, 5.0, 0.0, true);

        // Assert - Verify steering command was sent
        Assert.Single(mockAutoSteer.SentCommands);
        var command = mockAutoSteer.SentCommands[0];

        // Verify parameters
        Assert.True((double)command[0] > 0); // speedMph should be > 0
        Assert.True((bool)command[3]); // isActive should be true
        Assert.Equal(500, (int)command[2]); // XTE: 0.5m = 500mm
    }

    [Fact]
    public void Update_PublishesSteeringUpdatedEvent()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService { FixedDistance = 6.0 };
        var mockAutoSteer = new MockAutoSteerCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockAutoSteer,
            vehicleConfig
        );

        coordinator.ActiveAlgorithm = SteeringAlgorithm.Stanley;

        SteeringUpdateEventArgs? capturedEvent = null;
        coordinator.SteeringUpdated += (sender, args) => capturedEvent = args;

        var pivotPos = new Position3D(100.0, 200.0, 0.0);
        var steerPos = new Position3D(102.0, 200.0, 0.0);
        var guidanceResult = new GuidanceLineResult
        {
            CrossTrackError = 0.5,
            HeadingError = 0.1,
            ClosestPoint = new Position { Easting = 100.0, Northing = 200.0 }
        };

        // Act
        coordinator.Update(pivotPos, steerPos, guidanceResult, 5.0, 0.0, true);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(SteeringAlgorithm.Stanley, capturedEvent.Algorithm);
        Assert.Equal(0.5, capturedEvent.CrossTrackError);
        Assert.Equal(6.0, capturedEvent.LookAheadDistance);
        Assert.True(capturedEvent.Timestamp > DateTime.MinValue);
    }

    [Fact]
    public void Update_Performance_CompletesInLessThan5Milliseconds()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockAutoSteer = new MockAutoSteerCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockAutoSteer,
            vehicleConfig
        );

        coordinator.ActiveAlgorithm = SteeringAlgorithm.Stanley;

        var pivotPos = new Position3D(100.0, 200.0, 0.0);
        var steerPos = new Position3D(102.0, 200.0, 0.0);
        var guidanceResult = new GuidanceLineResult
        {
            CrossTrackError = 0.5,
            HeadingError = 0.1,
            ClosestPoint = new Position { Easting = 100.0, Northing = 200.0 }
        };

        // Warmup
        coordinator.Update(pivotPos, steerPos, guidanceResult, 5.0, 0.0, true);

        // Act - Measure 100 iterations
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            coordinator.Update(pivotPos, steerPos, guidanceResult, 5.0, 0.0, true);
        }
        stopwatch.Stop();

        // Assert - Average should be well under 5ms per iteration
        double averageMs = stopwatch.ElapsedMilliseconds / 100.0;
        Assert.True(averageMs < 5.0, $"Average execution time {averageMs:F2}ms exceeds 5ms target");
    }
}
