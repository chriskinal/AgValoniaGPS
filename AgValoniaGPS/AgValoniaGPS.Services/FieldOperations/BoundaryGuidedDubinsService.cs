using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Enhanced Dubins path service with boundary-guided sampling.
/// Handles complex field geometries and internal obstacles.
/// Designed for 20Hz steering loops with <10ms budget.
/// </summary>
public class BoundaryGuidedDubinsService : IBoundaryGuidedDubinsService
{
    private readonly IDubinsPathService _dubinsPathService;
    private const double DefaultInfluenceRadius = 3.0; // meters

    public BoundaryGuidedDubinsService(IDubinsPathService dubinsPathService)
    {
        _dubinsPathService = dubinsPathService ?? throw new ArgumentNullException(nameof(dubinsPathService));
    }

    public BoundaryGuidedDubinsResult GenerateBoundaryAwarePath(
        double startEasting,
        double startNorthing,
        double startHeading,
        double goalEasting,
        double goalNorthing,
        double goalHeading,
        double turningRadius,
        List<List<Position2D>> boundaries,
        double minBoundaryDistance,
        double waypointSpacing = 0.1,
        int maxIterations = 8)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BoundaryGuidedDubinsResult(false);

        // Step 1: Try standard Dubins first (fast path, handles most cases)
        var standardPath = TryStandardDubins(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            turningRadius, boundaries, minBoundaryDistance, waypointSpacing);

        if (standardPath != null)
        {
            result.Succeeded = true;
            result.ResultPath = standardPath;
            result.Strategy = PathGenerationStrategy.StandardDubins;
            result.IterationCount = 0;
            result.MinBoundaryDistance = CalculateMinBoundaryDistance(standardPath, boundaries);
            result.ComputationTime = stopwatch.Elapsed;
            return result;
        }

        // Step 2: Try boundary-guided sampling (handles complex cases)
        var guidedPath = TryGuidedSampling(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            turningRadius, boundaries, minBoundaryDistance, waypointSpacing, maxIterations, stopwatch);

        if (guidedPath != null)
        {
            result.Succeeded = true;
            result.ResultPath = guidedPath.Item1;
            result.IntermediateWaypoints = guidedPath.Item2;
            result.DubinsSegments = guidedPath.Item3;
            result.Strategy = PathGenerationStrategy.GuidedSampling;
            result.IterationCount = guidedPath.Item4;
            result.MinBoundaryDistance = CalculateMinBoundaryDistance(guidedPath.Item1, boundaries);
            result.ComputationTime = stopwatch.Elapsed;
            return result;
        }

        // Step 3: Return failure (UTurnService will use fallback K-turn)
        result.Succeeded = false;
        result.Strategy = PathGenerationStrategy.FallbackKTurn;
        result.ComputationTime = stopwatch.Elapsed;
        return result;
    }

    private DubinsPath? TryStandardDubins(
        double startEasting, double startNorthing, double startHeading,
        double goalEasting, double goalNorthing, double goalHeading,
        double turningRadius,
        List<List<Position2D>> boundaries,
        double minBoundaryDistance,
        double waypointSpacing)
    {
        // Try all 6 Dubins path types
        var allPaths = _dubinsPathService.GenerateAllPaths(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            turningRadius, waypointSpacing);

        if (allPaths == null || allPaths.Count == 0)
            return null;

        // Return first path that doesn't violate boundaries
        foreach (var path in allPaths.OrderBy(p => p.TotalLength))
        {
            if (IsPathValid(path, boundaries, minBoundaryDistance))
                return path;
        }

        return null;
    }

    private Tuple<DubinsPath, List<Position2D>, List<DubinsPath>, int>? TryGuidedSampling(
        double startEasting, double startNorthing, double startHeading,
        double goalEasting, double goalNorthing, double goalHeading,
        double turningRadius,
        List<List<Position2D>> boundaries,
        double minBoundaryDistance,
        double waypointSpacing,
        int maxIterations,
        Stopwatch stopwatch)
    {
        var startPos = new Position2D(startEasting, startNorthing);
        var goalPos = new Position2D(goalEasting, goalNorthing);

        // Sample initial waypoints along straight line
        var waypoints = SampleInitialWaypoints(startPos, goalPos, 3); // Start with 3 intermediate points

        for (int iteration = 1; iteration <= maxIterations; iteration++)
        {
            // Performance budget check: bail if approaching 9ms
            if (stopwatch.ElapsedMilliseconds > 9)
                break;

            // Apply boundary repulsion to waypoints
            var adjustedWaypoints = new List<Position2D>();
            foreach (var wp in waypoints)
            {
                var repulsion = CalculateBoundaryRepulsion(wp, boundaries, DefaultInfluenceRadius);
                var adjusted = new Position2D(wp.Easting + repulsion.Easting, wp.Northing + repulsion.Northing);

                // Ensure adjusted point is valid
                if (IsPointValid(adjusted, boundaries, minBoundaryDistance))
                    adjustedWaypoints.Add(adjusted);
                else
                    adjustedWaypoints.Add(wp); // Keep original if adjustment makes it worse
            }

            // Try to connect waypoints with Dubins segments
            var segments = new List<DubinsPath>();
            bool allSegmentsValid = true;

            // Connect start → waypoint1
            var prevPos = startPos;
            var prevHeading = startHeading;

            foreach (var wp in adjustedWaypoints)
            {
                // Estimate heading toward next waypoint
                double dx = wp.Easting - prevPos.Easting;
                double dy = wp.Northing - prevPos.Northing;
                double targetHeading = Math.Atan2(dx, dy);

                var segment = _dubinsPathService.GeneratePath(
                    prevPos.Easting, prevPos.Northing, prevHeading,
                    wp.Easting, wp.Northing, targetHeading,
                    turningRadius, waypointSpacing);

                if (segment == null || !IsPathValid(segment, boundaries, minBoundaryDistance))
                {
                    allSegmentsValid = false;
                    break;
                }

                segments.Add(segment);
                prevPos = wp;
                prevHeading = targetHeading;
            }

            if (!allSegmentsValid)
            {
                // Add more waypoints and retry
                waypoints = RefineWaypoints(waypoints, startPos, goalPos);
                continue;
            }

            // Connect last waypoint → goal
            var finalSegment = _dubinsPathService.GeneratePath(
                prevPos.Easting, prevPos.Northing, prevHeading,
                goalEasting, goalNorthing, goalHeading,
                turningRadius, waypointSpacing);

            if (finalSegment == null || !IsPathValid(finalSegment, boundaries, minBoundaryDistance))
            {
                waypoints = RefineWaypoints(waypoints, startPos, goalPos);
                continue;
            }

            segments.Add(finalSegment);

            // Success! Combine segments into single path
            var combinedPath = CombineSegments(segments, waypointSpacing);
            return Tuple.Create(combinedPath, adjustedWaypoints, segments, iteration);
        }

        return null;
    }

    public Position2D CalculateBoundaryRepulsion(
        Position2D point,
        List<List<Position2D>> boundaries,
        double influenceRadius)
    {
        double repulsionX = 0;
        double repulsionY = 0;

        foreach (var boundary in boundaries)
        {
            for (int i = 0; i < boundary.Count; i++)
            {
                var p1 = boundary[i];
                var p2 = boundary[(i + 1) % boundary.Count];

                // Calculate closest point on edge
                var closestPoint = ClosestPointOnSegment(point, p1, p2);
                double dx = point.Easting - closestPoint.Easting;
                double dy = point.Northing - closestPoint.Northing;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < influenceRadius && distance > 0.01)
                {
                    // Repulsion strength inversely proportional to distance
                    double strength = (influenceRadius - distance) / influenceRadius;
                    double nx = dx / distance; // Normalize
                    double ny = dy / distance;

                    repulsionX += nx * strength * 0.5; // 0.5m push per unit strength
                    repulsionY += ny * strength * 0.5;
                }
            }
        }

        return new Position2D(repulsionX, repulsionY);
    }

    public bool IsPointValid(
        Position2D point,
        List<List<Position2D>> boundaries,
        double minDistance)
    {
        if (boundaries == null || boundaries.Count == 0)
            return true;

        // Check distance to all boundaries
        foreach (var boundary in boundaries)
        {
            double dist = DistanceToPolygon(point, boundary);
            if (dist < minDistance)
                return false;
        }

        return true;
    }

    private bool IsPathValid(DubinsPath path, List<List<Position2D>> boundaries, double minDistance)
    {
        if (path == null || path.Waypoints.Count == 0)
            return false;

        // Check every waypoint
        foreach (var wp in path.Waypoints)
        {
            var point = new Position2D(wp.Easting, wp.Northing);
            if (!IsPointValid(point, boundaries, minDistance))
                return false;
        }

        return true;
    }

    private List<Position2D> SampleInitialWaypoints(Position2D start, Position2D goal, int count)
    {
        var waypoints = new List<Position2D>();

        for (int i = 1; i <= count; i++)
        {
            double t = (double)i / (count + 1);
            double e = start.Easting + t * (goal.Easting - start.Easting);
            double n = start.Northing + t * (goal.Northing - start.Northing);
            waypoints.Add(new Position2D(e, n));
        }

        return waypoints;
    }

    private List<Position2D> RefineWaypoints(List<Position2D> currentWaypoints, Position2D start, Position2D goal)
    {
        // Add midpoint between each pair of waypoints
        var refined = new List<Position2D>();

        var allPoints = new List<Position2D> { start };
        allPoints.AddRange(currentWaypoints);
        allPoints.Add(goal);

        for (int i = 0; i < allPoints.Count - 1; i++)
        {
            if (i > 0) // Don't add start
                refined.Add(allPoints[i]);

            // Add midpoint
            double midE = (allPoints[i].Easting + allPoints[i + 1].Easting) / 2.0;
            double midN = (allPoints[i].Northing + allPoints[i + 1].Northing) / 2.0;
            refined.Add(new Position2D(midE, midN));
        }

        return refined;
    }

    private DubinsPath CombineSegments(List<DubinsPath> segments, double waypointSpacing)
    {
        if (segments.Count == 0)
            throw new InvalidOperationException("No segments to combine");

        if (segments.Count == 1)
            return segments[0];

        // Combine all waypoints
        var allWaypoints = new List<DubinsPathWaypoint>();
        double totalLength = 0;

        foreach (var segment in segments)
        {
            allWaypoints.AddRange(segment.Waypoints);
            totalLength += segment.TotalLength;
        }

        // Create combined path with distributed segment lengths
        // Use equal distribution for simplicity (actual segments are multiple paths connected)
        var firstSegment = segments[0];
        double segmentLength = totalLength / 3.0;

        var combined = new DubinsPath(
            firstSegment.PathType,
            segmentLength,
            segmentLength,
            segmentLength,
            firstSegment.Segment1Tangent,
            firstSegment.Segment2Tangent)
        {
            Waypoints = allWaypoints
        };

        return combined;
    }

    private Position2D ClosestPointOnSegment(Position2D point, Position2D lineStart, Position2D lineEnd)
    {
        double dx = lineEnd.Easting - lineStart.Easting;
        double dy = lineEnd.Northing - lineStart.Northing;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared < 1e-10)
            return lineStart;

        double t = ((point.Easting - lineStart.Easting) * dx + (point.Northing - lineStart.Northing) * dy) / lengthSquared;
        t = Math.Clamp(t, 0.0, 1.0);

        return new Position2D(
            lineStart.Easting + t * dx,
            lineStart.Northing + t * dy);
    }

    private double DistanceToPolygon(Position2D point, List<Position2D> polygon)
    {
        double minDistance = double.MaxValue;

        for (int i = 0; i < polygon.Count; i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[(i + 1) % polygon.Count];
            var closest = ClosestPointOnSegment(point, p1, p2);

            double dx = point.Easting - closest.Easting;
            double dy = point.Northing - closest.Northing;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < minDistance)
                minDistance = distance;
        }

        return minDistance;
    }

    private double CalculateMinBoundaryDistance(DubinsPath path, List<List<Position2D>> boundaries)
    {
        double minDist = double.MaxValue;

        foreach (var wp in path.Waypoints)
        {
            var point = new Position2D(wp.Easting, wp.Northing);
            foreach (var boundary in boundaries)
            {
                double dist = DistanceToPolygon(point, boundary);
                if (dist < minDist)
                    minDist = dist;
            }
        }

        return minDist;
    }
}
