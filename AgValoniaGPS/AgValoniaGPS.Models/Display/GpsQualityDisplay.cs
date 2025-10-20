namespace AgValoniaGPS.Models.Display;

/// <summary>
/// Represents formatted GPS quality information for display purposes, including
/// human-readable text and associated color for visual indication of quality level.
/// </summary>
public class GpsQualityDisplay
{
    /// <summary>
    /// Gets or sets the formatted text describing the GPS fix quality and age.
    /// Example: "RTK fix: Age: 0.0", "DGPS: Age: 1.2", "No Fix: Age: 0.0"
    /// </summary>
    public string FormattedText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color name for visual indication of GPS quality level.
    /// Valid values: "PaleGreen" (RTK Fixed), "Orange" (RTK Float),
    /// "Yellow" (DGPS), "Red" (Autonomous or No Fix).
    /// </summary>
    public string ColorName { get; set; } = string.Empty;
}
