using System;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for U-turn completed events
/// </summary>
public class UTurnCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Type of turn that was completed
    /// </summary>
    public readonly UTurnType TurnType;

    /// <summary>
    /// Position where the turn ended
    /// </summary>
    public readonly Position EndPosition;

    /// <summary>
    /// Duration of the turn in seconds
    /// </summary>
    public readonly double TurnDuration;

    /// <summary>
    /// When the turn completed
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of UTurnCompletedEventArgs
    /// </summary>
    /// <param name="turnType">Type of turn</param>
    /// <param name="endPosition">End position</param>
    /// <param name="turnDuration">Turn duration in seconds</param>
    public UTurnCompletedEventArgs(UTurnType turnType, Position endPosition, double turnDuration)
    {
        if (endPosition == null)
            throw new ArgumentNullException(nameof(endPosition));
        if (turnDuration < 0)
            throw new ArgumentOutOfRangeException(nameof(turnDuration), "Turn duration must be non-negative");

        TurnType = turnType;
        EndPosition = endPosition;
        TurnDuration = turnDuration;
        Timestamp = DateTime.UtcNow;
    }
}
