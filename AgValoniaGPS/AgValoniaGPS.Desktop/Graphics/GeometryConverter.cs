using System;
using System.Collections.Generic;
using System.Numerics;

namespace AgValoniaGPS.Desktop.Graphics;

/// <summary>
/// Converts geometry data from services to GPU-ready mesh format.
/// Handles conversion from 2D to 3D coordinates and adds normals/colors.
/// </summary>
public static class GeometryConverter
{
    /// <summary>
    /// Converts 2D vertex data (x, y, r, g, b, a) to 3D mesh format.
    /// Format: position (x, y, z=0), normal (0, 0, 1), color (r, g, b, a), texCoord (0, 0)
    /// </summary>
    public static float[] Convert2DTo3D(float[] vertices2D)
    {
        if (vertices2D == null || vertices2D.Length == 0)
            return Array.Empty<float>();

        int vertexCount = vertices2D.Length / 6; // 6 floats per 2D vertex (x, y, r, g, b, a)
        float[] vertices3D = new float[vertexCount * 12]; // 12 floats per 3D vertex

        for (int i = 0; i < vertexCount; i++)
        {
            int srcIdx = i * 6;
            int dstIdx = i * 12;

            // Position (x, y, z=0)
            vertices3D[dstIdx + 0] = vertices2D[srcIdx + 0]; // x
            vertices3D[dstIdx + 1] = vertices2D[srcIdx + 1]; // y
            vertices3D[dstIdx + 2] = 0.0f;                    // z

            // Normal (pointing up for 2D geometry)
            vertices3D[dstIdx + 3] = 0.0f;  // nx
            vertices3D[dstIdx + 4] = 0.0f;  // ny
            vertices3D[dstIdx + 5] = 1.0f;  // nz

            // Color
            vertices3D[dstIdx + 6] = vertices2D[srcIdx + 2];  // r
            vertices3D[dstIdx + 7] = vertices2D[srcIdx + 3];  // g
            vertices3D[dstIdx + 8] = vertices2D[srcIdx + 4];  // b
            vertices3D[dstIdx + 9] = vertices2D[srcIdx + 5];  // a

            // TexCoord (unused for most geometry)
            vertices3D[dstIdx + 10] = 0.0f; // u
            vertices3D[dstIdx + 11] = 0.0f; // v
        }

        return vertices3D;
    }

    /// <summary>
    /// Creates a grid mesh for the ground plane.
    /// </summary>
    public static float[] CreateGridMesh(float size, float spacing, out int lineCount)
    {
        List<float> vertices = new List<float>();
        Vector4 color = new Vector4(0.35f, 0.35f, 0.35f, 0.8f); // Light gray to contrast with white UI buttons
        Vector3 normal = new Vector3(0, 0, 1); // Up

        // Vertical lines
        for (float x = -size; x <= size; x += spacing)
        {
            // Brighter every 50m
            float alpha = (Math.Abs(x % 50.0f) < 0.1f) ? 1.0f : 0.8f;
            Vector4 lineColor = new Vector4(color.X, color.Y, color.Z, alpha);

            AddVertex(vertices, new Vector3(x, -size, 0), normal, lineColor, Vector2.Zero);
            AddVertex(vertices, new Vector3(x, size, 0), normal, lineColor, Vector2.Zero);
        }

        // Horizontal lines
        for (float y = -size; y <= size; y += spacing)
        {
            float alpha = (Math.Abs(y % 50.0f) < 0.1f) ? 1.0f : 0.8f;
            Vector4 lineColor = new Vector4(color.X, color.Y, color.Z, alpha);

            AddVertex(vertices, new Vector3(-size, y, 0), normal, lineColor, Vector2.Zero);
            AddVertex(vertices, new Vector3(size, y, 0), normal, lineColor, Vector2.Zero);
        }

        // Axis lines (X = red, Y = green) - brighter and fully opaque
        // X-axis (East-West) - Bright red
        Vector4 xAxisColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        AddVertex(vertices, new Vector3(-size, 0, 0.01f), normal, xAxisColor, Vector2.Zero);
        AddVertex(vertices, new Vector3(size, 0, 0.01f), normal, xAxisColor, Vector2.Zero);

        // Y-axis (North-South) - Bright green
        Vector4 yAxisColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        AddVertex(vertices, new Vector3(0, -size, 0.01f), normal, yAxisColor, Vector2.Zero);
        AddVertex(vertices, new Vector3(0, size, 0.01f), normal, yAxisColor, Vector2.Zero);

        lineCount = vertices.Count / 12; // 12 floats per vertex

        // DEBUG: Print first few vertices to verify data
        if (vertices.Count >= 24)
        {
            Console.WriteLine($"[GridMesh] Sample vertices:");
            Console.WriteLine($"  Vertex 0: pos=({vertices[0]}, {vertices[1]}, {vertices[2]}), color=({vertices[6]}, {vertices[7]}, {vertices[8]}, {vertices[9]})");
            Console.WriteLine($"  Vertex 1: pos=({vertices[12]}, {vertices[13]}, {vertices[14]}), color=({vertices[18]}, {vertices[19]}, {vertices[20]}, {vertices[21]})");
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Creates a simple vehicle mesh (rectangle with direction indicator).
    /// Width = side-to-side (X), Length = front-to-back (Y), arrow points forward (+Y)
    /// </summary>
    public static float[] CreateVehicleMesh(float width, float length, Vector4 color)
    {
        List<float> vertices = new List<float>();
        Vector3 normal = new Vector3(0, 0, 1);

        float halfWidth = width / 2;
        float halfLength = length / 2;

        Console.WriteLine($"[CreateVehicleMesh] Creating vehicle: width={width}m, length={length}m, color=({color.X}, {color.Y}, {color.Z}, {color.W})");
        Console.WriteLine($"[CreateVehicleMesh] Body: X={-halfWidth} to {halfWidth}, Y={-halfLength} to {halfLength}");
        Console.WriteLine($"[CreateVehicleMesh] BODY COLOR: R={color.X}, G={color.Y}, B={color.Z}, A={color.W}");

        // Vehicle body (rectangle) - raised to 2.0f above grid to avoid Z-fighting
        // Triangle 1
        AddVertex(vertices, new Vector3(-halfWidth, -halfLength, 2.0f), normal, color, new Vector2(0, 0));
        AddVertex(vertices, new Vector3(halfWidth, -halfLength, 2.0f), normal, color, new Vector2(1, 0));
        AddVertex(vertices, new Vector3(halfWidth, halfLength, 2.0f), normal, color, new Vector2(1, 1));

        // Triangle 2
        AddVertex(vertices, new Vector3(-halfWidth, -halfLength, 2.0f), normal, color, new Vector2(0, 0));
        AddVertex(vertices, new Vector3(halfWidth, halfLength, 2.0f), normal, color, new Vector2(1, 1));
        AddVertex(vertices, new Vector3(-halfWidth, halfLength, 2.0f), normal, color, new Vector2(0, 1));

        // Direction indicator (arrow at front pointing in +Y direction)
        Vector4 arrowColor = new Vector4(1.0f, 1.0f, 0.0f, 1.0f); // YELLOW arrow
        float arrowSize = width * 0.3f;
        float arrowY = halfLength + arrowSize;

        Console.WriteLine($"[CreateVehicleMesh] Arrow: tip at Y={arrowY}, base at Y={halfLength}, width={arrowSize * 2}m");
        Console.WriteLine($"[CreateVehicleMesh] ARROW COLOR: R={arrowColor.X}, G={arrowColor.Y}, B={arrowColor.Z}, A={arrowColor.W}");

        AddVertex(vertices, new Vector3(0, arrowY, 2.0f), normal, arrowColor, Vector2.Zero);
        AddVertex(vertices, new Vector3(-arrowSize, halfLength, 2.0f), normal, arrowColor, Vector2.Zero);
        AddVertex(vertices, new Vector3(arrowSize, halfLength, 2.0f), normal, arrowColor, Vector2.Zero);

        var result = vertices.ToArray();

        // Print first vertex data to verify colors
        Console.WriteLine($"[CreateVehicleMesh] First vertex (should be RED body):");
        Console.WriteLine($"  Pos: ({result[0]}, {result[1]}, {result[2]})");
        Console.WriteLine($"  Normal: ({result[3]}, {result[4]}, {result[5]})");
        Console.WriteLine($"  Color: R={result[6]}, G={result[7]}, B={result[8]}, A={result[9]}");
        Console.WriteLine($"  TexCoord: ({result[10]}, {result[11]})");

        return result;
    }

    /// <summary>
    /// Converts coverage triangles to mesh format with overlap coloring.
    /// </summary>
    public static (float[] vertices, uint[] indices) ConvertCoverageTriangles(
        IEnumerable<Models.Section.CoverageTriangle> triangles)
    {
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        uint vertexIndex = 0;

        Vector3 normal = new Vector3(0, 0, 1);

        foreach (var triangle in triangles)
        {
            if (triangle.Vertices == null || triangle.Vertices.Length < 3)
                continue;

            // Color based on overlap
            Vector4 color = triangle.OverlapCount > 1
                ? new Vector4(1.0f, 0.6f, 0.0f, 0.5f)  // Orange for overlap
                : new Vector4(0.3f, 0.7f, 0.3f, 0.5f); // Green for normal coverage

            // Add three vertices
            for (int i = 0; i < 3; i++)
            {
                var pos = triangle.Vertices[i];
                AddVertex(vertices,
                    new Vector3((float)pos.Easting, (float)pos.Northing, 0.01f), // Slightly above ground
                    normal,
                    color,
                    Vector2.Zero);

                indices.Add(vertexIndex++);
            }
        }

        return (vertices.ToArray(), indices.ToArray());
    }

    /// <summary>
    /// Converts boundary line vertices to mesh format.
    /// </summary>
    public static float[] ConvertBoundaryLines(float[] lineVertices, Vector4 color)
    {
        if (lineVertices == null || lineVertices.Length == 0)
            return Array.Empty<float>();

        int vertexCount = lineVertices.Length / 2; // 2 floats per vertex (x, y)
        List<float> vertices = new List<float>();
        Vector3 normal = new Vector3(0, 0, 1);

        for (int i = 0; i < vertexCount; i++)
        {
            int srcIdx = i * 2;
            Vector3 pos = new Vector3(lineVertices[srcIdx], lineVertices[srcIdx + 1], 0.05f); // Above ground
            AddVertex(vertices, pos, normal, color, Vector2.Zero);
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Converts guidance line vertices to mesh format.
    /// </summary>
    public static float[] ConvertGuidanceLines(float[] lineVertices, Vector4 color)
    {
        if (lineVertices == null || lineVertices.Length == 0)
            return Array.Empty<float>();

        int vertexCount = lineVertices.Length / 2;
        List<float> vertices = new List<float>();
        Vector3 normal = new Vector3(0, 0, 1);

        for (int i = 0; i < vertexCount; i++)
        {
            int srcIdx = i * 2;
            Vector3 pos = new Vector3(lineVertices[srcIdx], lineVertices[srcIdx + 1], 0.06f); // Above boundaries
            AddVertex(vertices, pos, normal, color, Vector2.Zero);
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Creates a textured quad for the vehicle sprite.
    /// Width = side-to-side (X), Height = front-to-back (Y)
    /// </summary>
    public static float[] CreateTexturedVehicleQuad(float width, float height)
    {
        List<float> vertices = new List<float>();
        Vector3 normal = new Vector3(0, 0, 1);
        Vector4 color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White to not tint texture

        float halfWidth = width / 2;
        float halfHeight = height / 2;

        Console.WriteLine($"[CreateTexturedVehicleQuad] Creating textured quad: width={width}m, height={height}m");

        // Two triangles forming a quad, raised above grid
        // Triangle 1 (bottom-left, top-left, top-right)
        AddVertex(vertices, new Vector3(-halfWidth, -halfHeight, 2.0f), normal, color, new Vector2(0, 0)); // Bottom-left
        AddVertex(vertices, new Vector3(-halfWidth, halfHeight, 2.0f), normal, color, new Vector2(0, 1));  // Top-left
        AddVertex(vertices, new Vector3(halfWidth, halfHeight, 2.0f), normal, color, new Vector2(1, 1));   // Top-right

        // Triangle 2 (bottom-left, top-right, bottom-right)
        AddVertex(vertices, new Vector3(-halfWidth, -halfHeight, 2.0f), normal, color, new Vector2(0, 0)); // Bottom-left
        AddVertex(vertices, new Vector3(halfWidth, halfHeight, 2.0f), normal, color, new Vector2(1, 1));   // Top-right
        AddVertex(vertices, new Vector3(halfWidth, -halfHeight, 2.0f), normal, color, new Vector2(1, 0));  // Bottom-right

        return vertices.ToArray();
    }

    private static void AddVertex(List<float> vertices, Vector3 position, Vector3 normal, Vector4 color, Vector2 texCoord)
    {
        // Position
        vertices.Add(position.X);
        vertices.Add(position.Y);
        vertices.Add(position.Z);

        // Normal
        vertices.Add(normal.X);
        vertices.Add(normal.Y);
        vertices.Add(normal.Z);

        // Color
        vertices.Add(color.X);
        vertices.Add(color.Y);
        vertices.Add(color.Z);
        vertices.Add(color.W);

        // TexCoord
        vertices.Add(texCoord.X);
        vertices.Add(texCoord.Y);
    }
}
