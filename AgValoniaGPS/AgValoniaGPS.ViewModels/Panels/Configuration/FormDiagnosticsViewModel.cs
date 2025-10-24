using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.Configuration;

/// <summary>
/// ViewModel for system diagnostics panel.
/// Displays real-time system status, module states, and error logs.
/// </summary>
public class FormDiagnosticsViewModel : PanelViewModelBase
{
    private readonly IModuleCoordinatorService _moduleCoordinator;
    private readonly IPositionUpdateService _positionService;
    private readonly ISteeringCoordinatorService _steeringService;

    private double _gpsUpdateRate;
    private double _steeringLoopRate;
    private string _autoSteerModuleStatus = "Unknown";
    private string _machineModuleStatus = "Unknown";
    private string _imuModuleStatus = "Unknown";
    private string _lastErrorMessage = string.Empty;
    private double _memoryUsageMb;
    private double _cpuUsagePercent;

    public FormDiagnosticsViewModel(
        IModuleCoordinatorService moduleCoordinator,
        IPositionUpdateService positionService,
        ISteeringCoordinatorService steeringService)
    {
        _moduleCoordinator = moduleCoordinator ?? throw new ArgumentNullException(nameof(moduleCoordinator));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        _steeringService = steeringService ?? throw new ArgumentNullException(nameof(steeringService));

        Title = "System Diagnostics";

        ErrorLog = new ObservableCollection<string>();

        // Commands
        ClearErrorLogCommand = ReactiveCommand.Create(OnClearErrorLog);
        ExportDiagnosticsCommand = ReactiveCommand.Create(OnExportDiagnostics);
        RefreshStatusCommand = ReactiveCommand.Create(OnRefreshStatus);

        // Subscribe to module events
        _moduleCoordinator.ModuleConnected += OnModuleConnected;
        _moduleCoordinator.ModuleDisconnected += OnModuleDisconnected;
        _moduleCoordinator.ModuleReady += OnModuleReady;

        // Initial status refresh
        RefreshModuleStatus();
    }

    public string Title { get; } = "System Diagnostics";

    /// <summary>
    /// GPS update rate in Hz
    /// </summary>
    public double GpsUpdateRate
    {
        get => _gpsUpdateRate;
        set => this.RaiseAndSetIfChanged(ref _gpsUpdateRate, value);
    }

    /// <summary>
    /// Steering loop update rate in Hz
    /// </summary>
    public double SteeringLoopRate
    {
        get => _steeringLoopRate;
        set => this.RaiseAndSetIfChanged(ref _steeringLoopRate, value);
    }

    /// <summary>
    /// AutoSteer module connection status
    /// </summary>
    public string AutoSteerModuleStatus
    {
        get => _autoSteerModuleStatus;
        set => this.RaiseAndSetIfChanged(ref _autoSteerModuleStatus, value);
    }

    /// <summary>
    /// Machine module connection status
    /// </summary>
    public string MachineModuleStatus
    {
        get => _machineModuleStatus;
        set => this.RaiseAndSetIfChanged(ref _machineModuleStatus, value);
    }

    /// <summary>
    /// IMU module connection status
    /// </summary>
    public string ImuModuleStatus
    {
        get => _imuModuleStatus;
        set => this.RaiseAndSetIfChanged(ref _imuModuleStatus, value);
    }

    /// <summary>
    /// Last error message
    /// </summary>
    public string LastErrorMessage
    {
        get => _lastErrorMessage;
        set => this.RaiseAndSetIfChanged(ref _lastErrorMessage, value);
    }

    /// <summary>
    /// Memory usage in MB
    /// </summary>
    public double MemoryUsageMb
    {
        get => _memoryUsageMb;
        set => this.RaiseAndSetIfChanged(ref _memoryUsageMb, value);
    }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsagePercent
    {
        get => _cpuUsagePercent;
        set => this.RaiseAndSetIfChanged(ref _cpuUsagePercent, value);
    }

    /// <summary>
    /// Error log entries
    /// </summary>
    public ObservableCollection<string> ErrorLog { get; }

    public ICommand ClearErrorLogCommand { get; }
    public ICommand ExportDiagnosticsCommand { get; }
    public ICommand RefreshStatusCommand { get; }

    private void OnModuleConnected(object? sender, Models.Events.ModuleConnectedEventArgs e)
    {
        RefreshModuleStatus();
        AddErrorLog($"{e.ModuleType} module connected - {e.Transport} transport");
    }

    private void OnModuleDisconnected(object? sender, Models.Events.ModuleDisconnectedEventArgs e)
    {
        RefreshModuleStatus();
        AddErrorLog($"{e.ModuleType} module disconnected - {e.Reason}");
        LastErrorMessage = $"{e.ModuleType} disconnected: {e.Reason}";
    }

    private void OnModuleReady(object? sender, Models.Events.ModuleReadyEventArgs e)
    {
        RefreshModuleStatus();
        AddErrorLog($"{e.ModuleType} module ready");
    }

    private void OnClearErrorLog()
    {
        ErrorLog.Clear();
        LastErrorMessage = string.Empty;
        ClearError();
    }

    private void OnExportDiagnostics()
    {
        try
        {
            // TODO: Implement diagnostics export to file
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to export diagnostics: {ex.Message}");
        }
    }

    private void OnRefreshStatus()
    {
        RefreshModuleStatus();
        UpdateSystemMetrics();
    }

    private void RefreshModuleStatus()
    {
        // Get module states
        var autoSteerState = _moduleCoordinator.GetModuleState(ModuleType.AutoSteer);
        var machineState = _moduleCoordinator.GetModuleState(ModuleType.Machine);
        var imuState = _moduleCoordinator.GetModuleState(ModuleType.IMU);

        // Update status strings
        AutoSteerModuleStatus = FormatModuleStatus(autoSteerState);
        MachineModuleStatus = FormatModuleStatus(machineState);
        ImuModuleStatus = FormatModuleStatus(imuState);
    }

    private void UpdateSystemMetrics()
    {
        // Update GPS update rate (placeholder)
        GpsUpdateRate = 10.0; // TODO: Calculate actual GPS update rate

        // Update steering loop rate (placeholder)
        SteeringLoopRate = 20.0; // TODO: Calculate actual steering loop rate

        // Update memory usage (placeholder)
        var process = System.Diagnostics.Process.GetCurrentProcess();
        MemoryUsageMb = process.WorkingSet64 / (1024.0 * 1024.0);

        // CPU usage (placeholder - requires performance counter)
        CpuUsagePercent = 0.0; // TODO: Implement CPU usage tracking
    }

    private string FormatModuleStatus(ModuleState state)
    {
        return state switch
        {
            ModuleState.Ready => "Connected",
            ModuleState.HelloReceived => "Connecting",
            ModuleState.Connecting => "Connecting",
            ModuleState.Disconnected => "Disconnected",
            ModuleState.Timeout => "Timeout",
            ModuleState.Error => "Error",
            _ => "Unknown"
        };
    }

    private void AddErrorLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        ErrorLog.Insert(0, $"[{timestamp}] {message}");

        // Keep only last 100 entries
        while (ErrorLog.Count > 100)
        {
            ErrorLog.RemoveAt(ErrorLog.Count - 1);
        }
    }
}
