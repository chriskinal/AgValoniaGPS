using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.ViewModels.Base;

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
        EditVehicleCommand = new RelayCommand(OnEditVehicle);
        EditUserCommand = new RelayCommand(OnEditUser);
        RefreshCommand = new RelayCommand(OnRefresh);

        // Load initial configuration
        LoadConfiguration();
    }

    /// <summary>
    /// Gets or sets the vehicle name.
    /// </summary>
    public string VehicleName
    {
        get => _vehicleName;
        set => SetProperty(ref _vehicleName, value);
    }

    /// <summary>
    /// Gets or sets the vehicle wheelbase in centimeters.
    /// </summary>
    public double VehicleWheelbase
    {
        get => _vehicleWheelbase;
        set => SetProperty(ref _vehicleWheelbase, value);
    }

    /// <summary>
    /// Gets or sets the vehicle track width in centimeters.
    /// </summary>
    public double VehicleTrack
    {
        get => _vehicleTrack;
        set => SetProperty(ref _vehicleTrack, value);
    }

    /// <summary>
    /// Gets or sets the maximum steering angle in degrees.
    /// </summary>
    public double MaxSteerAngle
    {
        get => _maxSteerAngle;
        set => SetProperty(ref _maxSteerAngle, value);
    }

    /// <summary>
    /// Gets or sets the GPS antenna offset in centimeters.
    /// </summary>
    public double AntennaOffset
    {
        get => _antennaOffset;
        set => SetProperty(ref _antennaOffset, value);
    }

    /// <summary>
    /// Gets or sets the vehicle type as a string.
    /// </summary>
    public string VehicleType
    {
        get => _vehicleType;
        set => SetProperty(ref _vehicleType, value);
    }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    /// <summary>
    /// Gets or sets the units preference (Metric or Imperial).
    /// </summary>
    public string Units
    {
        get => _units;
        set => SetProperty(ref _units, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether auto-save is enabled.
    /// </summary>
    public bool AutoSave
    {
        get => _autoSave;
        set => SetProperty(ref _autoSave, value);
    }

    /// <summary>
    /// Gets or sets the auto-save interval in seconds.
    /// </summary>
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => SetProperty(ref _autoSaveInterval, value);
    }

    /// <summary>
    /// Gets the command to edit vehicle settings.
    /// </summary>
    public ICommand EditVehicleCommand { get; }

    /// <summary>
    /// Gets the command to edit user settings.
    /// </summary>
    public ICommand EditUserCommand { get; }

    /// <summary>
    /// Gets the command to refresh all values.
    /// </summary>
    public ICommand RefreshCommand { get; }

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
