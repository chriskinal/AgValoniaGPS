using System;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using ImuDataModel = AgValoniaGPS.Models.Communication.ImuData;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for communicating with IMU (Inertial Measurement Unit) hardware module.
/// Implements PGN message sending and data parsing with thread-safe state management.
/// </summary>
public class ImuCommunicationService : IImuCommunicationService
{
    private readonly IModuleCoordinatorService _coordinator;
    private readonly IPgnMessageBuilderService _builder;
    private readonly IPgnMessageParserService _parser;
    private readonly ITransportAbstractionService _transport;
    private readonly object _lock = new object();

    private ImuDataModel? _currentData;
    private bool _previousCalibrationState = false;

    /// <summary>
    /// Creates a new instance of ImuCommunicationService.
    /// </summary>
    public ImuCommunicationService(
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
    public void SendConfiguration(byte configFlags)
    {
        if (!_coordinator.IsModuleReady(ModuleType.IMU))
        {
            return;
        }

        byte[] message = _builder.BuildImuConfig(configFlags);
        _transport.SendMessage(ModuleType.IMU, message);
    }

    /// <inheritdoc/>
    public void RequestCalibration()
    {
        if (!_coordinator.IsModuleReady(ModuleType.IMU))
        {
            return;
        }

        // Send calibration request using config flags
        // Bit 0 = request calibration
        byte calibrationFlag = 0x01;
        byte[] message = _builder.BuildImuConfig(calibrationFlag);
        _transport.SendMessage(ModuleType.IMU, message);
    }

    #endregion

    #region State Properties

    /// <inheritdoc/>
    public ImuDataModel? CurrentData
    {
        get
        {
            lock (_lock)
            {
                return _currentData;
            }
        }
    }

    /// <inheritdoc/>
    public double Roll
    {
        get
        {
            lock (_lock)
            {
                return _currentData?.Roll ?? 0.0;
            }
        }
    }

    /// <inheritdoc/>
    public double Pitch
    {
        get
        {
            lock (_lock)
            {
                return _currentData?.Pitch ?? 0.0;
            }
        }
    }

    /// <inheritdoc/>
    public double Heading
    {
        get
        {
            lock (_lock)
            {
                return _currentData?.Heading ?? 0.0;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsCalibrated
    {
        get
        {
            lock (_lock)
            {
                return _currentData?.IsCalibrated ?? false;
            }
        }
    }

    #endregion

    #region Events

    /// <inheritdoc/>
    public event EventHandler<ImuDataReceivedEventArgs>? DataReceived;

    /// <inheritdoc/>
    public event EventHandler<ImuCalibrationChangedEventArgs>? CalibrationChanged;

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles data received from transport layer.
    /// Filters for IMU module data and parses IMU messages.
    /// </summary>
    private void OnTransportDataReceived(object? sender, TransportDataReceivedEventArgs e)
    {
        // Only process IMU module data
        if (e.Module != ModuleType.IMU)
        {
            return;
        }

        // Try to parse as IMU data (PGN 219)
        var imuData = _parser.ParseImuData(e.Data);
        if (imuData != null)
        {
            // Update current data (thread-safe)
            lock (_lock)
            {
                _currentData = imuData;

                // Check for calibration status change
                if (_previousCalibrationState != imuData.IsCalibrated)
                {
                    _previousCalibrationState = imuData.IsCalibrated;
                    RaiseCalibrationChanged(imuData.IsCalibrated);
                }
            }

            // Raise data received event
            RaiseDataReceived(imuData);
        }
    }

    /// <summary>
    /// Raises the DataReceived event in a thread-safe manner.
    /// </summary>
    private void RaiseDataReceived(ImuDataModel data)
    {
        DataReceived?.Invoke(this, new ImuDataReceivedEventArgs(data));
    }

    /// <summary>
    /// Raises the CalibrationChanged event in a thread-safe manner.
    /// </summary>
    private void RaiseCalibrationChanged(bool isCalibrated)
    {
        CalibrationChanged?.Invoke(this, new ImuCalibrationChangedEventArgs(isCalibrated));
    }

    #endregion
}
