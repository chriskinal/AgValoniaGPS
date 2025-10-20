using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for transport data reception events.
/// Raised when raw data is received from a transport layer.
/// </summary>
public class TransportDataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the module this data is associated with.
    /// </summary>
    public readonly ModuleType Module;

    /// <summary>
    /// Gets the raw data bytes received.
    /// </summary>
    public readonly byte[] Data;

    /// <summary>
    /// Gets the timestamp when the data was received.
    /// </summary>
    public readonly DateTime ReceivedAt;

    /// <summary>
    /// Creates a new instance of TransportDataReceivedEventArgs.
    /// </summary>
    /// <param name="module">Module type</param>
    /// <param name="data">Raw data bytes</param>
    public TransportDataReceivedEventArgs(ModuleType module, byte[] data)
    {
        Module = module;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        ReceivedAt = DateTime.UtcNow;
    }
}
