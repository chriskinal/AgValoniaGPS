using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.StateManagement;
using ValidationResult = AgValoniaGPS.Models.Validation.ValidationResult;

namespace AgValoniaGPS.Services.Configuration;

/// <summary>
/// Provides centralized configuration management as the single source of truth for all application settings.
/// Supports dual-format persistence (JSON primary, XML legacy) with atomic dual-write and fallback loading.
/// Thread-safe for concurrent access across multiple services.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Raised when any setting changes.
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Loads settings from file with fallback strategy: JSON → XML → Defaults.
    /// </summary>
    /// <param name="vehicleName">Name of the vehicle profile to load</param>
    /// <returns>Result containing loaded settings and source information</returns>
    Task<SettingsLoadResult> LoadSettingsAsync(string vehicleName);

    /// <summary>
    /// Saves settings to both JSON and XML formats atomically.
    /// Both writes must succeed or both fail (rollback on partial failure).
    /// </summary>
    /// <returns>Result indicating success/failure for each format</returns>
    Task<SettingsSaveResult> SaveSettingsAsync();

    /// <summary>
    /// Gets the current vehicle settings.
    /// </summary>
    /// <returns>Current vehicle settings from in-memory cache</returns>
    VehicleSettings GetVehicleSettings();

    /// <summary>
    /// Gets the current steering settings.
    /// </summary>
    /// <returns>Current steering settings from in-memory cache</returns>
    SteeringSettings GetSteeringSettings();

    /// <summary>
    /// Gets the current tool settings.
    /// </summary>
    /// <returns>Current tool settings from in-memory cache</returns>
    ToolSettings GetToolSettings();

    /// <summary>
    /// Gets the current section control settings.
    /// </summary>
    /// <returns>Current section control settings from in-memory cache</returns>
    SectionControlSettings GetSectionControlSettings();

    /// <summary>
    /// Gets the current GPS settings.
    /// </summary>
    /// <returns>Current GPS settings from in-memory cache</returns>
    GpsSettings GetGpsSettings();

    /// <summary>
    /// Gets the current IMU settings.
    /// </summary>
    /// <returns>Current IMU settings from in-memory cache</returns>
    ImuSettings GetImuSettings();

    /// <summary>
    /// Gets the current guidance settings.
    /// </summary>
    /// <returns>Current guidance settings from in-memory cache</returns>
    GuidanceSettings GetGuidanceSettings();

    /// <summary>
    /// Gets the current work mode settings.
    /// </summary>
    /// <returns>Current work mode settings from in-memory cache</returns>
    WorkModeSettings GetWorkModeSettings();

    /// <summary>
    /// Gets the current culture settings.
    /// </summary>
    /// <returns>Current culture settings from in-memory cache</returns>
    CultureSettings GetCultureSettings();

    /// <summary>
    /// Gets the current system state settings.
    /// </summary>
    /// <returns>Current system state settings from in-memory cache</returns>
    SystemStateSettings GetSystemStateSettings();

    /// <summary>
    /// Gets the current display settings.
    /// </summary>
    /// <returns>Current display settings from in-memory cache</returns>
    DisplaySettings GetDisplaySettings();

    /// <summary>
    /// Updates vehicle settings after validation.
    /// </summary>
    /// <param name="settings">New vehicle settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateVehicleSettingsAsync(VehicleSettings settings);

    /// <summary>
    /// Updates steering settings after validation.
    /// </summary>
    /// <param name="settings">New steering settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateSteeringSettingsAsync(SteeringSettings settings);

    /// <summary>
    /// Updates tool settings after validation.
    /// </summary>
    /// <param name="settings">New tool settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateToolSettingsAsync(ToolSettings settings);

    /// <summary>
    /// Updates section control settings after validation.
    /// </summary>
    /// <param name="settings">New section control settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateSectionControlSettingsAsync(SectionControlSettings settings);

    /// <summary>
    /// Updates GPS settings after validation.
    /// </summary>
    /// <param name="settings">New GPS settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateGpsSettingsAsync(GpsSettings settings);

    /// <summary>
    /// Updates IMU settings after validation.
    /// </summary>
    /// <param name="settings">New IMU settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateImuSettingsAsync(ImuSettings settings);

    /// <summary>
    /// Updates guidance settings after validation.
    /// </summary>
    /// <param name="settings">New guidance settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateGuidanceSettingsAsync(GuidanceSettings settings);

    /// <summary>
    /// Updates work mode settings after validation.
    /// </summary>
    /// <param name="settings">New work mode settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateWorkModeSettingsAsync(WorkModeSettings settings);

    /// <summary>
    /// Updates culture settings after validation.
    /// </summary>
    /// <param name="settings">New culture settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateCultureSettingsAsync(CultureSettings settings);

    /// <summary>
    /// Updates system state settings after validation.
    /// </summary>
    /// <param name="settings">New system state settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateSystemStateSettingsAsync(SystemStateSettings settings);

    /// <summary>
    /// Updates display settings after validation.
    /// </summary>
    /// <param name="settings">New display settings</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> UpdateDisplaySettingsAsync(DisplaySettings settings);

    /// <summary>
    /// Gets all settings as a unified object.
    /// </summary>
    /// <returns>Complete application settings</returns>
    ApplicationSettings GetAllSettings();

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    Task ResetToDefaultsAsync();
}
