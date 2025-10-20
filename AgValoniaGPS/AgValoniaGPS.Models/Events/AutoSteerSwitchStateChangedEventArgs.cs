namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for AutoSteer switch state change events.
/// Raised when switch states on the AutoSteer module change.
/// </summary>
public class AutoSteerSwitchStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the switch states bitmap.
    /// Each bit represents a switch input on the AutoSteer module.
    /// </summary>
    public readonly byte[] SwitchStates;

    /// <summary>
    /// Gets the timestamp when the switch states changed.
    /// </summary>
    public readonly DateTime ChangedAt;

    /// <summary>
    /// Creates a new instance of AutoSteerSwitchStateChangedEventArgs.
    /// </summary>
    /// <param name="switchStates">Switch states bitmap</param>
    public AutoSteerSwitchStateChangedEventArgs(byte[] switchStates)
    {
        SwitchStates = switchStates ?? throw new ArgumentNullException(nameof(switchStates));
        ChangedAt = DateTime.UtcNow;
    }
}
