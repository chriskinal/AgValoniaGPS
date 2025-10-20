namespace AgValoniaGPS.Models.Validation;

/// <summary>
/// Represents the validation constraints for a specific setting.
/// Used to communicate valid ranges and allowed values to users and validators.
/// </summary>
public class SettingConstraints
{
    /// <summary>
    /// Gets or sets the minimum allowed value for numeric settings.
    /// Null if no minimum constraint exists.
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value for numeric settings.
    /// Null if no maximum constraint exists.
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the array of allowed values for enumeration or choice settings.
    /// Null if not applicable.
    /// </summary>
    public string[]? AllowedValues { get; set; }

    /// <summary>
    /// Gets or sets the data type of the setting (e.g., "int", "double", "bool", "string").
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the array of setting paths that this setting depends on.
    /// Used for cross-setting validation rules.
    /// </summary>
    public string[]? Dependencies { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description of the validation rule.
    /// </summary>
    public string ValidationRule { get; set; } = string.Empty;
}
