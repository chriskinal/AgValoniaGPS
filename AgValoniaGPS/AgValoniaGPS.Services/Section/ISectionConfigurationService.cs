using System;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Service interface for managing section configuration
/// Provides validation, state management, and width calculations
/// </summary>
public interface ISectionConfigurationService
{
    /// <summary>
    /// Event fired when configuration changes
    /// </summary>
    event EventHandler? ConfigurationChanged;

    /// <summary>
    /// Loads and validates a section configuration
    /// </summary>
    /// <param name="configuration">Configuration to load</param>
    /// <exception cref="ArgumentNullException">If configuration is null</exception>
    /// <exception cref="ArgumentException">If configuration is invalid</exception>
    void LoadConfiguration(SectionConfiguration configuration);

    /// <summary>
    /// Gets the current configuration (returns copy for thread safety)
    /// </summary>
    /// <returns>Current section configuration</returns>
    SectionConfiguration GetConfiguration();

    /// <summary>
    /// Gets the width of a specific section
    /// </summary>
    /// <param name="sectionId">Zero-based section ID</param>
    /// <returns>Width of the section in meters</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    double GetSectionWidth(int sectionId);

    /// <summary>
    /// Gets the total width of all sections combined
    /// </summary>
    /// <returns>Total width in meters</returns>
    double GetTotalWidth();

    /// <summary>
    /// Gets the lateral offset of a section from the vehicle centerline
    /// </summary>
    /// <param name="sectionId">Zero-based section ID</param>
    /// <returns>Offset in meters (negative = left, positive = right)</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    double GetSectionOffset(int sectionId);
}
