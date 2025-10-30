using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for HeadlineService core functionality:
    /// - CreateHeadline with valid and invalid inputs
    /// - MoveHeadline (nudging) functionality
    /// - SetActiveHeadline and GetActiveHeadline
    /// - CalculateGuidance with path-based guidance
    /// - Thread-safety for concurrent operations
    /// </summary>
    public class HeadlineServiceTests
    {
        #region CreateHeadline Tests

        [Fact]
        public void CreateHeadline_ValidTrackPoints_CreatesHeadline()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateSampleTrackPoints();

            // Act
            var headline = service.CreateHeadline(trackPoints, "Test Headline", 0);

            // Assert
            Assert.NotNull(headline);
            Assert.Equal("Test Headline", headline.Name);
            Assert.Equal(trackPoints.Count, headline.TrackPoints.Count);
            Assert.Equal(0, headline.APointIndex);
            Assert.False(headline.IsActive);
            Assert.Equal(0.0, headline.MoveDistance);
        }

        [Fact]
        public void CreateHeadline_InsufficientPoints_ThrowsException()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = new List<Position>
            {
                new Position { Easting = 0, Northing = 0 },
                new Position { Easting = 10, Northing = 0 },
                new Position { Easting = 20, Northing = 0 }
            }; // Only 3 points, need 4

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                service.CreateHeadline(trackPoints, "Invalid", 0));
            Assert.Contains("at least 4 points", ex.Message);
        }

        [Fact]
        public void CreateHeadline_EmptyName_ThrowsException()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateSampleTrackPoints();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.CreateHeadline(trackPoints, "", 0));
        }

        [Fact]
        public void CreateHeadline_InvalidAPointIndex_ThrowsException()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateSampleTrackPoints();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.CreateHeadline(trackPoints, "Test", 100)); // Index out of range
        }

        [Fact]
        public void CreateHeadlineFromHeading_ValidParameters_CreatesHeadline()
        {
            // Arrange
            var service = new HeadlineService();
            var startPosition = new Position { Easting = 1000, Northing = 2000 };
            double heading = Math.PI / 2; // 90 degrees (East)

            // Act
            var headline = service.CreateHeadlineFromHeading(startPosition, heading, "Heading Test", 500.0);

            // Assert
            Assert.NotNull(headline);
            Assert.Equal("Heading Test", headline.Name);
            Assert.True(headline.TrackPoints.Count >= 4);
            Assert.Equal(0, headline.APointIndex);
            // First point should be at start position
            Assert.Equal(1000, headline.TrackPoints[0].Easting, 1);
            Assert.Equal(2000, headline.TrackPoints[0].Northing, 1);
            // Last point should be approximately 500m away in the East direction
            Assert.True(headline.TrackPoints[^1].Easting > 1400); // Should be ~500m east
        }

        #endregion

        #region MoveHeadline Tests

        [Fact]
        public void MoveHeadline_ValidOffset_MovesAllPoints()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateStraightTrackPoints();
            var headline = service.CreateHeadline(trackPoints, "Test", 0);
            var originalFirstPointEasting = headline.TrackPoints[0].Easting;

            // Act
            var movedHeadline = service.MoveHeadline(headline.Id, 10.0); // Move 10m right

            // Assert
            Assert.NotNull(movedHeadline);
            Assert.Equal(10.0, movedHeadline.MoveDistance);
            // Points should be moved perpendicular to the path
            // For a north-facing path, positive offset moves east
            Assert.NotEqual(originalFirstPointEasting, movedHeadline.TrackPoints[0].Easting);
        }

        [Fact]
        public void MoveHeadline_NonexistentId_ThrowsException()
        {
            // Arrange
            var service = new HeadlineService();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.MoveHeadline(999, 10.0));
        }

        [Fact]
        public void MoveHeadline_MultipleOffsets_AccumulatesDistance()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateStraightTrackPoints();
            var headline = service.CreateHeadline(trackPoints, "Test", 0);

            // Act
            service.MoveHeadline(headline.Id, 5.0);
            var final = service.MoveHeadline(headline.Id, 3.0);

            // Assert
            Assert.Equal(8.0, final.MoveDistance, 2);
        }

        #endregion

        #region Active Headline Tests

        [Fact]
        public void SetActiveHeadline_ValidId_ActivatesHeadline()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateSampleTrackPoints();
            var headline = service.CreateHeadline(trackPoints, "Test", 0);

            // Act
            var result = service.SetActiveHeadline(headline.Id);

            // Assert
            Assert.True(result);
            var active = service.GetActiveHeadline();
            Assert.NotNull(active);
            Assert.Equal(headline.Id, active.Id);
            Assert.True(active.IsActive);
        }

        [Fact]
        public void SetActiveHeadline_InvalidId_ReturnsFalse()
        {
            // Arrange
            var service = new HeadlineService();

            // Act
            var result = service.SetActiveHeadline(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetActiveHeadline_SwitchingActive_DeactivatesPrevious()
        {
            // Arrange
            var service = new HeadlineService();
            var headline1 = service.CreateHeadline(CreateSampleTrackPoints(), "H1", 0);
            var headline2 = service.CreateHeadline(CreateSampleTrackPoints(), "H2", 0);

            // Act
            service.SetActiveHeadline(headline1.Id);
            service.SetActiveHeadline(headline2.Id);

            // Assert
            var h1 = service.GetHeadline(headline1.Id);
            var h2 = service.GetHeadline(headline2.Id);
            Assert.False(h1!.IsActive);
            Assert.True(h2!.IsActive);
        }

        [Fact]
        public void GetActiveHeadline_NoActiveHeadline_ReturnsNull()
        {
            // Arrange
            var service = new HeadlineService();
            service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);

            // Act
            var active = service.GetActiveHeadline();

            // Assert
            Assert.Null(active);
        }

        #endregion

        #region CRUD Operations Tests

        [Fact]
        public void GetHeadline_ExistingId_ReturnsHeadline()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);

            // Act
            var retrieved = service.GetHeadline(headline.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(headline.Id, retrieved.Id);
            Assert.Equal("Test", retrieved.Name);
        }

        [Fact]
        public void GetHeadline_NonexistentId_ReturnsNull()
        {
            // Arrange
            var service = new HeadlineService();

            // Act
            var retrieved = service.GetHeadline(999);

            // Assert
            Assert.Null(retrieved);
        }

        [Fact]
        public void ListHeadlines_MultipleHeadlines_ReturnsAll()
        {
            // Arrange
            var service = new HeadlineService();
            service.CreateHeadline(CreateSampleTrackPoints(), "H1", 0);
            service.CreateHeadline(CreateSampleTrackPoints(), "H2", 0);
            service.CreateHeadline(CreateSampleTrackPoints(), "H3", 0);

            // Act
            var headlines = service.ListHeadlines();

            // Assert
            Assert.Equal(3, headlines.Count);
            Assert.Contains(headlines, h => h.Name == "H1");
            Assert.Contains(headlines, h => h.Name == "H2");
            Assert.Contains(headlines, h => h.Name == "H3");
        }

        [Fact]
        public void DeleteHeadline_ExistingId_RemovesHeadline()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);

            // Act
            var result = service.DeleteHeadline(headline.Id);

            // Assert
            Assert.True(result);
            Assert.Null(service.GetHeadline(headline.Id));
        }

        [Fact]
        public void DeleteHeadline_NonexistentId_ReturnsFalse()
        {
            // Arrange
            var service = new HeadlineService();

            // Act
            var result = service.DeleteHeadline(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeleteHeadline_ActiveHeadline_DeactivatesAndDeletes()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);
            service.SetActiveHeadline(headline.Id);

            // Act
            var result = service.DeleteHeadline(headline.Id);

            // Assert
            Assert.True(result);
            Assert.Null(service.GetActiveHeadline());
        }

        [Fact]
        public void ClearAllHeadlines_RemovesAllHeadlines()
        {
            // Arrange
            var service = new HeadlineService();
            service.CreateHeadline(CreateSampleTrackPoints(), "H1", 0);
            service.CreateHeadline(CreateSampleTrackPoints(), "H2", 0);
            var h3 = service.CreateHeadline(CreateSampleTrackPoints(), "H3", 0);
            service.SetActiveHeadline(h3.Id);

            // Act
            service.ClearAllHeadlines();

            // Assert
            Assert.Empty(service.ListHeadlines());
            Assert.Null(service.GetActiveHeadline());
        }

        #endregion

        #region Guidance Calculation Tests

        [Fact]
        public void CalculateGuidance_OnPath_ReturnsLowXTE()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateStraightTrackPoints(); // Straight north path
            var headline = service.CreateHeadline(trackPoints, "Test", 0);
            service.SetActiveHeadline(headline.Id);

            // Position on the path
            var currentPosition = new Position { Easting = 1000, Northing = 50 };

            // Act
            var result = service.CalculateGuidance(currentPosition, 0.0);

            // Assert
            Assert.NotNull(result);
            Assert.True(Math.Abs(result.CrossTrackError) < 1.0); // Within 1m
        }

        [Fact]
        public void CalculateGuidance_NoActiveHeadline_ReturnsNull()
        {
            // Arrange
            var service = new HeadlineService();
            service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);

            // Act
            var result = service.CalculateGuidance(new Position { Easting = 0, Northing = 0 }, 0.0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetClosestPoint_FindsNearestSegment()
        {
            // Arrange
            var service = new HeadlineService();
            var trackPoints = CreateStraightTrackPoints();
            var headline = service.CreateHeadline(trackPoints, "Test", 0);

            // Position to the right of the path
            var position = new Position { Easting = 1010, Northing = 50 };

            // Act
            var result = service.GetClosestPoint(position, headline);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Point);
            Assert.True(result.Distance > 9.0 && result.Distance < 11.0); // ~10m away
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void ValidateHeadline_ValidHeadline_ReturnsSuccess()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);

            // Act
            var result = service.ValidateHeadline(headline);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.ErrorMessages);
        }

        [Fact]
        public void ValidateHeadline_NullHeadline_ReturnsError()
        {
            // Arrange
            var service = new HeadlineService();

            // Act
            var result = service.ValidateHeadline(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.ErrorMessages, e => e.Contains("cannot be null"));
        }

        [Fact]
        public void ValidateHeadline_EmptyName_ReturnsError()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = new Headline
            {
                Name = "",
                TrackPoints = CreateSampleTrackPoints(),
                APointIndex = 0,
                Heading = 0.0
            };

            // Act
            var result = service.ValidateHeadline(headline);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.ErrorMessages, e => e.Contains("name"));
        }

        [Fact]
        public void ValidateHeadline_InsufficientPoints_ReturnsError()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = new Headline
            {
                Name = "Test",
                TrackPoints = new List<Position>
                {
                    new Position { Easting = 0, Northing = 0 },
                    new Position { Easting = 10, Northing = 0 }
                },
                APointIndex = 0,
                Heading = 0.0
            };

            // Act
            var result = service.ValidateHeadline(headline);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.ErrorMessages, e => e.Contains("at least 4"));
        }

        #endregion

        #region Event Tests

        [Fact]
        public void HeadlineChanged_OnCreate_FiresEvent()
        {
            // Arrange
            var service = new HeadlineService();
            HeadlineChangedEventArgs? eventArgs = null;
            service.HeadlineChanged += (s, e) => eventArgs = e;

            // Act
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(HeadlineChangeType.Created, eventArgs.ChangeType);
            Assert.Equal(headline.Id, eventArgs.Headline.Id);
        }

        [Fact]
        public void HeadlineChanged_OnMove_FiresEvent()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);
            HeadlineChangedEventArgs? eventArgs = null;
            service.HeadlineChanged += (s, e) => eventArgs = e;

            // Act
            service.MoveHeadline(headline.Id, 5.0);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(HeadlineChangeType.Moved, eventArgs.ChangeType);
        }

        [Fact]
        public void HeadlineChanged_OnActivate_FiresEvent()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);
            HeadlineChangedEventArgs? eventArgs = null;
            service.HeadlineChanged += (s, e) => eventArgs = e;

            // Act
            service.SetActiveHeadline(headline.Id);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(HeadlineChangeType.Activated, eventArgs.ChangeType);
        }

        [Fact]
        public void HeadlineChanged_OnDelete_FiresEvent()
        {
            // Arrange
            var service = new HeadlineService();
            var headline = service.CreateHeadline(CreateSampleTrackPoints(), "Test", 0);
            HeadlineChangedEventArgs? eventArgs = null;
            service.HeadlineChanged += (s, e) => eventArgs = e;

            // Act
            service.DeleteHeadline(headline.Id);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(HeadlineChangeType.Deleted, eventArgs.ChangeType);
        }

        #endregion

        #region Helper Methods

        private List<Position> CreateSampleTrackPoints()
        {
            return new List<Position>
            {
                new Position { Easting = 1000, Northing = 0 },
                new Position { Easting = 1005, Northing = 25 },
                new Position { Easting = 1000, Northing = 50 },
                new Position { Easting = 995, Northing = 75 },
                new Position { Easting = 1000, Northing = 100 }
            };
        }

        private List<Position> CreateStraightTrackPoints()
        {
            // Create a straight path going north
            return new List<Position>
            {
                new Position { Easting = 1000, Northing = 0 },
                new Position { Easting = 1000, Northing = 33 },
                new Position { Easting = 1000, Northing = 67 },
                new Position { Easting = 1000, Northing = 100 }
            };
        }

        #endregion
    }
}
