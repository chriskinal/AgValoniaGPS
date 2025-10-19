using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for analog switch state changes
/// </summary>
public class SwitchStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Type of switch that changed
    /// </summary>
    public readonly AnalogSwitchType SwitchType;

    /// <summary>
    /// Previous state of the switch
    /// </summary>
    public readonly SwitchState OldState;

    /// <summary>
    /// New state of the switch
    /// </summary>
    public readonly SwitchState NewState;

    /// <summary>
    /// When the state change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of SwitchStateChangedEventArgs
    /// </summary>
    /// <param name="switchType">Type of switch</param>
    /// <param name="oldState">Previous state</param>
    /// <param name="newState">New state</param>
    public SwitchStateChangedEventArgs(AnalogSwitchType switchType, SwitchState oldState, SwitchState newState)
    {
        SwitchType = switchType;
        OldState = oldState;
        NewState = newState;
        Timestamp = DateTime.UtcNow;
    }
}
