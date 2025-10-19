using System;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Stanley steering algorithm implementation
/// Combines heading error and cross-track error with integral control
/// Thread-safe for concurrent UI and guidance loop access
/// </summary>
public class StanleySteeringService : IStanleySteeringService
{
    private readonly VehicleConfiguration _config;
    private readonly object _integralLock = new object();
    private double _integralAccumulator = 0.0;
    private double _lastHeadingErrorSign = 0.0;

    private const double RadiansToDegrees = 180.0 / Math.PI;
    private const double MinimumSpeed = 0.1; // m/s - minimum speed to avoid division by zero

    public StanleySteeringService(VehicleConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Current value of the integral accumulator
    /// Thread-safe property access
    /// </summary>
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

    /// <summary>
    /// Calculate steering angle using Stanley algorithm
    /// Formula: steerAngle = headingError * K_h + atan(K_d * xte / speed) + integral
    /// Thread-safe for concurrent access
    /// </summary>
    public double CalculateSteeringAngle(
        double crossTrackError,
        double headingError,
        double speed,
        double pivotDistanceError,
        bool isReverse)
    {
        // Protect against division by zero
        double effectiveSpeed = Math.Max(Math.Abs(speed), MinimumSpeed);

        // Reverse mode: negate heading error
        double effectiveHeadingError = isReverse ? -headingError : headingError;

        // Heading component: heading error * gain
        double headingComponent = effectiveHeadingError * _config.StanleyHeadingErrorGain;

        // Distance component with speed adaptation
        // Speed adaptation: scale cross-track gain by (1 + 0.277 * (speed - 1)) when speed > 1 m/s
        double distanceGain = _config.StanleyDistanceErrorGain;
        if (effectiveSpeed > 1.0)
        {
            distanceGain *= (1.0 + 0.277 * (effectiveSpeed - 1.0));
        }

        double distanceComponent = Math.Atan(distanceGain * crossTrackError / effectiveSpeed);

        // Integral control with thread safety
        double integralComponent = 0.0;
        lock (_integralLock)
        {
            // Check if we should reset integral (heading error sign change = crossing line)
            double currentHeadingErrorSign = Math.Sign(effectiveHeadingError);
            if (_lastHeadingErrorSign != 0.0 && currentHeadingErrorSign != 0.0 &&
                _lastHeadingErrorSign != currentHeadingErrorSign)
            {
                _integralAccumulator = 0.0;
            }
            _lastHeadingErrorSign = currentHeadingErrorSign;

            // Accumulate integral when pivot distance error exceeds trigger threshold
            if (Math.Abs(pivotDistanceError) > _config.StanleyIntegralDistanceAwayTriggerAB)
            {
                _integralAccumulator += pivotDistanceError * _config.StanleyIntegralGainAB;
            }

            integralComponent = _integralAccumulator;
        }

        // Combine components (in radians)
        double steerAngleRadians = headingComponent + distanceComponent + integralComponent;

        // Convert to degrees
        double steerAngleDegrees = steerAngleRadians * RadiansToDegrees;

        // Clamp to max steer angle
        steerAngleDegrees = Math.Clamp(steerAngleDegrees, -_config.MaxSteerAngle, _config.MaxSteerAngle);

        return steerAngleDegrees;
    }

    /// <summary>
    /// Reset the integral accumulator
    /// Thread-safe operation
    /// </summary>
    public void ResetIntegral()
    {
        lock (_integralLock)
        {
            _integralAccumulator = 0.0;
            _lastHeadingErrorSign = 0.0;
        }
    }
}
