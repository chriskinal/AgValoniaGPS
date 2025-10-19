using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for coordinating steering calculations and PGN output.
/// Routes calculations to active steering algorithm (Stanley or Pure Pursuit)
/// and transmits steering commands via PGN 254 messages.
/// </summary>
public interface ISteeringCoordinatorService
{
    /// <summary>
    /// Gets or sets the active steering algorithm.
    /// Switching algorithms automatically resets integrals in both services.
    /// </summary>
    SteeringAlgorithm ActiveAlgorithm { get; set; }

    /// <summary>
    /// Updates steering calculation based on current position and guidance data.
    /// Calculates steering angle using active algorithm and sends PGN 254 message.
    /// Performance target: <5ms execution time.
    /// </summary>
    /// <param name="pivotPosition">Vehicle pivot point position</param>
    /// <param name="steerPosition">Vehicle steer axle position</param>
    /// <param name="guidanceResult">Guidance line calculation result with cross-track error</param>
    /// <param name="speed">Current vehicle speed in m/s</param>
    /// <param name="heading">Current vehicle heading in radians</param>
    /// <param name="isAutoSteerActive">Whether AutoSteer is currently active</param>
    void Update(
        Position3D pivotPosition,
        Position3D steerPosition,
        GuidanceLineResult guidanceResult,
        double speed,
        double heading,
        bool isAutoSteerActive);

    /// <summary>
    /// Gets the current calculated steering angle in degrees.
    /// Positive = turn right, negative = turn left.
    /// </summary>
    double CurrentSteeringAngle { get; }

    /// <summary>
    /// Gets the current cross-track error in meters.
    /// Positive = right of line, negative = left of line.
    /// </summary>
    double CurrentCrossTrackError { get; }

    /// <summary>
    /// Gets the current look-ahead distance in meters (for Pure Pursuit).
    /// </summary>
    double CurrentLookAheadDistance { get; }

    /// <summary>
    /// Event raised when steering values are updated.
    /// Provides current steering angle, cross-track error, and look-ahead distance for UI display.
    /// </summary>
    event EventHandler<SteeringUpdateEventArgs>? SteeringUpdated;
}
