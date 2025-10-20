using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates GPS settings including update rates and quality thresholds.
/// </summary>
internal static class GpsSettingsValidator
{
    public static ValidationResult Validate(GpsSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate Hz: 1-20
        if (settings.Hz < 1.0 || settings.Hz > 20.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Gps.Hz",
                Message = $"GPS update rate must be between 1 and 20 Hz. Current value: {settings.Hz} Hz.",
                InvalidValue = settings.Hz,
                Constraints = new SettingConstraints
                {
                    MinValue = 1.0,
                    MaxValue = 20.0,
                    DataType = "double",
                    ValidationRule = "GPS update rate must be within realistic GPS update rates"
                }
            });
        }

        // Validate GpsAgeAlarm: 1-60 seconds
        if (settings.GpsAgeAlarm < 1 || settings.GpsAgeAlarm > 60)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Gps.GpsAgeAlarm",
                Message = $"GPS age alarm must be between 1 and 60 seconds. Current value: {settings.GpsAgeAlarm} seconds.",
                InvalidValue = settings.GpsAgeAlarm,
                Constraints = new SettingConstraints
                {
                    MinValue = 1,
                    MaxValue = 60,
                    DataType = "int",
                    ValidationRule = "GPS age alarm must be within reasonable data freshness threshold"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "Hz" => new SettingConstraints
            {
                MinValue = 1.0,
                MaxValue = 20.0,
                DataType = "double",
                ValidationRule = "GPS update rate must be within realistic GPS update rates (1-20 Hz)"
            },
            "GpsAgeAlarm" => new SettingConstraints
            {
                MinValue = 1,
                MaxValue = 60,
                DataType = "int",
                ValidationRule = "GPS age alarm must be within reasonable data freshness threshold (1-60 seconds)"
            },
            _ => null
        };
    }
}
