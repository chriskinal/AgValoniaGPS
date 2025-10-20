using AgValoniaGPS.Models.Configuration;

namespace AgValoniaGPS.Services.Configuration;

/// <summary>
/// Abstraction for configuration file providers.
/// Supports different file formats (JSON, XML) for settings persistence.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Loads application settings from a file.
    /// </summary>
    /// <param name="filePath">Full path to the settings file</param>
    /// <returns>Loaded application settings</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist</exception>
    /// <exception cref="InvalidDataException">Thrown if the file format is invalid</exception>
    Task<ApplicationSettings> LoadAsync(string filePath);

    /// <summary>
    /// Saves application settings to a file.
    /// </summary>
    /// <param name="filePath">Full path to the settings file</param>
    /// <param name="settings">Settings to save</param>
    /// <exception cref="IOException">Thrown if the file cannot be written</exception>
    Task SaveAsync(string filePath, ApplicationSettings settings);
}
