using System;
using System.Globalization;
using NUnit.Framework;
using AgValoniaGPS.Services.Display;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Display;

namespace AgValoniaGPS.Services.Tests.Display
{
    /// <summary>
    /// Tests for DisplayFormatterService advanced formatters.
    /// Covers area, cross-track error, GPS quality, percentage, time, application rate, and guidance line formatters.
    /// </summary>
    [TestFixture]
    public class DisplayFormatterServiceAdvancedTests
    {
        private IDisplayFormatterService _formatter = null!;

        [SetUp]
        public void SetUp()
        {
            _formatter = new DisplayFormatterService();
        }

        #region Area Formatting Tests

        /// <summary>
        /// Test area formatter converts to hectares for metric system
        /// </summary>
        [Test]
        public void FormatArea_Metric_ConvertsToHectares()
        {
            // Arrange: 10000 m² = 1 hectare
            double areaSquareMeters = 10000.0;

            // Act
            string result = _formatter.FormatArea(areaSquareMeters, UnitSystem.Metric);

            // Assert: Should convert to hectares with 2 decimals
            Assert.That(result, Is.EqualTo("1.00 ha"));
        }

        /// <summary>
        /// Test area formatter converts to acres for imperial system
        /// </summary>
        [Test]
        public void FormatArea_Imperial_ConvertsToAcres()
        {
            // Arrange: 10000 m² ≈ 2.47 acres
            double areaSquareMeters = 10000.0;

            // Act
            string result = _formatter.FormatArea(areaSquareMeters, UnitSystem.Imperial);

            // Assert: Should convert to acres with 2 decimals
            Assert.That(result, Is.EqualTo("2.47 ac"));
        }

        #endregion

        #region Cross-Track Error Formatting Tests

        /// <summary>
        /// Test cross-track error formatter converts to centimeters for metric system
        /// </summary>
        [Test]
        public void FormatCrossTrackError_Metric_ConvertsToCentimeters()
        {
            // Arrange: 0.018 m = 1.8 cm
            double errorMeters = 0.018;

            // Act
            string result = _formatter.FormatCrossTrackError(errorMeters, UnitSystem.Metric);

            // Assert: Should convert to cm with 1 decimal
            Assert.That(result, Is.EqualTo("1.8 cm"));
        }

        /// <summary>
        /// Test cross-track error formatter converts to inches for imperial system
        /// </summary>
        [Test]
        public void FormatCrossTrackError_Imperial_ConvertsToInches()
        {
            // Arrange: 0.018 m ≈ 0.7 inches
            double errorMeters = 0.018;

            // Act
            string result = _formatter.FormatCrossTrackError(errorMeters, UnitSystem.Imperial);

            // Assert: Should convert to inches with 1 decimal
            Assert.That(result, Is.EqualTo("0.7 in"));
        }

        #endregion

        #region GPS Quality Formatting Tests

        /// <summary>
        /// Test GPS quality formatter returns PaleGreen color for RTK Fixed
        /// </summary>
        [Test]
        public void FormatGpsQuality_RtkFixed_ReturnsPaleGreen()
        {
            // Arrange
            GpsFixType fixType = GpsFixType.RtkFixed;
            double age = 0.0;

            // Act
            GpsQualityDisplay result = _formatter.FormatGpsQuality(fixType, age);

            // Assert: Should have RTK fix text and PaleGreen color
            Assert.That(result.FormattedText, Is.EqualTo("RTK fix: Age: 0.0"));
            Assert.That(result.ColorName, Is.EqualTo("PaleGreen"));
        }

        /// <summary>
        /// Test GPS quality formatter returns Orange color for RTK Float
        /// </summary>
        [Test]
        public void FormatGpsQuality_RtkFloat_ReturnsOrange()
        {
            // Arrange
            GpsFixType fixType = GpsFixType.RtkFloat;
            double age = 1.2;

            // Act
            GpsQualityDisplay result = _formatter.FormatGpsQuality(fixType, age);

            // Assert: Should have RTK float text and Orange color
            Assert.That(result.FormattedText, Is.EqualTo("RTK float: Age: 1.2"));
            Assert.That(result.ColorName, Is.EqualTo("Orange"));
        }

        /// <summary>
        /// Test GPS quality formatter returns Yellow color for DGPS
        /// </summary>
        [Test]
        public void FormatGpsQuality_DGPS_ReturnsYellow()
        {
            // Arrange
            GpsFixType fixType = GpsFixType.DGPS;
            double age = 0.5;

            // Act
            GpsQualityDisplay result = _formatter.FormatGpsQuality(fixType, age);

            // Assert: Should have DGPS text and Yellow color
            Assert.That(result.FormattedText, Is.EqualTo("DGPS: Age: 0.5"));
            Assert.That(result.ColorName, Is.EqualTo("Yellow"));
        }

        /// <summary>
        /// Test GPS quality formatter returns Red color for all other fix types
        /// </summary>
        [Test]
        public void FormatGpsQuality_AutonomousAndNone_ReturnsRed()
        {
            // Arrange - Test Autonomous
            GpsFixType autonomousFixType = GpsFixType.Autonomous;
            double autonomousAge = 2.0;

            // Act - Test Autonomous
            GpsQualityDisplay autonomousResult = _formatter.FormatGpsQuality(autonomousFixType, autonomousAge);

            // Assert - Autonomous should have Autonomous text and Red color
            Assert.That(autonomousResult.FormattedText, Is.EqualTo("Autonomous: Age: 2.0"));
            Assert.That(autonomousResult.ColorName, Is.EqualTo("Red"));

            // Arrange - Test None
            GpsFixType noneFixType = GpsFixType.None;
            double noneAge = 0.0;

            // Act - Test None
            GpsQualityDisplay noneResult = _formatter.FormatGpsQuality(noneFixType, noneAge);

            // Assert - None should have No Fix text and Red color
            Assert.That(noneResult.FormattedText, Is.EqualTo("No Fix: Age: 0.0"));
            Assert.That(noneResult.ColorName, Is.EqualTo("Red"));
        }

        #endregion

        #region Percentage Formatting Tests

        /// <summary>
        /// Test percentage formatter uses 1 decimal precision
        /// </summary>
        [Test]
        public void FormatPercentage_UsesOneDecimalPrecision()
        {
            // Arrange
            double percentage = 75.5;

            // Act
            string result = _formatter.FormatPercentage(percentage);

            // Assert: Should format with 1 decimal and % suffix
            Assert.That(result, Is.EqualTo("75.5%"));
        }

        #endregion

        #region Time and Application Rate Formatting Tests

        /// <summary>
        /// Test time formatter uses 2 decimal precision
        /// </summary>
        [Test]
        public void FormatTime_UsesTwoDecimalPrecision()
        {
            // Arrange
            double hours = 2.25;

            // Act
            string result = _formatter.FormatTime(hours);

            // Assert: Should format with 2 decimals and " hr" suffix
            Assert.That(result, Is.EqualTo("2.25 hr"));
        }

        /// <summary>
        /// Test application rate formatter uses correct units for both systems
        /// </summary>
        [Test]
        public void FormatApplicationRate_UseCorrectUnitsForBothSystems()
        {
            // Arrange
            double rate = 5.0;

            // Act
            string metricResult = _formatter.FormatApplicationRate(rate, UnitSystem.Metric);
            string imperialResult = _formatter.FormatApplicationRate(rate, UnitSystem.Imperial);

            // Assert: Should use ha/hr for metric, ac/hr for imperial
            Assert.That(metricResult, Is.EqualTo("5.00 ha/hr"));
            Assert.That(imperialResult, Is.EqualTo("5.00 ac/hr"));
        }

        #endregion

        #region Guidance Line Formatting Tests

        /// <summary>
        /// Test guidance line formatter formats with correct pattern
        /// </summary>
        [Test]
        public void FormatGuidanceLine_FormatsWithCorrectPattern()
        {
            // Arrange
            GuidanceLineType lineType = GuidanceLineType.ABLine;
            double heading = 0.0;

            // Act
            string result = _formatter.FormatGuidanceLine(lineType, heading);

            // Assert: Should format as "Line: [Type] [heading]°"
            Assert.That(result, Is.EqualTo("Line: ABLine 0°"));
        }

        #endregion
    }
}
