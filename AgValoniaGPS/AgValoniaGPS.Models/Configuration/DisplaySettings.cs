using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Display and visualization settings including unit system and position coordinates.
/// </summary>
public class DisplaySettings
{
    /// <summary>
    /// Gets or sets the dead head delay values.
    /// Array of 2 integer values.
    /// </summary>
    public int[] DeadHeadDelay { get; set; } = new[] { 10, 10 };

    /// <summary>
    /// Gets or sets the northing coordinate.
    /// </summary>
    public double North { get; set; } = 1.15;

    /// <summary>
    /// Gets or sets the easting coordinate.
    /// </summary>
    public double East { get; set; } = -1.76;

    /// <summary>
    /// Gets or sets the elevation in meters.
    /// </summary>
    public double Elevation { get; set; } = 200.8;

    /// <summary>
    /// Gets or sets the unit system (Metric or Imperial).
    /// </summary>
    public UnitSystem UnitSystem { get; set; } = UnitSystem.Metric;

    /// <summary>
    /// Gets or sets the speed source.
    /// Valid values: "GPS", "Wheel", "Simulated".
    /// </summary>
    public string SpeedSource { get; set; } = "GPS";
}
