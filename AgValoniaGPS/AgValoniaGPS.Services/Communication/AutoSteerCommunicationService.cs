using System;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for communicating with AutoSteer hardware module.
/// Implements PGN message sending and feedback parsing with thread-safe state management.
/// </summary>
public class AutoSteerCommunicationService : IAutoSteerCommunicationService
{
    private readonly IModuleCoordinatorService _coordinator;
    private readonly IPgnMessageBuilderService _builder;
    private readonly IPgnMessageParserService _parser;
    private readonly ITransportAbstractionService _transport;
    private readonly object _lock = new object();

    private AutoSteerFeedback? _currentFeedback;
    private byte[] _previousSwitchStates = Array.Empty<byte>();

    /// <summary>
    /// Creates a new instance of AutoSteerCommunicationService.
    /// </summary>
    public AutoSteerCommunicationService(
        IModuleCoordinatorService coordinator,
        IPgnMessageBuilderService builder,
        IPgnMessageParserService parser,
        ITransportAbstractionService transport)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));

        // Subscribe to transport data for this module
        _transport.DataReceived += OnTransportDataReceived;
    }

    #region Commands

    /// <inheritdoc/>
    public void SendSteeringCommand(double speedMph, double steerAngle, int xteErrorMm, bool isActive)
    {
        // Check module ready state before sending
        if (!_coordinator.IsModuleReady(ModuleType.AutoSteer))
        {
            // Module not ready - silently drop command
            // Coordinator will handle reconnection if needed
            return;
        }

        // Build PGN 254 message
        byte status = isActive ? (byte)1 : (byte)0;
        byte[] message = _builder.BuildAutoSteerData(speedMph, status, steerAngle, xteErrorMm);

        // Send via transport
        _transport.SendMessage(ModuleType.AutoSteer, message);
    }

    /// <inheritdoc/>
    public void SendSettings(byte pwmDrive, byte minPwm, float proportionalGain, byte highPwm, float lowSpeedPwm)
    {
        if (!_coordinator.IsModuleReady(ModuleType.AutoSteer))
        {
            return;
        }

        byte[] message = _builder.BuildAutoSteerSettings(pwmDrive, minPwm, proportionalGain, highPwm, lowSpeedPwm);
        _transport.SendMessage(ModuleType.AutoSteer, message);
    }

    /// <inheritdoc/>
    public void SendConfiguration(byte configFlags)
    {
        if (!_coordinator.IsModuleReady(ModuleType.AutoSteer))
        {
            return;
        }

        // Use IMU config builder for now - could add dedicated AutoSteer config PGN
        byte[] message = _builder.BuildImuConfig(configFlags);
        _transport.SendMessage(ModuleType.AutoSteer, message);
    }

    #endregion

    #region State Properties

    /// <inheritdoc/>
    public AutoSteerFeedback? CurrentFeedback
    {
        get
        {
            lock (_lock)
            {
                return _currentFeedback;
            }
        }
    }

    /// <inheritdoc/>
    public double ActualWheelAngle
    {
        get
        {
            lock (_lock)
            {
                return _currentFeedback?.ActualWheelAngle ?? 0.0;
            }
        }
    }

    /// <inheritdoc/>
    public byte[] SwitchStates
    {
        get
        {
            lock (_lock)
            {
                return _currentFeedback?.SwitchStates ?? Array.Empty<byte>();
            }
        }
    }

    #endregion

    #region Events

    /// <inheritdoc/>
    public event EventHandler<AutoSteerFeedbackEventArgs>? FeedbackReceived;

    /// <inheritdoc/>
    public event EventHandler<AutoSteerSwitchStateChangedEventArgs>? SwitchStateChanged;

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles data received from transport layer.
    /// Filters for AutoSteer module data and parses feedback messages.
    /// </summary>
    private void OnTransportDataReceived(object? sender, TransportDataReceivedEventArgs e)
    {
        // Only process AutoSteer module data
        if (e.Module != ModuleType.AutoSteer)
        {
            return;
        }

        // Try to parse as AutoSteer feedback (PGN 253)
        var feedback = _parser.ParseAutoSteerData(e.Data);
        if (feedback != null)
        {
            // Update current feedback (thread-safe)
            lock (_lock)
            {
                _currentFeedback = feedback;

                // Check for switch state changes
                if (!AreSwitchStatesEqual(_previousSwitchStates, feedback.SwitchStates))
                {
                    _previousSwitchStates = feedback.SwitchStates;
                    RaiseSwitchStateChanged(feedback.SwitchStates);
                }
            }

            // Raise feedback event
            RaiseFeedbackReceived(feedback);
        }

        // Could also parse AutoSteer config responses here if needed
        // var configResponse = _parser.ParseAutoSteerConfig(e.Data);
    }

    /// <summary>
    /// Compares two switch state arrays for equality.
    /// </summary>
    private bool AreSwitchStatesEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Raises the FeedbackReceived event in a thread-safe manner.
    /// </summary>
    private void RaiseFeedbackReceived(AutoSteerFeedback feedback)
    {
        FeedbackReceived?.Invoke(this, new AutoSteerFeedbackEventArgs(feedback));
    }

    /// <summary>
    /// Raises the SwitchStateChanged event in a thread-safe manner.
    /// </summary>
    private void RaiseSwitchStateChanged(byte[] switchStates)
    {
        SwitchStateChanged?.Invoke(this, new AutoSteerSwitchStateChangedEventArgs(switchStates));
    }

    #endregion
}
