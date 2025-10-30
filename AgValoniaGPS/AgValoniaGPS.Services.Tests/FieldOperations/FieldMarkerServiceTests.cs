using System;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for FieldMarkerService
/// Validates marker management, filtering, proximity search, and visibility
/// </summary>
[TestFixture]
public class FieldMarkerServiceTests
{
    private FieldMarkerService? _service;

    [SetUp]
    public void SetUp()
    {
        _service = new FieldMarkerService();
    }

    #region Add/Remove Markers

    [Test]
    public void AddMarker_WithObject_AddsToCollection()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = FieldMarker.CreateDefault(MarkerType.Note, position, "Test Note");

        // Act
        var added = _service!.AddMarker(marker);

        // Assert
        Assert.That(added, Is.Not.Null);
        Assert.That(added.Id, Is.GreaterThan(0));
        Assert.That(added.Name, Is.EqualTo("Test Note"));
        Assert.That(_service.GetMarkerCount(), Is.EqualTo(1));
    }

    [Test]
    public void AddMarker_WithParameters_CreatesAndAddsMarker()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };

        // Act
        var marker = _service!.AddMarker(MarkerType.Obstacle, position, "Tree", "Large oak tree");

        // Assert
        Assert.That(marker, Is.Not.Null);
        Assert.That(marker.Id, Is.GreaterThan(0));
        Assert.That(marker.Name, Is.EqualTo("Tree"));
        Assert.That(marker.Note, Is.EqualTo("Large oak tree"));
        Assert.That(marker.Type, Is.EqualTo(MarkerType.Obstacle));
    }

    [Test]
    public void AddMarker_MultipleMarkers_AssignsUniqueIds()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };

        // Act
        var marker1 = _service!.AddMarker(MarkerType.Note, position, "Marker 1");
        var marker2 = _service.AddMarker(MarkerType.Flag, position, "Marker 2");
        var marker3 = _service.AddMarker(MarkerType.Waypoint, position, "Marker 3");

        // Assert
        Assert.That(marker1.Id, Is.Not.EqualTo(marker2.Id));
        Assert.That(marker2.Id, Is.Not.EqualTo(marker3.Id));
        Assert.That(_service.GetMarkerCount(), Is.EqualTo(3));
    }

    [Test]
    public void RemoveMarker_ExistingMarker_RemovesFromCollection()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = _service!.AddMarker(MarkerType.Note, position, "Test");

        // Act
        bool removed = _service.RemoveMarker(marker.Id);

        // Assert
        Assert.That(removed, Is.True);
        Assert.That(_service.GetMarkerCount(), Is.EqualTo(0));
        Assert.That(_service.GetMarker(marker.Id), Is.Null);
    }

    [Test]
    public void RemoveMarker_NonExistentMarker_ReturnsFalse()
    {
        // Act
        bool removed = _service!.RemoveMarker(999);

        // Assert
        Assert.That(removed, Is.False);
    }

    [Test]
    public void ClearMarkers_RemovesAllMarkers()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        _service!.AddMarker(MarkerType.Note, position, "Note 1");
        _service.AddMarker(MarkerType.Flag, position, "Flag 1");
        _service.AddMarker(MarkerType.Waypoint, position, "Waypoint 1");

        // Act
        _service.ClearMarkers();

        // Assert
        Assert.That(_service.GetMarkerCount(), Is.EqualTo(0));
        Assert.That(_service.GetAllMarkers(), Is.Empty);
    }

    #endregion

    #region Update Markers

    [Test]
    public void UpdateMarker_ExistingMarker_UpdatesProperties()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = _service!.AddMarker(MarkerType.Note, position, "Original");

        // Modify marker
        marker.Name = "Updated";
        marker.Note = "Updated note";

        // Act
        bool updated = _service.UpdateMarker(marker);

        // Assert
        Assert.That(updated, Is.True);
        var retrieved = _service.GetMarker(marker.Id);
        Assert.That(retrieved!.Name, Is.EqualTo("Updated"));
        Assert.That(retrieved.Note, Is.EqualTo("Updated note"));
        Assert.That(retrieved.ModifiedDate, Is.Not.Null);
    }

    [Test]
    public void UpdateMarker_NonExistentMarker_ReturnsFalse()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = FieldMarker.CreateDefault(MarkerType.Note, position);
        marker.Id = 999;

        // Act
        bool updated = _service!.UpdateMarker(marker);

        // Assert
        Assert.That(updated, Is.False);
    }

    #endregion

    #region Filtering

    [Test]
    public void GetMarkersByType_FiltersCorrectly()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        _service!.AddMarker(MarkerType.Note, position, "Note 1");
        _service.AddMarker(MarkerType.Note, position, "Note 2");
        _service.AddMarker(MarkerType.Obstacle, position, "Obstacle 1");
        _service.AddMarker(MarkerType.Waypoint, position, "Waypoint 1");

        // Act
        var notes = _service.GetMarkersByType(MarkerType.Note);
        var obstacles = _service.GetMarkersByType(MarkerType.Obstacle);

        // Assert
        Assert.That(notes.Count, Is.EqualTo(2));
        Assert.That(obstacles.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetMarkersByCategory_FiltersCorrectly()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker1 = _service!.AddMarker(MarkerType.Note, position, "Note 1");
        marker1.Category = "Important";
        _service.UpdateMarker(marker1);

        var marker2 = _service.AddMarker(MarkerType.Note, position, "Note 2");
        marker2.Category = "Important";
        _service.UpdateMarker(marker2);

        var marker3 = _service.AddMarker(MarkerType.Note, position, "Note 3");
        marker3.Category = "General";
        _service.UpdateMarker(marker3);

        // Act
        var important = _service.GetMarkersByCategory("Important");
        var general = _service.GetMarkersByCategory("General");

        // Assert
        Assert.That(important.Count, Is.EqualTo(2));
        Assert.That(general.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetMarkersForField_FiltersCorrectly()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker1 = _service!.AddMarker(MarkerType.Note, position, "Field 1 Marker 1");
        marker1.FieldId = 1;
        _service.UpdateMarker(marker1);

        var marker2 = _service.AddMarker(MarkerType.Note, position, "Field 1 Marker 2");
        marker2.FieldId = 1;
        _service.UpdateMarker(marker2);

        var marker3 = _service.AddMarker(MarkerType.Note, position, "Field 2 Marker");
        marker3.FieldId = 2;
        _service.UpdateMarker(marker3);

        // Act
        var field1Markers = _service.GetMarkersForField(1);
        var field2Markers = _service.GetMarkersForField(2);

        // Assert
        Assert.That(field1Markers.Count, Is.EqualTo(2));
        Assert.That(field2Markers.Count, Is.EqualTo(1));
    }

    [Test]
    public void ClearMarkersForField_RemovesOnlyFieldMarkers()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker1 = _service!.AddMarker(MarkerType.Note, position, "Field 1");
        marker1.FieldId = 1;
        _service.UpdateMarker(marker1);

        var marker2 = _service.AddMarker(MarkerType.Note, position, "Field 2");
        marker2.FieldId = 2;
        _service.UpdateMarker(marker2);

        // Act
        _service.ClearMarkersForField(1);

        // Assert
        Assert.That(_service.GetMarkerCount(), Is.EqualTo(1));
        Assert.That(_service.GetMarker(marker2.Id), Is.Not.Null);
        Assert.That(_service.GetMarker(marker1.Id), Is.Null);
    }

    #endregion

    #region Proximity Search

    [Test]
    public void GetMarkersNearPosition_FindsMarkersWithinRange()
    {
        // Arrange
        var centerPos = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var nearPos = new Position { Easting = 500005, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~5m away
        var farPos = new Position { Easting = 500100, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~100m away

        _service!.AddMarker(MarkerType.Note, centerPos, "Center");
        _service.AddMarker(MarkerType.Note, nearPos, "Near");
        _service.AddMarker(MarkerType.Note, farPos, "Far");

        // Act
        var markersWithin50m = _service.GetMarkersNearPosition(centerPos, 50.0);
        var markersWithin10m = _service.GetMarkersNearPosition(centerPos, 10.0);

        // Assert
        Assert.That(markersWithin50m.Count, Is.EqualTo(2), "Should find 2 markers within 50m");
        Assert.That(markersWithin10m.Count, Is.EqualTo(2), "Should find 2 markers within 10m");
    }

    [Test]
    public void GetMarkersNearPosition_ReturnsInDistanceOrder()
    {
        // Arrange
        var centerPos = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var pos1 = new Position { Easting = 500050, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~50m
        var pos2 = new Position { Easting = 500010, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~10m
        var pos3 = new Position { Easting = 500030, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~30m

        _service!.AddMarker(MarkerType.Note, pos1, "Far");
        _service.AddMarker(MarkerType.Note, pos2, "Near");
        _service.AddMarker(MarkerType.Note, pos3, "Middle");

        // Act
        var markers = _service.GetMarkersNearPosition(centerPos, 100.0);

        // Assert
        Assert.That(markers.Count, Is.EqualTo(3));
        Assert.That(markers[0].Name, Is.EqualTo("Near"), "Closest marker should be first");
        Assert.That(markers[1].Name, Is.EqualTo("Middle"), "Middle marker should be second");
        Assert.That(markers[2].Name, Is.EqualTo("Far"), "Farthest marker should be last");
    }

    [Test]
    public void GetNearestMarker_FindsClosestMarker()
    {
        // Arrange
        var centerPos = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var pos1 = new Position { Easting = 500050, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~50m
        var pos2 = new Position { Easting = 500010, Northing = 4500000, Zone = 15, Hemisphere = 'N' }; // ~10m

        _service!.AddMarker(MarkerType.Note, pos1, "Far");
        _service.AddMarker(MarkerType.Note, pos2, "Near");

        // Act
        var nearest = _service.GetNearestMarker(centerPos);

        // Assert
        Assert.That(nearest, Is.Not.Null);
        Assert.That(nearest!.Name, Is.EqualTo("Near"));
    }

    [Test]
    public void GetNearestMarker_NoMarkersInRange_ReturnsNull()
    {
        // Arrange
        var centerPos = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var farPos = new Position { Easting = 600000, Northing = 4600000, Zone = 15, Hemisphere = 'N' }; // Very far

        _service!.AddMarker(MarkerType.Note, farPos, "Far");

        // Act
        var nearest = _service.GetNearestMarker(centerPos, maxDistanceMeters: 50.0);

        // Assert
        Assert.That(nearest, Is.Null);
    }

    #endregion

    #region Visibility

    [Test]
    public void ToggleMarkerVisibility_TogglesState()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = _service!.AddMarker(MarkerType.Note, position, "Test");
        bool initialVisibility = marker.IsVisible;

        // Act
        _service.ToggleMarkerVisibility(marker.Id);

        // Assert
        var updated = _service.GetMarker(marker.Id);
        Assert.That(updated!.IsVisible, Is.Not.EqualTo(initialVisibility));
    }

    [Test]
    public void SetTypeVisibility_SetsVisibilityForAllMarkersOfType()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        _service!.AddMarker(MarkerType.Note, position, "Note 1");
        _service.AddMarker(MarkerType.Note, position, "Note 2");
        _service.AddMarker(MarkerType.Obstacle, position, "Obstacle 1");

        // Act
        _service.SetTypeVisibility(MarkerType.Note, false);

        // Assert
        var notes = _service.GetMarkersByType(MarkerType.Note);
        var obstacles = _service.GetMarkersByType(MarkerType.Obstacle);

        Assert.That(notes.All(m => !m.IsVisible), Is.True, "All notes should be hidden");
        Assert.That(obstacles.All(m => m.IsVisible), Is.True, "Obstacles should still be visible");
    }

    #endregion

    #region Search

    [Test]
    public void SearchMarkers_FindsMatchingMarkers()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        _service!.AddMarker(MarkerType.Note, position, "Important tree", "Large oak");
        _service.AddMarker(MarkerType.Note, position, "Rock pile", "Small rocks");
        _service.AddMarker(MarkerType.Obstacle, position, "Tree stump", "Old oak stump");

        // Act
        var oakResults = _service.SearchMarkers("oak");
        var treeResults = _service.SearchMarkers("tree");

        // Assert
        Assert.That(oakResults.Count, Is.EqualTo(2), "Should find 2 markers containing 'oak'");
        Assert.That(treeResults.Count, Is.EqualTo(2), "Should find 2 markers containing 'tree'");
    }

    [Test]
    public void SearchMarkers_CaseSensitive_RespectsCase()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        _service!.AddMarker(MarkerType.Note, position, "IMPORTANT", "This is URGENT");
        _service.AddMarker(MarkerType.Note, position, "important", "This is urgent");

        // Act
        var caseSensitiveResults = _service.SearchMarkers("IMPORTANT", caseSensitive: true);
        var caseInsensitiveResults = _service.SearchMarkers("IMPORTANT", caseSensitive: false);

        // Assert
        Assert.That(caseSensitiveResults.Count, Is.EqualTo(1), "Case sensitive should find only exact match");
        Assert.That(caseInsensitiveResults.Count, Is.EqualTo(2), "Case insensitive should find both");
    }

    #endregion

    #region Counting

    [Test]
    public void GetMarkerCountByType_ReturnsCorrectCount()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        _service!.AddMarker(MarkerType.Note, position, "Note 1");
        _service.AddMarker(MarkerType.Note, position, "Note 2");
        _service.AddMarker(MarkerType.Obstacle, position, "Obstacle 1");

        // Act
        int noteCount = _service.GetMarkerCountByType(MarkerType.Note);
        int obstacleCount = _service.GetMarkerCountByType(MarkerType.Obstacle);
        int waypointCount = _service.GetMarkerCountByType(MarkerType.Waypoint);

        // Assert
        Assert.That(noteCount, Is.EqualTo(2));
        Assert.That(obstacleCount, Is.EqualTo(1));
        Assert.That(waypointCount, Is.EqualTo(0));
    }

    #endregion

    #region Events

    [Test]
    public void AddMarker_RaisesMarkerAddedEvent()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        bool eventRaised = false;
        FieldMarker? eventMarker = null;

        _service!.MarkerAdded += (sender, args) =>
        {
            eventRaised = true;
            eventMarker = args.Marker;
        };

        // Act
        var marker = _service.AddMarker(MarkerType.Note, position, "Test");

        // Assert
        Assert.That(eventRaised, Is.True);
        Assert.That(eventMarker, Is.Not.Null);
        Assert.That(eventMarker!.Id, Is.EqualTo(marker.Id));
    }

    [Test]
    public void RemoveMarker_RaisesMarkerRemovedEvent()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = _service!.AddMarker(MarkerType.Note, position, "Test");

        bool eventRaised = false;
        _service.MarkerRemoved += (sender, args) => { eventRaised = true; };

        // Act
        _service.RemoveMarker(marker.Id);

        // Assert
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void UpdateMarker_RaisesMarkerUpdatedEvent()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var marker = _service!.AddMarker(MarkerType.Note, position, "Original");

        bool eventRaised = false;
        _service.MarkerUpdated += (sender, args) => { eventRaised = true; };

        marker.Name = "Updated";

        // Act
        _service.UpdateMarker(marker);

        // Assert
        Assert.That(eventRaised, Is.True);
    }

    #endregion
}
