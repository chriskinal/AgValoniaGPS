using System;
using System.Globalization;
using NUnit.Framework;
using AgValoniaGPS.Services.Display;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Display;

namespace AgValoniaGPS.Services.Tests.Display
{
    /// <summary>
    /// Edge case and boundary condition tests for DisplayFormatterService.
    /// Fills critical test coverage gaps including NaN, Infinity, negative values,
    /// exact boundary conditions, InvariantCulture verification, and unit conversion accuracy.
    /// </summary>
    [TestFixture]
    public class DisplayFormatterServiceEdgeCaseTests
    {
        private IDisplayFormatterService _formatter = null!;

        [SetUp]
        public void SetUp()
        {
            _formatter = new DisplayFormatterService();
        }

        #region Edge Case Tests: NaN and Infinity

        /// <summary>
        /// Test all formatters return safe defaults for NaN input (never throw exceptions).
        /// Critical for robust error handling.
        /// </summary>
        [Test]
        public void FormatSpeed_NaN_ReturnsSafeDefault()
        {
            // Arrange
            double nanSpeed = double.NaN;

            // Act
            string result = _formatter.FormatSpeed(nanSpeed, UnitSystem.Metric);

            // Assert: Should return "0.0" not throw exception
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Not.Contain("NaN"));
            Assert.That(result, Is.EqualTo("0.0")); // Safe default is "0.0"
        }

        /// <summary>
        /// Test formatters return safe defaults for Infinity input.
        /// </summary>
        [Test]
        public void FormatDistance_Infinity_ReturnsSafeDefault()
        {
            // Arrange
            double infinityDistance = double.PositiveInfinity;

            // Act
            string result = _formatter.FormatDistance(infinityDistance, UnitSystem.Metric);

            // Assert: Should return safe default not throw exception
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Not.Contain("Infinity"));
            Assert.That(result, Is.EqualTo("0.0 m"));
        }

        /// <summary>
        /// Test area formatter with NaN returns safe default.
        /// </summary>
        [Test]
        public void FormatArea_NaN_ReturnsSafeDefault()
        {
            // Arrange
            double nanArea = double.NaN;

            // Act
            string resultMetric = _formatter.FormatArea(nanArea, UnitSystem.Metric);
            string resultImperial = _formatter.FormatArea(nanArea, UnitSystem.Imperial);

            // Assert: Should return safe defaults for both unit systems
            Assert.That(resultMetric, Is.EqualTo("0.00 ha"));
            Assert.That(resultImperial, Is.EqualTo("0.00 ac"));
        }

        #endregion

        #region Boundary Condition Tests

        /// <summary>
        /// Test speed formatter at exact precision threshold boundary (2.0 km/h).
        /// This is the critical boundary for switching from 2 decimals to 1 decimal.
        /// </summary>
        [Test]
        public void FormatSpeed_ExactlyTwoKmh_UsesTwoDecimals()
        {
            // Arrange: Calculate m/s that converts to exactly 2.0 km/h
            // 2.0 km/h = 2.0 / 3.6 = 0.555556 m/s
            double speedMetersPerSecond = 2.0 / 3.6;

            // Act
            string result = _formatter.FormatSpeed(speedMetersPerSecond, UnitSystem.Metric);

            // Assert: At exactly 2.0 km/h, should use 2 decimals (<=2 threshold)
            Assert.That(result, Is.EqualTo("2.00"));
        }

        /// <summary>
        /// Test speed formatter just above precision threshold (2.01 km/h).
        /// Should use 1 decimal for values > 2.0 km/h.
        /// </summary>
        [Test]
        public void FormatSpeed_JustAboveTwoKmh_UsesOneDecimal()
        {
            // Arrange: 2.01 km/h = 2.01 / 3.6 m/s
            double speedMetersPerSecond = 2.01 / 3.6;

            // Act
            string result = _formatter.FormatSpeed(speedMetersPerSecond, UnitSystem.Metric);

            // Assert: Just above 2.0 km/h, should use 1 decimal
            Assert.That(result, Is.EqualTo("2.0"));
        }

        /// <summary>
        /// Test distance formatter at exact boundary (1000m for metric).
        /// Critical for verifying unit switching logic.
        /// </summary>
        [Test]
        public void FormatDistance_ExactBoundaries_SwitchesUnitsCorrectly()
        {
            // Arrange: Exact metric boundary (1000m)
            double exactMetricBoundary = 1000.0;
            // Just above imperial boundary to trigger miles (5281 ft)
            double justAboveImperialBoundary = 5281.0 / 3.28084; // ~1609.65m

            // Act
            string metricResult = _formatter.FormatDistance(exactMetricBoundary, UnitSystem.Metric);
            string imperialResult = _formatter.FormatDistance(justAboveImperialBoundary, UnitSystem.Imperial);

            // Assert: At exactly 1000m, should switch to km
            Assert.That(metricResult, Is.EqualTo("1.00 km"));
            // Just above 5280ft, should switch to miles
            Assert.That(imperialResult, Is.EqualTo("1.00 mi"));
        }

        /// <summary>
        /// Test heading wrapping at 360 degrees boundary.
        /// Verifies whether 359.8 rounds to 360° or wraps to 0°.
        /// </summary>
        [Test]
        public void FormatHeading_NearThreeSixty_RoundsCorrectly()
        {
            // Arrange
            double heading359_8 = 359.8;

            // Act
            string result = _formatter.FormatHeading(heading359_8);

            // Assert: Should round to 360° (not wrap to 0°)
            Assert.That(result, Is.EqualTo("360°"));
        }

        #endregion

        #region InvariantCulture Verification Tests

        /// <summary>
        /// Test all formatters use InvariantCulture (. not , for decimal separator).
        /// Critical for ensuring consistent formatting regardless of system culture.
        /// </summary>
        [Test]
        public void AllFormatters_UseInvariantCulture_DecimalSeparatorIsPeriod()
        {
            // Arrange: Sample data
            double speed = 1.85 / 3.6; // Will format to "1.85"
            double distance = 1250.0; // Will format to "1.25 km"
            double area = 10000.0; // Will format to "1.00 ha"
            double error = 0.018; // Will format to "1.8 cm"
            double precision = 0.012; // Will format to "0.01"
            double percentage = 75.5; // Will format to "75.5%"
            double time = 2.25; // Will format to "2.25 hr"
            double rate = 5.0; // Will format to "5.00 ac/hr"

            // Act: Format all values
            string speedResult = _formatter.FormatSpeed(speed, UnitSystem.Metric);
            string distanceResult = _formatter.FormatDistance(distance, UnitSystem.Metric);
            string areaResult = _formatter.FormatArea(area, UnitSystem.Metric);
            string errorResult = _formatter.FormatCrossTrackError(error, UnitSystem.Metric);
            string precisionResult = _formatter.FormatGpsPrecision(precision);
            string percentageResult = _formatter.FormatPercentage(percentage);
            string timeResult = _formatter.FormatTime(time);
            string rateResult = _formatter.FormatApplicationRate(rate, UnitSystem.Imperial);

            // Assert: All results should contain period (.) not comma (,)
            Assert.That(speedResult, Does.Contain("."), "Speed should use period for decimal separator");
            Assert.That(distanceResult, Does.Contain("."), "Distance should use period for decimal separator");
            Assert.That(areaResult, Does.Contain("."), "Area should use period for decimal separator");
            Assert.That(errorResult, Does.Contain("."), "Error should use period for decimal separator");
            Assert.That(precisionResult, Does.Contain("."), "Precision should use period for decimal separator");
            Assert.That(percentageResult, Does.Contain("."), "Percentage should use period for decimal separator");
            Assert.That(timeResult, Does.Contain("."), "Time should use period for decimal separator");
            Assert.That(rateResult, Does.Contain("."), "Rate should use period for decimal separator");

            // Assert: None should contain comma
            Assert.That(speedResult, Does.Not.Contain(","), "Speed should not use comma");
            Assert.That(distanceResult, Does.Not.Contain(","), "Distance should not use comma");
            Assert.That(areaResult, Does.Not.Contain(","), "Area should not use comma");
        }

        #endregion

        #region Unit Conversion Accuracy Tests

        /// <summary>
        /// Test speed conversion constants match spec exactly.
        /// Verifies MetersPerSecondToKmh = 3.6 and MetersPerSecondToMph = 2.23694.
        /// </summary>
        [Test]
        public void FormatSpeed_ConversionAccuracy_MatchesSpecConstants()
        {
            // Arrange: 1 m/s should convert to exactly 3.6 km/h and 2.23694 mph
            double oneMetersPerSecond = 1.0;

            // Act
            string kmhResult = _formatter.FormatSpeed(oneMetersPerSecond, UnitSystem.Metric);
            string mphResult = _formatter.FormatSpeed(oneMetersPerSecond, UnitSystem.Imperial);

            // Assert: 1 m/s = 3.6 km/h (should format as "3.6" with 1 decimal since > 2)
            Assert.That(kmhResult, Is.EqualTo("3.6"));
            // Assert: 1 m/s = 2.23694 mph (should format as "2.2" with 1 decimal since > 2)
            Assert.That(mphResult, Is.EqualTo("2.2"));
        }

        /// <summary>
        /// Test area conversion constants match spec exactly.
        /// Verifies SquareMetersToHectares = 0.0001 and SquareMetersToAcres = 0.000247105.
        /// </summary>
        [Test]
        public void FormatArea_ConversionAccuracy_MatchesSpecConstants()
        {
            // Arrange: 1 hectare = 10000 m², 1 acre = 4046.856 m²
            double oneHectareInSquareMeters = 10000.0;
            double oneAcreInSquareMeters = 4046.856;

            // Act
            string hectareResult = _formatter.FormatArea(oneHectareInSquareMeters, UnitSystem.Metric);
            string acreResult = _formatter.FormatArea(oneAcreInSquareMeters, UnitSystem.Imperial);

            // Assert: 10000 m² should format as exactly "1.00 ha"
            Assert.That(hectareResult, Is.EqualTo("1.00 ha"));
            // Assert: 4046.856 m² should format as "1.00 ac" (within rounding tolerance)
            Assert.That(acreResult, Is.EqualTo("1.00 ac"));
        }

        #endregion

        #region Negative Values and Zero Tests

        /// <summary>
        /// Test formatters handle negative values appropriately.
        /// Some formatters may format as-is, others may return zero.
        /// </summary>
        [Test]
        public void FormatCrossTrackError_NegativeValue_FormatsWithSign()
        {
            // Arrange: Negative cross-track error (left of line)
            double negativeError = -0.025; // -2.5 cm

            // Act
            string result = _formatter.FormatCrossTrackError(negativeError, UnitSystem.Metric);

            // Assert: Should format negative value (cross-track error can be negative)
            Assert.That(result, Does.StartWith("-"));
            Assert.That(result, Is.EqualTo("-2.5 cm"));
        }

        /// <summary>
        /// Test formatters handle zero values correctly.
        /// </summary>
        [Test]
        public void FormatPercentage_ZeroAndHundred_FormatsCorrectly()
        {
            // Arrange
            double zeroPercent = 0.0;
            double hundredPercent = 100.0;

            // Act
            string zeroResult = _formatter.FormatPercentage(zeroPercent);
            string hundredResult = _formatter.FormatPercentage(hundredPercent);

            // Assert: Should format with 1 decimal
            Assert.That(zeroResult, Is.EqualTo("0.0%"));
            Assert.That(hundredResult, Is.EqualTo("100.0%"));
        }

        #endregion
    }
}
