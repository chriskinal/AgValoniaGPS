namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Implement/tool configuration including width, position flags, and operational parameters.
/// </summary>
public class ToolSettings
{
    /// <summary>
    /// Gets or sets the tool width in meters.
    /// Valid range: 0.1-50.0 meters.
    /// </summary>
    public double ToolWidth { get; set; } = 1.828;

    /// <summary>
    /// Gets or sets whether the tool is front-mounted.
    /// </summary>
    public bool ToolFront { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the tool is rear-fixed.
    /// </summary>
    public bool ToolRearFixed { get; set; } = true;

    /// <summary>
    /// Gets or sets whether tool is trailed behind tractor (TBT).
    /// </summary>
    public bool ToolTBT { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the tool is trailing.
    /// </summary>
    public bool ToolTrailing { get; set; } = false;

    /// <summary>
    /// Gets or sets the distance from pivot to tool in meters.
    /// </summary>
    public double ToolToPivotLength { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the look-ahead distance when tool is on in meters.
    /// </summary>
    public double ToolLookAheadOn { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the look-ahead distance when tool is off in meters.
    /// </summary>
    public double ToolLookAheadOff { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the tool off delay in seconds.
    /// </summary>
    public double ToolOffDelay { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the tool lateral offset in meters.
    /// </summary>
    public double ToolOffset { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the tool overlap distance in meters.
    /// </summary>
    public double ToolOverlap { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the trailing hitch length in meters.
    /// </summary>
    public double TrailingHitchLength { get; set; } = -2.5;

    /// <summary>
    /// Gets or sets the tank hitch length in meters.
    /// </summary>
    public double TankHitchLength { get; set; } = 3.0;

    /// <summary>
    /// Gets or sets the hydraulic lift look-ahead distance in meters.
    /// </summary>
    public double HydLiftLookAhead { get; set; } = 2.0;
}
