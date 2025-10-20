using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Setup;

/// <summary>
/// Provides sensible default values for all application settings categories.
/// Used for first-time setup, wizard defaults, and fallback values.
/// Values based on spec.md JSON example (lines 775-886).
/// </summary>
public static class DefaultSettingsProvider
{
    /// <summary>
    /// Gets complete default application settings with sensible values for all categories.
    /// </summary>
    public static ApplicationSettings GetDefaultSettings()
    {
        return new ApplicationSettings
        {
            Vehicle = GetDefaultVehicleSettings(),
            Steering = GetDefaultSteeringSettings(),
            Tool = GetDefaultToolSettings(),
            SectionControl = GetDefaultSectionControlSettings(),
            Gps = GetDefaultGpsSettings(),
            Imu = GetDefaultImuSettings(),
            Guidance = GetDefaultGuidanceSettings(),
            WorkMode = GetDefaultWorkModeSettings(),
            Culture = GetDefaultCultureSettings(),
            SystemState = GetDefaultSystemStateSettings(),
            Display = GetDefaultDisplaySettings()
        };
    }

    /// <summary>
    /// Gets default vehicle settings (medium tractor configuration).
    /// </summary>
    public static VehicleSettings GetDefaultVehicleSettings()
    {
        return new VehicleSettings
        {
            Wheelbase = 180.0, // cm
            Track = 30.0, // cm
            MaxSteerAngle = 45.0, // degrees
            MaxAngularVelocity = 100.0, // degrees/sec
            AntennaPivot = 25.0, // cm
            AntennaHeight = 50.0, // cm
            AntennaOffset = 0.0, // cm
            PivotBehindAnt = 30.0, // cm
            SteerAxleAhead = 110.0, // cm
            VehicleType = 0, // Tractor
            VehicleHitchLength = 0.0, // cm
            MinUturnRadius = 3.0 // meters
        };
    }

    /// <summary>
    /// Gets default steering settings (typical PWM and calibration values).
    /// </summary>
    public static SteeringSettings GetDefaultSteeringSettings()
    {
        return new SteeringSettings
        {
            CountsPerDegree = 100,
            Ackermann = 100,
            WasOffset = 0.04,
            HighPwm = 235,
            LowPwm = 78,
            MinPwm = 5,
            MaxPwm = 10,
            PanicStop = 0
        };
    }

    /// <summary>
    /// Gets default tool settings (6-foot implement).
    /// </summary>
    public static ToolSettings GetDefaultToolSettings()
    {
        return new ToolSettings
        {
            ToolWidth = 1.828, // meters (6 feet)
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
        };
    }

    /// <summary>
    /// Gets default section control settings (3 sections).
    /// </summary>
    public static SectionControlSettings GetDefaultSectionControlSettings()
    {
        return new SectionControlSettings
        {
            NumberSections = 3,
            HeadlandSecControl = false,
            FastSections = true,
            SectionOffOutBounds = true,
            SectionsNotZones = true,
            LowSpeedCutoff = 1.0, // m/s
            SectionPositions = new[] { -0.914, 0.0, 0.914 } // Even spacing for 3 sections
        };
    }

    /// <summary>
    /// Gets default GPS settings (10Hz dual antenna).
    /// </summary>
    public static GpsSettings GetDefaultGpsSettings()
    {
        return new GpsSettings
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
        };
    }

    /// <summary>
    /// Gets default IMU settings (dual antenna as IMU disabled).
    /// </summary>
    public static ImuSettings GetDefaultImuSettings()
    {
        return new ImuSettings
        {
            DualAsImu = false,
            DualHeadingOffset = 90,
            ImuFusionWeight = 0.06,
            MinHeadingStep = 0.5,
            MinStepLimit = 0.05,
            RollZero = 0.0,
            InvertRoll = false,
            RollFilter = 0.15
        };
    }

    /// <summary>
    /// Gets default guidance settings (3m look-ahead).
    /// </summary>
    public static GuidanceSettings GetDefaultGuidanceSettings()
    {
        return new GuidanceSettings
        {
            AcquireFactor = 0.90,
            LookAhead = 3.0, // meters
            SpeedFactor = 1.0,
            PurePursuitIntegral = 0.0,
            SnapDistance = 20.0, // meters
            RefSnapDistance = 5.0, // meters
            SideHillComp = 0.0
        };
    }

    /// <summary>
    /// Gets default work mode settings (manual control).
    /// </summary>
    public static WorkModeSettings GetDefaultWorkModeSettings()
    {
        return new WorkModeSettings
        {
            RemoteWork = false,
            SteerWorkSwitch = false,
            SteerWorkManual = true,
            WorkActiveLow = false,
            WorkSwitch = false,
            WorkManualSection = true
        };
    }

    /// <summary>
    /// Gets default culture settings (English).
    /// </summary>
    public static CultureSettings GetDefaultCultureSettings()
    {
        return new CultureSettings
        {
            CultureCode = "en",
            LanguageName = "English"
        };
    }

    /// <summary>
    /// Gets default system state settings (runtime defaults).
    /// </summary>
    public static SystemStateSettings GetDefaultSystemStateSettings()
    {
        return new SystemStateSettings
        {
            StanleyUsed = false,
            SteerInReverse = false,
            ReverseOn = true,
            Heading = 20.6,
            ImuHeading = 0.0,
            HeadingError = 1,
            DistanceError = 1,
            SteerIntegral = 0
        };
    }

    /// <summary>
    /// Gets default display settings (metric, GPS speed source).
    /// </summary>
    public static DisplaySettings GetDefaultDisplaySettings()
    {
        return new DisplaySettings
        {
            DeadHeadDelay = new[] { 10, 10 },
            North = 1.15,
            East = -1.76,
            Elevation = 200.8,
            UnitSystem = UnitSystem.Metric,
            SpeedSource = "GPS"
        };
    }

    /// <summary>
    /// Gets default user preferences (metric, English, rotating display enabled).
    /// </summary>
    public static UserPreferences GetDefaultUserPreferences()
    {
        return new UserPreferences
        {
            PreferredUnitSystem = UnitSystem.Metric,
            PreferredLanguage = "en",
            DisplayPreferences = new DisplayPreferences
            {
                ShowSatelliteCount = true,
                ShowSpeedGauge = true,
                RotatingDisplayEnabled = true,
                RotatingDisplayInterval = 5
            },
            LastUsedVehicleProfile = string.Empty
        };
    }
}
