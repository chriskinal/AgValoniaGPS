using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Service for generating GPU-ready guidance line geometry meshes.
/// Converts guidance lines into vertex arrays suitable for OpenGL rendering.
/// </summary>
public interface IGuidanceGeometryService
{
    /// <summary>
    /// Generates an AB line geometry extending along the line's heading.
    /// </summary>
    /// <param name="line">AB line definition</param>
    /// <param name="visibleLength">Length to extend in each direction (meters)</param>
    /// <returns>Vertex array: [StartX, StartY, EndX, EndY]</returns>
    /// <remarks>
    /// Line extends from (origin - heading * visibleLength) to (origin + heading * visibleLength).
    /// Output format: Two vertices defining a line segment.
    /// </remarks>
    float[] GenerateABLine(ABLine line, float visibleLength);

    /// <summary>
    /// Generates curve line geometry from curve points.
    /// </summary>
    /// <param name="curve">Curve line with point list</param>
    /// <returns>Vertex array: [X1, Y1, X2, Y2, X3, Y3, ...]</returns>
    /// <remarks>
    /// Converts curve points to vertex array for GL_LINE_STRIP rendering.
    /// </remarks>
    float[] GenerateCurveLine(CurveLine curve);

    /// <summary>
    /// Generates contour line geometry from contour points.
    /// </summary>
    /// <param name="contour">Contour line with point list</param>
    /// <returns>Vertex array: [X1, Y1, X2, Y2, X3, Y3, ...]</returns>
    /// <remarks>
    /// Converts contour points to vertex array for GL_LINE_STRIP rendering.
    /// </remarks>
    float[] GenerateContourLine(ContourLine contour);

    /// <summary>
    /// Generates cross-track error indicator (line from vehicle to guidance).
    /// </summary>
    /// <param name="vehiclePos">Current vehicle position</param>
    /// <param name="closestPoint">Closest point on guidance line</param>
    /// <param name="crossTrackError">Cross-track error distance (meters)</param>
    /// <returns>Vertex array: [VehicleX, VehicleY, ClosestX, ClosestY]</returns>
    /// <remarks>
    /// Draws a perpendicular line from vehicle to guidance line.
    /// Output format: Two vertices defining a line segment.
    /// </remarks>
    float[] GenerateCrossTrackIndicator(Position vehiclePos, Position closestPoint, double crossTrackError);
}
