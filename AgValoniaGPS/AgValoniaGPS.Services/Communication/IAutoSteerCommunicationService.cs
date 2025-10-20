using System;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service interface for communicating with AutoSteer hardware module.
/// Handles sending steering commands, settings, and receiving feedback.
/// </summary>
/// <remarks>
/// This service provides closed-loop control integration with Wave 3 steering services:
/// - Outbound: Sends steering commands (PGN 254) and settings (PGN 252)
/// - Inbound: Receives wheel angle feedback and switch states (PGN 253)
/// - Events: Publishes feedback for closed-loop error tracking
///
/// All send methods check module ready state before transmitting.
/// Commands sent to non-ready modules are silently dropped with warning log.
///
/// Thread Safety: All properties and methods are thread-safe for concurrent access
/// from guidance loop and UI threads.
/// </remarks>
public interface IAutoSteerCommunicationService
{
    #region Commands

    /// <summary>
    /// Send steering command to AutoSteer module (PGN 254).
    /// </summary>
    /// <param name="speedMph">Vehicle speed in miles per hour</param>
    /// <param name="steerAngle">Desired steering angle in degrees (positive = right)</param>
    /// <param name="xteErrorMm">Cross-track error in millimeters</param>
    /// <param name="isActive">Whether auto-steer is active (true) or off (false)</param>
    /// <remarks>
    /// This method is called by SteeringCoordinatorService after calculating desired angle.
    /// Command is only sent if module is in Ready state.
    /// </remarks>
    void SendSteeringCommand(double speedMph, double steerAngle, int xteErrorMm, bool isActive);

    /// <summary>
    /// Send AutoSteer settings to module (PGN 252).
    /// Configures PWM and PID parameters.
    /// </summary>
    /// <param name="pwmDrive">PWM drive level (0-255)</param>
    /// <param name="minPwm">Minimum PWM threshold (0-255)</param>
    /// <param name="proportionalGain">Proportional gain for PID control</param>
    /// <param name="highPwm">High PWM limit (0-255)</param>
    /// <param name="lowSpeedPwm">Low speed PWM multiplier</param>
    void SendSettings(byte pwmDrive, byte minPwm, float proportionalGain, byte highPwm, float lowSpeedPwm);

    /// <summary>
    /// Send configuration to AutoSteer module.
    /// Used for capability negotiation and advanced settings.
    /// </summary>
    /// <param name="configFlags">Configuration bitmap</param>
    void SendConfiguration(byte configFlags);

    #endregion

    #region State Properties

    /// <summary>
    /// Gets the most recent feedback received from the AutoSteer module.
    /// Returns null if no feedback has been received.
    /// </summary>
    AutoSteerFeedback? CurrentFeedback { get; }

    /// <summary>
    /// Gets the actual wheel angle in degrees from most recent feedback.
    /// Returns 0.0 if no feedback received.
    /// </summary>
    /// <remarks>
    /// This is used by SteeringCoordinatorService for closed-loop error tracking:
    /// error = CurrentSteeringAngle - ActualWheelAngle
    /// </remarks>
    double ActualWheelAngle { get; }

    /// <summary>
    /// Gets the switch states bitmap from most recent feedback.
    /// Returns empty array if no feedback received.
    /// </summary>
    byte[] SwitchStates { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when feedback is received from the AutoSteer module.
    /// Includes actual wheel angle and switch states for closed-loop control.
    /// </summary>
    event EventHandler<AutoSteerFeedbackEventArgs> FeedbackReceived;

    /// <summary>
    /// Raised when switch state changes on the AutoSteer module.
    /// Used for detecting remote engage/disengage commands.
    /// </summary>
    event EventHandler<AutoSteerSwitchStateChangedEventArgs> SwitchStateChanged;

    #endregion
}
