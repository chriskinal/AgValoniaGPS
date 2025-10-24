using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.FieldOperations;

/// <summary>
/// ViewModel for the Tram Line configuration panel.
/// Allows users to configure and generate tram lines for field operations.
/// </summary>
public partial class FormTramLineViewModel : PanelViewModelBase
{
    private readonly ITramLineService _tramLineService;
    private readonly IGuidanceService _guidanceService;

    private double _spacing = 18.0; // meters
    private int _passesBeforeTram = 6;
    private int _skipLines = 0;
    private bool _tramEnabled;
    private string _mode = "Auto";
    private bool _canGenerate = true;
    private int _tramLineCount;

    public FormTramLineViewModel(
        ITramLineService tramLineService,
        IGuidanceService guidanceService)
    {
        _tramLineService = tramLineService ?? throw new ArgumentNullException(nameof(tramLineService));
        _guidanceService = guidanceService ?? throw new ArgumentNullException(nameof(guidanceService));

        Title = "Tram Lines";

        // Commands
        GenerateCommand = ReactiveCommand.Create(OnGenerate, this.WhenAnyValue(x => x.CanGenerate));
        ClearCommand = ReactiveCommand.Create(OnClear);
        ToggleEnabledCommand = ReactiveCommand.Create(OnToggleEnabled);

        // Subscribe to service events
        _tramLineService.TramLineProximity += OnTramLineProximity;

        // Initialize with current values
        UpdateFromService();
    }

    public string Title { get; } = "Tram Lines";

    /// <summary>
    /// Spacing between tram lines in meters
    /// </summary>
    public double Spacing
    {
        get => _spacing;
        set
        {
            if (value >= 1.0 && value <= 100.0)
            {
                this.RaiseAndSetIfChanged(ref _spacing, value);
                _tramLineService.SetSpacing(value);
            }
        }
    }

    /// <summary>
    /// Number of passes before creating a tram line (1-20)
    /// </summary>
    public int PassesBeforeTram
    {
        get => _passesBeforeTram;
        set
        {
            if (value >= 1 && value <= 20)
            {
                this.RaiseAndSetIfChanged(ref _passesBeforeTram, value);
            }
        }
    }

    /// <summary>
    /// Number of lines to skip in multi-pass patterns
    /// </summary>
    public int SkipLines
    {
        get => _skipLines;
        set
        {
            if (value >= 0 && value <= 10)
            {
                this.RaiseAndSetIfChanged(ref _skipLines, value);
            }
        }
    }

    /// <summary>
    /// Whether tram lines are enabled
    /// </summary>
    public bool TramEnabled
    {
        get => _tramEnabled;
        set => this.RaiseAndSetIfChanged(ref _tramEnabled, value);
    }

    /// <summary>
    /// Tram line mode (Auto/Manual)
    /// </summary>
    public string Mode
    {
        get => _mode;
        set => this.RaiseAndSetIfChanged(ref _mode, value);
    }

    /// <summary>
    /// Whether tram lines can be generated
    /// </summary>
    public bool CanGenerate
    {
        get => _canGenerate;
        set => this.RaiseAndSetIfChanged(ref _canGenerate, value);
    }

    /// <summary>
    /// Number of tram lines currently loaded
    /// </summary>
    public int TramLineCount
    {
        get => _tramLineCount;
        set => this.RaiseAndSetIfChanged(ref _tramLineCount, value);
    }

    public ICommand GenerateCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ToggleEnabledCommand { get; }

    private void OnGenerate()
    {
        try
        {
            // Generate tram lines based on active AB line
            // This is a simplified implementation - actual implementation would need active AB line ID
            if (_guidanceService.IsActive)
            {
                // Placeholder: In real implementation, get active AB line ID from guidance service
                int activeABLineId = 0; // This would come from guidance service
                _tramLineService.GenerateFromABLine(activeABLineId, Spacing);

                TramEnabled = true;
                UpdateTramLineCount();
                ClearError();
            }
            else
            {
                SetError("No active guidance line. Please set an AB line first.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to generate tram lines: {ex.Message}");
        }
    }

    private void OnClear()
    {
        try
        {
            _tramLineService.ClearTramLines();
            TramEnabled = false;
            TramLineCount = 0;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to clear tram lines: {ex.Message}");
        }
    }

    private void OnToggleEnabled()
    {
        TramEnabled = !TramEnabled;
        // Note: ITramLineService doesn't have Enable/Disable methods,
        // so this just tracks the enabled state in the ViewModel
    }

    private void OnTramLineProximity(object? sender, EventArgs e)
    {
        // Handle tram line proximity event
        UpdateTramLineCount();
    }

    private void UpdateFromService()
    {
        // Update CanGenerate based on whether a guidance line is active
        CanGenerate = _guidanceService.IsActive;

        // Update tram line count
        UpdateTramLineCount();

        // Update spacing from service
        _spacing = _tramLineService.GetSpacing();
        this.RaisePropertyChanged(nameof(Spacing));
    }

    private void UpdateTramLineCount()
    {
        TramLineCount = _tramLineService.GetTramLineCount();
        TramEnabled = TramLineCount > 0;
    }
}
