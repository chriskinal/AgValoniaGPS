using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using AgValoniaGPS.Desktop.Views.Panels.Configuration;
using System;

namespace AgValoniaGPS.Desktop.Views.Controls.DockButtons;

/// <summary>
/// Dockable button for the AutoSteer Configuration panel.
/// Shows AutoSteer active/inactive status indicator (gray/green).
/// </summary>
public partial class FormSteerButton : UserControl
{
    private FormSteer? _panel;
    private bool _isActive;
    private bool _autoSteerActive;

    public FormSteerButton()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor with steering service for status indicators.
    /// </summary>
    public FormSteerButton(object? steeringService) : this()
    {
        // TODO: Subscribe to ISteeringCoordinatorService events when service is available
        // For now, default to inactive
        UpdateSteerStatus(false);
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
            _panel = new FormSteer();
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
