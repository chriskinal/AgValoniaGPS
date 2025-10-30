using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Implements vehicle geometry mesh generation for OpenGL rendering.
/// Generates GPU-ready vertex arrays from vehicle configuration.
/// </summary>
/// <remarks>
/// Thread Safety: Stateless service, safe for concurrent use
/// Performance: <1ms for all geometry generation
/// Coordinate System: World coordinates (meters), origin at vehicle center
/// </remarks>
public class VehicleGeometryService : IVehicleGeometryService
{
    /// <summary>
    /// Generates a mesh representing the vehicle body as a rectangle.
    /// </summary>
    /// <param name="config">Vehicle configuration with physical dimensions</param>
    /// <param name="r">Red color component (0.0-1.0)</param>
    /// <param name="g">Green color component (0.0-1.0)</param>
    /// <param name="b">Blue color component (0.0-1.0)</param>
    /// <returns>Interleaved vertex array for 2 triangles (6 vertices = 30 floats)</returns>
    public float[] GenerateVehicleMesh(VehicleConfiguration config, float r, float g, float b)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        // Vehicle dimensions from config
        float width = (float)config.TrackWidth;
        float length = (float)config.Wheelbase;

        // Half dimensions for centering at origin
        float halfWidth = width / 2.0f;
        float halfLength = length / 2.0f;

        // Rectangle vertices (2 triangles = 6 vertices)
        // Triangle 1: Top-left, Bottom-left, Bottom-right
        // Triangle 2: Top-left, Bottom-right, Top-right
        // Coordinate system: +X = right, +Y = forward

        return new float[]
        {
            // Triangle 1
            -halfWidth, halfLength, r, g, b,    // Top-left
            -halfWidth, -halfLength, r, g, b,   // Bottom-left
            halfWidth, -halfLength, r, g, b,    // Bottom-right

            // Triangle 2
            -halfWidth, halfLength, r, g, b,    // Top-left
            halfWidth, -halfLength, r, g, b,    // Bottom-right
            halfWidth, halfLength, r, g, b      // Top-right
        };
    }

    /// <summary>
    /// Generates a mesh representing the implement/tool as a rectangle.
    /// </summary>
    /// <param name="toolSettings">Tool configuration with width and offset</param>
    /// <param name="r">Red color component (0.0-1.0)</param>
    /// <param name="g">Green color component (0.0-1.0)</param>
    /// <param name="b">Blue color component (0.0-1.0)</param>
    /// <returns>Interleaved vertex array for 2 triangles (6 vertices = 30 floats)</returns>
    public float[] GenerateImplementMesh(ToolSettings toolSettings, float r, float g, float b)
    {
        if (toolSettings == null)
            throw new ArgumentNullException(nameof(toolSettings));

        float width = (float)toolSettings.ToolWidth;
        float offset = (float)toolSettings.ToolOffset;
        float depth = 1.0f; // Standard implement depth

        // Calculate implement position (centered laterally, behind vehicle)
        // Lateral offset: positive = right, negative = left
        float leftEdge = -width / 2.0f + offset;
        float rightEdge = width / 2.0f + offset;

        // Longitudinal position (typically behind vehicle, adjust based on tool position)
        float frontEdge = -0.5f; // Just behind vehicle center
        float backEdge = frontEdge - depth;

        // Rectangle vertices (2 triangles = 6 vertices)
        return new float[]
        {
            // Triangle 1
            leftEdge, frontEdge, r, g, b,       // Top-left
            leftEdge, backEdge, r, g, b,        // Bottom-left
            rightEdge, backEdge, r, g, b,       // Bottom-right

            // Triangle 2
            leftEdge, frontEdge, r, g, b,       // Top-left
            rightEdge, backEdge, r, g, b,       // Bottom-right
            rightEdge, frontEdge, r, g, b       // Top-right
        };
    }

    /// <summary>
    /// Generates a heading arrow mesh (triangle pointing forward in +Y direction).
    /// </summary>
    /// <param name="length">Length of the arrow in meters</param>
    /// <param name="r">Red color component (0.0-1.0)</param>
    /// <param name="g">Green color component (0.0-1.0)</param>
    /// <param name="b">Blue color component (0.0-1.0)</param>
    /// <returns>Interleaved vertex array for 1 triangle (3 vertices = 15 floats)</returns>
    public float[] GenerateHeadingArrow(float length, float r, float g, float b)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Arrow length must be positive");

        float baseWidth = 0.5f; // Base width of arrow
        float halfBase = baseWidth / 2.0f;

        // Triangle pointing forward (+Y direction)
        // Base at Y=0, tip at Y=length
        return new float[]
        {
            0.0f, length, r, g, b,          // Tip (forward)
            -halfBase, 0.0f, r, g, b,       // Base left
            halfBase, 0.0f, r, g, b         // Base right
        };
    }
}
