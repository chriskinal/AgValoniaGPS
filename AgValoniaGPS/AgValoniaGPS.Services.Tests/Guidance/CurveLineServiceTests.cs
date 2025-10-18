using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Guidance;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Guidance;

[TestFixture]
public class CurveLineServiceTests
{
    private CurveLineService _service;
    private const double Tolerance = 0.001; // 1mm tolerance

    [SetUp]
    public void SetUp()
    {
        _service = new CurveLineService();
    }

    #region Core Recording Tests

    [Test]
    public void StartRecording_ValidPosition_InitializesRecording()
    {
        // Arrange
        var startPos = new Position { Easting = 1000.0, Northing = 2000.0, Altitude = 0.0 };

        // Act
        _service.StartRecording(startPos);

        // Assert
        Assert.That(_service.IsRecording, Is.True);
    }

    [Test]
    public void AddPoint_WithinMinDistance_SkipsPoint()
    {
        // Arrange
        var startPos = new Position { Easting = 0.0, Northing = 0.0, Altitude = 0.0 };
        _service.StartRecording(startPos);

        int pointCount = 0;
        _service.CurveChanged += (s, e) =>
        {
            if (e.ChangeType == CurveLineChangeType.PointAdded)
                pointCount++;
        };

        var nearbyPoint = new Position { Easting = 0.2, Northing = 0.2, Altitude = 0.0 }; // ~0.28m away

        // Act
        _service.AddPoint(nearbyPoint, minDistanceMeters: 1.0); // 1.0m minimum distance

        // Assert
        Assert.That(pointCount, Is.EqualTo(0)); // Point should not be added
    }

    [Test]
    public void AddPoint_ExceedsMinDistance_AddsPoint()
    {
        // Arrange
        var startPos = new Position { Easting = 0.0, Northing = 0.0, Altitude = 0.0 };
        _service.StartRecording(startPos);

        int pointCount = 0;
        _service.CurveChanged += (s, e) =>
        {
            if (e.ChangeType == CurveLineChangeType.PointAdded)
                pointCount++;
        };

        var farPoint = new Position { Easting = 2.0, Northing = 0.0, Altitude = 0.0 }; // 2.0m away

        // Act
        _service.AddPoint(farPoint, minDistanceMeters: 1.0); // 1.0m minimum distance

        // Assert
        Assert.That(pointCount, Is.EqualTo(1)); // Point should be added
    }

    [Test]
    public void FinishRecording_SufficientPoints_CreatesCurveLine()
    {
        // Arrange
        var startPos = new Position { Easting = 0.0, Northing = 0.0, Altitude = 0.0 };
        _service.StartRecording(startPos);

        _service.AddPoint(new Position { Easting = 2.0, Northing = 0.0, Altitude = 0.0 }, 1.0);
        _service.AddPoint(new Position { Easting = 4.0, Northing = 0.0, Altitude = 0.0 }, 1.0);

        // Act
        var curve = _service.FinishRecording("Test Curve");

        // Assert
        Assert.That(curve, Is.Not.Null);
        Assert.That(curve.Name, Is.EqualTo("Test Curve"));
        Assert.That(curve.PointCount, Is.GreaterThanOrEqualTo(3)); // Start + 2 added points
        Assert.That(_service.IsRecording, Is.False);
    }

    [Test]
    public void FinishRecording_InsufficientPoints_ThrowsException()
    {
        // Arrange
        var startPos = new Position { Easting = 0.0, Northing = 0.0, Altitude = 0.0 };
        _service.StartRecording(startPos);
        // Only start position, no additional points added

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.FinishRecording("Invalid Curve"));
    }

    #endregion

    #region Guidance Calculation Tests

    [Test]
    public void CalculateGuidance_OnCurve_ReturnsSmallXTE()
    {
        // Arrange - create a simple straight curve for testing
        var curve = CreateStraightCurve();
        var onCurvePosition = new Position { Easting = 50.0, Northing = 0.0, Altitude = 0.0 };

        // Act
        var result = _service.CalculateGuidance(onCurvePosition, 0.0, curve, findGlobal: true);

        // Assert - should be very close to zero XTE
        Assert.That(Math.Abs(result.CrossTrackError), Is.LessThan(0.1));
    }

    [Test]
    public void GetClosestPoint_FindsNearestCurvePoint()
    {
        // Arrange
        var curve = CreateStraightCurve();
        var queryPosition = new Position { Easting = 50.0, Northing = 5.0, Altitude = 0.0 }; // 5m north of curve

        // Act
        var result = _service.GetClosestPoint(queryPosition, curve);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Distance, Is.GreaterThan(0));
        Assert.That(result.Index, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.Index, Is.LessThan(curve.PointCount));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create a simple straight curve for testing (100m east-west line).
    /// </summary>
    private CurveLine CreateStraightCurve()
    {
        var curve = new CurveLine
        {
            Name = "Test Curve",
            Points = new System.Collections.Generic.List<Position>()
        };

        // Create 11 points along east-west line (0 to 100m)
        for (int i = 0; i <= 10; i++)
        {
            curve.Points.Add(new Position
            {
                Easting = i * 10.0,
                Northing = 0.0,
                Altitude = 0.0
            });
        }

        return curve;
    }

    #endregion
}
