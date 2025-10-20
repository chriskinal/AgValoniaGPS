namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents the capabilities of a hardware module.
/// Provides typed access to capability bitmap flags.
/// </summary>
public class ModuleCapabilities
{
    /// <summary>
    /// Gets or sets the raw capabilities bitmap from the module.
    /// </summary>
    public byte[] RawCapabilities { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets whether the module supports V2 protocol features.
    /// </summary>
    public bool SupportsV2Protocol => RawCapabilities.Length > 0 && (RawCapabilities[0] & 0x01) != 0;

    /// <summary>
    /// Gets whether the module supports dual antenna GPS.
    /// </summary>
    public bool SupportsDualAntenna => RawCapabilities.Length > 0 && (RawCapabilities[0] & 0x02) != 0;

    /// <summary>
    /// Gets whether the module supports roll compensation.
    /// </summary>
    public bool SupportsRollCompensation => RawCapabilities.Length > 0 && (RawCapabilities[0] & 0x04) != 0;
}
