using System;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.GPS;
using Microsoft.Extensions.Logging;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Implements the section control finite state machine with timer management,
/// boundary checking, manual override handling, and Machine hardware integration.
/// Integrates with MachineCommunicationService for closed-loop section control.
/// </summary>
/// <remarks>
/// Thread-safe implementation using lock object pattern.
/// Performance optimized for &lt;5ms execution time for all 31 sections.
/// </remarks>
public class SectionControlService : ISectionControlService
{
    private readonly object _lockObject = new object();
    private readonly ISectionSpeedService _speedService;
    private readonly IMachineCommunicationService _machineComm;
    private readonly IAnalogSwitchStateService _switchService;
    private readonly ISectionConfigurationService _configService;
    private readonly IPositionUpdateService _positionService;
    private readonly ILogger<SectionControlService>? _logger;

    private Models.Section.Section[] _sections = Array.Empty<Models.Section.Section>();
    private SectionConfiguration _config = new SectionConfiguration();

    private const double SectionSensorMismatchWarningTimeMs = 500;

    public event EventHandler<SectionStateChangedEventArgs>? SectionStateChanged;

    /// <summary>
    /// Creates a new SectionControlService with required dependencies and Wave 6 Machine integration.
    /// </summary>
    /// <param name="speedService">Section speed service</param>
    /// <param name="machineComm">Machine hardware communication service (Wave 6)</param>
    /// <param name="switchService">Analog switch state service</param>
    /// <param name="configService">Section configuration service</param>
    /// <param name="positionService">Position update service</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    public SectionControlService(
        ISectionSpeedService speedService,
        IMachineCommunicationService machineComm,
        IAnalogSwitchStateService switchService,
        ISectionConfigurationService configService,
        IPositionUpdateService positionService,
        ILogger<SectionControlService>? logger = null)
    {
        _speedService = speedService ?? throw new ArgumentNullException(nameof(speedService));
        _machineComm = machineComm ?? throw new ArgumentNullException(nameof(machineComm));
        _switchService = switchService ?? throw new ArgumentNullException(nameof(switchService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        _logger = logger;

        // Subscribe to configuration changes
        _configService.ConfigurationChanged += OnConfigurationChanged;

        // Subscribe to switch state changes
        _switchService.SwitchStateChanged += OnSwitchStateChanged;

        // Subscribe to Machine feedback for closed-loop control
        _machineComm.FeedbackReceived += OnMachineFeedbackReceived;
        _machineComm.WorkSwitchChanged += OnWorkSwitchChanged;

        // Initialize sections from current configuration
        InitializeSections();
    }

    public void UpdateSectionStates(GeoCoord position, double heading, double speed)
    {
        lock (_lockObject)
        {
            // Check work switch - if inactive, turn off all sections immediately
            var workSwitchState = _switchService.GetSwitchState(AnalogSwitchType.WorkSwitch);
            if (workSwitchState == SwitchState.Inactive)
            {
                TurnOffAllSections(immediate: true);
                return;
            }

            // Check if reversing - immediate turn-off (no delay)
            bool isReversing = _positionService.IsReversing();
            if (isReversing)
            {
                TurnOffAllSections(immediate: true);
                return;
            }

            // Check minimum speed threshold
            bool isBelowMinSpeed = speed < _config.MinimumSpeed;
            if (isBelowMinSpeed)
            {
                TurnOffAllSections(immediate: false); // Use normal delay
                return;
            }

            // Update each section based on its state and conditions
            for (int i = 0; i < _sections.Length; i++)
            {
                UpdateSectionState(i, position, heading, speed);
            }

            // Send section states to Machine module via Wave 6 communication service
            SendSectionStatesToMachine();
        }
    }

    public void SetManualOverride(int sectionId, SectionState state)
    {
        lock (_lockObject)
        {
            ValidateSectionId(sectionId);

            if (state != SectionState.ManualOn && state != SectionState.ManualOff && state != SectionState.Auto)
                throw new ArgumentException(
                    "Manual override state must be ManualOn, ManualOff, or Auto", nameof(state));

            var section = _sections[sectionId];
            var oldState = section.State;

            if (state == SectionState.Auto)
            {
                // Release manual override
                section.IsManualOverride = false;
                section.State = SectionState.Auto;
                RaiseSectionStateChanged(sectionId, oldState, SectionState.Auto, SectionStateChangeType.ToAuto);
            }
            else
            {
                // Set manual override
                section.IsManualOverride = true;
                section.State = state;

                // Cancel any pending timers
                section.TurnOnTimerStart = null;
                section.TurnOffTimerStart = null;

                var changeType = state == SectionState.ManualOn
                    ? SectionStateChangeType.ToManualOn
                    : SectionStateChangeType.ToManualOff;

                RaiseSectionStateChanged(sectionId, oldState, state, changeType);
            }

            // Send updated states to Machine
            SendSectionStatesToMachine();
        }
    }

    public SectionState GetSectionState(int sectionId)
    {
        lock (_lockObject)
        {
            ValidateSectionId(sectionId);
            return _sections[sectionId].State;
        }
    }

    public SectionState[] GetAllSectionStates()
    {
        lock (_lockObject)
        {
            var states = new SectionState[_sections.Length];
            for (int i = 0; i < _sections.Length; i++)
            {
                states[i] = _sections[i].State;
            }
            return states;
        }
    }

    public bool IsManualOverride(int sectionId)
    {
        lock (_lockObject)
        {
            ValidateSectionId(sectionId);
            return _sections[sectionId].IsManualOverride;
        }
    }

    public void ResetAllSections()
    {
        lock (_lockObject)
        {
            for (int i = 0; i < _sections.Length; i++)
            {
                var oldState = _sections[i].State;
                _sections[i].State = SectionState.Auto;
                _sections[i].IsManualOverride = false;
                _sections[i].TurnOnTimerStart = null;
                _sections[i].TurnOffTimerStart = null;

                if (oldState != SectionState.Auto)
                {
                    RaiseSectionStateChanged(i, oldState, SectionState.Auto, SectionStateChangeType.ToAuto);
                }
            }

            // Send updated states to Machine
            SendSectionStatesToMachine();
        }
    }

    private void UpdateSectionState(int sectionId, GeoCoord position, double heading, double speed)
    {
        var section = _sections[sectionId];

        // Manual override takes precedence - skip automatic control
        if (section.IsManualOverride)
            return;

        // Get section speed from speed service
        double sectionSpeed = _speedService.GetSectionSpeed(sectionId);

        // Check if section speed is below threshold (tight inside turn)
        bool speedBelowThreshold = sectionSpeed < _config.MinimumSpeed;

        // Determine desired state based on conditions
        SectionState desiredState = speedBelowThreshold ? SectionState.Off : SectionState.Auto;

        // Handle state transitions with timers
        if (section.State == SectionState.Auto && desiredState == SectionState.Off)
        {
            // Should turn off - start turn-off timer if not already started
            if (section.TurnOffTimerStart == null)
            {
                section.TurnOffTimerStart = DateTime.UtcNow;
                RaiseSectionStateChanged(sectionId, section.State, section.State, SectionStateChangeType.TimerStarted);
            }
            else
            {
                // Check if timer has expired
                var elapsed = (DateTime.UtcNow - section.TurnOffTimerStart.Value).TotalSeconds;
                if (elapsed >= _config.TurnOffDelay)
                {
                    // Turn off section
                    var oldState = section.State;
                    section.State = SectionState.Off;
                    section.TurnOffTimerStart = null;
                    RaiseSectionStateChanged(sectionId, oldState, SectionState.Off, SectionStateChangeType.ToOff);
                }
            }
        }
        else if (section.State == SectionState.Off && desiredState == SectionState.Auto)
        {
            // Should turn on - start turn-on timer if not already started
            if (section.TurnOnTimerStart == null)
            {
                section.TurnOnTimerStart = DateTime.UtcNow;
                RaiseSectionStateChanged(sectionId, section.State, section.State, SectionStateChangeType.TimerStarted);
            }
            else
            {
                // Check if timer has expired
                var elapsed = (DateTime.UtcNow - section.TurnOnTimerStart.Value).TotalSeconds;
                if (elapsed >= _config.TurnOnDelay)
                {
                    // Turn on section
                    var oldState = section.State;
                    section.State = SectionState.Auto;
                    section.TurnOnTimerStart = null;
                    RaiseSectionStateChanged(sectionId, oldState, SectionState.Auto, SectionStateChangeType.ToAuto);
                }
            }
        }
        else
        {
            // Conditions match current state - cancel any pending timers
            if (section.TurnOnTimerStart != null || section.TurnOffTimerStart != null)
            {
                section.TurnOnTimerStart = null;
                section.TurnOffTimerStart = null;
                RaiseSectionStateChanged(sectionId, section.State, section.State, SectionStateChangeType.TimerCancelled);
            }
        }
    }

    private void TurnOffAllSections(bool immediate)
    {
        for (int i = 0; i < _sections.Length; i++)
        {
            var section = _sections[i];

            // Manual override sections are not affected
            if (section.IsManualOverride)
                continue;

            if (section.State == SectionState.Auto || section.State == SectionState.Off)
            {
                if (immediate)
                {
                    // Immediate turn-off (reversing, work switch off)
                    if (section.State != SectionState.Off)
                    {
                        var oldState = section.State;
                        section.State = SectionState.Off;
                        section.TurnOnTimerStart = null;
                        section.TurnOffTimerStart = null;
                        RaiseSectionStateChanged(i, oldState, SectionState.Off, SectionStateChangeType.ToOff);
                    }
                }
                else
                {
                    // Normal turn-off with delay
                    if (section.State == SectionState.Auto)
                    {
                        if (section.TurnOffTimerStart == null)
                        {
                            section.TurnOffTimerStart = DateTime.UtcNow;
                            RaiseSectionStateChanged(i, section.State, section.State, SectionStateChangeType.TimerStarted);
                        }
                        else
                        {
                            var elapsed = (DateTime.UtcNow - section.TurnOffTimerStart.Value).TotalSeconds;
                            if (elapsed >= _config.TurnOffDelay)
                            {
                                var oldState = section.State;
                                section.State = SectionState.Off;
                                section.TurnOffTimerStart = null;
                                RaiseSectionStateChanged(i, oldState, SectionState.Off, SectionStateChangeType.ToOff);
                            }
                        }
                    }
                }
            }
        }

        // Send updated states to Machine
        SendSectionStatesToMachine();
    }

    /// <summary>
    /// Sends current section states to Machine module via MachineCommunicationService.
    /// Converts section state enum to byte array format expected by Machine PGN.
    /// </summary>
    private void SendSectionStatesToMachine()
    {
        byte[] sectionStates = new byte[_sections.Length];
        for (int i = 0; i < _sections.Length; i++)
        {
            sectionStates[i] = _sections[i].State switch
            {
                SectionState.Off => 0,
                SectionState.Auto => 1,
                SectionState.ManualOn => 1,
                SectionState.ManualOff => 0,
                _ => 0
            };
        }

        // Send via Wave 6 Machine communication service
        _machineComm.SendSectionStates(sectionStates);
    }

    /// <summary>
    /// Handles Machine feedback for closed-loop section control.
    /// Compares commanded vs actual section sensor states and logs mismatches.
    /// </summary>
    private void OnMachineFeedbackReceived(object? sender, MachineFeedbackEventArgs e)
    {
        lock (_lockObject)
        {
            // Update actual section sensor states for coverage mapping
            var sectionSensors = e.Feedback.SectionSensors;

            for (int i = 0; i < Math.Min(_sections.Length, sectionSensors.Length); i++)
            {
                bool commandedOn = _sections[i].State == SectionState.Auto || _sections[i].State == SectionState.ManualOn;
                bool actuallyOn = (sectionSensors[i] & 0x01) != 0;

                // Store actual state for coverage mapping
                _sections[i].ActualState = actuallyOn;

                // Log warning if commanded != actual for extended period
                if (commandedOn != actuallyOn)
                {
                    // TODO: Track mismatch duration and warn if > 500ms
                    _logger?.LogDebug(
                        "Section {SectionId} state mismatch: Commanded={Commanded}, Actual={Actual}",
                        i,
                        commandedOn,
                        actuallyOn);
                }
            }
        }
    }

    /// <summary>
    /// Handles work switch state changes from Machine module.
    /// Automatically disables all sections when work switch is released.
    /// </summary>
    private void OnWorkSwitchChanged(object? sender, WorkSwitchChangedEventArgs e)
    {
        lock (_lockObject)
        {
            if (!e.IsActive)
            {
                // Work switch released - turn off all sections immediately
                _logger?.LogInformation("Work switch released - disabling all sections");
                TurnOffAllSections(immediate: true);
            }
            else
            {
                // Work switch pressed - resume auto control
                _logger?.LogInformation("Work switch activated - resuming auto control");
                // Sections will be turned on automatically in next UpdateSectionStates() call
            }
        }
    }

    private void OnConfigurationChanged(object? sender, EventArgs e)
    {
        InitializeSections();
    }

    private void OnSwitchStateChanged(object? sender, SwitchStateChangedEventArgs e)
    {
        // Work switch state changes are handled in UpdateSectionStates
        // This handler is for future extensions
    }

    private void InitializeSections()
    {
        lock (_lockObject)
        {
            _config = _configService.GetConfiguration();
            int sectionCount = _config.SectionCount;

            // Preserve existing section states if resizing
            var oldSections = _sections;
            _sections = new Models.Section.Section[sectionCount];

            for (int i = 0; i < sectionCount; i++)
            {
                if (i < oldSections.Length)
                {
                    // Preserve existing section
                    _sections[i] = oldSections[i];
                    _sections[i].Width = _config.SectionWidths[i];
                }
                else
                {
                    // Create new section
                    _sections[i] = new Models.Section.Section(i, _config.SectionWidths[i]);
                }

                // Update lateral offset from config service
                _sections[i].LateralOffset = _configService.GetSectionOffset(i);
            }
        }
    }

    private void ValidateSectionId(int sectionId)
    {
        if (sectionId < 0 || sectionId >= _sections.Length)
            throw new ArgumentOutOfRangeException(nameof(sectionId),
                $"Section ID must be between 0 and {_sections.Length - 1}");
    }

    private void RaiseSectionStateChanged(int sectionId, SectionState oldState, SectionState newState, SectionStateChangeType changeType)
    {
        SectionStateChanged?.Invoke(this, new SectionStateChangedEventArgs(sectionId, oldState, newState, changeType));
    }
}
