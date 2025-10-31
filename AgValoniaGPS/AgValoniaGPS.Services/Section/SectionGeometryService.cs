using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Implements section boundary point calculation for coverage triangle generation.
/// Converts vehicle position, heading, and section configuration into left/right
/// boundary points for each section.
/// </summary>
/// <remarks>
/// Algorithm:
/// 1. Calculate perpendicular vector to vehicle heading (left direction)
/// 2. For each section:
///    - Calculate section center offset from vehicle centerline
///    - Calculate left edge = center - (width/2)
///    - Calculate right edge = center + (width/2)
///    - Apply perpendicular offset to vehicle position
///
/// Performance: <1ms for typical configurations (5-31 sections)
/// Thread Safety: Stateless service, safe for concurrent use
/// </remarks>
public class SectionGeometryService : ISectionGeometryService
{
    private readonly ISectionConfigurationService _configService;

    /// <summary>
    /// Creates a new SectionGeometryService with required dependencies
    /// </summary>
    /// <param name="configService">Section configuration service</param>
    public SectionGeometryService(ISectionConfigurationService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// Calculates the left and right boundary points for a specific section
    /// based on vehicle position, heading, and section configuration.
    /// </summary>
    /// <param name="sectionId">Zero-based section ID</param>
    /// <param name="vehiclePosition">Current vehicle position (UTM coordinates)</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians</param>
    /// <param name="sectionWidth">Width of the section in meters</param>
    /// <param name="sectionOffset">Lateral offset from vehicle centerline in meters (negative=left, positive=right)</param>
    /// <returns>Tuple of (leftPoint, rightPoint) in UTM coordinates</returns>
    public (Position leftPoint, Position rightPoint) CalculateSectionBoundaryPoints(
        int sectionId,
        Position vehiclePosition,
        double vehicleHeading,
        double sectionWidth,
        double sectionOffset)
    {
        if (vehiclePosition == null)
            throw new ArgumentNullException(nameof(vehiclePosition));

        if (sectionWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(sectionWidth), "Section width must be positive");

        // Calculate perpendicular vector to heading (points to the left of vehicle)
        // Heading is in radians, 0 = North, PI/2 = East
        // Perpendicular to the left is heading - PI/2
        double perpHeading = vehicleHeading - Math.PI / 2.0;
        double cosPerp = Math.Cos(perpHeading);
        double sinPerp = Math.Sin(perpHeading);

        // Calculate left and right edge offsets from centerline
        double leftEdgeOffset = sectionOffset - (sectionWidth / 2.0);
        double rightEdgeOffset = sectionOffset + (sectionWidth / 2.0);

        // Calculate left point: vehicle position + perpendicular offset
        Position leftPoint = new Position
        {
            Latitude = vehiclePosition.Latitude,
            Longitude = vehiclePosition.Longitude,
            Altitude = vehiclePosition.Altitude,
            Easting = vehiclePosition.Easting + (leftEdgeOffset * cosPerp),
            Northing = vehiclePosition.Northing + (leftEdgeOffset * sinPerp),
            Zone = vehiclePosition.Zone,
            Hemisphere = vehiclePosition.Hemisphere
        };

        // Calculate right point: vehicle position + perpendicular offset
        Position rightPoint = new Position
        {
            Latitude = vehiclePosition.Latitude,
            Longitude = vehiclePosition.Longitude,
            Altitude = vehiclePosition.Altitude,
            Easting = vehiclePosition.Easting + (rightEdgeOffset * cosPerp),
            Northing = vehiclePosition.Northing + (rightEdgeOffset * sinPerp),
            Zone = vehiclePosition.Zone,
            Hemisphere = vehiclePosition.Hemisphere
        };

        return (leftPoint, rightPoint);
    }

    /// <summary>
    /// Calculates boundary points for all active sections at once.
    /// More efficient than calling CalculateSectionBoundaryPoints repeatedly.
    /// </summary>
    /// <param name="vehiclePosition">Current vehicle position (UTM coordinates)</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians</param>
    /// <returns>Array of (leftPoint, rightPoint) tuples, indexed by section ID</returns>
    public (Position leftPoint, Position rightPoint)[] CalculateAllSectionBoundaryPoints(
        Position vehiclePosition,
        double vehicleHeading)
    {
        if (vehiclePosition == null)
            throw new ArgumentNullException(nameof(vehiclePosition));

        // Get configuration
        var config = _configService.GetConfiguration();
        int sectionCount = config.SectionCount;

        // Pre-calculate perpendicular vector (shared by all sections)
        double perpHeading = vehicleHeading - Math.PI / 2.0;
        double cosPerp = Math.Cos(perpHeading);
        double sinPerp = Math.Sin(perpHeading);

        // Allocate result array
        var boundaryPoints = new (Position leftPoint, Position rightPoint)[sectionCount];

        // Calculate boundary points for each section
        for (int i = 0; i < sectionCount; i++)
        {
            double sectionWidth = config.SectionWidths[i];
            double sectionOffset = _configService.GetSectionOffset(i);

            // Calculate left and right edge offsets from centerline
            double leftEdgeOffset = sectionOffset - (sectionWidth / 2.0);
            double rightEdgeOffset = sectionOffset + (sectionWidth / 2.0);

            // Calculate left point
            Position leftPoint = new Position
            {
                Latitude = vehiclePosition.Latitude,
                Longitude = vehiclePosition.Longitude,
                Altitude = vehiclePosition.Altitude,
                Easting = vehiclePosition.Easting + (leftEdgeOffset * cosPerp),
                Northing = vehiclePosition.Northing + (leftEdgeOffset * sinPerp),
                Zone = vehiclePosition.Zone,
                Hemisphere = vehiclePosition.Hemisphere
            };

            // Calculate right point
            Position rightPoint = new Position
            {
                Latitude = vehiclePosition.Latitude,
                Longitude = vehiclePosition.Longitude,
                Altitude = vehiclePosition.Altitude,
                Easting = vehiclePosition.Easting + (rightEdgeOffset * cosPerp),
                Northing = vehiclePosition.Northing + (rightEdgeOffset * sinPerp),
                Zone = vehiclePosition.Zone,
                Hemisphere = vehiclePosition.Hemisphere
            };

            boundaryPoints[i] = (leftPoint, rightPoint);
        }

        return boundaryPoints;
    }

    /// <summary>
    /// Generates section overlay mesh for OpenGL rendering (Wave 11).
    /// Creates colored rectangles showing section ON/OFF states.
    /// </summary>
    public float[] GenerateSectionOverlayMesh(
        Position vehiclePosition,
        double vehicleHeading,
        SectionConfiguration sectionConfig,
        ToolSettings toolSettings,
        bool[] sectionStates)
    {
        if (vehiclePosition == null)
            throw new ArgumentNullException(nameof(vehiclePosition));

        if (sectionConfig == null)
            throw new ArgumentNullException(nameof(sectionConfig));

        if (toolSettings == null)
            throw new ArgumentNullException(nameof(toolSettings));

        if (sectionStates == null)
            throw new ArgumentNullException(nameof(sectionStates));

        int sectionCount = sectionConfig.SectionCount;

        if (sectionStates.Length < sectionCount)
            throw new ArgumentException("Section states array must match section count", nameof(sectionStates));

        // Each section = 2 triangles = 6 vertices
        // Each vertex = 6 floats (X, Y, R, G, B, A)
        var vertices = new float[sectionCount * 6 * 6];
        int index = 0;

        // Calculate perpendicular vector (points left of vehicle)
        double perpHeading = vehicleHeading - Math.PI / 2.0;
        double cosPerp = Math.Cos(perpHeading);
        double sinPerp = Math.Sin(perpHeading);

        // Vehicle length for rectangle depth
        float vehicleLength = 3.0f; // Standard depth in meters
        float frontEdge = vehicleLength / 2.0f;
        float backEdge = -vehicleLength / 2.0f;

        // Calculate forward vector for positioning rectangles
        double cosHeading = Math.Cos(vehicleHeading);
        double sinHeading = Math.Sin(vehicleHeading);

        for (int i = 0; i < sectionCount; i++)
        {
            double sectionWidth = sectionConfig.SectionWidths[i];
            double sectionOffset = _configService.GetSectionOffset(i);

            // Calculate left and right edges
            double leftEdgeOffset = sectionOffset - (sectionWidth / 2.0);
            double rightEdgeOffset = sectionOffset + (sectionWidth / 2.0);

            // Get color based on section state
            var (r, g, b, a) = sectionStates[i]
                ? (0.0f, 1.0f, 0.0f, 0.5f)  // Green with 50% alpha for ON
                : (1.0f, 0.0f, 0.0f, 0.3f); // Red with 30% alpha for OFF

            // Calculate 4 corner points of the rectangle
            // Front-left
            double flX = vehiclePosition.Easting + (leftEdgeOffset * cosPerp) + (frontEdge * cosHeading);
            double flY = vehiclePosition.Northing + (leftEdgeOffset * sinPerp) + (frontEdge * sinHeading);

            // Front-right
            double frX = vehiclePosition.Easting + (rightEdgeOffset * cosPerp) + (frontEdge * cosHeading);
            double frY = vehiclePosition.Northing + (rightEdgeOffset * sinPerp) + (frontEdge * sinHeading);

            // Back-left
            double blX = vehiclePosition.Easting + (leftEdgeOffset * cosPerp) + (backEdge * cosHeading);
            double blY = vehiclePosition.Northing + (leftEdgeOffset * sinPerp) + (backEdge * sinHeading);

            // Back-right
            double brX = vehiclePosition.Easting + (rightEdgeOffset * cosPerp) + (backEdge * cosHeading);
            double brY = vehiclePosition.Northing + (rightEdgeOffset * sinPerp) + (backEdge * sinHeading);

            // Triangle 1: Front-left, Back-left, Back-right
            AddVertex(ref vertices, ref index, flX, flY, r, g, b, a);
            AddVertex(ref vertices, ref index, blX, blY, r, g, b, a);
            AddVertex(ref vertices, ref index, brX, brY, r, g, b, a);

            // Triangle 2: Front-left, Back-right, Front-right
            AddVertex(ref vertices, ref index, flX, flY, r, g, b, a);
            AddVertex(ref vertices, ref index, brX, brY, r, g, b, a);
            AddVertex(ref vertices, ref index, frX, frY, r, g, b, a);
        }

        return vertices;
    }

    /// <summary>
    /// Helper method to add a vertex to the vertex array.
    /// </summary>
    private void AddVertex(ref float[] vertices, ref int index, double x, double y, float r, float g, float b, float a)
    {
        vertices[index++] = (float)x;
        vertices[index++] = (float)y;
        vertices[index++] = r;
        vertices[index++] = g;
        vertices[index++] = b;
        vertices[index++] = a;
    }
}
