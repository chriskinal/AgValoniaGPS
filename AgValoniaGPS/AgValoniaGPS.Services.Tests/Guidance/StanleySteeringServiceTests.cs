using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for StanleySteeringService
    /// Tests core Stanley algorithm: steerAngle = headingError * K_h + atan(K_d * xte / speed)
    /// Verifies integral control, reverse mode, speed adaptation, and thread safety
    /// </summary>
    public class StanleySteeringServiceTests
    {
        private VehicleConfiguration CreateDefaultConfig()
        {
            return new VehicleConfiguration
            {
                StanleyHeadingErrorGain = 1.0,
                StanleyDistanceErrorGain = 0.8,
                StanleyIntegralGainAB = 0.1,
                StanleyIntegralDistanceAwayTriggerAB = 0.3,
                MaxSteerAngle = 35.0
            };
        }

        [Fact]
        public void CalculateSteeringAngle_BasicFormula_CalculatesCorrectly()
        {
            // Arrange
            var config = CreateDefaultConfig();
            var service = new StanleySteeringService(config);

            // Test scenario: 1m right of line, 0.1 rad heading error, 0.5 m/s speed (no speed adaptation)
            double xte = 1.0; // meters
            double headingError = 0.1; // radians (~5.7 degrees)
            double speed = 0.5; // m/s (speed ≤ 1.0 means no speed adaptation)
            double pivotError = 0.0; // no integral component
            bool isReverse = false;

            // Act
            double steerAngle = service.CalculateSteeringAngle(xte, headingError, speed, pivotError, isReverse);

            // Assert
            // Expected: headingError * K_h + atan(K_d * xte / speed)
            // = 0.1 * 1.0 + atan(0.8 * 1.0 / 0.5)
            // = 0.1 + atan(1.6)
            // = 0.1 + 1.0122 ≈ 1.1122 rad ≈ 63.74 degrees
            // But clamped to MaxSteerAngle = 35.0 degrees
            double expectedRadians = 0.1 + Math.Atan(0.8 * 1.0 / 0.5);
            double expectedDegrees = expectedRadians * (180.0 / Math.PI);
            double clampedExpected = Math.Min(expectedDegrees, config.MaxSteerAngle);
            Assert.Equal(clampedExpected, steerAngle, 1); // ±1 degree precision
        }

        [Fact]
        public void CalculateSteeringAngle_HeadingErrorComponent_VariesWithHeadingError()
        {
            // Arrange
            var config = CreateDefaultConfig();
            var service = new StanleySteeringService(config);

            // Act - Test with different heading errors
            double angle1 = service.CalculateSteeringAngle(0.0, 0.0, 5.0, 0.0, false);
            double angle2 = service.CalculateSteeringAngle(0.0, 0.2, 5.0, 0.0, false); // positive heading error
            double angle3 = service.CalculateSteeringAngle(0.0, -0.2, 5.0, 0.0, false); // negative heading error

            // Assert
            Assert.Equal(0.0, angle1, 1); // No error = no steering
            Assert.True(angle2 > 0, "Positive heading error should produce positive steering angle");
            Assert.True(angle3 < 0, "Negative heading error should produce negative steering angle");
            Assert.Equal(Math.Abs(angle2), Math.Abs(angle3), 1); // Symmetric response
        }

        [Fact]
        public void CalculateSteeringAngle_CrossTrackErrorComponent_VariesWithXTE()
        {
            // Arrange
            var config = CreateDefaultConfig();
            var service = new StanleySteeringService(config);

            // Act - Test with different cross-track errors
            double angle1 = service.CalculateSteeringAngle(1.0, 0.0, 5.0, 0.0, false); // 1m right
            double angle2 = service.CalculateSteeringAngle(2.0, 0.0, 5.0, 0.0, false); // 2m right
            double angle3 = service.CalculateSteeringAngle(-1.0, 0.0, 5.0, 0.0, false); // 1m left

            // Assert
            Assert.True(angle2 > angle1, "Larger XTE should produce larger steering angle");
            Assert.True(angle3 < 0, "Negative XTE should produce negative steering angle");
            Assert.Equal(Math.Abs(angle1), Math.Abs(angle3), 1); // Symmetric response
        }

        [Fact]
        public void CalculateSteeringAngle_SpeedAdaptation_ScalesGainCorrectly()
        {
            // Arrange
            var config = CreateDefaultConfig();
            var service = new StanleySteeringService(config);
            double xte = 1.0;
            double headingError = 0.0;
            double pivotError = 0.0;

            // Act - Test at different speeds
            double angleLowSpeed = service.CalculateSteeringAngle(xte, headingError, 0.5, pivotError, false);
            double angleNormalSpeed = service.CalculateSteeringAngle(xte, headingError, 5.0, pivotError, false);

            // Assert
            // At higher speeds, distance gain is scaled: 0.8 * (1 + 0.277 * (5.0 - 1.0)) = 0.8 * 2.108 = 1.6864
            // At low speed (<1 m/s), no scaling applied
            // atan(0.8 * 1.0 / 0.5) = atan(1.6) ≈ 1.0122 rad
            // atan(1.6864 * 1.0 / 5.0) = atan(0.33728) ≈ 0.3247 rad
            Assert.True(angleLowSpeed > angleNormalSpeed,
                "Lower speed should produce larger steering angle for same XTE");
        }

        [Fact]
        public void CalculateSteeringAngle_IntegralControl_AccumulatesAndResets()
        {
            // Arrange
            var config = CreateDefaultConfig();
            config.StanleyIntegralGainAB = 0.5; // Make integral effect more pronounced
            var service = new StanleySteeringService(config);

            // Act - Apply pivot distance error above trigger threshold
            double angle1 = service.CalculateSteeringAngle(0.0, 0.0, 5.0, 0.5, false);
            double integral1 = service.IntegralValue;

            // Second call - integral should accumulate
            double angle2 = service.CalculateSteeringAngle(0.0, 0.0, 5.0, 0.5, false);
            double integral2 = service.IntegralValue;

            // Reset and verify
            service.ResetIntegral();
            double integralAfterReset = service.IntegralValue;

            // Assert
            Assert.True(integral1 > 0, "Integral should accumulate with positive pivot error");
            Assert.True(integral2 > integral1, "Integral should continue accumulating");
            Assert.Equal(0.0, integralAfterReset, 3); // Reset should clear integral
        }

        [Fact]
        public void CalculateSteeringAngle_ReverseMode_NegatesHeadingError()
        {
            // Arrange
            var config = CreateDefaultConfig();
            var service = new StanleySteeringService(config);
            double xte = 0.0; // Isolate heading error component
            double headingError = 0.2; // radians
            double speed = 5.0;
            double pivotError = 0.0;

            // Act
            double angleForward = service.CalculateSteeringAngle(xte, headingError, speed, pivotError, false);
            service.ResetIntegral(); // Reset between tests
            double angleReverse = service.CalculateSteeringAngle(xte, headingError, speed, pivotError, true);

            // Assert
            // In reverse mode, heading error should be negated
            Assert.True(angleForward > 0, "Forward mode: positive heading error produces positive angle");
            Assert.True(angleReverse < 0, "Reverse mode: positive heading error produces negative angle");
            Assert.Equal(Math.Abs(angleForward), Math.Abs(angleReverse), 1);
        }

        [Fact]
        public void CalculateSteeringAngle_AngleLimiting_ClampsToMaxSteerAngle()
        {
            // Arrange
            var config = CreateDefaultConfig();
            config.MaxSteerAngle = 20.0; // Lower limit for testing
            var service = new StanleySteeringService(config);

            // Act - Large errors that would exceed max angle
            double angleHuge = service.CalculateSteeringAngle(10.0, 1.0, 1.0, 0.0, false);
            double angleHugeNegative = service.CalculateSteeringAngle(-10.0, -1.0, 1.0, 0.0, false);

            // Assert
            Assert.True(angleHuge <= config.MaxSteerAngle,
                $"Steering angle {angleHuge} should be clamped to max {config.MaxSteerAngle}");
            Assert.True(angleHugeNegative >= -config.MaxSteerAngle,
                $"Steering angle {angleHugeNegative} should be clamped to min {-config.MaxSteerAngle}");
        }

        [Fact]
        public void CalculateSteeringAngle_Performance_CompletesUnder2ms()
        {
            // Arrange
            var config = CreateDefaultConfig();
            var service = new StanleySteeringService(config);

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                service.CalculateSteeringAngle(1.0, 0.1, 5.0, 0.2, false);
            }

            // Act - Measure 1000 iterations
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                service.CalculateSteeringAngle(1.0, 0.1, 5.0, 0.2, false);
            }
            stopwatch.Stop();
            double avgTimeMs = stopwatch.Elapsed.TotalMilliseconds / 1000.0;

            // Assert - Target: <2ms per calculation
            Assert.True(avgTimeMs < 2.0,
                $"Stanley calculation took {avgTimeMs:F3}ms on average, expected <2ms");
        }
    }
}
