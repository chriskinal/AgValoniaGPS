using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for simulating hardware modules (AutoSteer, Machine, IMU) for testing without physical hardware.
/// Provides realistic behavior simulation including gradual steering response, sensor delays, IMU drift, and GPS jitter.
/// </summary>
/// <remarks>
/// The simulator operates at 10Hz (100ms cycles) sending data for all three modules:
/// - AutoSteer: Receives commands, sends feedback with gradual steering response
/// - Machine: Receives section commands, sends sensor feedback with configurable delay
/// - IMU: Sends roll/pitch/heading data with optional drift and jitter
///
/// Two operation modes:
/// 1. Realistic mode (default): Gradual changes, delays, drift, jitter
/// 2. Instant mode: Immediate response, no delays, no drift/jitter
///
/// Scriptable interface allows automated test scenarios by loading and executing
/// time-based command sequences for repeatable integration testing.
///
/// Thread-safe: All methods can be called concurrently.
/// Performance requirement: <30ms per update cycle for all 3 modules.
/// </remarks>
public interface IHardwareSimulatorService
{
    // ========== Lifecycle Management ==========

    /// <summary>
    /// Starts the simulator and begins 10Hz update loop.
    /// Begins sending hello packets (1Hz) and data packets (10Hz) for all modules.
    /// </summary>
    /// <returns>Task that completes when simulator is started</returns>
    /// <exception cref="InvalidOperationException">Thrown if simulator is already running</exception>
    Task StartAsync();

    /// <summary>
    /// Stops the simulator and terminates update loop.
    /// Stops all module data transmission.
    /// </summary>
    /// <returns>Task that completes when simulator is stopped</returns>
    Task StopAsync();

    /// <summary>
    /// Gets whether the simulator is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Enables or disables realistic behavior mode.
    /// </summary>
    /// <param name="enabled">True for realistic behavior (gradual changes, delays, drift), false for instant response</param>
    /// <remarks>
    /// Realistic mode ON:
    /// - Steering: Gradual angle change based on SetSteeringResponseTime
    /// - Sections: Sensor feedback delayed by configurable amount
    /// - IMU: Roll/pitch drift at configurable rate, GPS jitter on heading
    ///
    /// Realistic mode OFF:
    /// - Steering: Instant angle matching commanded value
    /// - Sections: Immediate sensor feedback matching commanded state
    /// - IMU: Constant values, no drift or jitter
    /// </remarks>
    void EnableRealisticBehavior(bool enabled);

    /// <summary>
    /// Gets whether realistic behavior mode is enabled.
    /// </summary>
    bool RealisticBehaviorEnabled { get; }

    // ========== Manual Value Control ==========

    /// <summary>
    /// Manually sets the steering angle reported in AutoSteer feedback.
    /// In realistic mode, actual angle will approach this value gradually.
    /// In instant mode, actual angle changes immediately.
    /// </summary>
    /// <param name="angle">Steering angle in degrees (positive = right)</param>
    void SetSteeringAngle(double angle);

    /// <summary>
    /// Manually sets a section sensor state in Machine feedback.
    /// In realistic mode, sensor state changes after configurable delay.
    /// In instant mode, sensor state changes immediately.
    /// </summary>
    /// <param name="section">Section index (0-based)</param>
    /// <param name="active">True if section sensor is active, false otherwise</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if section index is invalid</exception>
    void SetSectionSensor(int section, bool active);

    /// <summary>
    /// Manually sets the IMU roll value.
    /// In realistic mode, drift will be applied on top of this value.
    /// In instant mode, exact value is reported.
    /// </summary>
    /// <param name="roll">Roll angle in degrees</param>
    void SetImuRoll(double roll);

    /// <summary>
    /// Manually sets the IMU pitch value.
    /// In realistic mode, drift will be applied on top of this value.
    /// In instant mode, exact value is reported.
    /// </summary>
    /// <param name="pitch">Pitch angle in degrees</param>
    void SetImuPitch(double pitch);

    /// <summary>
    /// Manually sets the work switch state in Machine feedback.
    /// </summary>
    /// <param name="active">True if work switch is active, false otherwise</param>
    void SetWorkSwitch(bool active);

    // ========== Scripted Behaviors ==========

    /// <summary>
    /// Loads a script file containing timed command sequences.
    /// Script format: JSON or text with timestamp, command type, and parameters.
    /// Example: "5.0s: SetSteeringAngle(10.0)"
    /// </summary>
    /// <param name="scriptPath">Path to script file</param>
    /// <exception cref="ArgumentNullException">Thrown if scriptPath is null</exception>
    /// <exception cref="System.IO.FileNotFoundException">Thrown if script file not found</exception>
    /// <exception cref="FormatException">Thrown if script format is invalid</exception>
    Task LoadScriptAsync(string scriptPath);

    /// <summary>
    /// Executes the currently loaded script.
    /// Commands are executed at their specified timestamps relative to execution start.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no script is loaded or simulator is not running</exception>
    Task ExecuteScriptAsync();

    /// <summary>
    /// Stops script execution.
    /// </summary>
    void StopScript();

    /// <summary>
    /// Gets whether a script is currently executing.
    /// </summary>
    bool IsScriptRunning { get; }

    // ========== Realism Configuration ==========

    /// <summary>
    /// Sets the steering response time (time to reach target angle from 0).
    /// Only applies when realistic behavior is enabled.
    /// </summary>
    /// <param name="seconds">Response time in seconds (default: 0.5)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if seconds is negative</exception>
    void SetSteeringResponseTime(double seconds);

    /// <summary>
    /// Sets the section sensor feedback delay.
    /// Only applies when realistic behavior is enabled.
    /// </summary>
    /// <param name="milliseconds">Delay in milliseconds (default: 50ms)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if milliseconds is negative</exception>
    void SetSectionSensorDelay(double milliseconds);

    /// <summary>
    /// Sets the IMU drift rate for roll and pitch.
    /// Only applies when realistic behavior is enabled.
    /// </summary>
    /// <param name="degreesPerMinute">Drift rate in degrees per minute (default: 0.1)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if degreesPerMinute is negative</exception>
    void SetImuDriftRate(double degreesPerMinute);

    /// <summary>
    /// Sets the GPS jitter magnitude added to heading.
    /// Only applies when realistic behavior is enabled.
    /// </summary>
    /// <param name="meters">Jitter magnitude in meters (default: 0.02m = 2cm)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if meters is negative</exception>
    void SetGpsJitterMagnitude(double meters);

    // ========== Events ==========

    /// <summary>
    /// Raised when simulator state changes (started, stopped, realistic mode toggled).
    /// </summary>
    event EventHandler<SimulatorStateChangedEventArgs>? StateChanged;
}
