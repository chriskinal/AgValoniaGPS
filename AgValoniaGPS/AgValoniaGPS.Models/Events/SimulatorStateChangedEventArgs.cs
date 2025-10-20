namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for hardware simulator state change events.
/// Raised when the simulator starts, stops, or changes behavior modes.
/// </summary>
public class SimulatorStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the simulator is currently running.
    /// </summary>
    public readonly bool IsRunning;

    /// <summary>
    /// Gets whether realistic behavior mode is enabled.
    /// </summary>
    public readonly bool RealisticBehaviorEnabled;

    /// <summary>
    /// Creates a new instance of SimulatorStateChangedEventArgs.
    /// </summary>
    /// <param name="isRunning">Simulator running state</param>
    /// <param name="realisticBehaviorEnabled">Realistic behavior enabled state</param>
    public SimulatorStateChangedEventArgs(bool isRunning, bool realisticBehaviorEnabled)
    {
        IsRunning = isRunning;
        RealisticBehaviorEnabled = realisticBehaviorEnabled;
    }
}
