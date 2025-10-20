using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates steering settings including PWM values and calibration parameters.
/// </summary>
internal static class SteeringSettingsValidator
{
    public static ValidationResult Validate(SteeringSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate CountsPerDegree: 1-1000
        if (settings.CountsPerDegree < 1 || settings.CountsPerDegree > 1000)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Steering.CountsPerDegree",
                Message = $"Counts per degree must be between 1 and 1000. Current value: {settings.CountsPerDegree}.",
                InvalidValue = settings.CountsPerDegree,
                Constraints = new SettingConstraints
                {
                    MinValue = 1,
                    MaxValue = 1000,
                    DataType = "int",
                    ValidationRule = "Counts per degree must be positive and hardware-dependent"
                }
            });
        }

        // Validate Ackermann: 0-200 percentage
        if (settings.Ackermann < 0 || settings.Ackermann > 200)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Steering.Ackermann",
                Message = $"Ackermann percentage must be between 0 and 200. Current value: {settings.Ackermann}.",
                InvalidValue = settings.Ackermann,
                Constraints = new SettingConstraints
                {
                    MinValue = 0,
                    MaxValue = 200,
                    DataType = "int",
                    ValidationRule = "Ackermann steering geometry compensation percentage"
                }
            });
        }

        // Validate WasOffset: -10.0 to 10.0
        if (settings.WasOffset < -10.0 || settings.WasOffset > 10.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Steering.WasOffset",
                Message = $"WAS offset must be between -10.0 and 10.0. Current value: {settings.WasOffset}.",
                InvalidValue = settings.WasOffset,
                Constraints = new SettingConstraints
                {
                    MinValue = -10.0,
                    MaxValue = 10.0,
                    DataType = "double",
                    ValidationRule = "Wheel angle sensor calibration offset"
                }
            });
        }

        // Validate PWM values: 0-255 (standard PWM range)
        ValidatePwmValue(result, "HighPwm", settings.HighPwm);
        ValidatePwmValue(result, "LowPwm", settings.LowPwm);
        ValidatePwmValue(result, "MinPwm", settings.MinPwm);
        ValidatePwmValue(result, "MaxPwm", settings.MaxPwm);

        return result;
    }

    private static void ValidatePwmValue(ValidationResult result, string propertyName, int value)
    {
        if (value < 0 || value > 255)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = $"Steering.{propertyName}",
                Message = $"{propertyName} must be between 0 and 255. Current value: {value}.",
                InvalidValue = value,
                Constraints = new SettingConstraints
                {
                    MinValue = 0,
                    MaxValue = 255,
                    DataType = "int",
                    ValidationRule = "PWM value must be within standard PWM range (0-255)"
                }
            });
        }
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "CountsPerDegree" => new SettingConstraints
            {
                MinValue = 1,
                MaxValue = 1000,
                DataType = "int",
                ValidationRule = "Counts per degree must be positive and hardware-dependent (1-1000)"
            },
            "Ackermann" => new SettingConstraints
            {
                MinValue = 0,
                MaxValue = 200,
                DataType = "int",
                ValidationRule = "Ackermann steering geometry compensation percentage (0-200)"
            },
            "WasOffset" => new SettingConstraints
            {
                MinValue = -10.0,
                MaxValue = 10.0,
                DataType = "double",
                ValidationRule = "Wheel angle sensor calibration offset (-10.0 to 10.0)"
            },
            "HighPwm" or "LowPwm" or "MinPwm" or "MaxPwm" => new SettingConstraints
            {
                MinValue = 0,
                MaxValue = 255,
                DataType = "int",
                ValidationRule = "PWM value must be within standard PWM range (0-255)"
            },
            _ => null
        };
    }
}
