namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// GPS receiver configuration including update rate, accuracy, and heading source.
/// </summary>
public class GpsSettings
{
    /// <summary>
    /// Gets or sets the Horizontal Dilution of Precision (HDOP).
    /// Lower values indicate better GPS accuracy.
    /// </summary>
    public double Hdop { get; set; } = 0.69;

    /// <summary>
    /// Gets or sets the raw GPS update frequency in Hz.
    /// </summary>
    public double RawHz { get; set; } = 9.890;

    /// <summary>
    /// Gets or sets the filtered GPS update frequency in Hz.
    /// Valid range: 1-20 Hz.
    /// </summary>
    public double Hz { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets the GPS data age alarm threshold in seconds.
    /// Valid range: 1-60 seconds.
    /// </summary>
    public int GpsAgeAlarm { get; set; } = 20;

    /// <summary>
    /// Gets or sets the heading source.
    /// Valid values: "GPS", "Dual", "IMU".
    /// </summary>
    public string HeadingFrom { get; set; } = "Dual";

    /// <summary>
    /// Gets or sets whether to auto-start AgIO communication on startup.
    /// </summary>
    public bool AutoStartAgIO { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to auto-close AgIO on application exit.
    /// </summary>
    public bool AutoOffAgIO { get; set; } = false;

    /// <summary>
    /// Gets or sets whether RTK (Real-Time Kinematic) correction is enabled.
    /// </summary>
    public bool Rtk { get; set; } = false;

    /// <summary>
    /// Gets or sets whether RTK loss kills auto-steer.
    /// </summary>
    public bool RtkKillAutoSteer { get; set; } = false;
}
