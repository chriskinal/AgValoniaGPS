using System;
using System.Linq;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Implements thread-safe section configuration management
/// Validates configuration, calculates widths and offsets, publishes change events
/// </summary>
/// <remarks>
/// Performance: Negligible (simple validation and calculation)
/// Thread Safety: Uses lock object pattern for concurrent access
/// Validation: Rejects invalid configurations (>31 sections, negative widths)
/// </remarks>
public class SectionConfigurationService : ISectionConfigurationService
{
    private readonly object _lockObject = new object();
    private SectionConfiguration _configuration;

    /// <summary>
    /// Event fired when configuration changes
    /// </summary>
    public event EventHandler? ConfigurationChanged;

    /// <summary>
    /// Creates a new instance with default configuration (5 sections @ 2.5m each)
    /// </summary>
    public SectionConfigurationService()
    {
        _configuration = new SectionConfiguration();
    }

    /// <summary>
    /// Loads and validates a section configuration
    /// </summary>
    /// <param name="configuration">Configuration to load</param>
    /// <exception cref="ArgumentNullException">If configuration is null</exception>
    /// <exception cref="ArgumentException">If configuration is invalid</exception>
    public void LoadConfiguration(SectionConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (!configuration.IsValid())
            throw new ArgumentException("Configuration is invalid. Check section count (1-31), widths (0.1-20m), delays (1.0-15.0s), and tolerances.", nameof(configuration));

        lock (_lockObject)
        {
            _configuration = configuration;
        }

        // Fire event outside of lock
        RaiseConfigurationChanged();
    }

    /// <summary>
    /// Gets the current configuration (returns defensive copy for thread safety)
    /// </summary>
    /// <returns>Copy of current section configuration</returns>
    public SectionConfiguration GetConfiguration()
    {
        lock (_lockObject)
        {
            // Return defensive copy to prevent external modification
            return new SectionConfiguration(_configuration.SectionCount, _configuration.SectionWidths.ToArray())
            {
                TurnOnDelay = _configuration.TurnOnDelay,
                TurnOffDelay = _configuration.TurnOffDelay,
                OverlapTolerance = _configuration.OverlapTolerance,
                LookAheadDistance = _configuration.LookAheadDistance,
                MinimumSpeed = _configuration.MinimumSpeed
            };
        }
    }

    /// <summary>
    /// Gets the width of a specific section
    /// </summary>
    /// <param name="sectionId">Zero-based section ID</param>
    /// <returns>Width of the section in meters</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    public double GetSectionWidth(int sectionId)
    {
        lock (_lockObject)
        {
            if (sectionId < 0 || sectionId >= _configuration.SectionCount)
                throw new ArgumentOutOfRangeException(nameof(sectionId),
                    $"Section ID {sectionId} is out of range. Valid range: 0-{_configuration.SectionCount - 1}");

            return _configuration.SectionWidths[sectionId];
        }
    }

    /// <summary>
    /// Gets the total width of all sections combined
    /// </summary>
    /// <returns>Total width in meters</returns>
    public double GetTotalWidth()
    {
        lock (_lockObject)
        {
            return _configuration.TotalWidth;
        }
    }

    /// <summary>
    /// Gets the lateral offset of a section from the vehicle centerline
    /// Offset is calculated assuming sections are arranged symmetrically
    /// </summary>
    /// <param name="sectionId">Zero-based section ID</param>
    /// <returns>Offset in meters (negative = left side, positive = right side)</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    public double GetSectionOffset(int sectionId)
    {
        lock (_lockObject)
        {
            if (sectionId < 0 || sectionId >= _configuration.SectionCount)
                throw new ArgumentOutOfRangeException(nameof(sectionId),
                    $"Section ID {sectionId} is out of range. Valid range: 0-{_configuration.SectionCount - 1}");

            // Calculate offset from centerline
            // Sections are numbered left to right (0 = leftmost)
            // Negative offset = left side, positive = right side

            double totalWidth = _configuration.TotalWidth;
            double leftEdge = -totalWidth / 2.0;

            // Sum widths of all sections to the left of this section
            double offsetToLeftEdge = 0.0;
            for (int i = 0; i < sectionId; i++)
            {
                offsetToLeftEdge += _configuration.SectionWidths[i];
            }

            // Offset to center of this section
            double sectionWidth = _configuration.SectionWidths[sectionId];
            double sectionCenterOffset = leftEdge + offsetToLeftEdge + (sectionWidth / 2.0);

            return sectionCenterOffset;
        }
    }

    private void RaiseConfigurationChanged()
    {
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
}
