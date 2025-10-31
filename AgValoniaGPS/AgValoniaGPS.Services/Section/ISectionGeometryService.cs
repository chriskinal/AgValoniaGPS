using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Section;

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

    /// <summary>
    /// Generates section overlay mesh for OpenGL rendering (Wave 11).
    /// Creates colored rectangles showing section ON/OFF states.
    /// </summary>
    /// <param name="vehiclePosition">Current vehicle position</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians</param>
    /// <param name="sectionConfig">Section configuration with widths</param>
    /// <param name="toolSettings">Tool settings with width and offset</param>
    /// <param name="sectionStates">Array of section ON/OFF states</param>
    /// <returns>Interleaved vertex array: [X, Y, R, G, B, A, ...] (6 vertices per section)</returns>
    /// <remarks>
    /// Output format: Vertex array with XYZRGBA interleaved
    /// Each section = 2 triangles = 6 vertices
    /// Colors: Green (0, 1, 0, 0.5) for ON, Red (1, 0, 0, 0.3) for OFF
    /// </remarks>
    float[] GenerateSectionOverlayMesh(
        Position vehiclePosition,
        double vehicleHeading,
        SectionConfiguration sectionConfig,
        ToolSettings toolSettings,
        bool[] sectionStates);
}
