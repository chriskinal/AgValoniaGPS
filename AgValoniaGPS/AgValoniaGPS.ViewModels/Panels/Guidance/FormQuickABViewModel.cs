using AgValoniaGPS.Models;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.Guidance;

/// <summary>
/// ViewModel for the Quick A-B Line setup panel.
/// Provides quick A-B line creation from current position and heading.
/// </summary>
public partial class FormQuickABViewModel : PanelViewModelBase
{
    private readonly IABLineService _abLineService;
    private readonly IPositionUpdateService _positionService;

    private Position? _pointA;
    private Position? _pointB;
    private double _headingAdjustment;
    private double _lineOffset;
    private bool _canApply;
    private bool _pointASet;
    private bool _pointBSet;

    public FormQuickABViewModel(
        IABLineService abLineService,
        IPositionUpdateService positionService)
    {
        _abLineService = abLineService ?? throw new ArgumentNullException(nameof(abLineService));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));

        Title = "Quick A-B Setup";

        // Commands
        SetPointACommand = ReactiveCommand.Create(OnSetPointA);
        SetPointBCommand = ReactiveCommand.Create(OnSetPointB);
        ApplyCommand = ReactiveCommand.Create(OnApply, this.WhenAnyValue(x => x.CanApply));
        CancelCommand = ReactiveCommand.Create(OnCancel);
        CreateFromHeadingCommand = ReactiveCommand.Create(OnCreateFromHeading);

        // Update CanApply when points change
        this.WhenAnyValue(
            x => x.PointASet,
            x => x.PointBSet,
            (a, b) => a && b)
            .Subscribe(canApply => CanApply = canApply);
    }

    public string Title { get; } = "Quick A-B Setup";

    /// <summary>
    /// Point A position
    /// </summary>
    public Position? PointA
    {
        get => _pointA;
        set
        {
            this.RaiseAndSetIfChanged(ref _pointA, value);
            PointASet = value != null;
        }
    }

    /// <summary>
    /// Point B position
    /// </summary>
    public Position? PointB
    {
        get => _pointB;
        set
        {
            this.RaiseAndSetIfChanged(ref _pointB, value);
            PointBSet = value != null;
        }
    }

    /// <summary>
    /// Heading adjustment in degrees
    /// </summary>
    public double HeadingAdjustment
    {
        get => _headingAdjustment;
        set
        {
            if (value >= -180 && value <= 180)
            {
                this.RaiseAndSetIfChanged(ref _headingAdjustment, value);
            }
        }
    }

    /// <summary>
    /// Line offset in meters (-100 to +100)
    /// </summary>
    public double LineOffset
    {
        get => _lineOffset;
        set
        {
            if (value >= -100 && value <= 100)
            {
                this.RaiseAndSetIfChanged(ref _lineOffset, value);
            }
        }
    }

    /// <summary>
    /// Whether the A-B line can be applied
    /// </summary>
    public bool CanApply
    {
        get => _canApply;
        set => this.RaiseAndSetIfChanged(ref _canApply, value);
    }

    /// <summary>
    /// Whether point A has been set
    /// </summary>
    public bool PointASet
    {
        get => _pointASet;
        private set => this.RaiseAndSetIfChanged(ref _pointASet, value);
    }

    /// <summary>
    /// Whether point B has been set
    /// </summary>
    public bool PointBSet
    {
        get => _pointBSet;
        private set => this.RaiseAndSetIfChanged(ref _pointBSet, value);
    }

    public ICommand SetPointACommand { get; }
    public ICommand SetPointBCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CreateFromHeadingCommand { get; }

    private void OnSetPointA()
    {
        try
        {
            var geoCoord = _positionService.GetCurrentPosition();
            if (geoCoord == null)
            {
                SetError("No GPS position available");
                return;
            }

            var heading = _positionService.GetCurrentHeading();
            var speed = _positionService.GetCurrentSpeed();

            PointA = new Position
            {
                Easting = geoCoord.Easting,
                Northing = geoCoord.Northing,
                Altitude = geoCoord.Altitude,
                Heading = heading * 180.0 / Math.PI, // Convert to degrees
                Speed = speed
            };
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to set point A: {ex.Message}");
        }
    }

    private void OnSetPointB()
    {
        try
        {
            var geoCoord = _positionService.GetCurrentPosition();
            if (geoCoord == null)
            {
                SetError("No GPS position available");
                return;
            }

            var heading = _positionService.GetCurrentHeading();
            var speed = _positionService.GetCurrentSpeed();

            PointB = new Position
            {
                Easting = geoCoord.Easting,
                Northing = geoCoord.Northing,
                Altitude = geoCoord.Altitude,
                Heading = heading * 180.0 / Math.PI, // Convert to degrees
                Speed = speed
            };
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to set point B: {ex.Message}");
        }
    }

    private void OnApply()
    {
        try
        {
            if (PointA == null || PointB == null)
            {
                SetError("Both points A and B must be set");
                return;
            }

            // Create A-B line using the service
            var line = _abLineService.CreateFromPoints(PointA, PointB, $"QuickAB_{DateTime.Now:HHmmss}");

            // Apply line offset if set
            if (Math.Abs(LineOffset) > 0.1)
            {
                line = _abLineService.NudgeLine(line, LineOffset);
            }

            // Reset the form
            OnCancel();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to create A-B line: {ex.Message}");
        }
    }

    private void OnCancel()
    {
        PointA = null;
        PointB = null;
        HeadingAdjustment = 0;
        LineOffset = 0;
        ClearError();
    }

    private void OnCreateFromHeading()
    {
        try
        {
            var geoCoord = _positionService.GetCurrentPosition();
            if (geoCoord == null)
            {
                SetError("No GPS position available");
                return;
            }

            // Create A-B line from current position and heading (adjusted)
            var currentHeading = _positionService.GetCurrentHeading();
            var currentSpeed = _positionService.GetCurrentSpeed();
            double headingDegrees = (currentHeading * 180.0 / Math.PI) + HeadingAdjustment;

            var currentPosition = new Position
            {
                Easting = geoCoord.Easting,
                Northing = geoCoord.Northing,
                Altitude = geoCoord.Altitude,
                Heading = headingDegrees,
                Speed = currentSpeed
            };

            var line = _abLineService.CreateFromHeading(
                currentPosition,
                headingDegrees,
                $"QuickAB_{DateTime.Now:HHmmss}"
            );

            // Apply line offset if set
            if (Math.Abs(LineOffset) > 0.1)
            {
                line = _abLineService.NudgeLine(line, LineOffset);
            }

            // Reset the form
            OnCancel();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to create line from heading: {ex.Message}");
        }
    }
}
