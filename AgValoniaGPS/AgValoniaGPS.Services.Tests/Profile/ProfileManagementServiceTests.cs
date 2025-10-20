using System;
using System.IO;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.Services.Profile;
using Moq;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Profile;

/// <summary>
/// Focused tests for ProfileManagementService.
/// Tests critical operations: profile CRUD, profile switching.
/// </summary>
public class ProfileManagementServiceTests : IDisposable
{
    private readonly Mock<IConfigurationService> _mockConfigurationService;
    private readonly VehicleProfileProvider _vehicleProvider;
    private readonly UserProfileProvider _userProvider;
    private readonly ProfileManagementService _service;
    private readonly string _testVehiclesDir;
    private readonly string _testUsersDir;

    public ProfileManagementServiceTests()
    {
        // Setup test directories
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _testVehiclesDir = Path.Combine(documentsPath, "AgValoniaGPS", "Vehicles");
        _testUsersDir = Path.Combine(documentsPath, "AgValoniaGPS", "Users");

        Directory.CreateDirectory(_testVehiclesDir);
        Directory.CreateDirectory(_testUsersDir);

        // Setup mocks
        _mockConfigurationService = new Mock<IConfigurationService>();
        _mockConfigurationService
            .Setup(x => x.LoadSettingsAsync(It.IsAny<string>()))
            .ReturnsAsync(new SettingsLoadResult
            {
                Success = true,
                Settings = new ApplicationSettings(),
                Source = SettingsLoadSource.Json
            });

        _mockConfigurationService
            .Setup(x => x.GetDisplaySettings())
            .Returns(new DisplaySettings());

        _mockConfigurationService
            .Setup(x => x.GetCultureSettings())
            .Returns(new CultureSettings());

        _mockConfigurationService
            .Setup(x => x.UpdateDisplaySettingsAsync(It.IsAny<DisplaySettings>()))
            .ReturnsAsync(new AgValoniaGPS.Models.Validation.ValidationResult { IsValid = true });

        _mockConfigurationService
            .Setup(x => x.UpdateCultureSettingsAsync(It.IsAny<CultureSettings>()))
            .ReturnsAsync(new AgValoniaGPS.Models.Validation.ValidationResult { IsValid = true });

        // Create providers and service
        _vehicleProvider = new VehicleProfileProvider();
        _userProvider = new UserProfileProvider();
        _service = new ProfileManagementService(_vehicleProvider, _userProvider, _mockConfigurationService.Object);
    }

    public void Dispose()
    {
        // Cleanup test files
        CleanupTestFiles();
    }

    private void CleanupTestFiles()
    {
        if (Directory.Exists(_testVehiclesDir))
        {
            foreach (var file in Directory.GetFiles(_testVehiclesDir, "Test*.json"))
            {
                try { File.Delete(file); } catch { }
            }
            foreach (var file in Directory.GetFiles(_testVehiclesDir, "Default*.json"))
            {
                try { File.Delete(file); } catch { }
            }
        }

        if (Directory.Exists(_testUsersDir))
        {
            foreach (var file in Directory.GetFiles(_testUsersDir, "Test*.json"))
            {
                try { File.Delete(file); } catch { }
            }
            foreach (var file in Directory.GetFiles(_testUsersDir, "Default*.json"))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    [Fact]
    public async Task CreateVehicleProfile_ValidProfile_CreatesSuccessfully()
    {
        // Arrange
        var vehicleName = "TestTractor1";
        var settings = new ApplicationSettings
        {
            Vehicle = new VehicleSettings { Wheelbase = 250.0 }
        };

        // Act
        var result = await _service.CreateVehicleProfileAsync(vehicleName, settings);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessage);

        var profiles = await _service.GetVehicleProfilesAsync();
        Assert.Contains(vehicleName, profiles);

        // Cleanup
        await _service.DeleteVehicleProfileAsync(vehicleName);
    }

    [Fact]
    public async Task SwitchVehicleProfile_WithCarryOver_RaisesEventAndLoadsSettings()
    {
        // Arrange
        var vehicleName = "TestTractor2";
        var settings = new ApplicationSettings();
        await _service.CreateVehicleProfileAsync(vehicleName, settings);

        ProfileChangedEventArgs? eventArgs = null;
        _service.ProfileChanged += (sender, args) => eventArgs = args;

        // Act
        var result = await _service.SwitchVehicleProfileAsync(vehicleName, carryOverSession: true);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.SessionCarriedOver);

        Assert.NotNull(eventArgs);
        Assert.Equal(ProfileType.Vehicle, eventArgs.ProfileType);
        Assert.Equal(vehicleName, eventArgs.ProfileName);
        Assert.True(eventArgs.SessionCarriedOver);

        _mockConfigurationService.Verify(x => x.LoadSettingsAsync(vehicleName), Times.Once);

        // Cleanup
        await _service.DeleteVehicleProfileAsync(vehicleName);
    }

    [Fact]
    public async Task SwitchVehicleProfile_WithoutCarryOver_RaisesEventWithCorrectFlag()
    {
        // Arrange
        var vehicleName = "TestTractor3";
        var settings = new ApplicationSettings();
        await _service.CreateVehicleProfileAsync(vehicleName, settings);

        ProfileChangedEventArgs? eventArgs = null;
        _service.ProfileChanged += (sender, args) => eventArgs = args;

        // Act
        var result = await _service.SwitchVehicleProfileAsync(vehicleName, carryOverSession: false);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.SessionCarriedOver);

        Assert.NotNull(eventArgs);
        Assert.False(eventArgs.SessionCarriedOver);

        // Cleanup
        await _service.DeleteVehicleProfileAsync(vehicleName);
    }

    [Fact]
    public async Task CreateUserProfile_ValidProfile_CreatesSuccessfully()
    {
        // Arrange
        var userName = "TestUser1";
        var preferences = new UserPreferences
        {
            PreferredUnitSystem = UnitSystem.Metric,
            PreferredLanguage = "en"
        };

        // Act
        var result = await _service.CreateUserProfileAsync(userName, preferences);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessage);

        var profiles = await _service.GetUserProfilesAsync();
        Assert.Contains(userName, profiles);

        // Cleanup
        await _service.DeleteUserProfileAsync(userName);
    }

    [Fact]
    public async Task SwitchUserProfile_UpdatesPreferencesAndRaisesEvent()
    {
        // Arrange
        var userName = "TestUser2";
        var preferences = new UserPreferences
        {
            PreferredUnitSystem = UnitSystem.Imperial,
            PreferredLanguage = "de"
        };
        await _service.CreateUserProfileAsync(userName, preferences);

        ProfileChangedEventArgs? eventArgs = null;
        _service.ProfileChanged += (sender, args) => eventArgs = args;

        // Act
        var result = await _service.SwitchUserProfileAsync(userName);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.SessionCarriedOver); // User switches never carry over session

        Assert.NotNull(eventArgs);
        Assert.Equal(ProfileType.User, eventArgs.ProfileType);
        Assert.Equal(userName, eventArgs.ProfileName);

        // Verify preferences were applied to configuration
        _mockConfigurationService.Verify(x => x.UpdateDisplaySettingsAsync(It.IsAny<DisplaySettings>()), Times.Once);
        _mockConfigurationService.Verify(x => x.UpdateCultureSettingsAsync(It.IsAny<CultureSettings>()), Times.Once);

        // Cleanup
        await _service.DeleteUserProfileAsync(userName);
    }

    [Fact]
    public async Task DeleteVehicleProfile_ProfileExists_DeletesSuccessfully()
    {
        // Arrange
        var vehicleName = "TestTractor4";
        await _service.CreateVehicleProfileAsync(vehicleName, new ApplicationSettings());

        // Act
        var result = await _service.DeleteVehicleProfileAsync(vehicleName);

        // Assert
        Assert.True(result.Success);

        var profiles = await _service.GetVehicleProfilesAsync();
        Assert.DoesNotContain(vehicleName, profiles);
    }

    [Fact]
    public async Task GetDefaultUserProfileName_ReturnsFirstUser()
    {
        // Arrange
        await _service.CreateUserProfileAsync("TestUserA", new UserPreferences());
        await _service.CreateUserProfileAsync("TestUserB", new UserPreferences());

        // Act
        var defaultUser = _service.GetDefaultUserProfileName();

        // Assert
        Assert.NotEmpty(defaultUser);
        Assert.Contains(defaultUser, new[] { "TestUserA", "TestUserB" });

        // Cleanup
        await _service.DeleteUserProfileAsync("TestUserA");
        await _service.DeleteUserProfileAsync("TestUserB");
    }

    [Fact]
    public async Task GetCurrentVehicleProfile_ReturnsActiveProfile()
    {
        // Arrange
        var vehicleName = "TestTractor5";
        await _service.CreateVehicleProfileAsync(vehicleName, new ApplicationSettings());
        await _service.SwitchVehicleProfileAsync(vehicleName, carryOverSession: false);

        // Act
        var currentProfile = _service.GetCurrentVehicleProfile();

        // Assert
        Assert.NotNull(currentProfile);
        Assert.Equal(vehicleName, currentProfile.VehicleName);

        // Cleanup
        await _service.DeleteVehicleProfileAsync(vehicleName);
    }
}
