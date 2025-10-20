namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents a hello packet response from a hardware module.
/// Used for connection monitoring and keepalive detection.
/// </summary>
public class HelloResponse
{
    /// <summary>
    /// Gets or sets the type of module that sent the hello packet.
    /// </summary>
    public ModuleType ModuleType { get; set; }

    /// <summary>
    /// Gets or sets the firmware version of the module (if provided).
    /// </summary>
    public byte Version { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the hello packet was received.
    /// Used for timeout detection (2-second threshold).
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
