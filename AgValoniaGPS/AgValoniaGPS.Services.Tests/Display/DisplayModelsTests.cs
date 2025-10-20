using System;
using AgValoniaGPS.Models.Display;
using AgValoniaGPS.Models.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Display
{
    /// <summary>
    /// Tests for Display domain models to ensure proper instantiation and basic behaviors.
    /// Focused tests for critical model functionality only.
    /// </summary>
    public class DisplayModelsTests
    {
        [Fact]
        public void ApplicationStatistics_Instantiation_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var stats = new ApplicationStatistics
            {
                TotalAreaCovered = 50000.0,
                ApplicationRateTarget = 10.5,
                ActualApplicationRate = 10.2,
                CoveragePercentage = 97.0,
                WorkRate = 5.5
            };

            // Assert
            Assert.Equal(50000.0, stats.TotalAreaCovered);
            Assert.Equal(10.5, stats.ApplicationRateTarget);
            Assert.Equal(10.2, stats.ActualApplicationRate);
            Assert.Equal(97.0, stats.CoveragePercentage);
            Assert.Equal(5.5, stats.WorkRate);
        }

        [Fact]
        public void GpsQualityDisplay_Instantiation_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var gpsQuality = new GpsQualityDisplay
            {
                FormattedText = "RTK fix: Age: 0.0",
                ColorName = "PaleGreen"
            };

            // Assert
            Assert.Equal("RTK fix: Age: 0.0", gpsQuality.FormattedText);
            Assert.Equal("PaleGreen", gpsQuality.ColorName);
        }

        [Fact]
        public void RotatingDisplayData_Instantiation_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var displayData = new RotatingDisplayData
            {
                CurrentScreen = 1,
                AppStats = new ApplicationStatistics { TotalAreaCovered = 100000.0 },
                FieldName = "Test Field",
                GuidanceLineInfo = "Line: AB 45°"
            };

            // Assert
            Assert.Equal(1, displayData.CurrentScreen);
            Assert.NotNull(displayData.AppStats);
            Assert.Equal(100000.0, displayData.AppStats.TotalAreaCovered);
            Assert.Equal("Test Field", displayData.FieldName);
            Assert.Equal("Line: AB 45°", displayData.GuidanceLineInfo);
        }

        [Fact]
        public void RotatingDisplayData_DefaultValues_AreEmpty()
        {
            // Arrange & Act
            var displayData = new RotatingDisplayData();

            // Assert
            Assert.Equal(0, displayData.CurrentScreen);
            Assert.Null(displayData.AppStats);
            Assert.Equal(string.Empty, displayData.FieldName);
            Assert.Equal(string.Empty, displayData.GuidanceLineInfo);
        }

        [Fact]
        public void GuidanceLineType_EnumValues_AreCorrect()
        {
            // Assert - using GuidanceLineType from Models.Guidance namespace
            Assert.Equal(0, (int)GuidanceLineType.ABLine);
            Assert.Equal(1, (int)GuidanceLineType.CurveLine);
            Assert.Equal(2, (int)GuidanceLineType.Contour);
        }

        [Fact]
        public void GpsFixType_EnumValues_MatchSpecification()
        {
            // Assert - verify enum values match NMEA GGA quality indicator standards
            Assert.Equal(0, (int)GpsFixType.None);
            Assert.Equal(1, (int)GpsFixType.Autonomous);
            Assert.Equal(2, (int)GpsFixType.DGPS);
            Assert.Equal(4, (int)GpsFixType.RtkFixed);
            Assert.Equal(5, (int)GpsFixType.RtkFloat);
        }
    }
}
