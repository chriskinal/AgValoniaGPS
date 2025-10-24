using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.ViewModels.Base;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.Configuration;

/// <summary>
/// ViewModel for IMU roll correction configuration panel.
/// Provides UI for calibrating and configuring roll compensation.
/// </summary>
public class FormRollCorrectionViewModel : PanelViewModelBase
{
    private readonly VehicleConfiguration _vehicleConfig;
    private readonly IConfigurationService _configService;

    private bool _rollCorrectionEnabled = true;
    private double _rollZeroOffset = 0.0;
    private double _rollFilterConstant = 0.5;
    private double _currentRollAngle = 0.0;
    private double _antennaHeight = 3.0;
    private double _antennaOffset = 0.0;

    public FormRollCorrectionViewModel(
        VehicleConfiguration vehicleConfig,
        IConfigurationService configService)
    {
        _vehicleConfig = vehicleConfig ?? throw new ArgumentNullException(nameof(vehicleConfig));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        Title = "Roll Correction";

        // Commands
        ZeroRollCommand = new RelayCommand(OnZeroRoll);
        ApplySettingsCommand = new RelayCommand(OnApplySettings);
        ResetCommand = new RelayCommand(OnReset);

        // Load current settings
        LoadCurrentSettings();
    }

    public string Title { get; } = "Roll Correction";

    /// <summary>
    /// Enable or disable roll correction
    /// </summary>
    public bool RollCorrectionEnabled
    {
        get => _rollCorrectionEnabled;
        set => SetProperty(ref _rollCorrectionEnabled, value);
    }

    /// <summary>
    /// Roll zero offset in degrees (-10 to +10)
    /// </summary>
    public double RollZeroOffset
    {
        get => _rollZeroOffset;
        set => SetProperty(ref _rollZeroOffset, Math.Clamp(value, -10.0, 10.0));
    }

    /// <summary>
    /// Roll filter constant (0.0-1.0) for low-pass filtering
    /// </summary>
    public double RollFilterConstant
    {
        get => _rollFilterConstant;
        set => SetProperty(ref _rollFilterConstant, Math.Clamp(value, 0.0, 1.0));
    }

    /// <summary>
    /// Current roll angle in degrees (read-only)
    /// </summary>
    public double CurrentRollAngle
    {
        get => _currentRollAngle;
        set => SetProperty(ref _currentRollAngle, value);
    }

    /// <summary>
    /// Antenna height in meters
    /// </summary>
    public double AntennaHeight
    {
        get => _antennaHeight;
        set => SetProperty(ref _antennaHeight, value);
    }

    /// <summary>
    /// Antenna lateral offset in meters
    /// </summary>
    public double AntennaOffset
    {
        get => _antennaOffset;
        set => SetProperty(ref _antennaOffset, value);
    }

    public ICommand ZeroRollCommand { get; }
    public ICommand ApplySettingsCommand { get; }
    public ICommand ResetCommand { get; }

    private void OnZeroRoll()
    {
        try
        {
            // Set current roll angle as zero offset
            RollZeroOffset = -CurrentRollAngle;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to zero roll: {ex.Message}");
        }
    }

    private async void OnApplySettings()
    {
        try
        {
            IsBusy = true;

            // Update vehicle configuration
            _vehicleConfig.AntennaHeight = AntennaHeight;
            _vehicleConfig.AntennaOffset = AntennaOffset;

            // Update IMU settings via configuration service
            var imuSettings = _configService.GetImuSettings();
            // imuSettings.RollZeroOffset = RollZeroOffset;
            // imuSettings.RollFilterConstant = RollFilterConstant;

            await _configService.UpdateImuSettingsAsync(imuSettings);

            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to apply settings: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnReset()
    {
        RollCorrectionEnabled = true;
        RollZeroOffset = 0.0;
        RollFilterConstant = 0.5;
        ClearError();
    }

    private void LoadCurrentSettings()
    {
        // Load from vehicle configuration
        AntennaHeight = _vehicleConfig.AntennaHeight;
        AntennaOffset = _vehicleConfig.AntennaOffset;

        // Load IMU settings
        var imuSettings = _configService.GetImuSettings();
        // RollZeroOffset = imuSettings.RollZeroOffset;
        // RollFilterConstant = imuSettings.RollFilterConstant;

        // TODO: Subscribe to IMU data events to update CurrentRollAngle
    }
}
