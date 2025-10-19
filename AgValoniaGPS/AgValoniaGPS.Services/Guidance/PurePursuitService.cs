using System;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Implementation of Pure Pursuit steering algorithm for agricultural guidance.
/// Pure Pursuit calculates steering commands by finding a goal point at a look-ahead
/// distance along the guidance line and computing the curvature needed to reach it.
///
/// Algorithm:
/// 1. Calculate angle (alpha) from vehicle heading to goal point
/// 2. Calculate curvature: 2 * sin(alpha) / lookAheadDistance
/// 3. Calculate steering angle: atan(curvature * wheelbase)
/// 4. Apply integral control to eliminate steady-state error
/// 5. Clamp to maximum steering angle limits
/// </summary>
public class PurePursuitService : IPurePursuitService
{
    private readonly VehicleConfiguration _config;
    private readonly object _integralLock = new object();
    private double _integralAccumulator = 0.0;

    public PurePursuitService(VehicleConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    public double CalculateSteeringAngle(
        Position3D steerAxlePosition,
        Position3D goalPoint,
        double speed,
        double pivotDistanceError)
    {
        // Calculate look-ahead distance from steer position to goal point
        double dx = goalPoint.Easting - steerAxlePosition.Easting;
        double dy = goalPoint.Northing - steerAxlePosition.Northing;
        double lookAheadDistance = Math.Sqrt(dx * dx + dy * dy);

        // Zero-distance protection: return 0 if goal point is too close
        if (lookAheadDistance < 0.1)
        {
            return 0.0;
        }

        // Calculate alpha: angle from vehicle heading to goal point
        // Use atan2(dx, dy) to get angle clockwise from North (matching heading convention)
        double angleToGoal = Math.Atan2(dx, dy);
        double alpha = angleToGoal - steerAxlePosition.Heading;

        // Normalize alpha to [-π, π]
        while (alpha > Math.PI) alpha -= 2.0 * Math.PI;
        while (alpha < -Math.PI) alpha += 2.0 * Math.PI;

        // Calculate curvature using Pure Pursuit formula
        // curvature = 2 * sin(alpha) / lookAheadDistance
        double curvature = 2.0 * Math.Sin(alpha) / lookAheadDistance;

        // Calculate steering angle from curvature
        // steerAngle = atan(curvature * wheelbase)
        double steerAngleRad = Math.Atan(curvature * _config.Wheelbase);

        // Convert to degrees
        double steerAngleDeg = steerAngleRad * 180.0 / Math.PI;

        // Apply integral control to pivot distance error
        double integralComponent = 0.0;
        lock (_integralLock)
        {
            // Accumulate integral only if gain is non-zero
            if (Math.Abs(_config.PurePursuitIntegralGain) > 0.001)
            {
                _integralAccumulator += pivotDistanceError * _config.PurePursuitIntegralGain;
                integralComponent = _integralAccumulator;
            }
        }

        // Add integral component
        steerAngleDeg += integralComponent;

        // Clamp to maximum steering angle
        if (steerAngleDeg > _config.MaxSteerAngle)
        {
            steerAngleDeg = _config.MaxSteerAngle;
        }
        else if (steerAngleDeg < -_config.MaxSteerAngle)
        {
            steerAngleDeg = -_config.MaxSteerAngle;
        }

        return steerAngleDeg;
    }

    /// <inheritdoc/>
    public void ResetIntegral()
    {
        lock (_integralLock)
        {
            _integralAccumulator = 0.0;
        }
    }

    /// <inheritdoc/>
    public double IntegralValue
    {
        get
        {
            lock (_integralLock)
            {
                return _integralAccumulator;
            }
        }
    }
}
