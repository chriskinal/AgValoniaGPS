namespace AgValoniaGPS.Models.Display;

/// <summary>
/// Represents data for rotating information display screens, cycling between
/// application statistics, field information, and guidance line details.
/// </summary>
public class RotatingDisplayData
{
    /// <summary>
    /// Gets or sets the current screen number being displayed.
    /// Valid values: 1 (Application Statistics), 2 (Field Name), 3 (Guidance Line Info).
    /// </summary>
    public int CurrentScreen { get; set; }

    /// <summary>
    /// Gets or sets the application statistics for Screen 1 display.
    /// Contains area coverage, application rates, and work efficiency metrics.
    /// </summary>
    public ApplicationStatistics? AppStats { get; set; }

    /// <summary>
    /// Gets or sets the field name for Screen 2 display.
    /// Example: "Test Field", "North 40", "Field: Sample"
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the guidance line information for Screen 3 display.
    /// Example: "Line: AB 45°", "Line: Curve 90°", "Line: Contour 180°"
    /// </summary>
    public string GuidanceLineInfo { get; set; } = string.Empty;
}
