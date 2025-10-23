using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// Tram line pattern mode.
/// </summary>
public enum TramLineMode
{
    All,
    APlus,
    ABOnly,
    BPlus,
    Skip2,
    Skip3
}

/// <summary>
/// Represents a tram line pattern configuration.
/// </summary>
public class TramLinePattern
{
    public TramLineMode Mode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for tram line configuration dialog (FormTram).
/// Configures tram line patterns, spacing, and generates tram lines.
/// </summary>
public class TramViewModel : DialogViewModelBase
{
    private TramLineMode _tramMode = TramLineMode.All;
    private double _tramSpacing = 1.0;
    private int _implementPasses = 1;
    private bool _showTramLines = true;
    private TramLinePattern? _selectedPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="TramViewModel"/> class.
    /// </summary>
    /// <param name="implementWidth">The implement width in meters for spacing calculations.</param>
    public TramViewModel(double implementWidth = 10.0)
    {
        ImplementWidth = implementWidth;

        Patterns = new ObservableCollection<TramLinePattern>
        {
            new TramLinePattern
            {
                Mode = TramLineMode.All,
                Name = "All Lines",
                Description = "Show all tram lines at specified spacing",
                IconPath = "/Assets/Icons/TramAll.png"
            },
            new TramLinePattern
            {
                Mode = TramLineMode.APlus,
                Name = "A+",
                Description = "Show A line and lines on positive side",
                IconPath = "/Assets/Icons/TramAPlus.png"
            },
            new TramLinePattern
            {
                Mode = TramLineMode.ABOnly,
                Name = "A & B Only",
                Description = "Show only A and B lines",
                IconPath = "/Assets/Icons/TramAB.png"
            },
            new TramLinePattern
            {
                Mode = TramLineMode.BPlus,
                Name = "B+",
                Description = "Show B line and lines on positive side",
                IconPath = "/Assets/Icons/TramBPlus.png"
            },
            new TramLinePattern
            {
                Mode = TramLineMode.Skip2,
                Name = "Skip 2",
                Description = "Show every other tram line",
                IconPath = "/Assets/Icons/TramSkip2.png"
            },
            new TramLinePattern
            {
                Mode = TramLineMode.Skip3,
                Name = "Skip 3",
                Description = "Show every third tram line",
                IconPath = "/Assets/Icons/TramSkip3.png"
            }
        };

        _selectedPattern = Patterns[0];

        SelectModeCommand = ReactiveCommand.Create<TramLineMode>(OnSelectMode);
        SetSpacingCommand = ReactiveCommand.Create<double>(OnSetSpacing);
        GenerateTramLinesCommand = ReactiveCommand.Create(OnGenerateTramLines,
            this.WhenAnyValue(x => x.TramSpacing).Select(spacing => spacing > 0));
        ClearTramLinesCommand = ReactiveCommand.Create(OnClearTramLines);
    }

    /// <summary>
    /// Gets the available tram line patterns.
    /// </summary>
    public ObservableCollection<TramLinePattern> Patterns { get; }

    /// <summary>
    /// Gets or sets the selected tram line pattern.
    /// </summary>
    public TramLinePattern? SelectedPattern
    {
        get => _selectedPattern;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPattern, value);
            if (value != null)
            {
                TramMode = value.Mode;
            }
        }
    }

    /// <summary>
    /// Gets or sets the tram line mode.
    /// </summary>
    public TramLineMode TramMode
    {
        get => _tramMode;
        set => this.RaiseAndSetIfChanged(ref _tramMode, value);
    }

    /// <summary>
    /// Gets or sets the tram spacing as multiple of implement width.
    /// Valid range: 1x to 12x.
    /// </summary>
    public double TramSpacing
    {
        get => _tramSpacing;
        set
        {
            this.RaiseAndSetIfChanged(ref _tramSpacing, Math.Clamp(value, 1.0, 12.0));
            this.RaisePropertyChanged(nameof(TramSpacingFormatted));
            this.RaisePropertyChanged(nameof(ActualSpacing));
            this.RaisePropertyChanged(nameof(ActualSpacingFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted tram spacing string.
    /// </summary>
    public string TramSpacingFormatted => $"{TramSpacing:F0}x";

    /// <summary>
    /// Gets the implement width in meters.
    /// </summary>
    public double ImplementWidth { get; }

    /// <summary>
    /// Gets the actual spacing in meters (TramSpacing × ImplementWidth).
    /// </summary>
    public double ActualSpacing => TramSpacing * ImplementWidth;

    /// <summary>
    /// Gets the actual spacing formatted string.
    /// </summary>
    public string ActualSpacingFormatted => $"{ActualSpacing:F1} m";

    /// <summary>
    /// Gets or sets the number of implement passes between tram lines.
    /// Valid range: 1-12.
    /// </summary>
    public int ImplementPasses
    {
        get => _implementPasses;
        set
        {
            this.RaiseAndSetIfChanged(ref _implementPasses, Math.Clamp(value, 1, 12));
            this.RaisePropertyChanged(nameof(ImplementPassesDisplay));
        }
    }

    /// <summary>
    /// Gets the implement passes display text.
    /// </summary>
    public string ImplementPassesDisplay => $"{ImplementPasses} {(ImplementPasses == 1 ? "pass" : "passes")}";

    /// <summary>
    /// Gets or sets a value indicating whether to show tram lines.
    /// </summary>
    public bool ShowTramLines
    {
        get => _showTramLines;
        set => this.RaiseAndSetIfChanged(ref _showTramLines, value);
    }

    /// <summary>
    /// Gets the command to select a tram mode.
    /// </summary>
    public ICommand SelectModeCommand { get; }

    /// <summary>
    /// Gets the command to set tram spacing.
    /// </summary>
    public ICommand SetSpacingCommand { get; }

    /// <summary>
    /// Gets the command to generate tram lines.
    /// </summary>
    public ICommand GenerateTramLinesCommand { get; }

    /// <summary>
    /// Gets the command to clear all tram lines.
    /// </summary>
    public ICommand ClearTramLinesCommand { get; }

    /// <summary>
    /// Selects a tram line mode.
    /// </summary>
    /// <param name="mode">The tram line mode to select.</param>
    private void OnSelectMode(TramLineMode mode)
    {
        TramMode = mode;
        SelectedPattern = Patterns.FirstOrDefault(p => p.Mode == mode);
    }

    /// <summary>
    /// Sets the tram spacing multiplier.
    /// </summary>
    /// <param name="spacing">The spacing multiplier (1-12x).</param>
    private void OnSetSpacing(double spacing)
    {
        TramSpacing = spacing;
    }

    /// <summary>
    /// Generates tram lines based on current configuration.
    /// </summary>
    private void OnGenerateTramLines()
    {
        if (TramSpacing <= 0)
        {
            SetError("Tram spacing must be greater than zero");
            return;
        }

        try
        {
            // TODO: When tram line service is integrated, generate tram lines
            // Example: _tramLineService?.GenerateTramLines(baseLine, ActualSpacing, TramMode);

            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Error generating tram lines: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears all tram lines.
    /// </summary>
    private void OnClearTramLines()
    {
        // TODO: When tram line service is integrated, clear tram lines
        // Example: _tramLineService?.ClearTramLines();

        ClearError();
    }

    /// <summary>
    /// Validates configuration before closing.
    /// </summary>
    protected override bool OnOK()
    {
        if (TramSpacing <= 0)
        {
            SetError("Tram spacing must be greater than zero");
            return false;
        }

        return base.OnOK();
    }
}
