using System;
using System.Globalization;
using NUnit.Framework;
using AgValoniaGPS.Services.Display;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Display;

namespace AgValoniaGPS.Services.Tests.Display
{
    /// <summary>
    /// Tests for DisplayFormatterService basic formatters (speed, heading, distance).
    /// Focused on dynamic precision, unit switching, and InvariantCulture formatting.
    /// </summary>
    [TestFixture]
    public class DisplayFormatterServiceTests
    {
        private IDisplayFormatterService _formatter = null!;

        [SetUp]
        public void SetUp()
        {
            _formatter = new DisplayFormatterService();
        }

        #region Speed Formatting Tests

        /// <summary>
        /// Test speed formatter with dynamic precision: 1 decimal if > 2 km/h
        /// </summary>
        [Test]
        public void FormatSpeed_HighSpeed_Metric_UsesOneDecimal()
        {
            // Arrange: 5.56 m/s = 20.016 km/h (> 2 km/h)
            double speedMetersPerSecond = 5.56;

            // Act
            string result = _formatter.FormatSpeed(speedMetersPerSecond, UnitSystem.Metric);

            // Assert: Should use 1 decimal place
            Assert.That(result, Is.EqualTo("20.0"));
        }

        /// <summary>
        /// Test speed formatter with dynamic precision: 2 decimals if <= 2 km/h
        /// </summary>
        [Test]
        public void FormatSpeed_LowSpeed_Metric_UsesTwoDecimals()
        {
            // Arrange: 0.51 m/s = 1.836 km/h (<= 2 km/h)
            double speedMetersPerSecond = 0.51;

            // Act
            string result = _formatter.FormatSpeed(speedMetersPerSecond, UnitSystem.Metric);

            // Assert: Should use 2 decimal places
            Assert.That(result, Is.EqualTo("1.84"));
        }

        /// <summary>
        /// Test speed formatter with imperial units
        /// </summary>
        [Test]
        public void FormatSpeed_HighSpeed_Imperial_UsesCorrectConversion()
        {
            // Arrange: 8.94 m/s = ~20.0 mph
            double speedMetersPerSecond = 8.94;

            // Act
            string result = _formatter.FormatSpeed(speedMetersPerSecond, UnitSystem.Imperial);

            // Assert: Should convert to mph with 1 decimal
            Assert.That(result, Is.EqualTo("20.0"));
        }

        #endregion

        #region Heading Formatting Tests

        /// <summary>
        /// Test heading formatter rounds to nearest degree and includes ° symbol
        /// </summary>
        [Test]
        public void FormatHeading_RoundsToWholeDegree_WithDegreeSymbol()
        {
            // Arrange
            double heading = 45.5;

            // Act
            string result = _formatter.FormatHeading(heading);

            // Assert: Should round to 46 and include °
            Assert.That(result, Is.EqualTo("46°"));
        }

        /// <summary>
        /// Test heading formatter handles zero correctly
        /// </summary>
        [Test]
        public void FormatHeading_Zero_ReturnsZeroDegrees()
        {
            // Arrange
            double heading = 0.0;

            // Act
            string result = _formatter.FormatHeading(heading);

            // Assert
            Assert.That(result, Is.EqualTo("0°"));
        }

        #endregion

        #region Distance Formatting Tests

        /// <summary>
        /// Test distance formatter switches from meters to kilometers at 1000m
        /// </summary>
        [Test]
        public void FormatDistance_Metric_SwitchesFromMetersToKilometers()
        {
            // Arrange
            double distanceUnder1000 = 450.5; // Should be "450.5 m"
            double distanceOver1000 = 1250.0; // Should be "1.25 km"

            // Act
            string resultMeters = _formatter.FormatDistance(distanceUnder1000, UnitSystem.Metric);
            string resultKilometers = _formatter.FormatDistance(distanceOver1000, UnitSystem.Metric);

            // Assert
            Assert.That(resultMeters, Is.EqualTo("450.5 m"));
            Assert.That(resultKilometers, Is.EqualTo("1.25 km"));
        }

        /// <summary>
        /// Test distance formatter switches from feet to miles at 5280ft
        /// </summary>
        [Test]
        public void FormatDistance_Imperial_SwitchesFromFeetToMiles()
        {
            // Arrange
            double distance914m = 914.4;  // ~3000 ft (under threshold)
            double distance1610m = 1610.0; // ~5283 ft (over threshold, should be miles)

            // Act
            string resultFeet = _formatter.FormatDistance(distance914m, UnitSystem.Imperial);
            string resultMiles = _formatter.FormatDistance(distance1610m, UnitSystem.Imperial);

            // Assert: Feet with 0 decimals, miles with 2 decimals
            Assert.That(resultFeet, Is.EqualTo("3000 ft"));
            Assert.That(resultMiles, Is.EqualTo("1.00 mi"));
        }

        #endregion
    }
}
