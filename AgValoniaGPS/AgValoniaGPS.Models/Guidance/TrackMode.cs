namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Defines the type/mode of a guidance track.
/// Used for identifying track behavior and rendering.
/// </summary>
public enum TrackMode
{
    /// <summary>
    /// Standard AB line track (straight line guidance)
    /// </summary>
    ABLine = 0,

    /// <summary>
    /// Curve line track (curved path guidance)
    /// </summary>
    CurveLine = 1,

    /// <summary>
    /// Contour track (elevation-following guidance)
    /// </summary>
    Contour = 2,

    /// <summary>
    /// Outer boundary track (field perimeter)
    /// </summary>
    BoundaryOuter = 3,

    /// <summary>
    /// Inner boundary track (internal obstacles)
    /// </summary>
    BoundaryInner = 4,

    /// <summary>
    /// Pivot track (center-pivot irrigation pattern)
    /// </summary>
    Pivot = 5,

    /// <summary>
    /// Recorded path (manually driven/recorded track)
    /// </summary>
    RecordedPath = 6,

    /// <summary>
    /// Headline track (moveable guidance path)
    /// </summary>
    Headline = 7
}
