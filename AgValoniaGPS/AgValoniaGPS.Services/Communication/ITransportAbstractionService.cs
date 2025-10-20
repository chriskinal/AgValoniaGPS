using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication
{
    /// <summary>
    /// Service interface for managing pluggable transport layers across multiple hardware modules.
    /// Supports per-module transport selection (e.g., AutoSteer via Bluetooth, Machine via UDP).
    /// </summary>
    /// <remarks>
    /// This abstraction layer routes messages to the appropriate transport based on module type,
    /// forwards transport events to higher-level services, and manages transport lifecycle.
    /// Enables seamless switching between UDP, Bluetooth, CAN, and Radio transports without
    /// changing protocol-level code (PGN message building/parsing).
    /// </remarks>
    public interface ITransportAbstractionService
    {
        /// <summary>
        /// Starts a transport for the specified module.
        /// Creates and initializes the transport instance based on transport type.
        /// </summary>
        /// <param name="module">The module to start transport for</param>
        /// <param name="transport">The transport type to use (UDP, Bluetooth, CAN, Radio)</param>
        /// <returns>Task that completes when transport is started</returns>
        /// <exception cref="InvalidOperationException">Thrown if module already has an active transport</exception>
        /// <exception cref="ArgumentException">Thrown if transport type is not supported</exception>
        Task StartTransportAsync(ModuleType module, TransportType transport);

        /// <summary>
        /// Stops the active transport for the specified module.
        /// Cleans up resources and removes the transport from the active dictionary.
        /// </summary>
        /// <param name="module">The module to stop transport for</param>
        /// <returns>Task that completes when transport is stopped</returns>
        Task StopTransportAsync(ModuleType module);

        /// <summary>
        /// Sends a message to the specified module using its active transport.
        /// </summary>
        /// <param name="module">The target module</param>
        /// <param name="data">PGN message byte array to send</param>
        /// <exception cref="InvalidOperationException">Thrown if module has no active transport</exception>
        /// <exception cref="ArgumentNullException">Thrown if data is null</exception>
        void SendMessage(ModuleType module, byte[] data);

        /// <summary>
        /// Gets the currently active transport type for a module.
        /// </summary>
        /// <param name="module">The module to query</param>
        /// <returns>The active transport type</returns>
        /// <exception cref="InvalidOperationException">Thrown if module has no active transport</exception>
        TransportType GetActiveTransport(ModuleType module);

        /// <summary>
        /// Sets the preferred transport type for a module.
        /// This preference will be used the next time StartTransportAsync is called.
        /// Does not automatically switch running transports.
        /// </summary>
        /// <param name="module">The module to set preference for</param>
        /// <param name="transport">The preferred transport type</param>
        void SetPreferredTransport(ModuleType module, TransportType transport);

        /// <summary>
        /// Raised when data is received from any transport.
        /// Event includes module identification and received data.
        /// </summary>
        event EventHandler<TransportDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Raised when a transport's connection state changes.
        /// Event includes module, transport type, and connection status.
        /// </summary>
        event EventHandler<TransportStateChangedEventArgs> TransportStateChanged;
    }
}
