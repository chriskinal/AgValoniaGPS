using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for calculating adaptive look-ahead distance for steering algorithms
/// Implements speed-based, cross-track error adaptive, and curvature-aware look-ahead calculation
/// Performance target: <1ms per calculation
/// </summary>
public class LookAheadDistanceService : ILookAheadDistanceService
{
    private const double MinimumLookAheadDistance = 2.0; // meters
    private const double OnLineThreshold = 0.1; // meters - considered "on line"
    private const double AcquireThreshold = 0.4; // meters - full acquisition mode
    private const double CurvatureReductionThreshold = 0.05; // 1/meters - reduce look-ahead in tight curves
    private const double CurvatureReductionFactor = 0.8; // reduce by 20% in curves
    private const double HarvesterScalingFactor = 1.1; // 10% larger for combines
    private const double FixedHoldDistance = 5.0; // meters when AutoSteer is off

    private readonly VehicleConfiguration _configuration;
    private LookAheadMode _mode = LookAheadMode.ToolWidthMultiplier;

    public LookAheadDistanceService(VehicleConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Look-ahead calculation mode
    /// </summary>
    public LookAheadMode Mode
    {
        get => _mode;
        set => _mode = value;
    }

    /// <summary>
    /// Calculate adaptive look-ahead distance for Pure Pursuit steering
    /// Adapts based on: speed, cross-track error zones, curvature, and vehicle type
    /// </summary>
    public double CalculateLookAheadDistance(
        double speed,
        double crossTrackError,
        double guidanceLineCurvature,
        VehicleType vehicleType,
        bool isAutoSteerActive)
    {
        // When AutoSteer is off, use fixed hold distance
        if (!isAutoSteerActive)
        {
            return FixedHoldDistance;
        }

        // Calculate base distance based on mode
        double baseDistance = _mode switch
        {
            LookAheadMode.Hold => _configuration.GoalPointLookAheadHold,
            LookAheadMode.TimeBased => CalculateTimeBasedDistance(speed),
            LookAheadMode.ToolWidthMultiplier => CalculateToolWidthBasedDistance(speed),
            _ => CalculateToolWidthBasedDistance(speed)
        };

        // Apply cross-track error adaptation
        double xteAdaptedDistance = ApplyCrossTrackErrorAdaptation(baseDistance, crossTrackError);

        // Apply curvature reduction for tight curves
        double curveAdaptedDistance = ApplyCurvatureAdaptation(xteAdaptedDistance, guidanceLineCurvature);

        // Apply vehicle type scaling
        double vehicleScaledDistance = ApplyVehicleTypeScaling(curveAdaptedDistance, vehicleType);

        // Enforce minimum distance
        return Math.Max(vehicleScaledDistance, _configuration.MinLookAheadDistance);
    }

    /// <summary>
    /// Calculate base distance using speed * multiplier formula
    /// </summary>
    private double CalculateToolWidthBasedDistance(double speed)
    {
        // Base formula: speed * 0.05 * lookAheadMult
        // Using GoalPointLookAheadMult from configuration
        return speed * 0.05 * _configuration.GoalPointLookAheadMult;
    }

    /// <summary>
    /// Calculate time-based distance (speed * time horizon)
    /// </summary>
    private double CalculateTimeBasedDistance(double speed)
    {
        // Use look-ahead multiplier as time factor (seconds)
        return speed * _configuration.GoalPointLookAheadMult;
    }

    /// <summary>
    /// Adapt look-ahead distance based on cross-track error zones
    /// Three zones: on-line (≤0.1m), transition (0.1-0.4m), acquire (≥0.4m)
    /// </summary>
    private double ApplyCrossTrackErrorAdaptation(double baseDistance, double crossTrackError)
    {
        double absXte = Math.Abs(crossTrackError);

        // Zone 1: On-line mode (XTE ≤ 0.1m) - use hold distance
        if (absXte <= OnLineThreshold)
        {
            return _configuration.GoalPointLookAheadHold;
        }

        // Zone 3: Acquire mode (XTE ≥ 0.4m) - use hold * acquire factor
        if (absXte >= AcquireThreshold)
        {
            return _configuration.GoalPointLookAheadHold * _configuration.GoalPointAcquireFactor;
        }

        // Zone 2: Transition zone (0.1m < XTE < 0.4m) - interpolate
        double holdDistance = _configuration.GoalPointLookAheadHold;
        double acquireDistance = holdDistance * _configuration.GoalPointAcquireFactor;

        // Linear interpolation factor (0.0 at OnLineThreshold, 1.0 at AcquireThreshold)
        double interpolationFactor = (absXte - OnLineThreshold) / (AcquireThreshold - OnLineThreshold);

        return holdDistance + (acquireDistance - holdDistance) * interpolationFactor;
    }

    /// <summary>
    /// Reduce look-ahead distance in tight curves for better tracking
    /// </summary>
    private double ApplyCurvatureAdaptation(double distance, double curvature)
    {
        double absCurvature = Math.Abs(curvature);

        // If curvature exceeds threshold, reduce look-ahead by 20%
        if (absCurvature > CurvatureReductionThreshold)
        {
            return distance * CurvatureReductionFactor;
        }

        return distance;
    }

    /// <summary>
    /// Apply vehicle type scaling factor
    /// Combines get 10% larger look-ahead, tractors use baseline
    /// </summary>
    private double ApplyVehicleTypeScaling(double distance, VehicleType vehicleType)
    {
        return vehicleType switch
        {
            VehicleType.Harvester => distance * HarvesterScalingFactor,
            VehicleType.Tractor => distance,
            VehicleType.FourWD => distance,
            _ => distance
        };
    }
}
