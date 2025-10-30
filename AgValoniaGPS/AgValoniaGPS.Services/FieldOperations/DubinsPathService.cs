using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Extensions;
using AgValoniaGPS.Models.FieldOperations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implementation of Dubins path generation service.
/// Calculates the shortest path between two poses with a given turning radius.
/// </summary>
public class DubinsPathService : IDubinsPathService
{
    private const double HalfPi = Math.PI / 2.0;

    public DubinsPath? GeneratePath(
        double startEasting,
        double startNorthing,
        double startHeading,
        double goalEasting,
        double goalNorthing,
        double goalHeading,
        double turningRadius,
        double waypointSpacing = 0.05)
    {
        var paths = GenerateAllPaths(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            turningRadius, waypointSpacing);

        return paths.Count > 0 ? paths[0] : null;
    }

    public List<DubinsPath> GenerateAllPaths(
        double startEasting,
        double startNorthing,
        double startHeading,
        double goalEasting,
        double goalNorthing,
        double goalHeading,
        double turningRadius,
        double waypointSpacing = 0.05)
    {
        var startPos = new Position2D(startEasting, startNorthing);
        var goalPos = new Position2D(goalEasting, goalNorthing);

        // Calculate circle centers for start and goal positions
        var startRightCircle = GetRightCircleCenter(startPos, startHeading, turningRadius);
        var startLeftCircle = GetLeftCircleCenter(startPos, startHeading, turningRadius);
        var goalRightCircle = GetRightCircleCenter(goalPos, goalHeading, turningRadius);
        var goalLeftCircle = GetLeftCircleCenter(goalPos, goalHeading, turningRadius);

        var paths = new List<DubinsPath>();

        // Calculate all valid path types
        // RSR - Only valid if circles don't have the same position
        if (!PositionsEqual(startRightCircle, goalRightCircle))
        {
            var rsrPath = CalculateRSR(startPos, goalPos, startHeading, goalHeading,
                startRightCircle, goalRightCircle, turningRadius);
            if (rsrPath != null) paths.Add(rsrPath);
        }

        // LSL - Only valid if circles don't have the same position
        if (!PositionsEqual(startLeftCircle, goalLeftCircle))
        {
            var lslPath = CalculateLSL(startPos, goalPos, startHeading, goalHeading,
                startLeftCircle, goalLeftCircle, turningRadius);
            if (lslPath != null) paths.Add(lslPath);
        }

        // RSL and LSR - Only valid if circles don't intersect
        double comparisonSqr = (2.0 * turningRadius) * (2.0 * turningRadius);

        if ((startRightCircle - goalLeftCircle).GetLengthSquared() > comparisonSqr)
        {
            var rslPath = CalculateRSL(startPos, goalPos, startHeading, goalHeading,
                startRightCircle, goalLeftCircle, turningRadius);
            if (rslPath != null) paths.Add(rslPath);
        }

        if ((startLeftCircle - goalRightCircle).GetLengthSquared() > comparisonSqr)
        {
            var lsrPath = CalculateLSR(startPos, goalPos, startHeading, goalHeading,
                startLeftCircle, goalRightCircle, turningRadius);
            if (lsrPath != null) paths.Add(lsrPath);
        }

        // RLR and LRL - Only valid if distance between circles is less than 4 * radius
        comparisonSqr = (4.0 * turningRadius) * (4.0 * turningRadius);

        if ((startRightCircle - goalRightCircle).GetLengthSquared() < comparisonSqr)
        {
            var rlrPath = CalculateRLR(startPos, goalPos, startHeading, goalHeading,
                startRightCircle, goalRightCircle, turningRadius);
            if (rlrPath != null) paths.Add(rlrPath);
        }

        if ((startLeftCircle - goalLeftCircle).GetLengthSquared() < comparisonSqr)
        {
            var lrlPath = CalculateLRL(startPos, goalPos, startHeading, goalHeading,
                startLeftCircle, goalLeftCircle, turningRadius);
            if (lrlPath != null) paths.Add(lrlPath);
        }

        // Sort by total length (shortest first)
        paths.Sort((a, b) => a.TotalLength.CompareTo(b.TotalLength));

        // Generate waypoints for all paths
        foreach (var path in paths)
        {
            GenerateWaypoints(path, startPos, startHeading, goalPos, turningRadius, waypointSpacing);
        }

        return paths;
    }

    #region Circle Center Calculations

    private Position2D GetRightCircleCenter(Position2D pos, double heading, double radius)
    {
        // Circle is 90 degrees (pi/2 radians) to the right of heading
        double easting = pos.Easting + radius * Math.Sin(heading + HalfPi);
        double northing = pos.Northing + radius * Math.Cos(heading + HalfPi);
        return new Position2D(easting, northing);
    }

    private Position2D GetLeftCircleCenter(Position2D pos, double heading, double radius)
    {
        // Circle is 90 degrees (pi/2 radians) to the left of heading
        double easting = pos.Easting + radius * Math.Sin(heading - HalfPi);
        double northing = pos.Northing + radius * Math.Cos(heading - HalfPi);
        return new Position2D(easting, northing);
    }

    #endregion

    #region Path Type Calculations

    private DubinsPath? CalculateRSR(
        Position2D startPos, Position2D goalPos, double startHeading, double goalHeading,
        Position2D startCircle, Position2D goalCircle, double radius)
    {
        // Find tangent points
        CalculateOuterTangent(startCircle, goalCircle, false, radius, out var startTangent, out var goalTangent);

        // Calculate segment lengths
        double length1 = GetArcLength(startCircle, startPos, startTangent, false, radius);
        double length2 = (startTangent - goalTangent).GetLength();
        double length3 = GetArcLength(goalCircle, goalTangent, goalPos, false, radius);

        var path = new DubinsPath(DubinsPathType.RSR, length1, length2, length3, startTangent, goalTangent);
        path.SetTurningDirections(true, false, true);
        path.Segment2IsTurning = false;

        return path;
    }

    private DubinsPath? CalculateLSL(
        Position2D startPos, Position2D goalPos, double startHeading, double goalHeading,
        Position2D startCircle, Position2D goalCircle, double radius)
    {
        // Find tangent points
        CalculateOuterTangent(startCircle, goalCircle, true, radius, out var startTangent, out var goalTangent);

        // Calculate segment lengths
        double length1 = GetArcLength(startCircle, startPos, startTangent, true, radius);
        double length2 = (startTangent - goalTangent).GetLength();
        double length3 = GetArcLength(goalCircle, goalTangent, goalPos, true, radius);

        var path = new DubinsPath(DubinsPathType.LSL, length1, length2, length3, startTangent, goalTangent);
        path.SetTurningDirections(false, false, false);
        path.Segment2IsTurning = false;

        return path;
    }

    private DubinsPath? CalculateRSL(
        Position2D startPos, Position2D goalPos, double startHeading, double goalHeading,
        Position2D startCircle, Position2D goalCircle, double radius)
    {
        // Find tangent points
        CalculateInnerTangent(startCircle, goalCircle, false, radius, out var startTangent, out var goalTangent);

        // Calculate segment lengths
        double length1 = GetArcLength(startCircle, startPos, startTangent, false, radius);
        double length2 = (startTangent - goalTangent).GetLength();
        double length3 = GetArcLength(goalCircle, goalTangent, goalPos, true, radius);

        var path = new DubinsPath(DubinsPathType.RSL, length1, length2, length3, startTangent, goalTangent);
        path.SetTurningDirections(true, false, false);
        path.Segment2IsTurning = false;

        return path;
    }

    private DubinsPath? CalculateLSR(
        Position2D startPos, Position2D goalPos, double startHeading, double goalHeading,
        Position2D startCircle, Position2D goalCircle, double radius)
    {
        // Find tangent points
        CalculateInnerTangent(startCircle, goalCircle, true, radius, out var startTangent, out var goalTangent);

        // Calculate segment lengths
        double length1 = GetArcLength(startCircle, startPos, startTangent, true, radius);
        double length2 = (startTangent - goalTangent).GetLength();
        double length3 = GetArcLength(goalCircle, goalTangent, goalPos, false, radius);

        var path = new DubinsPath(DubinsPathType.LSR, length1, length2, length3, startTangent, goalTangent);
        path.SetTurningDirections(false, false, true);
        path.Segment2IsTurning = false;

        return path;
    }

    private DubinsPath? CalculateRLR(
        Position2D startPos, Position2D goalPos, double startHeading, double goalHeading,
        Position2D startCircle, Position2D goalCircle, double radius)
    {
        // Find tangent points and middle circle
        if (!CalculateCCCTangents(startCircle, goalCircle, false, radius,
            out var startTangent, out var goalTangent, out var middleCircle))
        {
            return null;
        }

        // Calculate segment lengths
        double length1 = GetArcLength(startCircle, startPos, startTangent, false, radius);
        double length2 = GetArcLength(middleCircle, startTangent, goalTangent, true, radius);
        double length3 = GetArcLength(goalCircle, goalTangent, goalPos, false, radius);

        var path = new DubinsPath(DubinsPathType.RLR, length1, length2, length3, startTangent, goalTangent);
        path.SetTurningDirections(true, false, true);
        path.Segment2IsTurning = true;

        return path;
    }

    private DubinsPath? CalculateLRL(
        Position2D startPos, Position2D goalPos, double startHeading, double goalHeading,
        Position2D startCircle, Position2D goalCircle, double radius)
    {
        // Find tangent points and middle circle
        if (!CalculateCCCTangents(startCircle, goalCircle, true, radius,
            out var startTangent, out var goalTangent, out var middleCircle))
        {
            return null;
        }

        // Calculate segment lengths
        double length1 = GetArcLength(startCircle, startPos, startTangent, true, radius);
        double length2 = GetArcLength(middleCircle, startTangent, goalTangent, false, radius);
        double length3 = GetArcLength(goalCircle, goalTangent, goalPos, true, radius);

        var path = new DubinsPath(DubinsPathType.LRL, length1, length2, length3, startTangent, goalTangent);
        path.SetTurningDirections(false, true, false);
        path.Segment2IsTurning = true;

        return path;
    }

    #endregion

    #region Tangent Calculations

    private void CalculateOuterTangent(
        Position2D startCircle,
        Position2D goalCircle,
        bool isBottom,
        double radius,
        out Position2D startTangent,
        out Position2D goalTangent)
    {
        // Angle to first tangent is always 90 degrees for circles with same radius
        double theta = HalfPi;

        // Adjust for circles not on same height
        theta += Math.Atan2(goalCircle.Northing - startCircle.Northing,
                           goalCircle.Easting - startCircle.Easting);

        // Add pi to get the "bottom" tangent (opposite side)
        if (isBottom)
        {
            theta += Math.PI;
        }

        // Calculate first tangent point
        double x1 = startCircle.Easting + radius * Math.Cos(theta);
        double y1 = startCircle.Northing + radius * Math.Sin(theta);

        // Second tangent point is offset by the direction between circles
        var dirVec = goalCircle - startCircle;
        double x2 = x1 + dirVec.Easting;
        double y2 = y1 + dirVec.Northing;

        startTangent = new Position2D(x1, y1);
        goalTangent = new Position2D(x2, y2);
    }

    private void CalculateInnerTangent(
        Position2D startCircle,
        Position2D goalCircle,
        bool isBottom,
        double radius,
        out Position2D startTangent,
        out Position2D goalTangent)
    {
        // Distance between circles
        double distance = (startCircle - goalCircle).GetLength();

        // Angle to first tangent using cosine (circles have same radius)
        double theta = Math.Acos((2.0 * radius) / distance);

        // Flip angle for LSR
        if (isBottom)
        {
            theta *= -1.0;
        }

        // Adjust for circles not on same height
        theta += Math.Atan2(goalCircle.Northing - startCircle.Northing,
                           goalCircle.Easting - startCircle.Easting);

        // Calculate first tangent point
        double x1 = startCircle.Easting + radius * Math.Cos(theta);
        double y1 = startCircle.Northing + radius * Math.Sin(theta);

        // Calculate intermediate point (2 radii away)
        double xTmp = startCircle.Easting + 2.0 * radius * Math.Cos(theta);
        double yTmp = startCircle.Northing + 2.0 * radius * Math.Sin(theta);

        // Direction from intermediate point to goal circle
        var dirVec = goalCircle - new Position2D(xTmp, yTmp);

        // Second tangent point
        double x2 = x1 + dirVec.Easting;
        double y2 = y1 + dirVec.Northing;

        startTangent = new Position2D(x1, y1);
        goalTangent = new Position2D(x2, y2);
    }

    private bool CalculateCCCTangents(
        Position2D startCircle,
        Position2D goalCircle,
        bool isLRL,
        double radius,
        out Position2D startTangent,
        out Position2D goalTangent,
        out Position2D middleCircle)
    {
        startTangent = new Position2D(0, 0);
        goalTangent = new Position2D(0, 0);
        middleCircle = new Position2D(0, 0);

        // Distance between circles
        double distance = (startCircle - goalCircle).GetLength();

        // Check if valid (distance must be less than 4 * radius)
        if (distance >= 4.0 * radius)
        {
            return false;
        }

        // Angle between goal and the 3rd circle using law of cosines
        double theta = Math.Acos(distance / (4.0 * radius));

        // Adjust angle based on circle positions
        var v1 = goalCircle - startCircle;

        if (isLRL)
        {
            theta = Math.Atan2(v1.Northing, v1.Easting) + theta;
        }
        else
        {
            theta = Math.Atan2(v1.Northing, v1.Easting) - theta;
        }

        // Calculate middle circle position
        double x = startCircle.Easting + 2.0 * radius * Math.Cos(theta);
        double y = startCircle.Northing + 2.0 * radius * Math.Sin(theta);
        middleCircle = new Position2D(x, y);

        // Calculate tangent points
        var v2 = (startCircle - middleCircle).Normalize();
        var v3 = (goalCircle - middleCircle).Normalize();

        startTangent = middleCircle + v2 * radius;
        goalTangent = middleCircle + v3 * radius;

        return true;
    }

    #endregion

    #region Arc Length and Waypoint Generation

    private double GetArcLength(
        Position2D circleCenter,
        Position2D startPos,
        Position2D goalPos,
        bool isLeftCircle,
        double radius)
    {
        var v1 = startPos - circleCenter;
        var v2 = goalPos - circleCenter;

        double theta = Math.Atan2(v2.Northing, v2.Easting) - Math.Atan2(v1.Northing, v1.Easting);

        if (theta < 0.0 && isLeftCircle)
        {
            theta += 2.0 * Math.PI;
        }
        else if (theta > 0.0 && !isLeftCircle)
        {
            theta -= 2.0 * Math.PI;
        }

        return Math.Abs(theta * radius);
    }

    private void GenerateWaypoints(
        DubinsPath path,
        Position2D startPos,
        double startHeading,
        Position2D goalPos,
        double radius,
        double spacing)
    {
        path.Waypoints.Clear();

        var currentPos = startPos;
        double currentHeading = startHeading;

        // Add start position
        path.Waypoints.Add(new DubinsPathWaypoint(currentPos.Easting, currentPos.Northing, currentHeading));

        // Segment 1
        int segments = (int)Math.Floor(path.Segment1Length / spacing);
        AddWaypointsToPath(ref currentPos, ref currentHeading, path.Waypoints, segments,
            true, path.Segment1TurningRight, radius, spacing);

        // Segment 2
        segments = (int)Math.Floor(path.Segment2Length / spacing);
        AddWaypointsToPath(ref currentPos, ref currentHeading, path.Waypoints, segments,
            path.Segment2IsTurning, path.Segment2TurningRight, radius, spacing);

        // Segment 3
        segments = (int)Math.Floor(path.Segment3Length / spacing);
        AddWaypointsToPath(ref currentPos, ref currentHeading, path.Waypoints, segments,
            true, path.Segment3TurningRight, radius, spacing);

        // Add final goal position
        path.Waypoints.Add(new DubinsPathWaypoint(goalPos.Easting, goalPos.Northing, currentHeading));
    }

    private void AddWaypointsToPath(
        ref Position2D currentPos,
        ref double heading,
        List<DubinsPathWaypoint> waypoints,
        int segments,
        bool isTurning,
        bool isTurningRight,
        double radius,
        double spacing)
    {
        for (int i = 0; i < segments; i++)
        {
            // Update position
            currentPos = new Position2D(
                currentPos.Easting + spacing * Math.Sin(heading),
                currentPos.Northing + spacing * Math.Cos(heading));

            // Update heading if turning
            if (isTurning)
            {
                double turnParameter = isTurningRight ? 1.0 : -1.0;
                heading += (spacing / radius) * turnParameter;
            }

            // Add waypoint
            waypoints.Add(new DubinsPathWaypoint(currentPos.Easting, currentPos.Northing, heading));
        }
    }

    #endregion

    #region Helper Methods

    private bool PositionsEqual(Position2D a, Position2D b)
    {
        const double epsilon = 1e-10;
        return Math.Abs(a.Easting - b.Easting) < epsilon &&
               Math.Abs(a.Northing - b.Northing) < epsilon;
    }

    #endregion
}
