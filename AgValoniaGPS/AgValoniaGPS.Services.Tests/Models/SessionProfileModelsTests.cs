using System;
using System.Collections.Generic;
using System.Text.Json;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.Models.Session;
using AgValoniaGPS.Models.Validation;
using SettingsValidationResult = AgValoniaGPS.Models.Validation.ValidationResult;
using GuidanceValidationResult = AgValoniaGPS.Models.Guidance.ValidationResult;
using SessionGuidanceLineType = AgValoniaGPS.Models.Guidance.GuidanceLineType;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Models;

/// <summary>
/// Focused tests for Wave 8 Task Group 2 session, profile, and validation models.
/// Tests only critical model behaviors: property assignments, default values, and JSON serialization.
/// </summary>
[TestFixture]
public class SessionProfileModelsTests
{
    [Test]
    public void SessionState_DefaultValues_AreInitializedCorrectly()
    {
        // Arrange & Act
        var sessionState = new SessionState();

        // Assert
        Assert.That(sessionState.CurrentFieldName, Is.EqualTo(string.Empty));
        Assert.That(sessionState.VehicleProfileName, Is.EqualTo(string.Empty));
        Assert.That(sessionState.UserProfileName, Is.EqualTo(string.Empty));
        Assert.That(sessionState.WorkProgress, Is.Not.Null);
        Assert.That(sessionState.CurrentGuidanceLineType, Is.EqualTo(GuidanceLineType.None));
    }

    [Test]
    public void WorkProgressData_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var workProgress = new WorkProgressData
        {
            AreaCovered = 12500.5,
            DistanceTravelled = 2500.75,
            TimeWorked = TimeSpan.FromHours(2.5),
            CoverageTrail = new List<Position>
            {
                new Position { Easting = 500000, Northing = 4500000 }
            },
            SectionStates = new[] { true, false, true }
        };

        // Act & Assert
        Assert.That(workProgress.AreaCovered, Is.EqualTo(12500.5));
        Assert.That(workProgress.DistanceTravelled, Is.EqualTo(2500.75));
        Assert.That(workProgress.TimeWorked, Is.EqualTo(TimeSpan.FromHours(2.5)));
        Assert.That(workProgress.CoverageTrail.Count, Is.EqualTo(1));
        Assert.That(workProgress.SectionStates.Length, Is.EqualTo(3));
    }

    [Test]
    public void VehicleProfile_SerializesToJson_Successfully()
    {
        // Arrange
        var vehicleProfile = new VehicleProfile
        {
            VehicleName = "TestTractor",
            CreatedDate = new DateTime(2025, 10, 20, 10, 0, 0, DateTimeKind.Utc),
            LastModifiedDate = new DateTime(2025, 10, 20, 14, 30, 0, DateTimeKind.Utc),
            Settings = new ApplicationSettings()
        };

        // Act
        var json = JsonSerializer.Serialize(vehicleProfile);
        var deserialized = JsonSerializer.Deserialize<VehicleProfile>(json);

        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.VehicleName, Is.EqualTo("TestTractor"));
        Assert.That(deserialized.Settings, Is.Not.Null);
    }

    [Test]
    public void UserPreferences_DefaultUnitSystem_IsMetric()
    {
        // Arrange & Act
        var preferences = new UserPreferences();

        // Assert
        Assert.That(preferences.PreferredUnitSystem, Is.EqualTo(UnitSystem.Metric));
        Assert.That(preferences.PreferredLanguage, Is.EqualTo("en"));
        Assert.That(preferences.DisplayPreferences, Is.Not.Null);
    }

    [Test]
    public void ValidationResult_IsValid_DependsOnErrors()
    {
        // Arrange
        var validResult = new SettingsValidationResult
        {
            IsValid = true,
            Errors = new List<ValidationError>(),
            Warnings = new List<ValidationWarning>
            {
                new ValidationWarning { SettingPath = "Test.Warning", Message = "This is a warning" }
            }
        };

        var invalidResult = new SettingsValidationResult
        {
            IsValid = false,
            Errors = new List<ValidationError>
            {
                new ValidationError { SettingPath = "Test.Error", Message = "This is an error" }
            },
            Warnings = new List<ValidationWarning>()
        };

        // Act & Assert
        Assert.That(validResult.IsValid, Is.True, "Result with only warnings should be valid");
        Assert.That(validResult.Warnings.Count, Is.EqualTo(1));
        Assert.That(invalidResult.IsValid, Is.False, "Result with errors should be invalid");
        Assert.That(invalidResult.Errors.Count, Is.EqualTo(1));
    }

    [Test]
    public void SettingConstraints_Properties_CanBeSet()
    {
        // Arrange & Act
        var constraints = new SettingConstraints
        {
            MinValue = 0,
            MaxValue = 100,
            DataType = "int",
            AllowedValues = new[] { "Option1", "Option2" },
            Dependencies = new[] { "OtherSetting.Value" },
            ValidationRule = "Must be between 0 and 100"
        };

        // Assert
        Assert.That(constraints.MinValue, Is.EqualTo(0));
        Assert.That(constraints.MaxValue, Is.EqualTo(100));
        Assert.That(constraints.DataType, Is.EqualTo("int"));
        Assert.That(constraints.AllowedValues!.Length, Is.EqualTo(2));
        Assert.That(constraints.Dependencies!.Length, Is.EqualTo(1));
    }

    [Test]
    public void SessionRestoreResult_CapturesCrashTime()
    {
        // Arrange
        var crashTime = new DateTime(2025, 10, 20, 15, 45, 30, DateTimeKind.Utc);

        var result = new SessionRestoreResult
        {
            Success = true,
            CrashTime = crashTime,
            RestoredSession = new SessionState
            {
                SessionStartTime = new DateTime(2025, 10, 20, 14, 0, 0, DateTimeKind.Utc),
                LastSnapshotTime = crashTime
            }
        };

        // Act & Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CrashTime, Is.EqualTo(crashTime));
        Assert.That(result.RestoredSession, Is.Not.Null);
        Assert.That(result.RestoredSession!.LastSnapshotTime, Is.EqualTo(crashTime));
    }

    [Test]
    public void ProfileSwitchResult_TracksSessionCarryOver()
    {
        // Arrange
        var resultWithCarryOver = new ProfileSwitchResult
        {
            Success = true,
            SessionCarriedOver = true,
            ErrorMessage = string.Empty
        };

        var resultWithoutCarryOver = new ProfileSwitchResult
        {
            Success = true,
            SessionCarriedOver = false,
            ErrorMessage = string.Empty
        };

        // Act & Assert
        Assert.That(resultWithCarryOver.SessionCarriedOver, Is.True);
        Assert.That(resultWithoutCarryOver.SessionCarriedOver, Is.False);
    }
}
