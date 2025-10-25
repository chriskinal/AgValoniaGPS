using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace AgValoniaGPS.Desktop.Services;

/// <summary>
/// Implementation of IPanelHostingService for managing dynamic panel docking.
/// </summary>
public class PanelHostingService : IPanelHostingService
{
    private readonly Dictionary<string, PanelRegistration> _panels = new();
    private readonly Dictionary<PanelDockLocation, Control> _dockContainers = new();

    /// <summary>
    /// Event raised when panel visibility changes.
    /// </summary>
    public event EventHandler<PanelVisibilityChangedEventArgs>? PanelVisibilityChanged;

    /// <summary>
    /// Initializes the panel hosting service with dock containers.
    /// </summary>
    /// <param name="panelLeft">Left dock container (Grid)</param>
    /// <param name="panelRight">Right dock container (StackPanel)</param>
    /// <param name="panelBottom">Bottom dock container (StackPanel)</param>
    /// <param name="panelNavigation">Navigation panel container (Border or Panel)</param>
    public void Initialize(Panel panelLeft, Panel panelRight, Panel panelBottom, Control panelNavigation)
    {
        _dockContainers[PanelDockLocation.Left] = panelLeft;
        _dockContainers[PanelDockLocation.Right] = panelRight;
        _dockContainers[PanelDockLocation.Bottom] = panelBottom;
        _dockContainers[PanelDockLocation.Navigation] = panelNavigation;
    }

    /// <summary>
    /// Registers a panel as dockable in a specific panel area.
    /// Immediately adds the button to the dock container (buttons are always visible).
    /// </summary>
    public void RegisterPanel(string panelId, PanelDockLocation location, Control panelControl)
    {
        var registration = new PanelRegistration
        {
            PanelId = panelId,
            Location = location,
            Control = panelControl,
            IsVisible = false
        };

        _panels[panelId] = registration;

        // Immediately add the button to its dock container
        // Buttons are always visible; IsVisible tracks whether the associated panel window is open
        if (!_dockContainers.TryGetValue(location, out var container))
            return;

        if (container is Grid grid)
        {
            // Find first available row in grid (for panelLeft)
            int row = FindAvailableRow(grid);
            Grid.SetRow(panelControl, row);
            grid.Children.Add(panelControl);
        }
        else if (container is StackPanel stackPanel)
        {
            // Add to stack panel (for panelRight, panelBottom)
            stackPanel.Children.Add(panelControl);
        }
        else if (container is Panel panel)
        {
            // For other panel types, just add to Children
            panel.Children.Add(panelControl);
        }
    }

    /// <summary>
    /// Marks a panel as visible/active (for state tracking).
    /// Note: Buttons are always visible; this tracks whether the panel window is open.
    /// </summary>
    public void ShowPanel(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var registration))
            return;

        if (registration.IsVisible)
            return;

        registration.IsVisible = true;
        PanelVisibilityChanged?.Invoke(this, new PanelVisibilityChangedEventArgs
        {
            PanelId = panelId,
            IsVisible = true,
            Location = registration.Location
        });
    }

    /// <summary>
    /// Marks a panel as hidden/inactive (for state tracking).
    /// Note: Buttons remain visible; this tracks whether the panel window is closed.
    /// </summary>
    public void HidePanel(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var registration))
            return;

        if (!registration.IsVisible)
            return;

        registration.IsVisible = false;
        PanelVisibilityChanged?.Invoke(this, new PanelVisibilityChangedEventArgs
        {
            PanelId = panelId,
            IsVisible = false,
            Location = registration.Location
        });
    }

    /// <summary>
    /// Toggles panel visibility.
    /// </summary>
    public void TogglePanel(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var registration))
            return;

        if (registration.IsVisible)
            HidePanel(panelId);
        else
            ShowPanel(panelId);
    }

    /// <summary>
    /// Checks if a panel is currently visible.
    /// </summary>
    public bool IsPanelVisible(string panelId)
    {
        return _panels.TryGetValue(panelId, out var registration) && registration.IsVisible;
    }

    /// <summary>
    /// Gets all panels in a specific dock location.
    /// </summary>
    public IEnumerable<string> GetPanelsInLocation(PanelDockLocation location)
    {
        return _panels.Values
            .Where(p => p.Location == location)
            .Select(p => p.PanelId);
    }

    /// <summary>
    /// Finds the first available row in a Grid that doesn't have children.
    /// </summary>
    private int FindAvailableRow(Grid grid)
    {
        // Find first row without children
        for (int i = 0; i < grid.RowDefinitions.Count; i++)
        {
            bool hasChild = grid.Children.Any(c => Grid.GetRow(c) == i);
            if (!hasChild)
                return i;
        }
        return 0; // Default to first row if all occupied
    }

    /// <summary>
    /// Internal class for tracking panel registrations.
    /// </summary>
    private class PanelRegistration
    {
        public string PanelId { get; set; } = string.Empty;
        public PanelDockLocation Location { get; set; }
        public Control Control { get; set; } = null!;
        public bool IsVisible { get; set; }
    }
}
