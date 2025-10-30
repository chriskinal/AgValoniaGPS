using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Service for generating GPU-ready boundary geometry meshes.
/// Converts boundary polygons into vertex arrays suitable for OpenGL rendering.
/// </summary>
public interface IBoundaryGeometryService
{
    /// <summary>
    /// Generates a line strip from boundary points.
    /// </summary>
    /// <param name="boundaryPoints">List of positions defining the boundary</param>
    /// <returns>Vertex array: [X1, Y1, X2, Y2, X3, Y3, ...]</returns>
    /// <remarks>
    /// For closed boundaries, ensure last point equals first point.
    /// Output format: Sequential (X, Y) pairs for GL_LINE_STRIP rendering.
    /// </remarks>
    float[] GenerateBoundaryLines(List<Position> boundaryPoints);

    /// <summary>
    /// Generates a triangulated filled mesh from boundary points.
    /// </summary>
    /// <param name="boundaryPoints">List of positions defining the boundary polygon</param>
    /// <returns>Vertex array: [X1, Y1, X2, Y2, X3, Y3, ...]</returns>
    /// <remarks>
    /// Uses simple ear clipping triangulation for convex/simple polygons.
    /// Output format: Triangle vertices (3 vertices per triangle).
    /// </remarks>
    float[] GenerateBoundaryFill(List<Position> boundaryPoints);

    /// <summary>
    /// Generates headland line strip from headland points.
    /// </summary>
    /// <param name="headlandPoints">List of positions defining the headland</param>
    /// <returns>Vertex array: [X1, Y1, X2, Y2, X3, Y3, ...]</returns>
    /// <remarks>
    /// Headlands are inner offset boundaries.
    /// Same output format as boundary lines.
    /// </remarks>
    float[] GenerateHeadlandLines(List<Position> headlandPoints);
}
