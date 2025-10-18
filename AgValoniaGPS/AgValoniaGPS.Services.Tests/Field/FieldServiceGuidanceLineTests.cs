using System;
using System.IO;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Field
{
    /// <summary>
    /// Unit tests for FieldService guidance line persistence functionality.
    /// Tests cover save/load round-trip accuracy and backward compatibility.
    /// </summary>
    public class FieldServiceGuidanceLineTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly FieldService _fieldService;

        public FieldServiceGuidanceLineTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"FieldServiceTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            _fieldService = new FieldService();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void SaveAndLoadABLine_RoundTrip_PreservesData()
        {
            // Arrange
            var abLine = new ABLine
            {
                Name = "Test AB Line",
                PointA = new Models.Position { Easting = 1000.0, Northing = 2000.0, Altitude = 100.0 },
                PointB = new Models.Position { Easting = 1100.0, Northing = 2100.0, Altitude = 105.0 },
                Heading = 0.785398, // 45 degrees in radians
                NudgeOffset = 2.5,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _fieldService.SaveABLine(abLine, _testDirectory);
            var loadedLine = _fieldService.LoadABLine(_testDirectory);

            // Assert
            Assert.NotNull(loadedLine);
            Assert.Equal(abLine.Name, loadedLine.Name);
            Assert.Equal(abLine.PointA.Easting, loadedLine.PointA.Easting, 3);
            Assert.Equal(abLine.PointA.Northing, loadedLine.PointA.Northing, 3);
            Assert.Equal(abLine.PointB.Easting, loadedLine.PointB.Easting, 3);
            Assert.Equal(abLine.PointB.Northing, loadedLine.PointB.Northing, 3);
            Assert.Equal(abLine.Heading, loadedLine.Heading, 6);
            Assert.Equal(abLine.NudgeOffset, loadedLine.NudgeOffset, 3);
        }

        [Fact]
        public void SaveAndLoadCurveLine_RoundTrip_PreservesPoints()
        {
            // Arrange
            var curveLine = new CurveLine
            {
                Name = "Test Curve",
                Points = new System.Collections.Generic.List<Models.Position>
                {
                    new Models.Position { Easting = 0.0, Northing = 0.0, Altitude = 10.0 },
                    new Models.Position { Easting = 10.0, Northing = 5.0, Altitude = 11.0 },
                    new Models.Position { Easting = 20.0, Northing = 15.0, Altitude = 12.0 },
                    new Models.Position { Easting = 30.0, Northing = 20.0, Altitude = 13.0 }
                },
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _fieldService.SaveCurveLine(curveLine, _testDirectory);
            var loadedCurve = _fieldService.LoadCurveLine(_testDirectory);

            // Assert
            Assert.NotNull(loadedCurve);
            Assert.Equal(curveLine.Name, loadedCurve.Name);
            Assert.Equal(curveLine.Points.Count, loadedCurve.Points.Count);
            for (int i = 0; i < curveLine.Points.Count; i++)
            {
                Assert.Equal(curveLine.Points[i].Easting, loadedCurve.Points[i].Easting, 3);
                Assert.Equal(curveLine.Points[i].Northing, loadedCurve.Points[i].Northing, 3);
            }
        }

        [Fact]
        public void SaveAndLoadContour_RoundTrip_PreservesPointsAndState()
        {
            // Arrange
            var contour = new ContourLine
            {
                Name = "Test Contour",
                Points = new System.Collections.Generic.List<Models.Position>
                {
                    new Models.Position { Easting = 100.0, Northing = 100.0, Altitude = 5.0 },
                    new Models.Position { Easting = 101.0, Northing = 101.0, Altitude = 5.5 },
                    new Models.Position { Easting = 102.0, Northing = 102.0, Altitude = 6.0 },
                    new Models.Position { Easting = 103.0, Northing = 103.0, Altitude = 6.5 },
                    new Models.Position { Easting = 104.0, Northing = 104.0, Altitude = 7.0 },
                    new Models.Position { Easting = 105.0, Northing = 105.0, Altitude = 7.5 },
                    new Models.Position { Easting = 106.0, Northing = 106.0, Altitude = 8.0 },
                    new Models.Position { Easting = 107.0, Northing = 107.0, Altitude = 8.5 },
                    new Models.Position { Easting = 108.0, Northing = 108.0, Altitude = 9.0 },
                    new Models.Position { Easting = 109.0, Northing = 109.0, Altitude = 9.5 },
                    new Models.Position { Easting = 110.0, Northing = 110.0, Altitude = 10.0 }
                },
                IsLocked = true,
                MinDistanceThreshold = 0.5,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _fieldService.SaveContour(contour, _testDirectory);
            var loadedContour = _fieldService.LoadContour(_testDirectory);

            // Assert
            Assert.NotNull(loadedContour);
            Assert.Equal(contour.Name, loadedContour.Name);
            Assert.Equal(contour.IsLocked, loadedContour.IsLocked);
            Assert.Equal(contour.MinDistanceThreshold, loadedContour.MinDistanceThreshold, 3);
            Assert.Equal(contour.Points.Count, loadedContour.Points.Count);
            for (int i = 0; i < contour.Points.Count; i++)
            {
                Assert.Equal(contour.Points[i].Easting, loadedContour.Points[i].Easting, 3);
                Assert.Equal(contour.Points[i].Northing, loadedContour.Points[i].Northing, 3);
            }
        }

        [Fact]
        public void LoadABLine_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            var nonExistentDirectory = Path.Combine(_testDirectory, "NonExistent");

            // Act
            var result = _fieldService.LoadABLine(nonExistentDirectory);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LoadCurveLine_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            var nonExistentDirectory = Path.Combine(_testDirectory, "NonExistent");

            // Act
            var result = _fieldService.LoadCurveLine(nonExistentDirectory);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteGuidanceLine_ExistingABLine_RemovesFile()
        {
            // Arrange
            var abLine = new ABLine
            {
                Name = "Test Delete",
                PointA = new Models.Position { Easting = 1000.0, Northing = 2000.0 },
                PointB = new Models.Position { Easting = 1100.0, Northing = 2100.0 },
                Heading = 0.0
            };
            _fieldService.SaveABLine(abLine, _testDirectory);
            var filePath = Path.Combine(_testDirectory, "ABLine.txt");
            Assert.True(File.Exists(filePath));

            // Act
            _fieldService.DeleteGuidanceLine(_testDirectory, GuidanceLineType.ABLine);

            // Assert
            Assert.False(File.Exists(filePath));
        }
    }
}
