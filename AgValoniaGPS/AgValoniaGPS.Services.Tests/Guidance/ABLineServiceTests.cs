using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for ABLineService core functionality:
    /// - CreateFromPoints with valid and invalid inputs
    /// - CreateFromHeading with correct orientation
    /// - CalculateGuidance with XTE accuracy (±1cm)
    /// - Performance requirements (<5ms for calculations)
    /// </summary>
    public class ABLineServiceTests
    {
        [Fact]
        public void CreateFromPoints_ValidPoints_CreatesLine()
        {
            // Arrange
            var service = new ABLineService();
            var pointA = new Position { Easting = 1000.0, Northing = 2000.0 };
            var pointB = new Position { Easting = 1100.0, Northing = 2100.0 };

            // Act
            var line = service.CreateFromPoints(pointA, pointB, "Test Line");

            // Assert
            Assert.NotNull(line);
            Assert.Equal("Test Line", line.Name);
            Assert.Equal(1000.0, line.PointA.Easting, 3);
            Assert.Equal(2000.0, line.PointA.Northing, 3);
            Assert.Equal(1100.0, line.PointB.Easting, 3);
            Assert.Equal(2100.0, line.PointB.Northing, 3);
            Assert.True(line.Heading >= 0 && line.Heading < 360);
        }

        [Fact]
        public void CreateFromPoints_IdenticalPoints_ThrowsException()
        {
            // Arrange
            var service = new ABLineService();
            var pointA = new Position { Easting = 1000.0, Northing = 2000.0 };
            var pointB = new Position { Easting = 1000.0, Northing = 2000.0 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.CreateFromPoints(pointA, pointB, "Invalid Line"));
        }

        [Fact]
        public void CreateFromHeading_ValidHeading_CreatesLineWithCorrectOrientation()
        {
            // Arrange
            var service = new ABLineService();
            var origin = new Position { Easting = 1000.0, Northing = 2000.0 };
            double headingDegrees = 90.0; // East

            // Act
            var line = service.CreateFromHeading(origin, headingDegrees, "Heading Line");

            // Assert
            Assert.NotNull(line);
            Assert.Equal("Heading Line", line.Name);
            Assert.Equal(90.0, line.Heading, 2);
            Assert.Equal(1000.0, line.PointA.Easting, 2);
            Assert.Equal(2000.0, line.PointA.Northing, 2);
            // Point B should be East of Point A (positive Easting delta)
            Assert.True(line.PointB.Easting > line.PointA.Easting);
        }

        [Fact]
        public void CalculateGuidance_OnLine_ReturnsZeroXTE()
        {
            // Arrange
            var service = new ABLineService();
            var line = service.CreateFromPoints(
                new Position { Easting = 0.0, Northing = 0.0 },
                new Position { Easting = 100.0, Northing = 0.0 },
                "Test Line"
            );
            // Position directly on the line
            var currentPos = new Position { Easting = 50.0, Northing = 0.0 };

            // Act
            var result = service.CalculateGuidance(currentPos, 90.0, line);

            // Assert - XTE should be 0 with ±1cm accuracy
            Assert.Equal(0.0, result.CrossTrackError, 2); // 2 decimal places = 1cm accuracy
            Assert.NotNull(result.ClosestPoint);
            Assert.Equal(50.0, result.ClosestPoint.Easting, 2);
            Assert.Equal(0.0, result.ClosestPoint.Northing, 2);
        }

        [Fact]
        public void CalculateGuidance_RightOfLine_ReturnsPositiveXTE()
        {
            // Arrange
            var service = new ABLineService();
            // Create line pointing East (90 degrees)
            var line = service.CreateFromPoints(
                new Position { Easting = 0.0, Northing = 0.0 },
                new Position { Easting = 100.0, Northing = 0.0 },
                "Test Line"
            );
            // Position 5m to the right (south) of the line
            var currentPos = new Position { Easting = 50.0, Northing = -5.0 };

            // Act
            var result = service.CalculateGuidance(currentPos, 90.0, line);

            // Assert - Right of line should be positive XTE
            Assert.True(result.CrossTrackError > 0);
            Assert.Equal(5.0, Math.Abs(result.CrossTrackError), 2); // ±1cm accuracy
        }

        [Fact]
        public void CalculateGuidance_LeftOfLine_ReturnsNegativeXTE()
        {
            // Arrange
            var service = new ABLineService();
            // Create line pointing East (90 degrees)
            var line = service.CreateFromPoints(
                new Position { Easting = 0.0, Northing = 0.0 },
                new Position { Easting = 100.0, Northing = 0.0 },
                "Test Line"
            );
            // Position 3m to the left (north) of the line
            var currentPos = new Position { Easting = 50.0, Northing = 3.0 };

            // Act
            var result = service.CalculateGuidance(currentPos, 90.0, line);

            // Assert - Left of line should be negative XTE
            Assert.True(result.CrossTrackError < 0);
            Assert.Equal(3.0, Math.Abs(result.CrossTrackError), 2); // ±1cm accuracy
        }

        [Fact]
        public void CalculateGuidance_Performance_CompletesUnder5ms()
        {
            // Arrange
            var service = new ABLineService();
            var line = service.CreateFromPoints(
                new Position { Easting = 0.0, Northing = 0.0 },
                new Position { Easting = 100.0, Northing = 0.0 },
                "Test Line"
            );
            var currentPos = new Position { Easting = 50.0, Northing = 5.0 };

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                service.CalculateGuidance(currentPos, 90.0, line);
            }

            // Act - Measure 100 iterations to get average
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                service.CalculateGuidance(currentPos, 90.0, line);
            }
            stopwatch.Stop();
            double avgTimeMs = stopwatch.Elapsed.TotalMilliseconds / 100.0;

            // Assert - Should complete in <5ms for 20-25 Hz operation
            Assert.True(avgTimeMs < 5.0,
                $"CalculateGuidance took {avgTimeMs:F3}ms on average, expected <5ms");
        }

        [Fact]
        public void GetClosestPoint_OnLine_ReturnsExactPoint()
        {
            // Arrange
            var service = new ABLineService();
            var line = service.CreateFromPoints(
                new Position { Easting = 0.0, Northing = 0.0 },
                new Position { Easting = 100.0, Northing = 0.0 },
                "Test Line"
            );
            var currentPos = new Position { Easting = 50.0, Northing = 0.0 };

            // Act
            var closestPoint = service.GetClosestPoint(currentPos, line);

            // Assert
            Assert.NotNull(closestPoint);
            Assert.Equal(50.0, closestPoint.Easting, 2);
            Assert.Equal(0.0, closestPoint.Northing, 2);
        }
    }
}
