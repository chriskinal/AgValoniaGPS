namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Defines the source from which settings were loaded.
/// </summary>
public enum SettingsLoadSource
{
    /// <summary>
    /// Settings were loaded from JSON file (primary format).
    /// </summary>
    Json = 0,

    /// <summary>
    /// Settings were loaded from XML file (legacy format).
    /// </summary>
    Xml = 1,

    /// <summary>
    /// Settings were loaded from default values (no file found).
    /// </summary>
    Defaults = 2
}
