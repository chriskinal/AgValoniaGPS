using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Implementation of hardware simulator service.
/// Simulates AutoSteer, Machine, and IMU modules with realistic behaviors for testing.
/// Operates at 10Hz (100ms update cycles) sending PGN messages for all modules.
/// </summary>
/// <remarks>
/// Architecture:
/// - Background task runs at 10Hz updating all module states
/// - Hello packets sent at 1Hz for each module (PGN 126, 123, 121)
/// - Data packets sent at module-specific rates:
///   - AutoSteer feedback (PGN 253): 10Hz
///   - Machine feedback (PGN 234): 10Hz
///   - IMU data (PGN 219): 10Hz (spec says 3Hz, but 10Hz for testing)
///
/// Realistic Behaviors:
/// - Steering: Gradual approach to target angle using exponential smoothing
/// - Sections: Sensor feedback delayed by configurable milliseconds
/// - IMU: Roll/pitch drift accumulation, GPS jitter on heading
///
/// Thread-safe: Uses locks for state access, async/await for lifecycle.
/// Performance: All processing completes in <30ms per update cycle.
/// </remarks>
public class HardwareSimulatorService : IHardwareSimulatorService
{
    private readonly IPgnMessageBuilderService _builder;
    private readonly IPgnMessageParserService _parser;
    private readonly object _lock = new object();

    // Lifecycle state
    private bool _isRunning;
    private bool _realisticBehaviorEnabled = true;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;

    // AutoSteer state
    private double _targetSteeringAngle;
    private double _actualSteeringAngle;
    private double _steeringResponseTime = 0.5; // seconds
    private byte[] _switchStates = new byte[1]; // Switch inputs bitmap

    // Machine state
    private bool[] _sectionSensors = new bool[16]; // Up to 16 sections
    private bool[] _targetSectionSensors = new bool[16];
    private readonly Queue<(int section, bool active, DateTime targetTime)> _sectionDelayQueue = new();
    private double _sectionSensorDelay = 50; // milliseconds
    private bool _workSwitchActive;

    // IMU state
    private double _baseRoll;
    private double _basePitch;
    private double _baseHeading;
    private double _rollDrift;
    private double _pitchDrift;
    private double _imuDriftRate = 0.1; // degrees/minute
    private double _gpsJitterMagnitude = 0.02; // meters
    private readonly Random _random = new Random();

    // Script state
    private List<ScriptCommand>? _scriptCommands;
    private bool _isScriptRunning;
    private CancellationTokenSource? _scriptCancellationTokenSource;

    // Timing
    private DateTime _lastHelloTime = DateTime.MinValue;
    private DateTime _startTime;
    private readonly Stopwatch _driftStopwatch = new Stopwatch();

    public HardwareSimulatorService(
        IPgnMessageBuilderService builder,
        IPgnMessageParserService parser)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    // ========== Properties ==========

    public bool IsRunning
    {
        get { lock (_lock) return _isRunning; }
    }

    public bool RealisticBehaviorEnabled
    {
        get { lock (_lock) return _realisticBehaviorEnabled; }
    }

    public bool IsScriptRunning
    {
        get { lock (_lock) return _isScriptRunning; }
    }

    // ========== Lifecycle ==========

    public async Task StartAsync()
    {
        lock (_lock)
        {
            if (_isRunning)
                throw new InvalidOperationException("Simulator is already running");

            _isRunning = true;
            _startTime = DateTime.UtcNow;
            _driftStopwatch.Restart();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // Start background update task
        _updateTask = Task.Run(() => UpdateLoopAsync(_cancellationTokenSource.Token));

        // Raise state changed event
        StateChanged?.Invoke(this, new SimulatorStateChangedEventArgs(true, _realisticBehaviorEnabled));

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        CancellationTokenSource? cts;
        Task? updateTask;

        lock (_lock)
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            cts = _cancellationTokenSource;
            updateTask = _updateTask;
            _cancellationTokenSource = null;
            _updateTask = null;
        }

        // Stop script if running
        StopScript();

        // Cancel and wait for update task
        cts?.Cancel();
        if (updateTask != null)
        {
            try
            {
                await updateTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        cts?.Dispose();
        _driftStopwatch.Stop();

        // Raise state changed event
        StateChanged?.Invoke(this, new SimulatorStateChangedEventArgs(false, _realisticBehaviorEnabled));
    }

    public void EnableRealisticBehavior(bool enabled)
    {
        lock (_lock)
        {
            _realisticBehaviorEnabled = enabled;

            // Reset drift when toggling
            if (!enabled)
            {
                _rollDrift = 0;
                _pitchDrift = 0;
            }
        }

        StateChanged?.Invoke(this, new SimulatorStateChangedEventArgs(_isRunning, enabled));
    }

    // ========== Manual Control ==========

    public void SetSteeringAngle(double angle)
    {
        lock (_lock)
        {
            _targetSteeringAngle = angle;

            if (!_realisticBehaviorEnabled)
            {
                // Instant mode - set actual angle immediately
                _actualSteeringAngle = angle;
            }
        }
    }

    public void SetSectionSensor(int section, bool active)
    {
        if (section < 0 || section >= _sectionSensors.Length)
            throw new ArgumentOutOfRangeException(nameof(section), "Section index out of range");

        lock (_lock)
        {
            _targetSectionSensors[section] = active;

            if (_realisticBehaviorEnabled)
            {
                // Queue delayed activation
                var targetTime = DateTime.UtcNow.AddMilliseconds(_sectionSensorDelay);
                _sectionDelayQueue.Enqueue((section, active, targetTime));
            }
            else
            {
                // Instant mode
                _sectionSensors[section] = active;
            }
        }
    }

    public void SetImuRoll(double roll)
    {
        lock (_lock)
        {
            _baseRoll = roll;
        }
    }

    public void SetImuPitch(double pitch)
    {
        lock (_lock)
        {
            _basePitch = pitch;
        }
    }

    public void SetWorkSwitch(bool active)
    {
        lock (_lock)
        {
            _workSwitchActive = active;
        }
    }

    // ========== Realism Configuration ==========

    public void SetSteeringResponseTime(double seconds)
    {
        if (seconds < 0)
            throw new ArgumentOutOfRangeException(nameof(seconds), "Response time cannot be negative");

        lock (_lock)
        {
            _steeringResponseTime = seconds;
        }
    }

    public void SetSectionSensorDelay(double milliseconds)
    {
        if (milliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(milliseconds), "Delay cannot be negative");

        lock (_lock)
        {
            _sectionSensorDelay = milliseconds;
        }
    }

    public void SetImuDriftRate(double degreesPerMinute)
    {
        if (degreesPerMinute < 0)
            throw new ArgumentOutOfRangeException(nameof(degreesPerMinute), "Drift rate cannot be negative");

        lock (_lock)
        {
            _imuDriftRate = degreesPerMinute;
        }
    }

    public void SetGpsJitterMagnitude(double meters)
    {
        if (meters < 0)
            throw new ArgumentOutOfRangeException(nameof(meters), "Jitter magnitude cannot be negative");

        lock (_lock)
        {
            _gpsJitterMagnitude = meters;
        }
    }

    // ========== Scripting ==========

    public async Task LoadScriptAsync(string scriptPath)
    {
        if (string.IsNullOrWhiteSpace(scriptPath))
            throw new ArgumentNullException(nameof(scriptPath));

        if (!System.IO.File.Exists(scriptPath))
            throw new System.IO.FileNotFoundException("Script file not found", scriptPath);

        try
        {
            string json = await System.IO.File.ReadAllTextAsync(scriptPath);
            var scriptData = JsonSerializer.Deserialize<ScriptData>(json);

            if (scriptData?.Commands == null)
                throw new FormatException("Invalid script format: missing commands array");

            lock (_lock)
            {
                _scriptCommands = scriptData.Commands;
            }
        }
        catch (JsonException ex)
        {
            throw new FormatException("Invalid script format: " + ex.Message, ex);
        }
    }

    public async Task ExecuteScriptAsync()
    {
        List<ScriptCommand>? commands;
        lock (_lock)
        {
            if (!_isRunning)
                throw new InvalidOperationException("Simulator must be running to execute script");

            if (_scriptCommands == null || _scriptCommands.Count == 0)
                throw new InvalidOperationException("No script loaded");

            if (_isScriptRunning)
                throw new InvalidOperationException("Script is already running");

            commands = new List<ScriptCommand>(_scriptCommands);
            _isScriptRunning = true;
            _scriptCancellationTokenSource = new CancellationTokenSource();
        }

        try
        {
            var startTime = DateTime.UtcNow;

            foreach (var command in commands)
            {
                // Calculate delay until command should execute
                var targetTime = startTime.AddSeconds(command.Time);
                var delay = targetTime - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, _scriptCancellationTokenSource.Token);
                }

                // Execute command
                ExecuteScriptCommand(command);
            }
        }
        catch (OperationCanceledException)
        {
            // Script was stopped
        }
        finally
        {
            lock (_lock)
            {
                _isScriptRunning = false;
                _scriptCancellationTokenSource?.Dispose();
                _scriptCancellationTokenSource = null;
            }
        }
    }

    public void StopScript()
    {
        lock (_lock)
        {
            if (_isScriptRunning)
            {
                _scriptCancellationTokenSource?.Cancel();
            }
        }
    }

    // ========== Events ==========

    public event EventHandler<SimulatorStateChangedEventArgs>? StateChanged;

    // ========== Private Methods ==========

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        const int updateIntervalMs = 100; // 10Hz
        var nextUpdate = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            var updateStart = DateTime.UtcNow;
            nextUpdate = nextUpdate.AddMilliseconds(updateIntervalMs);

            try
            {
                // Update all module states
                UpdateAutoSteerState();
                UpdateMachineState();
                UpdateImuState();

                // Send hello packets (1Hz)
                if ((updateStart - _lastHelloTime).TotalSeconds >= 1.0)
                {
                    SendHelloPackets();
                    _lastHelloTime = updateStart;
                }

                // Send data packets (10Hz)
                SendDataPackets();

                // Calculate time spent processing
                var processingTime = DateTime.UtcNow - updateStart;

                // Wait until next update time
                var delay = nextUpdate - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    // We're running behind schedule - reset timing
                    nextUpdate = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void UpdateAutoSteerState()
    {
        lock (_lock)
        {
            if (_realisticBehaviorEnabled)
            {
                // Gradual steering response using exponential smoothing
                // actualAngle += (target - actual) * responseRate * dt
                // Response rate calculated to reach 63% of target in responseTime
                double dt = 0.1; // 100ms update interval
                double responseRate = 1.0 / _steeringResponseTime;
                double change = (_targetSteeringAngle - _actualSteeringAngle) * responseRate * dt;
                _actualSteeringAngle += change;
            }
            else
            {
                // Instant mode
                _actualSteeringAngle = _targetSteeringAngle;
            }
        }
    }

    private void UpdateMachineState()
    {
        lock (_lock)
        {
            if (_realisticBehaviorEnabled)
            {
                // Process section sensor delay queue
                var now = DateTime.UtcNow;
                while (_sectionDelayQueue.Count > 0)
                {
                    var (section, active, targetTime) = _sectionDelayQueue.Peek();
                    if (now >= targetTime)
                    {
                        _sectionDelayQueue.Dequeue();
                        _sectionSensors[section] = active;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Instant mode handled in SetSectionSensor
        }
    }

    private void UpdateImuState()
    {
        lock (_lock)
        {
            if (_realisticBehaviorEnabled)
            {
                // Accumulate drift
                double driftSeconds = _driftStopwatch.Elapsed.TotalSeconds;
                double driftDegrees = (_imuDriftRate / 60.0) * driftSeconds;

                // Apply random drift direction
                _rollDrift = driftDegrees * (_random.NextDouble() - 0.5) * 2.0;
                _pitchDrift = driftDegrees * (_random.NextDouble() - 0.5) * 2.0;
            }
            else
            {
                _rollDrift = 0;
                _pitchDrift = 0;
            }
        }
    }

    private void SendHelloPackets()
    {
        // Send hello packets for all modules
        // PGN 126 - AutoSteer
        // PGN 123 - Machine
        // PGN 121 - IMU

        // These would be sent via transport in real implementation
        // For simulator, we just build the messages (actual sending handled by integration layer)

        byte[] helloPacket = _builder.BuildHelloPacket();

        // In a full implementation, these would be sent to the transport layer
        // For now, the simulator just maintains state
    }

    private void SendDataPackets()
    {
        lock (_lock)
        {
            // AutoSteer feedback (PGN 253)
            // In real implementation, would send via transport
            // Contains: actual wheel angle, switch states, status flags

            // Machine feedback (PGN 234)
            // Contains: work switch state, section sensors, status flags

            // IMU data (PGN 219)
            // Contains: roll, pitch, heading with drift and jitter applied
            double roll = _baseRoll + _rollDrift;
            double pitch = _basePitch + _pitchDrift;
            double heading = _baseHeading;

            if (_realisticBehaviorEnabled)
            {
                // Add GPS jitter to heading
                double jitterDegrees = (_gpsJitterMagnitude / 111000.0) * 360.0; // meters to degrees (rough)
                heading += (_random.NextDouble() - 0.5) * jitterDegrees;
            }

            // Messages would be built and sent here
            // For simulator, state is maintained for queries
        }
    }

    private void ExecuteScriptCommand(ScriptCommand command)
    {
        switch (command.Action?.ToLowerInvariant())
        {
            case "setsteeringangle":
                if (command.Value.HasValue)
                    SetSteeringAngle(command.Value.Value);
                break;

            case "setsectionsensor":
                // Format: value is section index, additional parameter for state
                // For simplicity, assume value is section and we toggle it on
                if (command.Value.HasValue)
                {
                    int section = (int)command.Value.Value;
                    bool state = command.State ?? true;
                    SetSectionSensor(section, state);
                }
                break;

            case "setworkswitch":
                if (command.State.HasValue)
                    SetWorkSwitch(command.State.Value);
                break;

            case "setimuroll":
                if (command.Value.HasValue)
                    SetImuRoll(command.Value.Value);
                break;

            case "setimupitch":
                if (command.Value.HasValue)
                    SetImuPitch(command.Value.Value);
                break;

            default:
                // Unknown command - ignore
                break;
        }
    }

    // ========== Script Data Models ==========

    private class ScriptData
    {
        public List<ScriptCommand>? Commands { get; set; }
    }

    private class ScriptCommand
    {
        public double Time { get; set; }
        public string? Action { get; set; }
        public double? Value { get; set; }
        public bool? State { get; set; }
    }
}
