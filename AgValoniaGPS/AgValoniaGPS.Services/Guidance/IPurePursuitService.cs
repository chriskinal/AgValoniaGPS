using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for calculating steering angles using the Pure Pursuit algorithm.
/// Pure Pursuit is a path-following algorithm that calculates steering commands
/// to reach a goal point located at a look-ahead distance along the guidance line.
/// </summary>
public interface IPurePursuitService
{
    /// <summary>
    /// Calculates the steering angle required to reach the goal point using Pure Pursuit algorithm.
    /// Formula:
    /// - Alpha = angle from vehicle heading to goal point
    /// - Curvature = 2 * sin(alpha) / lookAheadDistance
    /// - SteeringAngle = atan(curvature * wheelbase)
    /// </summary>
    /// <param name="steerAxlePosition">Current position of the steer axle with heading</param>
    /// <param name="goalPoint">Target point on guidance line at look-ahead distance</param>
    /// <param name="speed">Current vehicle speed in m/s (not currently used in Pure Pursuit, but kept for consistency)</param>
    /// <param name="pivotDistanceError">Distance error at pivot point for integral control (meters)</param>
    /// <returns>Steering angle in degrees (positive = right turn, negative = left turn)</returns>
    double CalculateSteeringAngle(
        Position3D steerAxlePosition,
        Position3D goalPoint,
        double speed,
        double pivotDistanceError);

    /// <summary>
    /// Resets the integral accumulator to zero.
    /// Should be called when:
    /// - Switching from another algorithm
    /// - AutoSteer is disabled
    /// - Significant course correction occurs
    /// </summary>
    void ResetIntegral();

    /// <summary>
    /// Gets the current integral accumulator value.
    /// Used for monitoring and debugging integral control.
    /// </summary>
    double IntegralValue { get; }
}
