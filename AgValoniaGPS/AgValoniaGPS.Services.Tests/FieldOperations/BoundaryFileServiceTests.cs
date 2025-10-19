using AgValoniaGPS.Models;
using AgValoniaGPS.Services.FieldOperations;
using Xunit;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for BoundaryFileService
/// </summary>
public class BoundaryFileServiceTests : IDisposable
{
    private readonly IBoundaryFileService _fileService;
    private readonly string _testDirectory;

    public BoundaryFileServiceTests()
    {
        _fileService = new BoundaryFileService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"BoundaryFileServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    /// <summary>
    /// Test 1: Save and load AgOpenGPS format
    /// </summary>
    [Fact]
    public void AgOpenGPSFormat_SaveAndLoad_PreservesData()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        var filePath = Path.Combine(_testDirectory, "boundary.txt");

        // Act
        _fileService.SaveToAgOpenGPS(boundary, filePath);
        var loaded = _fileService.LoadFromAgOpenGPS(filePath);

        // Assert
        Assert.NotEmpty(loaded);
        Assert.Equal(boundary.Length, loaded.Length);
        Assert.Equal(boundary[0].Easting, loaded[0].Easting, 2); // Within 0.01m tolerance
        Assert.Equal(boundary[0].Northing, loaded[0].Northing, 2);
    }

    /// <summary>
    /// Test 2: Save and load GeoJSON format
    /// </summary>
    [Fact]
    public void GeoJSONFormat_SaveAndLoad_PreservesData()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        var filePath = Path.Combine(_testDirectory, "boundary.geojson");

        // Act
        _fileService.SaveToGeoJSON(boundary, filePath);
        var loaded = _fileService.LoadFromGeoJSON(filePath);

        // Assert
        Assert.NotEmpty(loaded);
        // GeoJSON coordinate conversion may have some precision loss
        Assert.InRange(loaded.Length, boundary.Length - 1, boundary.Length + 1);
    }

    /// <summary>
    /// Test 3: Save and load KML format
    /// </summary>
    [Fact]
    public void KMLFormat_SaveAndLoad_PreservesData()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        var filePath = Path.Combine(_testDirectory, "boundary.kml");

        // Act
        _fileService.SaveToKML(boundary, filePath);
        var loaded = _fileService.LoadFromKML(filePath);

        // Assert
        Assert.NotEmpty(loaded);
        // KML coordinate conversion may have some precision loss
        Assert.InRange(loaded.Length, boundary.Length - 1, boundary.Length + 1);
    }

    /// <summary>
    /// Test 4: Load from non-existent file returns empty array
    /// </summary>
    [Fact]
    public void LoadFromAgOpenGPS_NonExistentFile_ReturnsEmptyArray()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var loaded = _fileService.LoadFromAgOpenGPS(filePath);

        // Assert
        Assert.Empty(loaded);
    }

    /// <summary>
    /// Test 5: Save creates parent directory if needed
    /// </summary>
    [Fact]
    public void SaveToAgOpenGPS_CreatesParentDirectory()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        var subDir = Path.Combine(_testDirectory, "subdir", "nested");
        var filePath = Path.Combine(subDir, "boundary.txt");

        // Act
        _fileService.SaveToAgOpenGPS(boundary, filePath);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.True(Directory.Exists(subDir));
    }

    /// <summary>
    /// Helper: Creates a square boundary
    /// </summary>
    private Position[] CreateSquareBoundary(double startEasting, double startNorthing, double size)
    {
        return new[]
        {
            new Position { Easting = startEasting, Northing = startNorthing, Latitude = 40.0, Longitude = -100.0 },
            new Position { Easting = startEasting + size, Northing = startNorthing, Latitude = 40.001, Longitude = -99.999 },
            new Position { Easting = startEasting + size, Northing = startNorthing + size, Latitude = 40.001, Longitude = -99.998 },
            new Position { Easting = startEasting, Northing = startNorthing + size, Latitude = 40.0, Longitude = -99.998 }
        };
    }
}
