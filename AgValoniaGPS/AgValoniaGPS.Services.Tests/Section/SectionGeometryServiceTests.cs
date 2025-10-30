using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Section;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Section;

[TestFixture]
public class SectionGeometryServiceTests
{
    private ISectionConfigurationService _configService = null!;
    private ISectionGeometryService _geometryService = null!;

    [SetUp]
    public void Setup()
    {
        _configService = new SectionConfigurationService();
        _geometryService = new SectionGeometryService(_configService);

        // Setup default configuration: 5 sections @ 2.5m each = 12.5m total width
        var config = new SectionConfiguration(5, new double[] { 2.5, 2.5, 2.5, 2.5, 2.5 });
        _configService.LoadConfiguration(config);
    }

    [Test]
    public void Constructor_WithNullConfigService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.That(() => new SectionGeometryService(null!),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("configService"));
    }

    [Test]
    public void CalculateSectionBoundaryPoints_WithNullPosition_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.That(() => _geometryService.CalculateSectionBoundaryPoints(0, null!, Math.PI / 4, 2.5, 0.0),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("vehiclePosition"));
    }

    [Test]
    public void CalculateSectionBoundaryPoints_WithNegativeWidth_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };

        // Act & Assert
        Assert.That(() => _geometryService.CalculateSectionBoundaryPoints(0, position, 0, -1.0, 0.0),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("sectionWidth"));
    }

    [Test]
    public void CalculateSectionBoundaryPoints_HeadingNorth_CalculatesCorrectPoints()
    {
        // Arrange
        var position = new Position
        {
            Easting = 500000,
            Northing = 4500000,
            Zone = 33,
            Hemisphere = 'N'
        };
        double heading = 0.0; // North
        double sectionWidth = 2.5;
        double sectionOffset = 0.0; // Center section

        // Act
        var (leftPoint, rightPoint) = _geometryService.CalculateSectionBoundaryPoints(
            0, position, heading, sectionWidth, sectionOffset);

        // Assert
        // When heading north (0 rad), perpendicular left is west (negative easting)
        Assert.That(leftPoint.Easting, Is.EqualTo(position.Easting - 1.25).Within(0.001), "Left point easting");
        Assert.That(leftPoint.Northing, Is.EqualTo(position.Northing).Within(0.001), "Left point northing");

        Assert.That(rightPoint.Easting, Is.EqualTo(position.Easting + 1.25).Within(0.001), "Right point easting");
        Assert.That(rightPoint.Northing, Is.EqualTo(position.Northing).Within(0.001), "Right point northing");

        // Width between points should equal section width
        double width = Math.Sqrt(Math.Pow(rightPoint.Easting - leftPoint.Easting, 2) +
                                  Math.Pow(rightPoint.Northing - leftPoint.Northing, 2));
        Assert.That(width, Is.EqualTo(2.5).Within(0.001), "Section width");
    }

    [Test]
    public void CalculateSectionBoundaryPoints_HeadingEast_CalculatesCorrectPoints()
    {
        // Arrange
        var position = new Position
        {
            Easting = 500000,
            Northing = 4500000,
            Zone = 33,
            Hemisphere = 'N'
        };
        double heading = Math.PI / 2; // East
        double sectionWidth = 2.5;
        double sectionOffset = 0.0;

        // Act
        var (leftPoint, rightPoint) = _geometryService.CalculateSectionBoundaryPoints(
            0, position, heading, sectionWidth, sectionOffset);

        // Assert
        // When heading east (PI/2), perpendicular left is north (positive northing)
        Assert.That(leftPoint.Easting, Is.EqualTo(position.Easting).Within(0.001), "Left point easting");
        Assert.That(leftPoint.Northing, Is.EqualTo(position.Northing + 1.25).Within(0.001), "Left point northing");

        Assert.That(rightPoint.Easting, Is.EqualTo(position.Easting).Within(0.001), "Right point easting");
        Assert.That(rightPoint.Northing, Is.EqualTo(position.Northing - 1.25).Within(0.001), "Right point northing");

        double width = Math.Sqrt(Math.Pow(rightPoint.Easting - leftPoint.Easting, 2) +
                                  Math.Pow(rightPoint.Northing - leftPoint.Northing, 2));
        Assert.That(width, Is.EqualTo(2.5).Within(0.001), "Section width");
    }

    [Test]
    public void CalculateSectionBoundaryPoints_WithPositiveOffset_ShiftsPointsRight()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = 0.0; // North
        double sectionWidth = 2.5;
        double sectionOffset = 3.0; // 3m to the right

        // Act
        var (leftPoint, rightPoint) = _geometryService.CalculateSectionBoundaryPoints(
            0, position, heading, sectionWidth, sectionOffset);

        // Assert
        // Offset should shift both points to the right (positive easting when heading north)
        Assert.That(leftPoint.Easting, Is.EqualTo(position.Easting + 3.0 - 1.25).Within(0.001));
        Assert.That(rightPoint.Easting, Is.EqualTo(position.Easting + 3.0 + 1.25).Within(0.001));
    }

    [Test]
    public void CalculateSectionBoundaryPoints_WithNegativeOffset_ShiftsPointsLeft()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = 0.0; // North
        double sectionWidth = 2.5;
        double sectionOffset = -3.0; // 3m to the left

        // Act
        var (leftPoint, rightPoint) = _geometryService.CalculateSectionBoundaryPoints(
            0, position, heading, sectionWidth, sectionOffset);

        // Assert
        Assert.That(leftPoint.Easting, Is.EqualTo(position.Easting - 3.0 - 1.25).Within(0.001));
        Assert.That(rightPoint.Easting, Is.EqualTo(position.Easting - 3.0 + 1.25).Within(0.001));
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_WithNullPosition_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.That(() => _geometryService.CalculateAllSectionBoundaryPoints(null!, 0.0),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("vehiclePosition"));
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_ReturnsCorrectCount()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;

        // Act
        var boundaryPoints = _geometryService.CalculateAllSectionBoundaryPoints(position, heading);

        // Assert
        Assert.That(boundaryPoints.Length, Is.EqualTo(5), "Should return boundary points for 5 sections");
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_SectionsAreSymmetricallyArranged()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = 0.0; // North

        // Act
        var boundaryPoints = _geometryService.CalculateAllSectionBoundaryPoints(position, heading);

        // Assert
        // With 5 sections @ 2.5m each, total width = 12.5m
        // Sections should span from -6.25m (left) to +6.25m (right)
        // Section 0 (leftmost): left = -6.25, right = -3.75
        // Section 2 (center): left = -1.25, right = +1.25
        // Section 4 (rightmost): left = +3.75, right = +6.25

        var (leftmost, _) = boundaryPoints[0];
        var (_, rightmost) = boundaryPoints[4];

        Assert.That(leftmost.Easting, Is.EqualTo(500000 - 6.25).Within(0.001), "Leftmost section left edge");
        Assert.That(rightmost.Easting, Is.EqualTo(500000 + 6.25).Within(0.001), "Rightmost section right edge");

        // Check center section is centered
        var (centerLeft, centerRight) = boundaryPoints[2];
        double centerOffset = (centerLeft.Easting + centerRight.Easting) / 2.0;
        Assert.That(centerOffset, Is.EqualTo(500000).Within(0.001), "Center section should be centered on vehicle");
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_SectionsDoNotOverlap()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;

        // Act
        var boundaryPoints = _geometryService.CalculateAllSectionBoundaryPoints(position, heading);

        // Assert
        // Each section's right edge should match the next section's left edge (adjacent, no gaps)
        for (int i = 0; i < boundaryPoints.Length - 1; i++)
        {
            var (_, rightEdge) = boundaryPoints[i];
            var (leftEdge, _) = boundaryPoints[i + 1];

            Assert.That(rightEdge.Easting, Is.EqualTo(leftEdge.Easting).Within(0.001),
                $"Section {i} right edge should match section {i + 1} left edge");
        }
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_WithVariableWidths_CalculatesCorrectly()
    {
        // Arrange
        var config = new SectionConfiguration(3, new double[] { 1.0, 3.0, 2.0 }); // Total = 6.0m
        _configService.LoadConfiguration(config);

        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;

        // Act
        var boundaryPoints = _geometryService.CalculateAllSectionBoundaryPoints(position, heading);

        // Assert
        Assert.That(boundaryPoints.Length, Is.EqualTo(3));

        // Section 0: 1m wide, centered at -2.5m, spans -3.0 to -2.0
        var (left0, right0) = boundaryPoints[0];
        Assert.That(left0.Easting, Is.EqualTo(500000 - 3.0).Within(0.001));
        Assert.That(right0.Easting, Is.EqualTo(500000 - 2.0).Within(0.001));

        // Section 1: 3m wide, centered at -0.5m, spans -2.0 to +1.0
        var (left1, right1) = boundaryPoints[1];
        Assert.That(left1.Easting, Is.EqualTo(500000 - 2.0).Within(0.001));
        Assert.That(right1.Easting, Is.EqualTo(500000 + 1.0).Within(0.001));

        // Section 2: 2m wide, centered at +2.0m, spans +1.0 to +3.0
        var (left2, right2) = boundaryPoints[2];
        Assert.That(left2.Easting, Is.EqualTo(500000 + 1.0).Within(0.001));
        Assert.That(right2.Easting, Is.EqualTo(500000 + 3.0).Within(0.001));
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_Performance_UnderOneMillisecond()
    {
        // Arrange
        var config = new SectionConfiguration(31, Enumerable.Repeat(2.5, 31).ToArray()); // Max sections
        _configService.LoadConfiguration(config);

        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = Math.PI / 4;

        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _geometryService.CalculateAllSectionBoundaryPoints(position, heading);
        }
        sw.Stop();

        double avgTimeMs = sw.Elapsed.TotalMilliseconds / 100.0;

        // Assert
        Assert.That(avgTimeMs, Is.LessThan(1.0), $"Average time: {avgTimeMs:F3}ms");
    }

    [Test]
    public void CalculateSectionBoundaryPoints_PreservesZoneAndHemisphere()
    {
        // Arrange
        var position = new Position
        {
            Easting = 500000,
            Northing = 4500000,
            Zone = 17,
            Hemisphere = 'S',
            Latitude = -45.0,
            Longitude = 170.0,
            Altitude = 123.45
        };

        // Act
        var (leftPoint, rightPoint) = _geometryService.CalculateSectionBoundaryPoints(
            0, position, 0.0, 2.5, 0.0);

        // Assert
        Assert.That(leftPoint.Zone, Is.EqualTo(17));
        Assert.That(leftPoint.Hemisphere, Is.EqualTo('S'));
        Assert.That(rightPoint.Zone, Is.EqualTo(17));
        Assert.That(rightPoint.Hemisphere, Is.EqualTo('S'));
    }

    [Test]
    public void CalculateAllSectionBoundaryPoints_Heading45Degrees_CalculatesCorrectly()
    {
        // Arrange
        var position = new Position { Easting = 500000, Northing = 4500000 };
        double heading = Math.PI / 4; // 45 degrees (NE)

        // Act
        var boundaryPoints = _geometryService.CalculateAllSectionBoundaryPoints(position, heading);

        // Assert
        // Verify center section points are correctly offset perpendicular to heading
        var (centerLeft, centerRight) = boundaryPoints[2];

        // Perpendicular to 45 deg is -45 deg (NW direction for left)
        double perpHeading = heading - Math.PI / 2; // -45 degrees
        double expectedOffsetX = 1.25 * Math.Cos(perpHeading);
        double expectedOffsetY = 1.25 * Math.Sin(perpHeading);

        Assert.That(centerLeft.Easting, Is.EqualTo(position.Easting - expectedOffsetX).Within(0.001));
        Assert.That(centerLeft.Northing, Is.EqualTo(position.Northing - expectedOffsetY).Within(0.001));
        Assert.That(centerRight.Easting, Is.EqualTo(position.Easting + expectedOffsetX).Within(0.001));
        Assert.That(centerRight.Northing, Is.EqualTo(position.Northing + expectedOffsetY).Within(0.001));
    }
}
