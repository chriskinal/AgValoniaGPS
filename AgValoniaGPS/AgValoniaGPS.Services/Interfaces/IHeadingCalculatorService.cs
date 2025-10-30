using System;

namespace AgValoniaGPS.Services.Interfaces;

/// <summary>
/// Service for calculating vehicle heading from various sources (GPS, IMU, Dual antenna)
/// </summary>
public interface IHeadingCalculatorService
{
    /// <summary>
    /// Event fired when heading is updated
    /// </summary>
    event EventHandler<HeadingUpdate>? HeadingChanged;

    /// <summary>
    /// Calculate heading from position changes (fix-to-fix dead reckoning)
    /// </summary>
    /// <param name="data">Data containing current and historical positions</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    double CalculateFixToFixHeading(FixToFixHeadingData data);

    /// <summary>
    /// Process VTG (Velocity Track Good) heading from GPS
    /// </summary>
    /// <param name="vtgHeadingDegrees">True heading from VTG NMEA sentence</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    double ProcessVtgHeading(double vtgHeadingDegrees);

    /// <summary>
    /// Process dual antenna heading
    /// </summary>
    /// <param name="dualHeadingDegrees">True heading from dual antenna GPS</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    double ProcessDualAntennaHeading(double dualHeadingDegrees);

    /// <summary>
    /// Fuse IMU heading with GPS heading using weighted averaging
    /// </summary>
    /// <param name="gpsHeading">GPS-derived heading in radians</param>
    /// <param name="imuHeadingDegrees">IMU heading in degrees</param>
    /// <param name="fusionWeight">Fusion weight (0-1, higher means more GPS influence)</param>
    /// <returns>Fused heading in radians (0 to 2π)</returns>
    double FuseImuHeading(double gpsHeading, double imuHeadingDegrees, double fusionWeight);

    /// <summary>
    /// Compensate heading for vehicle roll (antenna height effect)
    /// </summary>
    /// <param name="heading">Current heading in radians</param>
    /// <param name="rollDegrees">Roll angle in degrees (positive = right)</param>
    /// <param name="antennaHeight">Height of antenna above ground in meters</param>
    /// <returns>Roll-corrected distance offset in meters</returns>
    double CalculateRollCorrectionDistance(double heading, double rollDegrees, double antennaHeight);

    /// <summary>
    /// Determine optimal heading source based on conditions
    /// </summary>
    /// <param name="speed">Current speed in m/s</param>
    /// <param name="hasDualAntenna">Whether dual antenna is available</param>
    /// <param name="hasImu">Whether IMU is available</param>
    /// <returns>Recommended heading source</returns>
    HeadingSource DetermineOptimalSource(double speed, bool hasDualAntenna, bool hasImu);

    /// <summary>
    /// Normalize angle to 0-2π range
    /// </summary>
    /// <param name="angle">Angle in radians</param>
    /// <returns>Normalized angle (0 to 2π)</returns>
    double NormalizeAngle(double angle);

    /// <summary>
    /// Calculate angular delta considering circular wrapping
    /// </summary>
    /// <param name="angle1">First angle in radians</param>
    /// <param name="angle2">Second angle in radians</param>
    /// <returns>Delta angle in radians (-π to π)</returns>
    double CalculateAngularDelta(double angle1, double angle2);

    /// <summary>
    /// Apply steer angle compensation to heading for low-speed accuracy (Section 6E)
    /// Compensates for antenna swing during steering at low speeds
    /// Based on AgOpenGPS Position.designer.cs lines 419-424
    /// </summary>
    /// <param name="heading">Current heading in radians</param>
    /// <param name="steerAngleDegrees">Actual steer angle from wheel angle sensor (degrees)</param>
    /// <param name="speed">Current vehicle speed (m/s)</param>
    /// <param name="isReversing">Whether vehicle is moving in reverse</param>
    /// <param name="antennaPivotDistance">Distance from rear axle to antenna (meters)</param>
    /// <param name="forwardCompensation">Forward compensation factor (default 1.0)</param>
    /// <param name="reverseCompensation">Reverse compensation factor (default 1.0)</param>
    /// <returns>Compensated heading in radians (0 to 2π)</returns>
    double ApplySteerAngleCompensation(double heading, double steerAngleDegrees, double speed,
        bool isReversing, double antennaPivotDistance, double forwardCompensation = 1.0,
        double reverseCompensation = 1.0);

    /// <summary>
    /// Get current IMU-GPS offset for fusion
    /// </summary>
    double ImuGpsOffset { get; }

    /// <summary>
    /// Get current fused heading
    /// </summary>
    double CurrentHeading { get; }
}

/// <summary>
/// Heading sources available for calculation
/// </summary>
public enum HeadingSource
{
    /// <summary>
    /// Calculate heading from position changes (dead reckoning)
    /// </summary>
    FixToFix,

    /// <summary>
    /// Use GPS VTG (Velocity Track Good) sentence
    /// </summary>
    VTG,

    /// <summary>
    /// Use dual antenna GPS heading
    /// </summary>
    DualAntenna,

    /// <summary>
    /// Use IMU compass heading
    /// </summary>
    IMU,

    /// <summary>
    /// Fuse IMU and GPS headings
    /// </summary>
    Fused
}

/// <summary>
/// Event data for heading updates
/// </summary>
public class HeadingUpdate : EventArgs
{
    /// <summary>
    /// Heading in radians (0 to 2π)
    /// </summary>
    public double Heading { get; set; }

    /// <summary>
    /// Heading source used
    /// </summary>
    public HeadingSource Source { get; set; }

    /// <summary>
    /// Timestamp of update
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Data required for fix-to-fix heading calculation
/// </summary>
public class FixToFixHeadingData
{
    /// <summary>
    /// Current position easting (meters)
    /// </summary>
    public double CurrentEasting { get; set; }

    /// <summary>
    /// Current position northing (meters)
    /// </summary>
    public double CurrentNorthing { get; set; }

    /// <summary>
    /// Previous position easting (meters)
    /// </summary>
    public double PreviousEasting { get; set; }

    /// <summary>
    /// Previous position northing (meters)
    /// </summary>
    public double PreviousNorthing { get; set; }

    /// <summary>
    /// Minimum distance required for heading calculation (meters)
    /// </summary>
    public double MinimumDistance { get; set; } = 0.5;
}
