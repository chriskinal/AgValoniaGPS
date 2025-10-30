using AgValoniaGPS.Models.FieldOperations;
using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Utility class for smoothing recorded paths using the Ramer-Douglas-Peucker algorithm.
/// Reduces the number of points while preserving the overall path shape.
/// </summary>
public static class PathSmoothingUtility
{
    /// <summary>
    /// Smooths a recorded path using the Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="points">Original path points</param>
    /// <param name="epsilon">Maximum allowed distance deviation in meters (smaller = more points retained)</param>
    /// <returns>Smoothed list of path points</returns>
    public static List<RecordedPathPoint> DouglasPeucker(List<RecordedPathPoint> points, double epsilon)
    {
        if (points == null || points.Count < 3)
        {
            return new List<RecordedPathPoint>(points ?? new List<RecordedPathPoint>());
        }

        var result = new List<RecordedPathPoint>();
        DouglasPeuckerRecursive(points, 0, points.Count - 1, epsilon, result);

        // Sort result by original index to maintain order
        // (result is built recursively and may not be in order)
        result.Sort((a, b) => points.IndexOf(a).CompareTo(points.IndexOf(b)));

        return result;
    }

    private static void DouglasPeuckerRecursive(
        List<RecordedPathPoint> points,
        int startIndex,
        int endIndex,
        double epsilon,
        List<RecordedPathPoint> result)
    {
        // Always include the first and last points
        if (!result.Contains(points[startIndex]))
        {
            result.Add(points[startIndex]);
        }

        if (startIndex >= endIndex - 1)
        {
            if (!result.Contains(points[endIndex]))
            {
                result.Add(points[endIndex]);
            }
            return;
        }

        // Find the point with maximum distance from the line segment
        double maxDistance = 0;
        int maxIndex = startIndex;

        var start = points[startIndex];
        var end = points[endIndex];

        for (int i = startIndex + 1; i < endIndex; i++)
        {
            double distance = PerpendicularDistance(points[i], start, end);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        // If max distance is greater than epsilon, recursively simplify
        if (maxDistance > epsilon)
        {
            // Recursively simplify the two sub-paths
            DouglasPeuckerRecursive(points, startIndex, maxIndex, epsilon, result);
            DouglasPeuckerRecursive(points, maxIndex, endIndex, epsilon, result);
        }
        else
        {
            // All points between start and end are within epsilon, so just keep the endpoints
            if (!result.Contains(points[endIndex]))
            {
                result.Add(points[endIndex]);
            }
        }
    }

    /// <summary>
    /// Calculates the perpendicular distance from a point to a line segment.
    /// </summary>
    private static double PerpendicularDistance(RecordedPathPoint point, RecordedPathPoint lineStart, RecordedPathPoint lineEnd)
    {
        double dx = lineEnd.Easting - lineStart.Easting;
        double dy = lineEnd.Northing - lineStart.Northing;

        // If the line segment is actually a point, return distance to that point
        double lengthSquared = dx * dx + dy * dy;
        if (lengthSquared < 1e-10)
        {
            double de = point.Easting - lineStart.Easting;
            double dn = point.Northing - lineStart.Northing;
            return Math.Sqrt(de * de + dn * dn);
        }

        // Calculate perpendicular distance using the cross product formula
        // distance = |(x2-x1)(y1-y0) - (x1-x0)(y2-y1)| / sqrt((x2-x1)^2 + (y2-y1)^2)
        double numerator = Math.Abs(
            dx * (lineStart.Northing - point.Northing) -
            (lineStart.Easting - point.Easting) * dy);

        return numerator / Math.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Simplifies a path by removing points that are too close together.
    /// </summary>
    /// <param name="points">Original path points</param>
    /// <param name="minDistance">Minimum distance between points in meters</param>
    /// <returns>Filtered list of path points</returns>
    public static List<RecordedPathPoint> FilterByDistance(List<RecordedPathPoint> points, double minDistance)
    {
        if (points == null || points.Count < 2)
        {
            return new List<RecordedPathPoint>(points ?? new List<RecordedPathPoint>());
        }

        var result = new List<RecordedPathPoint> { points[0] };
        var lastPoint = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            double de = points[i].Easting - lastPoint.Easting;
            double dn = points[i].Northing - lastPoint.Northing;
            double distance = Math.Sqrt(de * de + dn * dn);

            if (distance >= minDistance)
            {
                result.Add(points[i]);
                lastPoint = points[i];
            }
        }

        // Always include the last point if it's not already included
        if (result[result.Count - 1] != points[points.Count - 1])
        {
            result.Add(points[points.Count - 1]);
        }

        return result;
    }
}
