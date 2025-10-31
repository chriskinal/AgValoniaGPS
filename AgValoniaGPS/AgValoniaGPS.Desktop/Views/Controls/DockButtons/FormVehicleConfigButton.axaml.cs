using Avalonia.Controls;
using Avalonia.Interactivity;
using AgValoniaGPS.Desktop.Services;
using System;

namespace AgValoniaGPS.Desktop.Views.Controls.DockButtons;

/// <summary>
/// Dockable button for the Vehicle Config panel.
/// </summary>
public partial class FormVehicleConfigButton : UserControl
{
    private readonly IPanelHostingService _panelHostingService;
    private const string PanelId = "vehicleConfig";

    public FormVehicleConfigButton(IPanelHostingService panelHostingService)
    {
        _panelHostingService = panelHostingService ?? throw new ArgumentNullException(nameof(panelHostingService));
        InitializeComponent();

        // Subscribe to panel visibility changes
        _panelHostingService.PanelVisibilityChanged += OnPanelVisibilityChanged;
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
}
