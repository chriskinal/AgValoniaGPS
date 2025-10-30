using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Interface for calculating section boundary points and generating coverage triangles
/// from vehicle position, heading, and section configuration.
/// </summary>
/// <remarks>
/// Responsible for geometric calculations that convert vehicle state + section configuration
/// into left/right boundary points for each section, which are then used to generate
/// coverage triangles in a triangle strip pattern.
///
/// Performance Target: Less than 1ms for all sections (typically 5-31 sections)
/// </remarks>
public interface ISectionGeometryService
{
    /// <summary>
    /// Calculates the left and right boundary points for a specific section
    /// based on vehicle position, heading, and section configuration.
    /// </summary>
    /// <param name="sectionId">Zero-based section ID</param>
    /// <param name="vehiclePosition">Current vehicle position (UTM coordinates)</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians</param>
    /// <param name="sectionWidth">Width of the section in meters</param>
    /// <param name="sectionOffset">Lateral offset from vehicle centerline in meters</param>
    /// <returns>Tuple of (leftPoint, rightPoint) in UTM coordinates</returns>
    (Position leftPoint, Position rightPoint) CalculateSectionBoundaryPoints(
        int sectionId,
        Position vehiclePosition,
        double vehicleHeading,
        double sectionWidth,
        double sectionOffset);

    /// <summary>
    /// Calculates boundary points for all active sections at once.
    /// More efficient than calling CalculateSectionBoundaryPoints repeatedly.
    /// </summary>
    /// <param name="vehiclePosition">Current vehicle position (UTM coordinates)</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians</param>
    /// <returns>Array of (leftPoint, rightPoint) tuples, indexed by section ID</returns>
    (Position leftPoint, Position rightPoint)[] CalculateAllSectionBoundaryPoints(
        Position vehiclePosition,
        double vehicleHeading);
}
