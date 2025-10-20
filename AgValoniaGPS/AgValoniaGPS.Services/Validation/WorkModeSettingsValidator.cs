using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates work mode settings (boolean flags only, minimal validation required).
/// </summary>
internal static class WorkModeSettingsValidator
{
    public static ValidationResult Validate(WorkModeSettings settings)
    {
        // Work mode settings are all boolean flags - type validation is implicit
        // No range checks or complex validation needed
        var result = new ValidationResult { IsValid = true };
        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        // All properties are boolean, return generic boolean constraint
        return new SettingConstraints
        {
            DataType = "bool",
            ValidationRule = "Boolean flag for work mode configuration"
        };
    }
}
