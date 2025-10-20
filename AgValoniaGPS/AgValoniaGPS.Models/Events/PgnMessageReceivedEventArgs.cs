using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for PGN message reception events.
/// Raised when a PGN message is received from a hardware module.
/// </summary>
public class PgnMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the PGN number of the received message.
    /// </summary>
    public readonly byte PgnNumber;

    /// <summary>
    /// Gets the raw data bytes of the message.
    /// </summary>
    public readonly byte[] Data;

    /// <summary>
    /// Gets the source module type.
    /// </summary>
    public readonly ModuleType SourceModule;

    /// <summary>
    /// Gets the timestamp when the message was received.
    /// </summary>
    public readonly DateTime ReceivedAt;

    /// <summary>
    /// Creates a new instance of PgnMessageReceivedEventArgs.
    /// </summary>
    /// <param name="pgnNumber">PGN number</param>
    /// <param name="data">Raw data bytes</param>
    /// <param name="sourceModule">Source module type</param>
    public PgnMessageReceivedEventArgs(byte pgnNumber, byte[] data, ModuleType sourceModule)
    {
        PgnNumber = pgnNumber;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        SourceModule = sourceModule;
        ReceivedAt = DateTime.UtcNow;
    }
}
