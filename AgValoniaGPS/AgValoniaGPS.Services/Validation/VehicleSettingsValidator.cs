using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates vehicle settings including physical dimensions and steering limits.
/// </summary>
internal static class VehicleSettingsValidator
{
    public static ValidationResult Validate(VehicleSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate Wheelbase: 50-500 cm
        if (settings.Wheelbase < 50.0 || settings.Wheelbase > 500.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Vehicle.Wheelbase",
                Message = $"Wheelbase must be between 50 and 500 cm. Current value: {settings.Wheelbase} cm.",
                InvalidValue = settings.Wheelbase,
                Constraints = new SettingConstraints
                {
                    MinValue = 50.0,
                    MaxValue = 500.0,
                    DataType = "double",
                    ValidationRule = "Wheelbase must be within realistic range for agricultural vehicles"
                }
            });
        }

        // Validate Track: 10-400 cm
        if (settings.Track < 10.0 || settings.Track > 400.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Vehicle.Track",
                Message = $"Track width must be between 10 and 400 cm. Current value: {settings.Track} cm.",
                InvalidValue = settings.Track,
                Constraints = new SettingConstraints
                {
                    MinValue = 10.0,
                    MaxValue = 400.0,
                    DataType = "double",
                    ValidationRule = "Track width must be within realistic range for agricultural vehicles"
                }
            });
        }

        // Validate MaxSteerAngle: 10-90 degrees
        if (settings.MaxSteerAngle < 10.0 || settings.MaxSteerAngle > 90.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Vehicle.MaxSteerAngle",
                Message = $"Maximum steer angle must be between 10 and 90 degrees. Current value: {settings.MaxSteerAngle} degrees.",
                InvalidValue = settings.MaxSteerAngle,
                Constraints = new SettingConstraints
                {
                    MinValue = 10.0,
                    MaxValue = 90.0,
                    DataType = "double",
                    ValidationRule = "Maximum steer angle must be within physically achievable steering limits"
                }
            });
        }

        // Validate MaxAngularVelocity: 10-200 degrees/sec
        if (settings.MaxAngularVelocity < 10.0 || settings.MaxAngularVelocity > 200.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Vehicle.MaxAngularVelocity",
                Message = $"Maximum angular velocity must be between 10 and 200 degrees/sec. Current value: {settings.MaxAngularVelocity} degrees/sec.",
                InvalidValue = settings.MaxAngularVelocity,
                Constraints = new SettingConstraints
                {
                    MinValue = 10.0,
                    MaxValue = 200.0,
                    DataType = "double",
                    ValidationRule = "Maximum angular velocity must be within realistic steering speed"
                }
            });
        }

        // Validate MinUturnRadius: 1-20 meters
        if (settings.MinUturnRadius < 1.0 || settings.MinUturnRadius > 20.0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                SettingPath = "Vehicle.MinUturnRadius",
                Message = $"Minimum U-turn radius must be between 1 and 20 meters. Current value: {settings.MinUturnRadius} meters.",
                InvalidValue = settings.MinUturnRadius,
                Constraints = new SettingConstraints
                {
                    MinValue = 1.0,
                    MaxValue = 20.0,
                    DataType = "double",
                    ValidationRule = "Minimum U-turn radius must be within realistic turning radius"
                }
            });
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "Wheelbase" => new SettingConstraints
            {
                MinValue = 50.0,
                MaxValue = 500.0,
                DataType = "double",
                ValidationRule = "Wheelbase must be within realistic range for agricultural vehicles (50-500 cm)"
            },
            "Track" => new SettingConstraints
            {
                MinValue = 10.0,
                MaxValue = 400.0,
                DataType = "double",
                ValidationRule = "Track width must be within realistic range for agricultural vehicles (10-400 cm)"
            },
            "MaxSteerAngle" => new SettingConstraints
            {
                MinValue = 10.0,
                MaxValue = 90.0,
                DataType = "double",
                ValidationRule = "Maximum steer angle must be within physically achievable steering limits (10-90 degrees)"
            },
            "MaxAngularVelocity" => new SettingConstraints
            {
                MinValue = 10.0,
                MaxValue = 200.0,
                DataType = "double",
                ValidationRule = "Maximum angular velocity must be within realistic steering speed (10-200 degrees/sec)"
            },
            "MinUturnRadius" => new SettingConstraints
            {
                MinValue = 1.0,
                MaxValue = 20.0,
                DataType = "double",
                ValidationRule = "Minimum U-turn radius must be within realistic turning radius (1-20 meters)"
            },
            _ => null
        };
    }
}
