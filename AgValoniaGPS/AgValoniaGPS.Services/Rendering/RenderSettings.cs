namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Configuration settings for controlling which render layers are visible.
/// Used by RenderPassManager to selectively enable/disable rendering passes.
/// </summary>
public class RenderSettings
{
    /// <summary>
    /// Show field boundaries (outer and inner boundaries)
    /// </summary>
    public bool ShowBoundaries { get; set; } = true;

    /// <summary>
    /// Show guidance lines (AB lines, curves, contours)
    /// </summary>
    public bool ShowGuidanceLines { get; set; } = true;

    /// <summary>
    /// Show coverage map triangles
    /// </summary>
    public bool ShowCoverageMap { get; set; } = true;

    /// <summary>
    /// Show section state overlays
    /// </summary>
    public bool ShowSections { get; set; } = true;

    /// <summary>
    /// Show vehicle icon and implement
    /// </summary>
    public bool ShowVehicle { get; set; } = true;

    /// <summary>
    /// Show tram lines
    /// </summary>
    public bool ShowTramLines { get; set; } = true;

    /// <summary>
    /// Creates a default RenderSettings instance with all layers enabled.
    /// </summary>
    public static RenderSettings Default => new RenderSettings();
}
