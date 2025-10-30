using System.Collections.Generic;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Service for generating GPU-ready coverage triangle meshes.
/// Converts coverage triangles into vertex arrays suitable for OpenGL rendering.
/// </summary>
public interface ICoverageGeometryService
{
    /// <summary>
    /// Generates a coverage mesh from coverage triangles.
    /// </summary>
    /// <param name="triangles">Collection of coverage triangles to render</param>
    /// <returns>Coverage mesh with interleaved vertices and indices</returns>
    /// <remarks>
    /// Vertex format: X, Y, R, G, B (5 floats per vertex)
    /// Index format: Sequential (0, 1, 2, 3, 4, 5, ...)
    /// Colors assigned based on pass number:
    /// - Pass 1 (OverlapCount=1) = Green (0.0, 1.0, 0.0)
    /// - Pass 2 (OverlapCount=2) = Yellow (1.0, 1.0, 0.0)
    /// - Pass 3+ (OverlapCount>=3) = Orange (1.0, 0.5, 0.0)
    /// </remarks>
    CoverageMesh GenerateCoverageMesh(IEnumerable<CoverageTriangle> triangles);
}
