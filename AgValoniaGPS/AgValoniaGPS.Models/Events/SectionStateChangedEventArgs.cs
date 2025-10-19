using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Specifies the type of change that occurred to a section state
/// </summary>
public enum SectionStateChangeType
{
    /// <summary>
    /// Section state changed to Auto
    /// </summary>
    ToAuto,

    /// <summary>
    /// Section state changed to ManualOn
    /// </summary>
    ToManualOn,

    /// <summary>
    /// Section state changed to ManualOff
    /// </summary>
    ToManualOff,

    /// <summary>
    /// Section state changed to Off
    /// </summary>
    ToOff,

    /// <summary>
    /// Section timer started
    /// </summary>
    TimerStarted,

    /// <summary>
    /// Section timer cancelled
    /// </summary>
    TimerCancelled
}

/// <summary>
/// Event arguments for section state changes
/// </summary>
public class SectionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// ID of the section that changed
    /// </summary>
    public readonly int SectionId;

    /// <summary>
    /// Previous state of the section
    /// </summary>
    public readonly SectionState OldState;

    /// <summary>
    /// New state of the section
    /// </summary>
    public readonly SectionState NewState;

    /// <summary>
    /// Type of change that occurred
    /// </summary>
    public readonly SectionStateChangeType ChangeType;

    /// <summary>
    /// When the change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of SectionStateChangedEventArgs
    /// </summary>
    /// <param name="sectionId">ID of the section that changed</param>
    /// <param name="oldState">Previous state</param>
    /// <param name="newState">New state</param>
    /// <param name="changeType">Type of change</param>
    public SectionStateChangedEventArgs(int sectionId, SectionState oldState, SectionState newState, SectionStateChangeType changeType)
    {
        if (sectionId < 0)
            throw new ArgumentOutOfRangeException(nameof(sectionId), "Section ID must be non-negative");

        SectionId = sectionId;
        OldState = oldState;
        NewState = newState;
        ChangeType = changeType;
        Timestamp = DateTime.UtcNow;
    }
}
