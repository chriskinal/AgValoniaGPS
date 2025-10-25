namespace AgValoniaGPS.Models.Enums;

/// <summary>
/// GPS fix quality levels indicating positioning accuracy.
/// </summary>
public enum GpsFixQuality
{
    /// <summary>
    /// No GPS fix available.
    /// </summary>
    NoFix = 0,

    /// <summary>
    /// Standard GPS fix (autonomous).
    /// </summary>
    GPS = 1,

    /// <summary>
    /// Differential GPS fix (DGPS).
    /// </summary>
    DGPS = 2,

    /// <summary>
    /// RTK Float fix (centimeter-level accuracy, not yet converged).
    /// </summary>
    RTKFloat = 4,

    /// <summary>
    /// RTK Fixed fix (centimeter-level accuracy, fully converged).
    /// </summary>
    RTKFixed = 5
}
