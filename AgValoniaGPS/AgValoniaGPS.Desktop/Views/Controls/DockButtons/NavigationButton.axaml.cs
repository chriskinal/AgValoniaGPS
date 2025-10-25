using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace AgValoniaGPS.Desktop.Views.Controls.DockButtons;

/// <summary>
/// Dockable button for showing/hiding the navigation panel.
/// Wave 10.5 Task Group 5: Navigation Panel Implementation
/// </summary>
public partial class NavigationButton : UserControl
{
    private bool _isActive;
    private Border? _navigationPanel;

    public NavigationButton()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event raised when the navigation button is clicked.
    /// The parent view should handle this to show/hide navigation panel.
    /// </summary>
    public event EventHandler<bool>? NavigationToggled;

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        // Find PanelNavigation if not already found
        if (_navigationPanel == null)
        {
            var mainWindow = GetMainWindow();
            _navigationPanel = mainWindow?.FindControl<Border>("PanelNavigation");
        }

        // Toggle panel visibility
        if (_navigationPanel != null)
        {
            _navigationPanel.IsVisible = !_navigationPanel.IsVisible;
            _isActive = _navigationPanel.IsVisible;

            // Update button visual state
            UpdateButtonState();

            // Raise event for external handlers
            NavigationToggled?.Invoke(this, _isActive);
        }
    }

    private void UpdateButtonState()
    {
        // Find the button within this UserControl
        var button = this.FindControl<Button>("DockButton");
        if (button == null)
        {
            // If button wasn't found by name, try finding the first Button child
            var stackPanel = this.Content as Button;
            button = stackPanel;
        }

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
    /// Gets the MainWindow instance from the visual tree.
    /// </summary>
    private MainWindow? GetMainWindow()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel as MainWindow;
    }

    /// <summary>
    /// Sets the navigation panel visibility state externally.
    /// </summary>
    public void SetNavigationState(bool isVisible)
    {
        _isActive = isVisible;
        UpdateButtonState();

        // Also update the actual panel if we have a reference
        if (_navigationPanel != null)
        {
            _navigationPanel.IsVisible = isVisible;
        }
    }
}
