namespace AgValoniaGPS.Models.Validation;

/// <summary>
/// Represents a validation error that prevents settings from being saved or applied.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Gets or sets the path to the setting that failed validation.
    /// Format: "Category.PropertyName" (e.g., "Vehicle.Wheelbase").
    /// </summary>
    public string SettingPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable error message describing the validation failure.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invalid value that caused the validation error.
    /// </summary>
    public object? InvalidValue { get; set; }

    /// <summary>
    /// Gets or sets the constraints that were violated.
    /// Provides information about valid ranges and allowed values.
    /// </summary>
    public SettingConstraints? Constraints { get; set; }
}
