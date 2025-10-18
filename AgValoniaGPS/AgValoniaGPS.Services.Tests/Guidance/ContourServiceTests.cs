using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for ContourService core functionality:
    /// - StartRecording initializes recording state
    /// - AddPoint distance threshold checking
    /// - LockContour validation with minimum 10 points requirement
    /// - CalculateGuidance performance and correctness
    /// </summary>
    public class ContourServiceTests
    {
        [Fact]
        public void StartRecording_ValidPosition_InitializesRecording()
        {
            // Arrange
            var service = new ContourService();
            var startPos = new Position
            {
                Easting = 1000.0,
                Northing = 2000.0,
                Altitude = 100.0
            };

            // Act
            service.StartRecording(startPos, minDistanceMeters: 0.5);

            // Assert
            Assert.True(service.IsRecording);
            Assert.False(service.IsLocked);
        }

        [Fact]
        public void AddPoint_WithinMinDistance_SkipsPoint()
        {
            // Arrange
            var service = new ContourService();
            var startPos = new Position { Easting = 0.0, Northing = 0.0 };
            service.StartRecording(startPos, minDistanceMeters: 1.0);

            int pointAddedCount = 0;
            service.StateChanged += (s, e) =>
            {
                if (e.EventType == ContourEventType.PointAdded)
                    pointAddedCount++;
            };

            // Act - add point that is 0.3m away (less than 1.0m threshold)
            var nearbyPoint = new Position { Easting = 0.2, Northing = 0.2 }; // ~0.28m away
            service.AddPoint(nearbyPoint, offset: 0.0);

            // Assert
            Assert.Equal(0, pointAddedCount); // Point should NOT be added
        }

        [Fact]
        public void AddPoint_ExceedsMinDistance_AddsPoint()
        {
            // Arrange
            var service = new ContourService();
            var startPos = new Position { Easting = 0.0, Northing = 0.0 };
            service.StartRecording(startPos, minDistanceMeters: 1.0);

            int pointAddedCount = 0;
            service.StateChanged += (s, e) =>
            {
                if (e.EventType == ContourEventType.PointAdded)
                    pointAddedCount++;
            };

            // Act - add point that is 2.0m away (exceeds 1.0m threshold)
            var farPoint = new Position { Easting = 2.0, Northing = 0.0 }; // 2.0m away
            service.AddPoint(farPoint, offset: 0.0);

            // Assert
            Assert.Equal(1, pointAddedCount); // Point should be added
        }

        [Fact]
        public void LockContour_SufficientPoints_LocksSuccessfully()
        {
            // Arrange
            var service = new ContourService();
            service.StartRecording(new Position { Easting = 0.0, Northing = 0.0 }, minDistanceMeters: 1.0);

            // Add 9 more points (total 10 including start)
            for (int i = 1; i <= 9; i++)
            {
                service.AddPoint(new Position { Easting = i * 2.0, Northing = 0.0 }, offset: 0.0);
            }

            // Act
            var contour = service.LockContour("Test Contour");

            // Assert
            Assert.NotNull(contour);
            Assert.True(service.IsLocked);
            Assert.False(service.IsRecording);
            Assert.Equal("Test Contour", contour.Name);
            Assert.Equal(10, contour.Points.Count);
        }

        [Fact]
        public void LockContour_InsufficientPoints_ThrowsException()
        {
            // Arrange
            var service = new ContourService();
            service.StartRecording(new Position { Easting = 0.0, Northing = 0.0 }, minDistanceMeters: 1.0);

            // Add only 5 more points (total 6, less than minimum 10)
            for (int i = 1; i <= 5; i++)
            {
                service.AddPoint(new Position { Easting = i * 2.0, Northing = 0.0 }, offset: 0.0);
            }

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => service.LockContour("Insufficient Points"));
            Assert.Contains("at least 10 points", exception.Message);
        }

        [Fact]
        public void CalculateGuidance_OnContour_ReturnsCorrectOffset()
        {
            // Arrange
            var service = new ContourService();
            service.StartRecording(new Position { Easting = 0.0, Northing = 0.0 }, minDistanceMeters: 0.5);

            // Create a straight contour along X-axis
            for (int i = 1; i <= 15; i++)
            {
                service.AddPoint(new Position { Easting = i * 1.0, Northing = 0.0 }, offset: 0.0);
            }

            var contour = service.LockContour("Test Contour");

            // Position on the contour line
            var currentPos = new Position { Easting = 7.5, Northing = 0.0 };

            // Act
            var result = service.CalculateGuidance(currentPos, currentHeading: 90.0, contour);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.CrossTrackError, 2); // Should be on the line (±0.01m tolerance)
        }

        [Fact]
        public void CalculateGuidance_OffContour_ReturnsCorrectCrossTrackError()
        {
            // Arrange
            var service = new ContourService();
            service.StartRecording(new Position { Easting = 0.0, Northing = 0.0 }, minDistanceMeters: 0.5);

            // Create a straight contour along X-axis
            for (int i = 1; i <= 15; i++)
            {
                service.AddPoint(new Position { Easting = i * 1.0, Northing = 0.0 }, offset: 0.0);
            }

            var contour = service.LockContour("Test Contour");

            // Position 3 meters to the right (north) of the contour line
            var currentPos = new Position { Easting = 7.5, Northing = 3.0 };

            // Act
            var result = service.CalculateGuidance(currentPos, currentHeading: 90.0, contour);

            // Assert
            Assert.NotNull(result);
            // Cross-track error should be approximately 3.0m (sign depends on orientation)
            Assert.Equal(3.0, Math.Abs(result.CrossTrackError), 1); // ±0.1m tolerance
        }
    }
}
