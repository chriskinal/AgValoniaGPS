namespace AgValoniaGPS.Models;

/// <summary>
/// Represents Inertial Measurement Unit (IMU) data for sensor fusion
/// </summary>
public class ImuData
{
    /// <summary>
    /// Heading from IMU compass in degrees (0-360)
    /// </summary>
    public double HeadingDegrees { get; set; }

    /// <summary>
    /// Roll angle in degrees (positive = right tilt)
    /// </summary>
    public double RollDegrees { get; set; }

    /// <summary>
    /// Pitch angle in degrees (positive = nose up)
    /// </summary>
    public double PitchDegrees { get; set; }

    /// <summary>
    /// Yaw rate in degrees per second
    /// </summary>
    public double YawRate { get; set; }

    /// <summary>
    /// Whether IMU data is currently valid
    /// </summary>
    public bool IsValid { get; set; }
}
