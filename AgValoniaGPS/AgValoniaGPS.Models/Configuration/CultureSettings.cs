namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Culture and localization settings for language and regional preferences.
/// </summary>
public class CultureSettings
{
    /// <summary>
    /// Gets or sets the culture code (e.g., "en", "de", "fr").
    /// Must match a valid locale code.
    /// </summary>
    public string CultureCode { get; set; } = "en";

    /// <summary>
    /// Gets or sets the display name of the language.
    /// </summary>
    public string LanguageName { get; set; } = "English";
}
