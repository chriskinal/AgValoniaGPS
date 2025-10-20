using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for coordinating hardware module lifecycle and connection monitoring.
/// Manages module state transitions, hello packet monitoring, data flow tracking,
/// and ready state management with lazy initialization support.
/// </summary>
/// <remarks>
/// Thread-safe implementation with lock-protected state access.
/// Background timer monitors hello and data timeouts at 50ms intervals.
/// Performance requirement: Timeout checking overhead less than 5ms per cycle.
/// </remarks>
public class ModuleCoordinatorService : IModuleCoordinatorService, IDisposable
{
    private readonly ITransportAbstractionService _transport;
    private readonly IPgnMessageParserService _parser;
    private readonly IPgnMessageBuilderService _builder;

    private readonly Dictionary<ModuleType, ModuleConnectionState> _moduleStates;
    private readonly object _stateLock = new object();

    private Timer? _timeoutMonitor;
    private const int TimeoutMonitorIntervalMs = 50;
    private const int HelloTimeoutMs = 2000; // 2 seconds
    private const int AutoSteerMachineDataTimeoutMs = 100; // 100ms for AutoSteer/Machine
    private const int ImuDataTimeoutMs = 300; // 300ms for IMU

    /// <summary>
    /// Raised when a module successfully connects (hello packet received).
    /// </summary>
    public event EventHandler<ModuleConnectedEventArgs>? ModuleConnected;

    /// <summary>
    /// Raised when a module disconnects or times out.
    /// </summary>
    public event EventHandler<ModuleDisconnectedEventArgs>? ModuleDisconnected;

    /// <summary>
    /// Raised when a module reaches Ready state (hello + capability negotiation complete).
    /// </summary>
    public event EventHandler<ModuleReadyEventArgs>? ModuleReady;

    /// <summary>
    /// Initializes a new instance of ModuleCoordinatorService.
    /// </summary>
    /// <param name="transport">Transport abstraction service for multi-transport routing</param>
    /// <param name="parser">PGN message parser for hello packet and data detection</param>
    /// <param name="builder">PGN message builder for hello packet generation</param>
    public ModuleCoordinatorService(
        ITransportAbstractionService transport,
        IPgnMessageParserService parser,
        IPgnMessageBuilderService builder)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));

        _moduleStates = new Dictionary<ModuleType, ModuleConnectionState>
        {
            { ModuleType.AutoSteer, new ModuleConnectionState() },
            { ModuleType.Machine, new ModuleConnectionState() },
            { ModuleType.IMU, new ModuleConnectionState() }
        };

        // Subscribe to transport events
        _transport.DataReceived += OnTransportDataReceived;
        _transport.TransportStateChanged += OnTransportStateChanged;

        // Start timeout monitoring timer
        _timeoutMonitor = new Timer(CheckTimeouts, null, TimeoutMonitorIntervalMs, TimeoutMonitorIntervalMs);
    }

    /// <summary>
    /// Gets the current connection state of a module.
    /// </summary>
    public ModuleState GetModuleState(ModuleType module)
    {
        lock (_stateLock)
        {
            return _moduleStates[module].State;
        }
    }

    /// <summary>
    /// Checks if a module is ready to receive commands.
    /// </summary>
    public bool IsModuleReady(ModuleType module)
    {
        lock (_stateLock)
        {
            return _moduleStates[module].State == ModuleState.Ready;
        }
    }

    /// <summary>
    /// Gets the timestamp of the last hello packet received from a module.
    /// </summary>
    public DateTime GetLastHelloTime(ModuleType module)
    {
        lock (_stateLock)
        {
            return _moduleStates[module].LastHelloTime;
        }
    }

    /// <summary>
    /// Gets the timestamp of the last data packet received from a module.
    /// </summary>
    public DateTime GetLastDataTime(ModuleType module)
    {
        lock (_stateLock)
        {
            return _moduleStates[module].LastDataTime;
        }
    }

    /// <summary>
    /// Initializes a module by starting its transport and sending hello packet.
    /// Waits for module to reach Ready state or timeout.
    /// </summary>
    public async Task InitializeModuleAsync(ModuleType module)
    {
        // Start transport (default to UDP)
        await _transport.StartTransportAsync(module, TransportType.UDP);

        // Send hello packet
        var helloPacket = _builder.BuildHelloPacket();
        _transport.SendMessage(module, helloPacket);

        // Wait for Ready state with timeout
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (!IsModuleReady(module) && DateTime.UtcNow < timeout)
        {
            await Task.Delay(50);
        }

        if (!IsModuleReady(module))
        {
            throw new TimeoutException($"Module {module} did not reach Ready state within timeout period");
        }
    }

    /// <summary>
    /// Resets a module to Disconnected state and clears all timestamps.
    /// </summary>
    public void ResetModule(ModuleType module)
    {
        lock (_stateLock)
        {
            var state = _moduleStates[module];
            state.State = ModuleState.Disconnected;
            state.LastHelloTime = DateTime.MinValue;
            state.LastDataTime = DateTime.MinValue;
            state.FirmwareVersion = 0;
            state.Capabilities = new ModuleCapabilities();
        }

        // Publish disconnection event
        ModuleDisconnected?.Invoke(this, new ModuleDisconnectedEventArgs(module, "Manual Reset"));
    }

    /// <summary>
    /// Handles data received from transport abstraction layer.
    /// Parses hello packets and data packets, updates timestamps and state.
    /// </summary>
    private void OnTransportDataReceived(object? sender, TransportDataReceivedEventArgs e)
    {
        // Try parsing as hello packet first
        var helloResponse = _parser.ParseHelloPacket(e.Data);
        if (helloResponse != null)
        {
            ProcessHelloPacket(helloResponse);
            return;
        }

        // Not a hello packet - update data flow timestamp
        lock (_stateLock)
        {
            _moduleStates[e.Module].LastDataTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Processes a hello packet received from a module.
    /// Updates state, timestamps, and triggers ready state transition.
    /// </summary>
    private void ProcessHelloPacket(HelloResponse helloResponse)
    {
        var module = helloResponse.ModuleType;
        bool wasDisconnected = false;
        bool transitionToReady = false;

        lock (_stateLock)
        {
            var state = _moduleStates[module];

            wasDisconnected = (state.State == ModuleState.Disconnected || state.State == ModuleState.Timeout);

            // Update hello timestamp
            state.LastHelloTime = DateTime.UtcNow;
            state.FirmwareVersion = helloResponse.Version;

            // Transition state
            if (state.State == ModuleState.Disconnected || state.State == ModuleState.Timeout)
            {
                state.State = ModuleState.HelloReceived;
            }

            // For V1 modules (version 0 or 1), transition to Ready immediately
            // V2 modules (version >= 2) require capability negotiation
            if (helloResponse.Version <= 1 && state.State == ModuleState.HelloReceived)
            {
                state.State = ModuleState.Ready;
                transitionToReady = true;
            }
        }

        // Publish events outside of lock
        if (wasDisconnected)
        {
            var version = new FirmwareVersion
            {
                Major = helloResponse.Version,
                Minor = 0,
                Patch = 0
            };

            ModuleConnected?.Invoke(this, new ModuleConnectedEventArgs(
                module,
                TransportType.UDP, // TODO: Get actual transport type from transport abstraction
                version));
        }

        if (transitionToReady)
        {
            ModuleReady?.Invoke(this, new ModuleReadyEventArgs(
                module,
                new ModuleCapabilities()));
        }
    }

    /// <summary>
    /// Handles transport state changes (connection/disconnection).
    /// </summary>
    private void OnTransportStateChanged(object? sender, TransportStateChangedEventArgs e)
    {
        if (e.IsConnected)
        {
            // Transport layer connection - transition to Connecting state
            lock (_stateLock)
            {
                if (_moduleStates.TryGetValue(e.Module, out var moduleState))
                {
                    if (moduleState.State == ModuleState.Disconnected)
                    {
                        moduleState.State = ModuleState.Connecting;
                    }
                }
            }
        }
        else
        {
            // Transport layer disconnection - reset module state
            ResetModule(e.Module);
        }
    }

    /// <summary>
    /// Background timer callback that checks for hello and data timeouts.
    /// Runs at 50ms intervals to detect timeouts and publish disconnection events.
    /// </summary>
    private void CheckTimeouts(object? state)
    {
        var now = DateTime.UtcNow;
        var modulesToTimeout = new List<(ModuleType, string)>();

        lock (_stateLock)
        {
            foreach (var kvp in _moduleStates)
            {
                var module = kvp.Key;
                var moduleState = kvp.Value;

                // Skip if already disconnected or in timeout
                if (moduleState.State == ModuleState.Disconnected || moduleState.State == ModuleState.Timeout)
                    continue;

                // Check hello timeout (2 seconds)
                if (moduleState.LastHelloTime != DateTime.MinValue)
                {
                    var helloAge = (now - moduleState.LastHelloTime).TotalMilliseconds;
                    if (helloAge > HelloTimeoutMs)
                    {
                        moduleState.State = ModuleState.Timeout;
                        modulesToTimeout.Add((module, "Hello packet timeout"));
                        continue;
                    }
                }

                // Check data flow timeout (100ms for AutoSteer/Machine, 300ms for IMU)
                if (moduleState.LastDataTime != DateTime.MinValue && moduleState.State == ModuleState.Ready)
                {
                    var dataAge = (now - moduleState.LastDataTime).TotalMilliseconds;
                    var dataTimeout = (module == ModuleType.IMU) ? ImuDataTimeoutMs : AutoSteerMachineDataTimeoutMs;

                    if (dataAge > dataTimeout)
                    {
                        moduleState.State = ModuleState.Timeout;
                        modulesToTimeout.Add((module, "Data flow timeout"));
                    }
                }
            }
        }

        // Publish disconnection events outside of lock
        foreach (var (module, reason) in modulesToTimeout)
        {
            ModuleDisconnected?.Invoke(this, new ModuleDisconnectedEventArgs(module, reason));
        }
    }

    /// <summary>
    /// Disposes resources including the timeout monitoring timer.
    /// </summary>
    public void Dispose()
    {
        _timeoutMonitor?.Dispose();
        _timeoutMonitor = null;
    }

    /// <summary>
    /// Internal state tracking for a single module.
    /// </summary>
    private class ModuleConnectionState
    {
        public ModuleState State { get; set; } = ModuleState.Disconnected;
        public DateTime LastHelloTime { get; set; } = DateTime.MinValue;
        public DateTime LastDataTime { get; set; } = DateTime.MinValue;
        public byte FirmwareVersion { get; set; } = 0;
        public ModuleCapabilities Capabilities { get; set; } = new ModuleCapabilities();
    }
}
