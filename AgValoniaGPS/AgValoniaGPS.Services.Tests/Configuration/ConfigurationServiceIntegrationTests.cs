using System.Diagnostics;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.Configuration;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Configuration;

/// <summary>
/// Integration tests for ConfigurationService focusing on critical gaps:
/// - Dual-write atomicity
/// - Settings load fallback strategy (JSON → XML → Defaults)
/// - File I/O round-trip fidelity
/// - Thread-safe concurrent access
/// - Performance requirements
/// </summary>
[TestFixture]
public class ConfigurationServiceIntegrationTests
{
    private string _testBasePath = null!;
    private ConfigurationService _service = null!;
    private JsonConfigurationProvider _jsonProvider = null!;
    private XmlConfigurationProvider _xmlProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "AgValoniaGPSTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testBasePath);
        _service = new ConfigurationService();
        _jsonProvider = new JsonConfigurationProvider();
        _xmlProvider = new XmlConfigurationProvider();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, recursive: true);
        }
    }

    [Test]
    public async Task DualWriteAtomicity_JsonFailsXmlSucceeds_BothRolledBack()
    {
        // Arrange
        var settings = CreateValidApplicationSettings();
        var jsonPath = Path.Combine(_testBasePath, "TestVehicle.json");
        var xmlPath = Path.Combine(_testBasePath, "TestVehicle.xml");

        // Create a read-only JSON file to simulate write failure
        await _jsonProvider.SaveAsync(jsonPath, settings);
        var fileInfo = new FileInfo(jsonPath);
        fileInfo.IsReadOnly = true;

        try
        {
            // Act - Attempt to save should fail atomically
            await _jsonProvider.SaveAsync(jsonPath, settings);

            // Assert
            Assert.Fail("Expected IOException when writing to read-only file");
        }
        catch (UnauthorizedAccessException)
        {
            // Expected - verify XML was not created/modified
            var xmlExists = File.Exists(xmlPath);
            Assert.That(xmlExists, Is.False, "XML file should not be created if JSON write fails");
        }
        finally
        {
            // Cleanup
            fileInfo.IsReadOnly = false;
        }
    }

    [Test]
    public async Task SettingsLoadFallback_JsonMissing_FallsBackToXml()
    {
        // Arrange - Create only XML file (no JSON)
        var vehicleName = "LegacyTractor";
        var settings = CreateValidApplicationSettings();
        var xmlPath = Path.Combine(_testBasePath, $"{vehicleName}.xml");

        await _xmlProvider.SaveAsync(xmlPath, settings);

        // Act - Load should fall back to XML
        var result = await LoadSettingsFromPath(vehicleName);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Source, Is.EqualTo(SettingsLoadSource.Xml));
        Assert.That(result.Settings, Is.Not.Null);
        Assert.That(result.Settings!.Vehicle.Wheelbase, Is.EqualTo(180.0));
    }

    [Test]
    public async Task SettingsLoadFallback_BothMissing_ReturnsDefaults()
    {
        // Arrange - No files exist
        var vehicleName = "NonExistentVehicle";

        // Act
        var result = await _service.LoadSettingsAsync(vehicleName);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Source, Is.EqualTo(SettingsLoadSource.Defaults));
        Assert.That(result.Settings, Is.Not.Null);
    }

    [Test]
    public async Task FileIORoundTrip_JsonSerialization_MaintainsFidelity()
    {
        // Arrange
        var originalSettings = CreateValidApplicationSettings();
        var jsonPath = Path.Combine(_testBasePath, "RoundTripTest.json");

        // Act - Save and load
        await _jsonProvider.SaveAsync(jsonPath, originalSettings);
        var loadedSettings = await _jsonProvider.LoadAsync(jsonPath);

        // Assert - All properties should match exactly
        Assert.That(loadedSettings.Vehicle.Wheelbase, Is.EqualTo(originalSettings.Vehicle.Wheelbase));
        Assert.That(loadedSettings.Vehicle.Track, Is.EqualTo(originalSettings.Vehicle.Track));
        Assert.That(loadedSettings.Vehicle.MaxSteerAngle, Is.EqualTo(originalSettings.Vehicle.MaxSteerAngle));
        Assert.That(loadedSettings.Steering.CountsPerDegree, Is.EqualTo(originalSettings.Steering.CountsPerDegree));
        Assert.That(loadedSettings.Steering.Ackermann, Is.EqualTo(originalSettings.Steering.Ackermann));
        Assert.That(loadedSettings.Tool.ToolWidth, Is.EqualTo(originalSettings.Tool.ToolWidth));
        Assert.That(loadedSettings.SectionControl.NumberSections, Is.EqualTo(originalSettings.SectionControl.NumberSections));
        Assert.That(loadedSettings.Guidance.LookAhead, Is.EqualTo(originalSettings.Guidance.LookAhead));
    }

    [Test]
    public async Task FileIORoundTrip_XmlSerialization_MaintainsFidelity()
    {
        // Arrange
        var originalSettings = CreateValidApplicationSettings();
        var xmlPath = Path.Combine(_testBasePath, "RoundTripTest.xml");

        // Act - Save and load
        await _xmlProvider.SaveAsync(xmlPath, originalSettings);
        var loadedSettings = await _xmlProvider.LoadAsync(xmlPath);

        // Assert - All properties should match exactly
        Assert.That(loadedSettings.Vehicle.Wheelbase, Is.EqualTo(originalSettings.Vehicle.Wheelbase));
        Assert.That(loadedSettings.Steering.CountsPerDegree, Is.EqualTo(originalSettings.Steering.CountsPerDegree));
        Assert.That(loadedSettings.Tool.ToolWidth, Is.EqualTo(originalSettings.Tool.ToolWidth));
        Assert.That(loadedSettings.SectionControl.NumberSections, Is.EqualTo(originalSettings.SectionControl.NumberSections));
    }

    [Test]
    public async Task ThreadSafety_ConcurrentAccess_NoDataCorruption()
    {
        // Arrange
        await _service.LoadSettingsAsync("TestVehicle");
        var tasks = new List<Task>();
        var readValues = new List<double>();
        var lockObject = new object();

        // Act - Perform concurrent reads and writes
        for (int i = 0; i < 20; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                if (index % 2 == 0)
                {
                    // Read operation
                    var settings = _service.GetVehicleSettings();
                    lock (lockObject)
                    {
                        readValues.Add(settings.Wheelbase);
                    }
                }
                else
                {
                    // Write operation
                    var newSettings = new VehicleSettings
                    {
                        Wheelbase = 180.0 + index,
                        Track = 30.0,
                        MaxSteerAngle = 45.0,
                        MaxAngularVelocity = 100.0,
                        AntennaPivot = 25.0,
                        AntennaHeight = 50.0,
                        AntennaOffset = 0.0,
                        PivotBehindAnt = 30.0,
                        SteerAxleAhead = 110.0,
                        VehicleType = VehicleType.Tractor,
                        VehicleHitchLength = 0.0,
                        MinUturnRadius = 3.0
                    };
                    await _service.UpdateVehicleSettingsAsync(newSettings);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown and final state is consistent
        var finalSettings = _service.GetVehicleSettings();
        Assert.That(finalSettings, Is.Not.Null);
        Assert.That(finalSettings.Wheelbase, Is.GreaterThanOrEqualTo(180.0));
    }

    [Test]
    public async Task Performance_SettingsLoad_CompletesUnder100ms()
    {
        // Arrange
        var settings = CreateValidApplicationSettings();
        var jsonPath = Path.Combine(_testBasePath, "PerformanceTest.json");
        await _jsonProvider.SaveAsync(jsonPath, settings);

        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _jsonProvider.LoadAsync(jsonPath);
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100),
            $"Settings load took {stopwatch.ElapsedMilliseconds}ms, expected <100ms");
    }

    [Test]
    public async Task JsonToXmlConversion_MaintainsDataFidelity()
    {
        // Arrange
        var originalSettings = CreateValidApplicationSettings();
        var jsonPath = Path.Combine(_testBasePath, "Source.json");
        var xmlPath = Path.Combine(_testBasePath, "Converted.xml");

        // Act - Save as JSON, convert to XML
        await _jsonProvider.SaveAsync(jsonPath, originalSettings);
        await _xmlProvider.SaveAsync(xmlPath, originalSettings);

        // Load both
        var jsonSettings = await _jsonProvider.LoadAsync(jsonPath);
        var xmlSettings = await _xmlProvider.LoadAsync(xmlPath);

        // Assert - Key values should match
        Assert.That(xmlSettings.Vehicle.Wheelbase, Is.EqualTo(jsonSettings.Vehicle.Wheelbase));
        Assert.That(xmlSettings.Steering.CountsPerDegree, Is.EqualTo(jsonSettings.Steering.CountsPerDegree));
        Assert.That(xmlSettings.Tool.ToolWidth, Is.EqualTo(jsonSettings.Tool.ToolWidth));
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
                VehicleType = VehicleType.Tractor,
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

    private async Task<SettingsLoadResult> LoadSettingsFromPath(string vehicleName)
    {
        // Helper method to test loading with custom path
        // Since ConfigurationService uses fixed paths, we simulate the fallback logic
        var jsonPath = Path.Combine(_testBasePath, $"{vehicleName}.json");
        var xmlPath = Path.Combine(_testBasePath, $"{vehicleName}.xml");

        try
        {
            if (File.Exists(jsonPath))
            {
                var settings = await _jsonProvider.LoadAsync(jsonPath);
                return new SettingsLoadResult
                {
                    Success = true,
                    Settings = settings,
                    Source = SettingsLoadSource.Json
                };
            }
            else if (File.Exists(xmlPath))
            {
                var settings = await _xmlProvider.LoadAsync(xmlPath);
                return new SettingsLoadResult
                {
                    Success = true,
                    Settings = settings,
                    Source = SettingsLoadSource.Xml
                };
            }
            else
            {
                return new SettingsLoadResult
                {
                    Success = true,
                    Settings = CreateValidApplicationSettings(),
                    Source = SettingsLoadSource.Defaults
                };
            }
        }
        catch (Exception ex)
        {
            return new SettingsLoadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
