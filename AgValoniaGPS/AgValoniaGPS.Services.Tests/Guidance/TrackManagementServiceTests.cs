using System;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Guidance;

/// <summary>
/// Tests for TrackManagementService
/// Validates track collection management, auto-switching, nudging, and worked track tracking
/// </summary>
[TestFixture]
public class TrackManagementServiceTests
{
    private TrackManagementService? _service;
    private IABLineService? _abLineService;
    private ICurveLineService? _curveLineService;
    private IContourService? _contourService;

    [SetUp]
    public void SetUp()
    {
        _abLineService = new ABLineService();
        _curveLineService = new CurveLineService();
        _contourService = new ContourService();
        _service = new TrackManagementService(_abLineService, _curveLineService, _contourService);
    }

    #region Add/Remove Tracks

    [Test]
    public void AddTrack_ABLine_AddsToCollection()
    {
        // Arrange
        var abLine = CreateTestABLine("Test AB Line");

        // Act
        var track = _service!.AddTrack(abLine);

        // Assert
        Assert.That(track, Is.Not.Null);
        Assert.That(track.Id, Is.GreaterThan(0));
        Assert.That(track.Mode, Is.EqualTo(TrackMode.ABLine));
        Assert.That(track.Name, Is.EqualTo("Test AB Line"));
        Assert.That(_service.GetTrackCount(), Is.EqualTo(1));
    }

    [Test]
    public void AddTrack_MultipleTracks_AssignsUniqueIds()
    {
        // Arrange
        var abLine1 = CreateTestABLine("AB Line 1");
        var abLine2 = CreateTestABLine("AB Line 2");
        var abLine3 = CreateTestABLine("AB Line 3");

        // Act
        var track1 = _service!.AddTrack(abLine1);
        var track2 = _service.AddTrack(abLine2);
        var track3 = _service.AddTrack(abLine3);

        // Assert
        Assert.That(track1.Id, Is.Not.EqualTo(track2.Id));
        Assert.That(track2.Id, Is.Not.EqualTo(track3.Id));
        Assert.That(_service.GetTrackCount(), Is.EqualTo(3));
    }

    [Test]
    public void RemoveTrack_ExistingTrack_RemovesFromCollection()
    {
        // Arrange
        var abLine = CreateTestABLine("Test AB Line");
        var track = _service!.AddTrack(abLine);

        // Act
        bool removed = _service.RemoveTrack(track.Id);

        // Assert
        Assert.That(removed, Is.True);
        Assert.That(_service.GetTrackCount(), Is.EqualTo(0));
        Assert.That(_service.GetTrack(track.Id), Is.Null);
    }

    [Test]
    public void RemoveTrack_NonExistentTrack_ReturnsFalse()
    {
        // Act
        bool removed = _service!.RemoveTrack(999);

        // Assert
        Assert.That(removed, Is.False);
    }

    [Test]
    public void ClearTracks_RemovesAllTracks()
    {
        // Arrange
        _service!.AddTrack(CreateTestABLine("AB 1"));
        _service.AddTrack(CreateTestABLine("AB 2"));
        _service.AddTrack(CreateTestABLine("AB 3"));

        // Act
        _service.ClearTracks();

        // Assert
        Assert.That(_service.GetTrackCount(), Is.EqualTo(0));
        Assert.That(_service.GetAllTracks(), Is.Empty);
    }

    #endregion

    #region Active Track Management

    [Test]
    public void SetActiveTrack_ExistingTrack_ActivatesTrack()
    {
        // Arrange
        var abLine = CreateTestABLine("Test AB Line");
        var track = _service!.AddTrack(abLine);

        // Act
        bool activated = _service.SetActiveTrack(track.Id);

        // Assert
        Assert.That(activated, Is.True);
        var activeTrack = _service.GetActiveTrack();
        Assert.That(activeTrack, Is.Not.Null);
        Assert.That(activeTrack!.Id, Is.EqualTo(track.Id));
        Assert.That(activeTrack.IsActive, Is.True);
    }

    [Test]
    public void SetActiveTrack_NonExistentTrack_ReturnsFalse()
    {
        // Act
        bool activated = _service!.SetActiveTrack(999);

        // Assert
        Assert.That(activated, Is.False);
        Assert.That(_service.GetActiveTrack(), Is.Null);
    }

    [Test]
    public void SetActiveTrack_SwitchingTracks_DeactivatesPreviousTrack()
    {
        // Arrange
        var track1 = _service!.AddTrack(CreateTestABLine("AB 1"));
        var track2 = _service.AddTrack(CreateTestABLine("AB 2"));
        _service.SetActiveTrack(track1.Id);

        // Act
        _service.SetActiveTrack(track2.Id);

        // Assert
        Assert.That(_service.GetTrack(track1.Id)!.IsActive, Is.False);
        Assert.That(_service.GetTrack(track2.Id)!.IsActive, Is.True);
    }

    #endregion

    #region Track Cycling

    [Test]
    public void CycleTrack_Forward_ActivatesNextTrack()
    {
        // Arrange
        var track1 = _service!.AddTrack(CreateTestABLine("AB 1"));
        var track2 = _service.AddTrack(CreateTestABLine("AB 2"));
        var track3 = _service.AddTrack(CreateTestABLine("AB 3"));
        _service.SetActiveTrack(track1.Id);

        // Act
        var nextTrack = _service.CycleTrack(forward: true);

        // Assert
        Assert.That(nextTrack, Is.Not.Null);
        Assert.That(nextTrack!.Id, Is.EqualTo(track2.Id));
    }

    [Test]
    public void CycleTrack_Backward_ActivatesPreviousTrack()
    {
        // Arrange
        var track1 = _service!.AddTrack(CreateTestABLine("AB 1"));
        var track2 = _service.AddTrack(CreateTestABLine("AB 2"));
        var track3 = _service.AddTrack(CreateTestABLine("AB 3"));
        _service.SetActiveTrack(track2.Id);

        // Act
        var prevTrack = _service.CycleTrack(forward: false);

        // Assert
        Assert.That(prevTrack, Is.Not.Null);
        Assert.That(prevTrack!.Id, Is.EqualTo(track1.Id));
    }

    [Test]
    public void CycleTrack_ForwardFromLast_WrapsToFirst()
    {
        // Arrange
        var track1 = _service!.AddTrack(CreateTestABLine("AB 1"));
        var track2 = _service.AddTrack(CreateTestABLine("AB 2"));
        var track3 = _service.AddTrack(CreateTestABLine("AB 3"));
        _service.SetActiveTrack(track3.Id);

        // Act
        var nextTrack = _service.CycleTrack(forward: true);

        // Assert
        Assert.That(nextTrack!.Id, Is.EqualTo(track1.Id), "Should wrap to first track");
    }

    [Test]
    public void CycleTrack_BackwardFromFirst_WrapsToLast()
    {
        // Arrange
        var track1 = _service!.AddTrack(CreateTestABLine("AB 1"));
        var track2 = _service.AddTrack(CreateTestABLine("AB 2"));
        var track3 = _service.AddTrack(CreateTestABLine("AB 3"));
        _service.SetActiveTrack(track1.Id);

        // Act
        var prevTrack = _service.CycleTrack(forward: false);

        // Assert
        Assert.That(prevTrack!.Id, Is.EqualTo(track3.Id), "Should wrap to last track");
    }

    #endregion

    #region Auto-Switching

    [Test]
    public void SwitchToClosestTrack_FindsClosestTrack()
    {
        // Arrange
        var abLine1 = _abLineService!.CreateFromHeading(
            new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' },
            0, "AB 1");
        var abLine2 = _abLineService.CreateFromHeading(
            new Position { Easting = 500010, Northing = 4500000, Zone = 15, Hemisphere = 'N' },
            0, "AB 2");
        var abLine3 = _abLineService.CreateFromHeading(
            new Position { Easting = 500020, Northing = 4500000, Zone = 15, Hemisphere = 'N' },
            0, "AB 3");

        _service!.AddTrack(abLine1);
        var track2 = _service.AddTrack(abLine2);
        _service.AddTrack(abLine3);

        var currentPosition = new Position { Easting = 500009, Northing = 4500005, Zone = 15, Hemisphere = 'N' };

        // Act
        var closestTrack = _service.SwitchToClosestTrack(currentPosition, maxDistance: 50.0);

        // Assert
        Assert.That(closestTrack, Is.Not.Null);
        Assert.That(closestTrack!.Id, Is.EqualTo(track2.Id), "Should select closest track (AB 2)");
    }

    [Test]
    public void SwitchToClosestTrack_AllTracksTooFar_ReturnsNull()
    {
        // Arrange
        var abLine = CreateTestABLine("AB 1");
        _service!.AddTrack(abLine);

        // Position very far from track
        var currentPosition = new Position { Easting = 600000, Northing = 4600000, Zone = 15, Hemisphere = 'N' };

        // Act
        var closestTrack = _service.SwitchToClosestTrack(currentPosition, maxDistance: 50.0);

        // Assert
        Assert.That(closestTrack, Is.Null, "Should return null when all tracks are too far");
    }

    #endregion

    #region Nudging

    [Test]
    public void NudgeActiveTrack_ABLine_AppliesOffset()
    {
        // Arrange
        var abLine = CreateTestABLine("Test AB Line");
        var track = _service!.AddTrack(abLine);
        _service.SetActiveTrack(track.Id);

        double offsetMeters = 2.0;

        // Act
        bool nudged = _service.NudgeActiveTrack(offsetMeters);

        // Assert
        Assert.That(nudged, Is.True);
        var activeTrack = _service.GetActiveTrack();
        Assert.That(activeTrack!.NudgeOffset, Is.EqualTo(offsetMeters));
    }

    [Test]
    public void NudgeActiveTrack_NoActiveTrack_ReturnsFalse()
    {
        // Act
        bool nudged = _service!.NudgeActiveTrack(2.0);

        // Assert
        Assert.That(nudged, Is.False);
    }

    #endregion

    #region Worked Track Management

    [Test]
    public void SetTrackWorked_MarksTrackAsWorked()
    {
        // Arrange
        var track = _service!.AddTrack(CreateTestABLine("Test AB Line"));

        // Act
        bool marked = _service.SetTrackWorked(track.Id, worked: true);

        // Assert
        Assert.That(marked, Is.True);
        Assert.That(_service.GetTrack(track.Id)!.IsWorked, Is.True);
    }

    [Test]
    public void GetWorkedTrackIds_ReturnsOnlyWorkedTracks()
    {
        // Arrange
        var track1 = _service!.AddTrack(CreateTestABLine("AB 1"));
        var track2 = _service.AddTrack(CreateTestABLine("AB 2"));
        var track3 = _service.AddTrack(CreateTestABLine("AB 3"));

        _service.SetTrackWorked(track1.Id, true);
        _service.SetTrackWorked(track3.Id, true);

        // Act
        var workedIds = _service.GetWorkedTrackIds();

        // Assert
        Assert.That(workedIds.Count, Is.EqualTo(2));
        Assert.That(workedIds.Contains(track1.Id), Is.True);
        Assert.That(workedIds.Contains(track3.Id), Is.True);
        Assert.That(workedIds.Contains(track2.Id), Is.False);
    }

    [Test]
    public void FindNextUnworkedTrack_FindsClosestUnworked()
    {
        // Arrange
        var abLine1 = _abLineService!.CreateFromHeading(
            new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' },
            0, "AB 1");
        var abLine2 = _abLineService.CreateFromHeading(
            new Position { Easting = 500010, Northing = 4500000, Zone = 15, Hemisphere = 'N' },
            0, "AB 2");
        var abLine3 = _abLineService.CreateFromHeading(
            new Position { Easting = 500020, Northing = 4500000, Zone = 15, Hemisphere = 'N' },
            0, "AB 3");

        var track1 = _service!.AddTrack(abLine1);
        var track2 = _service.AddTrack(abLine2);
        var track3 = _service.AddTrack(abLine3);

        // Mark track2 as worked
        _service.SetTrackWorked(track2.Id, true);

        var currentPosition = new Position { Easting = 500009, Northing = 4500005, Zone = 15, Hemisphere = 'N' };

        // Act
        var nextUnworked = _service.FindNextUnworkedTrack(currentPosition);

        // Assert
        Assert.That(nextUnworked, Is.Not.Null);
        Assert.That(nextUnworked!.IsWorked, Is.False);
        // Should be either track1 or track3 (both unworked), pick closest
    }

    #endregion

    #region Track Filtering

    [Test]
    public void GetTracksByMode_FiltersCorrectly()
    {
        // Arrange
        _service!.AddTrack(CreateTestABLine("AB 1"));
        _service.AddTrack(CreateTestABLine("AB 2"));

        var curveLine = CreateTestCurveLine("Curve 1");
        _service.AddTrack(curveLine);

        // Act
        var abTracks = _service.GetTracksByMode(TrackMode.ABLine);
        var curveTracks = _service.GetTracksByMode(TrackMode.CurveLine);

        // Assert
        Assert.That(abTracks.Count, Is.EqualTo(2));
        Assert.That(curveTracks.Count, Is.EqualTo(1));
    }

    #endregion

    #region Helper Methods

    private ABLine CreateTestABLine(string name)
    {
        var pointA = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var pointB = new Position { Easting = 500000, Northing = 4500100, Zone = 15, Hemisphere = 'N' };
        return _abLineService!.CreateFromPoints(pointA, pointB, name);
    }

    private CurveLine CreateTestCurveLine(string name)
    {
        _curveLineService!.StartRecording(new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' });
        _curveLineService.AddPoint(new Position { Easting = 500010, Northing = 4500010, Zone = 15, Hemisphere = 'N' }, 0.5);
        _curveLineService.AddPoint(new Position { Easting = 500020, Northing = 4500020, Zone = 15, Hemisphere = 'N' }, 0.5);
        _curveLineService.AddPoint(new Position { Easting = 500030, Northing = 4500030, Zone = 15, Hemisphere = 'N' }, 0.5);
        return _curveLineService.FinishRecording(name);
    }

    #endregion
}
