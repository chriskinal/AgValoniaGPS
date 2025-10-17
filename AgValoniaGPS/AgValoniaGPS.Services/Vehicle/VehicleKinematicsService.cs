using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services.Vehicle;

/// <summary>
/// Implements vehicle kinematics calculations for multi-articulated agricultural equipment.
/// </summary>
/// <remarks>
/// <para>
/// This service transforms GPS antenna positions to various vehicle reference points:
/// - Pivot axle (rear axle center - vehicle reference point)
/// - Steer axle (front axle - steering point)
/// - Hitch (implement attachment point)
/// - Tool positions (rigid or articulated)
/// - Tank positions (for tank-between-tractor configurations)
/// </para>
/// <para>
/// All calculations use a coordinate system where:
/// - Heading 0 = North
/// - Heading increases clockwise
/// - Easting increases to the East
/// - Northing increases to the North
/// </para>
/// <para>
/// Thread-safe: All methods are stateless pure functions.
/// Performance: Typical method execution &lt;1ms on modern hardware.
/// </para>
/// </remarks>
public class VehicleKinematicsService : IVehicleKinematicsService
{
    private const double TwoPi = 2.0 * Math.PI;
    private const double Pi = Math.PI;

    /// <inheritdoc/>
    public Position3D CalculatePivotPosition(Position2D gpsPosition, double heading, double antennaPivotDistance)
    {
        // Transform GPS antenna position to pivot axle by subtracting antenna offset
        // The antenna is typically ahead of the pivot (positive antennaPivot)
        double pivotEasting = gpsPosition.Easting - (Math.Sin(heading) * antennaPivotDistance);
        double pivotNorthing = gpsPosition.Northing - (Math.Cos(heading) * antennaPivotDistance);

        return new Position3D(pivotEasting, pivotNorthing, heading);
    }

    /// <inheritdoc/>
    public Position3D CalculateSteerAxlePosition(Position3D pivotPosition, double heading, double wheelbase)
    {
        // Transform from pivot axle to steer axle by adding wheelbase
        // Steer axle is ahead of pivot by wheelbase distance
        double steerEasting = pivotPosition.Easting + (Math.Sin(heading) * wheelbase);
        double steerNorthing = pivotPosition.Northing + (Math.Cos(heading) * wheelbase);

        return new Position3D(steerEasting, steerNorthing, heading);
    }

    /// <inheritdoc/>
    public Position2D CalculateHitchPosition(Position2D gpsPosition, double heading, double hitchLength, double antennaPivotDistance)
    {
        // Calculate hitch position from GPS antenna
        // Hitch is behind pivot, so we use (hitchLength - antennaPivot)
        double hitchEasting = gpsPosition.Easting + (Math.Sin(heading) * (hitchLength - antennaPivotDistance));
        double hitchNorthing = gpsPosition.Northing + (Math.Cos(heading) * (hitchLength - antennaPivotDistance));

        return new Position2D(hitchEasting, hitchNorthing);
    }

    /// <inheritdoc/>
    public Position3D CalculateRigidToolPosition(Position2D hitchPosition, double heading)
    {
        // For rigidly-attached implements, tool is at hitch with vehicle heading
        return new Position3D(hitchPosition.Easting, hitchPosition.Northing, heading);
    }

    /// <inheritdoc/>
    public Position3D CalculateTrailingToolPosition(
        Position2D currentHitchPosition,
        Position3D previousToolPosition,
        double trailingHitchLength,
        double distanceMoved,
        double vehicleHeading,
        Position3D? tankPosition)
    {
        // For trailing implements, calculate heading based on movement
        Position3D newToolPosition;

        // Use tank position as reference if TBT configuration
        Position2D referencePosition = tankPosition?.Position2D ?? currentHitchPosition;
        double referenceHeading = tankPosition?.Heading ?? vehicleHeading;

        // If vehicle has moved, update tool heading based on hitch-to-tool vector
        if (distanceMoved > 0.0001) // Minimum movement threshold
        {
            // Calculate new heading from reference to previous tool position
            double toolHeading = Math.Atan2(
                referencePosition.Easting - previousToolPosition.Easting,
                referencePosition.Northing - previousToolPosition.Northing);

            if (toolHeading < 0) toolHeading += TwoPi;

            // Check for jackknife condition
            bool isJackknifed = IsJackknifed(toolHeading, referenceHeading, 1.9);

            if (!isJackknifed)
            {
                // Normal case: update tool position along calculated heading
                double newEasting = referencePosition.Easting + (Math.Sin(toolHeading) * trailingHitchLength);
                double newNorthing = referencePosition.Northing + (Math.Cos(toolHeading) * trailingHitchLength);
                newToolPosition = new Position3D(newEasting, newNorthing, toolHeading);
            }
            else
            {
                // Jackknifed: force tool to align behind vehicle/tank
                double newEasting = referencePosition.Easting + (Math.Sin(referenceHeading) * trailingHitchLength);
                double newNorthing = referencePosition.Northing + (Math.Cos(referenceHeading) * trailingHitchLength);
                newToolPosition = new Position3D(newEasting, newNorthing, referenceHeading);
            }
        }
        else
        {
            // No movement: keep previous position
            newToolPosition = previousToolPosition;
        }

        return newToolPosition;
    }

    /// <inheritdoc/>
    public Position3D CalculateTankPosition(
        Position2D currentHitchPosition,
        Position3D previousTankPosition,
        double tankTrailingHitchLength,
        double distanceMoved,
        double vehicleHeading)
    {
        // Tank calculation is similar to trailing tool but with different jackknife threshold
        Position3D newTankPosition;

        if (distanceMoved > 0.0001) // Minimum movement threshold
        {
            // Calculate new heading from hitch to previous tank position
            double tankHeading = Math.Atan2(
                currentHitchPosition.Easting - previousTankPosition.Easting,
                currentHitchPosition.Northing - previousTankPosition.Northing);

            if (tankHeading < 0) tankHeading += TwoPi;

            // Check for jackknife condition (tank uses 2.0 radians threshold)
            bool isJackknifed = IsJackknifed(tankHeading, vehicleHeading, 2.0);

            if (!isJackknifed)
            {
                // Normal case: update tank position
                double newEasting = currentHitchPosition.Easting + (Math.Sin(tankHeading) * tankTrailingHitchLength);
                double newNorthing = currentHitchPosition.Northing + (Math.Cos(tankHeading) * tankTrailingHitchLength);
                newTankPosition = new Position3D(newEasting, newNorthing, tankHeading);
            }
            else
            {
                // Jackknifed: force tank to align behind vehicle
                double newEasting = currentHitchPosition.Easting + (Math.Sin(vehicleHeading) * tankTrailingHitchLength);
                double newNorthing = currentHitchPosition.Northing + (Math.Cos(vehicleHeading) * tankTrailingHitchLength);
                newTankPosition = new Position3D(newEasting, newNorthing, vehicleHeading);
            }
        }
        else
        {
            // No movement: keep previous position
            newTankPosition = previousTankPosition;
        }

        return newTankPosition;
    }

    /// <inheritdoc/>
    public (Position3D toolPivot, Position3D toolWorkingPosition) CalculateTBTToolPosition(
        Position3D currentTankPosition,
        Position3D previousToolPosition,
        double trailingHitchLength,
        double toolToPivotLength,
        double distanceMoved,
        double tankHeading)
    {
        // First calculate tool pivot from tank position
        Position3D toolPivot = CalculateTrailingToolPosition(
            currentTankPosition.Position2D,
            previousToolPosition,
            trailingHitchLength,
            distanceMoved,
            tankHeading,
            null); // No tank reference for tool-from-tank calculation

        // Then calculate working position from tool pivot
        // Tool working position is offset from pivot point
        double workingEasting = currentTankPosition.Easting +
            (Math.Sin(toolPivot.Heading) * (trailingHitchLength - toolToPivotLength));
        double workingNorthing = currentTankPosition.Northing +
            (Math.Cos(toolPivot.Heading) * (trailingHitchLength - toolToPivotLength));

        Position3D toolWorkingPosition = new Position3D(workingEasting, workingNorthing, toolPivot.Heading);

        return (toolPivot, toolWorkingPosition);
    }

    /// <inheritdoc/>
    public bool IsJackknifed(double implementHeading, double referenceHeading, double jackknifThreshold = 1.9)
    {
        // Calculate the absolute angle difference using circular math
        // This handles the 0/2π wraparound correctly
        double angleDifference = Math.Abs(Pi - Math.Abs(Math.Abs(implementHeading - referenceHeading) - Pi));

        return angleDifference > jackknifThreshold;
    }

    /// <inheritdoc/>
    public Position2D CalculateLookAheadPosition(
        Position3D pivotPosition,
        double heading,
        double toolWidth,
        double speed,
        double lookAheadTime)
    {
        // Look ahead distance is the greater of:
        // 1. Half the tool width (ensures at least tool width look ahead)
        // 2. Speed-based look ahead (speed * time)
        double lookAheadDistance = Math.Max(toolWidth * 0.5, speed * lookAheadTime);

        // Calculate look ahead position from pivot
        double lookAheadEasting = pivotPosition.Easting + (Math.Sin(heading) * lookAheadDistance);
        double lookAheadNorthing = pivotPosition.Northing + (Math.Cos(heading) * lookAheadDistance);

        return new Position2D(lookAheadEasting, lookAheadNorthing);
    }
}
