using System;

namespace AgValoniaGPS.Models.StateManagement;

/// <summary>
/// Event arguments for session state change notifications.
/// </summary>
public class SessionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of session state change that occurred.
    /// </summary>
    public SessionStateChangeType ChangeType { get; }

    /// <summary>
    /// Gets the timestamp when the state change occurred.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets additional state change data (optional, context-specific).
    /// </summary>
    public object? StateData { get; }

    /// <summary>
    /// Initializes a new instance of the SessionStateChangedEventArgs class.
    /// </summary>
    /// <param name="changeType">The type of session state change</param>
    /// <param name="stateData">Optional state change data</param>
    public SessionStateChangedEventArgs(SessionStateChangeType changeType, object? stateData = null)
    {
        ChangeType = changeType;
        Timestamp = DateTime.UtcNow;
        StateData = stateData;
    }
}

/// <summary>
/// Defines the types of session state changes.
/// </summary>
public enum SessionStateChangeType
{
    /// <summary>
    /// Session has been started.
    /// </summary>
    SessionStarted,

    /// <summary>
    /// Session has been ended.
    /// </summary>
    SessionEnded,

    /// <summary>
    /// Current field name has been updated.
    /// </summary>
    FieldUpdated,

    /// <summary>
    /// Current guidance line has been updated.
    /// </summary>
    GuidanceLineUpdated,

    /// <summary>
    /// Work progress data has been updated.
    /// </summary>
    WorkProgressUpdated,

    /// <summary>
    /// Crash recovery snapshot has been saved.
    /// </summary>
    SnapshotSaved
}
