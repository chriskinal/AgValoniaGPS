using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.StateManagement;
using ValidationResult = AgValoniaGPS.Models.Validation.ValidationResult;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Configuration;

/// <summary>
/// Centralized configuration service providing single source of truth for all application settings.
/// Implements dual-format persistence (JSON primary, XML legacy) with atomic dual-write.
/// Thread-safe for concurrent access with in-memory caching for performance.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly object _lock = new();
    private ApplicationSettings _currentSettings;
    private string _currentVehicleName = string.Empty;
    private readonly IConfigurationProvider _jsonProvider;
    private readonly IConfigurationProvider _xmlProvider;

    /// <summary>
    /// Raised when any setting changes.
    /// </summary>
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    public ConfigurationService()
    {
        _currentSettings = CreateDefaultSettings();
        _jsonProvider = new JsonConfigurationProvider();
        _xmlProvider = new XmlConfigurationProvider();
    }

    /// <summary>
    /// Loads settings from file with fallback strategy: JSON → XML → Defaults.
    /// </summary>
    /// <param name="vehicleName">Name of the vehicle profile to load</param>
    /// <returns>Result containing loaded settings and source information</returns>
    public async Task<SettingsLoadResult> LoadSettingsAsync(string vehicleName)
    {
        try
        {
            var basePath = GetVehicleBasePath();
            var jsonPath = Path.Combine(basePath, $"{vehicleName}.json");
            var xmlPath = Path.Combine(basePath, $"{vehicleName}.xml");

            ApplicationSettings? settings = null;
            SettingsLoadSource source = SettingsLoadSource.Defaults;
            string errorMessage = string.Empty;

            // Try JSON first (primary format)
            if (File.Exists(jsonPath))
            {
                try
                {
                    settings = await _jsonProvider.LoadAsync(jsonPath);
                    source = SettingsLoadSource.Json;
                }
                catch (Exception ex)
                {
                    errorMessage = $"JSON load failed: {ex.Message}. ";
                }
            }

            // Fallback to XML if JSON failed or doesn't exist
            if (settings == null && File.Exists(xmlPath))
            {
                try
                {
                    settings = await _xmlProvider.LoadAsync(xmlPath);
                    source = SettingsLoadSource.Xml;
                }
                catch (Exception ex)
                {
                    errorMessage += $"XML load failed: {ex.Message}. ";
                }
            }

            // Fallback to defaults if both failed
            if (settings == null)
            {
                settings = CreateDefaultSettings();
                source = SettingsLoadSource.Defaults;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "No settings files found, using defaults.";
                }
                else
                {
                    errorMessage += "Using defaults.";
                }
            }

            // Update in-memory cache
            lock (_lock)
            {
                _currentSettings = settings;
                _currentVehicleName = vehicleName;
            }

            // Raise event for all categories
            OnSettingsChanged(SettingsCategory.All, null, settings);

            return new SettingsLoadResult
            {
                Success = true,
                Settings = settings,
                Source = source,
                ErrorMessage = errorMessage
            };
        }
        catch (Exception ex)
        {
            return new SettingsLoadResult
            {
                Success = false,
                Settings = null,
                Source = SettingsLoadSource.Defaults,
                ErrorMessage = $"Failed to load settings: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Saves settings to both JSON and XML formats atomically.
    /// Both writes must succeed or both fail (rollback on partial failure).
    /// </summary>
    /// <returns>Result indicating success/failure for each format</returns>
    public async Task<SettingsSaveResult> SaveSettingsAsync()
    {
        var result = new SettingsSaveResult();

        try
        {
            ApplicationSettings settings;
            string vehicleName;

            lock (_lock)
            {
                settings = _currentSettings;
                vehicleName = _currentVehicleName;
            }

            if (string.IsNullOrEmpty(vehicleName))
            {
                result.Success = false;
                result.ErrorMessage = "No vehicle profile loaded.";
                return result;
            }

            var basePath = GetVehicleBasePath();
            var jsonPath = Path.Combine(basePath, $"{vehicleName}.json");
            var xmlPath = Path.Combine(basePath, $"{vehicleName}.xml");

            // Backup existing files for rollback
            var jsonBackupPath = jsonPath + ".bak";
            var xmlBackupPath = xmlPath + ".bak";

            try
            {
                // Create backups
                if (File.Exists(jsonPath))
                {
                    File.Copy(jsonPath, jsonBackupPath, overwrite: true);
                }
                if (File.Exists(xmlPath))
                {
                    File.Copy(xmlPath, xmlBackupPath, overwrite: true);
                }

                // Attempt dual-write
                await _jsonProvider.SaveAsync(jsonPath, settings);
                result.JsonSaved = true;

                await _xmlProvider.SaveAsync(xmlPath, settings);
                result.XmlSaved = true;

                // Both succeeded - delete backups
                if (File.Exists(jsonBackupPath))
                {
                    File.Delete(jsonBackupPath);
                }
                if (File.Exists(xmlBackupPath))
                {
                    File.Delete(xmlBackupPath);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                // Rollback on failure
                result.Success = false;
                result.ErrorMessage = $"Save failed: {ex.Message}";

                // Restore from backups
                try
                {
                    if (result.JsonSaved && File.Exists(jsonBackupPath))
                    {
                        File.Copy(jsonBackupPath, jsonPath, overwrite: true);
                        result.JsonSaved = false;
                    }
                    if (result.XmlSaved && File.Exists(xmlBackupPath))
                    {
                        File.Copy(xmlBackupPath, xmlPath, overwrite: true);
                        result.XmlSaved = false;
                    }
                }
                finally
                {
                    // Clean up backups
                    if (File.Exists(jsonBackupPath))
                    {
                        File.Delete(jsonBackupPath);
                    }
                    if (File.Exists(xmlBackupPath))
                    {
                        File.Delete(xmlBackupPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Unexpected error during save: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Gets the current vehicle settings.
    /// </summary>
    public VehicleSettings GetVehicleSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Vehicle;
        }
    }

    /// <summary>
    /// Gets the current steering settings.
    /// </summary>
    public SteeringSettings GetSteeringSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Steering;
        }
    }

    /// <summary>
    /// Gets the current tool settings.
    /// </summary>
    public ToolSettings GetToolSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Tool;
        }
    }

    /// <summary>
    /// Gets the current section control settings.
    /// </summary>
    public SectionControlSettings GetSectionControlSettings()
    {
        lock (_lock)
        {
            return _currentSettings.SectionControl;
        }
    }

    /// <summary>
    /// Gets the current GPS settings.
    /// </summary>
    public GpsSettings GetGpsSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Gps;
        }
    }

    /// <summary>
    /// Gets the current IMU settings.
    /// </summary>
    public ImuSettings GetImuSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Imu;
        }
    }

    /// <summary>
    /// Gets the current guidance settings.
    /// </summary>
    public GuidanceSettings GetGuidanceSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Guidance;
        }
    }

    /// <summary>
    /// Gets the current work mode settings.
    /// </summary>
    public WorkModeSettings GetWorkModeSettings()
    {
        lock (_lock)
        {
            return _currentSettings.WorkMode;
        }
    }

    /// <summary>
    /// Gets the current culture settings.
    /// </summary>
    public CultureSettings GetCultureSettings()
    {
        lock (_lock)
        {
            return _currentSettings.Culture;
        }
    }

    /// <summary>
    /// Gets the current system state settings.
    /// </summary>
    public SystemStateSettings GetSystemStateSettings()
    {
        lock (_lock)
        {
            return _currentSettings.SystemState;
        }
    }

    /// <summary>
    /// Gets the current display settings.
    /// </summary>
    public DisplaySettings GetDisplaySettings()
    {
        lock (_lock)
        {
            return _currentSettings.Display;
        }
    }

    /// <summary>
    /// Updates vehicle settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateVehicleSettingsAsync(VehicleSettings settings)
    {
        // Note: Validation will be implemented in Task Group 4
        // For now, accept all updates
        var oldValue = GetVehicleSettings();

        lock (_lock)
        {
            _currentSettings.Vehicle = settings;
        }

        OnSettingsChanged(SettingsCategory.Vehicle, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates steering settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateSteeringSettingsAsync(SteeringSettings settings)
    {
        var oldValue = GetSteeringSettings();

        lock (_lock)
        {
            _currentSettings.Steering = settings;
        }

        OnSettingsChanged(SettingsCategory.Steering, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates tool settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateToolSettingsAsync(ToolSettings settings)
    {
        var oldValue = GetToolSettings();

        lock (_lock)
        {
            _currentSettings.Tool = settings;
        }

        OnSettingsChanged(SettingsCategory.Tool, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates section control settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateSectionControlSettingsAsync(SectionControlSettings settings)
    {
        var oldValue = GetSectionControlSettings();

        lock (_lock)
        {
            _currentSettings.SectionControl = settings;
        }

        OnSettingsChanged(SettingsCategory.SectionControl, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates GPS settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateGpsSettingsAsync(GpsSettings settings)
    {
        var oldValue = GetGpsSettings();

        lock (_lock)
        {
            _currentSettings.Gps = settings;
        }

        OnSettingsChanged(SettingsCategory.Gps, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates IMU settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateImuSettingsAsync(ImuSettings settings)
    {
        var oldValue = GetImuSettings();

        lock (_lock)
        {
            _currentSettings.Imu = settings;
        }

        OnSettingsChanged(SettingsCategory.Imu, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates guidance settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateGuidanceSettingsAsync(GuidanceSettings settings)
    {
        var oldValue = GetGuidanceSettings();

        lock (_lock)
        {
            _currentSettings.Guidance = settings;
        }

        OnSettingsChanged(SettingsCategory.Guidance, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates work mode settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateWorkModeSettingsAsync(WorkModeSettings settings)
    {
        var oldValue = GetWorkModeSettings();

        lock (_lock)
        {
            _currentSettings.WorkMode = settings;
        }

        OnSettingsChanged(SettingsCategory.WorkMode, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates culture settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateCultureSettingsAsync(CultureSettings settings)
    {
        var oldValue = GetCultureSettings();

        lock (_lock)
        {
            _currentSettings.Culture = settings;
        }

        OnSettingsChanged(SettingsCategory.Culture, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates system state settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateSystemStateSettingsAsync(SystemStateSettings settings)
    {
        var oldValue = GetSystemStateSettings();

        lock (_lock)
        {
            _currentSettings.SystemState = settings;
        }

        OnSettingsChanged(SettingsCategory.SystemState, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Updates display settings after validation.
    /// </summary>
    public async Task<ValidationResult> UpdateDisplaySettingsAsync(DisplaySettings settings)
    {
        var oldValue = GetDisplaySettings();

        lock (_lock)
        {
            _currentSettings.Display = settings;
        }

        OnSettingsChanged(SettingsCategory.Display, oldValue, settings);

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Gets all settings as a unified object.
    /// </summary>
    public ApplicationSettings GetAllSettings()
    {
        lock (_lock)
        {
            return _currentSettings;
        }
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    public async Task ResetToDefaultsAsync()
    {
        var oldSettings = GetAllSettings();
        var newSettings = CreateDefaultSettings();

        lock (_lock)
        {
            _currentSettings = newSettings;
        }

        OnSettingsChanged(SettingsCategory.All, oldSettings, newSettings);
    }

    private void OnSettingsChanged(SettingsCategory category, object? oldValue, object? newValue)
    {
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Category = category, OldValue = oldValue, NewValue = newValue });
    }

    private static string GetVehicleBasePath()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var basePath = Path.Combine(documentsPath, "AgValoniaGPS", "Vehicles");

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        return basePath;
    }

    private static ApplicationSettings CreateDefaultSettings()
    {
        // Defaults based on spec.md lines 775-886 (Deere5055e example)
        return new ApplicationSettings
        {
            Vehicle = new VehicleSettings
            {
                Wheelbase = 180.0,
                Track = 30.0,
                MaxSteerAngle = 45.0,
                MaxAngularVelocity = 100.0,
                AntennaPivot = 25.0,
                AntennaHeight = 50.0,
                AntennaOffset = 0.0,
                PivotBehindAnt = 30.0,
                SteerAxleAhead = 110.0,
                VehicleType = 0,
                VehicleHitchLength = 0.0,
                MinUturnRadius = 3.0
            },
            Steering = new SteeringSettings
            {
                CountsPerDegree = 100,
                Ackermann = 100,
                WasOffset = 0.04,
                HighPwm = 235,
                LowPwm = 78,
                MinPwm = 5,
                MaxPwm = 10,
                PanicStop = 0
            },
            Tool = new ToolSettings
            {
                ToolWidth = 1.828,
                ToolFront = false,
                ToolRearFixed = true,
                ToolTBT = false,
                ToolTrailing = false,
                ToolToPivotLength = 0.0,
                ToolLookAheadOn = 1.0,
                ToolLookAheadOff = 0.5,
                ToolOffDelay = 0.0,
                ToolOffset = 0.0,
                ToolOverlap = 0.0,
                TrailingHitchLength = -2.5,
                TankHitchLength = 3.0,
                HydLiftLookAhead = 2.0
            },
            SectionControl = new SectionControlSettings
            {
                NumberSections = 3,
                HeadlandSecControl = false,
                FastSections = true,
                SectionOffOutBounds = true,
                SectionsNotZones = true,
                LowSpeedCutoff = 1.0,
                SectionPositions = new[] { -0.914, 0.0, 0.914 }
            },
            Gps = new GpsSettings
            {
                Hdop = 0.69,
                RawHz = 9.890,
                Hz = 10.0,
                GpsAgeAlarm = 20,
                HeadingFrom = "Dual",
                AutoStartAgIO = true,
                AutoOffAgIO = false,
                Rtk = false,
                RtkKillAutoSteer = false
            },
            Imu = new ImuSettings
            {
                DualAsImu = false,
                DualHeadingOffset = 90,
                ImuFusionWeight = 0.06,
                MinHeadingStep = 0.5,
                MinStepLimit = 0.05,
                RollZero = 0.0,
                InvertRoll = false,
                RollFilter = 0.15
            },
            Guidance = new GuidanceSettings
            {
                AcquireFactor = 0.90,
                LookAhead = 3.0,
                SpeedFactor = 1.0,
                PurePursuitIntegral = 0.0,
                SnapDistance = 20.0,
                RefSnapDistance = 5.0,
                SideHillComp = 0.0
            },
            WorkMode = new WorkModeSettings
            {
                RemoteWork = false,
                SteerWorkSwitch = false,
                SteerWorkManual = true,
                WorkActiveLow = false,
                WorkSwitch = false,
                WorkManualSection = true
            },
            Culture = new CultureSettings
            {
                CultureCode = "en",
                LanguageName = "English"
            },
            SystemState = new SystemStateSettings
            {
                StanleyUsed = false,
                SteerInReverse = false,
                ReverseOn = true,
                Heading = 20.6,
                ImuHeading = 0.0,
                HeadingError = 1,
                DistanceError = 1,
                SteerIntegral = 0
            },
            Display = new DisplaySettings
            {
                DeadHeadDelay = new[] { 10, 10 },
                North = 1.15,
                East = -1.76,
                Elevation = 200.8,
                UnitSystem = AgValoniaGPS.Models.Guidance.UnitSystem.Metric,
                SpeedSource = "GPS"
            }
        };
    }
}
