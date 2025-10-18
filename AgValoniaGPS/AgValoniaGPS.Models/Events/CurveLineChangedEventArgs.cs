using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Specifies the type of change that occurred to a curve line
/// </summary>
public enum CurveLineChangeType
{
    /// <summary>
    /// Curve recording started
    /// </summary>
    RecordingStarted,

    /// <summary>
    /// Point added during recording
    /// </summary>
    PointAdded,

    /// <summary>
    /// Curve line points were recorded
    /// </summary>
    Recorded,

    /// <summary>
    /// Curve line was smoothed/processed
    /// </summary>
    Smoothed,

    /// <summary>
    /// Curve line was activated for guidance
    /// </summary>
    Activated
}

/// <summary>
/// Event arguments for curve line state changes
/// </summary>
public class CurveLineChangedEventArgs : EventArgs
{
    /// <summary>
    /// The curve line that changed (may be null for recording events)
    /// </summary>
    public readonly CurveLine? Curve;

    /// <summary>
    /// The type of change that occurred
    /// </summary>
    public readonly CurveLineChangeType ChangeType;

    /// <summary>
    /// Number of points in curve (useful for recording events)
    /// </summary>
    public readonly int PointCount;

    /// <summary>
    /// When the change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of CurveLineChangedEventArgs
    /// </summary>
    /// <param name="curve">The curve line that changed (may be null for recording events)</param>
    /// <param name="changeType">The type of change that occurred</param>
    /// <param name="pointCount">Number of points in curve</param>
    public CurveLineChangedEventArgs(CurveLine? curve, CurveLineChangeType changeType, int pointCount = 0)
    {
        Curve = curve;
        ChangeType = changeType;
        PointCount = pointCount;
        Timestamp = DateTime.UtcNow;
    }
}
