using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
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
    /// Mock implementation of IUdpCommunicationService for testing
    /// </summary>
    private class MockUdpCommunicationService : IUdpCommunicationService
    {
        public List<byte[]> SentMessages { get; } = new();
        public bool IsConnected => true;
        public string? LocalIPAddress => "192.168.1.100";

        public event EventHandler<UdpDataReceivedEventArgs>? DataReceived;
        public event EventHandler<ModuleConnectionEventArgs>? ModuleConnectionChanged;

        public void SendToModules(byte[] data)
        {
            SentMessages.Add((byte[])data.Clone());
        }

        public void SendHelloPacket() { }

        public bool IsModuleHelloOk(ModuleType moduleType) => true;

        public bool IsModuleDataOk(ModuleType moduleType) => true;

        public System.Threading.Tasks.Task StartAsync() => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task StopAsync() => System.Threading.Tasks.Task.CompletedTask;
    }

    [Fact]
    public void Update_WithStanleyAlgorithm_CallsStanleyService()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockUdp = new MockUdpCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockUdp,
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
        var mockUdp = new MockUdpCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockUdp,
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
        var mockUdp = new MockUdpCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockUdp,
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
    public void Update_SendsPGN254Message_WithCorrectFormat()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService();
        var mockUdp = new MockUdpCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockUdp,
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

        // Assert - Verify PGN 254 message was sent
        Assert.Single(mockUdp.SentMessages);
        var message = mockUdp.SentMessages[0];

        // Verify message structure
        Assert.Equal(0x80, message[0]); // Header byte 1
        Assert.Equal(0x81, message[1]); // Header byte 2
        Assert.Equal(0x7F, message[2]); // Source
        Assert.Equal(254, message[3]);  // PGN 254
        Assert.Equal(10, message[4]);   // Length (10 data bytes)

        // Verify speed field (bytes 5-6): speed * 10
        int speedValue = (message[5] << 8) | message[6];
        Assert.Equal(50, speedValue); // 5.0 m/s * 10 = 50

        // Verify status byte (byte 7): 1 = on
        Assert.Equal(1, message[7]);

        // Verify cross-track error field (bytes 10-11): in mm
        short xteValue = (short)((message[10] << 8) | message[11]);
        Assert.Equal(500, xteValue); // 0.5m = 500mm

        // Verify CRC is present at byte 14
        Assert.True(message.Length >= 15);
    }

    [Fact]
    public void Update_PublishesSteeringUpdatedEvent()
    {
        // Arrange
        var mockStanley = new MockStanleySteeringService();
        var mockPurePursuit = new MockPurePursuitService();
        var mockLookAhead = new MockLookAheadDistanceService { FixedDistance = 6.0 };
        var mockUdp = new MockUdpCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockUdp,
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
        var mockUdp = new MockUdpCommunicationService();
        var vehicleConfig = new VehicleConfiguration();

        var coordinator = new SteeringCoordinatorService(
            mockStanley,
            mockPurePursuit,
            mockLookAhead,
            mockUdp,
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
