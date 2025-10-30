using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Implements coverage triangle mesh generation for OpenGL rendering.
/// Converts coverage triangles into GPU-ready interleaved vertex arrays.
/// </summary>
public class CoverageGeometryService : ICoverageGeometryService
{
    /// <summary>
    /// Generates a coverage mesh from coverage triangles.
    /// </summary>
    public CoverageMesh GenerateCoverageMesh(IEnumerable<CoverageTriangle> triangles)
    {
        // Call overload with default zoom level (no LOD)
        return GenerateCoverageMesh(triangles, zoomLevel: 1.0f);
    }

    /// <summary>
    /// Generates a coverage mesh from coverage triangles with LOD support.
    /// </summary>
    /// <param name="triangles">Coverage triangles to render</param>
    /// <param name="zoomLevel">Current camera zoom level in meters per pixel</param>
    /// <returns>Coverage mesh with LOD applied</returns>
    public CoverageMesh GenerateCoverageMesh(IEnumerable<CoverageTriangle> triangles, float zoomLevel)
    {
        if (triangles == null)
            throw new ArgumentNullException(nameof(triangles));

        var triangleList = triangles.ToList();

        if (triangleList.Count == 0)
        {
            return new CoverageMesh
            {
                Vertices = Array.Empty<float>(),
                Indices = Array.Empty<uint>(),
                TriangleCount = 0
            };
        }

        // Determine LOD skip factor based on zoom level
        int skipFactor = CalculateLODSkipFactor(zoomLevel);

        // Apply LOD: render every Nth triangle
        var filteredTriangles = skipFactor > 1
            ? triangleList.Where((t, i) => i % skipFactor == 0).ToList()
            : triangleList;

        int triangleCount = filteredTriangles.Count;
        int vertexCount = triangleCount * 3; // 3 vertices per triangle

        // 5 floats per vertex (X, Y, R, G, B)
        var vertices = new float[vertexCount * 5];
        var indices = new uint[vertexCount]; // 3 indices per triangle

        int vertexIndex = 0;
        uint indexValue = 0;

        foreach (var triangle in filteredTriangles)
        {
            if (triangle.Vertices == null || triangle.Vertices.Length != 3)
                continue;

            // Get color based on overlap count
            var (r, g, b) = GetColorForOverlapCount(triangle.OverlapCount);

            // Add 3 vertices for this triangle
            for (int i = 0; i < 3; i++)
            {
                var vertex = triangle.Vertices[i];

                // Position
                vertices[vertexIndex++] = (float)vertex.Easting;
                vertices[vertexIndex++] = (float)vertex.Northing;

                // Color
                vertices[vertexIndex++] = r;
                vertices[vertexIndex++] = g;
                vertices[vertexIndex++] = b;

                // Index (sequential: 0, 1, 2, 3, 4, 5, ...)
                indices[indexValue] = indexValue;
                indexValue++;
            }
        }

        return new CoverageMesh
        {
            Vertices = vertices,
            Indices = indices,
            TriangleCount = triangleCount
        };
    }

    /// <summary>
    /// Calculates the LOD skip factor based on zoom level.
    /// </summary>
    /// <param name="zoomLevel">Camera zoom level in meters per pixel</param>
    /// <returns>Skip factor (1 = render all, 2 = render every 2nd, 4 = render every 4th, etc.)</returns>
    private int CalculateLODSkipFactor(float zoomLevel)
    {
        // LOD thresholds based on zoom level (meters per pixel)
        // Close zoom (< 0.5 m/px): Render all triangles
        // Medium zoom (0.5-2.0 m/px): Render every 2nd triangle
        // Far zoom (2.0-10.0 m/px): Render every 4th triangle
        // Very far zoom (> 10.0 m/px): Render every 8th triangle

        if (zoomLevel < 0.5f)
            return 1;   // Render all triangles
        else if (zoomLevel < 2.0f)
            return 2;   // Render every 2nd triangle
        else if (zoomLevel < 10.0f)
            return 4;   // Render every 4th triangle
        else
            return 8;   // Render every 8th triangle
    }

    /// <summary>
    /// Gets RGB color based on overlap count (pass number).
    /// </summary>
    private (float r, float g, float b) GetColorForOverlapCount(int overlapCount)
    {
        return overlapCount switch
        {
            1 => (0.0f, 1.0f, 0.0f),      // Green - first pass
            2 => (1.0f, 1.0f, 0.0f),      // Yellow - second pass
            _ => (1.0f, 0.5f, 0.0f)       // Orange - third+ pass
        };
    }
}
