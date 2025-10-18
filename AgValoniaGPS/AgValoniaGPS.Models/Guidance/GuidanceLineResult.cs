namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Results from guidance calculation
/// </summary>
public class GuidanceLineResult
{
    /// <summary>
    /// Cross-track error in meters (positive = right of line, negative = left)
    /// </summary>
    public double CrossTrackError { get; set; }

    /// <summary>
    /// Closest point on the line to current position
    /// </summary>
    public Position ClosestPoint { get; set; } = new();

    /// <summary>
    /// Heading error in degrees (difference between vehicle and line heading)
    /// </summary>
    public double HeadingError { get; set; }

    /// <summary>
    /// Distance from current position to line in meters
    /// </summary>
    public double DistanceToLine { get; set; }

    /// <summary>
    /// Index of closest point (for curves, not applicable to AB lines)
    /// </summary>
    public int ClosestPointIndex { get; set; } = -1;
}
