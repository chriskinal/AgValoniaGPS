namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Defines the types of geometry that can be rendered.
/// Used for tracking dirty flags and invalidating specific geometry.
/// </summary>
public enum GeometryType
{
    /// <summary>
    /// Vehicle body and implement geometry (Wave 1)
    /// </summary>
    Vehicle,

    /// <summary>
    /// Field boundary lines and polygons (Wave 5)
    /// </summary>
    Boundaries,

    /// <summary>
    /// Guidance lines: AB lines, curves, contours (Wave 2)
    /// </summary>
    Guidance,

    /// <summary>
    /// Coverage map triangles (Wave 6B)
    /// </summary>
    Coverage,

    /// <summary>
    /// Section state overlay rectangles (Wave 4)
    /// </summary>
    Sections,

    /// <summary>
    /// Tram line geometry (Wave 5)
    /// </summary>
    TramLines
}
