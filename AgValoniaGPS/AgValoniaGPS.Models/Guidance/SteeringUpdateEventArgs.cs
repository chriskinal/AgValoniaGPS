using System;

namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Event arguments for steering update events.
/// Published by SteeringCoordinatorService after each Update() call
/// to provide current steering values for UI display and logging.
/// </summary>
public class SteeringUpdateEventArgs : EventArgs
{
    /// <summary>
    /// Calculated steering angle in degrees.
    /// Positive = turn right, negative = turn left.
    /// </summary>
    public double SteeringAngle { get; init; }

    /// <summary>
    /// Current cross-track error in meters.
    /// Positive = right of line, negative = left of line.
    /// </summary>
    public double CrossTrackError { get; init; }

    /// <summary>
    /// Current look-ahead distance in meters (for Pure Pursuit algorithm).
    /// </summary>
    public double LookAheadDistance { get; init; }

    /// <summary>
    /// Active steering algorithm used for this calculation.
    /// </summary>
    public SteeringAlgorithm Algorithm { get; init; }

    /// <summary>
    /// Timestamp when the steering update was calculated.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Whether AutoSteer was active during this update.
    /// </summary>
    public bool IsAutoSteerActive { get; init; }
}
