using System;
using System.IO;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for FieldMarkerFileService
/// Validates marker file I/O operations
/// </summary>
[TestFixture]
public class FieldMarkerFileServiceTests
{
    private FieldMarkerFileService? _service;
    private string? _testDirectory;

    [SetUp]
    public void SetUp()
    {
        _service = new FieldMarkerFileService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "AgValoniaGPS_Tests_" + Guid.NewGuid());
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

    #region Load/Save Markers

    [Test]
    public void LoadMarkers_NoFile_ReturnsEmptyList()
    {
        // Act
        var markers = _service!.LoadMarkers(_testDirectory!);

        // Assert
        Assert.That(markers, Is.Not.Null);
        Assert.That(markers.Count, Is.EqualTo(0));
    }

    [Test]
    public void SaveMarkers_CreatesFile()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var markers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Test Marker 1"),
            FieldMarker.CreateDefault(MarkerType.Obstacle, position, "Test Marker 2")
        };

        // Act
        _service!.SaveMarkers(markers, _testDirectory!);

        // Assert
        var filePath = _service.GetMarkerFilePath(_testDirectory!);
        Assert.That(File.Exists(filePath), Is.True);
    }

    [Test]
    public void SaveAndLoadMarkers_PreservesData()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var originalMarkers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Test Note"),
            FieldMarker.CreateDefault(MarkerType.Obstacle, position, "Test Obstacle"),
            FieldMarker.CreateDefault(MarkerType.Waypoint, position, "Test Waypoint")
        };

        originalMarkers[0].Id = 1;
        originalMarkers[0].Note = "Important note";
        originalMarkers[0].Category = "Category1";

        originalMarkers[1].Id = 2;
        originalMarkers[1].Note = "Large tree";
        originalMarkers[1].Category = "Hazards";

        originalMarkers[2].Id = 3;
        originalMarkers[2].Note = "Navigation point";

        // Act
        _service!.SaveMarkers(originalMarkers, _testDirectory!);
        var loadedMarkers = _service.LoadMarkers(_testDirectory!);

        // Assert
        Assert.That(loadedMarkers.Count, Is.EqualTo(3));

        var marker1 = loadedMarkers.FirstOrDefault(m => m.Id == 1);
        Assert.That(marker1, Is.Not.Null);
        Assert.That(marker1!.Name, Is.EqualTo("Test Note"));
        Assert.That(marker1.Type, Is.EqualTo(MarkerType.Note));
        Assert.That(marker1.Note, Is.EqualTo("Important note"));
        Assert.That(marker1.Category, Is.EqualTo("Category1"));

        var marker2 = loadedMarkers.FirstOrDefault(m => m.Id == 2);
        Assert.That(marker2, Is.Not.Null);
        Assert.That(marker2!.Name, Is.EqualTo("Test Obstacle"));
        Assert.That(marker2.Type, Is.EqualTo(MarkerType.Obstacle));
        Assert.That(marker2.Note, Is.EqualTo("Large tree"));
    }

    #endregion

    #region Single Marker Operations

    [Test]
    public void SaveMarker_NewMarker_AddsToFile()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker1 = FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 1");
        marker1.Id = 1;

        var marker2 = FieldMarker.CreateDefault(MarkerType.Flag, position, "Marker 2");
        marker2.Id = 2;

        // Act
        _service!.SaveMarker(marker1, _testDirectory!);
        _service.SaveMarker(marker2, _testDirectory!);

        // Assert
        var markers = _service.LoadMarkers(_testDirectory!);
        Assert.That(markers.Count, Is.EqualTo(2));
        Assert.That(markers.Any(m => m.Id == 1), Is.True);
        Assert.That(markers.Any(m => m.Id == 2), Is.True);
    }

    [Test]
    public void SaveMarker_ExistingMarker_UpdatesInFile()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = FieldMarker.CreateDefault(MarkerType.Note, position, "Original Name");
        marker.Id = 1;

        // Act
        _service!.SaveMarker(marker, _testDirectory!);

        // Modify marker
        marker.Name = "Updated Name";
        marker.Note = "Updated note";
        _service.SaveMarker(marker, _testDirectory!);

        // Assert
        var markers = _service.LoadMarkers(_testDirectory!);
        Assert.That(markers.Count, Is.EqualTo(1), "Should still have only one marker");

        var updatedMarker = markers.First();
        Assert.That(updatedMarker.Id, Is.EqualTo(1));
        Assert.That(updatedMarker.Name, Is.EqualTo("Updated Name"));
        Assert.That(updatedMarker.Note, Is.EqualTo("Updated note"));
    }

    #endregion

    #region Delete Operations

    [Test]
    public void DeleteMarker_ExistingMarker_RemovesFromFile()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var markers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 1"),
            FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 2"),
            FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 3")
        };

        markers[0].Id = 1;
        markers[1].Id = 2;
        markers[2].Id = 3;

        _service!.SaveMarkers(markers, _testDirectory!);

        // Act
        bool deleted = _service.DeleteMarker(2, _testDirectory!);

        // Assert
        Assert.That(deleted, Is.True);

        var remaining = _service.LoadMarkers(_testDirectory!);
        Assert.That(remaining.Count, Is.EqualTo(2));
        Assert.That(remaining.Any(m => m.Id == 1), Is.True);
        Assert.That(remaining.Any(m => m.Id == 2), Is.False);
        Assert.That(remaining.Any(m => m.Id == 3), Is.True);
    }

    [Test]
    public void DeleteMarker_NonExistentMarker_ReturnsFalse()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 1");
        marker.Id = 1;

        _service!.SaveMarker(marker, _testDirectory!);

        // Act
        bool deleted = _service.DeleteMarker(999, _testDirectory!);

        // Assert
        Assert.That(deleted, Is.False);
        Assert.That(_service.LoadMarkers(_testDirectory!).Count, Is.EqualTo(1));
    }

    [Test]
    public void DeleteAllMarkers_RemovesFile()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var markers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 1")
        };
        _service!.SaveMarkers(markers, _testDirectory!);

        // Act
        bool deleted = _service.DeleteAllMarkers(_testDirectory!);

        // Assert
        Assert.That(deleted, Is.True);
        Assert.That(_service.MarkersFileExists(_testDirectory!), Is.False);
    }

    [Test]
    public void DeleteAllMarkers_NoFile_ReturnsFalse()
    {
        // Act
        bool deleted = _service!.DeleteAllMarkers(_testDirectory!);

        // Assert
        Assert.That(deleted, Is.False);
    }

    #endregion

    #region File Utilities

    [Test]
    public void MarkersFileExists_ReturnsCorrectStatus()
    {
        // Assert - Initially no file
        Assert.That(_service!.MarkersFileExists(_testDirectory!), Is.False);

        // Act - Create file
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = FieldMarker.CreateDefault(MarkerType.Note, position, "Test");
        _service.SaveMarker(marker, _testDirectory!);

        // Assert - File now exists
        Assert.That(_service.MarkersFileExists(_testDirectory!), Is.True);
    }

    [Test]
    public void GetMarkerFilePath_ReturnsCorrectPath()
    {
        // Act
        var filePath = _service!.GetMarkerFilePath(_testDirectory!);

        // Assert
        Assert.That(filePath, Is.Not.Null);
        Assert.That(filePath, Does.EndWith("Markers.json"));
        Assert.That(filePath, Does.StartWith(_testDirectory!));
    }

    #endregion

    #region Import/Export

    [Test]
    public void ImportMarkers_Merge_CombinesMarkers()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory!, "Source");
        var targetDir = Path.Combine(_testDirectory!, "Target");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(targetDir);

        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };

        // Create source markers
        var sourceMarkers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Source Marker 1"),
            FieldMarker.CreateDefault(MarkerType.Note, position, "Source Marker 2")
        };
        sourceMarkers[0].Id = 1;
        sourceMarkers[1].Id = 2;
        _service!.SaveMarkers(sourceMarkers, sourceDir);

        // Create target markers
        var targetMarkers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Target Marker 1")
        };
        targetMarkers[0].Id = 10;
        _service.SaveMarkers(targetMarkers, targetDir);

        // Act
        int imported = _service.ImportMarkers(sourceDir, targetDir, merge: true);

        // Assert
        Assert.That(imported, Is.EqualTo(2));

        var result = _service.LoadMarkers(targetDir);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Any(m => m.Id == 1), Is.True);
        Assert.That(result.Any(m => m.Id == 2), Is.True);
        Assert.That(result.Any(m => m.Id == 10), Is.True);
    }

    [Test]
    public void ImportMarkers_Replace_OverwritesMarkers()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory!, "Source");
        var targetDir = Path.Combine(_testDirectory!, "Target");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(targetDir);

        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };

        // Create source markers
        var sourceMarkers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Source Marker 1")
        };
        sourceMarkers[0].Id = 1;
        _service!.SaveMarkers(sourceMarkers, sourceDir);

        // Create target markers
        var targetMarkers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Target Marker 1")
        };
        targetMarkers[0].Id = 10;
        _service.SaveMarkers(targetMarkers, targetDir);

        // Act
        int imported = _service.ImportMarkers(sourceDir, targetDir, merge: false);

        // Assert
        Assert.That(imported, Is.EqualTo(1));

        var result = _service.LoadMarkers(targetDir);
        Assert.That(result.Count, Is.EqualTo(1), "Should only have source markers");
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Source Marker 1"));
    }

    [Test]
    public void ExportMarkers_CreatesExportFile()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var markers = new System.Collections.Generic.List<FieldMarker>
        {
            FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 1"),
            FieldMarker.CreateDefault(MarkerType.Note, position, "Marker 2")
        };
        _service!.SaveMarkers(markers, _testDirectory!);

        var exportPath = Path.Combine(_testDirectory!, "Export", "ExportedMarkers.json");

        // Act
        int exported = _service.ExportMarkers(_testDirectory!, exportPath);

        // Assert
        Assert.That(exported, Is.EqualTo(2));
        Assert.That(File.Exists(exportPath), Is.True);

        // Verify exported content
        var exportedContent = File.ReadAllText(exportPath);
        Assert.That(exportedContent, Does.Contain("Marker 1"));
        Assert.That(exportedContent, Does.Contain("Marker 2"));
    }

    #endregion
}
