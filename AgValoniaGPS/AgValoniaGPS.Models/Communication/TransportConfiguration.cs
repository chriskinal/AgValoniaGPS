namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents the configuration for a module's transport layer.
/// Allows per-module transport selection with custom parameters.
/// </summary>
public class TransportConfiguration
{
    /// <summary>
    /// Gets or sets the module this configuration applies to.
    /// </summary>
    public ModuleType Module { get; set; }

    /// <summary>
    /// Gets or sets the transport type for this module.
    /// </summary>
    public TransportType Type { get; set; }

    /// <summary>
    /// Gets or sets the transport-specific parameters.
    /// Examples:
    /// - Bluetooth: "DeviceAddress", "Mode" (SPP or BLE)
    /// - CAN: "AdapterPath", "Baudrate"
    /// - Radio: "RadioType", "Frequency", "TransmitPower"
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}
