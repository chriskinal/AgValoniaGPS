namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Represents the result of a settings load operation.
/// </summary>
public class SettingsLoadResult
{
    /// <summary>
    /// Gets or sets whether the load operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the loaded application settings.
    /// Null if load failed.
    /// </summary>
    public ApplicationSettings? Settings { get; set; }

    /// <summary>
    /// Gets or sets the error message if the load failed.
    /// Empty if successful.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source from which settings were loaded.
    /// </summary>
    public SettingsLoadSource Source { get; set; }
}
