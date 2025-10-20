namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents a firmware version with major, minor, and patch components.
/// </summary>
public class FirmwareVersion
{
    /// <summary>
    /// Gets or sets the major version number.
    /// </summary>
    public byte Major { get; set; }

    /// <summary>
    /// Gets or sets the minor version number.
    /// </summary>
    public byte Minor { get; set; }

    /// <summary>
    /// Gets or sets the patch version number.
    /// </summary>
    public byte Patch { get; set; }

    /// <summary>
    /// Returns the firmware version in "major.minor.patch" format.
    /// </summary>
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
