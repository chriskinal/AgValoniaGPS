using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Interfaces;

/// <summary>
/// Provides vehicle kinematics calculations for multi-articulated agricultural equipment.
/// Transforms GPS antenna position to pivot, steer axle, tool, and articulated implement positions.
/// </summary>
public interface IVehicleKinematicsService
{
    /// <summary>
    /// Calculates the pivot axle position from GPS antenna position.
    /// The pivot point is the reference point on the vehicle (typically rear axle center).
    /// </summary>
    /// <param name="gpsPosition">GPS antenna position in UTM coordinates</param>
    /// <param name="heading">Vehicle heading in radians</param>
    /// <param name="antennaPivotDistance">Distance from antenna to pivot point in meters (positive = antenna ahead of pivot)</param>
    /// <returns>Pivot axle position with heading</returns>
    /// <remarks>
    /// Formula: pivot = gps - (sin(heading) * antennaPivot, cos(heading) * antennaPivot)
    /// Thread-safe. Typical execution time: &lt;0.5ms
    /// </remarks>
    Position3D CalculatePivotPosition(Position2D gpsPosition, double heading, double antennaPivotDistance);

    /// <summary>
    /// Calculates the steer axle position from pivot axle position.
    /// The steer axle is the front axle where steering occurs.
    /// </summary>
    /// <param name="pivotPosition">Pivot axle position</param>
    /// <param name="heading">Vehicle heading in radians</param>
    /// <param name="wheelbase">Distance from pivot to steer axle in meters</param>
    /// <returns>Steer axle position with heading</returns>
    /// <remarks>
    /// Formula: steerAxle = pivot + (sin(heading) * wheelbase, cos(heading) * wheelbase)
    /// Thread-safe. Typical execution time: &lt;0.5ms
    /// </remarks>
    Position3D CalculateSteerAxlePosition(Position3D pivotPosition, double heading, double wheelbase);

    /// <summary>
    /// Calculates the hitch position from GPS antenna position.
    /// The hitch is the attachment point for implements.
    /// </summary>
    /// <param name="gpsPosition">GPS antenna position</param>
    /// <param name="heading">Vehicle heading in radians</param>
    /// <param name="hitchLength">Distance from GPS to hitch point in meters</param>
    /// <param name="antennaPivotDistance">Distance from antenna to pivot in meters</param>
    /// <returns>Hitch position (2D only, no heading)</returns>
    /// <remarks>
    /// Formula: hitch = gps + (sin(heading) * (hitchLength - antennaPivot), cos(heading) * (hitchLength - antennaPivot))
    /// Thread-safe. Typical execution time: &lt;0.5ms
    /// </remarks>
    Position2D CalculateHitchPosition(Position2D gpsPosition, double heading, double hitchLength, double antennaPivotDistance);

    /// <summary>
    /// Calculates tool pivot position for rigidly-attached implements.
    /// For rigid tools, the tool pivot is at the hitch position.
    /// </summary>
    /// <param name="hitchPosition">Hitch position</param>
    /// <param name="heading">Vehicle heading in radians</param>
    /// <returns>Tool pivot position with heading (same as hitch for rigid tools)</returns>
    /// <remarks>
    /// For rigid implements, tool position = hitch position with vehicle heading.
    /// Thread-safe. Typical execution time: &lt;0.1ms
    /// </remarks>
    Position3D CalculateRigidToolPosition(Position2D hitchPosition, double heading);

    /// <summary>
    /// Calculates tool pivot position for trailing articulated implements.
    /// Uses kinematic model to track tool position based on movement history.
    /// </summary>
    /// <param name="currentHitchPosition">Current hitch position</param>
    /// <param name="previousToolPosition">Previous tool pivot position from last update</param>
    /// <param name="trailingHitchLength">Length of trailing hitch in meters</param>
    /// <param name="distanceMoved">Distance moved since last update in meters</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians (for jackknife detection)</param>
    /// <param name="tankPosition">Tank position if using tank-between-tractor (TBT), null otherwise</param>
    /// <returns>New tool pivot position with heading</returns>
    /// <remarks>
    /// <para>
    /// Uses heading tracking: toolHeading = atan2(hitch - toolPivot).
    /// Implements jackknife prevention - if articulation angle > 1.9 radians (~109°),
    /// forces tool to align behind vehicle.
    /// </para>
    /// <para>
    /// Thread-safe. Typical execution time: &lt;1ms
    /// </para>
    /// </remarks>
    Position3D CalculateTrailingToolPosition(
        Position2D currentHitchPosition,
        Position3D previousToolPosition,
        double trailingHitchLength,
        double distanceMoved,
        double vehicleHeading,
        Position3D? tankPosition);

    /// <summary>
    /// Calculates tank position for Tank-Between-Tractor (TBT) multi-articulated implements.
    /// The tank is an intermediate articulation point between tractor and tool.
    /// </summary>
    /// <param name="currentHitchPosition">Current hitch position</param>
    /// <param name="previousTankPosition">Previous tank position from last update</param>
    /// <param name="tankTrailingHitchLength">Length from hitch to tank in meters</param>
    /// <param name="distanceMoved">Distance moved since last update in meters</param>
    /// <param name="vehicleHeading">Current vehicle heading in radians (for jackknife detection)</param>
    /// <returns>New tank position with heading</returns>
    /// <remarks>
    /// <para>
    /// Similar to trailing tool calculation but with different jackknife threshold (2.0 radians ~114°).
    /// The tank position is used as input for calculating final tool position in TBT configurations.
    /// </para>
    /// <para>
    /// Thread-safe. Typical execution time: &lt;1ms
    /// </para>
    /// </remarks>
    Position3D CalculateTankPosition(
        Position2D currentHitchPosition,
        Position3D previousTankPosition,
        double tankTrailingHitchLength,
        double distanceMoved,
        double vehicleHeading);

    /// <summary>
    /// Calculates final tool position for TBT (Tank-Between-Tractor) configuration.
    /// </summary>
    /// <param name="currentTankPosition">Current tank position</param>
    /// <param name="previousToolPosition">Previous tool position</param>
    /// <param name="trailingHitchLength">Total hitch length from tank to tool pivot</param>
    /// <param name="toolToPivotLength">Distance from tool working position to pivot point</param>
    /// <param name="distanceMoved">Distance moved since last update</param>
    /// <param name="tankHeading">Current tank heading in radians</param>
    /// <returns>Tool position and working position with heading</returns>
    /// <remarks>
    /// <para>
    /// For TBT: First calculates tool pivot from tank, then adjusts for working position offset.
    /// Tool working position = tank + (sin(toolHeading) * (trailingHitch - toolToPivot), cos(toolHeading) * (trailingHitch - toolToPivot))
    /// </para>
    /// <para>
    /// Thread-safe. Typical execution time: &lt;1ms
    /// </para>
    /// </remarks>
    (Position3D toolPivot, Position3D toolWorkingPosition) CalculateTBTToolPosition(
        Position3D currentTankPosition,
        Position3D previousToolPosition,
        double trailingHitchLength,
        double toolToPivotLength,
        double distanceMoved,
        double tankHeading);

    /// <summary>
    /// Checks if implement is jackknifed (articulation angle too extreme).
    /// </summary>
    /// <param name="implementHeading">Heading of the implement in radians</param>
    /// <param name="referenceHeading">Reference heading (vehicle or tank) in radians</param>
    /// <param name="jackknifThreshold">Threshold angle in radians (default 1.9 for tool, 2.0 for tank)</param>
    /// <returns>True if jackknifed (angle difference > threshold)</returns>
    /// <remarks>
    /// <para>
    /// Calculates: angle = |PI - ||implementHeading - referenceHeading| - PI||
    /// If angle > threshold, implement is jackknifed and should be reset.
    /// </para>
    /// <para>
    /// Typical thresholds:
    /// - Tool: 1.9 radians (~109°)
    /// - Tank: 2.0 radians (~114°)
    /// </para>
    /// <para>
    /// Thread-safe. Typical execution time: &lt;0.1ms
    /// </para>
    /// </remarks>
    bool IsJackknifed(double implementHeading, double referenceHeading, double jackknifThreshold = 1.9);

    /// <summary>
    /// Calculates guidance look-ahead position.
    /// Used for pure pursuit and other look-ahead steering algorithms.
    /// </summary>
    /// <param name="pivotPosition">Current pivot position</param>
    /// <param name="heading">Vehicle heading in radians</param>
    /// <param name="toolWidth">Tool width in meters</param>
    /// <param name="speed">Vehicle speed in m/s</param>
    /// <param name="lookAheadTime">Look-ahead time in seconds</param>
    /// <returns>Look-ahead position</returns>
    /// <remarks>
    /// <para>
    /// Distance = max(toolWidth * 0.5, speed * lookAheadTime)
    /// Position = pivot + (sin(heading) * distance, cos(heading) * distance)
    /// </para>
    /// <para>
    /// Thread-safe. Typical execution time: &lt;0.5ms
    /// </para>
    /// </remarks>
    Position2D CalculateLookAheadPosition(
        Position3D pivotPosition,
        double heading,
        double toolWidth,
        double speed,
        double lookAheadTime);
}
