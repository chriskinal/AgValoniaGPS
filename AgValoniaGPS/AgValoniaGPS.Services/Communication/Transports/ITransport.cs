using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// Base interface for all hardware communication transports.
    /// Provides a common abstraction for UDP, Bluetooth, CAN, and Radio transports.
    /// </summary>
    /// <remarks>
    /// This interface enables pluggable transport implementations for module communication.
    /// Each transport handles the physical/network layer communication, while higher-level
    /// services handle protocol logic (PGN message building/parsing).
    /// Thread-safe implementations required for concurrent access from multiple modules.
    /// </remarks>
    public interface ITransport
    {
        /// <summary>
        /// Gets the transport type (UDP, Bluetooth, CAN, Radio).
        /// </summary>
        TransportType Type { get; }

        /// <summary>
        /// Gets the current connection status.
        /// True if transport is connected and ready to send/receive data.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Starts the transport and establishes connection.
        /// </summary>
        /// <returns>Task that completes when transport is started</returns>
        /// <exception cref="InvalidOperationException">Thrown if transport is already started</exception>
        /// <exception cref="System.IO.IOException">Thrown if connection cannot be established</exception>
        Task StartAsync();

        /// <summary>
        /// Stops the transport and releases resources.
        /// </summary>
        /// <returns>Task that completes when transport is stopped</returns>
        Task StopAsync();

        /// <summary>
        /// Sends data through the transport.
        /// </summary>
        /// <param name="data">Byte array containing PGN message to send</param>
        /// <exception cref="ArgumentNullException">Thrown if data is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if transport is not connected</exception>
        void Send(byte[] data);

        /// <summary>
        /// Raised when data is received from the transport.
        /// Event handlers receive the raw byte array.
        /// </summary>
        event EventHandler<byte[]> DataReceived;

        /// <summary>
        /// Raised when connection state changes.
        /// Event handlers receive the new connection state (true=connected, false=disconnected).
        /// </summary>
        event EventHandler<bool> ConnectionChanged;
    }
}
