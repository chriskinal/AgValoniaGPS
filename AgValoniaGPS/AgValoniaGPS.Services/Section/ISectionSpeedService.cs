using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Calculates individual section speeds based on vehicle turning radius and section offset.
/// Provides differential speed calculations for sections during turning maneuvers.
/// </summary>
/// <remarks>
/// <para>
/// During turns, sections on the inside of the turn move slower than the vehicle centerline,
/// while sections on the outside move faster. Speed is proportional to the section's distance
/// from the turn center.
/// </para>
/// <para>
/// Thread-safe: All methods use lock-based synchronization.
/// Performance: &lt;1ms for all sections (31 sections maximum).
/// </para>
/// </remarks>
public interface ISectionSpeedService
{
    /// <summary>
    /// Raised when section speeds have been recalculated.
    /// </summary>
    event EventHandler<SectionSpeedChangedEventArgs>? SectionSpeedChanged;

    /// <summary>
    /// Calculates individual section speeds based on vehicle kinematics and section configuration.
    /// </summary>
    /// <param name="vehicleSpeed">Vehicle centerline speed in meters per second</param>
    /// <param name="turningRadius">Vehicle turning radius in meters (positive = right turn, negative = left turn, >1000 = straight)</param>
    /// <param name="heading">Vehicle heading in radians</param>
    /// <remarks>
    /// <para>
    /// Algorithm:
    /// - Straight-line (|radius| > 1000m): All sections = vehicle speed
    /// - Turning: Section speed = vehicle speed × (section radius / |turning radius|)
    /// - Section radius = |turning radius| + section center offset
    /// - Left sections (negative offset) slower on right turn, faster on left turn
    /// - Right sections (positive offset) faster on right turn, slower on left turn
    /// - Clamp to 0 if section radius becomes negative (tight inside turn)
    /// </para>
    /// <para>
    /// Performance target: &lt;1ms for all 31 sections
    /// </para>
    /// </remarks>
    void CalculateSectionSpeeds(double vehicleSpeed, double turningRadius, double heading);

    /// <summary>
    /// Gets the current speed for a specific section.
    /// </summary>
    /// <param name="sectionId">Section identifier (0-based)</param>
    /// <returns>Section speed in meters per second</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    double GetSectionSpeed(int sectionId);

    /// <summary>
    /// Gets all section speeds.
    /// </summary>
    /// <returns>Array of section speeds indexed by section ID</returns>
    double[] GetAllSectionSpeeds();
}
