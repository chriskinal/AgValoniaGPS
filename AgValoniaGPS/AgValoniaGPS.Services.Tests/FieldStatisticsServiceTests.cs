using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Display;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services;
using Xunit;

namespace AgValoniaGPS.Services.Tests
{
    /// <summary>
    /// Unit tests for FieldStatisticsService expansion covering:
    /// - Application statistics calculation
    /// - Rotating display data generation
    /// - Field name retrieval
    /// - Active guidance line info retrieval
    /// </summary>
    public class FieldStatisticsServiceTests
    {
        [Fact]
        public void CalculateApplicationStatistics_WithWorkedArea_ReturnsPopulatedStats()
        {
            // Arrange
            var service = new FieldStatisticsService();
            service.WorkedAreaSquareMeters = 50000; // 5 hectares

            // Create a boundary to set BoundaryAreaSquareMeters via UpdateBoundaryArea
            var boundary = CreateBoundary(100000); // 10 hectares
            service.UpdateBoundaryArea(boundary);

            // Act
            var stats = service.CalculateApplicationStatistics(5.5, 10.0); // speed 5.5 km/h, width 10m

            // Assert
            Assert.NotNull(stats);
            Assert.Equal(50000, stats.TotalAreaCovered);
            Assert.True(stats.WorkRate > 0); // Should have calculated work rate
            Assert.True(stats.CoveragePercentage >= 0 && stats.CoveragePercentage <= 100);
        }

        [Fact]
        public void GetRotatingDisplayData_Screen1_ReturnsAppStats()
        {
            // Arrange
            var service = new FieldStatisticsService();
            service.WorkedAreaSquareMeters = 25000;

            var boundary = CreateBoundary(50000);
            service.UpdateBoundaryArea(boundary);

            // Act
            var displayData = service.GetRotatingDisplayData(1, 5.0, 12.0);

            // Assert
            Assert.Equal(1, displayData.CurrentScreen);
            Assert.NotNull(displayData.AppStats);
            Assert.Equal(25000, displayData.AppStats.TotalAreaCovered);
        }

        [Fact]
        public void GetRotatingDisplayData_Screen2_ReturnsFieldName()
        {
            // Arrange
            var service = new FieldStatisticsService();

            // Act
            var displayData = service.GetRotatingDisplayData(2, 0, 0);

            // Assert
            Assert.Equal(2, displayData.CurrentScreen);
            Assert.NotNull(displayData.FieldName);
        }

        [Fact]
        public void GetRotatingDisplayData_Screen3_ReturnsGuidanceLineInfo()
        {
            // Arrange
            var service = new FieldStatisticsService();

            // Act
            var displayData = service.GetRotatingDisplayData(3, 0, 0);

            // Assert
            Assert.Equal(3, displayData.CurrentScreen);
            Assert.NotNull(displayData.GuidanceLineInfo);
            Assert.Contains("Line:", displayData.GuidanceLineInfo);
        }

        [Fact]
        public void GetCurrentFieldName_NoFieldSet_ReturnsDefault()
        {
            // Arrange
            var service = new FieldStatisticsService();

            // Act
            var fieldName = service.GetCurrentFieldName();

            // Assert
            Assert.NotNull(fieldName);
            Assert.False(string.IsNullOrWhiteSpace(fieldName));
        }

        [Fact]
        public void GetActiveGuidanceLineInfo_NoActiveLine_ReturnsDefault()
        {
            // Arrange
            var service = new FieldStatisticsService();

            // Act
            var (lineType, heading) = service.GetActiveGuidanceLineInfo();

            // Assert
            Assert.Equal(GuidanceLineType.ABLine, lineType);
            Assert.Equal(0.0, heading);
        }

        /// <summary>
        /// Helper method to create a simple test boundary with specified area
        /// </summary>
        private Boundary CreateBoundary(double areaSquareMeters)
        {
            // Create a simple square boundary with the desired area
            double sideLength = Math.Sqrt(areaSquareMeters);

            var outerBoundary = new BoundaryPolygon
            {
                Points = new List<BoundaryPoint>
                {
                    new BoundaryPoint { Easting = 0, Northing = 0, Heading = 0 },
                    new BoundaryPoint { Easting = sideLength, Northing = 0, Heading = 90 },
                    new BoundaryPoint { Easting = sideLength, Northing = sideLength, Heading = 180 },
                    new BoundaryPoint { Easting = 0, Northing = sideLength, Heading = 270 }
                }
            };

            return new Boundary
            {
                OuterBoundary = outerBoundary,
                InnerBoundaries = new List<BoundaryPolygon>()
            };
        }
    }
}
