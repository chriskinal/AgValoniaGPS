namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Work mode configuration including remote control, switches, and manual operation flags.
/// </summary>
public class WorkModeSettings
{
    /// <summary>
    /// Gets or sets whether remote work mode is enabled.
    /// </summary>
    public bool RemoteWork { get; set; } = false;

    /// <summary>
    /// Gets or sets whether steer work switch is enabled.
    /// </summary>
    public bool SteerWorkSwitch { get; set; } = false;

    /// <summary>
    /// Gets or sets whether steer work is in manual mode.
    /// </summary>
    public bool SteerWorkManual { get; set; } = true;

    /// <summary>
    /// Gets or sets whether work switch is active low.
    /// </summary>
    public bool WorkActiveLow { get; set; } = false;

    /// <summary>
    /// Gets or sets whether work switch is enabled.
    /// </summary>
    public bool WorkSwitch { get; set; } = false;

    /// <summary>
    /// Gets or sets whether section control is in manual mode.
    /// </summary>
    public bool WorkManualSection { get; set; } = true;
}
