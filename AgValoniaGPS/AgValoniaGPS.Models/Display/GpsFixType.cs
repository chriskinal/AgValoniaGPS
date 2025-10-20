namespace AgValoniaGPS.Models.Display;

/// <summary>
/// Defines GPS fix quality types based on NMEA GGA quality indicator values.
/// These values indicate the accuracy and reliability of GPS positioning data.
/// </summary>
public enum GpsFixType
{
    /// <summary>
    /// No GPS fix available - position is invalid.
    /// Indicates GPS receiver is not receiving signals or position cannot be calculated.
    /// </summary>
    None = 0,

    /// <summary>
    /// Autonomous GPS fix - standard GPS positioning without corrections.
    /// Typical accuracy: 5-15 meters horizontal. No differential corrections applied.
    /// </summary>
    Autonomous = 1,

    /// <summary>
    /// Differential GPS (DGPS) fix - GPS with real-time corrections from base station.
    /// Typical accuracy: 1-5 meters horizontal. Uses SBAS (WAAS/EGNOS) or local corrections.
    /// </summary>
    DGPS = 2,

    /// <summary>
    /// RTK Fixed solution - highest precision GPS with carrier-phase corrections and ambiguity resolution.
    /// Typical accuracy: 1-2 centimeters horizontal. Requires stable base station signal.
    /// </summary>
    RtkFixed = 4,

    /// <summary>
    /// RTK Float solution - high precision GPS with carrier-phase corrections but without full ambiguity resolution.
    /// Typical accuracy: 10-50 centimeters horizontal. Intermediate state between DGPS and RTK Fixed.
    /// </summary>
    RtkFloat = 5
}
