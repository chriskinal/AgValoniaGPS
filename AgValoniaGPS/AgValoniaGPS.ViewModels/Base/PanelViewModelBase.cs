using ReactiveUI;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Base;

/// <summary>
/// Base class for panel ViewModels that can be shown/hidden in the UI.
/// Unlike DialogViewModelBase, panels are meant to be displayed alongside the main content
/// and can be collapsed, expanded, or pinned.
/// </summary>
public abstract class PanelViewModelBase : ViewModelBase
{
    private bool _isExpanded = true;
    private bool _isPinned;
    private bool _canCollapse = true;

    /// <summary>
    /// Gets or sets whether the panel is expanded.
    /// When collapsed, the panel may show only a header or be completely hidden.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary>
    /// Gets or sets whether the panel is pinned (cannot be auto-closed).
    /// Pinned panels remain open even when other panels are opened.
    /// </summary>
    public bool IsPinned
    {
        get => _isPinned;
        set => this.RaiseAndSetIfChanged(ref _isPinned, value);
    }

    /// <summary>
    /// Gets or sets whether the panel can be collapsed.
    /// Some critical panels may not allow collapsing.
    /// </summary>
    public bool CanCollapse
    {
        get => _canCollapse;
        set => this.RaiseAndSetIfChanged(ref _canCollapse, value);
    }

    /// <summary>
    /// Command to close/hide the panel.
    /// </summary>
    public ICommand CloseCommand { get; }

    /// <summary>
    /// Event raised when panel requests to be closed.
    /// The parent view should handle this event to hide the panel.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="PanelViewModelBase"/> class.
    /// </summary>
    protected PanelViewModelBase()
    {
        CloseCommand = ReactiveCommand.Create(OnClose);
    }

    /// <summary>
    /// Handles the close command. Raises the CloseRequested event if the panel can be collapsed.
    /// </summary>
    private void OnClose()
    {
        if (CanCollapse)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Toggles the expanded/collapsed state of the panel.
    /// </summary>
    public void ToggleExpanded()
    {
        if (CanCollapse)
        {
            IsExpanded = !IsExpanded;
        }
    }
}
