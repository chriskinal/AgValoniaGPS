namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Interface for Stanley steering algorithm
/// Calculates steering angle using cross-track error and heading error
/// Formula: steerAngle = headingError * K_h + atan(K_d * xte / speed)
/// </summary>
public interface IStanleySteeringService
{
    /// <summary>
    /// Calculate steering angle using Stanley algorithm
    /// </summary>
    /// <param name="crossTrackError">Cross-track error in meters (positive = right of line)</param>
    /// <param name="headingError">Heading error in radians (vehicle heading - line heading)</param>
    /// <param name="speed">Vehicle speed in m/s</param>
    /// <param name="pivotDistanceError">Distance error at pivot point in meters (for integral control)</param>
    /// <param name="isReverse">True if vehicle is in reverse</param>
    /// <returns>Steering angle in degrees</returns>
    double CalculateSteeringAngle(
        double crossTrackError,
        double headingError,
        double speed,
        double pivotDistanceError,
        bool isReverse);

    /// <summary>
    /// Reset the integral accumulator
    /// </summary>
    void ResetIntegral();

    /// <summary>
    /// Current value of the integral accumulator
    /// </summary>
    double IntegralValue { get; }
}
