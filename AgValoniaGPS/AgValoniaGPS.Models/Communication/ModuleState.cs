namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Specifies the connection state of a hardware module.
/// </summary>
public enum ModuleState
{
    /// <summary>
    /// Module is disconnected and not communicating
    /// </summary>
    Disconnected,

    /// <summary>
    /// Module connection is being established
    /// </summary>
    Connecting,

    /// <summary>
    /// Hello packet received from module
    /// </summary>
    HelloReceived,

    /// <summary>
    /// Module is ready for communication and sending commands
    /// </summary>
    Ready,

    /// <summary>
    /// Module is in error state
    /// </summary>
    Error,

    /// <summary>
    /// Module connection timed out
    /// </summary>
    Timeout
}
