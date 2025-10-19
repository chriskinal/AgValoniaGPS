using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for PurePursuitService core functionality:
    /// - Goal point calculation on guidance lines
    /// - Alpha angle calculation (vehicle heading to goal point)
    /// - Curvature formula: curvature = 2 * sin(alpha) / lookAheadDistance
    /// - Steering angle formula: steerAngle = atan(curvature * wheelbase)
    /// - Integral control accumulation and reset
    /// - Zero look-ahead protection
    /// - Angle limiting (clamp to ±maxSteerAngle)
    /// </summary>
    public class PurePursuitServiceTests
    {
        private readonly VehicleConfiguration _testConfig;

        public PurePursuitServiceTests()
        {
            _testConfig = new VehicleConfiguration
            {
                Wheelbase = 2.5, // meters
                MaxSteerAngle = 35.0, // degrees
                PurePursuitIntegralGain = 0.1
            };
        }

        [Fact]
        public void CalculateSteeringAngle_StraightAhead_ReturnsZeroAngle()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0); // Heading north
            var goalPoint = new Position3D(100.0, 105.0, 0.0); // 5m ahead, directly north
            double speed = 2.0;
            double pivotDistanceError = 0.0;

            // Act
            double steerAngle = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Assert - Goal point directly ahead should result in ~0 steering angle
            Assert.True(Math.Abs(steerAngle) < 1.0,
                $"Expected near-zero steering angle for straight ahead, got {steerAngle:F3} degrees");
        }

        [Fact]
        public void CalculateSteeringAngle_GoalPointToRight_ReturnsPositiveAngle()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0); // Heading north
            var goalPoint = new Position3D(103.0, 104.0, 0.0); // 3m right, 4m ahead (53° to right)
            double speed = 2.0;
            double pivotDistanceError = 0.0;

            // Act
            double steerAngle = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Assert - Goal point to the right should result in positive steering angle
            Assert.True(steerAngle > 0,
                $"Expected positive steering angle for right turn, got {steerAngle:F3} degrees");
        }

        [Fact]
        public void CalculateSteeringAngle_GoalPointToLeft_ReturnsNegativeAngle()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0); // Heading north
            var goalPoint = new Position3D(97.0, 104.0, 0.0); // 3m left, 4m ahead
            double speed = 2.0;
            double pivotDistanceError = 0.0;

            // Act
            double steerAngle = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Assert - Goal point to the left should result in negative steering angle
            Assert.True(steerAngle < 0,
                $"Expected negative steering angle for left turn, got {steerAngle:F3} degrees");
        }

        [Fact]
        public void CalculateSteeringAngle_CurvatureFormula_IsCorrect()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(0.0, 0.0, Math.PI / 2); // Heading east
            var lookAheadDistance = 5.0; // meters

            // Goal point 30 degrees to the right of heading (using navigation coordinates)
            // Vehicle heading East (π/2), goal 30° right = π/2 + π/6 = 2π/3 (120° from North clockwise)
            double alpha = Math.PI / 6; // 30 degrees
            double goalHeading = Math.PI / 2 + alpha; // 120 degrees from North
            var goalPoint = new Position3D(
                lookAheadDistance * Math.Sin(goalHeading), // Easting = r * sin(heading)
                lookAheadDistance * Math.Cos(goalHeading), // Northing = r * cos(heading)
                0.0
            );

            double speed = 2.0;
            double pivotDistanceError = 0.0;

            // Act
            double steerAngle = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Expected curvature = 2 * sin(alpha) / lookAheadDistance
            double expectedCurvature = 2.0 * Math.Sin(alpha) / lookAheadDistance;
            // Expected steer angle = atan(curvature * wheelbase) in radians, then convert to degrees
            double expectedSteerAngleRad = Math.Atan(expectedCurvature * _testConfig.Wheelbase);
            double expectedSteerAngleDeg = expectedSteerAngleRad * 180.0 / Math.PI;

            // Assert - Steering angle should match Pure Pursuit formula
            Assert.Equal(expectedSteerAngleDeg, steerAngle, 1); // Within 1 degree
        }

        [Fact]
        public void CalculateSteeringAngle_ZeroLookAhead_ReturnsZero()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0);
            var goalPoint = new Position3D(100.0, 100.0, 0.0); // Same as steer position
            double speed = 2.0;
            double pivotDistanceError = 0.0;

            // Act
            double steerAngle = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Assert - Zero look-ahead should return 0
            Assert.Equal(0.0, steerAngle, 3);
        }

        [Fact]
        public void CalculateSteeringAngle_IntegralControl_Accumulates()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0);
            var goalPoint = new Position3D(100.0, 105.0, 0.0);
            double speed = 2.0;
            double pivotDistanceError = 0.5; // 0.5m error

            // Act - First call should accumulate integral
            double angle1 = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);
            double integral1 = service.IntegralValue;

            // Act - Second call should accumulate more
            double angle2 = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);
            double integral2 = service.IntegralValue;

            // Assert - Integral should accumulate
            Assert.True(Math.Abs(integral2) > Math.Abs(integral1),
                $"Integral should accumulate: first={integral1:F3}, second={integral2:F3}");
        }

        [Fact]
        public void ResetIntegral_ClearsAccumulatedValue()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0);
            var goalPoint = new Position3D(100.0, 105.0, 0.0);
            double speed = 2.0;
            double pivotDistanceError = 0.5;

            // Act - Accumulate some integral
            service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);
            service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Reset
            service.ResetIntegral();

            // Assert
            Assert.Equal(0.0, service.IntegralValue, 6);
        }

        [Fact]
        public void CalculateSteeringAngle_ExceedsMaxAngle_ClampedToLimit()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, 0.0); // Heading north

            // Goal point far to the right to create very large steering demand
            var goalPoint = new Position3D(105.0, 101.0, 0.0); // Almost perpendicular
            double speed = 2.0;
            double pivotDistanceError = 0.0;

            // Act
            double steerAngle = service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);

            // Assert - Should be clamped to ±MaxSteerAngle
            Assert.True(Math.Abs(steerAngle) <= _testConfig.MaxSteerAngle,
                $"Steering angle {steerAngle:F3} should be clamped to ±{_testConfig.MaxSteerAngle}");
        }

        [Fact]
        public void CalculateSteeringAngle_Performance_CompletesUnder3ms()
        {
            // Arrange
            var service = new PurePursuitService(_testConfig);
            var steerPosition = new Position3D(100.0, 100.0, Math.PI / 4);
            var goalPoint = new Position3D(105.0, 105.0, 0.0);
            double speed = 2.5;
            double pivotDistanceError = 0.2;

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);
            }

            // Act - Measure 1000 iterations to get average
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                service.CalculateSteeringAngle(steerPosition, goalPoint, speed, pivotDistanceError);
            }
            stopwatch.Stop();
            double avgTimeMs = stopwatch.Elapsed.TotalMilliseconds / 1000.0;

            // Assert - Should complete in <3ms for 100Hz capability
            Assert.True(avgTimeMs < 3.0,
                $"CalculateSteeringAngle took {avgTimeMs:F3}ms on average, expected <3ms");
        }
    }
}
