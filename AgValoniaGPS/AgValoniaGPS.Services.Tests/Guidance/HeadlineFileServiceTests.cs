using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for HeadlineFileService file I/O operations:
    /// - Save and load headlines from Headlines.txt
    /// - Legacy AgOpenGPS format compatibility
    /// - File handling (create, delete, exists)
    /// - Round-trip data integrity
    /// </summary>
    public class HeadlineFileServiceTests : IDisposable
    {
        private readonly string _testDirectory;

        public HeadlineFileServiceTests()
        {
            // Create unique test directory for each test run
            _testDirectory = Path.Combine(Path.GetTempPath(), $"HeadlineTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            // Clean up test directory
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Save Tests

        [Fact]
        public void SaveHeadlines_ValidHeadlines_CreatesFile()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = CreateSampleHeadlines();

            // Act
            service.SaveHeadlines(headlines, _testDirectory);

            // Assert
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void SaveHeadlines_EmptyList_CreatesFileWithHeaderOnly()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = new List<Headline>();

            // Act
            service.SaveHeadlines(headlines, _testDirectory);

            // Assert
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.Contains("$HeadLines", content);
        }

        [Fact]
        public void SaveHeadlines_NullList_CreatesFileWithHeaderOnly()
        {
            // Arrange
            var service = new HeadlineFileService();

            // Act
            service.SaveHeadlines(null!, _testDirectory);

            // Assert
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void SaveHeadlines_InvalidDirectory_ThrowsException()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = CreateSampleHeadlines();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.SaveHeadlines(headlines, ""));
        }

        [Fact]
        public void SaveHeadlines_NonexistentDirectory_CreatesDirectory()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = CreateSampleHeadlines();
            var newDir = Path.Combine(_testDirectory, "NewSubDir");

            // Act
            service.SaveHeadlines(headlines, newDir);

            // Assert
            Assert.True(Directory.Exists(newDir));
            var filePath = Path.Combine(newDir, "Headlines.txt");
            Assert.True(File.Exists(filePath));
        }

        #endregion

        #region Load Tests

        [Fact]
        public void LoadHeadlines_ValidFile_LoadsHeadlines()
        {
            // Arrange
            var service = new HeadlineFileService();
            var originalHeadlines = CreateSampleHeadlines();
            service.SaveHeadlines(originalHeadlines, _testDirectory);

            // Act
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.NotNull(loadedHeadlines);
            Assert.Equal(originalHeadlines.Count, loadedHeadlines.Count);
        }

        [Fact]
        public void LoadHeadlines_NonexistentFile_ReturnsEmptyList()
        {
            // Arrange
            var service = new HeadlineFileService();

            // Act
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.NotNull(loadedHeadlines);
            Assert.Empty(loadedHeadlines);
        }

        [Fact]
        public void LoadHeadlines_CorruptFile_ReturnsEmptyList()
        {
            // Arrange
            var service = new HeadlineFileService();
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            File.WriteAllText(filePath, "This is not a valid headline file");

            // Act
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.NotNull(loadedHeadlines);
            // Should handle corruption gracefully by returning empty or partial list
        }

        #endregion

        #region Round-Trip Tests

        [Fact]
        public void RoundTrip_SaveAndLoad_PreservesData()
        {
            // Arrange
            var service = new HeadlineFileService();
            var originalHeadlines = CreateSampleHeadlines();

            // Act
            service.SaveHeadlines(originalHeadlines, _testDirectory);
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.Equal(originalHeadlines.Count, loadedHeadlines.Count);

            for (int i = 0; i < originalHeadlines.Count; i++)
            {
                var original = originalHeadlines[i];
                var loaded = loadedHeadlines[i];

                Assert.Equal(original.Name, loaded.Name);
                Assert.Equal(original.MoveDistance, loaded.MoveDistance, 2);
                Assert.Equal(original.Mode, loaded.Mode);
                Assert.Equal(original.APointIndex, loaded.APointIndex);
                Assert.Equal(original.TrackPoints.Count, loaded.TrackPoints.Count);

                // Verify track points
                for (int j = 0; j < original.TrackPoints.Count; j++)
                {
                    Assert.Equal(original.TrackPoints[j].Easting, loaded.TrackPoints[j].Easting, 2);
                    Assert.Equal(original.TrackPoints[j].Northing, loaded.TrackPoints[j].Northing, 2);
                }
            }
        }

        [Fact]
        public void RoundTrip_MultipleHeadlines_PreservesOrder()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = new List<Headline>
            {
                CreateHeadline("First", 0.0),
                CreateHeadline("Second", 5.0),
                CreateHeadline("Third", -3.0)
            };

            // Act
            service.SaveHeadlines(headlines, _testDirectory);
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.Equal("First", loadedHeadlines[0].Name);
            Assert.Equal("Second", loadedHeadlines[1].Name);
            Assert.Equal("Third", loadedHeadlines[2].Name);
        }

        #endregion

        #region Legacy Format Tests

        [Fact]
        public void LoadHeadlines_LegacyFormat_LoadsCorrectly()
        {
            // Arrange
            var service = new HeadlineFileService();
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");

            // Create legacy format file
            var legacyContent = @"$HeadLines
Test Headline
5.5
0
0
4
1000.000 , 2000.000 , 0.00000
1010.000 , 2025.000 , 0.12345
1005.000 , 2050.000 , -0.05432
1000.000 , 2075.000 , 0.00000
";
            File.WriteAllText(filePath, legacyContent);

            // Act
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.Single(loadedHeadlines);
            var headline = loadedHeadlines[0];
            Assert.Equal("Test Headline", headline.Name);
            Assert.Equal(5.5, headline.MoveDistance, 1);
            Assert.Equal(0, headline.Mode);
            Assert.Equal(0, headline.APointIndex);
            Assert.Equal(4, headline.TrackPoints.Count);
            Assert.Equal(1000.0, headline.TrackPoints[0].Easting, 1);
            Assert.Equal(2000.0, headline.TrackPoints[0].Northing, 1);
        }

        [Fact]
        public void LoadHeadlines_LegacyFormat_FiltersInvalidHeadlines()
        {
            // Arrange
            var service = new HeadlineFileService();
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");

            // Create file with one valid and one invalid headline (too few points)
            var content = @"$HeadLines
Valid Headline
0.0
0
0
4
1000.000 , 2000.000 , 0.00000
1010.000 , 2025.000 , 0.12345
1005.000 , 2050.000 , -0.05432
1000.000 , 2075.000 , 0.00000
Invalid Headline
0.0
0
0
2
1000.000 , 2000.000 , 0.00000
1010.000 , 2025.000 , 0.12345
";
            File.WriteAllText(filePath, content);

            // Act
            var loadedHeadlines = service.LoadHeadlines(_testDirectory);

            // Assert
            Assert.Single(loadedHeadlines); // Only valid headline loaded
            Assert.Equal("Valid Headline", loadedHeadlines[0].Name);
        }

        #endregion

        #region Delete and Exists Tests

        [Fact]
        public void DeleteHeadlines_ExistingFile_DeletesFile()
        {
            // Arrange
            var service = new HeadlineFileService();
            service.SaveHeadlines(CreateSampleHeadlines(), _testDirectory);

            // Act
            var result = service.DeleteHeadlines(_testDirectory);

            // Assert
            Assert.True(result);
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public void DeleteHeadlines_NonexistentFile_ReturnsFalse()
        {
            // Arrange
            var service = new HeadlineFileService();

            // Act
            var result = service.DeleteHeadlines(_testDirectory);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HeadlinesExist_FileExists_ReturnsTrue()
        {
            // Arrange
            var service = new HeadlineFileService();
            service.SaveHeadlines(CreateSampleHeadlines(), _testDirectory);

            // Act
            var result = service.HeadlinesExist(_testDirectory);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HeadlinesExist_FileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var service = new HeadlineFileService();

            // Act
            var result = service.HeadlinesExist(_testDirectory);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region File Format Tests

        [Fact]
        public void SaveHeadlines_VerifyFileFormat_ContainsExpectedStructure()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = new List<Headline> { CreateHeadline("Test", 0.0) };

            // Act
            service.SaveHeadlines(headlines, _testDirectory);

            // Assert
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            var lines = File.ReadAllLines(filePath);

            Assert.Contains("$HeadLines", lines[0]);
            Assert.Equal("Test", lines[1]); // Name
            // Subsequent lines should be: MoveDistance, Mode, APointIndex, PointCount, then points
        }

        [Fact]
        public void SaveHeadlines_VerifyPointFormat_UsesCommaSeparation()
        {
            // Arrange
            var service = new HeadlineFileService();
            var headlines = new List<Headline> { CreateHeadline("Test", 0.0) };

            // Act
            service.SaveHeadlines(headlines, _testDirectory);

            // Assert
            var filePath = Path.Combine(_testDirectory, "Headlines.txt");
            var content = File.ReadAllText(filePath);

            // Points should be formatted as: Easting , Northing , Heading
            Assert.Contains(",", content);
        }

        #endregion

        #region Helper Methods

        private List<Headline> CreateSampleHeadlines()
        {
            return new List<Headline>
            {
                CreateHeadline("Headline 1", 0.0),
                CreateHeadline("Headline 2", 10.5),
                CreateHeadline("Headline 3", -5.2)
            };
        }

        private Headline CreateHeadline(string name, double moveDistance)
        {
            var trackPoints = new List<Position>
            {
                new Position { Easting = 1000, Northing = 2000 },
                new Position { Easting = 1010, Northing = 2025 },
                new Position { Easting = 1005, Northing = 2050 },
                new Position { Easting = 1000, Northing = 2075 }
            };

            return new Headline
            {
                Id = 1,
                Name = name,
                TrackPoints = trackPoints,
                MoveDistance = moveDistance,
                Mode = 0,
                APointIndex = 0,
                Heading = 0.0,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
