using System.Text.Json;
using System.Text.Json.Serialization;
using AgValoniaGPS.Models.Configuration;

namespace AgValoniaGPS.Services.Configuration;

/// <summary>
/// Provides JSON-based configuration file I/O using hierarchical structure.
/// Primary format for modern configuration management.
/// </summary>
public class JsonConfigurationProvider : IConfigurationProvider
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Loads application settings from a JSON file.
    /// </summary>
    /// <param name="filePath">Full path to the JSON settings file</param>
    /// <returns>Loaded application settings</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist</exception>
    /// <exception cref="InvalidDataException">Thrown if the JSON format is invalid</exception>
    public async Task<ApplicationSettings> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Settings file not found: {filePath}", filePath);
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            var settings = await JsonSerializer.DeserializeAsync<ApplicationSettings>(stream, _jsonOptions);

            if (settings == null)
            {
                throw new InvalidDataException($"Failed to deserialize settings from JSON file: {filePath}");
            }

            return settings;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Invalid JSON format in file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Saves application settings to a JSON file.
    /// </summary>
    /// <param name="filePath">Full path to the JSON settings file</param>
    /// <param name="settings">Settings to save</param>
    /// <exception cref="IOException">Thrown if the file cannot be written</exception>
    public async Task SaveAsync(string filePath, ApplicationSettings settings)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write to temporary file first for atomic operation
            var tempFilePath = filePath + ".tmp";
            using (var stream = File.Create(tempFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions);
            }

            // Replace original file with temp file (atomic on most filesystems)
            File.Move(tempFilePath, filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Failed to write JSON settings file: {filePath}", ex);
        }
    }
}
