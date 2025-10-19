using System;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for U-turn started events
/// </summary>
public class UTurnStartedEventArgs : EventArgs
{
    /// <summary>
    /// Type of turn that was started
    /// </summary>
    public readonly UTurnType TurnType;

    /// <summary>
    /// Position where the turn started
    /// </summary>
    public readonly Position StartPosition;

    /// <summary>
    /// Waypoints defining the turn path
    /// </summary>
    public readonly Position[] TurnPath;

    /// <summary>
    /// When the turn started
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of UTurnStartedEventArgs
    /// </summary>
    /// <param name="turnType">Type of turn</param>
    /// <param name="startPosition">Start position</param>
    /// <param name="turnPath">Turn path waypoints</param>
    public UTurnStartedEventArgs(UTurnType turnType, Position startPosition, Position[] turnPath)
    {
        if (startPosition == null)
            throw new ArgumentNullException(nameof(startPosition));
        if (turnPath == null || turnPath.Length == 0)
            throw new ArgumentException("Turn path cannot be null or empty", nameof(turnPath));

        TurnType = turnType;
        StartPosition = startPosition;
        TurnPath = turnPath;
        Timestamp = DateTime.UtcNow;
    }
}
