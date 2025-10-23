namespace AgValoniaGPS.Models;

/// <summary>
/// Enumeration of boundary editing tool modes
/// </summary>
public enum BoundaryToolMode
{
    /// <summary>
    /// Draw new boundary points
    /// </summary>
    Draw,

    /// <summary>
    /// Erase boundary points
    /// </summary>
    Erase,

    /// <summary>
    /// Simplify boundary (reduce point count)
    /// </summary>
    Simplify,

    /// <summary>
    /// Move boundary points
    /// </summary>
    Move
}
