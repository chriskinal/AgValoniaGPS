namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for work switch state change events.
/// Raised when the machine work switch is activated or deactivated.
/// </summary>
public class WorkSwitchChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the work switch is active.
    /// </summary>
    public readonly bool IsActive;

    /// <summary>
    /// Gets the timestamp when the work switch changed.
    /// </summary>
    public readonly DateTime ChangedAt;

    /// <summary>
    /// Creates a new instance of WorkSwitchChangedEventArgs.
    /// </summary>
    /// <param name="isActive">Work switch active state</param>
    public WorkSwitchChangedEventArgs(bool isActive)
    {
        IsActive = isActive;
        ChangedAt = DateTime.UtcNow;
    }
}
