using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Enhanced Dubins path service with boundary-guided sampling.
/// Falls back to intelligent sampling when standard Dubins paths violate boundaries.
/// Performance target: <10ms even for complex scenarios.
/// </summary>
public interface IBoundaryGuidedDubinsService
{
    /// <summary>
    /// Generates a Dubins path with boundary awareness.
    /// First tries standard Dubins paths (fast).
    /// Falls back to boundary-guided sampling if needed.
    /// </summary>
    /// <param name="startEasting">Start easting coordinate</param>
    /// <param name="startNorthing">Start northing coordinate</param>
    /// <param name="startHeading">Start heading in radians</param>
    /// <param name="goalEasting">Goal easting coordinate</param>
    /// <param name="goalNorthing">Goal northing coordinate</param>
    /// <param name="goalHeading">Goal heading in radians</param>
    /// <param name="turningRadius">Minimum turning radius in meters</param>
    /// <param name="boundaries">Field boundaries and obstacles (closed polygons)</param>
    /// <param name="minBoundaryDistance">Minimum required distance from boundaries in meters</param>
    /// <param name="waypointSpacing">Spacing between waypoints along path in meters</param>
    /// <param name="maxIterations">Maximum sampling iterations (default 8 for <10ms budget)</param>
    /// <returns>Result with path and generation metadata</returns>
    BoundaryGuidedDubinsResult GenerateBoundaryAwarePath(
        double startEasting,
        double startNorthing,
        double startHeading,
        double goalEasting,
        double goalNorthing,
        double goalHeading,
        double turningRadius,
        List<List<Position2D>> boundaries,
        double minBoundaryDistance,
        double waypointSpacing = 0.1,
        int maxIterations = 8);

    /// <summary>
    /// Calculates repulsion vector from boundaries at a given point.
    /// Used to push sampled waypoints away from violations.
    /// </summary>
    /// <param name="point">Point to evaluate</param>
    /// <param name="boundaries">Field boundaries and obstacles</param>
    /// <param name="influenceRadius">Radius within which boundaries exert repulsion</param>
    /// <returns>Repulsion vector (zero if far from boundaries)</returns>
    Position2D CalculateBoundaryRepulsion(
        Position2D point,
        List<List<Position2D>> boundaries,
        double influenceRadius);

    /// <summary>
    /// Checks if a point is valid (inside field, outside obstacles, maintains distance).
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <param name="boundaries">Field boundaries and obstacles</param>
    /// <param name="minDistance">Minimum required distance from boundaries</param>
    /// <returns>True if point is valid</returns>
    bool IsPointValid(
        Position2D point,
        List<List<Position2D>> boundaries,
        double minDistance);
}
