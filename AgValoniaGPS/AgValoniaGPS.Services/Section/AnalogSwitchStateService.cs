using System;
using System.Collections.Generic;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Implements thread-safe analog switch state management
/// Tracks work/steer/lock switch states and publishes change events
/// </summary>
/// <remarks>
/// Performance: Negligible (simple state tracking)
/// Thread Safety: Uses lock object pattern for concurrent access
/// </remarks>
public class AnalogSwitchStateService : IAnalogSwitchStateService
{
    private readonly object _lockObject = new object();
    private readonly Dictionary<AnalogSwitchType, SwitchState> _switchStates;

    /// <summary>
    /// Event fired when any switch state changes
    /// </summary>
    public event EventHandler<SwitchStateChangedEventArgs>? SwitchStateChanged;

    /// <summary>
    /// Creates a new instance of AnalogSwitchStateService with all switches inactive
    /// </summary>
    public AnalogSwitchStateService()
    {
        _switchStates = new Dictionary<AnalogSwitchType, SwitchState>
        {
            { AnalogSwitchType.WorkSwitch, SwitchState.Inactive },
            { AnalogSwitchType.SteerSwitch, SwitchState.Inactive },
            { AnalogSwitchType.LockSwitch, SwitchState.Inactive }
        };
    }

    /// <summary>
    /// Gets the current state of the specified switch
    /// </summary>
    /// <param name="switchType">Type of switch to query</param>
    /// <returns>Current state of the switch</returns>
    public SwitchState GetSwitchState(AnalogSwitchType switchType)
    {
        lock (_lockObject)
        {
            return _switchStates[switchType];
        }
    }

    /// <summary>
    /// Sets the state of the specified switch and publishes change event if state changed
    /// </summary>
    /// <param name="switchType">Type of switch to set</param>
    /// <param name="state">New state for the switch</param>
    public void SetSwitchState(AnalogSwitchType switchType, SwitchState state)
    {
        SwitchState oldState;

        lock (_lockObject)
        {
            oldState = _switchStates[switchType];

            // Only update and fire event if state actually changed
            if (oldState == state)
                return;

            _switchStates[switchType] = state;
        }

        // Fire event outside of lock to prevent potential deadlocks
        RaiseSwitchStateChanged(switchType, oldState, state);
    }

    /// <summary>
    /// Resets all switches to inactive state
    /// </summary>
    public void ResetAllSwitches()
    {
        lock (_lockObject)
        {
            var changes = new List<(AnalogSwitchType type, SwitchState oldState)>();

            foreach (var switchType in _switchStates.Keys)
            {
                var oldState = _switchStates[switchType];
                if (oldState != SwitchState.Inactive)
                {
                    changes.Add((switchType, oldState));
                    _switchStates[switchType] = SwitchState.Inactive;
                }
            }

            // Fire events outside of lock
            foreach (var (switchType, oldState) in changes)
            {
                RaiseSwitchStateChanged(switchType, oldState, SwitchState.Inactive);
            }
        }
    }

    private void RaiseSwitchStateChanged(AnalogSwitchType switchType, SwitchState oldState, SwitchState newState)
    {
        SwitchStateChanged?.Invoke(this, new SwitchStateChangedEventArgs(switchType, oldState, newState));
    }
}
