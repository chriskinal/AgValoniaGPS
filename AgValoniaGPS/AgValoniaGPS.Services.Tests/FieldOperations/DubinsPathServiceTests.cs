using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using Xunit;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

/// <summary>
/// Tests for DubinsPathService - shortest path between two poses with turning radius constraint.
/// Validates all 6 path types (RSR, LSL, RSL, LSR, RLR, LRL) and path generation correctness.
/// </summary>
public class DubinsPathServiceTests
{
    private readonly IDubinsPathService _service;
    private const double TurningRadius = 5.0; // 5 meter turning radius for tests
    private const double Tolerance = 0.01; // 1cm tolerance for length comparisons

    public DubinsPathServiceTests()
    {
        _service = new DubinsPathService();
    }

    #region RSR Path Tests

    /// <summary>
    /// Test RSR: Right-Straight-Right path
    /// Start and goal are aligned, both require right turns
    /// </summary>
    [Fact]
    public void GeneratePath_RSR_ParallelPoses_ReturnsValidPath()
    {
        // Arrange: Two poses facing same direction, offset to trigger RSR
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0; // Facing north

        double goalEasting = 20;
        double goalNorthing = 0;
        double goalHeading = 0; // Facing north

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        Assert.Equal(DubinsPathType.RSR, path.PathType);
        Assert.True(path.TotalLength > 0, "Path length should be positive");
        Assert.True(path.Waypoints.Count > 0, "Path should have waypoints");

        // Verify start and end positions
        var firstWaypoint = path.Waypoints[0];
        Assert.Equal(startEasting, firstWaypoint.Easting, 2);
        Assert.Equal(startNorthing, firstWaypoint.Northing, 2);

        var lastWaypoint = path.Waypoints[^1];
        Assert.Equal(goalEasting, lastWaypoint.Easting, 2);
        Assert.Equal(goalNorthing, lastWaypoint.Northing, 2);
    }

    #endregion

    #region LSL Path Tests

    /// <summary>
    /// Test LSL: Left-Straight-Left path
    /// Similar to RSR but with left turns
    /// </summary>
    [Fact]
    public void GeneratePath_LSL_ParallelPoses_ReturnsValidPath()
    {
        // Arrange: Two poses with configuration favoring LSL
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = Math.PI; // Facing south

        double goalEasting = -20;
        double goalNorthing = 0;
        double goalHeading = Math.PI; // Facing south

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        Assert.Equal(DubinsPathType.LSL, path.PathType);
        Assert.True(path.TotalLength > 0);
        Assert.True(path.Waypoints.Count > 0);
    }

    #endregion

    #region RSL Path Tests

    /// <summary>
    /// Test RSL: Right-Straight-Left path
    /// Start with right turn, end with left turn
    /// </summary>
    [Fact]
    public void GeneratePath_RSL_OppositeTurns_ReturnsValidPath()
    {
        // Arrange: Poses requiring opposite turn directions
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0; // Facing north

        double goalEasting = 30;
        double goalNorthing = 30;
        double goalHeading = Math.PI; // Facing south (opposite direction)

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        // RSL or LSR should be chosen for opposite directions
        Assert.True(path.PathType == DubinsPathType.RSL || path.PathType == DubinsPathType.LSR);
        Assert.True(path.TotalLength > 0);
        Assert.True(path.Waypoints.Count > 0);
    }

    #endregion

    #region LSR Path Tests

    /// <summary>
    /// Test LSR: Left-Straight-Right path
    /// Start with left turn, end with right turn
    /// </summary>
    [Fact]
    public void GeneratePath_LSR_OppositeTurns_ReturnsValidPath()
    {
        // Arrange: Mirror of RSL test
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = Math.PI; // Facing south

        double goalEasting = 30;
        double goalNorthing = -30;
        double goalHeading = 0; // Facing north (opposite direction)

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.PathType == DubinsPathType.LSR || path.PathType == DubinsPathType.RSL);
        Assert.True(path.TotalLength > 0);
        Assert.True(path.Waypoints.Count > 0);
    }

    #endregion

    #region RLR Path Tests

    /// <summary>
    /// Test RLR: Right-Left-Right path (C-curve)
    /// Used for tight maneuvers when distance is less than 4 * radius
    /// </summary>
    [Fact]
    public void GeneratePath_RLR_TightTurn_ReturnsValidPath()
    {
        // Arrange: Close poses requiring S-curve
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0; // Facing north

        double goalEasting = 5;
        double goalNorthing = 15;
        double goalHeading = 0; // Facing north (parallel)

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        // For this configuration, RLR or LRL might be shortest
        Assert.True(
            path.PathType == DubinsPathType.RLR ||
            path.PathType == DubinsPathType.LRL ||
            path.PathType == DubinsPathType.RSR ||
            path.PathType == DubinsPathType.LSL);
        Assert.True(path.TotalLength > 0);
        Assert.True(path.Waypoints.Count > 0);
    }

    #endregion

    #region LRL Path Tests

    /// <summary>
    /// Test LRL: Left-Right-Left path (C-curve)
    /// Mirror of RLR test
    /// </summary>
    [Fact]
    public void GeneratePath_LRL_TightTurn_ReturnsValidPath()
    {
        // Arrange: Close poses requiring reverse S-curve
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = Math.PI; // Facing south

        double goalEasting = -5;
        double goalNorthing = -15;
        double goalHeading = Math.PI; // Facing south (parallel)

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.TotalLength > 0);
        Assert.True(path.Waypoints.Count > 0);
    }

    #endregion

    #region Path Length Tests

    /// <summary>
    /// Test that straight-line distance is always less than or equal to path length
    /// (triangle inequality)
    /// </summary>
    [Fact]
    public void GeneratePath_PathLength_IsGreaterThanOrEqualToStraightDistance()
    {
        // Arrange
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0;

        double goalEasting = 50;
        double goalNorthing = 50;
        double goalHeading = Math.PI / 2;

        double straightDistance = Math.Sqrt(50 * 50 + 50 * 50);

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.TotalLength >= straightDistance,
            $"Path length ({path.TotalLength:F2}) should be >= straight distance ({straightDistance:F2})");
    }

    /// <summary>
    /// Test that total length equals sum of segment lengths
    /// </summary>
    [Fact]
    public void GeneratePath_TotalLength_EqualsSumOfSegments()
    {
        // Arrange
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0;

        double goalEasting = 20;
        double goalNorthing = 20;
        double goalHeading = Math.PI / 4;

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        double segmentSum = path.Segment1Length + path.Segment2Length + path.Segment3Length;
        Assert.Equal(path.TotalLength, segmentSum, 2);
    }

    #endregion

    #region Waypoint Tests

    /// <summary>
    /// Test that waypoints are correctly spaced along the path
    /// </summary>
    [Fact]
    public void GeneratePath_Waypoints_AreCorrectlySpaced()
    {
        // Arrange
        double spacing = 0.5; // 50cm spacing
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0;

        double goalEasting = 30;
        double goalNorthing = 0;
        double goalHeading = 0;

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius, spacing);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.Waypoints.Count > 2, "Should have multiple waypoints");

        // Check approximate spacing between consecutive waypoints (allowing tolerance for turns)
        for (int i = 0; i < path.Waypoints.Count - 1; i++)
        {
            var wp1 = path.Waypoints[i];
            var wp2 = path.Waypoints[i + 1];

            double de = wp2.Easting - wp1.Easting;
            double dn = wp2.Northing - wp1.Northing;
            double distance = Math.Sqrt(de * de + dn * dn);

            // Spacing should be approximately equal to requested spacing (with tolerance)
            Assert.True(distance <= spacing * 1.5,
                $"Waypoint spacing ({distance:F3}) should be <= {spacing * 1.5:F3}");
        }
    }

    /// <summary>
    /// Test that waypoint headings are continuous (no sudden jumps)
    /// </summary>
    [Fact]
    public void GeneratePath_Waypoints_HeadingsAreContinuous()
    {
        // Arrange
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0;

        double goalEasting = 20;
        double goalNorthing = 20;
        double goalHeading = Math.PI / 2;

        // Act
        var path = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);

        for (int i = 0; i < path.Waypoints.Count - 1; i++)
        {
            double heading1 = path.Waypoints[i].Heading;
            double heading2 = path.Waypoints[i + 1].Heading;

            // Calculate heading difference (handling wrap-around at ±π)
            double diff = Math.Abs(heading2 - heading1);
            if (diff > Math.PI)
            {
                diff = 2 * Math.PI - diff;
            }

            // Heading change should be small between consecutive waypoints
            Assert.True(diff < Math.PI / 2,
                $"Heading change ({diff:F3} rad) is too large between consecutive waypoints");
        }
    }

    #endregion

    #region All Paths Generation Tests

    /// <summary>
    /// Test that GenerateAllPaths returns multiple valid paths sorted by length
    /// </summary>
    [Fact]
    public void GenerateAllPaths_ReturnsSortedPaths()
    {
        // Arrange
        double startEasting = 0;
        double startNorthing = 0;
        double startHeading = 0;

        double goalEasting = 30;
        double goalNorthing = 30;
        double goalHeading = Math.PI / 2;

        // Act
        var paths = _service.GenerateAllPaths(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(paths);
        Assert.True(paths.Count > 0, "Should generate at least one path");
        Assert.True(paths.Count <= 6, "Should generate at most 6 paths");

        // Verify sorted by length
        for (int i = 0; i < paths.Count - 1; i++)
        {
            Assert.True(paths[i].TotalLength <= paths[i + 1].TotalLength,
                $"Paths should be sorted by length: path[{i}] = {paths[i].TotalLength:F2}, " +
                $"path[{i + 1}] = {paths[i + 1].TotalLength:F2}");
        }
    }

    /// <summary>
    /// Test that the shortest path from GeneratePath matches the first path from GenerateAllPaths
    /// </summary>
    [Fact]
    public void GeneratePath_MatchesFirstPathFromGenerateAllPaths()
    {
        // Arrange
        double startEasting = 10;
        double startNorthing = 10;
        double startHeading = Math.PI / 4;

        double goalEasting = 40;
        double goalNorthing = 40;
        double goalHeading = 3 * Math.PI / 4;

        // Act
        var singlePath = _service.GeneratePath(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        var allPaths = _service.GenerateAllPaths(
            startEasting, startNorthing, startHeading,
            goalEasting, goalNorthing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(singlePath);
        Assert.NotNull(allPaths);
        Assert.True(allPaths.Count > 0);

        var firstPath = allPaths[0];
        Assert.Equal(singlePath.PathType, firstPath.PathType);
        Assert.Equal(singlePath.TotalLength, firstPath.TotalLength, 2);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Test with very small turning radius
    /// </summary>
    [Fact]
    public void GeneratePath_SmallTurningRadius_ReturnsValidPath()
    {
        // Arrange
        double smallRadius = 1.0; // 1 meter

        // Act
        var path = _service.GeneratePath(
            0, 0, 0,
            10, 10, Math.PI / 2,
            smallRadius);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.TotalLength > 0);
    }

    /// <summary>
    /// Test with large turning radius
    /// </summary>
    [Fact]
    public void GeneratePath_LargeTurningRadius_ReturnsValidPath()
    {
        // Arrange
        double largeRadius = 50.0; // 50 meters

        // Act
        var path = _service.GeneratePath(
            0, 0, 0,
            100, 100, 0,
            largeRadius);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.TotalLength > 0);
    }

    /// <summary>
    /// Test with same start and goal position but different headings
    /// </summary>
    [Fact]
    public void GeneratePath_SamePositionDifferentHeading_ReturnsValidPath()
    {
        // Arrange: Turn in place
        double easting = 10;
        double northing = 10;
        double startHeading = 0;
        double goalHeading = Math.PI; // 180 degree turn

        // Act
        var path = _service.GeneratePath(
            easting, northing, startHeading,
            easting, northing, goalHeading,
            TurningRadius);

        // Assert
        Assert.NotNull(path);
        Assert.True(path.TotalLength > 0, "Should generate path for in-place turn");
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Test that path generation completes in reasonable time
    /// Target: <50ms for single path generation
    /// </summary>
    [Fact]
    public void GeneratePath_Performance_CompletesQuickly()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            _service.GeneratePath(
                0, 0, 0,
                30, 30, Math.PI / 2,
                TurningRadius);
        }

        stopwatch.Stop();

        // Assert
        double avgTime = stopwatch.ElapsedMilliseconds / 100.0;
        Assert.True(avgTime < 50,
            $"Average path generation time ({avgTime:F2}ms) should be < 50ms");
    }

    #endregion
}
