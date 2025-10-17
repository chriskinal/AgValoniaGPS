using System;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services;

/// <summary>
/// Service for calculating vehicle heading from various sources
/// Extracted from AgOpenGPS FormGPS/Position.designer.cs (lines 190-810)
/// </summary>
public class HeadingCalculatorService : IHeadingCalculatorService
{
    public event EventHandler<HeadingUpdate>? HeadingChanged;

    private const double TWO_PI = Math.PI * 2.0;
    private const double PI_BY_2 = Math.PI / 2.0;

    private double _imuGpsOffset = 0.0;
    private double _currentHeading = 0.0;

    public double ImuGpsOffset => _imuGpsOffset;
    public double CurrentHeading => _currentHeading;

    /// <summary>
    /// Calculate heading from position changes (fix-to-fix dead reckoning)
    /// </summary>
    public double CalculateFixToFixHeading(FixToFixHeadingData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        double deltaEasting = data.CurrentEasting - data.PreviousEasting;
        double deltaNorthing = data.CurrentNorthing - data.PreviousNorthing;

        // Check minimum distance threshold
        double distance = Math.Sqrt(deltaEasting * deltaEasting + deltaNorthing * deltaNorthing);
        if (distance < data.MinimumDistance)
        {
            // Insufficient movement for reliable heading
            return _currentHeading;
        }

        // Calculate heading using atan2 (east, north)
        double heading = Math.Atan2(deltaEasting, deltaNorthing);

        // Normalize to 0-2π
        heading = NormalizeAngle(heading);

        _currentHeading = heading;
        RaiseHeadingChanged(heading, HeadingSource.FixToFix);

        return heading;
    }

    /// <summary>
    /// Process VTG (Velocity Track Good) heading from GPS
    /// </summary>
    public double ProcessVtgHeading(double vtgHeadingDegrees)
    {
        // Convert degrees to radians
        double heading = DegreesToRadians(vtgHeadingDegrees);

        // Normalize to 0-2π
        heading = NormalizeAngle(heading);

        _currentHeading = heading;
        RaiseHeadingChanged(heading, HeadingSource.VTG);

        return heading;
    }

    /// <summary>
    /// Process dual antenna heading
    /// </summary>
    public double ProcessDualAntennaHeading(double dualHeadingDegrees)
    {
        // Convert degrees to radians
        double heading = DegreesToRadians(dualHeadingDegrees);

        // Normalize to 0-2π
        heading = NormalizeAngle(heading);

        _currentHeading = heading;
        RaiseHeadingChanged(heading, HeadingSource.DualAntenna);

        return heading;
    }

    /// <summary>
    /// Fuse IMU heading with GPS heading using weighted averaging
    /// Based on original AgOpenGPS IMU fusion algorithm
    /// </summary>
    public double FuseImuHeading(double gpsHeading, double imuHeadingDegrees, double fusionWeight)
    {
        // Validate inputs
        if (fusionWeight < 0.0 || fusionWeight > 1.0)
            throw new ArgumentOutOfRangeException(nameof(fusionWeight), "Fusion weight must be between 0 and 1");

        // Convert IMU heading from degrees to radians
        double imuHeading = DegreesToRadians(imuHeadingDegrees);

        // Normalize both headings
        gpsHeading = NormalizeAngle(gpsHeading);
        imuHeading = NormalizeAngle(imuHeading);

        // Calculate the difference between IMU (with current offset) and GPS heading
        double gyroDelta = (imuHeading + _imuGpsOffset) - gpsHeading;

        // Normalize delta to 0-2π first
        if (gyroDelta < 0) gyroDelta += TWO_PI;
        else if (gyroDelta >= TWO_PI) gyroDelta -= TWO_PI;

        // Convert to signed delta (-π to π) based on circular data problem
        // This handles the 0/360 degree wrap-around correctly
        if (gyroDelta >= -PI_BY_2 && gyroDelta <= PI_BY_2)
        {
            gyroDelta *= -1.0;
        }
        else
        {
            if (gyroDelta > PI_BY_2)
            {
                gyroDelta = TWO_PI - gyroDelta;
            }
            else
            {
                gyroDelta = (TWO_PI + gyroDelta) * -1.0;
            }
        }

        // Clamp delta to ±2π
        if (gyroDelta > TWO_PI) gyroDelta -= TWO_PI;
        else if (gyroDelta < -TWO_PI) gyroDelta += TWO_PI;

        // Update the IMU-GPS offset using fusion weight
        // Higher fusion weight = more GPS influence = faster correction
        _imuGpsOffset += gyroDelta * fusionWeight;

        // Normalize offset to 0-2π
        _imuGpsOffset = NormalizeAngle(_imuGpsOffset);

        // Calculate the corrected heading based on IMU and offset
        double fusedHeading = imuHeading + _imuGpsOffset;
        fusedHeading = NormalizeAngle(fusedHeading);

        _currentHeading = fusedHeading;
        RaiseHeadingChanged(fusedHeading, HeadingSource.Fused);

        return fusedHeading;
    }

    /// <summary>
    /// Calculate roll correction distance for antenna position
    /// Based on AgOpenGPS roll compensation algorithm
    /// </summary>
    public double CalculateRollCorrectionDistance(double heading, double rollDegrees, double antennaHeight)
    {
        if (Math.Abs(rollDegrees) < 0.01 || Math.Abs(antennaHeight) < 0.01)
        {
            // No significant roll or antenna height
            return 0.0;
        }

        // Convert roll to radians
        double rollRadians = DegreesToRadians(rollDegrees);

        // Calculate correction distance
        // Roll to the right is positive (AgOpenGPS convention as of April 30, 2019)
        // Use Sin for the correction (based on original code)
        double correctionDistance = Math.Sin(rollRadians) * -antennaHeight;

        return correctionDistance;
    }

    /// <summary>
    /// Determine optimal heading source based on conditions
    /// </summary>
    public HeadingSource DetermineOptimalSource(double speed, bool hasDualAntenna, bool hasImu)
    {
        // Dual antenna is most accurate when available
        if (hasDualAntenna)
        {
            return HeadingSource.DualAntenna;
        }

        // IMU fusion provides best results at all speeds when IMU available
        if (hasImu)
        {
            return HeadingSource.Fused;
        }

        // At higher speeds (>1.5 m/s), fix-to-fix or VTG are reliable
        // At low speeds, less reliable but still best option
        if (speed > 1.0)
        {
            return HeadingSource.FixToFix; // or VTG depending on GPS capabilities
        }

        // Very slow or stopped - hold last heading
        return HeadingSource.FixToFix;
    }

    /// <summary>
    /// Normalize angle to 0-2π range
    /// </summary>
    public double NormalizeAngle(double angle)
    {
        // Reduce to 0-2π range
        while (angle < 0)
            angle += TWO_PI;
        while (angle >= TWO_PI)
            angle -= TWO_PI;

        return angle;
    }

    /// <summary>
    /// Calculate angular delta considering circular wrapping
    /// Returns shortest angular distance between two angles
    /// </summary>
    public double CalculateAngularDelta(double angle1, double angle2)
    {
        // Normalize both angles
        angle1 = NormalizeAngle(angle1);
        angle2 = NormalizeAngle(angle2);

        // Calculate raw delta
        double delta = angle2 - angle1;

        // Find shortest path around circle
        if (delta > Math.PI)
        {
            delta -= TWO_PI;
        }
        else if (delta < -Math.PI)
        {
            delta += TWO_PI;
        }

        return delta;
    }

    /// <summary>
    /// Convert degrees to radians
    /// </summary>
    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Raise the HeadingChanged event
    /// </summary>
    private void RaiseHeadingChanged(double heading, HeadingSource source)
    {
        HeadingChanged?.Invoke(this, new HeadingUpdate
        {
            Heading = heading,
            Source = source,
            Timestamp = DateTime.UtcNow
        });
    }
}
