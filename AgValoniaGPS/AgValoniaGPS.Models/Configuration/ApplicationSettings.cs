namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Root container for all application settings, organized into logical categories.
/// Supports hierarchical JSON serialization for modern configuration management.
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Gets or sets the vehicle physical configuration and dimensions.
    /// </summary>
    public VehicleSettings Vehicle { get; set; } = new();

    /// <summary>
    /// Gets or sets the steering system configuration and parameters.
    /// </summary>
    public SteeringSettings Steering { get; set; } = new();

    /// <summary>
    /// Gets or sets the implement/tool configuration and parameters.
    /// </summary>
    public ToolSettings Tool { get; set; } = new();

    /// <summary>
    /// Gets or sets the section control configuration.
    /// </summary>
    public SectionControlSettings SectionControl { get; set; } = new();

    /// <summary>
    /// Gets or sets the GPS receiver configuration.
    /// </summary>
    public GpsSettings Gps { get; set; } = new();

    /// <summary>
    /// Gets or sets the IMU (Inertial Measurement Unit) configuration.
    /// </summary>
    public ImuSettings Imu { get; set; } = new();

    /// <summary>
    /// Gets or sets the guidance system configuration.
    /// </summary>
    public GuidanceSettings Guidance { get; set; } = new();

    /// <summary>
    /// Gets or sets the work mode and switch configuration.
    /// </summary>
    public WorkModeSettings WorkMode { get; set; } = new();

    /// <summary>
    /// Gets or sets the culture and localization settings.
    /// </summary>
    public CultureSettings Culture { get; set; } = new();

    /// <summary>
    /// Gets or sets the system runtime state settings.
    /// </summary>
    public SystemStateSettings SystemState { get; set; } = new();

    /// <summary>
    /// Gets or sets the display and visualization settings.
    /// </summary>
    public DisplaySettings Display { get; set; } = new();
}
