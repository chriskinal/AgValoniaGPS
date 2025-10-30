using System;
using System.IO;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

[TestFixture]
public class ElevationFileServiceTests
{
    private ElevationFileService _service = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ElevationFileService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ElevationTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public async Task SaveElevationDataAsync_CreatesFile()
    {
        // Arrange
        var grid = new ElevationGrid { GridResolution = 5.0 };
        grid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        grid.ElevationPoints[new GridCell(1, 0)] = 105.0;
        grid.UpdateStatistics();

        // Act
        await _service.SaveElevationDataAsync(_testDirectory, grid);

        // Assert
        var filePath = Path.Combine(_testDirectory, "Elevation.txt");
        Assert.That(File.Exists(filePath), Is.True);
    }

    [Test]
    public async Task SaveElevationDataAsync_WritesCorrectFormat()
    {
        // Arrange
        var grid = new ElevationGrid { GridResolution = 5.0 };
        grid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        grid.UpdateStatistics();
        var startFix = new Position { Latitude = 45.0, Longitude = -93.0 };

        // Act
        await _service.SaveElevationDataAsync(_testDirectory, grid, startFix);

        // Assert
        var filePath = Path.Combine(_testDirectory, "Elevation.txt");
        var lines = await File.ReadAllLinesAsync(filePath);

        Assert.That(lines.Length, Is.GreaterThan(10));
        Assert.That(lines[1], Is.EqualTo("$FieldDir"));
        Assert.That(lines[2], Is.EqualTo("Elevation"));
        Assert.That(lines[7], Is.EqualTo("StartFix"));
    }

    [Test]
    public void SaveElevationDataAsync_ThrowsForNullDirectory()
    {
        // Arrange
        var grid = new ElevationGrid();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.SaveElevationDataAsync(null!, grid));
    }

    [Test]
    public void SaveElevationDataAsync_ThrowsForNullGrid()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _service.SaveElevationDataAsync(_testDirectory, null!));
    }

    [Test]
    public async Task LoadElevationDataAsync_ReturnsEmptyGrid_WhenFileDoesNotExist()
    {
        // Act
        var grid = await _service.LoadElevationDataAsync(_testDirectory);

        // Assert
        Assert.That(grid, Is.Not.Null);
        Assert.That(grid.PointCount, Is.EqualTo(0));
    }

    [Test]
    public async Task LoadElevationDataAsync_LoadsDataCorrectly()
    {
        // Arrange
        var originalGrid = new ElevationGrid { GridResolution = 10.0 };
        originalGrid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        originalGrid.ElevationPoints[new GridCell(1, 0)] = 110.0;
        originalGrid.ElevationPoints[new GridCell(0, 1)] = 120.0;
        originalGrid.UpdateStatistics();

        await _service.SaveElevationDataAsync(_testDirectory, originalGrid);

        // Act
        var loadedGrid = await _service.LoadElevationDataAsync(_testDirectory);

        // Assert
        Assert.That(loadedGrid.PointCount, Is.EqualTo(3));
        Assert.That(loadedGrid.ElevationPoints[new GridCell(0, 0)], Is.EqualTo(100.0));
        Assert.That(loadedGrid.ElevationPoints[new GridCell(1, 0)], Is.EqualTo(110.0));
        Assert.That(loadedGrid.ElevationPoints[new GridCell(0, 1)], Is.EqualTo(120.0));
    }

    [Test]
    public async Task RoundTrip_PreservesData()
    {
        // Arrange
        var originalGrid = new ElevationGrid { GridResolution = 7.5 };

        // Add multiple points
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                originalGrid.ElevationPoints[new GridCell(x, y)] = 100.0 + x * 10 + y;
            }
        }
        originalGrid.UpdateStatistics();

        // Act - Save and load
        await _service.SaveElevationDataAsync(_testDirectory, originalGrid);
        var loadedGrid = await _service.LoadElevationDataAsync(_testDirectory);

        // Assert
        Assert.That(loadedGrid.PointCount, Is.EqualTo(originalGrid.PointCount));
        Assert.That(loadedGrid.GridResolution, Is.EqualTo(originalGrid.GridResolution));

        foreach (var kvp in originalGrid.ElevationPoints)
        {
            Assert.That(loadedGrid.ElevationPoints[kvp.Key], Is.EqualTo(kvp.Value).Within(0.001));
        }
    }

    [Test]
    public async Task ValidateElevationFileAsync_ReturnsFalse_WhenFileDoesNotExist()
    {
        // Act
        var isValid = await _service.ValidateElevationFileAsync(_testDirectory);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public async Task ValidateElevationFileAsync_ReturnsTrue_ForValidFile()
    {
        // Arrange
        var grid = new ElevationGrid();
        grid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        await _service.SaveElevationDataAsync(_testDirectory, grid);

        // Act
        var isValid = await _service.ValidateElevationFileAsync(_testDirectory);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public async Task ValidateElevationFileAsync_ReturnsFalse_ForCorruptedFile()
    {
        // Arrange - Create a corrupted file
        var filePath = Path.Combine(_testDirectory, "Elevation.txt");
        await File.WriteAllTextAsync(filePath, "Invalid content");

        // Act
        var isValid = await _service.ValidateElevationFileAsync(_testDirectory);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public async Task ExportElevationDataAsync_CreatesFile()
    {
        // Arrange
        var grid = new ElevationGrid { GridResolution = 5.0 };
        grid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        grid.UpdateStatistics();

        var exportPath = Path.Combine(_testDirectory, "export.txt");

        // Act
        await _service.ExportElevationDataAsync(exportPath, grid);

        // Assert
        Assert.That(File.Exists(exportPath), Is.True);
    }

    [Test]
    public async Task ExportElevationDataAsync_WritesMetadata()
    {
        // Arrange
        var grid = new ElevationGrid { GridResolution = 5.0 };
        grid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        grid.ElevationPoints[new GridCell(1, 0)] = 150.0;
        grid.UpdateStatistics();

        var exportPath = Path.Combine(_testDirectory, "export.txt");

        // Act
        await _service.ExportElevationDataAsync(exportPath, grid);

        // Assert
        var content = await File.ReadAllTextAsync(exportPath);
        Assert.That(content, Does.Contain("Grid Resolution: 5.00 meters"));
        Assert.That(content, Does.Contain("Point Count: 2"));
        Assert.That(content, Does.Contain("Min Elevation: 100.00m"));
        Assert.That(content, Does.Contain("Max Elevation: 150.00m"));
    }

    [Test]
    public async Task ImportElevationDataAsync_LoadsExportedData()
    {
        // Arrange
        var originalGrid = new ElevationGrid { GridResolution = 5.0 };
        originalGrid.ElevationPoints[new GridCell(0, 0)] = 100.0;
        originalGrid.ElevationPoints[new GridCell(1, 1)] = 150.0;
        originalGrid.UpdateStatistics();

        var exportPath = Path.Combine(_testDirectory, "export.txt");
        await _service.ExportElevationDataAsync(exportPath, originalGrid);

        // Act
        var importedGrid = await _service.ImportElevationDataAsync(exportPath);

        // Assert
        Assert.That(importedGrid.PointCount, Is.EqualTo(2));
        Assert.That(importedGrid.ElevationPoints[new GridCell(0, 0)], Is.EqualTo(100.0));
        Assert.That(importedGrid.ElevationPoints[new GridCell(1, 1)], Is.EqualTo(150.0));
    }

    [Test]
    public void ImportElevationDataAsync_ThrowsForNonExistentFile()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _service.ImportElevationDataAsync(nonExistentPath));
    }

    [Test]
    public async Task SaveElevationDataAsync_HandlesNegativeGridCells()
    {
        // Arrange
        var grid = new ElevationGrid { GridResolution = 5.0 };
        grid.ElevationPoints[new GridCell(-5, -3)] = 100.0;
        grid.ElevationPoints[new GridCell(2, -1)] = 110.0;
        grid.UpdateStatistics();

        // Act
        await _service.SaveElevationDataAsync(_testDirectory, grid);
        var loadedGrid = await _service.LoadElevationDataAsync(_testDirectory);

        // Assert
        Assert.That(loadedGrid.ElevationPoints[new GridCell(-5, -3)], Is.EqualTo(100.0));
        Assert.That(loadedGrid.ElevationPoints[new GridCell(2, -1)], Is.EqualTo(110.0));
    }

    [Test]
    public async Task LoadElevationDataAsync_HandlesLargeDataset()
    {
        // Arrange
        var grid = new ElevationGrid { GridResolution = 5.0 };

        // Add 1000 points
        for (int x = 0; x < 50; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                grid.ElevationPoints[new GridCell(x, y)] = 100.0 + x + y * 0.5;
            }
        }
        grid.UpdateStatistics();

        // Act
        await _service.SaveElevationDataAsync(_testDirectory, grid);
        var loadedGrid = await _service.LoadElevationDataAsync(_testDirectory);

        // Assert
        Assert.That(loadedGrid.PointCount, Is.EqualTo(1000));
        Assert.That(loadedGrid.MinElevation, Is.EqualTo(grid.MinElevation).Within(0.001));
        Assert.That(loadedGrid.MaxElevation, Is.EqualTo(grid.MaxElevation).Within(0.001));
    }

    [Test]
    public async Task SaveElevationDataAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var newDirectory = Path.Combine(_testDirectory, "NewSubDir");
        var grid = new ElevationGrid();
        grid.ElevationPoints[new GridCell(0, 0)] = 100.0;

        // Act
        await _service.SaveElevationDataAsync(newDirectory, grid);

        // Assert
        Assert.That(Directory.Exists(newDirectory), Is.True);
        Assert.That(File.Exists(Path.Combine(newDirectory, "Elevation.txt")), Is.True);
    }
}
