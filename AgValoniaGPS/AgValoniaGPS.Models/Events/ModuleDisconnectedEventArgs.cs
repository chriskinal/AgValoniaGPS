using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for module disconnection events.
/// Raised when a hardware module disconnects or times out.
/// </summary>
public class ModuleDisconnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of module that disconnected.
    /// </summary>
    public readonly ModuleType ModuleType;

    /// <summary>
    /// Gets the reason for disconnection (e.g., "Timeout", "Transport Error", "Manual").
    /// </summary>
    public readonly string Reason;

    /// <summary>
    /// Gets the timestamp when the module disconnected.
    /// </summary>
    public readonly DateTime DisconnectedAt;

    /// <summary>
    /// Creates a new instance of ModuleDisconnectedEventArgs.
    /// </summary>
    /// <param name="moduleType">Type of module that disconnected</param>
    /// <param name="reason">Reason for disconnection</param>
    public ModuleDisconnectedEventArgs(ModuleType moduleType, string reason)
    {
        ModuleType = moduleType;
        Reason = reason ?? string.Empty;
        DisconnectedAt = DateTime.UtcNow;
    }
}
