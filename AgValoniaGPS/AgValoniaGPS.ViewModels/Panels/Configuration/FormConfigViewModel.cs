using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.Services.Session;
using AgValoniaGPS.ViewModels.Base;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.Configuration;

/// <summary>
/// ViewModel for general application configuration panel.
/// Provides UI for display settings, GPS settings, auto-save, and system preferences.
/// </summary>
public class FormConfigViewModel : PanelViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly ISessionManagementService _sessionService;

    private string _displayUnits = "Metric";
    private int _fixUpdateRate = 10;
    private int _simulatedRollingAverage = 5;
    private bool _showRtkAge = true;
    private bool _showSatelliteCount = true;
    private int _autoSaveInterval = 5;
    private bool _keepAwakeWhileRunning = true;
    private string _language = "English";

    public FormConfigViewModel(
        IConfigurationService configService,
        ISessionManagementService sessionService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

        Title = "General Configuration";

        // Commands
        SaveConfigCommand = new RelayCommand(OnSaveConfig);
        LoadConfigCommand = new RelayCommand(OnLoadConfig);
        ExportConfigCommand = new RelayCommand(OnExportConfig);
        ImportConfigCommand = new RelayCommand(OnImportConfig);

        // Load current settings
        LoadCurrentSettings();
    }

    public string Title { get; } = "General Configuration";

    /// <summary>
    /// Display units (Metric / Imperial)
    /// </summary>
    public string DisplayUnits
    {
        get => _displayUnits;
        set => SetProperty(ref _displayUnits, value);
    }

    /// <summary>
    /// GPS fix update rate in Hz (10 / 20 / 25)
    /// </summary>
    public int FixUpdateRate
    {
        get => _fixUpdateRate;
        set => SetProperty(ref _fixUpdateRate, value);
    }

    /// <summary>
    /// Simulated rolling average points (1-10)
    /// </summary>
    public int SimulatedRollingAverage
    {
        get => _simulatedRollingAverage;
        set => SetProperty(ref _simulatedRollingAverage, Math.Clamp(value, 1, 10));
    }

    /// <summary>
    /// Show RTK age indicator
    /// </summary>
    public bool ShowRtkAge
    {
        get => _showRtkAge;
        set => SetProperty(ref _showRtkAge, value);
    }

    /// <summary>
    /// Show satellite count
    /// </summary>
    public bool ShowSatelliteCount
    {
        get => _showSatelliteCount;
        set => SetProperty(ref _showSatelliteCount, value);
    }

    /// <summary>
    /// Auto-save interval in minutes (1-60)
    /// </summary>
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => SetProperty(ref _autoSaveInterval, Math.Clamp(value, 1, 60));
    }

    /// <summary>
    /// Keep screen awake while running
    /// </summary>
    public bool KeepAwakeWhileRunning
    {
        get => _keepAwakeWhileRunning;
        set => SetProperty(ref _keepAwakeWhileRunning, value);
    }

    /// <summary>
    /// Application language
    /// </summary>
    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }

    public ICommand SaveConfigCommand { get; }
    public ICommand LoadConfigCommand { get; }
    public ICommand ExportConfigCommand { get; }
    public ICommand ImportConfigCommand { get; }

    private async void OnSaveConfig()
    {
        try
        {
            IsBusy = true;

            // Update configuration service with current settings
            var gpsSettings = _configService.GetGpsSettings();
            gpsSettings.Hz = FixUpdateRate;

            await _configService.UpdateGpsSettingsAsync(gpsSettings);

            // Save configuration to file
            var result = await _configService.SaveSettingsAsync();

            if (result.JsonSaved && result.XmlSaved)
            {
                ClearError();
            }
            else
            {
                SetError("Configuration saved with warnings. Check log for details.");
            }
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

    private async void OnLoadConfig()
    {
        try
        {
            IsBusy = true;

            // Load configuration from file
            var result = await _configService.LoadSettingsAsync("Default");

            if (result.Success)
            {
                LoadCurrentSettings();
                ClearError();
            }
            else
            {
                SetError($"Failed to load configuration: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load configuration: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnExportConfig()
    {
        try
        {
            // TODO: Implement configuration export to file dialog
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to export configuration: {ex.Message}");
        }
    }

    private void OnImportConfig()
    {
        try
        {
            // TODO: Implement configuration import from file dialog
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to import configuration: {ex.Message}");
        }
    }

    private void LoadCurrentSettings()
    {
        // Load from configuration service
        var gpsSettings = _configService.GetGpsSettings();
        FixUpdateRate = (int)gpsSettings.Hz;

        var displaySettings = _configService.GetDisplaySettings();
        // DisplayUnits, ShowRtkAge, ShowSatelliteCount would come from display settings

        var cultureSettings = _configService.GetCultureSettings();
        Language = cultureSettings.LanguageName ?? "English";
    }
}
