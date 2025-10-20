using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.Profile;

/// <summary>
/// Manages vehicle and user profiles with runtime switching capabilities.
/// Provides profile CRUD operations and coordinates profile switches across the application.
/// </summary>
public interface IProfileManagementService
{
    /// <summary>
    /// Raised when the active profile changes (vehicle or user).
    /// </summary>
    event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    // Vehicle profile management
    /// <summary>
    /// Gets all available vehicle profile names.
    /// </summary>
    /// <returns>Array of vehicle profile names</returns>
    Task<string[]> GetVehicleProfilesAsync();

    /// <summary>
    /// Gets a specific vehicle profile by name.
    /// </summary>
    /// <param name="vehicleName">Name of the vehicle profile</param>
    /// <returns>Vehicle profile with all settings</returns>
    Task<VehicleProfile> GetVehicleProfileAsync(string vehicleName);

    /// <summary>
    /// Creates a new vehicle profile with specified settings.
    /// </summary>
    /// <param name="vehicleName">Name for the new vehicle profile</param>
    /// <param name="settings">Initial vehicle settings</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileCreateResult> CreateVehicleProfileAsync(string vehicleName, AgValoniaGPS.Models.Configuration.ApplicationSettings settings);

    /// <summary>
    /// Deletes a vehicle profile.
    /// </summary>
    /// <param name="vehicleName">Name of the vehicle profile to delete</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileDeleteResult> DeleteVehicleProfileAsync(string vehicleName);

    /// <summary>
    /// Switches to a different vehicle profile.
    /// </summary>
    /// <param name="vehicleName">Name of the vehicle profile to switch to</param>
    /// <param name="carryOverSession">If true, preserve current field and work progress; if false, clear session</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileSwitchResult> SwitchVehicleProfileAsync(string vehicleName, bool carryOverSession);

    // User profile management
    /// <summary>
    /// Gets all available user profile names.
    /// </summary>
    /// <returns>Array of user profile names</returns>
    Task<string[]> GetUserProfilesAsync();

    /// <summary>
    /// Gets a specific user profile by name.
    /// </summary>
    /// <param name="userName">Name of the user profile</param>
    /// <returns>User profile with preferences</returns>
    Task<UserProfile> GetUserProfileAsync(string userName);

    /// <summary>
    /// Creates a new user profile with specified preferences.
    /// </summary>
    /// <param name="userName">Name for the new user profile</param>
    /// <param name="preferences">Initial user preferences</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileCreateResult> CreateUserProfileAsync(string userName, UserPreferences preferences);

    /// <summary>
    /// Deletes a user profile.
    /// </summary>
    /// <param name="userName">Name of the user profile to delete</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileDeleteResult> DeleteUserProfileAsync(string userName);

    /// <summary>
    /// Switches to a different user profile.
    /// User profile switches do not affect vehicle settings or session state.
    /// </summary>
    /// <param name="userName">Name of the user profile to switch to</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileSwitchResult> SwitchUserProfileAsync(string userName);

    // Get current profiles
    /// <summary>
    /// Gets the currently active vehicle profile.
    /// </summary>
    /// <returns>Current vehicle profile</returns>
    VehicleProfile GetCurrentVehicleProfile();

    /// <summary>
    /// Gets the currently active user profile.
    /// </summary>
    /// <returns>Current user profile</returns>
    UserProfile GetCurrentUserProfile();

    /// <summary>
    /// Gets the default user profile name (first user added to system).
    /// Used at application startup if no user preference is saved.
    /// </summary>
    /// <returns>Default user profile name, or empty string if no users exist</returns>
    string GetDefaultUserProfileName();
}
