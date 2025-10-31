using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using AgValoniaGPS.Desktop.Services;
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
    private readonly IPanelHostingService _panelHostingService;
    private readonly IPositionUpdateService? _gpsService;
    private const string PanelId = "gpsData";

    public FormGPSDataButton(IPanelHostingService panelHostingService, IPositionUpdateService? gpsService = null)
    {
        _panelHostingService = panelHostingService ?? throw new ArgumentNullException(nameof(panelHostingService));
        _gpsService = gpsService;

        InitializeComponent();

        // Subscribe to panel visibility changes
        _panelHostingService.PanelVisibilityChanged += OnPanelVisibilityChanged;

        // Subscribe to GPS fix quality changes
        if (_gpsService != null)
        {
            _gpsService.PositionUpdated += OnGpsPositionUpdated;
            UpdateGpsStatus(GpsFixQuality.NoFix); // Initial state
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        _panelHostingService.TogglePanel(PanelId);
    }

    private void OnPanelVisibilityChanged(object? sender, PanelVisibilityChangedEventArgs e)
    {
        if (e.PanelId == PanelId)
        {
            UpdateButtonState(e.IsVisible);
        }
    }

    private void UpdateButtonState(bool isActive)
    {
        var button = this.FindControl<Button>("DockButton");
        if (button != null)
        {
            if (isActive)
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
