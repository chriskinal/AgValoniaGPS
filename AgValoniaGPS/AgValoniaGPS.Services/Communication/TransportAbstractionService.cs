using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Communication.Transports;

namespace AgValoniaGPS.Services.Communication
{
    /// <summary>
    /// Manages pluggable transport layers across multiple hardware modules.
    /// Routes messages to appropriate transports, forwards events, and handles lifecycle management.
    /// </summary>
    /// <remarks>
    /// This service enables per-module transport selection (e.g., AutoSteer via Bluetooth, Machine via UDP).
    /// Thread-safe for concurrent access from multiple services and UI thread.
    /// Transport instances are created via factory pattern for testability.
    /// </remarks>
    public class TransportAbstractionService : ITransportAbstractionService
    {
        private readonly object _lockObject = new object();
        private readonly Dictionary<ModuleType, ITransport> _activeTransports = new Dictionary<ModuleType, ITransport>();
        private readonly Dictionary<ModuleType, TransportType> _preferredTransports = new Dictionary<ModuleType, TransportType>();
        private readonly Dictionary<TransportType, Func<ITransport>> _transportFactories = new Dictionary<TransportType, Func<ITransport>>();

        public event EventHandler<TransportDataReceivedEventArgs> DataReceived;
        public event EventHandler<TransportStateChangedEventArgs> TransportStateChanged;

        /// <summary>
        /// Creates a new TransportAbstractionService with default transport factories.
        /// </summary>
        public TransportAbstractionService()
        {
            // Transport factories will be registered by DI or explicitly for testing
        }

        /// <summary>
        /// Registers a factory function for creating transport instances.
        /// Used for dependency injection and testing with mock transports.
        /// </summary>
        /// <param name="transportType">The transport type to register</param>
        /// <param name="factory">Factory function that creates transport instances</param>
        public void RegisterTransportFactory(TransportType transportType, Func<ITransport> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (_lockObject)
            {
                _transportFactories[transportType] = factory;
            }
        }

        public async Task StartTransportAsync(ModuleType module, TransportType transport)
        {
            ITransport transportInstance;

            lock (_lockObject)
            {
                // Check if module already has an active transport
                if (_activeTransports.ContainsKey(module))
                {
                    throw new InvalidOperationException(
                        $"Module {module} already has an active transport. Stop the current transport before starting a new one.");
                }

                // Get or create transport instance
                if (!_transportFactories.ContainsKey(transport))
                {
                    throw new ArgumentException(
                        $"Transport type {transport} is not registered. Register a factory before starting.",
                        nameof(transport));
                }

                transportInstance = _transportFactories[transport]();

                // Subscribe to transport events before starting
                transportInstance.DataReceived += OnTransportDataReceived;
                transportInstance.ConnectionChanged += OnTransportConnectionChanged;

                // Store transport in active dictionary
                _activeTransports[module] = transportInstance;
            }

            // Start the transport (outside lock to avoid deadlock)
            await transportInstance.StartAsync();

            // Raise state changed event
            OnTransportStateChanged(new TransportStateChangedEventArgs(
                module,
                transport,
                transportInstance.IsConnected,
                $"{module} transport started"));
        }

        public async Task StopTransportAsync(ModuleType module)
        {
            ITransport transportInstance;

            lock (_lockObject)
            {
                if (!_activeTransports.TryGetValue(module, out transportInstance))
                {
                    // No active transport - nothing to stop
                    return;
                }

                // Remove from active transports
                _activeTransports.Remove(module);
            }

            // Unsubscribe from events
            transportInstance.DataReceived -= OnTransportDataReceived;
            transportInstance.ConnectionChanged -= OnTransportConnectionChanged;

            // Stop the transport (outside lock)
            await transportInstance.StopAsync();

            // Raise state changed event
            OnTransportStateChanged(new TransportStateChangedEventArgs(
                module,
                transportInstance.Type,
                false,
                $"{module} transport stopped"));
        }

        public void SendMessage(ModuleType module, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ITransport transport;

            lock (_lockObject)
            {
                if (!_activeTransports.TryGetValue(module, out transport))
                {
                    throw new InvalidOperationException(
                        $"Module {module} has no active transport. Start a transport before sending messages.");
                }
            }

            // Send message through the transport (outside lock)
            transport.Send(data);
        }

        public TransportType GetActiveTransport(ModuleType module)
        {
            lock (_lockObject)
            {
                if (!_activeTransports.TryGetValue(module, out var transport))
                {
                    throw new InvalidOperationException(
                        $"Module {module} has no active transport.");
                }

                return transport.Type;
            }
        }

        public void SetPreferredTransport(ModuleType module, TransportType transport)
        {
            lock (_lockObject)
            {
                _preferredTransports[module] = transport;
            }
        }

        /// <summary>
        /// Gets the preferred transport for a module (if set).
        /// </summary>
        /// <param name="module">The module to query</param>
        /// <param name="transportType">Output parameter for the preferred transport</param>
        /// <returns>True if a preference is set, false otherwise</returns>
        public bool TryGetPreferredTransport(ModuleType module, out TransportType transportType)
        {
            lock (_lockObject)
            {
                return _preferredTransports.TryGetValue(module, out transportType);
            }
        }

        /// <summary>
        /// Handles data received events from transports.
        /// Identifies the source module and forwards the event.
        /// </summary>
        private void OnTransportDataReceived(object sender, byte[] data)
        {
            if (!(sender is ITransport transport))
                return;

            ModuleType sourceModule;

            lock (_lockObject)
            {
                // Find which module this transport belongs to
                sourceModule = GetModuleForTransport(transport);
            }

            // Raise the DataReceived event with module identification
            DataReceived?.Invoke(this, new TransportDataReceivedEventArgs(sourceModule, data));
        }

        /// <summary>
        /// Handles connection state change events from transports.
        /// Identifies the source module and forwards the event.
        /// </summary>
        private void OnTransportConnectionChanged(object sender, bool isConnected)
        {
            if (!(sender is ITransport transport))
                return;

            ModuleType sourceModule;

            lock (_lockObject)
            {
                sourceModule = GetModuleForTransport(transport);
            }

            // Raise the TransportStateChanged event
            OnTransportStateChanged(new TransportStateChangedEventArgs(
                sourceModule,
                transport.Type,
                isConnected,
                isConnected ? "Connected" : "Disconnected"));
        }

        /// <summary>
        /// Finds the module type for a given transport instance.
        /// Must be called within lock.
        /// </summary>
        private ModuleType GetModuleForTransport(ITransport transport)
        {
            foreach (var kvp in _activeTransports)
            {
                if (ReferenceEquals(kvp.Value, transport))
                {
                    return kvp.Key;
                }
            }

            // Default to AutoSteer if not found (shouldn't happen in normal operation)
            return ModuleType.AutoSteer;
        }

        /// <summary>
        /// Raises the TransportStateChanged event in a thread-safe manner.
        /// </summary>
        private void OnTransportStateChanged(TransportStateChangedEventArgs e)
        {
            TransportStateChanged?.Invoke(this, e);
        }
    }
}
