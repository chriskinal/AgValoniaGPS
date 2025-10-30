namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents the six types of Dubins paths.
/// Each path consists of three segments: two circular arcs and one straight line or arc.
/// R = Right turn, L = Left turn, S = Straight line
/// </summary>
public enum DubinsPathType
{
    /// <summary>
    /// Right-Straight-Right: Right arc, straight line, right arc
    /// </summary>
    RSR,

    /// <summary>
    /// Left-Straight-Left: Left arc, straight line, left arc
    /// </summary>
    LSL,

    /// <summary>
    /// Right-Straight-Left: Right arc, straight line, left arc
    /// </summary>
    RSL,

    /// <summary>
    /// Left-Straight-Right: Left arc, straight line, right arc
    /// </summary>
    LSR,

    /// <summary>
    /// Right-Left-Right: Right arc, left arc, right arc
    /// </summary>
    RLR,

    /// <summary>
    /// Left-Right-Left: Left arc, right arc, left arc
    /// </summary>
    LRL
}
