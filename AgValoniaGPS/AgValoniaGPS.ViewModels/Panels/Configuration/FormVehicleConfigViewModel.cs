using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.ViewModels.Base;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.Configuration;

/// <summary>
/// ViewModel for vehicle configuration panel.
/// Provides UI for configuring vehicle physical dimensions and parameters.
/// </summary>
public class FormVehicleConfigViewModel : PanelViewModelBase
{
    private readonly VehicleConfiguration _vehicleConfig;
    private readonly IConfigurationService _configService;

    private string _vehicleName = "Default";
    private string _vehicleType = "Tractor";
    private double _wheelbase = 2.5;
    private double _trackWidth = 1.8;
    private double _antennaHeight = 3.0;
    private double _antennaForwardOffset = 0.0;
    private double _antennaRightOffset = 0.0;
    private double _maxSteerAngle = 35.0;
    private double _hitchLength = 0.0;

    public FormVehicleConfigViewModel(
        VehicleConfiguration vehicleConfig,
        IConfigurationService configService)
    {
        _vehicleConfig = vehicleConfig ?? throw new ArgumentNullException(nameof(vehicleConfig));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        Title = "Vehicle Configuration";

        // Commands
        SaveVehicleCommand = new RelayCommand(OnSaveVehicle);
        LoadVehicleCommand = new RelayCommand(OnLoadVehicle);
        NewVehicleCommand = new RelayCommand(OnNewVehicle);

        // Load current settings
        LoadCurrentSettings();
    }

    public string Title { get; } = "Vehicle Configuration";

    /// <summary>
    /// Vehicle name
    /// </summary>
    public string VehicleName
    {
        get => _vehicleName;
        set => SetProperty(ref _vehicleName, value);
    }

    /// <summary>
    /// Vehicle type (Tractor / Sprayer / Harvester / Other)
    /// </summary>
    public string VehicleType
    {
        get => _vehicleType;
        set => SetProperty(ref _vehicleType, value);
    }

    /// <summary>
    /// Wheelbase in meters (1.0-10.0)
    /// </summary>
    public double Wheelbase
    {
        get => _wheelbase;
        set => SetProperty(ref _wheelbase, Math.Clamp(value, 1.0, 10.0));
    }

    /// <summary>
    /// Track width in meters (1.0-10.0)
    /// </summary>
    public double TrackWidth
    {
        get => _trackWidth;
        set => SetProperty(ref _trackWidth, Math.Clamp(value, 1.0, 10.0));
    }

    /// <summary>
    /// Antenna height in meters (0.5-5.0)
    /// </summary>
    public double AntennaHeight
    {
        get => _antennaHeight;
        set => SetProperty(ref _antennaHeight, Math.Clamp(value, 0.5, 5.0));
    }

    /// <summary>
    /// Antenna forward offset in meters (-5.0 to +5.0)
    /// </summary>
    public double AntennaForwardOffset
    {
        get => _antennaForwardOffset;
        set => SetProperty(ref _antennaForwardOffset, Math.Clamp(value, -5.0, 5.0));
    }

    /// <summary>
    /// Antenna right offset in meters (-5.0 to +5.0)
    /// </summary>
    public double AntennaRightOffset
    {
        get => _antennaRightOffset;
        set => SetProperty(ref _antennaRightOffset, Math.Clamp(value, -5.0, 5.0));
    }

    /// <summary>
    /// Maximum steering angle in degrees (10-45)
    /// </summary>
    public double MaxSteerAngle
    {
        get => _maxSteerAngle;
        set => SetProperty(ref _maxSteerAngle, Math.Clamp(value, 10.0, 45.0));
    }

    /// <summary>
    /// Hitch length in meters (0.0-10.0)
    /// </summary>
    public double HitchLength
    {
        get => _hitchLength;
        set => SetProperty(ref _hitchLength, Math.Clamp(value, 0.0, 10.0));
    }

    public ICommand SaveVehicleCommand { get; }
    public ICommand LoadVehicleCommand { get; }
    public ICommand NewVehicleCommand { get; }

    private async void OnSaveVehicle()
    {
        try
        {
            IsBusy = true;

            // Update vehicle configuration
            _vehicleConfig.Wheelbase = Wheelbase;
            _vehicleConfig.TrackWidth = TrackWidth;
            _vehicleConfig.AntennaHeight = AntennaHeight;
            _vehicleConfig.AntennaPivot = AntennaForwardOffset;
            _vehicleConfig.AntennaOffset = AntennaRightOffset;
            _vehicleConfig.MaxSteerAngle = MaxSteerAngle;

            // Convert vehicle type string to enum
            _vehicleConfig.Type = VehicleType switch
            {
                "Tractor" => Models.VehicleType.Tractor,
                "Harvester" => Models.VehicleType.Harvester,
                "4WD" => Models.VehicleType.FourWD,
                _ => Models.VehicleType.Tractor
            };

            // Update vehicle settings in configuration service
            var vehicleSettings = _configService.GetVehicleSettings();
            vehicleSettings.Wheelbase = Wheelbase * 100.0; // Convert to cm
            vehicleSettings.Track = TrackWidth * 100.0; // Convert to cm
            vehicleSettings.AntennaHeight = AntennaHeight * 100.0; // Convert to cm
            vehicleSettings.AntennaPivot = AntennaForwardOffset * 100.0; // Convert to cm
            vehicleSettings.AntennaOffset = AntennaRightOffset * 100.0; // Convert to cm
            vehicleSettings.MaxSteerAngle = MaxSteerAngle;
            vehicleSettings.VehicleHitchLength = HitchLength * 100.0; // Convert to cm

            await _configService.UpdateVehicleSettingsAsync(vehicleSettings);

            // Save configuration to file
            await _configService.SaveSettingsAsync();

            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save vehicle: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnLoadVehicle()
    {
        try
        {
            IsBusy = true;

            // Load configuration from file
            var result = await _configService.LoadSettingsAsync(VehicleName);

            if (result.Success)
            {
                LoadCurrentSettings();
                ClearError();
            }
            else
            {
                SetError($"Failed to load vehicle: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load vehicle: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnNewVehicle()
    {
        VehicleName = "New Vehicle";
        VehicleType = "Tractor";
        Wheelbase = 2.5;
        TrackWidth = 1.8;
        AntennaHeight = 3.0;
        AntennaForwardOffset = 0.0;
        AntennaRightOffset = 0.0;
        MaxSteerAngle = 35.0;
        HitchLength = 0.0;

        ClearError();
    }

    private void LoadCurrentSettings()
    {
        // Load from vehicle configuration
        Wheelbase = _vehicleConfig.Wheelbase;
        TrackWidth = _vehicleConfig.TrackWidth;
        AntennaHeight = _vehicleConfig.AntennaHeight;
        AntennaForwardOffset = _vehicleConfig.AntennaPivot;
        AntennaRightOffset = _vehicleConfig.AntennaOffset;
        MaxSteerAngle = _vehicleConfig.MaxSteerAngle;

        // Convert vehicle type enum to string
        VehicleType = _vehicleConfig.Type switch
        {
            Models.VehicleType.Tractor => "Tractor",
            Models.VehicleType.Harvester => "Harvester",
            Models.VehicleType.FourWD => "4WD",
            _ => "Tractor"
        };

        // Load from configuration service
        var vehicleSettings = _configService.GetVehicleSettings();
        HitchLength = vehicleSettings.VehicleHitchLength / 100.0; // Convert from cm to meters
    }
}
