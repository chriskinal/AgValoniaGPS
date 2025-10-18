using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Specifies the type of active guidance line
/// </summary>
public enum ActiveLineType
{
    /// <summary>
    /// No guidance line is active
    /// </summary>
    None,

    /// <summary>
    /// AB line guidance is active
    /// </summary>
    ABLine,

    /// <summary>
    /// Curve line guidance is active
    /// </summary>
    Curve,

    /// <summary>
    /// Contour line guidance is active
    /// </summary>
    Contour
}

/// <summary>
/// Event arguments for guidance state changes
/// </summary>
public class GuidanceStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// The type of guidance line currently active
    /// </summary>
    public readonly ActiveLineType ActiveLineType;

    /// <summary>
    /// The current guidance calculation result (if active)
    /// </summary>
    public readonly GuidanceLineResult? Result;

    /// <summary>
    /// Whether guidance is currently active
    /// </summary>
    public readonly bool IsActive;

    /// <summary>
    /// When the state change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of GuidanceStateChangedEventArgs
    /// </summary>
    /// <param name="activeLineType">The type of guidance line currently active</param>
    /// <param name="result">The current guidance calculation result (null if not active)</param>
    /// <param name="isActive">Whether guidance is currently active</param>
    public GuidanceStateChangedEventArgs(
        ActiveLineType activeLineType,
        GuidanceLineResult? result,
        bool isActive)
    {
        ActiveLineType = activeLineType;
        Result = result;
        IsActive = isActive;
        Timestamp = DateTime.UtcNow;
    }
}
