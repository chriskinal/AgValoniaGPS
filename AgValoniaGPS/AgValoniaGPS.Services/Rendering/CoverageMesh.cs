namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Represents a GPU-ready coverage triangle mesh.
/// Contains interleaved vertex data and index array for rendering.
/// </summary>
public class CoverageMesh
{
    /// <summary>
    /// Interleaved vertex array: X, Y, R, G, B per vertex.
    /// Format: [X1, Y1, R1, G1, B1, X2, Y2, R2, G2, B2, ...]
    /// </summary>
    public float[] Vertices { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Triangle indices (0, 1, 2, 3, 4, 5, ...).
    /// 3 indices per triangle, sequential ordering.
    /// </summary>
    public uint[] Indices { get; set; } = Array.Empty<uint>();

    /// <summary>
    /// Number of triangles in the mesh.
    /// </summary>
    public int TriangleCount { get; set; }

    /// <summary>
    /// Number of vertices in the mesh.
    /// </summary>
    public int VertexCount => Vertices.Length / 5; // 5 floats per vertex (X, Y, R, G, B)
}
