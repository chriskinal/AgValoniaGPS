using System.Numerics;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Represents an axis-aligned bounding box in world coordinates.
/// Used for camera FitBounds operations and spatial queries.
/// </summary>
public struct BoundingBox
{
    /// <summary>
    /// Gets or sets the minimum X coordinate (western edge) in meters.
    /// </summary>
    public float MinX { get; set; }

    /// <summary>
    /// Gets or sets the minimum Y coordinate (southern edge) in meters.
    /// </summary>
    public float MinY { get; set; }

    /// <summary>
    /// Gets or sets the maximum X coordinate (eastern edge) in meters.
    /// </summary>
    public float MaxX { get; set; }

    /// <summary>
    /// Gets or sets the maximum Y coordinate (northern edge) in meters.
    /// </summary>
    public float MaxY { get; set; }

    /// <summary>
    /// Gets the width of the bounding box in meters.
    /// </summary>
    public readonly float Width => MaxX - MinX;

    /// <summary>
    /// Gets the height of the bounding box in meters.
    /// </summary>
    public readonly float Height => MaxY - MinY;

    /// <summary>
    /// Gets the center point of the bounding box in world coordinates.
    /// </summary>
    public readonly Vector2 Center => new Vector2((MinX + MaxX) / 2, (MinY + MaxY) / 2);

    /// <summary>
    /// Initializes a new instance of the BoundingBox struct.
    /// </summary>
    /// <param name="minX">Minimum X coordinate (meters)</param>
    /// <param name="minY">Minimum Y coordinate (meters)</param>
    /// <param name="maxX">Maximum X coordinate (meters)</param>
    /// <param name="maxY">Maximum Y coordinate (meters)</param>
    public BoundingBox(float minX, float minY, float maxX, float maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    /// <summary>
    /// Creates a bounding box from a center point and size.
    /// </summary>
    /// <param name="center">Center point in world coordinates</param>
    /// <param name="width">Width in meters</param>
    /// <param name="height">Height in meters</param>
    /// <returns>A bounding box centered at the specified point</returns>
    public static BoundingBox FromCenter(Vector2 center, float width, float height)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;
        return new BoundingBox(
            center.X - halfWidth,
            center.Y - halfHeight,
            center.X + halfWidth,
            center.Y + halfHeight
        );
    }
}
