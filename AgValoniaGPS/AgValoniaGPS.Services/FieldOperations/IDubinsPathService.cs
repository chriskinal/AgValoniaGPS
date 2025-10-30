using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using System.Collections.Generic;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for generating Dubins paths - the shortest path between two poses
/// (position + heading) with a given turning radius.
/// </summary>
/// <remarks>
/// A Dubins path consists of at most three segments: two circular arcs and one straight line.
/// There are six possible path types: RSR, LSL, RSL, LSR, RLR, LRL.
///
/// Special thanks to erik.nordeus@gmail.com for the original Unity implementation
/// (http://www.habrador.com/about/) which was adapted for AgValoniaGPS.
/// </remarks>
public interface IDubinsPathService
{
    /// <summary>
    /// Generates a Dubins path from start pose to goal pose.
    /// Returns the shortest valid path with waypoints.
    /// </summary>
    /// <param name="startEasting">Starting easting coordinate in meters</param>
    /// <param name="startNorthing">Starting northing coordinate in meters</param>
    /// <param name="startHeading">Starting heading in radians (0 = North, clockwise positive)</param>
    /// <param name="goalEasting">Goal easting coordinate in meters</param>
    /// <param name="goalNorthing">Goal northing coordinate in meters</param>
    /// <param name="goalHeading">Goal heading in radians</param>
    /// <param name="turningRadius">Minimum turning radius in meters</param>
    /// <param name="waypointSpacing">Distance between waypoints in meters (default 0.05m)</param>
    /// <returns>
    /// The shortest Dubins path, or null if no valid path exists.
    /// The returned path includes a list of waypoints with positions and headings.
    /// </returns>
    DubinsPath? GeneratePath(
        double startEasting,
        double startNorthing,
        double startHeading,
        double goalEasting,
        double goalNorthing,
        double goalHeading,
        double turningRadius,
        double waypointSpacing = 0.05);

    /// <summary>
    /// Generates all valid Dubins paths (up to 6) sorted by length.
    /// Useful for analyzing different path options or debugging.
    /// </summary>
    /// <param name="startEasting">Starting easting coordinate in meters</param>
    /// <param name="startNorthing">Starting northing coordinate in meters</param>
    /// <param name="startHeading">Starting heading in radians</param>
    /// <param name="goalEasting">Goal easting coordinate in meters</param>
    /// <param name="goalNorthing">Goal northing coordinate in meters</param>
    /// <param name="goalHeading">Goal heading in radians</param>
    /// <param name="turningRadius">Minimum turning radius in meters</param>
    /// <param name="waypointSpacing">Distance between waypoints in meters</param>
    /// <returns>List of all valid paths sorted by total length (shortest first)</returns>
    List<DubinsPath> GenerateAllPaths(
        double startEasting,
        double startNorthing,
        double startHeading,
        double goalEasting,
        double goalNorthing,
        double goalHeading,
        double turningRadius,
        double waypointSpacing = 0.05);
}
