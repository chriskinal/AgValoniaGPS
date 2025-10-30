using System;
using System.Diagnostics;
using System.IO;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.Services.Guidance;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for TramLineService
/// Validates tram line generation, proximity detection, and file I/O
/// </summary>
[TestFixture]
public class TramLineServiceTests
{
    private TramLineService? _service;
    private TramLineFileService? _fileService;
    private IABLineService? _abLineService;
    private string? _testDirectory;

    [SetUp]
    public void SetUp()
    {
        _abLineService = new ABLineService();
        _service = new TramLineService(_abLineService);
        _fileService = new TramLineFileService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "TramLineTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_testDirectory != null && Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public void GenerateTramLines_CreatesParallelLines()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 2;

        // Act
        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);
        var tramLines = _service.GetTramLines();

        // Assert
        Assert.That(tramLines, Is.Not.Null);
        Assert.That(tramLines!.Length, Is.EqualTo(5)); // 1 center + 2 left + 2 right
        Assert.That(_service.GetTramLineCount(), Is.EqualTo(5));
    }

    [Test]
    public void GenerateTramLines_CorrectSpacing()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 1;

        // Act
        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);
        var tramLines = _service.GetTramLines();

        // Assert - center line should be at original position
        Assert.That(tramLines![0][0].Easting, Is.EqualTo(lineStart.Easting).Within(0.01));
        Assert.That(tramLines![0][0].Northing, Is.EqualTo(lineStart.Northing).Within(0.01));

        // Left line should be offset by -spacing (in perpendicular direction)
        // For a north-south line (heading = 0), perpendicular offset is in east-west direction
        // Negative offset should be to the west (negative easting)
        Assert.That(tramLines![1][0].Easting, Is.LessThan(lineStart.Easting));

        // Right line should be offset by +spacing
        Assert.That(tramLines![2][0].Easting, Is.GreaterThan(lineStart.Easting));
    }

    [Test]
    public void GetDistanceToNearestTramLine_ReturnsCorrectDistance()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 1;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);

        // Position on the center line
        var position = new Position { Easting = 500000, Northing = 4500050, Zone = 15, Hemisphere = 'N' };

        // Act
        double distance = _service.GetDistanceToNearestTramLine(position);

        // Assert - should be very close to 0 (on the line)
        Assert.That(distance, Is.LessThan(0.01));
    }

    [Test]
    public void GetNearestTramLineId_ReturnsCorrectId()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 1;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);

        // Position on the center line (first tram line, ID = 0)
        var position = new Position { Easting = 500000, Northing = 4500050, Zone = 15, Hemisphere = 'N' };

        // Act
        int nearestId = _service.GetNearestTramLineId(position);

        // Assert
        Assert.That(nearestId, Is.EqualTo(0));
    }

    [Test]
    public void CheckProximity_RaisesEvent_WhenWithinThreshold()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 1;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);

        TramLineProximityEventArgs? eventArgs = null;
        _service.TramLineProximity += (sender, args) => eventArgs = args;

        // Position close to the center line
        var position = new Position { Easting = 500000.5, Northing = 4500050, Zone = 15, Hemisphere = 'N' };
        double threshold = 1.0; // 1 meter threshold

        // Act
        _service.CheckProximity(position, threshold);

        // Assert
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.TramLineId, Is.EqualTo(0));
        Assert.That(eventArgs.Distance, Is.LessThan(threshold));
    }

    [Test]
    public void CheckProximity_DoesNotRaiseEvent_WhenOutsideThreshold()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 1;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);

        TramLineProximityEventArgs? eventArgs = null;
        _service.TramLineProximity += (sender, args) => eventArgs = args;

        // Position far from any tram line
        var position = new Position { Easting = 500010, Northing = 4500050, Zone = 15, Hemisphere = 'N' };
        double threshold = 1.0; // 1 meter threshold

        // Act
        _service.CheckProximity(position, threshold);

        // Assert
        Assert.That(eventArgs, Is.Null);
    }

    [Test]
    public void Performance_GenerateTramLines_CompletesInUnder5ms()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 10; // Generate 21 tram lines (1 center + 10 left + 10 right)

        var stopwatch = Stopwatch.StartNew();

        // Act
        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);

        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5));
        Console.WriteLine($"Tram line generation time: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Test]
    public void Performance_ProximityDetection_CompletesInUnder2ms()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 10;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);

        var position = new Position { Easting = 500000.5, Northing = 4500050, Zone = 15, Hemisphere = 'N' };

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            _service.GetDistanceToNearestTramLine(position);
        }

        stopwatch.Stop();
        double avgTime = stopwatch.ElapsedMilliseconds / 100.0;

        // Assert
        Assert.That(avgTime, Is.LessThan(2.0));
        Console.WriteLine($"Average proximity detection time: {avgTime:F3}ms");
    }

    [Test]
    public void FileService_AgOpenGPSFormat_SaveAndLoad_PreservesData()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 2;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);
        var tramLines = _service.GetTramLines();

        string filePath = Path.Combine(_testDirectory!, "TramLines.txt");

        // Act
        _fileService!.SaveAgOpenGPSFormat(tramLines!, filePath);
        var loadedTramLines = _fileService.LoadAgOpenGPSFormat(filePath);

        // Assert
        Assert.That(loadedTramLines, Is.Not.Null);
        Assert.That(loadedTramLines!.Length, Is.EqualTo(tramLines!.Length));

        // Verify first tram line positions match
        Assert.That(loadedTramLines[0][0].Easting, Is.EqualTo(tramLines[0][0].Easting).Within(0.01));
        Assert.That(loadedTramLines[0][0].Northing, Is.EqualTo(tramLines[0][0].Northing).Within(0.01));
    }

    [Test]
    public void FileService_GeoJSONFormat_ExportAndImport_PreservesData()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 2;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);
        var tramLines = _service.GetTramLines();

        string filePath = Path.Combine(_testDirectory!, "tramlines.geojson");

        // Act
        _fileService!.ExportGeoJSON(tramLines!, filePath);
        var loadedTramLines = _fileService.ImportGeoJSON(filePath);

        // Assert
        Assert.That(loadedTramLines, Is.Not.Null);
        Assert.That(loadedTramLines.Length, Is.EqualTo(tramLines!.Length));

        // Verify first tram line positions match
        Assert.That(loadedTramLines[0][0].Easting, Is.EqualTo(tramLines[0][0].Easting).Within(0.01));
        Assert.That(loadedTramLines[0][0].Northing, Is.EqualTo(tramLines[0][0].Northing).Within(0.01));
    }

    [Test]
    public void FileService_KMLFormat_ExportAndImport_PreservesData()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        double spacing = 3.0;
        int count = 2;

        _service!.GenerateTramLines(lineStart, lineEnd, spacing, count);
        var tramLines = _service.GetTramLines();

        string filePath = Path.Combine(_testDirectory!, "tramlines.kml");

        // Act
        _fileService!.ExportKML(tramLines!, filePath);
        var loadedTramLines = _fileService.ImportKML(filePath);

        // Assert
        Assert.That(loadedTramLines, Is.Not.Null);
        Assert.That(loadedTramLines.Length, Is.EqualTo(tramLines!.Length));

        // Verify first tram line positions match
        Assert.That(loadedTramLines[0][0].Easting, Is.EqualTo(tramLines[0][0].Easting).Within(0.01));
        Assert.That(loadedTramLines[0][0].Northing, Is.EqualTo(tramLines[0][0].Northing).Within(0.01));
    }

    [Test]
    public void SetSpacing_UpdatesSpacing()
    {
        // Arrange
        double newSpacing = 5.0;

        // Act
        _service!.SetSpacing(newSpacing);
        double spacing = _service.GetSpacing();

        // Assert
        Assert.That(spacing, Is.EqualTo(newSpacing));
    }

    [Test]
    public void ClearTramLines_RemovesAllTramLines()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        _service!.GenerateTramLines(lineStart, lineEnd, 3.0, 2);

        // Act
        _service.ClearTramLines();

        // Assert
        Assert.That(_service.GetTramLineCount(), Is.EqualTo(0));
        Assert.That(_service.GetTramLines(), Is.Null);
    }

    [Test]
    public void GetTramLine_ReturnsSpecificTramLine()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        _service!.GenerateTramLines(lineStart, lineEnd, 3.0, 2);

        // Act
        var tramLine = _service.GetTramLine(0);

        // Assert
        Assert.That(tramLine, Is.Not.Null);
        Assert.That(tramLine!.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GetTramLine_ReturnsNull_ForInvalidId()
    {
        // Arrange
        var lineStart = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var lineEnd = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        _service!.GenerateTramLines(lineStart, lineEnd, 3.0, 2);

        // Act
        var tramLine = _service.GetTramLine(999);

        // Assert
        Assert.That(tramLine, Is.Null);
    }
}
