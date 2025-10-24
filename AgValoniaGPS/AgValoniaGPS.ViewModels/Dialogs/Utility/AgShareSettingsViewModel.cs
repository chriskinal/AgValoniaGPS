using System;
using System.Reactive;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for AgShare configuration settings.
/// AgShare is a cloud service for sharing field data, boundaries, and guidance lines.
/// </summary>
public class AgShareSettingsViewModel : DialogViewModelBase
{
    private bool _enableAgShare;
    private string _serverUrl = "https://agshare.example.com";
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _autoSync;
    private bool _syncFields = true;
    private bool _syncBoundaries = true;
    private bool _syncGuidanceLines = true;
    private bool _syncFlags = true;
    private int _syncIntervalMinutes = 15;
    private bool _isConnected;
    private DateTime? _lastSyncTime;
    private bool _useSSL = true;
    private int _port = 443;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgShareSettingsViewModel"/> class.
    /// </summary>
    public AgShareSettingsViewModel()
    {
        TestConnectionCommand = ReactiveCommand.Create(OnTestConnection);
        SyncNowCommand = ReactiveCommand.Create(OnSyncNow, this.WhenAnyValue(x => x.EnableAgShare, x => x.IsConnected, (enabled, connected) => enabled && connected));

        // TODO: Inject IConfigurationService to load/save settings

        // Subscribe to enable changes
        this.WhenAnyValue(x => x.EnableAgShare)
            .Subscribe(enabled => OnEnableChanged(enabled));
    }

    /// <summary>
    /// Gets or sets whether AgShare is enabled.
    /// </summary>
    public bool EnableAgShare
    {
        get => _enableAgShare;
        set => this.RaiseAndSetIfChanged(ref _enableAgShare, value);
    }

    /// <summary>
    /// Gets or sets the AgShare server URL.
    /// </summary>
    public string ServerUrl
    {
        get => _serverUrl;
        set => this.RaiseAndSetIfChanged(ref _serverUrl, value);
    }

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    /// <summary>
    /// Gets or sets whether to automatically sync at intervals.
    /// </summary>
    public bool AutoSync
    {
        get => _autoSync;
        set => this.RaiseAndSetIfChanged(ref _autoSync, value);
    }

    /// <summary>
    /// Gets or sets whether to sync field data.
    /// </summary>
    public bool SyncFields
    {
        get => _syncFields;
        set => this.RaiseAndSetIfChanged(ref _syncFields, value);
    }

    /// <summary>
    /// Gets or sets whether to sync boundaries.
    /// </summary>
    public bool SyncBoundaries
    {
        get => _syncBoundaries;
        set => this.RaiseAndSetIfChanged(ref _syncBoundaries, value);
    }

    /// <summary>
    /// Gets or sets whether to sync guidance lines.
    /// </summary>
    public bool SyncGuidanceLines
    {
        get => _syncGuidanceLines;
        set => this.RaiseAndSetIfChanged(ref _syncGuidanceLines, value);
    }

    /// <summary>
    /// Gets or sets whether to sync flags.
    /// </summary>
    public bool SyncFlags
    {
        get => _syncFlags;
        set => this.RaiseAndSetIfChanged(ref _syncFlags, value);
    }

    /// <summary>
    /// Gets or sets the sync interval in minutes.
    /// </summary>
    public int SyncIntervalMinutes
    {
        get => _syncIntervalMinutes;
        set => this.RaiseAndSetIfChanged(ref _syncIntervalMinutes, value);
    }

    /// <summary>
    /// Gets or sets whether currently connected to AgShare.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            this.RaiseAndSetIfChanged(ref _isConnected, value);
            this.RaisePropertyChanged(nameof(ConnectionStatusText));
        }
    }

    /// <summary>
    /// Gets the connection status text.
    /// </summary>
    public string ConnectionStatusText => IsConnected ? "Connected" : "Not Connected";

    /// <summary>
    /// Gets or sets the last successful sync time.
    /// </summary>
    public DateTime? LastSyncTime
    {
        get => _lastSyncTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _lastSyncTime, value);
            this.RaisePropertyChanged(nameof(LastSyncText));
        }
    }

    /// <summary>
    /// Gets the last sync time text.
    /// </summary>
    public string LastSyncText => LastSyncTime.HasValue
        ? $"Last sync: {LastSyncTime.Value:yyyy-MM-dd HH:mm:ss}"
        : "Never synchronized";

    /// <summary>
    /// Gets or sets whether to use SSL/TLS.
    /// </summary>
    public bool UseSSL
    {
        get => _useSSL;
        set => this.RaiseAndSetIfChanged(ref _useSSL, value);
    }

    /// <summary>
    /// Gets or sets the server port.
    /// </summary>
    public int Port
    {
        get => _port;
        set => this.RaiseAndSetIfChanged(ref _port, value);
    }

    /// <summary>
    /// Gets the command to test the connection.
    /// </summary>
    public ReactiveCommand<Unit, Unit> TestConnectionCommand { get; }

    /// <summary>
    /// Gets the command to sync now.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SyncNowCommand { get; }

    /// <summary>
    /// Tests the connection to AgShare server.
    /// </summary>
    private void OnTestConnection()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
        {
            SetError("Please enter a server URL.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter username and password.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            // TODO: Implement actual connection test
            // For now, simulate a successful connection
            System.Threading.Thread.Sleep(1000); // Simulate network delay
            IsConnected = true;

            IsBusy = false;
        }
        catch (Exception ex)
        {
            IsBusy = false;
            IsConnected = false;
            SetError($"Connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs an immediate sync.
    /// </summary>
    private void OnSyncNow()
    {
        try
        {
            IsBusy = true;
            ClearError();

            // TODO: Implement actual sync logic
            System.Threading.Thread.Sleep(2000); // Simulate sync
            LastSyncTime = DateTime.Now;

            IsBusy = false;
        }
        catch (Exception ex)
        {
            IsBusy = false;
            SetError($"Sync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when enable state changes.
    /// </summary>
    private void OnEnableChanged(bool enabled)
    {
        if (!enabled)
        {
            IsConnected = false;
        }
    }

    /// <summary>
    /// Validates settings before accepting.
    /// </summary>
    protected override void OnOK()
    {
        if (EnableAgShare)
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                SetError("Server URL is required when AgShare is enabled.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                SetError("Username and password are required when AgShare is enabled.");
                return;
            }

            if (Port <= 0 || Port > 65535)
            {
                SetError("Port must be between 1 and 65535.");
                return;
            }

            if (AutoSync && (SyncIntervalMinutes < 1 || SyncIntervalMinutes > 1440))
            {
                SetError("Sync interval must be between 1 and 1440 minutes.");
                return;
            }
        }

        // TODO: Save settings via IConfigurationService

        ClearError();
        base.OnOK();
    }
}
