using Xunit;
using AgValoniaGPS.Services.Validation;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Tests.Validation;

/// <summary>
/// Focused tests for ValidationService covering critical validation scenarios.
/// Tests only essential validation paths - comprehensive validation rule testing
/// is handled by individual validator tests.
/// </summary>
public class ValidationServiceTests
{
    private readonly IValidationService _validationService;

    public ValidationServiceTests()
    {
        _validationService = new ValidationService();
    }

    [Fact]
    public void ValidateVehicleSettings_ValidSettings_ReturnsValid()
    {
        // Arrange - Valid vehicle settings within all constraints
        var settings = new VehicleSettings
        {
            Wheelbase = 180.0,
            Track = 30.0,
            MaxSteerAngle = 45.0,
            MaxAngularVelocity = 100.0,
            MinUturnRadius = 3.0
        };

        // Act
        var result = _validationService.ValidateVehicleSettings(settings);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateVehicleSettings_WheelbaseOutOfRange_ReturnsInvalid()
    {
        // Arrange - Wheelbase below minimum (50 cm)
        var settings = new VehicleSettings
        {
            Wheelbase = 10.0, // Too small
            Track = 30.0,
            MaxSteerAngle = 45.0,
            MaxAngularVelocity = 100.0,
            MinUturnRadius = 3.0
        };

        // Act
        var result = _validationService.ValidateVehicleSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.SettingPath == "Vehicle.Wheelbase");
    }

    [Fact]
    public void ValidateSteeringSettings_PwmValuesOutOfRange_ReturnsInvalid()
    {
        // Arrange - PWM value exceeds maximum (255)
        var settings = new SteeringSettings
        {
            CountsPerDegree = 100,
            Ackermann = 100,
            WasOffset = 0.0,
            HighPwm = 300, // Too high
            LowPwm = 78,
            MinPwm = 5,
            MaxPwm = 10,
            PanicStop = 0
        };

        // Act
        var result = _validationService.ValidateSteeringSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.SettingPath == "Steering.HighPwm");
    }

    [Fact]
    public void ValidateAllSettings_CrossSettingDependency_ToolWidthAffectsGuidance_ReturnsWarning()
    {
        // Arrange - LookAhead is too short relative to ToolWidth
        var settings = new ApplicationSettings
        {
            Vehicle = new VehicleSettings
            {
                Wheelbase = 180.0,
                Track = 30.0,
                MaxSteerAngle = 45.0,
                MaxAngularVelocity = 100.0,
                MinUturnRadius = 3.0
            },
            Tool = new ToolSettings
            {
                ToolWidth = 10.0 // 10 meters wide
            },
            Guidance = new GuidanceSettings
            {
                LookAhead = 2.0, // Only 2 meters - too short for 10m tool (should be at least 5m)
                AcquireFactor = 0.9,
                SpeedFactor = 1.0,
                SnapDistance = 20.0,
                RefSnapDistance = 5.0
            },
            Steering = new SteeringSettings(),
            SectionControl = new SectionControlSettings(),
            Gps = new GpsSettings(),
            Imu = new ImuSettings(),
            WorkMode = new WorkModeSettings(),
            Culture = new CultureSettings(),
            SystemState = new SystemStateSettings(),
            Display = new DisplaySettings()
        };

        // Act
        var result = _validationService.ValidateAllSettings(settings);

        // Assert
        Assert.True(result.IsValid); // Should be valid but with warnings
        Assert.Contains(result.Warnings, w => w.SettingPath.Contains("Guidance.LookAhead"));
    }

    [Fact]
    public void ValidateAllSettings_SectionCountMismatch_ReturnsInvalid()
    {
        // Arrange - SectionPositions array length doesn't match NumberSections
        var settings = new ApplicationSettings
        {
            Vehicle = new VehicleSettings
            {
                Wheelbase = 180.0,
                Track = 30.0,
                MaxSteerAngle = 45.0,
                MaxAngularVelocity = 100.0,
                MinUturnRadius = 3.0
            },
            SectionControl = new SectionControlSettings
            {
                NumberSections = 5,
                SectionPositions = new double[] { -1.0, 0.0, 1.0 }, // Only 3 positions but 5 sections declared
                LowSpeedCutoff = 1.0
            },
            Tool = new ToolSettings(),
            Guidance = new GuidanceSettings(),
            Steering = new SteeringSettings(),
            Gps = new GpsSettings(),
            Imu = new ImuSettings(),
            WorkMode = new WorkModeSettings(),
            Culture = new CultureSettings(),
            SystemState = new SystemStateSettings(),
            Display = new DisplaySettings()
        };

        // Act
        var result = _validationService.ValidateAllSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.SettingPath.Contains("SectionControl"));
    }

    [Fact]
    public void GetConstraints_VehicleWheelbase_ReturnsCorrectConstraints()
    {
        // Act
        var constraints = _validationService.GetConstraints("Vehicle.Wheelbase");

        // Assert
        Assert.NotNull(constraints);
        Assert.Equal("double", constraints.DataType);
        Assert.Equal(50.0, constraints.MinValue);
        Assert.Equal(500.0, constraints.MaxValue);
    }

    [Fact]
    public void ValidateAllSettings_PerformanceTest_CompletesUnder10ms()
    {
        // Arrange - Full valid settings
        var settings = CreateValidApplicationSettings();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _validationService.ValidateAllSettings(settings);

        stopwatch.Stop();

        // Assert
        Assert.True(result.IsValid);
        Assert.True(stopwatch.ElapsedMilliseconds < 10,
            $"Validation took {stopwatch.ElapsedMilliseconds}ms, should be <10ms");
    }

    private ApplicationSettings CreateValidApplicationSettings()
    {
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
                SectionPositions = new double[] { -0.914, 0.0, 0.914 }
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
                DeadHeadDelay = new int[] { 10, 10 },
                North = 1.15,
                East = -1.76,
                Elevation = 200.8,
                UnitSystem = UnitSystem.Metric,
                SpeedSource = "GPS"
            }
        };
    }
}
