namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Defines the style of U-turn to execute at field headlands.
/// Different styles optimize for different field conditions and vehicle types.
/// </summary>
public enum TurnStyle
{
    /// <summary>
    /// Omega turn: Smooth semicircular turn using Dubins path (RSR/LSL).
    /// Best for most conditions, provides smoothest path.
    /// </summary>
    Omega = 0,

    /// <summary>
    /// K-turn: Three-point turn for tight spaces.
    /// Forward, reverse, forward pattern.
    /// </summary>
    K = 1,

    /// <summary>
    /// Wide turn: Larger radius turn for implements that need more space.
    /// Uses increased turning radius.
    /// </summary>
    Wide = 2,

    /// <summary>
    /// T-turn: Simple 90-degree turn pattern.
    /// Straight forward, turn 90°, straight, turn 90°.
    /// </summary>
    T = 3,

    /// <summary>
    /// Y-turn: Diagonal entry/exit turn pattern.
    /// Forward diagonal, reverse, forward to next track.
    /// </summary>
    Y = 4
}
