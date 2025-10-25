using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.Services.Session;
using AgValoniaGPS.Services.Profile;
using AgValoniaGPS.Services.UI;
using AgValoniaGPS.ViewModels.Panels.Display;
using AgValoniaGPS.ViewModels.Panels.FieldManagement;
using AgValoniaGPS.ViewModels.Panels.FieldOperations;
using AgValoniaGPS.ViewModels.Panels.Guidance;
using AgValoniaGPS.ViewModels.Panels.Configuration;
using Avalonia.Threading;

namespace AgValoniaGPS.ViewModels;

public class MainViewModel : ObservableObject
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

    // Wave 9 UI Services
    private readonly IDialogService _dialogService;

    // Wave 10 Panel ViewModels
    public FormGPSViewModel GPSVM { get; } // Main GPS view overlay
    public FormFieldDataViewModel FieldDataVM { get; }
    public FormGPSDataViewModel GPSDataVM { get; }
    public FormTramLineViewModel TramLineVM { get; }
    public FormQuickABViewModel QuickABVM { get; }
    public FormSteerViewModel SteerVM { get; }
    public FormConfigViewModel ConfigVM { get; }
    public FormDiagnosticsViewModel DiagnosticsVM { get; }
    public FormRollCorrectionViewModel RollCorrectionVM { get; }
    public FormVehicleConfigViewModel VehicleConfigVM { get; }
    public FormFlagsViewModel FlagsVM { get; }
    public FormCameraViewModel CameraVM { get; }
    public FormBoundaryEditorViewModel BoundaryEditorVM { get; }
    public FormFieldToolsViewModel FieldToolsVM { get; }
    public FormFieldFileManagerViewModel FieldFileManagerVM { get; }

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
        IProfileManagementService profileService,
        IDialogService dialogService,
        FormGPSViewModel gpsVM,
        FormFieldDataViewModel fieldDataVM,
        FormGPSDataViewModel gpsDataVM,
        FormTramLineViewModel tramLineVM,
        FormQuickABViewModel quickABVM,
        FormSteerViewModel steerVM,
        FormConfigViewModel configVM,
        FormDiagnosticsViewModel diagnosticsVM,
        FormRollCorrectionViewModel rollCorrectionVM,
        FormVehicleConfigViewModel vehicleConfigVM,
        FormFlagsViewModel flagsVM,
        FormCameraViewModel cameraVM,
        FormBoundaryEditorViewModel boundaryEditorVM,
        FormFieldToolsViewModel fieldToolsVM,
        FormFieldFileManagerViewModel fieldFileManagerVM)
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
        _dialogService = dialogService;
        _nmeaParser = new NmeaParserService(gpsService);

        // Store panel ViewModels
        GPSVM = gpsVM;
        FieldDataVM = fieldDataVM;
        GPSDataVM = gpsDataVM;
        TramLineVM = tramLineVM;
        QuickABVM = quickABVM;
        SteerVM = steerVM;
        ConfigVM = configVM;
        DiagnosticsVM = diagnosticsVM;
        RollCorrectionVM = rollCorrectionVM;
        VehicleConfigVM = vehicleConfigVM;
        FlagsVM = flagsVM;
        CameraVM = cameraVM;
        BoundaryEditorVM = boundaryEditorVM;
        FieldToolsVM = fieldToolsVM;
        FieldFileManagerVM = fieldFileManagerVM;

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

        // Subscribe to panel CloseRequested events
        fieldDataVM.CloseRequested += (_, _) => IsFieldDataPanelVisible = false;
        gpsDataVM.CloseRequested += (_, _) => IsGPSDataPanelVisible = false;
        tramLineVM.CloseRequested += (_, _) => IsTramLinePanelVisible = false;
        quickABVM.CloseRequested += (_, _) => IsQuickABPanelVisible = false;
        steerVM.CloseRequested += (_, _) => IsSteerConfigPanelVisible = false;
        configVM.CloseRequested += (_, _) => IsGeneralConfigPanelVisible = false;
        diagnosticsVM.CloseRequested += (_, _) => IsDiagnosticsPanelVisible = false;
        rollCorrectionVM.CloseRequested += (_, _) => IsRollCorrectionPanelVisible = false;
        vehicleConfigVM.CloseRequested += (_, _) => IsVehicleConfigPanelVisible = false;
        flagsVM.CloseRequested += (_, _) => IsFlagsPanelVisible = false;
        cameraVM.CloseRequested += (_, _) => IsCameraPanelVisible = false;
        boundaryEditorVM.CloseRequested += (_, _) => IsBoundaryEditorPanelVisible = false;
        fieldToolsVM.CloseRequested += (_, _) => IsFieldToolsPanelVisible = false;
        fieldFileManagerVM.CloseRequested += (_, _) => IsFieldFileManagerPanelVisible = false;

        // Initialize panel toggle commands
        InitializePanelToggleCommands();

        // Initialize menu commands
        InitializeMenuCommands();

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
        set => SetProperty(ref _statusMessage, value);
    }

    public double Latitude
    {
        get => _latitude;
        set => SetProperty(ref _latitude, value);
    }

    public double Longitude
    {
        get => _longitude;
        set => SetProperty(ref _longitude, value);
    }

    public double Speed
    {
        get => _speed;
        set => SetProperty(ref _speed, value);
    }

    public int SatelliteCount
    {
        get => _satelliteCount;
        set => SetProperty(ref _satelliteCount, value);
    }

    public string FixQuality
    {
        get => _fixQuality;
        set => SetProperty(ref _fixQuality, value);
    }

    public string NetworkStatus
    {
        get => _networkStatus;
        set => SetProperty(ref _networkStatus, value);
    }

    // AutoSteer Hello and Data properties
    public bool IsAutoSteerHelloOk
    {
        get => _isAutoSteerHelloOk;
        set => SetProperty(ref _isAutoSteerHelloOk, value);
    }

    public bool IsAutoSteerDataOk
    {
        get => _isAutoSteerDataOk;
        set => SetProperty(ref _isAutoSteerDataOk, value);
    }

    // Machine Hello and Data properties
    public bool IsMachineHelloOk
    {
        get => _isMachineHelloOk;
        set => SetProperty(ref _isMachineHelloOk, value);
    }

    public bool IsMachineDataOk
    {
        get => _isMachineDataOk;
        set => SetProperty(ref _isMachineDataOk, value);
    }

    // IMU Hello and Data properties
    public bool IsImuHelloOk
    {
        get => _isImuHelloOk;
        set => SetProperty(ref _isImuHelloOk, value);
    }

    public bool IsImuDataOk
    {
        get => _isImuDataOk;
        set => SetProperty(ref _isImuDataOk, value);
    }

    // GPS Hello and Data properties (GPS doesn't have hello, just data from NMEA)
    public bool IsGpsDataOk
    {
        get => _isGpsDataOk;
        set => SetProperty(ref _isGpsDataOk, value);
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
        set => SetProperty(ref _isNtripConnected, value);
    }

    public string NtripStatus
    {
        get => _ntripStatus;
        set => SetProperty(ref _ntripStatus, value);
    }

    public string NtripBytesReceived
    {
        get => $"{(_ntripBytesReceived / 1024):N0} KB";
    }

    public string NtripCasterAddress
    {
        get => _ntripCasterAddress;
        set => SetProperty(ref _ntripCasterAddress, value);
    }

    public int NtripCasterPort
    {
        get => _ntripCasterPort;
        set => SetProperty(ref _ntripCasterPort, value);
    }

    public string NtripMountPoint
    {
        get => _ntripMountPoint;
        set => SetProperty(ref _ntripMountPoint, value);
    }

    public string NtripUsername
    {
        get => _ntripUsername;
        set => SetProperty(ref _ntripUsername, value);
    }

    public string NtripPassword
    {
        get => _ntripPassword;
        set => SetProperty(ref _ntripPassword, value);
    }

    public string DebugLog
    {
        get => _debugLog;
        set => SetProperty(ref _debugLog, value);
    }

    public double Easting
    {
        get => _easting;
        set => SetProperty(ref _easting, value);
    }

    public double Northing
    {
        get => _northing;
        set => SetProperty(ref _northing, value);
    }

    public double Heading
    {
        get => _heading;
        set => SetProperty(ref _heading, value);
    }

    public string HeadingSource
    {
        get => _headingSource;
        set => SetProperty(ref _headingSource, value);
    }

    public bool IsReversing
    {
        get => _isReversing;
        set => SetProperty(ref _isReversing, value);
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
        OnPropertyChanged(nameof(NtripBytesReceived));
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
        set => SetProperty(ref _activeField, value);
    }

    public string FieldsRootDirectory
    {
        get => _fieldsRootDirectory;
        set => SetProperty(ref _fieldsRootDirectory, value);
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
        OnPropertyChanged(nameof(ActiveFieldName));
        OnPropertyChanged(nameof(ActiveFieldArea));

        // Update field statistics service with new boundary
        if (field?.Boundary != null)
        {
            _fieldStatistics.UpdateBoundaryArea(field.Boundary);
            OnPropertyChanged(nameof(BoundaryAreaDisplay));
            OnPropertyChanged(nameof(WorkedAreaDisplay));
            OnPropertyChanged(nameof(RemainingPercent));
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
        set => SetProperty(ref _currentVehicleProfile, value);
    }

    public string CurrentUserProfile
    {
        get => _currentUserProfile;
        set => SetProperty(ref _currentUserProfile, value);
    }

    public string SessionStateInfo
    {
        get => _sessionStateInfo;
        set => SetProperty(ref _sessionStateInfo, value);
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

    // ========== Wave 10 Panel Visibility Management ==========

    // Task Group 1: Core Operations Panels
    private bool _isFieldDataPanelVisible;
    private bool _isGPSDataPanelVisible;
    private bool _isTramLinePanelVisible;
    private bool _isQuickABPanelVisible;

    // Task Group 2: Configuration Panels
    private bool _isSteerConfigPanelVisible;
    private bool _isGeneralConfigPanelVisible;
    private bool _isDiagnosticsPanelVisible;
    private bool _isRollCorrectionPanelVisible;
    private bool _isVehicleConfigPanelVisible;

    // Task Group 3: Field Management Panels
    private bool _isFlagsPanelVisible;
    private bool _isCameraPanelVisible;
    private bool _isBoundaryEditorPanelVisible;
    private bool _isFieldToolsPanelVisible;
    private bool _isFieldFileManagerPanelVisible;

    public bool IsFieldDataPanelVisible
    {
        get => _isFieldDataPanelVisible;
        set => SetProperty(ref _isFieldDataPanelVisible, value);
    }

    public bool IsGPSDataPanelVisible
    {
        get => _isGPSDataPanelVisible;
        set => SetProperty(ref _isGPSDataPanelVisible, value);
    }

    public bool IsTramLinePanelVisible
    {
        get => _isTramLinePanelVisible;
        set => SetProperty(ref _isTramLinePanelVisible, value);
    }

    public bool IsQuickABPanelVisible
    {
        get => _isQuickABPanelVisible;
        set => SetProperty(ref _isQuickABPanelVisible, value);
    }

    public bool IsSteerConfigPanelVisible
    {
        get => _isSteerConfigPanelVisible;
        set => SetProperty(ref _isSteerConfigPanelVisible, value);
    }

    public bool IsGeneralConfigPanelVisible
    {
        get => _isGeneralConfigPanelVisible;
        set => SetProperty(ref _isGeneralConfigPanelVisible, value);
    }

    public bool IsDiagnosticsPanelVisible
    {
        get => _isDiagnosticsPanelVisible;
        set => SetProperty(ref _isDiagnosticsPanelVisible, value);
    }

    public bool IsRollCorrectionPanelVisible
    {
        get => _isRollCorrectionPanelVisible;
        set => SetProperty(ref _isRollCorrectionPanelVisible, value);
    }

    public bool IsVehicleConfigPanelVisible
    {
        get => _isVehicleConfigPanelVisible;
        set => SetProperty(ref _isVehicleConfigPanelVisible, value);
    }

    public bool IsFlagsPanelVisible
    {
        get => _isFlagsPanelVisible;
        set => SetProperty(ref _isFlagsPanelVisible, value);
    }

    public bool IsCameraPanelVisible
    {
        get => _isCameraPanelVisible;
        set => SetProperty(ref _isCameraPanelVisible, value);
    }

    public bool IsBoundaryEditorPanelVisible
    {
        get => _isBoundaryEditorPanelVisible;
        set => SetProperty(ref _isBoundaryEditorPanelVisible, value);
    }

    public bool IsFieldToolsPanelVisible
    {
        get => _isFieldToolsPanelVisible;
        set => SetProperty(ref _isFieldToolsPanelVisible, value);
    }

    public bool IsFieldFileManagerPanelVisible
    {
        get => _isFieldFileManagerPanelVisible;
        set => SetProperty(ref _isFieldFileManagerPanelVisible, value);
    }

    // Panel Toggle Commands (initialized in constructor to fix threading issues)
    public ICommand ToggleFieldDataPanelCommand { get; private set; } = null!;
    public ICommand ToggleGPSDataPanelCommand { get; private set; } = null!;
    public ICommand ToggleTramLinePanelCommand { get; private set; } = null!;
    public ICommand ToggleQuickABPanelCommand { get; private set; } = null!;
    public ICommand ToggleSteerConfigPanelCommand { get; private set; } = null!;
    public ICommand ToggleGeneralConfigPanelCommand { get; private set; } = null!;
    public ICommand ToggleDiagnosticsPanelCommand { get; private set; } = null!;
    public ICommand ToggleRollCorrectionPanelCommand { get; private set; } = null!;
    public ICommand ToggleVehicleConfigPanelCommand { get; private set; } = null!;
    public ICommand ToggleFlagsPanelCommand { get; private set; } = null!;
    public ICommand ToggleCameraPanelCommand { get; private set; } = null!;
    public ICommand ToggleBoundaryEditorPanelCommand { get; private set; } = null!;
    public ICommand ToggleFieldToolsPanelCommand { get; private set; } = null!;
    public ICommand ToggleFieldFileManagerPanelCommand { get; private set; } = null!;

    /// <summary>
    /// Initialize panel toggle commands.
    /// Uses CommunityToolkit.Mvvm RelayCommand which has no threading issues.
    /// </summary>
    private void InitializePanelToggleCommands()
    {
        // Create commands using RelayCommand (no UI thread dependency)
        ToggleFieldDataPanelCommand = new RelayCommand(() => IsFieldDataPanelVisible = !IsFieldDataPanelVisible);
        ToggleGPSDataPanelCommand = new RelayCommand(() => IsGPSDataPanelVisible = !IsGPSDataPanelVisible);
        ToggleTramLinePanelCommand = new RelayCommand(() => IsTramLinePanelVisible = !IsTramLinePanelVisible);
        ToggleQuickABPanelCommand = new RelayCommand(() => IsQuickABPanelVisible = !IsQuickABPanelVisible);
        ToggleSteerConfigPanelCommand = new RelayCommand(() => IsSteerConfigPanelVisible = !IsSteerConfigPanelVisible);
        ToggleGeneralConfigPanelCommand = new RelayCommand(() => IsGeneralConfigPanelVisible = !IsGeneralConfigPanelVisible);
        ToggleDiagnosticsPanelCommand = new RelayCommand(() => IsDiagnosticsPanelVisible = !IsDiagnosticsPanelVisible);
        ToggleRollCorrectionPanelCommand = new RelayCommand(() => IsRollCorrectionPanelVisible = !IsRollCorrectionPanelVisible);
        ToggleVehicleConfigPanelCommand = new RelayCommand(() => IsVehicleConfigPanelVisible = !IsVehicleConfigPanelVisible);
        ToggleFlagsPanelCommand = new RelayCommand(() => IsFlagsPanelVisible = !IsFlagsPanelVisible);
        ToggleCameraPanelCommand = new RelayCommand(() => IsCameraPanelVisible = !IsCameraPanelVisible);
        ToggleBoundaryEditorPanelCommand = new RelayCommand(() => IsBoundaryEditorPanelVisible = !IsBoundaryEditorPanelVisible);
        ToggleFieldToolsPanelCommand = new RelayCommand(() => IsFieldToolsPanelVisible = !IsFieldToolsPanelVisible);
        ToggleFieldFileManagerPanelCommand = new RelayCommand(() => IsFieldFileManagerPanelVisible = !IsFieldFileManagerPanelVisible);
    }

    // ========== Wave 10.5 Menu Commands ==========

    // Menu command properties
    public ICommand NewProfileCommand { get; private set; } = null!;
    public ICommand LoadProfileCommand { get; private set; } = null!;
    public ICommand LanguageCommand { get; private set; } = null!;
    public ICommand ToggleSimulatorCommand { get; private set; } = null!;
    public ICommand EnterSimCoordsCommand { get; private set; } = null!;
    public ICommand KioskModeCommand { get; private set; } = null!;
    public ICommand ResetToDefaultCommand { get; private set; } = null!;
    public ICommand AboutCommand { get; private set; } = null!;
    public ICommand AgShareApiCommand { get; private set; } = null!;

    /// <summary>
    /// Initialize menu commands (Wave 10.5 Task 1.2)
    /// Stubs for now - full implementation will come in later task groups
    /// </summary>
    private void InitializeMenuCommands()
    {
        // File Menu commands
        NewProfileCommand = new RelayCommand(() => StatusMessage = "New Profile - Not yet implemented");
        LoadProfileCommand = new RelayCommand(() => StatusMessage = "Load Profile - Not yet implemented");
        LanguageCommand = new RelayCommand(() => StatusMessage = "Language - Not yet implemented");
        ToggleSimulatorCommand = new RelayCommand(() => StatusMessage = "Toggle Simulator - Not yet implemented");
        EnterSimCoordsCommand = new RelayCommand(() => StatusMessage = "Enter Sim Coords - Not yet implemented");
        KioskModeCommand = new RelayCommand(() => StatusMessage = "Kiosk Mode - Not yet implemented");
        ResetToDefaultCommand = new RelayCommand(() => StatusMessage = "Reset To Default - Not yet implemented");
        AboutCommand = new RelayCommand(() => StatusMessage = "About - Not yet implemented");
        AgShareApiCommand = new RelayCommand(() => StatusMessage = "AgShare API - Not yet implemented");

        InitializeNavigationCommands();
    }


    // ========== Wave 10.5 Task Group 5: Navigation Panel Commands ==========

    // Navigation Panel Commands
    public ICommand TiltDownCommand { get; private set; } = null!;
    public ICommand TiltUpCommand { get; private set; } = null!;
    public ICommand Camera2DCommand { get; private set; } = null!;
    public ICommand Camera3DCommand { get; private set; } = null!;
    public ICommand CameraNorth2DCommand { get; private set; } = null!;
    public ICommand ToggleGridCommand { get; private set; } = null!;
    public ICommand ToggleDayNightCommand { get; private set; } = null!;
    public ICommand BrightnessDownCommand { get; private set; } = null!;
    public ICommand BrightnessUpCommand { get; private set; } = null!;

    /// <summary>
    /// Initialize navigation panel commands (Wave 10.5 Task Group 5)
    /// Stubs for now - full implementation will come in Wave 11 (OpenGL)
    /// </summary>
    private void InitializeNavigationCommands()
    {
        // Camera tilt controls
        TiltDownCommand = new RelayCommand(() => StatusMessage = "Camera Tilt Down - Not yet implemented (Wave 11)");
        TiltUpCommand = new RelayCommand(() => StatusMessage = "Camera Tilt Up - Not yet implemented (Wave 11)");

        // Camera view mode controls
        Camera2DCommand = new RelayCommand(() => StatusMessage = "2D View Mode - Not yet implemented (Wave 11)");
        Camera3DCommand = new RelayCommand(() => StatusMessage = "3D View Mode - Not yet implemented (Wave 11)");
        CameraNorth2DCommand = new RelayCommand(() => StatusMessage = "North-Locked 2D View - Not yet implemented (Wave 11)");

        // Display controls
        ToggleGridCommand = new RelayCommand(() => StatusMessage = "Toggle Grid Display - Not yet implemented (Wave 11)");
        ToggleDayNightCommand = new RelayCommand(() => StatusMessage = "Toggle Day/Night Mode - Not yet implemented (Wave 11)");

        // Brightness controls
        BrightnessDownCommand = new RelayCommand(() => StatusMessage = "Decrease Brightness - Not yet implemented (Wave 11)");
        BrightnessUpCommand = new RelayCommand(() => StatusMessage = "Increase Brightness - Not yet implemented (Wave 11)");
    }

    // ========== Wave 9 Task 8.1: Dialog Launching Methods ==========

    /// <summary>
    /// Show Settings dialog (general application configuration)
    /// </summary>
    public async System.Threading.Tasks.Task ShowSettingsDialogAsync()
    {
        // TODO: Implement settings dialog launch
        await System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// Show Field Picker dialog workflow (select field directory and field)
    /// </summary>
    public async System.Threading.Tasks.Task<Field?> ShowFieldPickerWorkflowAsync()
    {
        // TODO: Implement field picker workflow
        // 1. Show FormFieldDir to select directory
        // 2. Show FormFieldExisting to select field from directory
        // 3. Return selected field
        return await System.Threading.Tasks.Task.FromResult<Field?>(null);
    }

    /// <summary>
    /// Expose DialogService for advanced UI scenarios
    /// </summary>
    public IDialogService DialogService => _dialogService;
}
