using System;
using System.Numerics;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Container for vehicle rendering data.
/// </summary>
public class VehicleRenderData
{
    /// <summary>
    /// Vehicle position in world coordinates (meters)
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Vehicle heading in radians (0 = North/+Y, increases clockwise)
    /// </summary>
    public float Heading { get; set; }

    /// <summary>
    /// Vehicle shape vertices (interleaved X, Y, R, G, B)
    /// </summary>
    public float[]? Vertices { get; set; }

    /// <summary>
    /// Index array for indexed rendering (optional, can be null for non-indexed)
    /// </summary>
    public uint[]? Indices { get; set; }

    /// <summary>
    /// RGB color for vehicle (if not per-vertex colors)
    /// </summary>
    public Vector3 Color { get; set; }
}

/// <summary>
/// Container for boundary rendering data.
/// </summary>
public class BoundaryRenderData
{
    /// <summary>
    /// Line strip vertices (interleaved X, Y)
    /// </summary>
    public float[]? Vertices { get; set; }

    /// <summary>
    /// RGB color for boundary lines
    /// </summary>
    public Vector3 Color { get; set; }

    /// <summary>
    /// Line width in pixels
    /// </summary>
    public float LineWidth { get; set; }
}

/// <summary>
/// Container for guidance line rendering data.
/// </summary>
public class GuidanceRenderData
{
    /// <summary>
    /// Line vertices (interleaved X, Y)
    /// </summary>
    public float[]? Vertices { get; set; }

    /// <summary>
    /// RGB color for guidance lines
    /// </summary>
    public Vector3 Color { get; set; }

    /// <summary>
    /// Line width in pixels
    /// </summary>
    public float LineWidth { get; set; }

    /// <summary>
    /// Type of guidance line (for different rendering styles)
    /// </summary>
    public string Type { get; set; } = "Unknown";
}

/// <summary>
/// Container for coverage map rendering data.
/// </summary>
public class CoverageRenderData
{
    /// <summary>
    /// Triangle vertices (interleaved X, Y, R, G, B)
    /// </summary>
    public float[]? Vertices { get; set; }

    /// <summary>
    /// Index array for indexed rendering
    /// </summary>
    public uint[]? Indices { get; set; }

    /// <summary>
    /// Number of triangles in the mesh
    /// </summary>
    public int TriangleCount { get; set; }
}

/// <summary>
/// Container for section overlay rendering data.
/// </summary>
public class SectionRenderData
{
    /// <summary>
    /// Section overlay vertices (interleaved X, Y, R, G, B, A)
    /// </summary>
    public float[]? Vertices { get; set; }

    /// <summary>
    /// Number of sections
    /// </summary>
    public int SectionCount { get; set; }
}

/// <summary>
/// Container for tram line rendering data.
/// </summary>
public class TramLineRenderData
{
    /// <summary>
    /// Line strip vertices (interleaved X, Y)
    /// </summary>
    public float[]? Vertices { get; set; }

    /// <summary>
    /// RGB color for tram lines
    /// </summary>
    public Vector3 Color { get; set; }

    /// <summary>
    /// Line width in pixels
    /// </summary>
    public float LineWidth { get; set; }
}
