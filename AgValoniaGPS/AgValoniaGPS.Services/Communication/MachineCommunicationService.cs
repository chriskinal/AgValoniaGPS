using System;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for communicating with Machine hardware module.
/// Implements PGN message sending and feedback parsing with thread-safe state management.
/// </summary>
public class MachineCommunicationService : IMachineCommunicationService
{
    private readonly IModuleCoordinatorService _coordinator;
    private readonly IPgnMessageBuilderService _builder;
    private readonly IPgnMessageParserService _parser;
    private readonly ITransportAbstractionService _transport;
    private readonly object _lock = new object();

    private MachineFeedback? _currentFeedback;
    private bool _previousWorkSwitchState = false;

    /// <summary>
    /// Creates a new instance of MachineCommunicationService.
    /// </summary>
    public MachineCommunicationService(
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
    public void SendSectionStates(byte[] sectionStates)
    {
        if (sectionStates == null)
        {
            throw new ArgumentNullException(nameof(sectionStates));
        }

        if (!_coordinator.IsModuleReady(ModuleType.Machine))
        {
            return;
        }

        // Build PGN 239 with default relay states and no tram line
        byte[] relayLo = new byte[8]; // All off
        byte[] relayHi = new byte[8]; // All off
        byte tramLine = 0;

        byte[] message = _builder.BuildMachineData(relayLo, relayHi, tramLine, sectionStates);
        _transport.SendMessage(ModuleType.Machine, message);
    }

    /// <inheritdoc/>
    public void SendRelayStates(byte[] relayLo, byte[] relayHi)
    {
        if (relayLo == null)
        {
            throw new ArgumentNullException(nameof(relayLo));
        }

        if (relayHi == null)
        {
            throw new ArgumentNullException(nameof(relayHi));
        }

        if (!_coordinator.IsModuleReady(ModuleType.Machine))
        {
            return;
        }

        // Build PGN 239 with empty section states
        byte[] sectionStates = Array.Empty<byte>();
        byte tramLine = 0;

        byte[] message = _builder.BuildMachineData(relayLo, relayHi, tramLine, sectionStates);
        _transport.SendMessage(ModuleType.Machine, message);
    }

    /// <inheritdoc/>
    public void SendConfiguration(ushort sections, ushort zones, ushort totalWidth)
    {
        if (!_coordinator.IsModuleReady(ModuleType.Machine))
        {
            return;
        }

        byte[] message = _builder.BuildMachineConfig(sections, zones, totalWidth);
        _transport.SendMessage(ModuleType.Machine, message);
    }

    #endregion

    #region State Properties

    /// <inheritdoc/>
    public MachineFeedback? CurrentFeedback
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
    public bool WorkSwitchActive
    {
        get
        {
            lock (_lock)
            {
                return _currentFeedback?.WorkSwitchActive ?? false;
            }
        }
    }

    /// <inheritdoc/>
    public byte[] SectionSensors
    {
        get
        {
            lock (_lock)
            {
                return _currentFeedback?.SectionSensors ?? Array.Empty<byte>();
            }
        }
    }

    #endregion

    #region Events

    /// <inheritdoc/>
    public event EventHandler<MachineFeedbackEventArgs>? FeedbackReceived;

    /// <inheritdoc/>
    public event EventHandler<WorkSwitchChangedEventArgs>? WorkSwitchChanged;

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles data received from transport layer.
    /// Filters for Machine module data and parses feedback messages.
    /// </summary>
    private void OnTransportDataReceived(object? sender, TransportDataReceivedEventArgs e)
    {
        // Only process Machine module data
        if (e.Module != ModuleType.Machine)
        {
            return;
        }

        // Try to parse as Machine feedback (PGN 234)
        var feedback = _parser.ParseMachineData(e.Data);
        if (feedback != null)
        {
            // Update current feedback (thread-safe)
            lock (_lock)
            {
                _currentFeedback = feedback;

                // Check for work switch state change
                if (_previousWorkSwitchState != feedback.WorkSwitchActive)
                {
                    _previousWorkSwitchState = feedback.WorkSwitchActive;
                    RaiseWorkSwitchChanged(feedback.WorkSwitchActive);
                }
            }

            // Raise feedback event
            RaiseFeedbackReceived(feedback);
        }

        // Could also parse Machine config responses here if needed
        // var configResponse = _parser.ParseMachineConfig(e.Data);
    }

    /// <summary>
    /// Raises the FeedbackReceived event in a thread-safe manner.
    /// </summary>
    private void RaiseFeedbackReceived(MachineFeedback feedback)
    {
        FeedbackReceived?.Invoke(this, new MachineFeedbackEventArgs(feedback));
    }

    /// <summary>
    /// Raises the WorkSwitchChanged event in a thread-safe manner.
    /// </summary>
    private void RaiseWorkSwitchChanged(bool isActive)
    {
        WorkSwitchChanged?.Invoke(this, new WorkSwitchChangedEventArgs(isActive));
    }

    #endregion
}
