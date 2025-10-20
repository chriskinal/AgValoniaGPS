using AgValoniaGPS.Models.Profile;

namespace AgValoniaGPS.Services.Profile;

/// <summary>
/// Provides abstraction for profile file I/O operations.
/// Implemented separately for vehicle and user profiles.
/// </summary>
/// <typeparam name="TProfile">Type of profile (VehicleProfile or UserProfile)</typeparam>
public interface IProfileProvider<TProfile>
{
    /// <summary>
    /// Gets all available profile names.
    /// </summary>
    /// <returns>Array of profile names</returns>
    Task<string[]> GetAllAsync();

    /// <summary>
    /// Gets a specific profile by name.
    /// </summary>
    /// <param name="profileName">Name of the profile</param>
    /// <returns>Profile instance</returns>
    Task<TProfile> GetAsync(string profileName);

    /// <summary>
    /// Creates a new profile.
    /// </summary>
    /// <param name="profile">Profile to create</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileCreateResult> CreateAsync(TProfile profile);

    /// <summary>
    /// Deletes a profile.
    /// </summary>
    /// <param name="profileName">Name of the profile to delete</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ProfileDeleteResult> DeleteAsync(string profileName);

    /// <summary>
    /// Saves changes to an existing profile.
    /// </summary>
    /// <param name="profile">Profile to save</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SaveAsync(TProfile profile);
}
