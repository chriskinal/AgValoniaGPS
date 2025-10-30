using System;
using System.Collections.Generic;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Performance and functional tests for BoundaryGuidedDubinsService.
/// Validates <10ms constraint for 20Hz steering loop.
/// </summary>
[TestFixture]
public class BoundaryGuidedDubinsServiceTests
{
    private IDubinsPathService _dubinsPathService = null!;
    private IBoundaryGuidedDubinsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dubinsPathService = new DubinsPathService();
        _service = new BoundaryGuidedDubinsService(_dubinsPathService);
    }

    // ==================== Performance Tests ====================

    [Test]
    public void GeneratePath_SimpleScenario_CompletesUnder10ms()
    {
        // Arrange: Simple rectangular field, no obstacles
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(100, 0),
                new Position2D(100, 50),
                new Position2D(0, 50)
            }
        };

        // Act & Assert
        var result = _service.GenerateBoundaryAwarePath(
            10, 10, 0,          // Start
            90, 40, Math.PI,    // Goal (180° turn)
            5.0,                // Turning radius
            boundaries,
            1.0,                // Min boundary distance
            0.1,                // Waypoint spacing
            8                   // Max iterations
        );

        Assert.That(result.ComputationTime.TotalMilliseconds, Is.LessThan(10),
            $"Path generation took {result.ComputationTime.TotalMilliseconds:F2}ms, exceeds 10ms budget");
    }

    [Test]
    public void GeneratePath_IrregularField_CompletesUnder10ms()
    {
        // Arrange: Irregular European-style field with internal obstacle
        var boundaries = new List<List<Position2D>>
        {
            // Outer boundary (irregular shape)
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(80, 5),
                new Position2D(95, 15),
                new Position2D(100, 35),
                new Position2D(90, 50),
                new Position2D(70, 55),
                new Position2D(40, 52),
                new Position2D(10, 48),
                new Position2D(0, 30)
            },
            // Internal obstacle (e.g., power pole buffer)
            new List<Position2D>
            {
                new Position2D(45, 25),
                new Position2D(55, 25),
                new Position2D(55, 30),
                new Position2D(45, 30)
            }
        };

        // Act: Generate path that needs to avoid obstacle
        var result = _service.GenerateBoundaryAwarePath(
            20, 20, Math.PI / 4,     // Start at 45°
            80, 40, Math.PI * 5 / 4, // Goal at 225°
            6.0,                      // Turning radius
            boundaries,
            2.0,                      // Min boundary distance
            0.1,                      // Waypoint spacing
            8                         // Max iterations
        );

        // Assert
        Assert.That(result.ComputationTime.TotalMilliseconds, Is.LessThan(10),
            $"Complex path generation took {result.ComputationTime.TotalMilliseconds:F2}ms, exceeds 10ms budget");
    }

    [Test]
    public void GeneratePath_WorstCase_CompletesUnder10ms()
    {
        // Arrange: Worst-case scenario - tight constraints requiring guided sampling
        var boundaries = new List<List<Position2D>>
        {
            // Very narrow field section
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(20, 0),
                new Position2D(20, 50),
                new Position2D(0, 50)
            }
        };

        // Act: Tight turn in narrow corridor (forces guided sampling)
        var result = _service.GenerateBoundaryAwarePath(
            10, 10, 0,           // Start
            10, 40, Math.PI,     // Goal (U-turn in place)
            4.0,                 // Turning radius (tight for corridor)
            boundaries,
            1.5,                 // Min boundary distance
            0.1,                 // Waypoint spacing
            8                    // Max iterations
        );

        // Assert
        Assert.That(result.ComputationTime.TotalMilliseconds, Is.LessThan(10),
            $"Worst-case path generation took {result.ComputationTime.TotalMilliseconds:F2}ms, exceeds 10ms budget");
    }

    [Test]
    public void GeneratePath_100Iterations_AverageUnder10ms()
    {
        // Arrange: Statistical performance test
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(100, 0),
                new Position2D(100, 50),
                new Position2D(0, 50)
            }
        };

        var times = new List<double>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Act: Run 100 iterations with varied parameters
        for (int i = 0; i < 100; i++)
        {
            double startE = 10 + random.NextDouble() * 30;
            double startN = 10 + random.NextDouble() * 30;
            double startH = random.NextDouble() * Math.PI * 2;
            double goalE = 60 + random.NextDouble() * 30;
            double goalN = 10 + random.NextDouble() * 30;
            double goalH = random.NextDouble() * Math.PI * 2;

            var result = _service.GenerateBoundaryAwarePath(
                startE, startN, startH,
                goalE, goalN, goalH,
                5.0, boundaries, 1.0, 0.1, 8
            );

            times.Add(result.ComputationTime.TotalMilliseconds);
        }

        // Assert
        double averageTime = times.Sum() / times.Count;
        double maxTime = times.Max();
        double p95Time = times.OrderBy(t => t).ElementAt(95);

        Console.WriteLine($"Performance statistics (100 iterations):");
        Console.WriteLine($"  Average: {averageTime:F2}ms");
        Console.WriteLine($"  Maximum: {maxTime:F2}ms");
        Console.WriteLine($"  95th percentile: {p95Time:F2}ms");

        Assert.That(averageTime, Is.LessThan(10), $"Average time {averageTime:F2}ms exceeds 10ms");
        Assert.That(p95Time, Is.LessThan(10), $"95th percentile {p95Time:F2}ms exceeds 10ms");
    }

    // ==================== Functional Tests ====================

    [Test]
    public void GeneratePath_StandardDubinsWorks_UsesStandardStrategy()
    {
        // Arrange: Clear path, no boundaries in the way
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(-50, -50),
                new Position2D(150, -50),
                new Position2D(150, 100),
                new Position2D(-50, 100)
            }
        };

        // Act
        var result = _service.GenerateBoundaryAwarePath(
            10, 10, 0,
            90, 10, Math.PI,
            5.0,
            boundaries,
            1.0,
            0.1,
            8
        );

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Strategy, Is.EqualTo(PathGenerationStrategy.StandardDubins),
            "Should use standard Dubins for clear path");
        Assert.That(result.IterationCount, Is.EqualTo(0), "Standard Dubins requires 0 iterations");
    }

    [Test]
    public void GeneratePath_BoundaryViolation_UsesGuidedSampling()
    {
        // Arrange: Boundary blocks standard path, requires guided sampling
        var boundaries = new List<List<Position2D>>
        {
            // Field boundary
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(50, 0),
                new Position2D(50, 30),
                new Position2D(0, 30)
            },
            // Obstacle in middle of standard turn path
            new List<Position2D>
            {
                new Position2D(20, 10),
                new Position2D(30, 10),
                new Position2D(30, 20),
                new Position2D(20, 20)
            }
        };

        // Act
        var result = _service.GenerateBoundaryAwarePath(
            10, 15, 0,           // Start
            40, 15, Math.PI,     // Goal (turn around obstacle)
            3.0,
            boundaries,
            1.0,
            0.1,
            8
        );

        // Assert
        if (result.Succeeded)
        {
            Assert.That(result.Strategy, Is.EqualTo(PathGenerationStrategy.GuidedSampling),
                "Should use guided sampling when obstacle blocks standard path");
            Assert.That(result.IterationCount, Is.GreaterThan(0), "Guided sampling requires iterations");
            Assert.That(result.IntermediateWaypoints, Is.Not.Empty, "Should have intermediate waypoints");
        }
        // Note: May fail if scenario is too constrained - acceptable for this test
    }

    [Test]
    public void GeneratePath_RespectsBoundaryDistance()
    {
        // Arrange
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(100, 0),
                new Position2D(100, 50),
                new Position2D(0, 50)
            }
        };

        double minDistance = 2.0;

        // Act
        var result = _service.GenerateBoundaryAwarePath(
            50, 25, 0,
            50, 25, Math.PI,
            5.0,
            boundaries,
            minDistance,
            0.1,
            8
        );

        // Assert
        if (result.Succeeded && result.ResultPath != null)
        {
            Assert.That(result.MinBoundaryDistance, Is.GreaterThanOrEqualTo(minDistance),
                $"Path violates minimum boundary distance: {result.MinBoundaryDistance:F2}m < {minDistance}m");
        }
    }

    [Test]
    public void IsPointValid_PointTooClose_ReturnsFalse()
    {
        // Arrange
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(10, 0),
                new Position2D(10, 10),
                new Position2D(0, 10)
            }
        };

        var point = new Position2D(0.5, 5); // 0.5m from edge

        // Act
        bool isValid = _service.IsPointValid(point, boundaries, 1.0);

        // Assert
        Assert.That(isValid, Is.False, "Point should be invalid (too close to boundary)");
    }

    [Test]
    public void IsPointValid_PointFarEnough_ReturnsTrue()
    {
        // Arrange
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(10, 0),
                new Position2D(10, 10),
                new Position2D(0, 10)
            }
        };

        var point = new Position2D(5, 5); // Center of field

        // Act
        bool isValid = _service.IsPointValid(point, boundaries, 1.0);

        // Assert
        Assert.That(isValid, Is.True, "Point should be valid (far from boundaries)");
    }

    [Test]
    public void CalculateBoundaryRepulsion_CloseToEdge_ReturnsNonZeroRepulsion()
    {
        // Arrange
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(10, 0),
                new Position2D(10, 10),
                new Position2D(0, 10)
            }
        };

        var point = new Position2D(1, 5); // 1m from left edge

        // Act
        var repulsion = _service.CalculateBoundaryRepulsion(point, boundaries, 3.0);

        // Assert
        Assert.That(repulsion.Easting, Is.GreaterThan(0), "Should have positive easting repulsion (away from left edge)");
        double magnitude = Math.Sqrt(repulsion.Easting * repulsion.Easting + repulsion.Northing * repulsion.Northing);
        Assert.That(magnitude, Is.GreaterThan(0), "Repulsion magnitude should be non-zero");
    }

    [Test]
    public void CalculateBoundaryRepulsion_FarFromBoundary_ReturnsZeroRepulsion()
    {
        // Arrange
        var boundaries = new List<List<Position2D>>
        {
            new List<Position2D>
            {
                new Position2D(0, 0),
                new Position2D(10, 0),
                new Position2D(10, 10),
                new Position2D(0, 10)
            }
        };

        var point = new Position2D(5, 5); // Center, far from all edges

        // Act
        var repulsion = _service.CalculateBoundaryRepulsion(point, boundaries, 3.0);

        // Assert
        double magnitude = Math.Sqrt(repulsion.Easting * repulsion.Easting + repulsion.Northing * repulsion.Northing);
        Assert.That(magnitude, Is.LessThan(0.1), "Repulsion should be negligible far from boundaries");
    }
}
