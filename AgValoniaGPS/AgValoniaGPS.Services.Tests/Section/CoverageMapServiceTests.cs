using Xunit;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Section;

namespace AgValoniaGPS.Services.Tests.Section;

/// <summary>
/// Tests for CoverageMapService
/// Focus: Triangle generation, overlap detection, area calculation
/// </summary>
public class CoverageMapServiceTests
{
    [Fact]
    public void AddCoverageTriangles_SingleTriangle_StoresAndCalculatesArea()
    {
        // Arrange
        var service = new CoverageMapService();
        var triangle = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);

        // Act
        service.AddCoverageTriangles(new[] { triangle });

        // Assert
        var triangles = service.GetAllTriangles();
        Assert.Single(triangles);
        Assert.True(service.GetTotalCoveredArea() > 0);
    }

    [Fact]
    public void AddCoverageTriangles_FiresEvent_WithCorrectData()
    {
        // Arrange
        var service = new CoverageMapService();
        CoverageMapUpdatedEventArgs? eventArgs = null;
        service.CoverageUpdated += (sender, args) => eventArgs = args;

        var triangle = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);

        // Act
        service.AddCoverageTriangles(new[] { triangle });

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal(1, eventArgs.AddedTrianglesCount);
        Assert.True(eventArgs.TotalCoveredArea > 0);
    }

    [Fact]
    public void GetCoverageAt_PointInsideTriangle_ReturnsOverlapCount()
    {
        // Arrange
        var service = new CoverageMapService();
        var triangle = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);
        service.AddCoverageTriangles(new[] { triangle });

        var pointInside = new Position { Easting = 5, Northing = 3 };

        // Act
        int coverage = service.GetCoverageAt(pointInside);

        // Assert
        Assert.Equal(1, coverage);
    }

    [Fact]
    public void GetCoverageAt_PointOutsideTriangle_ReturnsZero()
    {
        // Arrange
        var service = new CoverageMapService();
        var triangle = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);
        service.AddCoverageTriangles(new[] { triangle });

        var pointOutside = new Position { Easting = 100, Northing = 100 };

        // Act
        int coverage = service.GetCoverageAt(pointOutside);

        // Assert
        Assert.Equal(0, coverage);
    }

    [Fact]
    public void AddCoverageTriangles_OverlappingTriangles_DetectsOverlap()
    {
        // Arrange
        var service = new CoverageMapService();

        // First pass
        var triangle1 = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);
        service.AddCoverageTriangles(new[] { triangle1 });

        // Second pass - overlapping area
        var triangle2 = CreateTriangle(2, 2, 12, 2, 7, 12, sectionId: 0);

        // Act
        service.AddCoverageTriangles(new[] { triangle2 });

        // Assert
        var triangles = service.GetAllTriangles();
        Assert.Equal(2, triangles.Count);

        // Second triangle should have overlap count > 1
        Assert.True(triangles.Any(t => t.OverlapCount > 1));
    }

    [Fact]
    public void GetOverlapStatistics_MultipleTriangles_CalculatesCorrectly()
    {
        // Arrange
        var service = new CoverageMapService();

        var triangle1 = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);
        var triangle2 = CreateTriangle(20, 0, 30, 0, 25, 10, sectionId: 0); // No overlap

        service.AddCoverageTriangles(new[] { triangle1, triangle2 });

        // Act
        var stats = service.GetOverlapStatistics();

        // Assert
        Assert.True(stats.ContainsKey(1)); // Single pass coverage
        Assert.True(stats[1] > 0); // Has some area
    }

    [Fact]
    public void Clear_RemovesAllData_ResetsToZero()
    {
        // Arrange
        var service = new CoverageMapService();
        var triangle = CreateTriangle(0, 0, 10, 0, 5, 10, sectionId: 0);
        service.AddCoverageTriangles(new[] { triangle });

        // Act
        service.Clear();

        // Assert
        Assert.Empty(service.GetAllTriangles());
        Assert.Equal(0, service.GetTotalCoveredArea());
    }

    private CoverageTriangle CreateTriangle(
        double e1, double n1,
        double e2, double n2,
        double e3, double n3,
        int sectionId)
    {
        var v1 = new Position { Easting = e1, Northing = n1 };
        var v2 = new Position { Easting = e2, Northing = n2 };
        var v3 = new Position { Easting = e3, Northing = n3 };

        return new CoverageTriangle(v1, v2, v3, sectionId);
    }
}
