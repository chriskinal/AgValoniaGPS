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
            // Calculate heading direction from reference to previous tool position
            double headingToTool = Math.Atan2(
                previousToolPosition.Easting - referencePosition.Easting,
                previousToolPosition.Northing - referencePosition.Northing);

            if (headingToTool < 0) headingToTool += TwoPi;

            // Check for jackknife using PREVIOUS tool heading vs reference heading
            bool isJackknifed = IsJackknifed(previousToolPosition.Heading, referenceHeading, 1.9);

            if (!isJackknifed)
            {
                // Normal case: update tool position along the line from reference to previous position
                // Tool trails behind reference at the calculated heading
                double newEasting = referencePosition.Easting + (Math.Sin(headingToTool) * trailingHitchLength);
                double newNorthing = referencePosition.Northing + (Math.Cos(headingToTool) * trailingHitchLength);
                newToolPosition = new Position3D(newEasting, newNorthing, headingToTool);
            }
            else
            {
                // Jackknifed: force tool to align behind vehicle/tank at reference heading
                double newEasting = referencePosition.Easting - (Math.Sin(referenceHeading) * trailingHitchLength);
                double newNorthing = referencePosition.Northing - (Math.Cos(referenceHeading) * trailingHitchLength);
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
            // Calculate heading direction from hitch to previous tank position
            double headingToTank = Math.Atan2(
                previousTankPosition.Easting - currentHitchPosition.Easting,
                previousTankPosition.Northing - currentHitchPosition.Northing);

            if (headingToTank < 0) headingToTank += TwoPi;

            // Check for jackknife using PREVIOUS tank heading vs vehicle heading
            bool isJackknifed = IsJackknifed(previousTankPosition.Heading, vehicleHeading, 2.0);

            if (!isJackknifed)
            {
                // Normal case: update tank position along the line from hitch to previous position
                // Tank trails behind hitch at the calculated heading
                double newEasting = currentHitchPosition.Easting + (Math.Sin(headingToTank) * tankTrailingHitchLength);
                double newNorthing = currentHitchPosition.Northing + (Math.Cos(headingToTank) * tankTrailingHitchLength);
                newTankPosition = new Position3D(newEasting, newNorthing, headingToTank);
            }
            else
            {
                // Jackknifed: force tank to align behind vehicle at vehicle heading
                double newEasting = currentHitchPosition.Easting - (Math.Sin(vehicleHeading) * tankTrailingHitchLength);
                double newNorthing = currentHitchPosition.Northing - (Math.Cos(vehicleHeading) * tankTrailingHitchLength);
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

        // Then calculate working position from tank toward tool pivot
        // Working position is between tank and pivot (offset is less than full trail length)
        // Calculate in the direction of the tool pivot
        double offsetDistance = trailingHitchLength - toolToPivotLength;
        double workingEasting = currentTankPosition.Easting +
            (Math.Sin(toolPivot.Heading) * offsetDistance);
        double workingNorthing = currentTankPosition.Northing +
            (Math.Cos(toolPivot.Heading) * offsetDistance);

        Position3D toolWorkingPosition = new Position3D(workingEasting, workingNorthing, toolPivot.Heading);

        return (toolPivot, toolWorkingPosition);
    }

    /// <inheritdoc/>
    public bool IsJackknifed(double implementHeading, double referenceHeading, double jackknifThreshold = 1.9)
    {
        // Calculate the absolute angle difference using circular math
        // This handles the 0/2π wraparound correctly
        double angleDifference = Math.Abs(Pi - Math.Abs(Math.Abs(implementHeading - referenceHeading) - Pi));

        // Safety check: perpendicular (near 90°) is always jackknifed regardless of threshold
        // This prevents dangerous sideways implement configurations
        const double perpendicularTolerance = 0.17; // ~10 degrees tolerance around 90° (80-100°)
        bool isPerpendicular = Math.Abs(angleDifference - (Pi / 2.0)) < perpendicularTolerance;

        return angleDifference > jackknifThreshold || isPerpendicular;
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
