namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents the response from a module scan/discovery request.
/// Contains module type, version, and capabilities.
/// </summary>
public class ScanResponse
{
    /// <summary>
    /// Gets or sets the type of module that responded.
    /// </summary>
    public ModuleType ModuleType { get; set; }

    /// <summary>
    /// Gets or sets the firmware version of the module.
    /// </summary>
    public byte Version { get; set; }

    /// <summary>
    /// Gets or sets the raw capabilities bitmap from the module.
    /// </summary>
    public byte[] Capabilities { get; set; } = Array.Empty<byte>();
}
