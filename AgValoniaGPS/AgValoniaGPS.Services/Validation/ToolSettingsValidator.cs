using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates tool settings including dimensions and configuration.
/// </summary>
internal static class ToolSettingsValidator
{
    public static ValidationResult Validate(ToolSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate ToolWidth: 0.1-50.0 meters
        if (settings.ToolWidth < 0.1 || settings.ToolWidth > 50.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Tool.ToolWidth",
                Message = $"Tool width must be between 0.1 and 50.0 meters. Current value: {settings.ToolWidth} meters.",
                InvalidValue = settings.ToolWidth,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.1,
                    MaxValue = 50.0,
                    DataType = "double",
                    ValidationRule = "Tool width must be within realistic implement widths"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "ToolWidth" => new SettingConstraints
            {
                MinValue = 0.1,
                MaxValue = 50.0,
                DataType = "double",
                ValidationRule = "Tool width must be within realistic implement widths (0.1-50.0 meters)"
            },
            _ => null
        };
    }
}
