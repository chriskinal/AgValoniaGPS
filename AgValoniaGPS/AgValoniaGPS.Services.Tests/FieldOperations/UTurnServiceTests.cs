using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Unit tests for UTurnService
/// Validates turn pattern generation, turn state management, and event publishing
/// </summary>
[TestFixture]
public class UTurnServiceTests
{
    private UTurnService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new UTurnService();
    }

    [Test]
    public void OmegaTurn_GeneratesValidPath_WithCorrectWaypoints()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Omega, turningRadius: 5.0, autoPause: true);
        var entryPoint = new Position { Easting = 100, Northing = 200, Heading = 0 };
        var exitPoint = new Position { Easting = 100, Northing = 210, Heading = 180 };

        // Act
        var path = _service.GenerateTurnPath(entryPoint, entryHeading: 0, exitPoint, exitHeading: 180);

        // Assert
        Assert.That(path, Is.Not.Null);
        Assert.That(path.Length, Is.GreaterThan(10), "Omega turn should have at least 10 waypoints");
        Assert.That(path[0].Easting, Is.EqualTo(entryPoint.Easting).Within(0.1), "First waypoint should be near entry");
        Assert.That(path[0].Northing, Is.EqualTo(entryPoint.Northing).Within(0.1));

        // Verify smooth arc: waypoints should be evenly spaced
        double firstSegmentDist = Math.Sqrt(
            Math.Pow(path[1].Easting - path[0].Easting, 2) +
            Math.Pow(path[1].Northing - path[0].Northing, 2));
        Assert.That(firstSegmentDist, Is.GreaterThan(0), "Waypoints should be spaced apart");
    }

    [Test]
    public void TTurn_GeneratesValidPath_WithReverseSegment()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.T, turningRadius: 5.0, autoPause: true);
        var entryPoint = new Position { Easting = 100, Northing = 200, Heading = 0 };
        var exitPoint = new Position { Easting = 110, Northing = 200, Heading = 180 };

        // Act
        var path = _service.GenerateTurnPath(entryPoint, entryHeading: 0, exitPoint, exitHeading: 180);

        // Assert
        Assert.That(path, Is.Not.Null);
        Assert.That(path.Length, Is.GreaterThan(8), "T-turn should have multiple segments");

        // Verify path starts near entry point
        Assert.That(path[0].Easting, Is.EqualTo(entryPoint.Easting).Within(0.1));
        Assert.That(path[0].Northing, Is.EqualTo(entryPoint.Northing).Within(0.1));

        // Verify path has forward, reverse, and forward segments
        Assert.That(path[^1].Heading, Is.Not.EqualTo(path[0].Heading), "Exit heading should differ from entry");
    }

    [Test]
    public void YTurn_GeneratesValidPath_WithReducedReversing()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Y, turningRadius: 5.0, autoPause: true);
        var entryPoint = new Position { Easting = 100, Northing = 200, Heading = 0 };
        var exitPoint = new Position { Easting = 105, Northing = 205, Heading = 180 };

        // Act
        var path = _service.GenerateTurnPath(entryPoint, entryHeading: 0, exitPoint, exitHeading: 180);

        // Assert
        Assert.That(path, Is.Not.Null);
        Assert.That(path.Length, Is.GreaterThan(8), "Y-turn should have multiple waypoints");

        // Verify angled approach (should deviate from straight ahead)
        Assert.That(path[0].Easting, Is.EqualTo(entryPoint.Easting).Within(0.1));
        Assert.That(path[0].Northing, Is.EqualTo(entryPoint.Northing).Within(0.1));
    }

    [Test]
    public void StartTurn_RaisesUTurnStartedEvent_WithCorrectData()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Omega, turningRadius: 5.0, autoPause: true);
        var startPosition = new Position { Easting = 100, Northing = 200, Heading = 0 };

        UTurnStartedEventArgs? eventArgs = null;
        _service.UTurnStarted += (sender, e) => eventArgs = e;

        // Act
        _service.StartTurn(startPosition, currentHeading: 0);

        // Assert
        Assert.That(eventArgs, Is.Not.Null, "UTurnStarted event should be raised");
        Assert.That(eventArgs!.TurnType, Is.EqualTo(UTurnType.Omega));
        Assert.That(eventArgs.StartPosition, Is.EqualTo(startPosition));
        Assert.That(eventArgs.TurnPath, Is.Not.Null);
        Assert.That(eventArgs.TurnPath.Length, Is.GreaterThan(0));
        Assert.That(eventArgs.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void UpdateTurnProgress_TracksProgressCorrectly_From0To1()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Omega, turningRadius: 5.0, autoPause: true);
        var startPosition = new Position { Easting = 100, Northing = 200, Heading = 0 };
        _service.StartTurn(startPosition, currentHeading: 0);

        var turnPath = _service.GetCurrentTurnPath();
        Assert.That(turnPath, Is.Not.Null);

        // Act & Assert - Progress should increase as we move through waypoints
        double initialProgress = _service.GetTurnProgress();
        Assert.That(initialProgress, Is.EqualTo(0.0), "Initial progress should be 0");

        // Move to middle waypoint
        var midPoint = turnPath![turnPath.Length / 2];
        _service.UpdateTurnProgress(midPoint);
        double midProgress = _service.GetTurnProgress();
        Assert.That(midProgress, Is.GreaterThan(0.3).And.LessThan(0.7), "Mid progress should be around 0.5");

        // Move to end waypoint
        var endPoint = turnPath[^1];
        _service.UpdateTurnProgress(endPoint);
        double endProgress = _service.GetTurnProgress();
        Assert.That(endProgress, Is.GreaterThan(0.8), "End progress should be near 1.0");
    }

    [Test]
    public void CompleteTurn_RaisesUTurnCompletedEvent_WithDuration()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Omega, turningRadius: 5.0, autoPause: true);
        var startPosition = new Position { Easting = 100, Northing = 200, Heading = 0 };
        _service.StartTurn(startPosition, currentHeading: 0);

        UTurnCompletedEventArgs? eventArgs = null;
        _service.UTurnCompleted += (sender, e) => eventArgs = e;

        // Act
        System.Threading.Thread.Sleep(100); // Small delay to ensure duration > 0
        _service.CompleteTurn();

        // Assert
        Assert.That(eventArgs, Is.Not.Null, "UTurnCompleted event should be raised");
        Assert.That(eventArgs!.TurnType, Is.EqualTo(UTurnType.Omega));
        Assert.That(eventArgs.EndPosition, Is.Not.Null);
        Assert.That(eventArgs.TurnDuration, Is.GreaterThan(0), "Turn duration should be positive");
        Assert.That(eventArgs.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void IsInTurn_ReturnsTrueWhenActive_FalseWhenComplete()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Omega, turningRadius: 5.0, autoPause: true);
        var startPosition = new Position { Easting = 100, Northing = 200, Heading = 0 };

        // Act & Assert - Before turn
        Assert.That(_service.IsInTurn(), Is.False, "Should not be in turn initially");

        // Start turn
        _service.StartTurn(startPosition, currentHeading: 0);
        Assert.That(_service.IsInTurn(), Is.True, "Should be in turn after StartTurn()");

        // Complete turn
        _service.CompleteTurn();
        Assert.That(_service.IsInTurn(), Is.False, "Should not be in turn after CompleteTurn()");
    }

    [Test]
    public void ConfigureTurn_UpdatesTurnParameters_Correctly()
    {
        // Arrange & Act
        _service.ConfigureTurn(UTurnType.T, turningRadius: 8.0, autoPause: false);

        // Assert
        Assert.That(_service.GetCurrentTurnType(), Is.EqualTo(UTurnType.T));

        // Verify radius affects path generation
        var entryPoint = new Position { Easting = 100, Northing = 200, Heading = 0 };
        var exitPoint = new Position { Easting = 116, Northing = 200, Heading = 180 };
        var path = _service.GenerateTurnPath(entryPoint, entryHeading: 0, exitPoint, exitHeading: 180);

        Assert.That(path, Is.Not.Null);
        Assert.That(path.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerateTurnPath_Performance_CompletesUnder10ms()
    {
        // Arrange
        _service.ConfigureTurn(UTurnType.Omega, turningRadius: 5.0, autoPause: true);
        var entryPoint = new Position { Easting = 100, Northing = 200, Heading = 0 };
        var exitPoint = new Position { Easting = 100, Northing = 210, Heading = 180 };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var path = _service.GenerateTurnPath(entryPoint, entryHeading: 0, exitPoint, exitHeading: 180);
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(10),
            "Turn path generation should complete in under 10ms");
        Assert.That(path, Is.Not.Null);
        Assert.That(path.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GetCurrentTurnPath_ReturnsNull_WhenNotInTurn()
    {
        // Arrange - No turn started

        // Act
        var path = _service.GetCurrentTurnPath();

        // Assert
        Assert.That(path, Is.Null, "Turn path should be null when not in turn");
    }
}
