using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace AgValoniaGPS.Desktop.Services;

/// <summary>
/// Service for managing dynamic panel loading/unloading into docking panel areas.
/// Provides core functionality for the panel docking system.
/// </summary>
public interface IPanelHostingService
{
    /// <summary>
    /// Initializes the panel hosting service with dock containers.
    /// Must be called before registering or showing panels.
    /// </summary>
    /// <param name="panelLeft">Left dock container (Grid)</param>
    /// <param name="panelRight">Right dock container (StackPanel)</param>
    /// <param name="panelBottom">Bottom dock container (StackPanel)</param>
    /// <param name="panelNavigation">Navigation panel container (Border or Panel)</param>
    void Initialize(Panel panelLeft, Panel panelRight, Panel panelBottom, Control panelNavigation);

    /// <summary>
    /// Registers a panel as dockable in a specific panel area.
    /// </summary>
    /// <param name="panelId">Unique identifier for the panel</param>
    /// <param name="location">Dock location where the panel should appear</param>
    /// <param name="panelControl">The control to be displayed in the dock area</param>
    void RegisterPanel(string panelId, PanelDockLocation location, Control panelControl);

    /// <summary>
    /// Shows a panel in its registered dock location.
    /// Adds the control to the appropriate container (Grid row or StackPanel).
    /// </summary>
    /// <param name="panelId">Unique identifier of the panel to show</param>
    void ShowPanel(string panelId);

    /// <summary>
    /// Hides a panel from its dock location.
    /// Removes the control from its container.
    /// </summary>
    /// <param name="panelId">Unique identifier of the panel to hide</param>
    void HidePanel(string panelId);

    /// <summary>
    /// Toggles panel visibility.
    /// Shows the panel if hidden, hides if visible.
    /// </summary>
    /// <param name="panelId">Unique identifier of the panel to toggle</param>
    void TogglePanel(string panelId);

    /// <summary>
    /// Checks if a panel is currently visible.
    /// </summary>
    /// <param name="panelId">Unique identifier of the panel</param>
    /// <returns>True if the panel is visible, false otherwise</returns>
    bool IsPanelVisible(string panelId);

    /// <summary>
    /// Gets all panels in a specific dock location.
    /// </summary>
    /// <param name="location">The dock location to query</param>
    /// <returns>Collection of panel IDs in the specified location</returns>
    IEnumerable<string> GetPanelsInLocation(PanelDockLocation location);

    /// <summary>
    /// Event raised when panel visibility changes.
    /// </summary>
    event EventHandler<PanelVisibilityChangedEventArgs>? PanelVisibilityChanged;
}

/// <summary>
/// Defines the docking locations where panels can be placed.
/// </summary>
public enum PanelDockLocation
{
    /// <summary>
    /// Left dock area - Grid with 8 rows, 72px wide
    /// </summary>
    Left,

    /// <summary>
    /// Right dock area - StackPanel flowing bottom-up, 70px wide
    /// </summary>
    Right,

    /// <summary>
    /// Bottom dock area - StackPanel flowing right-to-left, 62px tall
    /// </summary>
    Bottom,

    /// <summary>
    /// Navigation panel - Floating panel (179×460px)
    /// </summary>
    Navigation
}

/// <summary>
/// Event arguments for panel visibility changes.
/// </summary>
public class PanelVisibilityChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the unique identifier of the panel.
    /// </summary>
    public string PanelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the panel is now visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the dock location of the panel.
    /// </summary>
    public PanelDockLocation Location { get; set; }
}
