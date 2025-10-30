namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Types of field markers that can be placed
/// </summary>
public enum MarkerType
{
    /// <summary>
    /// General note or point of interest
    /// </summary>
    Note = 0,

    /// <summary>
    /// Obstacle or hazard marker
    /// </summary>
    Obstacle = 1,

    /// <summary>
    /// Navigation waypoint
    /// </summary>
    Waypoint = 2,

    /// <summary>
    /// Flag marker (generic)
    /// </summary>
    Flag = 3,

    /// <summary>
    /// Reference point
    /// </summary>
    Reference = 4,

    /// <summary>
    /// Warning or caution marker
    /// </summary>
    Warning = 5
}
