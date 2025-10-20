using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for module connection events.
/// Raised when a hardware module successfully connects and is ready for communication.
/// </summary>
public class ModuleConnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of module that connected.
    /// </summary>
    public readonly ModuleType ModuleType;

    /// <summary>
    /// Gets the transport type used for the connection.
    /// </summary>
    public readonly TransportType Transport;

    /// <summary>
    /// Gets the firmware version of the connected module.
    /// </summary>
    public readonly FirmwareVersion Version;

    /// <summary>
    /// Gets the timestamp when the module connected.
    /// </summary>
    public readonly DateTime ConnectedAt;

    /// <summary>
    /// Creates a new instance of ModuleConnectedEventArgs.
    /// </summary>
    /// <param name="moduleType">Type of module that connected</param>
    /// <param name="transport">Transport type used</param>
    /// <param name="version">Firmware version</param>
    public ModuleConnectedEventArgs(ModuleType moduleType, TransportType transport, FirmwareVersion version)
    {
        ModuleType = moduleType;
        Transport = transport;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        ConnectedAt = DateTime.UtcNow;
    }
}
