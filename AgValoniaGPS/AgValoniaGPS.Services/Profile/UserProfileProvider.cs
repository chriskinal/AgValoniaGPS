using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Profile;

namespace AgValoniaGPS.Services.Profile;

/// <summary>
/// Provides file I/O operations for user profiles.
/// Manages profiles in Documents/AgValoniaGPS/Users/ directory.
/// </summary>
public class UserProfileProvider : IProfileProvider<UserProfile>
{
    private readonly string _usersDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the UserProfileProvider.
    /// </summary>
    public UserProfileProvider()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _usersDirectory = Path.Combine(documentsPath, "AgValoniaGPS", "Users");

        // Ensure directory exists
        Directory.CreateDirectory(_usersDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Gets all available user profile names.
    /// </summary>
    public async Task<string[]> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(_usersDirectory))
            {
                return Array.Empty<string>();
            }

            var jsonFiles = Directory.GetFiles(_usersDirectory, "*.json");
            return jsonFiles
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name))
                .OrderBy(name => name)
                .ToArray()!;
        });
    }

    /// <summary>
    /// Gets a specific user profile by name.
    /// </summary>
    public async Task<UserProfile> GetAsync(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ArgumentException("Profile name cannot be empty", nameof(profileName));
        }

        var filePath = GetProfileFilePath(profileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"User profile '{profileName}' not found", filePath);
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var profile = JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions);

            if (profile == null)
            {
                throw new InvalidOperationException($"Failed to deserialize user profile '{profileName}'");
            }

            return profile;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse user profile '{profileName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a new user profile.
    /// </summary>
    public async Task<ProfileCreateResult> CreateAsync(UserProfile profile)
    {
        if (profile == null)
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = "Profile cannot be null"
            };
        }

        if (string.IsNullOrWhiteSpace(profile.UserName))
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = "User name cannot be empty"
            };
        }

        var filePath = GetProfileFilePath(profile.UserName);

        if (File.Exists(filePath))
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = $"User profile '{profile.UserName}' already exists"
            };
        }

        try
        {
            // Set timestamps
            profile.CreatedDate = DateTime.UtcNow;
            profile.LastModifiedDate = DateTime.UtcNow;

            // Ensure preferences object exists
            if (profile.Preferences == null)
            {
                profile.Preferences = new UserPreferences();
            }

            // Serialize to JSON
            var json = JsonSerializer.Serialize(profile, _jsonOptions);

            // Write to file
            await File.WriteAllTextAsync(filePath, json);

            return new ProfileCreateResult
            {
                Success = true,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = $"Failed to create user profile: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Deletes a user profile.
    /// </summary>
    public async Task<ProfileDeleteResult> DeleteAsync(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return new ProfileDeleteResult
            {
                Success = false,
                ErrorMessage = "Profile name cannot be empty"
            };
        }

        var filePath = GetProfileFilePath(profileName);

        if (!File.Exists(filePath))
        {
            return new ProfileDeleteResult
            {
                Success = false,
                ErrorMessage = $"User profile '{profileName}' not found"
            };
        }

        try
        {
            await Task.Run(() => File.Delete(filePath));

            return new ProfileDeleteResult
            {
                Success = true,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProfileDeleteResult
            {
                Success = false,
                ErrorMessage = $"Failed to delete user profile: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Saves changes to an existing user profile.
    /// </summary>
    public async Task<bool> SaveAsync(UserProfile profile)
    {
        if (profile == null || string.IsNullOrWhiteSpace(profile.UserName))
        {
            return false;
        }

        try
        {
            // Update modification timestamp
            profile.LastModifiedDate = DateTime.UtcNow;

            var filePath = GetProfileFilePath(profile.UserName);
            var json = JsonSerializer.Serialize(profile, _jsonOptions);

            await File.WriteAllTextAsync(filePath, json);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the full file path for a user profile JSON file.
    /// </summary>
    private string GetProfileFilePath(string profileName)
    {
        return Path.Combine(_usersDirectory, $"{profileName}.json");
    }
}
