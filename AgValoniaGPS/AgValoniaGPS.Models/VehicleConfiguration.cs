using System;

namespace AgValoniaGPS.Models;

/// <summary>
/// Vehicle configuration and steering parameters
/// Ported from AOG_Dev CVehicle.cs
/// </summary>
public class VehicleConfiguration
{
    // Vehicle physical dimensions
    public double AntennaHeight { get; set; } = 3.0; // meters
    public double AntennaPivot { get; set; } = 0.0; // meters from pivot to antenna
    public double AntennaOffset { get; set; } = 0.0; // lateral offset
    public double Wheelbase { get; set; } = 2.5; // meters
    public double TrackWidth { get; set; } = 1.8; // meters

    // Steer angle heading compensation parameters (Section 6E)
    // Distance from rear axle to antenna for low-speed steering compensation
    public double AntennaPivotDistance { get; set; } = 0.0; // meters

    // Compensation factors for forward/reverse motion (default 1.0)
    // Higher values increase compensation effect
    public double ForwardCompensationFactor { get; set; } = 1.0;
    public double ReverseCompensationFactor { get; set; } = 1.0;

    // Vehicle type (0=Tractor, 1=Harvester, 2=4WD)
    public VehicleType Type { get; set; } = VehicleType.Tractor;

    // Steering limits
    public double MaxSteerAngle { get; set; } = 35.0; // degrees
    public double MaxAngularVelocity { get; set; } = 35.0; // degrees/second

    // Guidance look-ahead parameters
    public double GoalPointLookAheadHold { get; set; } = 4.0; // meters when on line
    public double GoalPointLookAheadMult { get; set; } = 1.4; // speed multiplier
    public double GoalPointAcquireFactor { get; set; } = 1.5; // factor when acquiring line
    public double MinLookAheadDistance { get; set; } = 2.0; // meters

    // Stanley steering algorithm parameters
    public double StanleyDistanceErrorGain { get; set; } = 0.8;
    public double StanleyHeadingErrorGain { get; set; } = 1.0;
    public double StanleyIntegralGainAB { get; set; } = 0.0;
    public double StanleyIntegralDistanceAwayTriggerAB { get; set; } = 0.3; // meters

    // Pure Pursuit algorithm parameters
    public double PurePursuitIntegralGain { get; set; } = 0.0;

    // Heading dead zone
    public double DeadZoneHeading { get; set; } = 0.5; // degrees (* 0.01 in original)
    public int DeadZoneDelay { get; set; } = 10; // cycles

    // U-turn compensation
    public double UTurnCompensation { get; set; } = 1.0;

    // Hydraulic lift look-ahead distances
    public double HydLiftLookAheadDistanceLeft { get; set; } = 1.0; // meters
    public double HydLiftLookAheadDistanceRight { get; set; } = 1.0; // meters
}

/// <summary>
/// Vehicle types supported
/// </summary>
public enum VehicleType
{
    Tractor = 0,
    Harvester = 1,
    FourWD = 2
}

/// <summary>
/// Steering algorithm selection
/// </summary>
public enum SteeringAlgorithm
{
    PurePursuit,
    Stanley
}
