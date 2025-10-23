using System;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// Headland mode enumeration.
/// </summary>
public enum HeadlandMode
{
    Auto,
    Manual
}

/// <summary>
/// ViewModel for headland management dialog (FormHeadAche).
/// Manages headland mode and pass navigation during field operations.
/// </summary>
public class HeadAcheViewModel : DialogViewModelBase
{
    private HeadlandMode _currentMode = HeadlandMode.Auto;
    private int _currentPass = 1;
    private bool _isInHeadland;
    private double _distanceToHeadland;
    private int _maxPasses = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadAcheViewModel"/> class.
    /// </summary>
    /// <param name="maxPasses">Maximum number of headland passes configured.</param>
    public HeadAcheViewModel(int maxPasses = 4)
    {
        _maxPasses = Math.Max(maxPasses, 1);

        SetModeCommand = ReactiveCommand.Create<HeadlandMode>(OnSetMode);
        NextPassCommand = ReactiveCommand.Create(OnNextPass,
            this.WhenAnyValue(x => x.CurrentPass, x => x.MaxPasses)
                .Select(tuple => tuple.Item1 < tuple.Item2));
        PreviousPassCommand = ReactiveCommand.Create(OnPreviousPass,
            this.WhenAnyValue(x => x.CurrentPass).Select(pass => pass > 1));

        // TODO: Subscribe to headland service events
        // Example: _headlandService.HeadlandEntered += OnHeadlandEntered;
    }

    /// <summary>
    /// Gets or sets the current headland mode (Auto or Manual).
    /// </summary>
    public HeadlandMode CurrentMode
    {
        get => _currentMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentMode, value);
            this.RaisePropertyChanged(nameof(CurrentModeDisplay));
            this.RaisePropertyChanged(nameof(IsAutoMode));
            this.RaisePropertyChanged(nameof(IsManualMode));
        }
    }

    /// <summary>
    /// Gets the current mode display text.
    /// </summary>
    public string CurrentModeDisplay => CurrentMode == HeadlandMode.Auto ? "Auto" : "Manual";

    /// <summary>
    /// Gets a value indicating whether auto mode is selected.
    /// </summary>
    public bool IsAutoMode => CurrentMode == HeadlandMode.Auto;

    /// <summary>
    /// Gets a value indicating whether manual mode is selected.
    /// </summary>
    public bool IsManualMode => CurrentMode == HeadlandMode.Manual;

    /// <summary>
    /// Gets or sets the current headland pass number (1-based).
    /// </summary>
    public int CurrentPass
    {
        get => _currentPass;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPass, Math.Clamp(value, 1, MaxPasses));
            this.RaisePropertyChanged(nameof(CurrentPassDisplay));
        }
    }

    /// <summary>
    /// Gets the current pass display text.
    /// </summary>
    public string CurrentPassDisplay => $"Pass {CurrentPass} of {MaxPasses}";

    /// <summary>
    /// Gets or sets the maximum number of headland passes.
    /// </summary>
    public int MaxPasses
    {
        get => _maxPasses;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxPasses, Math.Max(value, 1));
            this.RaisePropertyChanged(nameof(CurrentPassDisplay));
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the vehicle is currently in the headland zone.
    /// </summary>
    public bool IsInHeadland
    {
        get => _isInHeadland;
        set
        {
            this.RaiseAndSetIfChanged(ref _isInHeadland, value);
            this.RaisePropertyChanged(nameof(HeadlandStatusText));
            this.RaisePropertyChanged(nameof(HeadlandStatusColor));
        }
    }

    /// <summary>
    /// Gets the headland status text for display.
    /// </summary>
    public string HeadlandStatusText => IsInHeadland ? "IN HEADLAND" : "In Field";

    /// <summary>
    /// Gets the headland status color (red when in headland, green otherwise).
    /// </summary>
    public string HeadlandStatusColor => IsInHeadland ? "#FF0000" : "#00FF00";

    /// <summary>
    /// Gets or sets the distance to nearest headland in meters.
    /// </summary>
    public double DistanceToHeadland
    {
        get => _distanceToHeadland;
        set
        {
            this.RaiseAndSetIfChanged(ref _distanceToHeadland, Math.Max(value, 0));
            this.RaisePropertyChanged(nameof(DistanceToHeadlandFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted distance to headland string.
    /// </summary>
    public string DistanceToHeadlandFormatted => $"{DistanceToHeadland:F1} m";

    /// <summary>
    /// Gets the command to set the headland mode.
    /// </summary>
    public ICommand SetModeCommand { get; }

    /// <summary>
    /// Gets the command to move to the next headland pass.
    /// </summary>
    public ICommand NextPassCommand { get; }

    /// <summary>
    /// Gets the command to move to the previous headland pass.
    /// </summary>
    public ICommand PreviousPassCommand { get; }

    /// <summary>
    /// Sets the headland mode (Auto or Manual).
    /// </summary>
    /// <param name="mode">The headland mode to set.</param>
    private void OnSetMode(HeadlandMode mode)
    {
        CurrentMode = mode;

        // TODO: When headland service is integrated, update the service
        // Example: _headlandService?.SetHeadlandMode(mode);

        ClearError();
    }

    /// <summary>
    /// Moves to the next headland pass.
    /// </summary>
    private void OnNextPass()
    {
        if (CurrentPass < MaxPasses)
        {
            CurrentPass++;
        }
    }

    /// <summary>
    /// Moves to the previous headland pass.
    /// </summary>
    private void OnPreviousPass()
    {
        if (CurrentPass > 1)
        {
            CurrentPass--;
        }
    }

    /// <summary>
    /// Updates headland entry status.
    /// Called when vehicle enters or exits headland zone.
    /// </summary>
    /// <param name="isInHeadland">True if in headland, false otherwise.</param>
    /// <param name="distance">Distance to nearest headland boundary.</param>
    public void UpdateHeadlandStatus(bool isInHeadland, double distance)
    {
        IsInHeadland = isInHeadland;
        DistanceToHeadland = distance;
    }

    /// <summary>
    /// Cleanup when dialog closes.
    /// </summary>
    protected override void OnCancel()
    {
        // TODO: Unsubscribe from headland service events
        // Example: _headlandService.HeadlandEntered -= OnHeadlandEntered;

        base.OnCancel();
    }
}
