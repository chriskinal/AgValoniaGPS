using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.FieldOperations;
using Xunit;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for BoundaryManagementService
/// </summary>
public class BoundaryManagementServiceTests
{
    private readonly IPointInPolygonService _pointInPolygonService;
    private readonly IBoundaryManagementService _boundaryManagementService;

    public BoundaryManagementServiceTests()
    {
        _pointInPolygonService = new PointInPolygonService();
        _boundaryManagementService = new BoundaryManagementService(_pointInPolygonService);
    }

    /// <summary>
    /// Test 1: Load and retrieve boundary
    /// </summary>
    [Fact]
    public void LoadBoundary_StoresAndRetrievesBoundary()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);

        // Act
        _boundaryManagementService.LoadBoundary(boundary);
        var retrieved = _boundaryManagementService.GetCurrentBoundary();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(boundary.Length, retrieved.Length);
        Assert.Equal(boundary[0].Easting, retrieved[0].Easting);
        Assert.Equal(boundary[0].Northing, retrieved[0].Northing);
        Assert.True(_boundaryManagementService.HasBoundary());
    }

    /// <summary>
    /// Test 2: Clear boundary removes all data
    /// </summary>
    [Fact]
    public void ClearBoundary_RemovesBoundaryData()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        // Act
        _boundaryManagementService.ClearBoundary();

        // Assert
        Assert.Null(_boundaryManagementService.GetCurrentBoundary());
        Assert.False(_boundaryManagementService.HasBoundary());
    }

    /// <summary>
    /// Test 3: Check point inside boundary returns true
    /// </summary>
    [Fact]
    public void IsInsideBoundary_PointInsideBoundary_ReturnsTrue()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        var insidePoint = new Position
        {
            Easting = 500050,
            Northing = 4500050
        };

        // Act
        bool isInside = _boundaryManagementService.IsInsideBoundary(insidePoint);

        // Assert
        Assert.True(isInside);
    }

    /// <summary>
    /// Test 4: Check point outside boundary returns false
    /// </summary>
    [Fact]
    public void IsInsideBoundary_PointOutsideBoundary_ReturnsFalse()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        var outsidePoint = new Position
        {
            Easting = 500200,
            Northing = 4500200
        };

        // Act
        bool isInside = _boundaryManagementService.IsInsideBoundary(outsidePoint);

        // Assert
        Assert.False(isInside);
    }

    /// <summary>
    /// Test 5: Boundary violation event fires when point is outside
    /// </summary>
    [Fact]
    public void CheckPosition_PointOutsideBoundary_RaisesBoundaryViolationEvent()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        BoundaryViolationEventArgs? eventArgs = null;
        _boundaryManagementService.BoundaryViolation += (sender, args) => eventArgs = args;

        var outsidePoint = new Position
        {
            Easting = 500200,
            Northing = 4500200
        };

        // Act
        _boundaryManagementService.CheckPosition(outsidePoint);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal(outsidePoint.Easting, eventArgs.ViolatingPosition.Easting);
        Assert.Equal(outsidePoint.Northing, eventArgs.ViolatingPosition.Northing);
        Assert.True(eventArgs.DistanceFromBoundary > 0);
    }

    /// <summary>
    /// Test 6: No violation event when point is inside boundary
    /// </summary>
    [Fact]
    public void CheckPosition_PointInsideBoundary_DoesNotRaiseEvent()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        bool eventRaised = false;
        _boundaryManagementService.BoundaryViolation += (sender, args) => eventRaised = true;

        var insidePoint = new Position
        {
            Easting = 500050,
            Northing = 4500050
        };

        // Act
        _boundaryManagementService.CheckPosition(insidePoint);

        // Assert
        Assert.False(eventRaised);
    }

    /// <summary>
    /// Test 7: Calculate area using Shoelace formula
    /// </summary>
    [Fact]
    public void CalculateArea_SquareBoundary_ReturnsCorrectArea()
    {
        // Arrange - 100m x 100m square = 10,000 m²
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        // Act
        double area = _boundaryManagementService.CalculateArea();

        // Assert
        Assert.Equal(10000.0, area, 1.0); // Within 1 m² tolerance
    }

    /// <summary>
    /// Test 8: Douglas-Peucker simplification reduces points
    /// </summary>
    [Fact]
    public void SimplifyBoundary_ReducesPointCount()
    {
        // Arrange - Create boundary with many collinear points
        var boundary = new List<Position>();
        for (int i = 0; i <= 100; i++)
        {
            boundary.Add(new Position
            {
                Easting = 500000 + i,
                Northing = 4500000
            });
        }
        for (int i = 0; i <= 100; i++)
        {
            boundary.Add(new Position
            {
                Easting = 500100,
                Northing = 4500000 + i
            });
        }
        for (int i = 100; i >= 0; i--)
        {
            boundary.Add(new Position
            {
                Easting = 500000 + i,
                Northing = 4500100
            });
        }
        for (int i = 100; i >= 0; i--)
        {
            boundary.Add(new Position
            {
                Easting = 500000,
                Northing = 4500000 + i
            });
        }

        _boundaryManagementService.LoadBoundary(boundary.ToArray());

        // Act
        var simplified = _boundaryManagementService.SimplifyBoundary(1.0);

        // Assert
        Assert.True(simplified.Length < boundary.Count);
        Assert.True(simplified.Length >= 4); // Should keep at least corner points
    }

    /// <summary>
    /// Test 9: Performance benchmark - boundary check <2ms
    /// </summary>
    [Fact]
    public void IsInsideBoundary_PerformanceBenchmark_CompletesInUnder2Ms()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        var testPoint = new Position
        {
            Easting = 500050,
            Northing = 4500050
        };

        // Warm up
        for (int i = 0; i < 10; i++)
        {
            _boundaryManagementService.IsInsideBoundary(testPoint);
        }

        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _boundaryManagementService.IsInsideBoundary(testPoint);
        }
        sw.Stop();

        double averageMs = sw.Elapsed.TotalMilliseconds / 100.0;

        // Assert
        Assert.True(averageMs < 2.0, $"Average check time {averageMs:F3}ms exceeds 2ms target");
    }

    /// <summary>
    /// Test 10: Thread-safe concurrent boundary checks
    /// </summary>
    [Fact]
    public void IsInsideBoundary_ConcurrentAccess_ThreadSafe()
    {
        // Arrange
        var boundary = CreateSquareBoundary(500000, 4500000, 100);
        _boundaryManagementService.LoadBoundary(boundary);

        var insidePoint = new Position
        {
            Easting = 500050,
            Northing = 4500050
        };

        var outsidePoint = new Position
        {
            Easting = 500200,
            Northing = 4500200
        };

        // Act - Run 100 concurrent checks
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    Assert.True(_boundaryManagementService.IsInsideBoundary(insidePoint));
                    Assert.False(_boundaryManagementService.IsInsideBoundary(outsidePoint));
                }
            }));
        }

        // Assert - Should complete without exceptions
        Task.WaitAll(tasks.ToArray());
    }

    /// <summary>
    /// Helper: Creates a square boundary
    /// </summary>
    private Position[] CreateSquareBoundary(double startEasting, double startNorthing, double size)
    {
        return new[]
        {
            new Position { Easting = startEasting, Northing = startNorthing },
            new Position { Easting = startEasting + size, Northing = startNorthing },
            new Position { Easting = startEasting + size, Northing = startNorthing + size },
            new Position { Easting = startEasting, Northing = startNorthing + size }
        };
    }
}
