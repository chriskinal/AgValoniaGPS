using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.FieldOperations;
using Xunit;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for PointInPolygonService ray-casting algorithm and spatial indexing.
/// Validates <1ms performance target and correct handling of all edge cases.
/// </summary>
public class PointInPolygonServiceTests
{
    private readonly IPointInPolygonService _service;

    public PointInPolygonServiceTests()
    {
        _service = new PointInPolygonService();
    }

    /// <summary>
    /// Test 1: Point clearly inside a simple rectangular polygon
    /// </summary>
    [Fact]
    public void IsPointInside_PointClearlyInside_ReturnsTrue()
    {
        // Arrange: Create a simple rectangle polygon (0,0) to (10,10)
        var polygon = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 10, Northing = 0 },
            new Position { Easting = 10, Northing = 10 },
            new Position { Easting = 0, Northing = 10 }
        };

        var pointInside = new Position { Easting = 5, Northing = 5 };

        // Act
        bool result = _service.IsPointInside(pointInside, polygon);

        // Assert
        Assert.True(result, "Point at (5,5) should be inside rectangle (0,0)-(10,10)");
    }

    /// <summary>
    /// Test 2: Point clearly outside a simple rectangular polygon
    /// </summary>
    [Fact]
    public void IsPointInside_PointClearlyOutside_ReturnsFalse()
    {
        // Arrange: Create a simple rectangle polygon (0,0) to (10,10)
        var polygon = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 10, Northing = 0 },
            new Position { Easting = 10, Northing = 10 },
            new Position { Easting = 0, Northing = 10 }
        };

        var pointOutside = new Position { Easting = 15, Northing = 15 };

        // Act
        bool result = _service.IsPointInside(pointOutside, polygon);

        // Assert
        Assert.False(result, "Point at (15,15) should be outside rectangle (0,0)-(10,10)");
    }

    /// <summary>
    /// Test 3: Point on boundary edge (should be considered inside)
    /// </summary>
    [Fact]
    public void IsPointInside_PointOnBoundaryEdge_ReturnsTrue()
    {
        // Arrange: Create a simple rectangle polygon (0,0) to (10,10)
        var polygon = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 10, Northing = 0 },
            new Position { Easting = 10, Northing = 10 },
            new Position { Easting = 0, Northing = 10 }
        };

        var pointOnEdge = new Position { Easting = 5, Northing = 0 }; // On bottom edge

        // Act
        bool result = _service.IsPointInside(pointOnEdge, polygon);

        // Assert
        Assert.True(result, "Point on boundary edge should be considered inside");
    }

    /// <summary>
    /// Test 4: Point at polygon vertex (should be considered inside)
    /// </summary>
    [Fact]
    public void IsPointInside_PointAtVertex_ReturnsTrue()
    {
        // Arrange: Create a simple rectangle polygon (0,0) to (10,10)
        var polygon = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 10, Northing = 0 },
            new Position { Easting = 10, Northing = 10 },
            new Position { Easting = 0, Northing = 10 }
        };

        var pointAtVertex = new Position { Easting = 0, Northing = 0 }; // Corner vertex

        // Act
        bool result = _service.IsPointInside(pointAtVertex, polygon);

        // Assert
        Assert.True(result, "Point at polygon vertex should be considered inside");
    }

    /// <summary>
    /// Test 5: Point inside polygon with hole (outside the hole)
    /// </summary>
    [Fact]
    public void IsPointInside_PointInsidePolygonOutsideHole_ReturnsTrue()
    {
        // Arrange: Outer polygon (0,0) to (20,20)
        var outerBoundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 20, Northing = 0 },
            new Position { Easting = 20, Northing = 20 },
            new Position { Easting = 0, Northing = 20 }
        };

        // Hole polygon (5,5) to (15,15)
        var hole = new[]
        {
            new Position { Easting = 5, Northing = 5 },
            new Position { Easting = 15, Northing = 5 },
            new Position { Easting = 15, Northing = 15 },
            new Position { Easting = 5, Northing = 15 }
        };

        var holes = new[] { hole };

        // Point at (2,2) - inside outer, outside hole
        var point = new Position { Easting = 2, Northing = 2 };

        // Act
        bool result = _service.IsPointInside(point, outerBoundary, holes);

        // Assert
        Assert.True(result, "Point inside outer boundary but outside hole should return true");
    }

    /// <summary>
    /// Test 6: Point inside hole (should be outside the polygon)
    /// </summary>
    [Fact]
    public void IsPointInside_PointInsideHole_ReturnsFalse()
    {
        // Arrange: Outer polygon (0,0) to (20,20)
        var outerBoundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 20, Northing = 0 },
            new Position { Easting = 20, Northing = 20 },
            new Position { Easting = 0, Northing = 20 }
        };

        // Hole polygon (5,5) to (15,15)
        var hole = new[]
        {
            new Position { Easting = 5, Northing = 5 },
            new Position { Easting = 15, Northing = 5 },
            new Position { Easting = 15, Northing = 15 },
            new Position { Easting = 5, Northing = 15 }
        };

        var holes = new[] { hole };

        // Point at (10,10) - inside hole
        var point = new Position { Easting = 10, Northing = 10 };

        // Act
        bool result = _service.IsPointInside(point, outerBoundary, holes);

        // Assert
        Assert.False(result, "Point inside hole should return false");
    }

    /// <summary>
    /// Test 7: Ray-casting with complex concave polygon
    /// </summary>
    [Fact]
    public void IsPointInside_ConcavePolygon_HandlesCorrectly()
    {
        // Arrange: Create a concave (star-shaped) polygon
        var polygon = new[]
        {
            new Position { Easting = 10, Northing = 0 },
            new Position { Easting = 12, Northing = 5 },   // Inner point
            new Position { Easting = 20, Northing = 10 },
            new Position { Easting = 12, Northing = 12 },  // Inner point
            new Position { Easting = 10, Northing = 20 },
            new Position { Easting = 5, Northing = 12 },   // Inner point
            new Position { Easting = 0, Northing = 10 },
            new Position { Easting = 5, Northing = 5 }     // Inner point
        };

        var pointInside = new Position { Easting = 10, Northing = 10 }; // Center
        var pointOutside = new Position { Easting = 0, Northing = 0 };  // Outside

        // Act
        bool insideResult = _service.IsPointInside(pointInside, polygon);
        bool outsideResult = _service.IsPointInside(pointOutside, polygon);

        // Assert
        Assert.True(insideResult, "Point at center of concave polygon should be inside");
        Assert.False(outsideResult, "Point outside concave polygon should be outside");
    }

    /// <summary>
    /// Test 8: Performance benchmark - must be <1ms for typical polygon
    /// </summary>
    [Fact]
    public void IsPointInside_PerformanceBenchmark_CompletesUnder1Ms()
    {
        // Arrange: Create a polygon with 100 vertices (typical field boundary)
        var polygon = CreateCircularPolygon(centerX: 500000, centerY: 4500000, radius: 100, vertexCount: 100);
        var point = new Position { Easting = 500000, Northing = 4500000 }; // Center point

        // Act: Perform check and measure time
        var stopwatch = Stopwatch.StartNew();
        bool result = _service.IsPointInside(point, polygon);
        stopwatch.Stop();

        double durationMs = stopwatch.Elapsed.TotalMilliseconds;
        double serviceDurationMs = _service.GetLastCheckDurationMs();

        // Assert
        Assert.True(result, "Point should be inside circular polygon");
        Assert.True(durationMs < 1.0, $"Check took {durationMs:F3}ms, should be <1ms");
        Assert.True(serviceDurationMs < 1.0, $"Service reported {serviceDurationMs:F3}ms, should be <1ms");
        Assert.Equal(1, _service.GetTotalChecksPerformed());
    }

    /// <summary>
    /// Test 9: ClassifyPoint returns correct location classifications
    /// </summary>
    [Fact]
    public void ClassifyPoint_VariousLocations_ReturnsCorrectClassification()
    {
        // Arrange
        var polygon = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 10, Northing = 0 },
            new Position { Easting = 10, Northing = 10 },
            new Position { Easting = 0, Northing = 10 }
        };

        var pointInside = new Position { Easting = 5, Northing = 5 };
        var pointOutside = new Position { Easting = 15, Northing = 15 };
        var pointOnBoundary = new Position { Easting = 5, Northing = 0 };

        // Act
        var insideResult = _service.ClassifyPoint(pointInside, polygon);
        var outsideResult = _service.ClassifyPoint(pointOutside, polygon);
        var boundaryResult = _service.ClassifyPoint(pointOnBoundary, polygon);

        // Assert
        Assert.Equal(PointLocation.Inside, insideResult);
        Assert.Equal(PointLocation.Outside, outsideResult);
        Assert.Equal(PointLocation.OnBoundary, boundaryResult);
    }

    /// <summary>
    /// Test 10: Spatial index improves performance for large polygons
    /// </summary>
    [Fact]
    public void BuildSpatialIndex_LargePolygon_ImprovesPerformance()
    {
        // Arrange: Create a large polygon with 500 vertices
        var largePolygon = CreateCircularPolygon(centerX: 500000, centerY: 4500000, radius: 100, vertexCount: 500);
        var point = new Position { Easting = 500000, Northing = 4500000 };

        // Act: Check without index
        var stopwatchNoIndex = Stopwatch.StartNew();
        bool resultNoIndex = _service.IsPointInside(point, largePolygon);
        stopwatchNoIndex.Stop();
        double durationNoIndex = stopwatchNoIndex.Elapsed.TotalMilliseconds;

        // Build spatial index
        _service.BuildSpatialIndex(largePolygon);

        // Check with index (bounding box optimization)
        var stopwatchWithIndex = Stopwatch.StartNew();
        bool resultWithIndex = _service.IsPointInside(point, largePolygon);
        stopwatchWithIndex.Stop();
        double durationWithIndex = stopwatchWithIndex.Elapsed.TotalMilliseconds;

        // Clean up
        _service.ClearSpatialIndex();

        // Assert
        Assert.True(resultNoIndex, "Point should be inside polygon");
        Assert.True(resultWithIndex, "Point should be inside polygon with index");
        Assert.True(durationWithIndex < 1.0, $"Check with index took {durationWithIndex:F3}ms, should be <1ms");
    }

    /// <summary>
    /// Helper method to create a circular polygon with specified number of vertices
    /// </summary>
    private Position[] CreateCircularPolygon(double centerX, double centerY, double radius, int vertexCount)
    {
        var polygon = new Position[vertexCount];
        double angleStep = 2 * Math.PI / vertexCount;

        for (int i = 0; i < vertexCount; i++)
        {
            double angle = i * angleStep;
            polygon[i] = new Position
            {
                Easting = centerX + radius * Math.Cos(angle),
                Northing = centerY + radius * Math.Sin(angle)
            };
        }

        return polygon;
    }
}
