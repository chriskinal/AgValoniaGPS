using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service interface for coordinating hardware module lifecycle and connection monitoring.
/// Manages module state transitions, hello packet monitoring, data flow tracking,
/// and ready state management with lazy initialization support.
/// </summary>
/// <remarks>
/// This service orchestrates the connection lifecycle of AutoSteer, Machine, and IMU modules:
///
/// State Machine:
/// Disconnected → Connecting → HelloReceived → Ready
///                              ↓              ↓
///                           Timeout ← - - - - ┘
///
/// Hello Monitoring:
/// - 2-second timeout threshold for hello packet detection
/// - Transitions to Timeout state if hello packet not received within threshold
///
/// Data Flow Monitoring:
/// - AutoSteer/Machine: 100ms timeout for data packets
/// - IMU: 300ms timeout for data packets
/// - Tracks last data time independently of hello packets
///
/// Ready State:
/// - V1 modules: Ready immediately after hello packet
/// - V2 modules: Ready after hello + capability negotiation (PGN 210/211)
/// - Commands should only be sent when module is in Ready state
///
/// Lazy Initialization:
/// - Modules initialize on-demand when first accessed
/// - InitializeModuleAsync can be called explicitly or triggered automatically
/// </remarks>
public interface IModuleCoordinatorService
{
    /// <summary>
    /// Gets the current connection state of a module.
    /// </summary>
    /// <param name="module">The module to query</param>
    /// <returns>Current ModuleState (Disconnected, Connecting, HelloReceived, Ready, Timeout, Error)</returns>
    ModuleState GetModuleState(ModuleType module);

    /// <summary>
    /// Checks if a module is ready to receive commands.
    /// Ready state requires hello packet received and capability negotiation complete (for V2 modules).
    /// </summary>
    /// <param name="module">The module to check</param>
    /// <returns>True if module is in Ready state, false otherwise</returns>
    bool IsModuleReady(ModuleType module);

    /// <summary>
    /// Gets the timestamp of the last hello packet received from a module.
    /// Used for timeout detection (2-second threshold).
    /// </summary>
    /// <param name="module">The module to query</param>
    /// <returns>DateTime of last hello packet, or DateTime.MinValue if never received</returns>
    DateTime GetLastHelloTime(ModuleType module);

    /// <summary>
    /// Gets the timestamp of the last data packet received from a module.
    /// Used for data flow monitoring (100ms AutoSteer/Machine, 300ms IMU).
    /// </summary>
    /// <param name="module">The module to query</param>
    /// <returns>DateTime of last data packet, or DateTime.MinValue if never received</returns>
    DateTime GetLastDataTime(ModuleType module);

    /// <summary>
    /// Initializes a module by starting its transport and sending hello packet.
    /// Waits for module to reach Ready state or timeout.
    /// </summary>
    /// <param name="module">The module to initialize</param>
    /// <returns>Task that completes when module is ready or timeout occurs</returns>
    /// <exception cref="TimeoutException">Thrown if module does not reach Ready state within timeout period</exception>
    Task InitializeModuleAsync(ModuleType module);

    /// <summary>
    /// Resets a module to Disconnected state and clears all timestamps.
    /// Use for error recovery or manual reconnection.
    /// Coordinates with steering/section services to reset integrals.
    /// </summary>
    /// <param name="module">The module to reset</param>
    void ResetModule(ModuleType module);

    /// <summary>
    /// Raised when a module successfully connects (hello packet received).
    /// Event includes module type, transport type, firmware version, and connection timestamp.
    /// </summary>
    event EventHandler<ModuleConnectedEventArgs> ModuleConnected;

    /// <summary>
    /// Raised when a module disconnects or times out.
    /// Event includes module type, disconnection reason, and timestamp.
    /// </summary>
    event EventHandler<ModuleDisconnectedEventArgs> ModuleDisconnected;

    /// <summary>
    /// Raised when a module reaches Ready state (hello + capability negotiation complete).
    /// Event includes module type, capabilities, and ready timestamp.
    /// Commands can be sent to module after this event.
    /// </summary>
    event EventHandler<ModuleReadyEventArgs> ModuleReady;
}
