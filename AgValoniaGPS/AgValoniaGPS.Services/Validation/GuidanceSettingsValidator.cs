using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates guidance settings including look-ahead and snap distances.
/// </summary>
internal static class GuidanceSettingsValidator
{
    public static ValidationResult Validate(GuidanceSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate AcquireFactor: 0.5-2.0
        if (settings.AcquireFactor < 0.5 || settings.AcquireFactor > 2.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Guidance.AcquireFactor",
                Message = $"Acquire factor must be between 0.5 and 2.0. Current value: {settings.AcquireFactor}.",
                InvalidValue = settings.AcquireFactor,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.5,
                    MaxValue = 2.0,
                    DataType = "double",
                    ValidationRule = "Line acquisition multiplier"
                }
            });
        }

        // Validate LookAhead: 1.0-10.0 meters
        if (settings.LookAhead < 1.0 || settings.LookAhead > 10.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Guidance.LookAhead",
                Message = $"Look-ahead distance must be between 1.0 and 10.0 meters. Current value: {settings.LookAhead} meters.",
                InvalidValue = settings.LookAhead,
                Constraints = new SettingConstraints
                {
                    MinValue = 1.0,
                    MaxValue = 10.0,
                    DataType = "double",
                    ValidationRule = "Look-ahead distance for guidance"
                }
            });
        }

        // Validate SpeedFactor: 0.5-2.0
        if (settings.SpeedFactor < 0.5 || settings.SpeedFactor > 2.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Guidance.SpeedFactor",
                Message = $"Speed factor must be between 0.5 and 2.0. Current value: {settings.SpeedFactor}.",
                InvalidValue = settings.SpeedFactor,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.5,
                    MaxValue = 2.0,
                    DataType = "double",
                    ValidationRule = "Speed-based adjustment factor"
                }
            });
        }

        // Validate SnapDistance: 1-100 meters
        if (settings.SnapDistance < 1.0 || settings.SnapDistance > 100.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Guidance.SnapDistance",
                Message = $"Snap distance must be between 1 and 100 meters. Current value: {settings.SnapDistance} meters.",
                InvalidValue = settings.SnapDistance,
                Constraints = new SettingConstraints
                {
                    MinValue = 1.0,
                    MaxValue = 100.0,
                    DataType = "double",
                    ValidationRule = "Maximum distance to snap to guidance line"
                }
            });
        }

        // Validate RefSnapDistance: 1-50 meters
        if (settings.RefSnapDistance < 1.0 || settings.RefSnapDistance > 50.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Guidance.RefSnapDistance",
                Message = $"Reference snap distance must be between 1 and 50 meters. Current value: {settings.RefSnapDistance} meters.",
                InvalidValue = settings.RefSnapDistance,
                Constraints = new SettingConstraints
                {
                    MinValue = 1.0,
                    MaxValue = 50.0,
                    DataType = "double",
                    ValidationRule = "Reference line snap distance"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "AcquireFactor" => new SettingConstraints
            {
                MinValue = 0.5,
                MaxValue = 2.0,
                DataType = "double",
                ValidationRule = "Line acquisition multiplier (0.5-2.0)"
            },
            "LookAhead" => new SettingConstraints
            {
                MinValue = 1.0,
                MaxValue = 10.0,
                DataType = "double",
                ValidationRule = "Look-ahead distance for guidance (1.0-10.0 meters)"
            },
            "SpeedFactor" => new SettingConstraints
            {
                MinValue = 0.5,
                MaxValue = 2.0,
                DataType = "double",
                ValidationRule = "Speed-based adjustment factor (0.5-2.0)"
            },
            "SnapDistance" => new SettingConstraints
            {
                MinValue = 1.0,
                MaxValue = 100.0,
                DataType = "double",
                ValidationRule = "Maximum distance to snap to guidance line (1-100 meters)"
            },
            "RefSnapDistance" => new SettingConstraints
            {
                MinValue = 1.0,
                MaxValue = 50.0,
                DataType = "double",
                ValidationRule = "Reference line snap distance (1-50 meters)"
            },
            _ => null
        };
    }
}
