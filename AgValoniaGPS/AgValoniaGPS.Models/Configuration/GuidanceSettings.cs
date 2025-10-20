namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Guidance system configuration including line acquisition, look-ahead, and snap distances.
/// </summary>
public class GuidanceSettings
{
    /// <summary>
    /// Gets or sets the line acquisition factor.
    /// Valid range: 0.5-2.0.
    /// </summary>
    public double AcquireFactor { get; set; } = 0.90;

    /// <summary>
    /// Gets or sets the look-ahead distance in meters.
    /// Valid range: 1.0-10.0 meters.
    /// </summary>
    public double LookAhead { get; set; } = 3.0;

    /// <summary>
    /// Gets or sets the speed factor for guidance adjustments.
    /// Valid range: 0.5-2.0.
    /// </summary>
    public double SpeedFactor { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the Pure Pursuit integral gain.
    /// Valid range: 0.0-10.0.
    /// </summary>
    public double PurePursuitIntegral { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the maximum snap distance to guidance line in meters.
    /// Valid range: 1-100 meters.
    /// </summary>
    public double SnapDistance { get; set; } = 20.0;

    /// <summary>
    /// Gets or sets the reference line snap distance in meters.
    /// Valid range: 1-50 meters.
    /// </summary>
    public double RefSnapDistance { get; set; } = 5.0;

    /// <summary>
    /// Gets or sets the side hill compensation factor.
    /// </summary>
    public double SideHillComp { get; set; } = 0.0;
}
