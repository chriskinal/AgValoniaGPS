using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for headline creation dialog (FormHeadLine).
/// Creates headlines (headland boundaries) for field operations.
/// </summary>
public class HeadLineViewModel : DialogViewModelBase
{
    private double _distanceFromBoundary = 10.0;
    private int _numberOfPasses = 1;
    private double _implementWidth = 10.0;
    private bool _hasHeadland;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadLineViewModel"/> class.
    /// </summary>
    /// <param name="implementWidth">The implement width in meters.</param>
    public HeadLineViewModel(double implementWidth = 10.0)
    {
        _implementWidth = implementWidth;

        BoundaryPoints = new List<Position>();
        HeadlandPasses = new ObservableCollection<ObservableCollection<Position>>();

        SetDistanceCommand = ReactiveCommand.Create<double>(OnSetDistance);
        GenerateHeadlandCommand = ReactiveCommand.Create(OnGenerateHeadland,
            this.WhenAnyValue(x => x.HasBoundary).Select(hasBoundary => hasBoundary));
        ClearHeadlandCommand = ReactiveCommand.Create(OnClearHeadland,
            this.WhenAnyValue(x => x.HasHeadland).Select(hasHeadland => hasHeadland));
    }

    /// <summary>
    /// Gets the field boundary points.
    /// </summary>
    public List<Position> BoundaryPoints { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a boundary is loaded.
    /// </summary>
    public bool HasBoundary => BoundaryPoints.Count >= 3;

    /// <summary>
    /// Gets the headland passes collection.
    /// Each pass is a collection of positions forming a headland line.
    /// </summary>
    public ObservableCollection<ObservableCollection<Position>> HeadlandPasses { get; }

    /// <summary>
    /// Gets or sets the distance from field boundary in meters.
    /// This is the offset distance for the first headland pass.
    /// </summary>
    public double DistanceFromBoundary
    {
        get => _distanceFromBoundary;
        set
        {
            this.RaiseAndSetIfChanged(ref _distanceFromBoundary, Math.Max(value, 0));
            this.RaisePropertyChanged(nameof(DistanceFromBoundaryFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted distance from boundary string.
    /// </summary>
    public string DistanceFromBoundaryFormatted => $"{DistanceFromBoundary:F1} m";

    /// <summary>
    /// Gets or sets the number of headland passes to generate.
    /// Valid range: 1-10.
    /// </summary>
    public int NumberOfPasses
    {
        get => _numberOfPasses;
        set
        {
            this.RaiseAndSetIfChanged(ref _numberOfPasses, Math.Clamp(value, 1, 10));
            this.RaisePropertyChanged(nameof(NumberOfPassesDisplay));
        }
    }

    /// <summary>
    /// Gets the number of passes display text.
    /// </summary>
    public string NumberOfPassesDisplay => $"{NumberOfPasses} {(NumberOfPasses == 1 ? "pass" : "passes")}";

    /// <summary>
    /// Gets or sets the implement width in meters.
    /// Used to calculate spacing between headland passes.
    /// </summary>
    public double ImplementWidth
    {
        get => _implementWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _implementWidth, Math.Max(value, 1.0));
            this.RaisePropertyChanged(nameof(ImplementWidthFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted implement width string.
    /// </summary>
    public string ImplementWidthFormatted => $"{ImplementWidth:F1} m";

    /// <summary>
    /// Gets or sets a value indicating whether headland lines have been generated.
    /// </summary>
    public bool HasHeadland
    {
        get => _hasHeadland;
        private set => this.RaiseAndSetIfChanged(ref _hasHeadland, value);
    }

    /// <summary>
    /// Gets the command to set the distance from boundary.
    /// </summary>
    public ICommand SetDistanceCommand { get; }

    /// <summary>
    /// Gets the command to generate headland passes.
    /// </summary>
    public ICommand GenerateHeadlandCommand { get; }

    /// <summary>
    /// Gets the command to clear all headland lines.
    /// </summary>
    public ICommand ClearHeadlandCommand { get; }

    /// <summary>
    /// Loads boundary points for headland generation.
    /// </summary>
    /// <param name="boundary">The field boundary points.</param>
    public void LoadBoundary(List<Position> boundary)
    {
        BoundaryPoints = new List<Position>(boundary);
        this.RaisePropertyChanged(nameof(BoundaryPoints));
        this.RaisePropertyChanged(nameof(HasBoundary));
    }

    /// <summary>
    /// Sets the distance from boundary.
    /// </summary>
    /// <param name="distance">The distance in meters.</param>
    private void OnSetDistance(double distance)
    {
        DistanceFromBoundary = distance;
    }

    /// <summary>
    /// Generates headland passes based on current configuration.
    /// </summary>
    private void OnGenerateHeadland()
    {
        if (!HasBoundary)
        {
            SetError("No field boundary loaded");
            return;
        }

        if (DistanceFromBoundary < 0)
        {
            SetError("Distance from boundary must be positive");
            return;
        }

        try
        {
            HeadlandPasses.Clear();

            // TODO: When headland service is integrated, generate headland passes
            // Example:
            // var passes = _headlandService?.GenerateHeadlandPasses(
            //     BoundaryPoints, ImplementWidth, NumberOfPasses);
            //
            // if (passes != null)
            // {
            //     foreach (var pass in passes)
            //     {
            //         HeadlandPasses.Add(new ObservableCollection<Position>(pass));
            //     }
            // }

            // For now, create placeholder passes
            for (int i = 0; i < NumberOfPasses; i++)
            {
                var pass = new ObservableCollection<Position>();
                // Offset each pass inward by implement width
                double offset = DistanceFromBoundary + (i * ImplementWidth);

                // In a real implementation, this would use polygon offset algorithms
                // For now, just mark as generated
                HeadlandPasses.Add(pass);
            }

            HasHeadland = true;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Error generating headland: {ex.Message}");
            HasHeadland = false;
        }
    }

    /// <summary>
    /// Clears all generated headland lines.
    /// </summary>
    private void OnClearHeadland()
    {
        HeadlandPasses.Clear();
        HasHeadland = false;
        ClearError();
    }

    /// <summary>
    /// Validates configuration before closing.
    /// </summary>
    protected override bool OnOK()
    {
        if (!HasBoundary)
        {
            SetError("No field boundary loaded");
            return false;
        }

        if (NumberOfPasses < 1 || NumberOfPasses > 10)
        {
            SetError("Number of passes must be between 1 and 10");
            return false;
        }

        return base.OnOK();
    }
}
