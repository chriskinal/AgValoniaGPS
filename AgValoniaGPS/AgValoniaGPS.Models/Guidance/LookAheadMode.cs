namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Look-ahead distance calculation modes
/// Controls how adaptive look-ahead distance is calculated
/// </summary>
public enum LookAheadMode
{
    /// <summary>
    /// Distance based on speed multiplier and tool width
    /// Distance = speed * multiplier + toolWidth factor
    /// Default mode for most field operations
    /// </summary>
    ToolWidthMultiplier = 0,

    /// <summary>
    /// Distance based on time-to-arrival
    /// Distance = speed * lookAheadTime
    /// Provides constant time horizon for steering response
    /// </summary>
    TimeBased = 1,

    /// <summary>
    /// Constant fixed distance
    /// Distance remains constant regardless of speed or XTE
    /// Useful for specific tuning scenarios
    /// </summary>
    Hold = 2
}
