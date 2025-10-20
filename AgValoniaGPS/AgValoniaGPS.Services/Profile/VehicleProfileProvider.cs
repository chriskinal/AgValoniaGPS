using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.Models.Configuration;

namespace AgValoniaGPS.Services.Profile;

/// <summary>
/// Provides file I/O operations for vehicle profiles.
/// Manages profiles in Documents/AgValoniaGPS/Vehicles/ directory.
/// </summary>
public class VehicleProfileProvider : IProfileProvider<VehicleProfile>
{
    private readonly string _vehiclesDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the VehicleProfileProvider.
    /// </summary>
    public VehicleProfileProvider()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _vehiclesDirectory = Path.Combine(documentsPath, "AgValoniaGPS", "Vehicles");

        // Ensure directory exists
        Directory.CreateDirectory(_vehiclesDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Gets all available vehicle profile names.
    /// </summary>
    public async Task<string[]> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(_vehiclesDirectory))
            {
                return Array.Empty<string>();
            }

            var jsonFiles = Directory.GetFiles(_vehiclesDirectory, "*.json");
            return jsonFiles
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name))
                .OrderBy(name => name)
                .ToArray()!;
        });
    }

    /// <summary>
    /// Gets a specific vehicle profile by name.
    /// </summary>
    public async Task<VehicleProfile> GetAsync(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ArgumentException("Profile name cannot be empty", nameof(profileName));
        }

        var filePath = GetProfileFilePath(profileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Vehicle profile '{profileName}' not found", filePath);
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var profile = JsonSerializer.Deserialize<VehicleProfile>(json, _jsonOptions);

            if (profile == null)
            {
                throw new InvalidOperationException($"Failed to deserialize vehicle profile '{profileName}'");
            }

            return profile;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse vehicle profile '{profileName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a new vehicle profile.
    /// </summary>
    public async Task<ProfileCreateResult> CreateAsync(VehicleProfile profile)
    {
        if (profile == null)
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = "Profile cannot be null"
            };
        }

        if (string.IsNullOrWhiteSpace(profile.VehicleName))
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = "Vehicle name cannot be empty"
            };
        }

        var filePath = GetProfileFilePath(profile.VehicleName);

        if (File.Exists(filePath))
        {
            return new ProfileCreateResult
            {
                Success = false,
                ErrorMessage = $"Vehicle profile '{profile.VehicleName}' already exists"
            };
        }

        try
        {
            // Set timestamps
            profile.CreatedDate = DateTime.UtcNow;
            profile.LastModifiedDate = DateTime.UtcNow;

            // Ensure settings object exists
            if (profile.Settings == null)
            {
                profile.Settings = new ApplicationSettings();
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
                ErrorMessage = $"Failed to create vehicle profile: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Deletes a vehicle profile.
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

        var jsonFilePath = GetProfileFilePath(profileName);
        var xmlFilePath = GetXmlFilePath(profileName);

        if (!File.Exists(jsonFilePath))
        {
            return new ProfileDeleteResult
            {
                Success = false,
                ErrorMessage = $"Vehicle profile '{profileName}' not found"
            };
        }

        try
        {
            await Task.Run(() =>
            {
                // Delete JSON file
                File.Delete(jsonFilePath);

                // Also delete XML file if it exists (legacy compatibility)
                if (File.Exists(xmlFilePath))
                {
                    File.Delete(xmlFilePath);
                }
            });

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
                ErrorMessage = $"Failed to delete vehicle profile: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Saves changes to an existing vehicle profile.
    /// </summary>
    public async Task<bool> SaveAsync(VehicleProfile profile)
    {
        if (profile == null || string.IsNullOrWhiteSpace(profile.VehicleName))
        {
            return false;
        }

        try
        {
            // Update modification timestamp
            profile.LastModifiedDate = DateTime.UtcNow;

            var filePath = GetProfileFilePath(profile.VehicleName);
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
    /// Gets the full file path for a vehicle profile JSON file.
    /// </summary>
    private string GetProfileFilePath(string profileName)
    {
        return Path.Combine(_vehiclesDirectory, $"{profileName}.json");
    }

    /// <summary>
    /// Gets the full file path for a vehicle profile XML file (legacy).
    /// </summary>
    private string GetXmlFilePath(string profileName)
    {
        return Path.Combine(_vehiclesDirectory, $"{profileName}.xml");
    }
}
