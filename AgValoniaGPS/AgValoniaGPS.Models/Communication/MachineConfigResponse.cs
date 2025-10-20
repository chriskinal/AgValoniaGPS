namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents the configuration response from a Machine module.
/// Contains module capabilities for section control and implement management.
/// </summary>
public class MachineConfigResponse
{
    /// <summary>
    /// Gets or sets the firmware version of the Machine module.
    /// </summary>
    public byte Version { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of sections supported by the module.
    /// </summary>
    public ushort MaxSections { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of zones supported by the module.
    /// </summary>
    public ushort MaxZones { get; set; }
}
