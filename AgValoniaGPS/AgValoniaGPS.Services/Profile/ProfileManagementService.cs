using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.Configuration;

namespace AgValoniaGPS.Services.Profile;

/// <summary>
/// Manages vehicle and user profiles with runtime switching capabilities.
/// Coordinates with IConfigurationService for settings loading.
/// </summary>
public class ProfileManagementService : IProfileManagementService
{
    private readonly IProfileProvider<VehicleProfile> _vehicleProvider;
    private readonly IProfileProvider<UserProfile> _userProvider;
    private readonly IConfigurationService _configurationService;

    private VehicleProfile _currentVehicleProfile;
    private UserProfile _currentUserProfile;

    /// <summary>
    /// Raised when the active profile changes (vehicle or user).
    /// </summary>
    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    /// <summary>
    /// Initializes a new instance of the ProfileManagementService.
    /// </summary>
    public ProfileManagementService(
        IProfileProvider<VehicleProfile> vehicleProvider,
        IProfileProvider<UserProfile> userProvider,
        IConfigurationService configurationService)
    {
        _vehicleProvider = vehicleProvider ?? throw new ArgumentNullException(nameof(vehicleProvider));
        _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

        _currentVehicleProfile = new VehicleProfile { VehicleName = "Default" };
        _currentUserProfile = new UserProfile { UserName = "Default" };
    }

    // Vehicle profile management

    /// <summary>
    /// Gets all available vehicle profile names.
    /// </summary>
    public async Task<string[]> GetVehicleProfilesAsync()
    {
        return await _vehicleProvider.GetAllAsync();
    }

    /// <summary>
    /// Gets a specific vehicle profile by name.
    /// </summary>
    public async Task<VehicleProfile> GetVehicleProfileAsync(string vehicleName)
    {
        return await _vehicleProvider.GetAsync(vehicleName);
    }

    /// <summary>
    /// Creates a new vehicle profile with specified settings.
    /// </summary>
    public async Task<ProfileCreateResult> CreateVehicleProfileAsync(string vehicleName, ApplicationSettings settings)
    {
        var profile = new VehicleProfile
        {
            VehicleName = vehicleName,
            Settings = settings ?? new ApplicationSettings(),
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        return await _vehicleProvider.CreateAsync(profile);
    }

    /// <summary>
    /// Deletes a vehicle profile.
    /// </summary>
    public async Task<ProfileDeleteResult> DeleteVehicleProfileAsync(string vehicleName)
    {
        // Prevent deleting the currently active profile
        if (_currentVehicleProfile?.VehicleName == vehicleName)
        {
            return new ProfileDeleteResult
            {
                Success = false,
                ErrorMessage = "Cannot delete the currently active vehicle profile"
            };
        }

        return await _vehicleProvider.DeleteAsync(vehicleName);
    }

    /// <summary>
    /// Switches to a different vehicle profile.
    /// </summary>
    /// <param name="vehicleName">Name of the vehicle profile to switch to</param>
    /// <param name="carryOverSession">If true, preserve current field and work progress; if false, clear session</param>
    public async Task<ProfileSwitchResult> SwitchVehicleProfileAsync(string vehicleName, bool carryOverSession)
    {
        try
        {
            // Load the new vehicle profile
            var newProfile = await _vehicleProvider.GetAsync(vehicleName);

            // Load vehicle settings into ConfigurationService
            var loadResult = await _configurationService.LoadSettingsAsync(vehicleName);

            if (!loadResult.Success)
            {
                return new ProfileSwitchResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to load vehicle settings: {loadResult.ErrorMessage}",
                    SessionCarriedOver = false
                };
            }

            // Update current vehicle profile
            var previousProfile = _currentVehicleProfile;
            _currentVehicleProfile = newProfile;

            // Raise ProfileChanged event
            ProfileChanged?.Invoke(this, new ProfileChangedEventArgs
            {
                ProfileType = ProfileType.Vehicle,
                ProfileName = vehicleName,
                SessionCarriedOver = carryOverSession,
                Timestamp = DateTime.UtcNow
            });

            return new ProfileSwitchResult
            {
                Success = true,
                ErrorMessage = string.Empty,
                SessionCarriedOver = carryOverSession
            };
        }
        catch (Exception ex)
        {
            return new ProfileSwitchResult
            {
                Success = false,
                ErrorMessage = $"Failed to switch vehicle profile: {ex.Message}",
                SessionCarriedOver = false
            };
        }
    }

    // User profile management

    /// <summary>
    /// Gets all available user profile names.
    /// </summary>
    public async Task<string[]> GetUserProfilesAsync()
    {
        return await _userProvider.GetAllAsync();
    }

    /// <summary>
    /// Gets a specific user profile by name.
    /// </summary>
    public async Task<UserProfile> GetUserProfileAsync(string userName)
    {
        return await _userProvider.GetAsync(userName);
    }

    /// <summary>
    /// Creates a new user profile with specified preferences.
    /// </summary>
    public async Task<ProfileCreateResult> CreateUserProfileAsync(string userName, UserPreferences preferences)
    {
        var profile = new UserProfile
        {
            UserName = userName,
            Preferences = preferences ?? new UserPreferences(),
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        return await _userProvider.CreateAsync(profile);
    }

    /// <summary>
    /// Deletes a user profile.
    /// </summary>
    public async Task<ProfileDeleteResult> DeleteUserProfileAsync(string userName)
    {
        // Prevent deleting the currently active profile
        if (_currentUserProfile?.UserName == userName)
        {
            return new ProfileDeleteResult
            {
                Success = false,
                ErrorMessage = "Cannot delete the currently active user profile"
            };
        }

        return await _userProvider.DeleteAsync(userName);
    }

    /// <summary>
    /// Switches to a different user profile.
    /// User profile switches do not affect vehicle settings or session state.
    /// </summary>
    public async Task<ProfileSwitchResult> SwitchUserProfileAsync(string userName)
    {
        try
        {
            // Load the new user profile
            var newProfile = await _userProvider.GetAsync(userName);

            // Update current user profile
            var previousProfile = _currentUserProfile;
            _currentUserProfile = newProfile;

            // Apply user preferences to display settings
            if (newProfile.Preferences != null)
            {
                var displaySettings = _configurationService.GetDisplaySettings();
                displaySettings.UnitSystem = newProfile.Preferences.PreferredUnitSystem;
                await _configurationService.UpdateDisplaySettingsAsync(displaySettings);

                var cultureSettings = _configurationService.GetCultureSettings();
                cultureSettings.CultureCode = newProfile.Preferences.PreferredLanguage;
                await _configurationService.UpdateCultureSettingsAsync(cultureSettings);
            }

            // Raise ProfileChanged event
            ProfileChanged?.Invoke(this, new ProfileChangedEventArgs
            {
                ProfileType = ProfileType.User,
                ProfileName = userName,
                SessionCarriedOver = false, // User switches never affect session
                Timestamp = DateTime.UtcNow
            });

            return new ProfileSwitchResult
            {
                Success = true,
                ErrorMessage = string.Empty,
                SessionCarriedOver = false
            };
        }
        catch (Exception ex)
        {
            return new ProfileSwitchResult
            {
                Success = false,
                ErrorMessage = $"Failed to switch user profile: {ex.Message}",
                SessionCarriedOver = false
            };
        }
    }

    // Get current profiles

    /// <summary>
    /// Gets the currently active vehicle profile.
    /// </summary>
    public VehicleProfile GetCurrentVehicleProfile()
    {
        return _currentVehicleProfile;
    }

    /// <summary>
    /// Gets the currently active user profile.
    /// </summary>
    public UserProfile GetCurrentUserProfile()
    {
        return _currentUserProfile;
    }

    /// <summary>
    /// Gets the default user profile name (first user added to system).
    /// Used at application startup if no user preference is saved.
    /// </summary>
    public string GetDefaultUserProfileName()
    {
        var users = _userProvider.GetAllAsync().GetAwaiter().GetResult();
        return users.Length > 0 ? users[0] : string.Empty;
    }
}
