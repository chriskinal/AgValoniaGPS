using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates section control settings including section count and positions.
/// </summary>
internal static class SectionControlSettingsValidator
{
    public static ValidationResult Validate(SectionControlSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate NumberSections: 1-32
        if (settings.NumberSections < 1 || settings.NumberSections > 32)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "SectionControl.NumberSections",
                Message = $"Number of sections must be between 1 and 32. Current value: {settings.NumberSections}.",
                InvalidValue = settings.NumberSections,
                Constraints = new SettingConstraints
                {
                    MinValue = 1,
                    MaxValue = 32,
                    DataType = "int",
                    ValidationRule = "Number of sections must match physical hardware"
                }
            });
        }

        // Validate LowSpeedCutoff: 0.1-5.0 m/s
        if (settings.LowSpeedCutoff < 0.1 || settings.LowSpeedCutoff > 5.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "SectionControl.LowSpeedCutoff",
                Message = $"Low speed cutoff must be between 0.1 and 5.0 m/s. Current value: {settings.LowSpeedCutoff} m/s.",
                InvalidValue = settings.LowSpeedCutoff,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.1,
                    MaxValue = 5.0,
                    DataType = "double",
                    ValidationRule = "Low speed cutoff must be within realistic minimum working speed"
                }
            });
        }

        // Validate SectionPositions array length matches NumberSections
        if (settings.SectionPositions != null &&
            settings.SectionPositions.Length != settings.NumberSections)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "SectionControl.SectionPositions",
                Message = $"Section positions array length ({settings.SectionPositions.Length}) must match number of sections ({settings.NumberSections}).",
                InvalidValue = settings.SectionPositions.Length,
                Constraints = new SettingConstraints
                {
                    DataType = "double[]",
                    Dependencies = new[] { "SectionControl.NumberSections" },
                    ValidationRule = "Section positions array length must equal NumberSections"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "NumberSections" => new SettingConstraints
            {
                MinValue = 1,
                MaxValue = 32,
                DataType = "int",
                ValidationRule = "Number of sections must match physical hardware (1-32)"
            },
            "LowSpeedCutoff" => new SettingConstraints
            {
                MinValue = 0.1,
                MaxValue = 5.0,
                DataType = "double",
                ValidationRule = "Low speed cutoff must be within realistic minimum working speed (0.1-5.0 m/s)"
            },
            "SectionPositions" => new SettingConstraints
            {
                DataType = "double[]",
                Dependencies = new[] { "SectionControl.NumberSections" },
                ValidationRule = "Section positions array length must equal NumberSections"
            },
            _ => null
        };
    }
}
