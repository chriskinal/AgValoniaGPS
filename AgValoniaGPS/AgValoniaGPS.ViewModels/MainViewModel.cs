using System;
using System.Windows.Input;
using ReactiveUI;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.Services.Session;
using AgValoniaGPS.Services.Profile;
using Avalonia.Threading;

namespace AgValoniaGPS.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly IUdpCommunicationService _udpService;
    private readonly IGpsService _gpsService;
    private readonly IFieldService _fieldService;
    private readonly IGuidanceService _guidanceService;
    private readonly INtripClientService _ntripService;
    private readonly IFieldStatisticsService _fieldStatistics;
    private readonly VehicleConfiguration _vehicleConfig;
    private readonly NmeaParserService _nmeaParser;

    // Wave 1 Services
    private readonly IPositionUpdateService _positionService;
    private readonly IHeadingCalculatorService _headingService;

    // Wave 8 State Management Services
    private readonly IConfigurationService _configService;
    private readonly ISessionManagementService _sessionService;
    private readonly IProfileManagementService _profileService;

    private string _statusMessage = "Starting...";
    private double _latitude;
    private double _longitude;
    private double _speed;
    private int _satelliteCount;
    private string _fixQuality = "No Fix";
    private string _networkStatus = "Disconnected";
    // Hello status (connection health)
    private bool _isAutoSteerHelloOk;
    private bool _isMachineHelloOk;
    private bool _isImuHelloOk;
    private bool _isGpsHelloOk;

    // Data status (data flow)
    private bool _isAutoSteerDataOk;
    private bool _isMachineDataOk;
    private bool _isImuDataOk;
    private bool _isGpsDataOk;
    private string _debugLog = "";
    private double _easting;
    private double _northing;
    private double _heading;
    private string _headingSource = "None";
    private bool _isReversing = false;

    // Field properties
    private Field? _activeField;
    private string _fieldsRootDirectory = string.Empty;

    public MainViewModel(
        IUdpCommunicationService udpService,
        IGpsService gpsService,
        IFieldService fieldService,
        IGuidanceService guidanceService,
        INtripClientService ntripService,
        IFieldStatisticsService fieldStatistics,
        VehicleConfiguration vehicleConfig,
        IPositionUpdateService positionService,
        IHeadingCalculatorService headingService,
        IConfigurationService configService,
        ISessionManagementService sessionService,
        IProfileManagementService profileService)
    {
        _udpService = udpService;
        _gpsService = gpsService;
        _fieldService = fieldService;
        _guidanceService = guidanceService;
        _ntripService = ntripService;
        _fieldStatistics = fieldStatistics;
        _vehicleConfig = vehicleConfig;
        _positionService = positionService;
        _headingService = headingService;
        _configService = configService;
        _sessionService = sessionService;
        _profileService = profileService;
        _nmeaParser = new NmeaParserService(gpsService);

        // Subscribe to GPS and UDP events
        _gpsService.GpsDataUpdated += OnGpsDataUpdated;
        _udpService.DataReceived += OnUdpDataReceived;
        _udpService.ModuleConnectionChanged += OnModuleConnectionChanged;
        _ntripService.ConnectionStatusChanged += OnNtripConnectionChanged;
        _ntripService.RtcmDataReceived += OnRtcmDataReceived;
        _fieldService.ActiveFieldChanged += OnActiveFieldChanged;

        // Subscribe to Wave 1 service events
        _positionService.PositionUpdated += OnPositionUpdated;
        _headingService.HeadingChanged += OnHeadingChanged;

        // Start UDP communication
        InitializeAsync();
    }

    public async System.Threading.Tasks.Task ConnectToNtripAsync()
    {
        try
        {
            var config = new NtripConfiguration
            {
                CasterAddress = NtripCasterAddress,
                CasterPort = NtripCasterPort,
                MountPoint = NtripMountPoint,
                Username = NtripUsername,
                Password = NtripPassword,
                SubnetAddress = "192.168.5",
                UdpForwardPort = 2233,
                GgaIntervalSeconds = 10,
                UseManualPosition = false
            };

            await _ntripService.ConnectAsync(config);
        }
        catch (Exception ex)
        {
            NtripStatus = $"Error: {ex.Message}";
        }
    }

    public async System.Threading.Tasks.Task DisconnectFromNtripAsync()
    {
        await _ntripService.DisconnectAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            await _udpService.StartAsync();
            NetworkStatus = $"UDP Connected: {_udpService.LocalIPAddress}";
            StatusMessage = "Ready - Waiting for modules...";

            // Start sending hello packets every second
            StartHelloTimer();
        }
        catch (Exception ex)
        {
            NetworkStatus = $"UDP Error: {ex.Message}";
            StatusMessage = "Network error";
        }
    }

    private async void StartHelloTimer()
    {
        while (_udpService.IsConnected)
        {
            // Send hello packet every second
            _udpService.SendHelloPacket();

            // Check module status using appropriate method for each:
            // - AutoSteer: Data flow (sends PGN 250/253 regularly)
            // - Machine: Hello only (receive-only, no data sent)
            // - IMU: Hello only (only sends when active)
            // - GPS: Data flow (sends NMEA regularly)

            var steerOk = _udpService.IsModuleDataOk(ModuleType.AutoSteer);
            var machineOk = _udpService.IsModuleHelloOk(ModuleType.Machine);
            var imuOk = _udpService.IsModuleHelloOk(ModuleType.IMU);
            var gpsOk = ((GpsService)_gpsService).IsGpsDataOk();

            // Update UI properties
            IsAutoSteerDataOk = steerOk;
            IsMachineDataOk = machineOk;
            IsImuDataOk = imuOk;
            IsGpsDataOk = gpsOk;

            if (!gpsOk)
            {
                StatusMessage = "GPS Timeout";
                FixQuality = "No Fix";
            }
            else
            {
                UpdateStatusMessage();
            }

            await System.Threading.Tasks.Task.Delay(100); // Check every 100ms for fast response
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public double Latitude
    {
        get => _latitude;
        set => this.RaiseAndSetIfChanged(ref _latitude, value);
    }

    public double Longitude
    {
        get => _longitude;
        set => this.RaiseAndSetIfChanged(ref _longitude, value);
    }

    public double Speed
    {
        get => _speed;
        set => this.RaiseAndSetIfChanged(ref _speed, value);
    }

    public int SatelliteCount
    {
        get => _satelliteCount;
        set => this.RaiseAndSetIfChanged(ref _satelliteCount, value);
    }

    public string FixQuality
    {
        get => _fixQuality;
        set => this.RaiseAndSetIfChanged(ref _fixQuality, value);
    }

    public string NetworkStatus
    {
        get => _networkStatus;
        set => this.RaiseAndSetIfChanged(ref _networkStatus, value);
    }

    // AutoSteer Hello and Data properties
    public bool IsAutoSteerHelloOk
    {
        get => _isAutoSteerHelloOk;
        set => this.RaiseAndSetIfChanged(ref _isAutoSteerHelloOk, value);
    }

    public bool IsAutoSteerDataOk
    {
        get => _isAutoSteerDataOk;
        set => this.RaiseAndSetIfChanged(ref _isAutoSteerDataOk, value);
    }

    // Machine Hello and Data properties
    public bool IsMachineHelloOk
    {
        get => _isMachineHelloOk;
        set => this.RaiseAndSetIfChanged(ref _isMachineHelloOk, value);
    }

    public bool IsMachineDataOk
    {
        get => _isMachineDataOk;
        set => this.RaiseAndSetIfChanged(ref _isMachineDataOk, value);
    }

    // IMU Hello and Data properties
    public bool IsImuHelloOk
    {
        get => _isImuHelloOk;
        set => this.RaiseAndSetIfChanged(ref _isImuHelloOk, value);
    }

    public bool IsImuDataOk
    {
        get => _isImuDataOk;
        set => this.RaiseAndSetIfChanged(ref _isImuDataOk, value);
    }

    // GPS Hello and Data properties (GPS doesn't have hello, just data from NMEA)
    public bool IsGpsDataOk
    {
        get => _isGpsDataOk;
        set => this.RaiseAndSetIfChanged(ref _isGpsDataOk, value);
    }

    // NTRIP properties
    private bool _isNtripConnected;
    private string _ntripStatus = "Not Connected";
    private ulong _ntripBytesReceived;
    private string _ntripCasterAddress = "rtk2go.com";
    private int _ntripCasterPort = 2101;
    private string _ntripMountPoint = "";
    private string _ntripUsername = "";
    private string _ntripPassword = "";

    public bool IsNtripConnected
    {
        get => _isNtripConnected;
        set => this.RaiseAndSetIfChanged(ref _isNtripConnected, value);
    }

    public string NtripStatus
    {
        get => _ntripStatus;
        set => this.RaiseAndSetIfChanged(ref _ntripStatus, value);
    }

    public string NtripBytesReceived
    {
        get => $"{(_ntripBytesReceived / 1024):N0} KB";
    }

    public string NtripCasterAddress
    {
        get => _ntripCasterAddress;
        set => this.RaiseAndSetIfChanged(ref _ntripCasterAddress, value);
    }

    public int NtripCasterPort
    {
        get => _ntripCasterPort;
        set => this.RaiseAndSetIfChanged(ref _ntripCasterPort, value);
    }

    public string NtripMountPoint
    {
        get => _ntripMountPoint;
        set => this.RaiseAndSetIfChanged(ref _ntripMountPoint, value);
    }

    public string NtripUsername
    {
        get => _ntripUsername;
        set => this.RaiseAndSetIfChanged(ref _ntripUsername, value);
    }

    public string NtripPassword
    {
        get => _ntripPassword;
        set => this.RaiseAndSetIfChanged(ref _ntripPassword, value);
    }

    public string DebugLog
    {
        get => _debugLog;
        set => this.RaiseAndSetIfChanged(ref _debugLog, value);
    }

    public double Easting
    {
        get => _easting;
        set => this.RaiseAndSetIfChanged(ref _easting, value);
    }

    public double Northing
    {
        get => _northing;
        set => this.RaiseAndSetIfChanged(ref _northing, value);
    }

    public double Heading
    {
        get => _heading;
        set => this.RaiseAndSetIfChanged(ref _heading, value);
    }

    public string HeadingSource
    {
        get => _headingSource;
        set => this.RaiseAndSetIfChanged(ref _headingSource, value);
    }

    public bool IsReversing
    {
        get => _isReversing;
        set => this.RaiseAndSetIfChanged(ref _isReversing, value);
    }

    /// <summary>
    /// Handles GPS data updates from GpsService and feeds them into Wave 1 position pipeline
    /// </summary>
    private void OnGpsDataUpdated(object? sender, GpsData data)
    {
        // Marshal to UI thread (use Invoke for synchronous execution to avoid modal dialog issues)
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            // Already on UI thread, execute directly
            UpdateGpsProperties(data);

            // Feed GPS data into Wave 1 position processing pipeline
            ProcessGpsDataThroughPipeline(data);
        }
        else
        {
            // Not on UI thread, invoke synchronously
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
            {
                UpdateGpsProperties(data);
                ProcessGpsDataThroughPipeline(data);
            });
        }
    }

    /// <summary>
    /// Process GPS data through Wave 1 services pipeline
    /// GPS Data → Position Service → Position Updated Event → Heading Service → Heading Changed Event
    /// </summary>
    private void ProcessGpsDataThroughPipeline(GpsData data)
    {
        try
        {
            // Pass GPS data to position update service
            // The service will calculate speed, detect reverse, maintain history, and fire PositionUpdated event
            _positionService.ProcessGpsPosition(data, null); // TODO: Add IMU data when available
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            DebugLog = $"Position processing error: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles position updates from IPositionUpdateService
    /// This is fired after GPS position has been processed
    /// </summary>
    private void OnPositionUpdated(object? sender, PositionUpdateEventArgs e)
    {
        // Marshal to UI thread
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            UpdatePositionProperties(e);
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => UpdatePositionProperties(e));
        }
    }

    private void UpdatePositionProperties(PositionUpdateEventArgs e)
    {
        // Update UI properties from processed position data
        Easting = e.Position.Easting;
        Northing = e.Position.Northing;
        Speed = e.Speed; // Use calculated speed from position service (more accurate than GPS speed)
        IsReversing = e.IsReversing;

        // Note: Heading will be updated separately by OnHeadingChanged event
    }

    /// <summary>
    /// Handles heading updates from IHeadingCalculatorService
    /// </summary>
    private void OnHeadingChanged(object? sender, HeadingUpdate e)
    {
        // Marshal to UI thread
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            UpdateHeadingProperties(e);
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => UpdateHeadingProperties(e));
        }
    }

    private void UpdateHeadingProperties(HeadingUpdate e)
    {
        // Update UI properties from calculated heading
        Heading = e.Heading; // Heading in radians (0 to 2π)
        HeadingSource = e.Source.ToString(); // Display which source is being used
    }

    private void UpdateGpsProperties(GpsData data)
    {
        Latitude = data.Latitude;
        Longitude = data.Longitude;
        // Note: Speed is now updated from Position Service for better accuracy
        SatelliteCount = data.SatellitesInUse;
        FixQuality = GetFixQualityString(data.FixQuality);
        StatusMessage = data.IsValid ? "GPS Active" : "Waiting for GPS";

        // Note: Easting, Northing, and Heading are now updated from Wave 1 services
        // for more accurate processing through the position pipeline
    }

    private void OnUdpDataReceived(object? sender, UdpDataReceivedEventArgs e)
    {
        var now = DateTime.Now;
        var packetAge = (now - e.Timestamp).TotalMilliseconds;

        // Handle different message types
        if (e.PGN == 0)
        {
            // NMEA text sentence
            try
            {
                string sentence = System.Text.Encoding.ASCII.GetString(e.Data);
                _nmeaParser.ParseSentence(sentence);
            }
            catch { }
        }
        else
        {
            // Binary PGN message - log it with age to detect buffering
            DebugLog = $"PGN: {e.PGN} (0x{e.PGN:X2}) @ {e.Timestamp:HH:mm:ss.fff} (age: {packetAge:F0}ms)";

            switch (e.PGN)
            {
                case PgnNumbers.HELLO_FROM_AUTOSTEER:
                    // AutoSteer module is alive
                    break;

                case PgnNumbers.HELLO_FROM_MACHINE:
                    // Machine module is alive
                    break;

                case PgnNumbers.HELLO_FROM_IMU:
                    // IMU module is alive
                    // TODO: Extract IMU data and pass to position service
                    break;

                // TODO: Add more PGN handlers as needed
            }
        }
    }

    private void OnModuleConnectionChanged(object? sender, ModuleConnectionEventArgs e)
    {
        // This event is no longer used - status is polled every 100ms
    }

    private void OnNtripConnectionChanged(object? sender, NtripConnectionEventArgs e)
    {
        // Marshal to UI thread (use Invoke for synchronous execution to avoid modal dialog issues)
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            UpdateNtripConnectionProperties(e);
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => UpdateNtripConnectionProperties(e));
        }
    }

    private void UpdateNtripConnectionProperties(NtripConnectionEventArgs e)
    {
        IsNtripConnected = e.IsConnected;
        NtripStatus = e.Message ?? (e.IsConnected ? "Connected" : "Not Connected");
    }

    private void OnRtcmDataReceived(object? sender, RtcmDataReceivedEventArgs e)
    {
        // Marshal to UI thread (use Invoke for synchronous execution to avoid modal dialog issues)
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            UpdateNtripDataProperties();
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => UpdateNtripDataProperties());
        }
    }

    private void UpdateNtripDataProperties()
    {
        _ntripBytesReceived = _ntripService.TotalBytesReceived;
        this.RaisePropertyChanged(nameof(NtripBytesReceived));
    }

    private void UpdateStatusMessage()
    {
        int connectedCount = 0;
        if (IsAutoSteerDataOk) connectedCount++;
        if (IsMachineDataOk) connectedCount++;
        if (IsImuDataOk) connectedCount++;

        StatusMessage = connectedCount > 0
            ? $"{connectedCount} module(s) active"
            : "Waiting for modules...";
    }

    private string GetFixQualityString(int fixQuality) => fixQuality switch
    {
        0 => "No Fix",
        1 => "GPS Fix",
        2 => "DGPS Fix",
        4 => "RTK Fixed",
        5 => "RTK Float",
        _ => "Unknown"
    };

    // Field management properties
    public Field? ActiveField
    {
        get => _activeField;
        set => this.RaiseAndSetIfChanged(ref _activeField, value);
    }

    public string FieldsRootDirectory
    {
        get => _fieldsRootDirectory;
        set => this.RaiseAndSetIfChanged(ref _fieldsRootDirectory, value);
    }

    public string? ActiveFieldName => ActiveField?.Name;
    public double? ActiveFieldArea => ActiveField?.TotalArea;

    // AOG_Dev services - expose for UI/control access
    public VehicleConfiguration VehicleConfig => _vehicleConfig;
    public IFieldStatisticsService FieldStatistics => _fieldStatistics;

    // Field statistics properties for UI binding
    public string WorkedAreaDisplay => _fieldStatistics.FormatArea(_fieldStatistics.WorkedAreaSquareMeters);
    public string BoundaryAreaDisplay => _fieldStatistics.FormatArea(_fieldStatistics.BoundaryAreaSquareMeters);
    public double RemainingPercent => _fieldStatistics.GetRemainingPercent();

    private void OnActiveFieldChanged(object? sender, Field? field)
    {
        // Marshal to UI thread
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            UpdateActiveField(field);
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => UpdateActiveField(field));
        }
    }

    private void UpdateActiveField(Field? field)
    {
        ActiveField = field;
        this.RaisePropertyChanged(nameof(ActiveFieldName));
        this.RaisePropertyChanged(nameof(ActiveFieldArea));

        // Update field statistics service with new boundary
        if (field?.Boundary != null)
        {
            _fieldStatistics.UpdateBoundaryArea(field.Boundary);
            this.RaisePropertyChanged(nameof(BoundaryAreaDisplay));
            this.RaisePropertyChanged(nameof(WorkedAreaDisplay));
            this.RaisePropertyChanged(nameof(RemainingPercent));
        }

        // Update session management with new field
        if (field != null)
        {
            _sessionService.UpdateCurrentField(field.Name);
        }
    }

    // ========== Wave 8 State Management Properties ==========

    private string _currentVehicleProfile = "Default";
    private string _currentUserProfile = "Default";
    private string _sessionStateInfo = "No active session";

    public string CurrentVehicleProfile
    {
        get => _currentVehicleProfile;
        set => this.RaiseAndSetIfChanged(ref _currentVehicleProfile, value);
    }

    public string CurrentUserProfile
    {
        get => _currentUserProfile;
        set => this.RaiseAndSetIfChanged(ref _currentUserProfile, value);
    }

    public string SessionStateInfo
    {
        get => _sessionStateInfo;
        set => this.RaiseAndSetIfChanged(ref _sessionStateInfo, value);
    }

    /// <summary>
    /// Load settings from configuration service (reads v6.x XML files)
    /// </summary>
    public async System.Threading.Tasks.Task LoadVehicleSettingsAsync(string vehicleName)
    {
        try
        {
            var result = await _configService.LoadSettingsAsync(vehicleName);

            if (result.Success)
            {
                CurrentVehicleProfile = vehicleName;
                SessionStateInfo = $"Settings loaded from {result.Source} for {vehicleName}";

                // Apply vehicle settings to VehicleConfiguration
                var vehicleSettings = _configService.GetVehicleSettings();
                _vehicleConfig.Wheelbase = vehicleSettings.Wheelbase;
                _vehicleConfig.TrackWidth = vehicleSettings.Track;
                _vehicleConfig.MaxSteerAngle = vehicleSettings.MaxSteerAngle;
                _vehicleConfig.AntennaHeight = vehicleSettings.AntennaHeight;
                _vehicleConfig.AntennaPivot = vehicleSettings.AntennaPivot;
                _vehicleConfig.AntennaOffset = vehicleSettings.AntennaOffset;

                StatusMessage = $"Vehicle profile '{vehicleName}' loaded successfully";
            }
            else
            {
                SessionStateInfo = $"Failed to load settings: {result.ErrorMessage}";
                StatusMessage = "Failed to load vehicle profile";
            }
        }
        catch (Exception ex)
        {
            SessionStateInfo = $"Error loading settings: {ex.Message}";
            StatusMessage = "Error loading vehicle profile";
        }
    }

    /// <summary>
    /// Save current settings using configuration service (saves to JSON + XML)
    /// </summary>
    public async System.Threading.Tasks.Task SaveVehicleSettingsAsync()
    {
        try
        {
            // Update configuration with current vehicle settings
            var vehicleSettings = new AgValoniaGPS.Models.Configuration.VehicleSettings
            {
                Wheelbase = _vehicleConfig.Wheelbase,
                Track = _vehicleConfig.TrackWidth,
                MaxSteerAngle = _vehicleConfig.MaxSteerAngle,
                AntennaHeight = _vehicleConfig.AntennaHeight,
                AntennaPivot = _vehicleConfig.AntennaPivot,
                AntennaOffset = _vehicleConfig.AntennaOffset,
                MaxAngularVelocity = 80.0, // Default values
                PivotBehindAnt = 0.0,
                SteerAxleAhead = 0.0,
                VehicleType = 0,
                VehicleHitchLength = 0.0,
                MinUturnRadius = 4.0
            };

            var updateResult = await _configService.UpdateVehicleSettingsAsync(vehicleSettings);

            if (updateResult.IsValid)
            {
                var saveResult = await _configService.SaveSettingsAsync();

                if (saveResult.Success)
                {
                    var formatCount = (saveResult.JsonSaved ? 1 : 0) + (saveResult.XmlSaved ? 1 : 0);
                    SessionStateInfo = $"Settings saved to {formatCount} format(s) (JSON: {saveResult.JsonSaved}, XML: {saveResult.XmlSaved})";
                    StatusMessage = "Vehicle settings saved successfully";
                }
                else
                {
                    SessionStateInfo = $"Save failed: {saveResult.ErrorMessage}";
                    StatusMessage = "Failed to save vehicle settings";
                }
            }
            else
            {
                SessionStateInfo = $"Validation failed: {string.Join(", ", updateResult.Errors)}";
                StatusMessage = "Invalid vehicle settings";
            }
        }
        catch (Exception ex)
        {
            SessionStateInfo = $"Error saving settings: {ex.Message}";
            StatusMessage = "Error saving vehicle settings";
        }
    }

    /// <summary>
    /// Get session state snapshot for display
    /// </summary>
    public System.Threading.Tasks.Task UpdateSessionStateDisplayAsync()
    {
        try
        {
            var sessionState = _sessionService.GetCurrentSessionState();

            if (sessionState != null)
            {
                SessionStateInfo = $"Field: {sessionState.CurrentFieldName ?? "None"} | " +
                                 $"User: {sessionState.UserProfileName} | " +
                                 $"Vehicle: {sessionState.VehicleProfileName} | " +
                                 $"Session: {(DateTime.UtcNow - sessionState.SessionStartTime).TotalMinutes:F0}m";
            }
            else
            {
                SessionStateInfo = "No active session";
            }
        }
        catch (Exception ex)
        {
            SessionStateInfo = $"Session error: {ex.Message}";
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// Switch vehicle profile using profile management service
    /// </summary>
    public async System.Threading.Tasks.Task SwitchVehicleProfileAsync(string profileName)
    {
        try
        {
            var result = await _profileService.SwitchVehicleProfileAsync(profileName, carryOverSession: true);

            if (result.Success)
            {
                CurrentVehicleProfile = profileName;
                StatusMessage = $"Switched to vehicle profile: {profileName}";

                // Reload settings from new profile
                await LoadVehicleSettingsAsync(profileName);
            }
            else
            {
                StatusMessage = $"Failed to switch profile: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error switching profile: {ex.Message}";
        }
    }

    // Expose services for UI access
    public IConfigurationService ConfigurationService => _configService;
    public ISessionManagementService SessionManagementService => _sessionService;
    public IProfileManagementService ProfileManagementService => _profileService;
}
