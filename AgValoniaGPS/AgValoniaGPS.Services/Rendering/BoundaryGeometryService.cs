using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Implements boundary geometry generation for OpenGL rendering.
/// Converts boundary polygons into GPU-ready vertex arrays.
/// </summary>
public class BoundaryGeometryService : IBoundaryGeometryService
{
    /// <summary>
    /// Generates a line strip from boundary points.
    /// </summary>
    public float[] GenerateBoundaryLines(List<Position> boundaryPoints)
    {
        if (boundaryPoints == null)
            throw new ArgumentNullException(nameof(boundaryPoints));

        if (boundaryPoints.Count < 2)
            throw new ArgumentException("Boundary must have at least 2 points", nameof(boundaryPoints));

        // Check if boundary is closed (last point ~= first point)
        bool isClosed = IsClosedBoundary(boundaryPoints);
        int pointCount = isClosed ? boundaryPoints.Count : boundaryPoints.Count + 1;

        var vertices = new List<float>(pointCount * 2);

        // Add all points
        foreach (var point in boundaryPoints)
        {
            vertices.Add((float)point.Easting);
            vertices.Add((float)point.Northing);
        }

        // Close the boundary if not already closed
        if (!isClosed)
        {
            vertices.Add((float)boundaryPoints[0].Easting);
            vertices.Add((float)boundaryPoints[0].Northing);
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Generates a triangulated filled mesh from boundary points.
    /// Uses simple ear clipping algorithm for convex/simple polygons.
    /// </summary>
    public float[] GenerateBoundaryFill(List<Position> boundaryPoints)
    {
        if (boundaryPoints == null)
            throw new ArgumentNullException(nameof(boundaryPoints));

        if (boundaryPoints.Count < 3)
            throw new ArgumentException("Boundary fill requires at least 3 points", nameof(boundaryPoints));

        // Simple triangulation: fan from first vertex
        // This works for convex polygons and is sufficient for basic visualization
        var vertices = new List<float>((boundaryPoints.Count - 2) * 6);

        var origin = boundaryPoints[0];

        for (int i = 1; i < boundaryPoints.Count - 1; i++)
        {
            // Triangle: origin, point[i], point[i+1]
            vertices.Add((float)origin.Easting);
            vertices.Add((float)origin.Northing);

            vertices.Add((float)boundaryPoints[i].Easting);
            vertices.Add((float)boundaryPoints[i].Northing);

            vertices.Add((float)boundaryPoints[i + 1].Easting);
            vertices.Add((float)boundaryPoints[i + 1].Northing);
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Generates headland line strip from headland points.
    /// </summary>
    public float[] GenerateHeadlandLines(List<Position> headlandPoints)
    {
        // Headlands use same logic as boundary lines
        return GenerateBoundaryLines(headlandPoints);
    }

    /// <summary>
    /// Checks if a boundary is closed (first and last points are very close).
    /// </summary>
    private bool IsClosedBoundary(List<Position> points)
    {
        if (points.Count < 2)
            return false;

        var first = points[0];
        var last = points[points.Count - 1];

        double dx = last.Easting - first.Easting;
        double dy = last.Northing - first.Northing;
        double distSq = dx * dx + dy * dy;

        // Consider closed if within 0.1m
        return distSq < 0.01; // 0.1 * 0.1
    }
}
