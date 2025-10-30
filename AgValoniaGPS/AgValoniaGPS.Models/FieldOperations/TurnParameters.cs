namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Configuration parameters for U-turn generation.
/// Encapsulates all settings needed to compute headland turns.
/// </summary>
public class TurnParameters
{
    /// <summary>
    /// Creates turn parameters with specified configuration.
    /// </summary>
    /// <param name="turnStyle">Style of turn to execute</param>
    /// <param name="turningRadius">Vehicle turning radius in meters</param>
    /// <param name="rowSkipMode">How to select the next track</param>
    /// <param name="rowSkipWidth">Width between tracks in meters (for skip calculations)</param>
    public TurnParameters(
        TurnStyle turnStyle,
        double turningRadius,
        RowSkipMode rowSkipMode,
        double rowSkipWidth)
    {
        TurnStyle = turnStyle;
        TurningRadius = turningRadius;
        RowSkipMode = rowSkipMode;
        RowSkipWidth = rowSkipWidth;
    }

    /// <summary>
    /// Style of turn to execute.
    /// </summary>
    public TurnStyle TurnStyle { get; set; }

    /// <summary>
    /// Vehicle turning radius in meters.
    /// Used for Dubins path generation and turn geometry.
    /// </summary>
    public double TurningRadius { get; set; }

    /// <summary>
    /// Mode for selecting the next track to work.
    /// </summary>
    public RowSkipMode RowSkipMode { get; set; }

    /// <summary>
    /// Width between adjacent tracks in meters.
    /// Used for calculating skip distances and finding next track.
    /// </summary>
    public double RowSkipWidth { get; set; }

    /// <summary>
    /// Smoothing factor for turn paths (0.0 to 1.0).
    /// Higher values create smoother but longer turns.
    /// Default: 0.5
    /// </summary>
    public double SmoothingFactor { get; set; } = 0.5;

    /// <summary>
    /// Minimum distance from field boundary in meters.
    /// Turn paths must maintain this clearance from boundaries.
    /// Default: 1.0 meter
    /// </summary>
    public double BoundaryMinDistance { get; set; } = 1.0;

    /// <summary>
    /// Waypoint spacing along turn path in meters.
    /// Smaller values create more waypoints for smoother following.
    /// Default: 0.1 meter
    /// </summary>
    public double WaypointSpacing { get; set; } = 0.1;

    /// <summary>
    /// Number of tracks to skip (for Alternative mode).
    /// Default: 1 (skip one track)
    /// </summary>
    public int TracksToSkip { get; set; } = 1;

    /// <summary>
    /// Whether to reverse direction when no forward tracks are available.
    /// Default: true
    /// </summary>
    public bool AllowDirectionReversal { get; set; } = true;

    /// <summary>
    /// Radius multiplier for Wide turn style.
    /// Wide turns use a larger turning radius for implements needing gentle turns.
    /// Applied as: effectiveRadius = TurningRadius * WideRadiusMultiplier
    /// Default: 1.5
    /// Range: 1.0 to 3.0 recommended
    /// </summary>
    public double WideRadiusMultiplier { get; set; } = 1.5;
}
