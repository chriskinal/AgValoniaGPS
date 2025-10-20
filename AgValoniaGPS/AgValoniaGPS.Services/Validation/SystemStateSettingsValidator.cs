using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates system state settings (runtime state, minimal validation required).
/// </summary>
internal static class SystemStateSettingsValidator
{
    public static ValidationResult Validate(SystemStateSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // System state settings are runtime values - minimal validation
        // Check for NaN or Infinity in double values
        if (double.IsNaN(settings.Heading) || double.IsInfinity(settings.Heading))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "SystemState.Heading",
                Message = "Heading must be a valid number (not NaN or Infinity).",
                InvalidValue = settings.Heading,
                Constraints = new SettingConstraints
                {
                    DataType = "double",
                    ValidationRule = "Must be a valid numeric value"
                }
            });
        }

        if (double.IsNaN(settings.ImuHeading) || double.IsInfinity(settings.ImuHeading))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "SystemState.ImuHeading",
                Message = "IMU heading must be a valid number (not NaN or Infinity).",
                InvalidValue = settings.ImuHeading,
                Constraints = new SettingConstraints
                {
                    DataType = "double",
                    ValidationRule = "Must be a valid numeric value"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "Heading" or "ImuHeading" => new SettingConstraints
            {
                DataType = "double",
                ValidationRule = "Must be a valid numeric value (not NaN or Infinity)"
            },
            "StanleyUsed" or "SteerInReverse" or "ReverseOn" => new SettingConstraints
            {
                DataType = "bool",
                ValidationRule = "Boolean runtime state flag"
            },
            "HeadingError" or "DistanceError" or "SteerIntegral" => new SettingConstraints
            {
                DataType = "int",
                ValidationRule = "Integer runtime state value"
            },
            _ => null
        };
    }
}
