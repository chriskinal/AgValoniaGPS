using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using AgValoniaGPS.Desktop.Services;
using System;

namespace AgValoniaGPS.Desktop.Views.Controls.DockButtons;

/// <summary>
/// Dockable button for the AutoSteer Configuration panel.
/// Shows AutoSteer active/inactive status indicator (gray/green).
/// </summary>
public partial class FormSteerButton : UserControl
{
    private readonly IPanelHostingService _panelHostingService;
    private bool _autoSteerActive;
    private const string PanelId = "steer";

    public FormSteerButton(IPanelHostingService panelHostingService, object? steeringService = null)
    {
        _panelHostingService = panelHostingService ?? throw new ArgumentNullException(nameof(panelHostingService));
        InitializeComponent();

        // Subscribe to panel visibility changes
        _panelHostingService.PanelVisibilityChanged += OnPanelVisibilityChanged;

        // TODO: Subscribe to ISteeringCoordinatorService events when service is available
        // For now, default to inactive
        UpdateSteerStatus(false);
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

    /// <summary>
    /// Updates the AutoSteer status indicator.
    /// </summary>
    /// <param name="isActive">True if AutoSteer is active, false otherwise</param>
    public void UpdateSteerStatus(bool isActive)
    {
        _autoSteerActive = isActive;
        var indicator = this.FindControl<Ellipse>("StatusIndicator");
        if (indicator == null) return;

        indicator.Fill = isActive
            ? new SolidColorBrush(Color.Parse("#27AE60")) // Green when active
            : new SolidColorBrush(Color.Parse("#95A5A6")); // Gray when inactive
    }
}
