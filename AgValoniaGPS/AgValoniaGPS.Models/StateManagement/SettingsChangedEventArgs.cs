using System;

namespace AgValoniaGPS.Models.StateManagement;

/// <summary>
/// Event arguments for settings change notifications.
/// Provides context about which settings category changed and when.
/// </summary>
public class SettingsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the category of settings that changed.
    /// </summary>
    public SettingsCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the previous value before the change.
    /// </summary>
    public object? OldValue { get; set; }

    /// <summary>
    /// Gets or sets the new value after the change.
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumeration of settings categories for change tracking.
/// </summary>
public enum SettingsCategory
{
    /// <summary>
    /// Vehicle physical configuration.
    /// </summary>
    Vehicle,

    /// <summary>
    /// Steering system configuration.
    /// </summary>
    Steering,

    /// <summary>
    /// Tool/implement configuration.
    /// </summary>
    Tool,

    /// <summary>
    /// Section control configuration.
    /// </summary>
    SectionControl,

    /// <summary>
    /// GPS receiver configuration.
    /// </summary>
    Gps,

    /// <summary>
    /// IMU configuration.
    /// </summary>
    Imu,

    /// <summary>
    /// Guidance system configuration.
    /// </summary>
    Guidance,

    /// <summary>
    /// Work mode configuration.
    /// </summary>
    WorkMode,

    /// <summary>
    /// Culture and localization configuration.
    /// </summary>
    Culture,

    /// <summary>
    /// System runtime state configuration.
    /// </summary>
    SystemState,

    /// <summary>
    /// Display and visualization configuration.
    /// </summary>
    Display,

    /// <summary>
    /// All settings categories (used for bulk changes).
    /// </summary>
    All
}
