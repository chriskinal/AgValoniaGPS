using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests;

/// <summary>
/// Tests for MathConstants and MathUtilities
/// Validates mathematical constants and utility methods
/// </summary>
[TestFixture]
public class MathUtilitiesTests
{
    #region Constants Tests

    [Test]
    public void MathConstants_AngularConstants_AreCorrect()
    {
        // Verify angular constants
        Assert.That(MathConstants.TwoPI, Is.EqualTo(Math.PI * 2).Within(1e-10));
        Assert.That(MathConstants.PIBy2, Is.EqualTo(Math.PI / 2).Within(1e-10));
    }

    [Test]
    public void MathConstants_DegreeRadianConversions_AreInverse()
    {
        // Verify degree/radian conversion factors are inverses
        Assert.That(MathConstants.DegreesToRadians * MathConstants.RadiansToDegrees,
            Is.EqualTo(1.0).Within(1e-10));
    }

    [Test]
    public void MathConstants_LengthConversions_AreInverse()
    {
        // Meters <-> Feet
        Assert.That(MathConstants.MetersToFeet * MathConstants.FeetToMeters,
            Is.EqualTo(1.0).Within(1e-6));

        // Meters <-> Inches
        Assert.That(MathConstants.MetersToInches * MathConstants.InchesToMeters,
            Is.EqualTo(1.0).Within(1e-6));

        // Meters <-> Miles (larger tolerance due to rounding)
        Assert.That(MathConstants.MetersToMiles * MathConstants.MilesToMeters,
            Is.EqualTo(1.0).Within(1e-5));

        // Meters <-> Kilometers
        Assert.That(MathConstants.MetersToKilometers * MathConstants.KilometersToMeters,
            Is.EqualTo(1.0).Within(1e-6));
    }

    [Test]
    public void MathConstants_AreaConversions_AreInverse()
    {
        // Hectares <-> Acres
        Assert.That(MathConstants.HectaresToAcres * MathConstants.AcresToHectares,
            Is.EqualTo(1.0).Within(1e-6));

        // Hectares <-> Square Meters
        Assert.That(MathConstants.HectaresToSquareMeters * MathConstants.SquareMetersToHectares,
            Is.EqualTo(1.0).Within(1e-6));

        // Acres <-> Square Meters
        Assert.That(MathConstants.AcresToSquareMeters * MathConstants.SquareMetersToAcres,
            Is.EqualTo(1.0).Within(1e-6));
    }

    [Test]
    public void MathConstants_VolumeConversions_AreInverse()
    {
        // Liters <-> Gallons
        Assert.That(MathConstants.LitersToGallons * MathConstants.GallonsToLiters,
            Is.EqualTo(1.0).Within(1e-6));

        // Gal/Ac <-> L/Ha
        Assert.That(MathConstants.GallonsPerAcreToLitersPerHectare * MathConstants.LitersPerHectareToGallonsPerAcre,
            Is.EqualTo(1.0).Within(1e-5));
    }

    #endregion

    #region Angular Operations Tests

    [Test]
    public void ToDegrees_ConvertsRadiansToDegrees()
    {
        // Test common angles
        Assert.That(MathUtilities.ToDegrees(0), Is.EqualTo(0).Within(1e-6));
        Assert.That(MathUtilities.ToDegrees(Math.PI), Is.EqualTo(180).Within(1e-6));
        Assert.That(MathUtilities.ToDegrees(Math.PI / 2), Is.EqualTo(90).Within(1e-6));
        Assert.That(MathUtilities.ToDegrees(2 * Math.PI), Is.EqualTo(360).Within(1e-6));
    }

    [Test]
    public void ToRadians_ConvertsDegreesToRadians()
    {
        // Test common angles
        Assert.That(MathUtilities.ToRadians(0), Is.EqualTo(0).Within(1e-10));
        Assert.That(MathUtilities.ToRadians(180), Is.EqualTo(Math.PI).Within(1e-10));
        Assert.That(MathUtilities.ToRadians(90), Is.EqualTo(Math.PI / 2).Within(1e-10));
        Assert.That(MathUtilities.ToRadians(360), Is.EqualTo(2 * Math.PI).Within(1e-10));
    }

    [Test]
    public void AngleDifference_CalculatesSmallestDifference()
    {
        // Test same angle
        Assert.That(MathUtilities.AngleDifference(Math.PI / 2, Math.PI / 2), Is.EqualTo(0).Within(1e-10));

        // Test small difference
        Assert.That(MathUtilities.AngleDifference(0, Math.PI / 4), Is.EqualTo(Math.PI / 4).Within(1e-10));

        // Test wraparound (should take shorter path)
        Assert.That(MathUtilities.AngleDifference(0.1, 2 * Math.PI - 0.1), Is.EqualTo(0.2).Within(1e-10));

        // Test opposite angles
        Assert.That(MathUtilities.AngleDifference(0, Math.PI), Is.EqualTo(Math.PI).Within(1e-10));
    }

    [Test]
    public void NormalizeAngle_WrapsToZeroTo2Pi()
    {
        // Test positive wraparound
        Assert.That(MathUtilities.NormalizeAngle(3 * Math.PI), Is.EqualTo(Math.PI).Within(1e-10));

        // Test negative wraparound
        Assert.That(MathUtilities.NormalizeAngle(-Math.PI / 2), Is.EqualTo(1.5 * Math.PI).Within(1e-10));

        // Test already normalized
        Assert.That(MathUtilities.NormalizeAngle(Math.PI / 2), Is.EqualTo(Math.PI / 2).Within(1e-10));
    }

    #endregion

    #region Distance Calculations Tests

    [Test]
    public void Distance_CalculatesCorrectEuclideanDistance()
    {
        // Test zero distance
        Assert.That(MathUtilities.Distance(0, 0, 0, 0), Is.EqualTo(0).Within(1e-10));

        // Test 3-4-5 triangle
        Assert.That(MathUtilities.Distance(0, 0, 3, 4), Is.EqualTo(5).Within(1e-10));

        // Test negative coordinates
        Assert.That(MathUtilities.Distance(-5, -5, -2, -1), Is.EqualTo(5).Within(1e-10));
    }

    [Test]
    public void Distance_WithPositions_CalculatesCorrectly()
    {
        // Arrange
        var pos1 = new Position { Easting = 500000, Northing = 4500000, Zone = 15, Hemisphere = 'N' };
        var pos2 = new Position { Easting = 500003, Northing = 4500004, Zone = 15, Hemisphere = 'N' };

        // Act
        double dist = MathUtilities.Distance(pos1, pos2);

        // Assert (3-4-5 triangle)
        Assert.That(dist, Is.EqualTo(5).Within(1e-10));
    }

    [Test]
    public void DistanceSquared_IsSquareOfDistance()
    {
        // Arrange
        double east1 = 100, north1 = 200;
        double east2 = 103, north2 = 204;

        // Act
        double dist = MathUtilities.Distance(east1, north1, east2, north2);
        double distSq = MathUtilities.DistanceSquared(east1, north1, east2, north2);

        // Assert
        Assert.That(distSq, Is.EqualTo(dist * dist).Within(1e-10));
    }

    #endregion

    #region Geometric Tests

    [Test]
    public void IsPointInRangeBetweenAB_PointOnSegment_ReturnsTrue()
    {
        // Point exactly in the middle of segment
        bool result = MathUtilities.IsPointInRangeBetweenAB(0, 0, 10, 0, 5, 0);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsPointInRangeBetweenAB_PointBeyondSegment_ReturnsFalse()
    {
        // Point beyond end of segment
        bool result = MathUtilities.IsPointInRangeBetweenAB(0, 0, 10, 0, 15, 0);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsPointInRangeBetweenAB_PointBeforeSegment_ReturnsFalse()
    {
        // Point before start of segment
        bool result = MathUtilities.IsPointInRangeBetweenAB(0, 0, 10, 0, -5, 0);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsPointInRangeBetweenAB_PointOffLine_CanBeInRange()
    {
        // Point off the line but projects onto segment
        bool result = MathUtilities.IsPointInRangeBetweenAB(0, 0, 10, 0, 5, 3);
        Assert.That(result, Is.True, "Point should project onto segment");
    }

    #endregion

    #region Catmull-Rom Spline Tests

    [Test]
    public void CatmullRom_AtT0_ReturnsP1()
    {
        // Arrange
        var p0 = CreatePosition(0, 0);
        var p1 = CreatePosition(10, 10);
        var p2 = CreatePosition(20, 10);
        var p3 = CreatePosition(30, 0);

        // Act
        var result = MathUtilities.CatmullRom(0, p0, p1, p2, p3);

        // Assert
        Assert.That(result.Easting, Is.EqualTo(p1.Easting).Within(1e-6));
        Assert.That(result.Northing, Is.EqualTo(p1.Northing).Within(1e-6));
    }

    [Test]
    public void CatmullRom_AtT1_ReturnsP2()
    {
        // Arrange
        var p0 = CreatePosition(0, 0);
        var p1 = CreatePosition(10, 10);
        var p2 = CreatePosition(20, 10);
        var p3 = CreatePosition(30, 0);

        // Act
        var result = MathUtilities.CatmullRom(1, p0, p1, p2, p3);

        // Assert
        Assert.That(result.Easting, Is.EqualTo(p2.Easting).Within(1e-6));
        Assert.That(result.Northing, Is.EqualTo(p2.Northing).Within(1e-6));
    }

    [Test]
    public void CatmullRomPath_GeneratesSmoothCurve()
    {
        // Arrange
        var controlPoints = new List<Position>
        {
            CreatePosition(0, 0),
            CreatePosition(10, 10),
            CreatePosition(20, 10),
            CreatePosition(30, 0)
        };

        // Act
        var curve = MathUtilities.CatmullRomPath(controlPoints, segmentsPerSpan: 5);

        // Assert
        Assert.That(curve.Count, Is.GreaterThan(controlPoints.Count));
        Assert.That(curve[0].Easting, Is.EqualTo(controlPoints[0].Easting).Within(1e-6));
        Assert.That(curve[curve.Count - 1].Easting, Is.EqualTo(controlPoints[controlPoints.Count - 1].Easting).Within(1e-6));
    }

    #endregion

    #region Raycasting Tests

    [Test]
    public void TryRaySegmentIntersection_RayHitsSegment_ReturnsTrue()
    {
        // Arrange: Ray from (0,0) pointing right, segment from (5,-1) to (5,1)
        bool hit = MathUtilities.TryRaySegmentIntersection(
            0, 0,      // Ray origin
            1, 0,      // Ray direction (pointing right)
            5, -1,     // Segment point A
            5, 1,      // Segment point B
            out double hitEast, out double hitNorth);

        // Assert
        Assert.That(hit, Is.True);
        Assert.That(hitEast, Is.EqualTo(5).Within(1e-10));
        Assert.That(hitNorth, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void TryRaySegmentIntersection_RayMissesSegment_ReturnsFalse()
    {
        // Arrange: Ray pointing away from segment
        bool hit = MathUtilities.TryRaySegmentIntersection(
            0, 0,      // Ray origin
            -1, 0,     // Ray direction (pointing left)
            5, -1,     // Segment point A (to the right)
            5, 1,      // Segment point B
            out _, out _);

        // Assert
        Assert.That(hit, Is.False);
    }

    [Test]
    public void TryRaySegmentIntersection_ParallelRayAndSegment_ReturnsFalse()
    {
        // Arrange: Ray and segment both horizontal
        bool hit = MathUtilities.TryRaySegmentIntersection(
            0, 0,      // Ray origin
            1, 0,      // Ray direction (horizontal)
            0, 5,      // Segment point A (horizontal, above ray)
            10, 5,     // Segment point B
            out _, out _);

        // Assert
        Assert.That(hit, Is.False);
    }

    [Test]
    public void RaycastToPolygon_FindsClosestIntersection()
    {
        // Arrange: Square polygon, ray from center
        var polygon = new List<Position>
        {
            CreatePosition(0, 0),
            CreatePosition(10, 0),
            CreatePosition(10, 10),
            CreatePosition(0, 10)
        };

        var origin = CreatePosition(5, 5);
        origin = origin with { Heading = 0 }; // Pointing north

        // Act
        bool hit = MathUtilities.RaycastToPolygon(origin, polygon, out double hitEast, out double hitNorth);

        // Assert
        Assert.That(hit, Is.True);
        Assert.That(hitEast, Is.EqualTo(5).Within(1e-6));
        Assert.That(hitNorth, Is.EqualTo(10).Within(1e-6));
    }

    #endregion

    #region Utility Methods Tests

    [Test]
    public void Clamp_ReturnsClampedValue()
    {
        Assert.That(MathUtilities.Clamp(5, 0, 10), Is.EqualTo(5));
        Assert.That(MathUtilities.Clamp(-5, 0, 10), Is.EqualTo(0));
        Assert.That(MathUtilities.Clamp(15, 0, 10), Is.EqualTo(10));
    }

    [Test]
    public void Lerp_InterpolatesLinearly()
    {
        Assert.That(MathUtilities.Lerp(0, 10, 0), Is.EqualTo(0).Within(1e-10));
        Assert.That(MathUtilities.Lerp(0, 10, 1), Is.EqualTo(10).Within(1e-10));
        Assert.That(MathUtilities.Lerp(0, 10, 0.5), Is.EqualTo(5).Within(1e-10));
        Assert.That(MathUtilities.Lerp(0, 10, 0.25), Is.EqualTo(2.5).Within(1e-10));
    }

    #endregion

    #region Vector Operations Tests

    [Test]
    public void Cross_WithComponentsZeroVectors_ReturnsZero()
    {
        // Arrange & Act
        double result = MathUtilities.Cross(0, 0, 0, 0);

        // Assert
        Assert.That(result, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void Cross_WithPerpendicularVectors_ReturnsAreaOfParallelogram()
    {
        // Arrange: (3,0) x (0,4) = 12 (area of 3x4 rectangle)
        double result = MathUtilities.Cross(3, 0, 0, 4);

        // Assert
        Assert.That(result, Is.EqualTo(12).Within(1e-10));
    }

    [Test]
    public void Cross_WithParallelVectors_ReturnsZero()
    {
        // Arrange: (2,3) x (4,6) = 0 (parallel vectors)
        double result = MathUtilities.Cross(2, 3, 4, 6);

        // Assert
        Assert.That(result, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void Cross_WithPosition2D_CalculatesCorrectly()
    {
        // Arrange
        var a = new Position2D(3, 0);
        var b = new Position2D(0, 4);

        // Act
        double result = MathUtilities.Cross(a, b);

        // Assert
        Assert.That(result, Is.EqualTo(12).Within(1e-10));
    }

    [Test]
    public void Cross_CounterClockwiseVectors_ReturnsPositive()
    {
        // Arrange: Vector b is 90° counter-clockwise from a
        var a = new Position2D(1, 0);
        var b = new Position2D(0, 1);

        // Act
        double result = MathUtilities.Cross(a, b);

        // Assert
        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    public void Cross_ClockwiseVectors_ReturnsNegative()
    {
        // Arrange: Vector b is 90° clockwise from a
        var a = new Position2D(0, 1);
        var b = new Position2D(1, 0);

        // Act
        double result = MathUtilities.Cross(a, b);

        // Assert
        Assert.That(result, Is.LessThan(0));
    }

    [Test]
    public void Dot_WithZeroVectors_ReturnsZero()
    {
        // Arrange & Act
        double result = MathUtilities.Dot(0, 0, 0, 0);

        // Assert
        Assert.That(result, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void Dot_WithPerpendicularVectors_ReturnsZero()
    {
        // Arrange: (1,0) · (0,1) = 0
        double result = MathUtilities.Dot(1, 0, 0, 1);

        // Assert
        Assert.That(result, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void Dot_WithParallelVectors_ReturnsProductOfMagnitudes()
    {
        // Arrange: (3,0) · (4,0) = 12
        double result = MathUtilities.Dot(3, 0, 4, 0);

        // Assert
        Assert.That(result, Is.EqualTo(12).Within(1e-10));
    }

    [Test]
    public void Dot_WithPosition2D_CalculatesCorrectly()
    {
        // Arrange
        var a = new Position2D(3, 4);
        var b = new Position2D(5, 12);

        // Act: 3*5 + 4*12 = 15 + 48 = 63
        double result = MathUtilities.Dot(a, b);

        // Assert
        Assert.That(result, Is.EqualTo(63).Within(1e-10));
    }

    [Test]
    public void HeadingXZ_WithNorthVector_ReturnsZero()
    {
        // Arrange: Vector pointing north (0,1)
        double result = MathUtilities.HeadingXZ(0, 1);

        // Assert
        Assert.That(result, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void HeadingXZ_WithEastVector_Returns90Degrees()
    {
        // Arrange: Vector pointing east (1,0)
        double result = MathUtilities.HeadingXZ(1, 0);

        // Assert
        Assert.That(result, Is.EqualTo(Math.PI / 2).Within(1e-10));
    }

    [Test]
    public void HeadingXZ_WithSouthVector_Returns180Degrees()
    {
        // Arrange: Vector pointing south (0,-1)
        double result = MathUtilities.HeadingXZ(0, -1);

        // Assert (Atan2 returns -PI for south)
        Assert.That(Math.Abs(result), Is.EqualTo(Math.PI).Within(1e-10));
    }

    [Test]
    public void HeadingXZ_WithPosition2D_CalculatesCorrectly()
    {
        // Arrange: Northeast direction (1,1)
        var vector = new Position2D(1, 1);

        // Act
        double result = MathUtilities.HeadingXZ(vector);

        // Assert: 45 degrees = PI/4
        Assert.That(result, Is.EqualTo(Math.PI / 4).Within(1e-10));
    }

    [Test]
    public void Magnitude_WithZeroVector_ReturnsZero()
    {
        // Arrange & Act
        double result = MathUtilities.Magnitude(0, 0);

        // Assert
        Assert.That(result, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void Magnitude_WithUnitVector_ReturnsOne()
    {
        // Arrange: (0.6, 0.8) is unit vector (3-4-5 triangle scaled)
        double result = MathUtilities.Magnitude(0.6, 0.8);

        // Assert
        Assert.That(result, Is.EqualTo(1.0).Within(1e-10));
    }

    [Test]
    public void Magnitude_WithGeneralVector_CalculatesCorrectly()
    {
        // Arrange: 3-4-5 triangle
        double result = MathUtilities.Magnitude(3, 4);

        // Assert
        Assert.That(result, Is.EqualTo(5).Within(1e-10));
    }

    [Test]
    public void TryNormalize_WithZeroVector_ReturnsFalse()
    {
        // Arrange & Act
        bool success = MathUtilities.TryNormalize(0, 0, out double outEast, out double outNorth);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(outEast, Is.EqualTo(0));
        Assert.That(outNorth, Is.EqualTo(0));
    }

    [Test]
    public void TryNormalize_WithNonZeroVector_ReturnsUnitVector()
    {
        // Arrange: 3-4-5 triangle
        bool success = MathUtilities.TryNormalize(3, 4, out double outEast, out double outNorth);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(outEast, Is.EqualTo(0.6).Within(1e-10));
        Assert.That(outNorth, Is.EqualTo(0.8).Within(1e-10));

        // Verify unit length
        double length = Math.Sqrt(outEast * outEast + outNorth * outNorth);
        Assert.That(length, Is.EqualTo(1.0).Within(1e-10));
    }

    [Test]
    public void PerpendicularLeft_RotatesVector90DegreesCounterClockwise()
    {
        // Arrange: East vector (1,0)
        MathUtilities.PerpendicularLeft(1, 0, out double outEast, out double outNorth);

        // Assert: Should point north (0,1)
        Assert.That(outEast, Is.EqualTo(0).Within(1e-10));
        Assert.That(outNorth, Is.EqualTo(1).Within(1e-10));
    }

    [Test]
    public void PerpendicularLeft_WithPosition2D_RotatesCorrectly()
    {
        // Arrange: East vector
        var vector = new Position2D(1, 0);

        // Act
        var result = MathUtilities.PerpendicularLeft(vector);

        // Assert: Should point north
        Assert.That(result.Easting, Is.EqualTo(0).Within(1e-10));
        Assert.That(result.Northing, Is.EqualTo(1).Within(1e-10));
    }

    [Test]
    public void PerpendicularRight_RotatesVector90DegreesClockwise()
    {
        // Arrange: North vector (0,1)
        MathUtilities.PerpendicularRight(0, 1, out double outEast, out double outNorth);

        // Assert: Should point east (1,0)
        Assert.That(outEast, Is.EqualTo(1).Within(1e-10));
        Assert.That(outNorth, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void PerpendicularRight_WithPosition2D_RotatesCorrectly()
    {
        // Arrange: North vector
        var vector = new Position2D(0, 1);

        // Act
        var result = MathUtilities.PerpendicularRight(vector);

        // Assert: Should point east
        Assert.That(result.Easting, Is.EqualTo(1).Within(1e-10));
        Assert.That(result.Northing, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void PerpendicularLeft_ThenRight_ReturnsOriginal()
    {
        // Arrange
        var original = new Position2D(3, 4);

        // Act
        var left = MathUtilities.PerpendicularLeft(original);
        var back = MathUtilities.PerpendicularRight(left);

        // Assert
        Assert.That(back.Easting, Is.EqualTo(original.Easting).Within(1e-10));
        Assert.That(back.Northing, Is.EqualTo(original.Northing).Within(1e-10));
    }

    [Test]
    public void ProjectOnSegment_WithPointOnSegment_ReturnsPoint()
    {
        // Arrange: Point exactly on segment
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(5, 0);

        // Act
        var projected = MathUtilities.ProjectOnSegment(start, end, point, out double t);

        // Assert
        Assert.That(t, Is.EqualTo(0.5).Within(1e-10));
        Assert.That(projected.Easting, Is.EqualTo(5).Within(1e-10));
        Assert.That(projected.Northing, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void ProjectOnSegment_WithPointAboveLine_ProjectsPerpendicularly()
    {
        // Arrange: Horizontal segment, point above
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(5, 5);

        // Act
        var projected = MathUtilities.ProjectOnSegment(start, end, point, out double t);

        // Assert
        Assert.That(t, Is.EqualTo(0.5).Within(1e-10));
        Assert.That(projected.Easting, Is.EqualTo(5).Within(1e-10));
        Assert.That(projected.Northing, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void ProjectOnSegment_WithPointBeyondEnd_ClampsToBoundary()
    {
        // Arrange: Point beyond end of segment
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(15, 0);

        // Act
        var projected = MathUtilities.ProjectOnSegment(start, end, point, out double t);

        // Assert
        Assert.That(t, Is.EqualTo(1.0).Within(1e-10));
        Assert.That(projected.Easting, Is.EqualTo(10).Within(1e-10));
        Assert.That(projected.Northing, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void ProjectOnSegment_WithPointBeforeStart_ClampsToStart()
    {
        // Arrange: Point before start of segment
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(-5, 0);

        // Act
        var projected = MathUtilities.ProjectOnSegment(start, end, point, out double t);

        // Assert
        Assert.That(t, Is.EqualTo(0.0).Within(1e-10));
        Assert.That(projected.Easting, Is.EqualTo(0).Within(1e-10));
        Assert.That(projected.Northing, Is.EqualTo(0).Within(1e-10));
    }

    [Test]
    public void ProjectOnSegment_WithDegenerateSegment_ReturnsStart()
    {
        // Arrange: Segment with zero length (start == end)
        var start = new Position2D(5, 5);
        var end = new Position2D(5, 5);
        var point = new Position2D(10, 10);

        // Act
        var projected = MathUtilities.ProjectOnSegment(start, end, point, out double t);

        // Assert
        Assert.That(t, Is.EqualTo(0.0).Within(1e-10));
        Assert.That(projected.Easting, Is.EqualTo(5).Within(1e-10));
        Assert.That(projected.Northing, Is.EqualTo(5).Within(1e-10));
    }

    [Test]
    public void IsPointOnSegment_WithPointOnSegment_ReturnsTrue()
    {
        // Arrange
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(5, 0);

        // Act
        bool result = MathUtilities.IsPointOnSegment(start, end, point);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsPointOnSegment_WithPointOffSegmentButOnLine_ReturnsTrue()
    {
        // Arrange: Point projects onto segment even though off the line
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(5, 3);

        // Act
        bool result = MathUtilities.IsPointOnSegment(start, end, point);

        // Assert
        Assert.That(result, Is.True, "Point should project onto segment");
    }

    [Test]
    public void IsPointOnSegment_WithPointBeyondEnd_ReturnsFalse()
    {
        // Arrange
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(15, 0);

        // Act
        bool result = MathUtilities.IsPointOnSegment(start, end, point);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsPointOnSegment_WithPointBeforeStart_ReturnsFalse()
    {
        // Arrange
        var start = new Position2D(0, 0);
        var end = new Position2D(10, 0);
        var point = new Position2D(-5, 0);

        // Act
        bool result = MathUtilities.IsPointOnSegment(start, end, point);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsPointOnSegment_WithDegenerateSegment_ReturnsFalse()
    {
        // Arrange: Zero-length segment
        var start = new Position2D(5, 5);
        var end = new Position2D(5, 5);
        var point = new Position2D(5, 5);

        // Act
        bool result = MathUtilities.IsPointOnSegment(start, end, point);

        // Assert
        Assert.That(result, Is.False, "Degenerate segment should return false");
    }

    #endregion

    #region Helper Methods

    private Position CreatePosition(double easting, double northing)
    {
        return new Position
        {
            Easting = easting,
            Northing = northing,
            Zone = 15,
            Hemisphere = 'N'
        };
    }

    #endregion
}
