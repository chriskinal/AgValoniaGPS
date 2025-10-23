using System;
using System.Reactive;
using System.Reactive.Linq;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Settings;

/// <summary>
/// ViewModel for ConfigSummaryControl displaying an overview of all key configuration values.
/// </summary>
public class ConfigSummaryViewModel : DialogViewModelBase
{
    private readonly IConfigurationService? _configService;

    private string _vehicleName = "Default Vehicle";
    private double _vehicleWheelbase = 180.0;
    private double _vehicleTrack = 30.0;
    private double _maxSteerAngle = 45.0;
    private double _antennaOffset = 0.0;
    private string _vehicleType = "Tractor";
    private string _userName = "Default User";
    private string _units = "Metric";
    private bool _autoSave = true;
    private int _autoSaveInterval = 300;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigSummaryViewModel"/> class.
    /// </summary>
    /// <param name="configService">Optional configuration service for dependency injection.</param>
    public ConfigSummaryViewModel(IConfigurationService? configService = null)
    {
        _configService = configService;

        // Initialize commands
        EditVehicleCommand = ReactiveCommand.Create(OnEditVehicle);
        EditUserCommand = ReactiveCommand.Create(OnEditUser);
        RefreshCommand = ReactiveCommand.Create(OnRefresh);

        // Load initial configuration
        LoadConfiguration();
    }

    /// <summary>
    /// Gets or sets the vehicle name.
    /// </summary>
    public string VehicleName
    {
        get => _vehicleName;
        set => this.RaiseAndSetIfChanged(ref _vehicleName, value);
    }

    /// <summary>
    /// Gets or sets the vehicle wheelbase in centimeters.
    /// </summary>
    public double VehicleWheelbase
    {
        get => _vehicleWheelbase;
        set => this.RaiseAndSetIfChanged(ref _vehicleWheelbase, value);
    }

    /// <summary>
    /// Gets or sets the vehicle track width in centimeters.
    /// </summary>
    public double VehicleTrack
    {
        get => _vehicleTrack;
        set => this.RaiseAndSetIfChanged(ref _vehicleTrack, value);
    }

    /// <summary>
    /// Gets or sets the maximum steering angle in degrees.
    /// </summary>
    public double MaxSteerAngle
    {
        get => _maxSteerAngle;
        set => this.RaiseAndSetIfChanged(ref _maxSteerAngle, value);
    }

    /// <summary>
    /// Gets or sets the GPS antenna offset in centimeters.
    /// </summary>
    public double AntennaOffset
    {
        get => _antennaOffset;
        set => this.RaiseAndSetIfChanged(ref _antennaOffset, value);
    }

    /// <summary>
    /// Gets or sets the vehicle type as a string.
    /// </summary>
    public string VehicleType
    {
        get => _vehicleType;
        set => this.RaiseAndSetIfChanged(ref _vehicleType, value);
    }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName
    {
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    /// <summary>
    /// Gets or sets the units preference (Metric or Imperial).
    /// </summary>
    public string Units
    {
        get => _units;
        set => this.RaiseAndSetIfChanged(ref _units, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether auto-save is enabled.
    /// </summary>
    public bool AutoSave
    {
        get => _autoSave;
        set => this.RaiseAndSetIfChanged(ref _autoSave, value);
    }

    /// <summary>
    /// Gets or sets the auto-save interval in seconds.
    /// </summary>
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => this.RaiseAndSetIfChanged(ref _autoSaveInterval, value);
    }

    /// <summary>
    /// Gets the command to edit vehicle settings.
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditVehicleCommand { get; }

    /// <summary>
    /// Gets the command to edit user settings.
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditUserCommand { get; }

    /// <summary>
    /// Gets the command to refresh all values.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

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
            VehicleWheelbase = vehicleSettings.Wheelbase;
            VehicleTrack = vehicleSettings.Track;
            MaxSteerAngle = vehicleSettings.MaxSteerAngle;
            AntennaOffset = vehicleSettings.AntennaOffset;
            VehicleType = vehicleSettings.VehicleType.ToString();

            // TODO: Load user settings when UserProfile is available
            // var userProfile = _configService.GetUserProfile();
            // UserName = userProfile.Name;
            // Units = userProfile.Units;

            // TODO: Load app settings when ApplicationSettings is available
            // var appSettings = _configService.GetApplicationSettings();
            // AutoSave = appSettings.AutoSave;
            // AutoSaveInterval = appSettings.AutoSaveInterval;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Edit Vehicle command.
    /// </summary>
    private void OnEditVehicle()
    {
        // TODO: Open vehicle settings dialog
        // This will be implemented when dialog service is integrated
    }

    /// <summary>
    /// Handles the Edit User command.
    /// </summary>
    private void OnEditUser()
    {
        // TODO: Open user settings dialog
        // This will be implemented when dialog service is integrated
    }

    /// <summary>
    /// Handles the Refresh command.
    /// </summary>
    private void OnRefresh()
    {
        LoadConfiguration();
    }
}
