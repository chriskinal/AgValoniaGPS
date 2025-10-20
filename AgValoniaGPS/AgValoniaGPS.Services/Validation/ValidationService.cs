using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;
using System.Collections.Generic;
using System.Linq;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Provides comprehensive validation services for all application settings categories.
/// Orchestrates individual category validators and cross-setting dependency validation.
/// Thread-safe implementation with performance target of less than 10ms for full validation.
/// </summary>
public class ValidationService : IValidationService
{
    /// <summary>
    /// Validates vehicle settings including physical dimensions and steering limits.
    /// </summary>
    public ValidationResult ValidateVehicleSettings(VehicleSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("VehicleSettings");
        }

        return VehicleSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates steering settings including PWM values and calibration parameters.
    /// </summary>
    public ValidationResult ValidateSteeringSettings(SteeringSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("SteeringSettings");
        }

        return SteeringSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates tool settings including dimensions and configuration.
    /// </summary>
    public ValidationResult ValidateToolSettings(ToolSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("ToolSettings");
        }

        return ToolSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates section control settings including section count and positions.
    /// </summary>
    public ValidationResult ValidateSectionControlSettings(SectionControlSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("SectionControlSettings");
        }

        return SectionControlSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates GPS settings including update rates and quality thresholds.
    /// </summary>
    public ValidationResult ValidateGpsSettings(GpsSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("GpsSettings");
        }

        return GpsSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates IMU settings including fusion weights and filter parameters.
    /// </summary>
    public ValidationResult ValidateImuSettings(ImuSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("ImuSettings");
        }

        return ImuSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates guidance settings including look-ahead and snap distances.
    /// </summary>
    public ValidationResult ValidateGuidanceSettings(GuidanceSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("GuidanceSettings");
        }

        return GuidanceSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates work mode settings (boolean flags only).
    /// </summary>
    public ValidationResult ValidateWorkModeSettings(WorkModeSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("WorkModeSettings");
        }

        return WorkModeSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates culture settings including language codes.
    /// </summary>
    public ValidationResult ValidateCultureSettings(CultureSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("CultureSettings");
        }

        return CultureSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates system state settings (runtime state).
    /// </summary>
    public ValidationResult ValidateSystemStateSettings(SystemStateSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("SystemStateSettings");
        }

        return SystemStateSettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates display settings including unit system and data source.
    /// </summary>
    public ValidationResult ValidateDisplaySettings(DisplaySettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("DisplaySettings");
        }

        return DisplaySettingsValidator.Validate(settings);
    }

    /// <summary>
    /// Validates entire application settings including all categories and cross-setting dependencies.
    /// Performs comprehensive validation including:
    /// - All individual category validations
    /// - Cross-setting dependency checks
    /// - Global consistency rules
    /// Performance target: Less than 10ms for complete validation.
    /// </summary>
    public ValidationResult ValidateAllSettings(ApplicationSettings settings)
    {
        if (settings == null)
        {
            return CreateNullSettingsError("ApplicationSettings");
        }

        var combinedResult = new ValidationResult { IsValid = true };
        var allErrors = new List<ValidationError>();
        var allWarnings = new List<ValidationWarning>();

        // Validate each category
        var categoryResults = new[]
        {
            ValidateVehicleSettings(settings.Vehicle),
            ValidateSteeringSettings(settings.Steering),
            ValidateToolSettings(settings.Tool),
            ValidateSectionControlSettings(settings.SectionControl),
            ValidateGpsSettings(settings.Gps),
            ValidateImuSettings(settings.Imu),
            ValidateGuidanceSettings(settings.Guidance),
            ValidateWorkModeSettings(settings.WorkMode),
            ValidateCultureSettings(settings.Culture),
            ValidateSystemStateSettings(settings.SystemState),
            ValidateDisplaySettings(settings.Display)
        };

        // Collect all errors and warnings from category validations
        foreach (var categoryResult in categoryResults)
        {
            if (!categoryResult.IsValid)
            {
                combinedResult.IsValid = false;
                allErrors.AddRange(categoryResult.Errors);
            }
            allWarnings.AddRange(categoryResult.Warnings);
        }

        // Perform cross-setting validation
        var crossSettingResult = CrossSettingValidator.Validate(settings);
        if (!crossSettingResult.IsValid)
        {
            combinedResult.IsValid = false;
            allErrors.AddRange(crossSettingResult.Errors);
        }
        allWarnings.AddRange(crossSettingResult.Warnings);

        // Assign collected errors and warnings
        combinedResult.Errors = allErrors;
        combinedResult.Warnings = allWarnings;

        return combinedResult;
    }

    /// <summary>
    /// Gets the validation constraints for a specific setting path.
    /// Path format: "Category.PropertyName" (e.g., "Vehicle.Wheelbase").
    /// </summary>
    public SettingConstraints? GetConstraints(string settingPath)
    {
        if (string.IsNullOrWhiteSpace(settingPath))
        {
            return null;
        }

        var parts = settingPath.Split('.');
        if (parts.Length != 2)
        {
            return null;
        }

        var category = parts[0];
        var propertyName = parts[1];

        return category switch
        {
            "Vehicle" => VehicleSettingsValidator.GetConstraints(propertyName),
            "Steering" => SteeringSettingsValidator.GetConstraints(propertyName),
            "Tool" => ToolSettingsValidator.GetConstraints(propertyName),
            "SectionControl" => SectionControlSettingsValidator.GetConstraints(propertyName),
            "Gps" => GpsSettingsValidator.GetConstraints(propertyName),
            "Imu" => ImuSettingsValidator.GetConstraints(propertyName),
            "Guidance" => GuidanceSettingsValidator.GetConstraints(propertyName),
            "WorkMode" => WorkModeSettingsValidator.GetConstraints(propertyName),
            "Culture" => CultureSettingsValidator.GetConstraints(propertyName),
            "SystemState" => SystemStateSettingsValidator.GetConstraints(propertyName),
            "Display" => DisplaySettingsValidator.GetConstraints(propertyName),
            _ => null
        };
    }

    private static ValidationResult CreateNullSettingsError(string settingsType)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = new List<ValidationError>
            {
                new ValidationError
                {
                    SettingPath = settingsType,
                    Message = $"{settingsType} cannot be null.",
                    InvalidValue = null,
                    Constraints = new SettingConstraints
                    {
                        DataType = "object",
                        ValidationRule = "Settings object must not be null"
                    }
                }
            }
        };
    }
}
