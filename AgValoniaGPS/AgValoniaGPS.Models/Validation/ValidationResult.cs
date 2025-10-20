using System.Collections.Generic;

namespace AgValoniaGPS.Models.Validation;

/// <summary>
/// Represents the result of a validation operation including errors and warnings.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation passed without errors.
    /// True if no errors are present (warnings do not prevent validity).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors found.
    /// Errors prevent the settings from being saved or applied.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation warnings found.
    /// Warnings indicate potential issues but do not prevent saving.
    /// </summary>
    public List<ValidationWarning> Warnings { get; set; } = new();
}
