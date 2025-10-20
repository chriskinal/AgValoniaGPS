namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Section control configuration for implements with multiple controllable sections.
/// </summary>
public class SectionControlSettings
{
    /// <summary>
    /// Gets or sets the number of sections.
    /// Valid range: 1-32.
    /// </summary>
    public int NumberSections { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether headland section control is enabled.
    /// </summary>
    public bool HeadlandSecControl { get; set; } = false;

    /// <summary>
    /// Gets or sets whether fast section response is enabled.
    /// </summary>
    public bool FastSections { get; set; } = true;

    /// <summary>
    /// Gets or sets whether sections turn off when out of bounds.
    /// </summary>
    public bool SectionOffOutBounds { get; set; } = true;

    /// <summary>
    /// Gets or sets whether sections are independent (not zones).
    /// </summary>
    public bool SectionsNotZones { get; set; } = true;

    /// <summary>
    /// Gets or sets the low speed cutoff in meters per second.
    /// Valid range: 0.1-5.0 m/s.
    /// </summary>
    public double LowSpeedCutoff { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the section positions across tool width.
    /// Array length must equal NumberSections.
    /// Positions in meters from tool center, spanning -ToolWidth/2 to +ToolWidth/2.
    /// </summary>
    public double[] SectionPositions { get; set; } = new[] { -0.914, 0.0, 0.914 };
}
