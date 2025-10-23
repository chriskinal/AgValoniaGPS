using System;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for AB line drawing interface dialog (FormABDraw).
/// Provides interactive canvas for drawing AB lines by clicking two points.
/// </summary>
public class ABDrawViewModel : DialogViewModelBase
{
    private Position? _pointA;
    private Position? _pointB;
    private double _lineHeading;
    private double _lineLength;
    private bool _isPointASet;
    private bool _isPointBSet;
    private string _instructions = "Click to set Point A";

    /// <summary>
    /// Initializes a new instance of the <see cref="ABDrawViewModel"/> class.
    /// </summary>
    public ABDrawViewModel()
    {
        // TODO: Inject IABLineService for AB line creation

        SetPointACommand = ReactiveCommand.Create<Position>(OnSetPointA);
        SetPointBCommand = ReactiveCommand.Create<Position>(OnSetPointB);
        ClearCommand = ReactiveCommand.Create(OnClear);
        CreateABLineCommand = ReactiveCommand.Create(OnCreateABLine,
            this.WhenAnyValue(x => x.IsPointASet, x => x.IsPointBSet)
                .Select(tuple => tuple.Item1 && tuple.Item2));
    }

    /// <summary>
    /// Gets or sets the first point (Point A) of the AB line.
    /// </summary>
    public Position? PointA
    {
        get => _pointA;
        set
        {
            this.RaiseAndSetIfChanged(ref _pointA, value);
            this.RaisePropertyChanged(nameof(PointAFormatted));
            UpdateLineCalculations();
        }
    }

    /// <summary>
    /// Gets the formatted Point A string.
    /// </summary>
    public string PointAFormatted =>
        PointA != null
            ? $"A: {PointA.Latitude:F6}°, {PointA.Longitude:F6}°"
            : "Point A: Not set";

    /// <summary>
    /// Gets or sets the second point (Point B) of the AB line.
    /// </summary>
    public Position? PointB
    {
        get => _pointB;
        set
        {
            this.RaiseAndSetIfChanged(ref _pointB, value);
            this.RaisePropertyChanged(nameof(PointBFormatted));
            UpdateLineCalculations();
        }
    }

    /// <summary>
    /// Gets the formatted Point B string.
    /// </summary>
    public string PointBFormatted =>
        PointB != null
            ? $"B: {PointB.Latitude:F6}°, {PointB.Longitude:F6}°"
            : "Point B: Not set";

    /// <summary>
    /// Gets or sets the calculated line heading in degrees (0-360).
    /// </summary>
    public double LineHeading
    {
        get => _lineHeading;
        set
        {
            this.RaiseAndSetIfChanged(ref _lineHeading, value);
            this.RaisePropertyChanged(nameof(LineHeadingFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted line heading string.
    /// </summary>
    public string LineHeadingFormatted => $"Heading: {LineHeading:F1}°";

    /// <summary>
    /// Gets or sets the calculated line length in meters.
    /// </summary>
    public double LineLength
    {
        get => _lineLength;
        set
        {
            this.RaiseAndSetIfChanged(ref _lineLength, value);
            this.RaisePropertyChanged(nameof(LineLengthFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted line length string.
    /// </summary>
    public string LineLengthFormatted => $"Length: {LineLength:F1} m";

    /// <summary>
    /// Gets or sets a value indicating whether Point A has been set.
    /// </summary>
    public bool IsPointASet
    {
        get => _isPointASet;
        private set => this.RaiseAndSetIfChanged(ref _isPointASet, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Point B has been set.
    /// </summary>
    public bool IsPointBSet
    {
        get => _isPointBSet;
        private set => this.RaiseAndSetIfChanged(ref _isPointBSet, value);
    }

    /// <summary>
    /// Gets or sets the instruction text for the user.
    /// </summary>
    public string Instructions
    {
        get => _instructions;
        set => this.RaiseAndSetIfChanged(ref _instructions, value);
    }

    /// <summary>
    /// Gets the command to set Point A from mouse click.
    /// </summary>
    public ICommand SetPointACommand { get; }

    /// <summary>
    /// Gets the command to set Point B from mouse click.
    /// </summary>
    public ICommand SetPointBCommand { get; }

    /// <summary>
    /// Gets the command to clear both points and start over.
    /// </summary>
    public ICommand ClearCommand { get; }

    /// <summary>
    /// Gets the command to create the AB line from the two points.
    /// </summary>
    public ICommand CreateABLineCommand { get; }

    /// <summary>
    /// Sets Point A from a mouse click position.
    /// </summary>
    /// <param name="position">The clicked position.</param>
    private void OnSetPointA(Position position)
    {
        PointA = position;
        IsPointASet = true;
        Instructions = "Click to set Point B";
        ClearError();
    }

    /// <summary>
    /// Sets Point B from a mouse click position.
    /// </summary>
    /// <param name="position">The clicked position.</param>
    private void OnSetPointB(Position position)
    {
        PointB = position;
        IsPointBSet = true;
        Instructions = "Click 'Create AB Line' to finish";
        ClearError();
    }

    /// <summary>
    /// Handles canvas click to set either Point A or Point B.
    /// </summary>
    /// <param name="position">The clicked position.</param>
    public void OnCanvasClick(Position position)
    {
        if (!IsPointASet)
        {
            OnSetPointA(position);
        }
        else if (!IsPointBSet)
        {
            OnSetPointB(position);
        }
    }

    /// <summary>
    /// Clears both points and starts over.
    /// </summary>
    private void OnClear()
    {
        PointA = null;
        PointB = null;
        IsPointASet = false;
        IsPointBSet = false;
        LineHeading = 0;
        LineLength = 0;
        Instructions = "Click to set Point A";
        ClearError();
    }

    /// <summary>
    /// Creates the AB line from the two points.
    /// </summary>
    private void OnCreateABLine()
    {
        if (PointA == null || PointB == null)
        {
            SetError("Both Point A and Point B must be set");
            return;
        }

        if (LineLength < 10.0)
        {
            SetError("AB line must be at least 10 meters long");
            return;
        }

        try
        {
            // TODO: When AB line service is integrated, create AB line
            // Example: _abLineService?.CreateABLine(PointA, PointB);

            ClearError();
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Error creating AB line: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates line heading and length calculations when points change.
    /// </summary>
    private void UpdateLineCalculations()
    {
        if (PointA == null || PointB == null)
        {
            LineHeading = 0;
            LineLength = 0;
            return;
        }

        // Calculate heading from Point A to Point B
        double lat1 = PointA.Latitude * Math.PI / 180.0;
        double lon1 = PointA.Longitude * Math.PI / 180.0;
        double lat2 = PointB.Latitude * Math.PI / 180.0;
        double lon2 = PointB.Longitude * Math.PI / 180.0;

        double dLon = lon2 - lon1;

        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) -
                   Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

        double heading = Math.Atan2(y, x) * 180.0 / Math.PI;

        // Normalize to 0-360
        if (heading < 0) heading += 360.0;

        LineHeading = heading;

        // Calculate distance using Haversine formula
        double R = 6371000; // Earth's radius in meters
        double dLat = lat2 - lat1;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        LineLength = R * c;
    }

    /// <summary>
    /// Validates AB line before closing.
    /// </summary>
    protected override bool OnOK()
    {
        if (!IsPointASet || !IsPointBSet)
        {
            SetError("Both Point A and Point B must be set");
            return false;
        }

        if (LineLength < 10.0)
        {
            SetError("AB line must be at least 10 meters long");
            return false;
        }

        return base.OnOK();
    }
}
