namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Result of closest point search on a curve or contour line.
/// Contains the closest position, its index, and distance to query point.
/// </summary>
public class ClosestPointResult
{
    /// <summary>
    /// Gets or sets the closest position on the curve/contour.
    /// </summary>
    public Position Position { get; set; } = new();

    /// <summary>
    /// Gets or sets the index of the closest point in the curve's point list.
    /// Used for optimizing subsequent local searches.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the straight-line distance from query position to closest point in meters.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this result was calculated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
