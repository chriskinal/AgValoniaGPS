using System;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Service interface for managing analog switch states (work/steer/lock switches)
/// Provides thread-safe state tracking and change notification
/// </summary>
public interface IAnalogSwitchStateService
{
    /// <summary>
    /// Event fired when any switch state changes
    /// </summary>
    event EventHandler<SwitchStateChangedEventArgs>? SwitchStateChanged;

    /// <summary>
    /// Gets the current state of the specified switch
    /// </summary>
    /// <param name="switchType">Type of switch to query</param>
    /// <returns>Current state of the switch</returns>
    SwitchState GetSwitchState(AnalogSwitchType switchType);

    /// <summary>
    /// Sets the state of the specified switch
    /// </summary>
    /// <param name="switchType">Type of switch to set</param>
    /// <param name="state">New state for the switch</param>
    void SetSwitchState(AnalogSwitchType switchType, SwitchState state);

    /// <summary>
    /// Resets all switches to inactive state
    /// </summary>
    void ResetAllSwitches();
}
