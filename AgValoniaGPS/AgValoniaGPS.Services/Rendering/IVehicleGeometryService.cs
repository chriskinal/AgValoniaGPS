using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Service for generating GPU-ready vehicle geometry meshes.
/// Converts vehicle configuration into vertex arrays suitable for OpenGL rendering.
/// </summary>
public interface IVehicleGeometryService
{
    /// <summary>
    /// Generates a mesh representing the vehicle body.
    /// </summary>
    /// <param name="config">Vehicle configuration with physical dimensions</param>
    /// <param name="r">Red color component (0.0-1.0)</param>
    /// <param name="g">Green color component (0.0-1.0)</param>
    /// <param name="b">Blue color component (0.0-1.0)</param>
    /// <returns>Interleaved vertex array: [X1, Y1, R1, G1, B1, X2, Y2, R2, G2, B2, ...]</returns>
    /// <remarks>
    /// Vehicle mesh is a rectangle centered at origin:
    /// - Width = track width from config
    /// - Length = wheelbase from config
    /// - Vehicle position/heading will be applied during rendering
    /// </remarks>
    float[] GenerateVehicleMesh(VehicleConfiguration config, float r, float g, float b);

    /// <summary>
    /// Generates a mesh representing the implement/tool.
    /// </summary>
    /// <param name="toolSettings">Tool configuration with width and offset</param>
    /// <param name="r">Red color component (0.0-1.0)</param>
    /// <param name="g">Green color component (0.0-1.0)</param>
    /// <param name="b">Blue color component (0.0-1.0)</param>
    /// <returns>Interleaved vertex array: [X1, Y1, R1, G1, B1, X2, Y2, R2, G2, B2, ...]</returns>
    /// <remarks>
    /// Implement mesh is a rectangle:
    /// - Width = tool width from config
    /// - Length = 1.0m (standard depth)
    /// - Offset = lateral offset from vehicle centerline
    /// </remarks>
    float[] GenerateImplementMesh(ToolSettings toolSettings, float r, float g, float b);

    /// <summary>
    /// Generates a heading arrow mesh (triangle pointing forward).
    /// </summary>
    /// <param name="length">Length of the arrow in meters</param>
    /// <param name="r">Red color component (0.0-1.0)</param>
    /// <param name="g">Green color component (0.0-1.0)</param>
    /// <param name="b">Blue color component (0.0-1.0)</param>
    /// <returns>Interleaved vertex array: [X1, Y1, R1, G1, B1, X2, Y2, R2, G2, B2, ...]</returns>
    /// <remarks>
    /// Arrow points in +Y direction (forward):
    /// - Triangle with base width 0.5m
    /// - Points from origin to (0, length)
    /// </remarks>
    float[] GenerateHeadingArrow(float length, float r, float g, float b);
}
