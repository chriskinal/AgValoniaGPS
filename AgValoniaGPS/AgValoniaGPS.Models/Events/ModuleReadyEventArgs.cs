using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for module ready events.
/// Raised when a module completes initialization and is ready to receive commands.
/// </summary>
public class ModuleReadyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of module that is ready.
    /// </summary>
    public readonly ModuleType ModuleType;

    /// <summary>
    /// Gets the capabilities of the ready module.
    /// </summary>
    public readonly ModuleCapabilities Capabilities;

    /// <summary>
    /// Gets the timestamp when the module became ready.
    /// </summary>
    public readonly DateTime ReadyAt;

    /// <summary>
    /// Creates a new instance of ModuleReadyEventArgs.
    /// </summary>
    /// <param name="moduleType">Type of module that is ready</param>
    /// <param name="capabilities">Module capabilities</param>
    public ModuleReadyEventArgs(ModuleType moduleType, ModuleCapabilities capabilities)
    {
        ModuleType = moduleType;
        Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
        ReadyAt = DateTime.UtcNow;
    }
}
