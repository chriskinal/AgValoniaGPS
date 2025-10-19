using System.Threading.Tasks;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Service interface for reading and writing section control configuration files
/// File format: SectionConfig.txt (AgOpenGPS-compatible text format)
/// </summary>
public interface ISectionControlFileService
{
    /// <summary>
    /// Saves section configuration to SectionConfig.txt in the field directory
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <param name="configuration">Configuration to save</param>
    /// <returns>Task that completes when save is done</returns>
    Task SaveConfigurationAsync(string fieldPath, SectionConfiguration configuration);

    /// <summary>
    /// Loads section configuration from SectionConfig.txt in the field directory
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <returns>Loaded configuration, or null if file doesn't exist or is invalid</returns>
    Task<SectionConfiguration?> LoadConfigurationAsync(string fieldPath);

    /// <summary>
    /// Synchronous version of SaveConfiguration for compatibility
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <param name="configuration">Configuration to save</param>
    void SaveConfiguration(string fieldPath, SectionConfiguration configuration);

    /// <summary>
    /// Synchronous version of LoadConfiguration for compatibility
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <returns>Loaded configuration, or null if file doesn't exist or is invalid</returns>
    SectionConfiguration? LoadConfiguration(string fieldPath);
}
