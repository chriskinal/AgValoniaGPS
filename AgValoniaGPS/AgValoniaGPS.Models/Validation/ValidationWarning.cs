namespace AgValoniaGPS.Models.Validation;

/// <summary>
/// Represents a validation warning that indicates a potential issue but does not prevent saving.
/// </summary>
public class ValidationWarning
{
    /// <summary>
    /// Gets or sets the path to the setting that triggered the warning.
    /// Format: "Category.PropertyName" (e.g., "Guidance.LookAhead").
    /// </summary>
    public string SettingPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value that triggered the warning.
    /// </summary>
    public object? Value { get; set; }
}
