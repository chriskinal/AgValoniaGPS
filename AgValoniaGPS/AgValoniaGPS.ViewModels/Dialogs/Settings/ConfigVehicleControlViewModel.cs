using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Settings;

/// <summary>
/// ViewModel for ConfigVehicleControl providing complete vehicle configuration with all parameters.
/// Organized in 5 tabs: Vehicle, Steering, GPS, Implement, and Advanced.
/// </summary>
public class ConfigVehicleControlViewModel : DialogViewModelBase
{
    private readonly IConfigurationService? _configService;

    // Vehicle Dimensions
    private string _vehicleName = "Default Vehicle";
    private double _vehicleWidth = 200.0;
    private double _vehicleWheelbase = 180.0;
    private double _vehicleTrackWidth = 150.0;
    private VehicleType _vehicleType = VehicleType.Tractor;

    // Steering Parameters
    private double _maxSteerAngle = 45.0;
    private double _minSteerAngle = -45.0;
    private double _ackermannPercentage = 100.0;
    private double _steeringDeadband = 0.5;

    // GPS Antenna
    private double _antennaHeight = 50.0;
    private double _antennaOffset = 0.0;
    private double _antennaForwardOffset = 25.0;

    // Implement
    private double _implementWidth = 300.0;
    private double _implementOffset = 0.0;
    private int _numberOfSections = 5;
    private bool _isTrailing = true;

    // Look-Ahead
    private double _minLookAhead = 3.0;
    private double _maxLookAhead = 10.0;
    private double _lookAheadSpeedGain = 1.5;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigVehicleControlViewModel"/> class.
    /// </summary>
    /// <param name="configService">Optional configuration service for dependency injection.</param>
    public ConfigVehicleControlViewModel(IConfigurationService? configService = null)
    {
        _configService = configService;

        // Initialize vehicle type collection
        VehicleTypes = new ObservableCollection<VehicleType>(Enum.GetValues<VehicleType>());

        // Initialize commands
        SaveConfigCommand = new RelayCommand(OnSaveConfig);
        ResetCommand = new RelayCommand(OnReset);
        LoadPresetCommand = new RelayCommand(OnLoadPreset);

        // Load current configuration
        LoadConfiguration();
    }

    #region Vehicle Dimensions Properties

    /// <summary>
    /// Gets or sets the vehicle profile name.
    /// </summary>
    public string VehicleName
    {
        get => _vehicleName;
        set => SetProperty(ref _vehicleName, value);
    }

    /// <summary>
    /// Gets or sets the total vehicle width including implement (0.5-30m).
    /// </summary>
    public double VehicleWidth
    {
        get => _vehicleWidth;
        set => SetProperty(ref _vehicleWidth, value);
    }

    /// <summary>
    /// Gets or sets the vehicle wheelbase - distance between axles (0.5-10m).
    /// </summary>
    public double VehicleWheelbase
    {
        get => _vehicleWheelbase;
        set => SetProperty(ref _vehicleWheelbase, value);
    }

    /// <summary>
    /// Gets or sets the track width (0.5-5m).
    /// </summary>
    public double VehicleTrackWidth
    {
        get => _vehicleTrackWidth;
        set => SetProperty(ref _vehicleTrackWidth, value);
    }

    /// <summary>
    /// Gets or sets the vehicle type.
    /// </summary>
    public VehicleType VehicleType
    {
        get => _vehicleType;
        set => SetProperty(ref _vehicleType, value);
    }

    /// <summary>
    /// Gets the available vehicle types.
    /// </summary>
    public ObservableCollection<VehicleType> VehicleTypes { get; }

    #endregion

    #region Steering Parameters Properties

    /// <summary>
    /// Gets or sets the maximum steering angle (5-60 degrees).
    /// </summary>
    public double MaxSteerAngle
    {
        get => _maxSteerAngle;
        set => SetProperty(ref _maxSteerAngle, value);
    }

    /// <summary>
    /// Gets or sets the minimum steering angle (-60 to -5 degrees).
    /// </summary>
    public double MinSteerAngle
    {
        get => _minSteerAngle;
        set => SetProperty(ref _minSteerAngle, value);
    }

    /// <summary>
    /// Gets or sets the Ackermann steering percentage (0-100%).
    /// </summary>
    public double AckermannPercentage
    {
        get => _ackermannPercentage;
        set => SetProperty(ref _ackermannPercentage, value);
    }

    /// <summary>
    /// Gets or sets the steering deadband zone (0-5 degrees).
    /// </summary>
    public double SteeringDeadband
    {
        get => _steeringDeadband;
        set => SetProperty(ref _steeringDeadband, value);
    }

    #endregion

    #region GPS Antenna Properties

    /// <summary>
    /// Gets or sets the antenna height above ground (0.5-5m).
    /// </summary>
    public double AntennaHeight
    {
        get => _antennaHeight;
        set => SetProperty(ref _antennaHeight, value);
    }

    /// <summary>
    /// Gets or sets the lateral offset from centerline (-5 to 5m).
    /// </summary>
    public double AntennaOffset
    {
        get => _antennaOffset;
        set => SetProperty(ref _antennaOffset, value);
    }

    /// <summary>
    /// Gets or sets the forward offset from rear axle (0-10m).
    /// </summary>
    public double AntennaForwardOffset
    {
        get => _antennaForwardOffset;
        set => SetProperty(ref _antennaForwardOffset, value);
    }

    #endregion

    #region Implement Properties

    /// <summary>
    /// Gets or sets the working width (0.5-30m).
    /// </summary>
    public double ImplementWidth
    {
        get => _implementWidth;
        set => SetProperty(ref _implementWidth, value);
    }

    /// <summary>
    /// Gets or sets the lateral offset (-10 to 10m).
    /// </summary>
    public double ImplementOffset
    {
        get => _implementOffset;
        set => SetProperty(ref _implementOffset, value);
    }

    /// <summary>
    /// Gets or sets the number of section control zones (1-16).
    /// </summary>
    public int NumberOfSections
    {
        get => _numberOfSections;
        set => SetProperty(ref _numberOfSections, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this is a trailing implement.
    /// </summary>
    public bool IsTrailing
    {
        get => _isTrailing;
        set => SetProperty(ref _isTrailing, value);
    }

    #endregion

    #region Look-Ahead Properties

    /// <summary>
    /// Gets or sets the minimum look-ahead distance (1-20m).
    /// </summary>
    public double MinLookAhead
    {
        get => _minLookAhead;
        set => SetProperty(ref _minLookAhead, value);
    }

    /// <summary>
    /// Gets or sets the maximum look-ahead distance (5-50m).
    /// </summary>
    public double MaxLookAhead
    {
        get => _maxLookAhead;
        set => SetProperty(ref _maxLookAhead, value);
    }

    /// <summary>
    /// Gets or sets the speed multiplier for look-ahead (0.5-5.0).
    /// </summary>
    public double LookAheadSpeedGain
    {
        get => _lookAheadSpeedGain;
        set => SetProperty(ref _lookAheadSpeedGain, value);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Gets the command to save all vehicle parameters.
    /// </summary>
    public ICommand SaveConfigCommand { get; }

    /// <summary>
    /// Gets the command to reset to defaults.
    /// </summary>
    public ICommand ResetCommand { get; }

    /// <summary>
    /// Gets the command to load a preset configuration.
    /// </summary>
    public ICommand LoadPresetCommand { get; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads configuration from the configuration service or uses defaults.
    /// </summary>
    private void LoadConfiguration()
    {
        if (_configService == null)
        {
            return;
        }

        try
        {
            var vehicleSettings = _configService.GetVehicleSettings();

            // Map from VehicleSettings (in cm) to ViewModel properties (in cm for display)
            VehicleWheelbase = vehicleSettings.Wheelbase;
            VehicleTrackWidth = vehicleSettings.Track;
            MaxSteerAngle = vehicleSettings.MaxSteerAngle;
            VehicleType = vehicleSettings.VehicleType;
            AntennaHeight = vehicleSettings.AntennaHeight;
            AntennaOffset = vehicleSettings.AntennaOffset;
            AntennaForwardOffset = vehicleSettings.AntennaPivot;

            // Load guidance settings for look-ahead
            var guidanceSettings = _configService.GetGuidanceSettings();
            // MinLookAhead = guidanceSettings.MinLookAhead;
            // MaxLookAhead = guidanceSettings.MaxLookAhead;
            // LookAheadSpeedGain = guidanceSettings.LookAheadGain;

            // Load tool settings for implement
            var toolSettings = _configService.GetToolSettings();
            // ImplementWidth = toolSettings.Width;
            // ImplementOffset = toolSettings.Offset;
            // IsTrailing = toolSettings.IsTrailing;

            // Load section control settings
            var sectionSettings = _configService.GetSectionControlSettings();
            // NumberOfSections = sectionSettings.NumberOfSections;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Save Config command.
    /// </summary>
    private async void OnSaveConfig()
    {
        if (_configService == null)
        {
            SetError("Configuration service not available");
            return;
        }

        // Validate all inputs
        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            IsBusy = true;

            // Create updated vehicle settings
            var vehicleSettings = _configService.GetVehicleSettings();
            vehicleSettings.Wheelbase = VehicleWheelbase;
            vehicleSettings.Track = VehicleTrackWidth;
            vehicleSettings.MaxSteerAngle = MaxSteerAngle;
            vehicleSettings.VehicleType = VehicleType;
            vehicleSettings.AntennaHeight = AntennaHeight;
            vehicleSettings.AntennaOffset = AntennaOffset;
            vehicleSettings.AntennaPivot = AntennaForwardOffset;

            // Save vehicle settings
            var result = await _configService.UpdateVehicleSettingsAsync(vehicleSettings);

            if (!result.IsValid)
            {
                SetError($"Validation failed: {string.Join(", ", result.Errors)}");
                return;
            }

            // TODO: Update guidance settings, tool settings, and section control settings
            // when those models have the appropriate properties

            // Save all settings
            await _configService.SaveSettingsAsync();

            // Close dialog with success
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Failed to save configuration: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validates all input values.
    /// </summary>
    /// <returns>True if all inputs are valid.</returns>
    private bool ValidateInputs()
    {
        // Vehicle Name
        if (string.IsNullOrWhiteSpace(VehicleName))
        {
            SetError("Vehicle name is required");
            return false;
        }

        if (VehicleName.Length > 50)
        {
            SetError("Vehicle name must be 50 characters or less");
            return false;
        }

        // Vehicle Dimensions
        if (VehicleWheelbase <= 0)
        {
            SetError("Wheelbase must be greater than 0");
            return false;
        }

        if (VehicleTrackWidth <= 0)
        {
            SetError("Track width must be greater than 0");
            return false;
        }

        // Steering Angles
        if (MaxSteerAngle <= MinSteerAngle)
        {
            SetError("Maximum steering angle must be greater than minimum");
            return false;
        }

        if (MaxSteerAngle < 5 || MaxSteerAngle > 60)
        {
            SetError("Maximum steering angle must be between 5 and 60 degrees");
            return false;
        }

        if (MinSteerAngle < -60 || MinSteerAngle > -5)
        {
            SetError("Minimum steering angle must be between -60 and -5 degrees");
            return false;
        }

        // Implement
        if (ImplementWidth <= 0)
        {
            SetError("Implement width must be greater than 0");
            return false;
        }

        if (NumberOfSections < 1 || NumberOfSections > 16)
        {
            SetError("Number of sections must be between 1 and 16");
            return false;
        }

        // Look-Ahead
        if (MinLookAhead >= MaxLookAhead)
        {
            SetError("Maximum look-ahead must be greater than minimum");
            return false;
        }

        ClearError();
        return true;
    }

    /// <summary>
    /// Handles the Reset command.
    /// </summary>
    private void OnReset()
    {
        // Reset to default values
        VehicleName = "Default Vehicle";
        VehicleWidth = 200.0;
        VehicleWheelbase = 180.0;
        VehicleTrackWidth = 150.0;
        VehicleType = VehicleType.Tractor;

        MaxSteerAngle = 45.0;
        MinSteerAngle = -45.0;
        AckermannPercentage = 100.0;
        SteeringDeadband = 0.5;

        AntennaHeight = 50.0;
        AntennaOffset = 0.0;
        AntennaForwardOffset = 25.0;

        ImplementWidth = 300.0;
        ImplementOffset = 0.0;
        NumberOfSections = 5;
        IsTrailing = true;

        MinLookAhead = 3.0;
        MaxLookAhead = 10.0;
        LookAheadSpeedGain = 1.5;

        ClearError();
    }

    /// <summary>
    /// Handles the Load Preset command.
    /// </summary>
    private void OnLoadPreset()
    {
        // TODO: Implement preset loading from predefined configurations
        // This could show a dialog with common vehicle presets
        // (e.g., "Small Tractor", "Large Tractor", "Combine", etc.)
    }

    #endregion
}
