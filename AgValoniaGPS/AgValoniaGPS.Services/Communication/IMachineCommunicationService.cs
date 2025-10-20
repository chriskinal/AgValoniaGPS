using System;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service interface for communicating with Machine hardware module.
/// Handles sending section states, relay commands, and receiving feedback.
/// </summary>
/// <remarks>
/// This service provides closed-loop control integration with Wave 4 section control services:
/// - Outbound: Sends section states (PGN 239) and configuration (PGN 238)
/// - Inbound: Receives work switch state and section sensors (PGN 234)
/// - Events: Publishes feedback for coverage verification
///
/// All send methods check module ready state before transmitting.
/// Commands sent to non-ready modules are silently dropped with warning log.
///
/// Thread Safety: All properties and methods are thread-safe for concurrent access
/// from section control loop and UI threads.
/// </remarks>
public interface IMachineCommunicationService
{
    #region Commands

    /// <summary>
    /// Send section states to Machine module (PGN 239).
    /// </summary>
    /// <param name="sectionStates">Section states: 0=off, 1=on, 2=auto (one byte per section)</param>
    /// <remarks>
    /// This method is called by SectionControlService when section states change.
    /// Command is only sent if module is in Ready state.
    /// </remarks>
    void SendSectionStates(byte[] sectionStates);

    /// <summary>
    /// Send relay states to Machine module (PGN 239).
    /// Typically bundled with section states in same message.
    /// </summary>
    /// <param name="relayLo">Low 8 relay states (1 bit per relay)</param>
    /// <param name="relayHi">High 8 relay states (1 bit per relay)</param>
    void SendRelayStates(byte[] relayLo, byte[] relayHi);

    /// <summary>
    /// Send Machine configuration (PGN 238).
    /// Configures section count, zones, and implement width.
    /// </summary>
    /// <param name="sections">Number of sections</param>
    /// <param name="zones">Number of zones</param>
    /// <param name="totalWidth">Total implement width in centimeters</param>
    void SendConfiguration(ushort sections, ushort zones, ushort totalWidth);

    #endregion

    #region State Properties

    /// <summary>
    /// Gets the most recent feedback received from the Machine module.
    /// Returns null if no feedback has been received.
    /// </summary>
    MachineFeedback? CurrentFeedback { get; }

    /// <summary>
    /// Gets whether the work switch is active from most recent feedback.
    /// Returns false if no feedback received.
    /// </summary>
    /// <remarks>
    /// SectionControlService uses this to enable/disable section control:
    /// - WorkSwitch pressed (true): Enable auto control
    /// - WorkSwitch released (false): Disable all sections
    /// </remarks>
    bool WorkSwitchActive { get; }

    /// <summary>
    /// Gets the section sensor states from most recent feedback.
    /// Returns empty array if no feedback received.
    /// </summary>
    /// <remarks>
    /// Used to update coverage map with actual section state (not commanded state).
    /// Prevents coverage gaps if section solenoid fails to activate.
    /// </remarks>
    byte[] SectionSensors { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when feedback is received from the Machine module.
    /// Includes work switch state and section sensor feedback.
    /// </summary>
    event EventHandler<MachineFeedbackEventArgs> FeedbackReceived;

    /// <summary>
    /// Raised when work switch state changes.
    /// Used by SectionControlService to enable/disable section control.
    /// </summary>
    event EventHandler<WorkSwitchChangedEventArgs> WorkSwitchChanged;

    #endregion
}
