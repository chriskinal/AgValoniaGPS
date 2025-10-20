namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents inertial measurement data from an IMU module.
/// Contains roll, pitch, heading, and calibration status for vehicle orientation.
/// </summary>
public class ImuData
{
    /// <summary>
    /// Gets or sets the roll angle in degrees.
    /// Positive = right side down, Negative = left side down.
    /// </summary>
    public double Roll { get; set; }

    /// <summary>
    /// Gets or sets the pitch angle in degrees.
    /// Positive = front up, Negative = front down.
    /// </summary>
    public double Pitch { get; set; }

    /// <summary>
    /// Gets or sets the heading angle in degrees (0-360).
    /// 0 = North, 90 = East, 180 = South, 270 = West.
    /// </summary>
    public double Heading { get; set; }

    /// <summary>
    /// Gets or sets the yaw rate in degrees per second.
    /// Rate of change of heading angle.
    /// </summary>
    public double YawRate { get; set; }

    /// <summary>
    /// Gets or sets whether the IMU is calibrated.
    /// When false, data may be unreliable.
    /// </summary>
    public bool IsCalibrated { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this data was received.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
