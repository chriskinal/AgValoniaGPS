using System;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Implements section speed calculations based on vehicle turning radius and section offset.
/// </summary>
/// <remarks>
/// Thread-safe implementation using lock object pattern from PositionUpdateService.
/// Performance optimized for &lt;1ms execution time for all 31 sections.
/// </remarks>
public class SectionSpeedService : ISectionSpeedService
{
    private const double StraightLineThreshold = 1000.0; // meters
    private const double MinimumSpeedThreshold = 0.1; // m/s

    private readonly object _lockObject = new object();
    private readonly ISectionConfigurationService _configService;
    private readonly IVehicleKinematicsService _kinematicsService;
    private readonly IPositionUpdateService _positionService;

    private double[] _sectionSpeeds = Array.Empty<double>();
    private double[] _sectionOffsets = Array.Empty<double>();

    public event EventHandler<SectionSpeedChangedEventArgs>? SectionSpeedChanged;

    /// <summary>
    /// Creates a new SectionSpeedService with required dependencies.
    /// </summary>
    /// <param name="configService">Section configuration service</param>
    /// <param name="kinematicsService">Vehicle kinematics service</param>
    /// <param name="positionService">Position update service</param>
    public SectionSpeedService(
        ISectionConfigurationService configService,
        IVehicleKinematicsService kinematicsService,
        IPositionUpdateService positionService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _kinematicsService = kinematicsService ?? throw new ArgumentNullException(nameof(kinematicsService));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));

        // Subscribe to configuration changes to update section offsets
        _configService.ConfigurationChanged += OnConfigurationChanged;

        // Initialize from current configuration
        UpdateSectionOffsets();
    }

    public void CalculateSectionSpeeds(double vehicleSpeed, double turningRadius, double heading)
    {
        lock (_lockObject)
        {
            // Handle straight-line movement (very large radius or very slow speed)
            if (Math.Abs(turningRadius) > StraightLineThreshold || vehicleSpeed < MinimumSpeedThreshold)
            {
                // All sections move at vehicle speed
                for (int i = 0; i < _sectionSpeeds.Length; i++)
                {
                    _sectionSpeeds[i] = vehicleSpeed;
                }
                return;
            }

            // Calculate section speeds for turning
            double absTurningRadius = Math.Abs(turningRadius);

            for (int i = 0; i < _sectionSpeeds.Length; i++)
            {
                // Calculate section radius from turn center
                // For right turn (positive radius): left sections have smaller radius, right sections larger
                // For left turn (negative radius): right sections have smaller radius, left sections larger
                double sectionRadius = absTurningRadius + (turningRadius > 0 ? _sectionOffsets[i] : -_sectionOffsets[i]);

                // Clamp to zero if section radius becomes negative (very tight inside turn)
                if (sectionRadius < 0)
                {
                    _sectionSpeeds[i] = 0.0;
                }
                else
                {
                    // Speed proportional to radius
                    _sectionSpeeds[i] = vehicleSpeed * (sectionRadius / absTurningRadius);
                }

                // Raise event for significant speed changes
                RaiseSectionSpeedChanged(i, _sectionSpeeds[i]);
            }
        }
    }

    public double GetSectionSpeed(int sectionId)
    {
        lock (_lockObject)
        {
            if (sectionId < 0 || sectionId >= _sectionSpeeds.Length)
                throw new ArgumentOutOfRangeException(nameof(sectionId),
                    $"Section ID must be between 0 and {_sectionSpeeds.Length - 1}");

            return _sectionSpeeds[sectionId];
        }
    }

    public double[] GetAllSectionSpeeds()
    {
        lock (_lockObject)
        {
            var speeds = new double[_sectionSpeeds.Length];
            Array.Copy(_sectionSpeeds, speeds, _sectionSpeeds.Length);
            return speeds;
        }
    }

    private void OnConfigurationChanged(object? sender, EventArgs e)
    {
        UpdateSectionOffsets();
    }

    private void UpdateSectionOffsets()
    {
        lock (_lockObject)
        {
            var config = _configService.GetConfiguration();
            int sectionCount = config.SectionCount;

            _sectionSpeeds = new double[sectionCount];
            _sectionOffsets = new double[sectionCount];

            // Calculate lateral offsets for each section from vehicle centerline
            // Sections are numbered left to right, with center at implement centerline
            double totalWidth = config.TotalWidth;
            double currentOffset = -totalWidth / 2.0; // Start at left edge

            for (int i = 0; i < sectionCount; i++)
            {
                double sectionWidth = config.SectionWidths[i];
                // Section center offset = current position + half section width
                _sectionOffsets[i] = currentOffset + (sectionWidth / 2.0);
                currentOffset += sectionWidth;
            }
        }
    }

    private void RaiseSectionSpeedChanged(int sectionId, double speed)
    {
        SectionSpeedChanged?.Invoke(this, new SectionSpeedChangedEventArgs(sectionId, speed));
    }
}
