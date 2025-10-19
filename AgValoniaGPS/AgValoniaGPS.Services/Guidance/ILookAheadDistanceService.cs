using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for calculating adaptive look-ahead distance for steering algorithms
/// Adapts distance based on vehicle speed, cross-track error, guidance line curvature, and vehicle type
/// </summary>
public interface ILookAheadDistanceService
{
    /// <summary>
    /// Calculate adaptive look-ahead distance for Pure Pursuit steering
    /// </summary>
    /// <param name="speed">Vehicle speed in m/s</param>
    /// <param name="crossTrackError">Current cross-track error in meters (absolute value)</param>
    /// <param name="guidanceLineCurvature">Curvature of guidance line in 1/meters (0 = straight)</param>
    /// <param name="vehicleType">Type of vehicle (affects scaling factor)</param>
    /// <param name="isAutoSteerActive">Whether AutoSteer is currently active</param>
    /// <returns>Look-ahead distance in meters (minimum 2.0m enforced)</returns>
    double CalculateLookAheadDistance(
        double speed,
        double crossTrackError,
        double guidanceLineCurvature,
        VehicleType vehicleType,
        bool isAutoSteerActive);

    /// <summary>
    /// Look-ahead calculation mode
    /// </summary>
    LookAheadMode Mode { get; set; }
}
