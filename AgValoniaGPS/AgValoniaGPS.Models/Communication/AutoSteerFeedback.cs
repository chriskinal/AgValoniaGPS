namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents feedback data from an AutoSteer module.
/// Contains actual wheel angle and switch states for closed-loop control.
/// </summary>
public class AutoSteerFeedback
{
    /// <summary>
    /// Gets or sets the actual wheel angle in degrees.
    /// Used for closed-loop comparison with desired angle.
    /// </summary>
    public double ActualWheelAngle { get; set; }

    /// <summary>
    /// Gets or sets the switch states bitmap.
    /// Each bit represents a switch input on the AutoSteer module.
    /// </summary>
    public byte[] SwitchStates { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the status flags from the module.
    /// </summary>
    public byte StatusFlags { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this feedback was received.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
