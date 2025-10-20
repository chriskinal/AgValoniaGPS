using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents user-specific preferences that are independent of vehicle configuration.
/// Allows multiple operators to maintain their own display and language settings.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// Gets or sets the user's preferred unit system (Metric or Imperial).
    /// </summary>
    public UnitSystem PreferredUnitSystem { get; set; } = UnitSystem.Metric;

    /// <summary>
    /// Gets or sets the user's preferred language code (e.g., "en", "de", "fr").
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets the user's display preferences for UI customization.
    /// </summary>
    public DisplayPreferences DisplayPreferences { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the last vehicle profile used by this user.
    /// Used to automatically load the correct vehicle on startup.
    /// </summary>
    public string LastUsedVehicleProfile { get; set; } = string.Empty;
}
