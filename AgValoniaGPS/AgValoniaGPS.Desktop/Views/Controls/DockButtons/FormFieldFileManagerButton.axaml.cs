using Avalonia.Controls;
using Avalonia.Interactivity;
using AgValoniaGPS.Desktop.Views.Panels.FieldManagement;
using System;

namespace AgValoniaGPS.Desktop.Views.Controls.DockButtons;

/// <summary>
/// Dockable button for the Field File Manager panel.
/// </summary>
public partial class FormFieldFileManagerButton : UserControl
{
    private FormFieldFileManager? _panel;
    private bool _isActive;

    public FormFieldFileManagerButton()
    {
        InitializeComponent();
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
            _panel = new FormFieldFileManager();
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
}
