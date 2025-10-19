namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for headland entry events
/// </summary>
public class HeadlandEntryEventArgs : EventArgs
{
    /// <summary>
    /// Pass number (0-based) that was entered
    /// </summary>
    public readonly int PassNumber;

    /// <summary>
    /// Position where headland was entered
    /// </summary>
    public readonly Position EntryPosition;

    /// <summary>
    /// When the entry occurred (UTC)
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of HeadlandEntryEventArgs
    /// </summary>
    /// <param name="passNumber">Pass number (0-based)</param>
    /// <param name="entryPosition">Position where headland was entered</param>
    public HeadlandEntryEventArgs(int passNumber, Position entryPosition)
    {
        if (passNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(passNumber), "Pass number must be non-negative");

        PassNumber = passNumber;
        EntryPosition = entryPosition ?? throw new ArgumentNullException(nameof(entryPosition));
        Timestamp = DateTime.UtcNow;
    }
}
