using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Provides comprehensive validation services for all application settings categories.
/// Includes range validation, type validation, and cross-setting dependency validation.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates vehicle settings including physical dimensions and steering limits.
    /// </summary>
    /// <param name="settings">The vehicle settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateVehicleSettings(VehicleSettings settings);

    /// <summary>
    /// Validates steering settings including PWM values and calibration parameters.
    /// </summary>
    /// <param name="settings">The steering settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateSteeringSettings(SteeringSettings settings);

    /// <summary>
    /// Validates tool settings including dimensions and configuration.
    /// </summary>
    /// <param name="settings">The tool settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateToolSettings(ToolSettings settings);

    /// <summary>
    /// Validates section control settings including section count and positions.
    /// </summary>
    /// <param name="settings">The section control settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateSectionControlSettings(SectionControlSettings settings);

    /// <summary>
    /// Validates GPS settings including update rates and quality thresholds.
    /// </summary>
    /// <param name="settings">The GPS settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateGpsSettings(GpsSettings settings);

    /// <summary>
    /// Validates IMU settings including fusion weights and filter parameters.
    /// </summary>
    /// <param name="settings">The IMU settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateImuSettings(ImuSettings settings);

    /// <summary>
    /// Validates guidance settings including look-ahead and snap distances.
    /// </summary>
    /// <param name="settings">The guidance settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateGuidanceSettings(GuidanceSettings settings);

    /// <summary>
    /// Validates work mode settings (boolean flags only).
    /// </summary>
    /// <param name="settings">The work mode settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateWorkModeSettings(WorkModeSettings settings);

    /// <summary>
    /// Validates culture settings including language codes.
    /// </summary>
    /// <param name="settings">The culture settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateCultureSettings(CultureSettings settings);

    /// <summary>
    /// Validates system state settings (runtime state).
    /// </summary>
    /// <param name="settings">The system state settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateSystemStateSettings(SystemStateSettings settings);

    /// <summary>
    /// Validates display settings including unit system and data source.
    /// </summary>
    /// <param name="settings">The display settings to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateDisplaySettings(DisplaySettings settings);

    /// <summary>
    /// Validates entire application settings including all categories and cross-setting dependencies.
    /// This method performs comprehensive validation including:
    /// - All individual category validations
    /// - Cross-setting dependency checks (e.g., tool width vs guidance, section count vs positions)
    /// - Global consistency rules
    /// </summary>
    /// <param name="settings">The complete application settings to validate</param>
    /// <returns>Validation result with any errors or warnings from all validations</returns>
    ValidationResult ValidateAllSettings(ApplicationSettings settings);

    /// <summary>
    /// Gets the validation constraints for a specific setting path.
    /// </summary>
    /// <param name="settingPath">The dot-notation path to the setting (e.g., "Vehicle.Wheelbase")</param>
    /// <returns>The constraints for the specified setting, or null if not found</returns>
    SettingConstraints? GetConstraints(string settingPath);
}
