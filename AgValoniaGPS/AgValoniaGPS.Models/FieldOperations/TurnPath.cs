using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a complete U-turn path from the end of one track to the start of the next.
/// Contains waypoints, metadata, and boundary validation results.
/// </summary>
public class TurnPath
{
    /// <summary>
    /// Creates a new turn path.
    /// </summary>
    /// <param name="turnStyle">Style of turn used</param>
    /// <param name="entryPoint">Entry position (end of current track)</param>
    /// <param name="exitPoint">Exit position (start of next track)</param>
    public TurnPath(TurnStyle turnStyle, Position2D entryPoint, Position2D exitPoint)
    {
        TurnStyle = turnStyle;
        EntryPoint = entryPoint;
        ExitPoint = exitPoint;
        Waypoints = new List<Position2D>();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Style of turn used to generate this path.
    /// </summary>
    public TurnStyle TurnStyle { get; }

    /// <summary>
    /// Entry point into the turn (end of current track).
    /// </summary>
    public Position2D EntryPoint { get; }

    /// <summary>
    /// Entry heading in radians.
    /// </summary>
    public double EntryHeading { get; set; }

    /// <summary>
    /// Exit point from the turn (start of next track).
    /// </summary>
    public Position2D ExitPoint { get; }

    /// <summary>
    /// Exit heading in radians.
    /// </summary>
    public double ExitHeading { get; set; }

    /// <summary>
    /// Waypoints along the turn path from entry to exit.
    /// </summary>
    public List<Position2D> Waypoints { get; set; }

    /// <summary>
    /// Total length of the turn path in meters.
    /// </summary>
    public double TotalLength { get; set; }

    /// <summary>
    /// Time taken to compute this turn path.
    /// </summary>
    public TimeSpan ComputationTime { get; set; }

    /// <summary>
    /// Timestamp when this turn path was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Boundary validation result for this turn path.
    /// Null if boundary checking was not performed.
    /// </summary>
    public TurnBoundaryCheck? BoundaryCheck { get; set; }

    /// <summary>
    /// Underlying Dubins path used for Omega turns.
    /// Null for non-Dubins turn styles (K, T, Y).
    /// </summary>
    public DubinsPath? DubinsPath { get; set; }

    /// <summary>
    /// Whether this turn requires reversing (K-turn).
    /// </summary>
    public bool RequiresReverse { get; set; }

    /// <summary>
    /// Index of the next track this turn connects to.
    /// Null if track selection was not performed.
    /// </summary>
    public int? NextTrackIndex { get; set; }

    /// <summary>
    /// Whether the next track is in the opposite direction (field direction reversal).
    /// </summary>
    public bool IsDirectionReversal { get; set; }

    /// <summary>
    /// Number of tracks skipped to reach the next track.
    /// 0 for adjacent tracks, >0 for skip modes.
    /// </summary>
    public int TracksSkipped { get; set; }
}
