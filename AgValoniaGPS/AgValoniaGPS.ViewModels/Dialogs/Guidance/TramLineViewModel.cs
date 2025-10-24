using System;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for individual tram line editor dialog (FormTramLine).
/// Edits properties of a single tram line including offset, active state, and color.
/// </summary>
public class TramLineViewModel : DialogViewModelBase
{
    private int _tramLineNumber;
    private double _offsetDistance;
    private bool _isActive = true;
    private string _lineColor = "#FFFF00"; // Default yellow

    /// <summary>
    /// Initializes a new instance of the <see cref="TramLineViewModel"/> class.
    /// </summary>
    /// <param name="tramLineNumber">The tram line number (1-99).</param>
    /// <param name="offsetDistance">Initial offset distance in meters.</param>
    public TramLineViewModel(int tramLineNumber = 1, double offsetDistance = 0.0)
    {
        _tramLineNumber = Math.Clamp(tramLineNumber, 1, 99);
        _offsetDistance = offsetDistance;

        SetOffsetCommand = ReactiveCommand.Create<double>(OnSetOffset);
        ToggleActiveCommand = ReactiveCommand.Create(OnToggleActive);
        PickColorCommand = ReactiveCommand.Create(OnPickColor);
    }

    /// <summary>
    /// Gets or sets the tram line number (1-99).
    /// </summary>
    public int TramLineNumber
    {
        get => _tramLineNumber;
        set
        {
            this.RaiseAndSetIfChanged(ref _tramLineNumber, Math.Clamp(value, 1, 99));
            this.RaisePropertyChanged(nameof(TramLineNumberDisplay));
        }
    }

    /// <summary>
    /// Gets the tram line number display text.
    /// </summary>
    public string TramLineNumberDisplay => $"Tram Line #{TramLineNumber}";

    /// <summary>
    /// Gets or sets the offset distance from base line in meters.
    /// Positive values offset to the right, negative to the left.
    /// </summary>
    public double OffsetDistance
    {
        get => _offsetDistance;
        set
        {
            this.RaiseAndSetIfChanged(ref _offsetDistance, value);
            this.RaisePropertyChanged(nameof(OffsetDistanceFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted offset distance string.
    /// </summary>
    public string OffsetDistanceFormatted => $"{OffsetDistance:F2} m";

    /// <summary>
    /// Gets or sets a value indicating whether this tram line is active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            this.RaiseAndSetIfChanged(ref _isActive, value);
            this.RaisePropertyChanged(nameof(ActiveStatusText));
        }
    }

    /// <summary>
    /// Gets the active status text for display.
    /// </summary>
    public string ActiveStatusText => IsActive ? "Active" : "Inactive";

    /// <summary>
    /// Gets or sets the line color as hex string (e.g., "#FFFF00").
    /// </summary>
    public string LineColor
    {
        get => _lineColor;
        set => this.RaiseAndSetIfChanged(ref _lineColor, value);
    }

    /// <summary>
    /// Gets the command to set the offset distance.
    /// </summary>
    public ICommand SetOffsetCommand { get; }

    /// <summary>
    /// Gets the command to toggle the active state.
    /// </summary>
    public ICommand ToggleActiveCommand { get; }

    /// <summary>
    /// Gets the command to open the color picker.
    /// </summary>
    public ICommand PickColorCommand { get; }

    /// <summary>
    /// Sets the offset distance.
    /// </summary>
    /// <param name="offset">The offset distance in meters.</param>
    private void OnSetOffset(double offset)
    {
        OffsetDistance = offset;
    }

    /// <summary>
    /// Toggles the active state of the tram line.
    /// </summary>
    private void OnToggleActive()
    {
        IsActive = !IsActive;
    }

    /// <summary>
    /// Opens color picker dialog to select tram line color.
    /// </summary>
    private void OnPickColor()
    {
        // TODO: When dialog service is integrated, show color picker dialog
        // Example: var color = await _dialogService.ShowColorPickerAsync(LineColor);
        // if (color != null) LineColor = color;
    }

    /// <summary>
    /// Validates and applies tram line settings.
    /// </summary>
    protected override void OnOK()
    {
        // TODO: When tram line service is integrated, update the tram line
        // Example: _tramLineService?.UpdateTramLine(TramLineNumber, OffsetDistance, IsActive, LineColor);

        base.OnOK();
    }
}
