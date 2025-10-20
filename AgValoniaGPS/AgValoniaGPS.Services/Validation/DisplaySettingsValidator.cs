using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates display settings including unit system and data source.
/// </summary>
internal static class DisplaySettingsValidator
{
    public static ValidationResult Validate(DisplaySettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate DeadHeadDelay array has exactly 2 elements
        if (settings.DeadHeadDelay != null && settings.DeadHeadDelay.Length != 2)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Display.DeadHeadDelay",
                Message = $"Dead head delay array must contain exactly 2 elements. Current length: {settings.DeadHeadDelay.Length}.",
                InvalidValue = settings.DeadHeadDelay.Length,
                Constraints = new SettingConstraints
                {
                    DataType = "int[]",
                    ValidationRule = "Array must contain exactly 2 delay values"
                }
            });
        }

        // Validate SpeedSource is one of allowed values
        if (!string.IsNullOrEmpty(settings.SpeedSource))
        {
            var allowedSources = new[] { "GPS", "Wheel", "Simulated" };
            if (!allowedSources.Contains(settings.SpeedSource))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    SettingPath = "Display.SpeedSource",
                    Message = $"Speed source must be one of: GPS, Wheel, Simulated. Current value: '{settings.SpeedSource}'.",
                    InvalidValue = settings.SpeedSource,
                    Constraints = new SettingConstraints
                    {
                        DataType = "string",
                        AllowedValues = allowedSources,
                        ValidationRule = "Speed source must be a valid option"
                    }
                });
            }
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "DeadHeadDelay" => new SettingConstraints
            {
                DataType = "int[]",
                ValidationRule = "Array must contain exactly 2 delay values"
            },
            "SpeedSource" => new SettingConstraints
            {
                DataType = "string",
                AllowedValues = new[] { "GPS", "Wheel", "Simulated" },
                ValidationRule = "Speed source must be GPS, Wheel, or Simulated"
            },
            "UnitSystem" => new SettingConstraints
            {
                DataType = "enum",
                AllowedValues = new[] { "Metric", "Imperial" },
                ValidationRule = "Unit system must be Metric or Imperial"
            },
            "North" or "East" or "Elevation" => new SettingConstraints
            {
                DataType = "double",
                ValidationRule = "Coordinate value in appropriate units"
            },
            _ => null
        };
    }
}
