using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Specifies the type of state change event for a contour
/// </summary>
public enum ContourEventType
{
    /// <summary>
    /// Contour recording has started
    /// </summary>
    RecordingStarted,

    /// <summary>
    /// A new point was added to the contour
    /// </summary>
    PointAdded,

    /// <summary>
    /// Contour was locked for guidance use
    /// </summary>
    Locked,

    /// <summary>
    /// Contour was unlocked
    /// </summary>
    Unlocked
}

/// <summary>
/// Event arguments for contour state changes
/// </summary>
public class ContourStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// The contour line that changed
    /// </summary>
    public readonly ContourLine Contour;

    /// <summary>
    /// The type of event that occurred
    /// </summary>
    public readonly ContourEventType EventType;

    /// <summary>
    /// Current number of points in the contour
    /// </summary>
    public readonly int PointCount;

    /// <summary>
    /// When the change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of ContourStateChangedEventArgs
    /// </summary>
    /// <param name="contour">The contour line that changed</param>
    /// <param name="eventType">The type of event that occurred</param>
    /// <param name="pointCount">Current number of points in the contour</param>
    public ContourStateChangedEventArgs(ContourLine contour, ContourEventType eventType, int pointCount)
    {
        Contour = contour ?? throw new ArgumentNullException(nameof(contour));
        EventType = eventType;
        PointCount = pointCount;
        Timestamp = DateTime.UtcNow;
    }
}
