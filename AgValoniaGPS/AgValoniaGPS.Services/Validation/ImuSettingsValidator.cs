using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates IMU settings including fusion weights and filter parameters.
/// </summary>
internal static class ImuSettingsValidator
{
    public static ValidationResult Validate(ImuSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate ImuFusionWeight: 0.0-1.0
        if (settings.ImuFusionWeight < 0.0 || settings.ImuFusionWeight > 1.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Imu.ImuFusionWeight",
                Message = $"IMU fusion weight must be between 0.0 and 1.0. Current value: {settings.ImuFusionWeight}.",
                InvalidValue = settings.ImuFusionWeight,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.0,
                    MaxValue = 1.0,
                    DataType = "double",
                    ValidationRule = "IMU fusion weight factor"
                }
            });
        }

        // Validate MinHeadingStep: 0.01-5.0 degrees
        if (settings.MinHeadingStep < 0.01 || settings.MinHeadingStep > 5.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Imu.MinHeadingStep",
                Message = $"Minimum heading step must be between 0.01 and 5.0 degrees. Current value: {settings.MinHeadingStep} degrees.",
                InvalidValue = settings.MinHeadingStep,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.01,
                    MaxValue = 5.0,
                    DataType = "double",
                    ValidationRule = "Heading change threshold"
                }
            });
        }

        // Validate RollFilter: 0.0-1.0
        if (settings.RollFilter < 0.0 || settings.RollFilter > 1.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Imu.RollFilter",
                Message = $"Roll filter must be between 0.0 and 1.0. Current value: {settings.RollFilter}.",
                InvalidValue = settings.RollFilter,
                Constraints = new SettingConstraints
                {
                    MinValue = 0.0,
                    MaxValue = 1.0,
                    DataType = "double",
                    ValidationRule = "Roll filter coefficient"
                }
            });
        }

        // Validate DualHeadingOffset: 0-359 degrees
        if (settings.DualHeadingOffset < 0 || settings.DualHeadingOffset > 359)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Imu.DualHeadingOffset",
                Message = $"Dual heading offset must be between 0 and 359 degrees. Current value: {settings.DualHeadingOffset} degrees.",
                InvalidValue = settings.DualHeadingOffset,
                Constraints = new SettingConstraints
                {
                    MinValue = 0,
                    MaxValue = 359,
                    DataType = "int",
                    ValidationRule = "Antenna orientation offset"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "ImuFusionWeight" => new SettingConstraints
            {
                MinValue = 0.0,
                MaxValue = 1.0,
                DataType = "double",
                ValidationRule = "IMU fusion weight factor (0.0-1.0)"
            },
            "MinHeadingStep" => new SettingConstraints
            {
                MinValue = 0.01,
                MaxValue = 5.0,
                DataType = "double",
                ValidationRule = "Heading change threshold (0.01-5.0 degrees)"
            },
            "RollFilter" => new SettingConstraints
            {
                MinValue = 0.0,
                MaxValue = 1.0,
                DataType = "double",
                ValidationRule = "Roll filter coefficient (0.0-1.0)"
            },
            "DualHeadingOffset" => new SettingConstraints
            {
                MinValue = 0,
                MaxValue = 359,
                DataType = "int",
                ValidationRule = "Antenna orientation offset (0-359 degrees)"
            },
            _ => null
        };
    }
}
