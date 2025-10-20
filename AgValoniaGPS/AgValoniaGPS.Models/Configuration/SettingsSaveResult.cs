namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Represents the result of a dual-write settings save operation (JSON and XML).
/// </summary>
public class SettingsSaveResult
{
    /// <summary>
    /// Gets or sets whether the save operation succeeded for both formats.
    /// True only if both JSON and XML were saved successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets whether the JSON file was saved successfully.
    /// </summary>
    public bool JsonSaved { get; set; }

    /// <summary>
    /// Gets or sets whether the XML file was saved successfully.
    /// </summary>
    public bool XmlSaved { get; set; }

    /// <summary>
    /// Gets or sets the error message if either save operation failed.
    /// Empty if both saves succeeded.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
