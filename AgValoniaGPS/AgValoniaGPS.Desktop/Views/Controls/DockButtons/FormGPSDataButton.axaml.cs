using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using AgValoniaGPS.Desktop.Views.Panels.Display;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Models.Enums;
using System;

namespace AgValoniaGPS.Desktop.Views.Controls.DockButtons;

/// <summary>
/// Dockable button for the GPS Data panel.
/// Shows GPS fix quality status indicator (red/yellow/green).
/// </summary>
public partial class FormGPSDataButton : UserControl
{
    private FormGPSData? _panel;
    private readonly IPositionUpdateService? _gpsService;
    private bool _isActive;

    public FormGPSDataButton()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor with GPS service for status indicators.
    /// </summary>
    public FormGPSDataButton(IPositionUpdateService? gpsService) : this()
    {
        _gpsService = gpsService;

        if (_gpsService != null)
        {
            // Subscribe to GPS fix quality changes
            _gpsService.PositionUpdated += OnGpsPositionUpdated;
            UpdateGpsStatus(GpsFixQuality.NoFix); // Initial state
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_panel == null || !_panel.IsVisible)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }

    private void ShowPanel()
    {
        if (_panel == null)
        {
            _panel = new FormGPSData();
            // Wire up close event
            if (_panel.DataContext is ViewModels.Base.PanelViewModelBase vm)
            {
                vm.CloseRequested += OnPanelCloseRequested;
            }
        }

        _panel.IsVisible = true;
        _isActive = true;
        UpdateButtonState();
    }

    private void HidePanel()
    {
        if (_panel != null)
        {
            _panel.IsVisible = false;
        }
        _isActive = false;
        UpdateButtonState();
    }

    private void OnPanelCloseRequested(object? sender, EventArgs e)
    {
        HidePanel();
    }

    private void UpdateButtonState()
    {
        var button = this.FindControl<Button>("DockButton");
        if (button != null)
        {
            if (_isActive)
            {
                button.Classes.Add("Active");
            }
            else
            {
                button.Classes.Remove("Active");
            }
        }
    }

    private void OnGpsPositionUpdated(object? sender, PositionUpdateEventArgs e)
    {
        // TODO: Determine fix quality from GPS service once accuracy property is available
        // For now, default to NoFix until GPS quality information is integrated
        GpsFixQuality quality = GpsFixQuality.NoFix;

        UpdateGpsStatus(quality);
    }

    private void UpdateGpsStatus(GpsFixQuality quality)
    {
        var indicator = this.FindControl<Ellipse>("StatusIndicator");
        if (indicator == null) return;

        indicator.Fill = quality switch
        {
            GpsFixQuality.NoFix => new SolidColorBrush(Color.Parse("#E74C3C")), // Red
            GpsFixQuality.RTKFloat => new SolidColorBrush(Color.Parse("#F39C12")), // Yellow
            GpsFixQuality.RTKFixed => new SolidColorBrush(Color.Parse("#27AE60")), // Green
            _ => new SolidColorBrush(Color.Parse("#E74C3C")) // Default red
        };
    }
}
