namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents the current state of path recording.
/// </summary>
public enum RecordingState
{
    /// <summary>
    /// Not currently recording a path.
    /// </summary>
    Stopped,

    /// <summary>
    /// Actively recording a path.
    /// </summary>
    Recording,

    /// <summary>
    /// Recording is paused (can be resumed).
    /// </summary>
    Paused
}
