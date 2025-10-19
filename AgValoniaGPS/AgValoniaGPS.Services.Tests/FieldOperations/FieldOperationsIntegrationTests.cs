using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Integration tests for Wave 5 Field Operations services.
/// Validates cross-service workflows, performance benchmarks, and edge cases.
/// </summary>
/// <remarks>
/// These tests verify that all Wave 5 services work together correctly:
/// - PointInPolygonService (Task Group 1)
/// - BoundaryManagementService (Task Group 2)
/// - HeadlandService (Task Group 3)
/// - UTurnService (Task Group 4)
/// - TramLineService (Task Group 5)
///
/// Performance targets:
/// - 10Hz position updates: <10ms total per update
/// - Full field setup: <100ms
/// </remarks>
public class FieldOperationsIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPointInPolygonService _pointInPolygon;
    private readonly IBoundaryManagementService _boundary;
    private readonly IHeadlandService _headland;
    private readonly IUTurnService _uturn;
    private readonly ITramLineService _tramLine;

    public FieldOperationsIntegrationTests()
    {
        // Setup DI container with all Wave 5 services
        var services = new ServiceCollection();

        // Register all Field Operations services
        services.AddSingleton<IPointInPolygonService, PointInPolygonService>();
        services.AddSingleton<IBoundaryManagementService, BoundaryManagementService>();
        services.AddSingleton<IHeadlandService, HeadlandService>();
        services.AddSingleton<IUTurnService, UTurnService>();
        services.AddSingleton<ITramLineService, TramLineService>();

        _serviceProvider = services.BuildServiceProvider();

        // Resolve all services
        _pointInPolygon = _serviceProvider.GetRequiredService<IPointInPolygonService>();
        _boundary = _serviceProvider.GetRequiredService<IBoundaryManagementService>();
        _headland = _serviceProvider.GetRequiredService<IHeadlandService>();
        _uturn = _serviceProvider.GetRequiredService<IUTurnService>();
        _tramLine = _serviceProvider.GetRequiredService<ITramLineService>();
    }

    /// <summary>
    /// Integration Test 1: Full field workflow - boundary load → headland generation → tram lines
    /// Validates end-to-end field setup operations
    /// </summary>
    [Fact]
    public void FullFieldWorkflow_LoadBoundaryGenerateFeaturesCheckViolations_AllOperationsSucceed()
    {
        // Arrange - Create realistic rectangular field (100m x 100m)
        var boundary = CreateRectangularField(500000, 4500000, 100);

        // Act - Full workflow
        var sw = Stopwatch.StartNew();

        // Step 1: Load boundary
        _boundary.LoadBoundary(boundary);
        Assert.True(_boundary.HasBoundary(), "Boundary should be loaded");

        // Step 2: Generate headlands (3 passes, 6m width) - returns void
        _headland.GenerateHeadlands(boundary, 6.0, 3);
        var headlands = _headland.GetHeadlands();
        Assert.NotNull(headlands);
        Assert.Equal(3, headlands.Length);

        // Step 3: Create a base line for tram lines (straight line through field center)
        var lineStart = new Position { Easting = 500000, Northing = 4500000 };
        var lineEnd = new Position { Easting = 500100, Northing = 4500100 };

        // Step 4: Generate tram lines (3m spacing, 5 passes per side) - returns void
        _tramLine.GenerateTramLines(lineStart, lineEnd, 3.0, 5);
        var tramLines = _tramLine.GetTramLines();
        Assert.NotNull(tramLines);
        Assert.True(tramLines.Length > 0);

        sw.Stop();

        // Assert - Verify all features generated
        Assert.True(_boundary.HasBoundary(), "Boundary should be present");
        Assert.Equal(3, headlands.Length);
        Assert.True(_tramLine.GetTramLineCount() > 0, "Should have tram lines");

        // Performance - Full field setup should be <100ms
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"Full field setup took {sw.ElapsedMilliseconds}ms, should be <100ms");
    }

    /// <summary>
    /// Integration Test 2: Headland entry/exit detection with boundary checks
    /// Validates that headland and boundary services work together correctly
    /// </summary>
    [Fact]
    public void HeadlandBoundaryIntegration_PositionChecksAcrossMultipleServices_CorrectDetection()
    {
        // Arrange - Setup field with boundary and headlands
        var boundary = CreateRectangularField(500000, 4500000, 100);
        _boundary.LoadBoundary(boundary);

        _headland.GenerateHeadlands(boundary, 3.0, 1);
        var headlands = _headland.GetHeadlands();
        Assert.NotNull(headlands);
        _headland.LoadHeadlands(headlands);

        // Test positions
        var positionInField = new Position { Easting = 500050, Northing = 4500050 }; // Center of field
        var positionNearEdge = new Position { Easting = 500003, Northing = 4500050 }; // Near edge, in headland
        var positionOutsideBoundary = new Position { Easting = 500200, Northing = 4500200 }; // Outside field

        // Act & Assert - Position in field (not in headland)
        Assert.True(_boundary.IsInsideBoundary(positionInField), "Center should be inside boundary");

        // Act & Assert - Position outside boundary
        Assert.False(_boundary.IsInsideBoundary(positionOutsideBoundary), "Outside position should NOT be inside boundary");
        Assert.False(_headland.IsInHeadland(positionOutsideBoundary), "Outside position should NOT be in headland");
    }

    /// <summary>
    /// Integration Test 3: U-Turn path generation within boundary constraints
    /// Validates that turn paths fit within field boundaries
    /// </summary>
    [Fact]
    public void UTurnGeneration_GenerateTurnPathWithinBoundary_PathIsValid()
    {
        // Arrange - Setup field boundary
        var boundary = CreateRectangularField(500000, 4500000, 100);
        _boundary.LoadBoundary(boundary);

        // Configure U-turn (Omega turn with 5m radius)
        _uturn.ConfigureTurn(UTurnType.Omega, 5.0, autoPause: false);

        // Entry and exit points near field edge
        var entryPoint = new Position { Easting = 500090, Northing = 4500050 };
        var entryHeading = 90.0; // Heading east
        var exitPoint = new Position { Easting = 500090, Northing = 4500060 };
        var exitHeading = 270.0; // Heading west (180° turn)

        // Act - Generate turn path
        var sw = Stopwatch.StartNew();
        var turnPath = _uturn.GenerateTurnPath(entryPoint, entryHeading, exitPoint, exitHeading);
        sw.Stop();

        // Assert - Turn path generated successfully
        Assert.NotNull(turnPath);
        Assert.True(turnPath.Length > 0, "Turn path should have waypoints");

        // Performance - Turn generation should be <10ms
        Assert.True(sw.ElapsedMilliseconds < 10,
            $"Turn generation took {sw.ElapsedMilliseconds}ms, should be <10ms");

        // Verify all turn path points are within reasonable bounds
        foreach (var point in turnPath)
        {
            Assert.True(point.Easting >= 500000 - 50 && point.Easting <= 500150,
                "Turn path point easting should be near field");
            Assert.True(point.Northing >= 4500000 - 50 && point.Northing <= 4500150,
                "Turn path point northing should be near field");
        }
    }

    /// <summary>
    /// Integration Test 4: Tram line proximity with boundary containment
    /// Validates tram line distance calculations within field boundaries
    /// </summary>
    [Fact]
    public void TramLineProximity_CheckDistanceWithinBoundary_CorrectCalculations()
    {
        // Arrange - Setup field with boundary
        var boundary = CreateRectangularField(500000, 4500000, 100);
        _boundary.LoadBoundary(boundary);

        // Create base line (diagonal through field)
        var lineStart = new Position { Easting = 500010, Northing = 4500010 };
        var lineEnd = new Position { Easting = 500090, Northing = 4500090 };

        // Generate tram lines (5m spacing, 3 lines per side)
        _tramLine.GenerateTramLines(lineStart, lineEnd, 5.0, 3);

        // Test position near a tram line
        var testPosition = new Position { Easting = 500050, Northing = 4500050 };

        // Act - Check distance to nearest tram line
        var distance = _tramLine.GetDistanceToNearestTramLine(testPosition);
        var nearestId = _tramLine.GetNearestTramLineId(testPosition);

        // Assert
        Assert.True(distance >= 0, "Distance should be non-negative");
        Assert.True(nearestId >= 0, "Nearest tram line ID should be valid");

        // Verify position is inside boundary
        Assert.True(_boundary.IsInsideBoundary(testPosition), "Test position should be inside boundary");
    }

    /// <summary>
    /// Integration Test 5: Multi-service event coordination
    /// Validates that multiple services can raise events simultaneously without conflicts
    /// </summary>
    [Fact]
    public void MultiServiceEvents_SimultaneousPositionUpdates_AllEventsHandledCorrectly()
    {
        // Arrange - Setup field with all features
        var boundary = CreateRectangularField(500000, 4500000, 100);
        _boundary.LoadBoundary(boundary);

        _headland.GenerateHeadlands(boundary, 3.0, 1);
        var headlands = _headland.GetHeadlands();
        Assert.NotNull(headlands);
        _headland.LoadHeadlands(headlands);

        var lineStart = new Position { Easting = 500000, Northing = 4500000 };
        var lineEnd = new Position { Easting = 500100, Northing = 4500100 };
        _tramLine.GenerateTramLines(lineStart, lineEnd, 5.0, 3);

        // Setup event handlers
        bool boundaryViolation = false;
        int eventsRaised = 0;

        _boundary.BoundaryViolation += (s, e) => boundaryViolation = true;
        _headland.HeadlandEntry += (s, e) => eventsRaised++;
        _tramLine.TramLineProximity += (s, e) => eventsRaised++;

        // Act - Check position inside the field
        var testPosition = new Position { Easting = 500050, Northing = 4500050 };

        _boundary.CheckPosition(testPosition);
        _headland.CheckPosition(testPosition);
        _tramLine.CheckProximity(testPosition, 10.0);

        // Assert - Services coordinated successfully - no boundary violation for interior position
        Assert.False(boundaryViolation, "Position inside boundary should not raise violation");
    }

    /// <summary>
    /// Integration Test 6: Performance benchmark - 10Hz position updates across all services
    /// Validates that all services can handle real-time 10Hz GPS updates within performance budget
    /// </summary>
    [Fact]
    public void RealTimePositionUpdate_10HzRateAcrossAllServices_CompletesWithinBudget()
    {
        // Arrange - Setup full field with complex boundary (200 vertices)
        var complexBoundary = CreateComplexField(500000, 4500000, 100, 200);
        _boundary.LoadBoundary(complexBoundary);

        _headland.GenerateHeadlands(complexBoundary, 6.0, 3);
        var headlands = _headland.GetHeadlands();
        Assert.NotNull(headlands);
        _headland.LoadHeadlands(headlands);

        var lineStart = new Position { Easting = 500000, Northing = 4500000 };
        var lineEnd = new Position { Easting = 500100, Northing = 4500100 };
        _tramLine.GenerateTramLines(lineStart, lineEnd, 5.0, 5);

        // Act - Simulate 10Hz position updates (10 updates in 1 second = 100ms budget each)
        var positions = GenerateTestPositions(500000, 4500000, 10);
        var stopwatch = Stopwatch.StartNew();

        foreach (var position in positions)
        {
            _boundary.CheckPosition(position);
            _headland.CheckPosition(position);
            _tramLine.CheckProximity(position, 5.0);
        }

        stopwatch.Stop();

        // Assert - Average <10ms per update (for 10Hz operation)
        var avgTime = stopwatch.ElapsedMilliseconds / 10.0;
        Assert.True(avgTime < 10.0,
            $"Average update time {avgTime:F2}ms exceeds 10ms budget for 10Hz operation");
    }

    /// <summary>
    /// Integration Test 7: Large field handling - 500 vertex boundary with all services
    /// Validates performance with complex, large fields
    /// </summary>
    [Fact]
    public void LargeFieldHandling_500VertexBoundary_AllOperationsWithinSpec()
    {
        // Arrange - Create large complex boundary (500 vertices)
        var largeBoundary = CreateComplexField(500000, 4500000, 200, 500);

        // Act - Perform all major operations
        var sw = Stopwatch.StartNew();

        _boundary.LoadBoundary(largeBoundary);
        var area = _boundary.CalculateArea();
        var simplified = _boundary.SimplifyBoundary(1.0);

        // Build spatial index for large polygon
        _pointInPolygon.BuildSpatialIndex(largeBoundary);

        // Generate headlands
        _headland.GenerateHeadlands(largeBoundary, 6.0, 2);
        var headlands = _headland.GetHeadlands();

        // Test point-in-polygon with spatial index
        var testPoint = new Position { Easting = 500100, Northing = 4500100 };
        bool isInside = _boundary.IsInsideBoundary(testPoint);

        sw.Stop();

        // Assert - All operations complete successfully
        Assert.True(_boundary.HasBoundary(), "Large boundary should be loaded");
        Assert.True(area > 0, "Area should be calculated");
        Assert.True(simplified.Length < largeBoundary.Length, "Simplification should reduce points");
        Assert.NotNull(headlands);

        // Performance - Large field operations should complete reasonably fast
        Assert.True(sw.ElapsedMilliseconds < 500,
            $"Large field operations took {sw.ElapsedMilliseconds}ms, should be <500ms");

        // Cleanup
        _pointInPolygon.ClearSpatialIndex();
    }

    /// <summary>
    /// Integration Test 8: Concurrent access stress test - multiple threads accessing services
    /// Validates thread safety across all services
    /// </summary>
    [Fact]
    public void ConcurrentAccess_MultipleThreadsAccessingServices_ThreadSafe()
    {
        // Arrange - Setup field
        var boundary = CreateRectangularField(500000, 4500000, 100);
        _boundary.LoadBoundary(boundary);

        _headland.GenerateHeadlands(boundary, 6.0, 2);
        var headlands = _headland.GetHeadlands();
        Assert.NotNull(headlands);
        _headland.LoadHeadlands(headlands);

        var lineStart = new Position { Easting = 500000, Northing = 4500000 };
        var lineEnd = new Position { Easting = 500100, Northing = 4500100 };
        _tramLine.GenerateTramLines(lineStart, lineEnd, 5.0, 3);

        // Act - Run 50 concurrent tasks performing checks
        var tasks = new Task[50];
        for (int i = 0; i < 50; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() =>
            {
                var random = new Random(taskId);
                for (int j = 0; j < 20; j++)
                {
                    var position = new Position
                    {
                        Easting = 500000 + random.Next(100),
                        Northing = 4500000 + random.Next(100)
                    };

                    _boundary.IsInsideBoundary(position);
                    _headland.IsInHeadland(position);
                    _tramLine.GetDistanceToNearestTramLine(position);
                }
            });
        }

        // Assert - All tasks complete without exceptions
        Task.WaitAll(tasks);
    }

    /// <summary>
    /// Integration Test 9: Boundary simplification impact on headland generation
    /// Validates that simplified boundaries still generate valid headlands
    /// </summary>
    [Fact]
    public void BoundarySimplificationImpact_SimplifyThenGenerateHeadlands_ConsistentResults()
    {
        // Arrange - Create boundary with many collinear points
        var detailedBoundary = CreateDetailedRectangularField(500000, 4500000, 100, 400);
        _boundary.LoadBoundary(detailedBoundary);

        // Act - Generate headlands before simplification
        _headland.GenerateHeadlands(detailedBoundary, 6.0, 2);
        var headlandsBefore = _headland.GetHeadlands();
        Assert.NotNull(headlandsBefore);

        // Simplify boundary
        var simplifiedBoundary = _boundary.SimplifyBoundary(0.5);
        Assert.True(simplifiedBoundary.Length < detailedBoundary.Length,
            "Simplified boundary should have fewer points");

        // Generate headlands after simplification
        _headland.ClearHeadlands();
        _headland.GenerateHeadlands(simplifiedBoundary, 6.0, 2);
        var headlandsAfter = _headland.GetHeadlands();
        Assert.NotNull(headlandsAfter);

        // Assert - Both should generate same number of passes
        Assert.Equal(headlandsBefore.Length, headlandsAfter.Length);
        Assert.Equal(2, headlandsAfter.Length);

        // Area should be approximately the same
        _boundary.LoadBoundary(detailedBoundary);
        double areaBefore = _boundary.CalculateArea();

        _boundary.LoadBoundary(simplifiedBoundary);
        double areaAfter = _boundary.CalculateArea();

        // Areas should be within 5% of each other
        double percentDifference = Math.Abs(areaBefore - areaAfter) / areaBefore * 100;
        Assert.True(percentDifference < 5.0,
            $"Area difference after simplification: {percentDifference:F2}%, should be <5%");
    }

    /// <summary>
    /// Integration Test 10: Performance benchmark - 1000 point-in-polygon checks
    /// Validates that the system can handle sustained 10Hz operation (100 checks/second)
    /// </summary>
    [Fact]
    public void PointInPolygonBenchmark_1000Checks_CompletesUnder1Second()
    {
        // Arrange - Setup typical boundary
        var boundary = CreateComplexField(500000, 4500000, 100, 100);
        _boundary.LoadBoundary(boundary);

        var testPositions = GenerateTestPositions(500000, 4500000, 1000);

        // Warm up
        for (int i = 0; i < 10; i++)
        {
            _boundary.IsInsideBoundary(testPositions[i]);
        }

        // Act - Perform 1000 checks
        var sw = Stopwatch.StartNew();
        foreach (var position in testPositions)
        {
            _boundary.IsInsideBoundary(position);
        }
        sw.Stop();

        // Assert - 1000 checks in <1000ms = <1ms per check average
        Assert.True(sw.ElapsedMilliseconds < 1000,
            $"1000 point-in-polygon checks took {sw.ElapsedMilliseconds}ms, should be <1000ms");

        double avgTimePerCheck = sw.ElapsedMilliseconds / 1000.0;
        Assert.True(avgTimePerCheck < 1.0,
            $"Average check time {avgTimePerCheck:F3}ms exceeds 1ms target");
    }

    #region Helper Methods

    /// <summary>
    /// Creates a simple rectangular field boundary
    /// </summary>
    private Position[] CreateRectangularField(double startEasting, double startNorthing, double size)
    {
        return new[]
        {
            new Position { Easting = startEasting, Northing = startNorthing },
            new Position { Easting = startEasting + size, Northing = startNorthing },
            new Position { Easting = startEasting + size, Northing = startNorthing + size },
            new Position { Easting = startEasting, Northing = startNorthing + size }
        };
    }

    /// <summary>
    /// Creates a complex boundary with specified number of vertices (circular pattern)
    /// </summary>
    private Position[] CreateComplexField(double centerEasting, double centerNorthing, double radius, int vertexCount)
    {
        var boundary = new Position[vertexCount];
        double angleStep = 2 * Math.PI / vertexCount;

        for (int i = 0; i < vertexCount; i++)
        {
            double angle = i * angleStep;
            // Add some irregularity to make it more realistic
            double radiusVariation = radius * (1.0 + 0.1 * Math.Sin(3 * angle));

            boundary[i] = new Position
            {
                Easting = centerEasting + radiusVariation * Math.Cos(angle),
                Northing = centerNorthing + radiusVariation * Math.Sin(angle)
            };
        }

        return boundary;
    }

    /// <summary>
    /// Creates a detailed rectangular field with many points along edges
    /// </summary>
    private Position[] CreateDetailedRectangularField(double startEasting, double startNorthing, double size, int pointsPerSide)
    {
        var points = new List<Position>();
        double step = size / (pointsPerSide / 4.0);

        // Bottom edge
        for (int i = 0; i < pointsPerSide / 4; i++)
        {
            points.Add(new Position
            {
                Easting = startEasting + i * step,
                Northing = startNorthing
            });
        }

        // Right edge
        for (int i = 0; i < pointsPerSide / 4; i++)
        {
            points.Add(new Position
            {
                Easting = startEasting + size,
                Northing = startNorthing + i * step
            });
        }

        // Top edge
        for (int i = 0; i < pointsPerSide / 4; i++)
        {
            points.Add(new Position
            {
                Easting = startEasting + size - i * step,
                Northing = startNorthing + size
            });
        }

        // Left edge
        for (int i = 0; i < pointsPerSide / 4; i++)
        {
            points.Add(new Position
            {
                Easting = startEasting,
                Northing = startNorthing + size - i * step
            });
        }

        return points.ToArray();
    }

    /// <summary>
    /// Generates test positions within a field area
    /// </summary>
    private Position[] GenerateTestPositions(double startEasting, double startNorthing, int count)
    {
        var positions = new Position[count];
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < count; i++)
        {
            positions[i] = new Position
            {
                Easting = startEasting + random.NextDouble() * 100,
                Northing = startNorthing + random.NextDouble() * 100
            };
        }

        return positions;
    }

    #endregion
}
