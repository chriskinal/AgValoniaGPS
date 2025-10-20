using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for transport state change events.
/// Raised when a module's transport connection state changes.
/// </summary>
public class TransportStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the module associated with this transport.
    /// </summary>
    public readonly ModuleType Module;

    /// <summary>
    /// Gets the transport type.
    /// </summary>
    public readonly TransportType Transport;

    /// <summary>
    /// Gets whether the transport is currently connected.
    /// </summary>
    public readonly bool IsConnected;

    /// <summary>
    /// Gets the state message describing the change.
    /// </summary>
    public readonly string StateMessage;

    /// <summary>
    /// Creates a new instance of TransportStateChangedEventArgs.
    /// </summary>
    /// <param name="module">Module type</param>
    /// <param name="transport">Transport type</param>
    /// <param name="isConnected">Connection state</param>
    /// <param name="stateMessage">State message</param>
    public TransportStateChangedEventArgs(ModuleType module, TransportType transport, bool isConnected, string stateMessage)
    {
        Module = module;
        Transport = transport;
        IsConnected = isConnected;
        StateMessage = stateMessage ?? string.Empty;
    }
}
