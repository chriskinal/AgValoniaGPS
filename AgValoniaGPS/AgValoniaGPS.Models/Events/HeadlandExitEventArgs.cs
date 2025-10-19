namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for headland exit events
/// </summary>
public class HeadlandExitEventArgs : EventArgs
{
    /// <summary>
    /// Pass number (0-based) that was exited
    /// </summary>
    public readonly int PassNumber;

    /// <summary>
    /// Position where headland was exited
    /// </summary>
    public readonly Position ExitPosition;

    /// <summary>
    /// When the exit occurred (UTC)
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of HeadlandExitEventArgs
    /// </summary>
    /// <param name="passNumber">Pass number (0-based)</param>
    /// <param name="exitPosition">Position where headland was exited</param>
    public HeadlandExitEventArgs(int passNumber, Position exitPosition)
    {
        if (passNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(passNumber), "Pass number must be non-negative");

        PassNumber = passNumber;
        ExitPosition = exitPosition ?? throw new ArgumentNullException(nameof(exitPosition));
        Timestamp = DateTime.UtcNow;
    }
}
