using System;
using AgValoniaGPS.Models.Events;
using ImuDataModel = AgValoniaGPS.Models.Communication.ImuData;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service interface for communicating with IMU (Inertial Measurement Unit) hardware module.
/// Handles sending configuration commands and receiving orientation data.
/// </summary>
/// <remarks>
/// This service provides IMU data for vehicle orientation and pitch/roll compensation:
/// - Outbound: Sends configuration (PGN 218) and calibration requests
/// - Inbound: Receives roll, pitch, heading, yaw rate data (PGN 219)
/// - Events: Publishes IMU data updates and calibration status changes
///
/// All send methods check module ready state before transmitting.
/// Commands sent to non-ready modules are silently dropped with warning log.
///
/// Thread Safety: All properties and methods are thread-safe for concurrent access
/// from multiple threads.
/// </remarks>
public interface IImuCommunicationService
{
    #region Commands

    /// <summary>
    /// Send IMU configuration to module (PGN 218).
    /// Configures IMU settings and calibration parameters.
    /// </summary>
    /// <param name="configFlags">Configuration bitmap for IMU settings</param>
    void SendConfiguration(byte configFlags);

    /// <summary>
    /// Request IMU calibration.
    /// Triggers calibration sequence on the IMU module.
    /// </summary>
    void RequestCalibration();

    #endregion

    #region State Properties

    /// <summary>
    /// Gets the most recent IMU data received from the module.
    /// Returns null if no data has been received.
    /// </summary>
    ImuDataModel? CurrentData { get; }

    /// <summary>
    /// Gets the roll angle in degrees from most recent data.
    /// Returns 0.0 if no data received.
    /// </summary>
    /// <remarks>
    /// Positive = right side down, Negative = left side down.
    /// Used for antenna height correction and implement leveling.
    /// </remarks>
    double Roll { get; }

    /// <summary>
    /// Gets the pitch angle in degrees from most recent data.
    /// Returns 0.0 if no data received.
    /// </summary>
    /// <remarks>
    /// Positive = front up, Negative = front down.
    /// Used for antenna height correction.
    /// </remarks>
    double Pitch { get; }

    /// <summary>
    /// Gets the heading angle in degrees (0-360) from most recent data.
    /// Returns 0.0 if no data received.
    /// </summary>
    /// <remarks>
    /// 0 = North, 90 = East, 180 = South, 270 = West.
    /// Can be used as alternative to GPS-derived heading at low speeds.
    /// </remarks>
    double Heading { get; }

    /// <summary>
    /// Gets whether the IMU is calibrated from most recent data.
    /// Returns false if no data received.
    /// </summary>
    /// <remarks>
    /// When false, IMU data may be unreliable and should not be used
    /// for critical calculations.
    /// </remarks>
    bool IsCalibrated { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when IMU data is received from the module.
    /// Includes roll, pitch, heading, yaw rate, and calibration status.
    /// </summary>
    event EventHandler<ImuDataReceivedEventArgs> DataReceived;

    /// <summary>
    /// Raised when IMU calibration status changes.
    /// Used to notify user when calibration is complete or lost.
    /// </summary>
    event EventHandler<ImuCalibrationChangedEventArgs> CalibrationChanged;

    #endregion
}
