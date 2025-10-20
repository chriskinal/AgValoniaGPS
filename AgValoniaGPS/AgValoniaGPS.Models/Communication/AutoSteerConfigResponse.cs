namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents the configuration response from an AutoSteer module.
/// Contains module capabilities and steering angle limits.
/// </summary>
public class AutoSteerConfigResponse
{
    /// <summary>
    /// Gets or sets the firmware version of the AutoSteer module.
    /// </summary>
    public byte Version { get; set; }

    /// <summary>
    /// Gets or sets the raw capabilities bitmap from the module.
    /// </summary>
    public byte[] Capabilities { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the maximum steering angle in degrees.
    /// </summary>
    public double MaxSteerAngle { get; set; }

    /// <summary>
    /// Gets or sets the minimum steering angle in degrees.
    /// </summary>
    public double MinSteerAngle { get; set; }
}
