using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Specifies the type of change that occurred to an AB line
/// </summary>
public enum ABLineChangeType
{
    /// <summary>
    /// AB line was newly created
    /// </summary>
    Created,

    /// <summary>
    /// AB line properties were modified
    /// </summary>
    Modified,

    /// <summary>
    /// AB line was activated for guidance
    /// </summary>
    Activated,

    /// <summary>
    /// AB line was nudged/offset
    /// </summary>
    Nudged
}

/// <summary>
/// Event arguments for AB line state changes
/// </summary>
public class ABLineChangedEventArgs : EventArgs
{
    /// <summary>
    /// The AB line that changed
    /// </summary>
    public readonly ABLine Line;

    /// <summary>
    /// The type of change that occurred
    /// </summary>
    public readonly ABLineChangeType ChangeType;

    /// <summary>
    /// When the change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of ABLineChangedEventArgs
    /// </summary>
    /// <param name="line">The AB line that changed</param>
    /// <param name="changeType">The type of change that occurred</param>
    public ABLineChangedEventArgs(ABLine line, ABLineChangeType changeType)
    {
        Line = line ?? throw new ArgumentNullException(nameof(line));
        ChangeType = changeType;
        Timestamp = DateTime.UtcNow;
    }
}
