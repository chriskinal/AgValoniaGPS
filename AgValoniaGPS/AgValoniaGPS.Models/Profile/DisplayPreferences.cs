using System.Collections.Generic;

namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents display-specific preferences for UI customization.
/// Controls which information is shown and how the display behaves.
/// </summary>
public class DisplayPreferences
{
    /// <summary>
    /// Gets or sets whether to show the satellite count in the display.
    /// </summary>
    public bool ShowSatelliteCount { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the speed gauge in the display.
    /// </summary>
    public bool ShowSpeedGauge { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the rotating display feature is enabled.
    /// Rotating display cycles through different information panels.
    /// </summary>
    public bool RotatingDisplayEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the interval in seconds between rotating display changes.
    /// Only used when RotatingDisplayEnabled is true.
    /// </summary>
    public int RotatingDisplayInterval { get; set; } = 5;

    /// <summary>
    /// Gets or sets the saved positions of floating buttons.
    /// Stores position offsets and visibility state for each button.
    /// </summary>
    public List<ButtonPosition> ButtonPositions { get; set; } = new();

    /// <summary>
    /// Gets or sets the saved window width.
    /// </summary>
    public double WindowWidth { get; set; } = 1366;

    /// <summary>
    /// Gets or sets the saved window height.
    /// </summary>
    public double WindowHeight { get; set; } = 768;

    /// <summary>
    /// Gets or sets whether the window was maximized.
    /// </summary>
    public bool WindowMaximized { get; set; } = false;
}
