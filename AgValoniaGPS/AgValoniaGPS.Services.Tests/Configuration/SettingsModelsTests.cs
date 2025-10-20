using System;
using System.Text.Json;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.StateManagement;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Configuration
{
    /// <summary>
    /// Focused tests for settings model classes covering critical behaviors:
    /// - Property assignment and default values
    /// - JSON serialization and deserialization
    /// - Settings change event args functionality
    /// </summary>
    public class SettingsModelsTests
    {
        [Fact]
        public void ApplicationSettings_DefaultConstruction_AllCategoriesInitialized()
        {
            // Arrange & Act
            var settings = new ApplicationSettings();

            // Assert - Verify all nested settings objects are initialized
            Assert.NotNull(settings.Vehicle);
            Assert.NotNull(settings.Steering);
            Assert.NotNull(settings.Tool);
            Assert.NotNull(settings.SectionControl);
            Assert.NotNull(settings.Gps);
            Assert.NotNull(settings.Imu);
            Assert.NotNull(settings.Guidance);
            Assert.NotNull(settings.WorkMode);
            Assert.NotNull(settings.Culture);
            Assert.NotNull(settings.SystemState);
            Assert.NotNull(settings.Display);
        }

        [Fact]
        public void VehicleSettings_DefaultValues_MatchSpecification()
        {
            // Arrange & Act
            var settings = new VehicleSettings();

            // Assert - Verify critical default values from spec
            Assert.Equal(180.0, settings.Wheelbase);
            Assert.Equal(30.0, settings.Track);
            Assert.Equal(45.0, settings.MaxSteerAngle);
            Assert.Equal(100.0, settings.MaxAngularVelocity);
            Assert.Equal(3.0, settings.MinUturnRadius);
            Assert.Equal(VehicleType.Tractor, settings.VehicleType);
        }

        [Fact]
        public void VehicleSettings_PropertyAssignment_ValuesUpdated()
        {
            // Arrange
            var settings = new VehicleSettings();

            // Act
            settings.Wheelbase = 250.0;
            settings.Track = 50.0;
            settings.MaxSteerAngle = 40.0;
            settings.VehicleType = VehicleType.Sprayer;

            // Assert
            Assert.Equal(250.0, settings.Wheelbase);
            Assert.Equal(50.0, settings.Track);
            Assert.Equal(40.0, settings.MaxSteerAngle);
            Assert.Equal(VehicleType.Sprayer, settings.VehicleType);
        }

        [Fact]
        public void SteeringSettings_DefaultValues_MatchSpecification()
        {
            // Arrange & Act
            var settings = new SteeringSettings();

            // Assert - Verify critical default values
            Assert.Equal(100, settings.CountsPerDegree);
            Assert.Equal(100, settings.Ackermann);
            Assert.Equal(0.04, settings.WasOffset);
            Assert.Equal(235, settings.HighPwm);
            Assert.Equal(78, settings.LowPwm);
        }

        [Fact]
        public void SectionControlSettings_ArrayInitialization_DefaultSectionPositions()
        {
            // Arrange & Act
            var settings = new SectionControlSettings();

            // Assert - Verify default section positions array
            Assert.NotNull(settings.SectionPositions);
            Assert.Equal(3, settings.SectionPositions.Length);
            Assert.Equal(-0.914, settings.SectionPositions[0]);
            Assert.Equal(0.0, settings.SectionPositions[1]);
            Assert.Equal(0.914, settings.SectionPositions[2]);
            Assert.Equal(3, settings.NumberSections);
        }

        [Fact]
        public void ApplicationSettings_JsonSerialization_RoundTripPreservesData()
        {
            // Arrange
            var original = new ApplicationSettings
            {
                Vehicle = new VehicleSettings
                {
                    Wheelbase = 200.0,
                    Track = 40.0,
                    MaxSteerAngle = 50.0
                },
                Steering = new SteeringSettings
                {
                    CountsPerDegree = 120,
                    HighPwm = 240
                },
                Gps = new GpsSettings
                {
                    Hz = 10.0,
                    HeadingFrom = "Dual"
                },
                Culture = new CultureSettings
                {
                    CultureCode = "en",
                    LanguageName = "English"
                }
            };

            // Act - Serialize to JSON and back
            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<ApplicationSettings>(json);

            // Assert - Verify critical values preserved
            Assert.NotNull(deserialized);
            Assert.Equal(200.0, deserialized.Vehicle.Wheelbase);
            Assert.Equal(40.0, deserialized.Vehicle.Track);
            Assert.Equal(120, deserialized.Steering.CountsPerDegree);
            Assert.Equal(240, deserialized.Steering.HighPwm);
            Assert.Equal(10.0, deserialized.Gps.Hz);
            Assert.Equal("Dual", deserialized.Gps.HeadingFrom);
            Assert.Equal("en", deserialized.Culture.CultureCode);
        }

        [Fact]
        public void DisplaySettings_UnitSystemEnum_CorrectlyAssigned()
        {
            // Arrange
            var settings = new DisplaySettings();

            // Act & Assert - Verify default is Metric
            Assert.Equal(UnitSystem.Metric, settings.UnitSystem);

            // Act - Change to Imperial
            settings.UnitSystem = UnitSystem.Imperial;

            // Assert
            Assert.Equal(UnitSystem.Imperial, settings.UnitSystem);
        }

        [Fact]
        public void SettingsChangedEventArgs_PropertiesCorrectlySet_TimestampGenerated()
        {
            // Arrange
            var oldValue = new VehicleSettings { Wheelbase = 180.0 };
            var newValue = new VehicleSettings { Wheelbase = 200.0 };
            var beforeTimestamp = DateTime.UtcNow;

            // Act
            var eventArgs = new SettingsChangedEventArgs
            {
                Category = SettingsCategory.Vehicle,
                OldValue = oldValue,
                NewValue = newValue
            };

            var afterTimestamp = DateTime.UtcNow;

            // Assert
            Assert.Equal(SettingsCategory.Vehicle, eventArgs.Category);
            Assert.Same(oldValue, eventArgs.OldValue);
            Assert.Same(newValue, eventArgs.NewValue);
            Assert.True(eventArgs.Timestamp >= beforeTimestamp);
            Assert.True(eventArgs.Timestamp <= afterTimestamp);
        }
    }
}
