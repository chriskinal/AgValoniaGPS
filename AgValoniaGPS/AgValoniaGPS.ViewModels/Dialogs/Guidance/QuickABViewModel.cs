using System;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for quick AB line creation dialog (FormQuickAB).
/// Creates AB line from current GPS position and heading with adjustable angle.
/// </summary>
public class QuickABViewModel : DialogViewModelBase
{
    private Position _currentPosition = new Position { Latitude = 0, Longitude = 0 };
    private double _currentHeading;
    private double _abLineHeading;
    private double _headingOffset;
    private bool _useCurrentHeading = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuickABViewModel"/> class.
    /// </summary>
    public QuickABViewModel()
    {
        // TODO: Inject IPositionUpdateService and IABLineService
        // Subscribe to position updates

        UseCurrentHeadingCommand = ReactiveCommand.Create(OnUseCurrentHeading);
        AdjustHeadingCommand = ReactiveCommand.Create<double>(OnAdjustHeading);
        CreateABLineCommand = ReactiveCommand.Create(OnCreateABLine,
            this.WhenAnyValue(x => x.CurrentPosition).Select(pos => pos != null));

        // Initialize with sample data
        UpdateSampleData();
    }

    /// <summary>
    /// Gets or sets the current GPS position.
    /// </summary>
    public Position CurrentPosition
    {
        get => _currentPosition;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPosition, value);
            this.RaisePropertyChanged(nameof(CurrentPositionFormatted));
            this.RaisePropertyChanged(nameof(LatitudeFormatted));
            this.RaisePropertyChanged(nameof(LongitudeFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted current position string.
    /// </summary>
    public string CurrentPositionFormatted =>
        $"{CurrentPosition.Latitude:F6}°, {CurrentPosition.Longitude:F6}°";

    /// <summary>
    /// Gets the formatted latitude string.
    /// </summary>
    public string LatitudeFormatted => $"{CurrentPosition.Latitude:F7}°";

    /// <summary>
    /// Gets the formatted longitude string.
    /// </summary>
    public string LongitudeFormatted => $"{CurrentPosition.Longitude:F7}°";

    /// <summary>
    /// Gets or sets the current GPS heading in degrees (0-360).
    /// 0 = North, 90 = East, 180 = South, 270 = West.
    /// </summary>
    public double CurrentHeading
    {
        get => _currentHeading;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentHeading, NormalizeAngle(value));
            this.RaisePropertyChanged(nameof(CurrentHeadingFormatted));

            if (UseCurrentHeading)
            {
                ABLineHeading = _currentHeading;
            }
        }
    }

    /// <summary>
    /// Gets the formatted current heading string.
    /// </summary>
    public string CurrentHeadingFormatted => $"{CurrentHeading:F1}°";

    /// <summary>
    /// Gets or sets the desired AB line heading in degrees (0-360).
    /// </summary>
    public double ABLineHeading
    {
        get => _abLineHeading;
        set
        {
            this.RaiseAndSetIfChanged(ref _abLineHeading, NormalizeAngle(value));
            this.RaisePropertyChanged(nameof(ABLineHeadingFormatted));
            UpdateHeadingOffset();
        }
    }

    /// <summary>
    /// Gets the formatted AB line heading string.
    /// </summary>
    public string ABLineHeadingFormatted => $"{ABLineHeading:F1}°";

    /// <summary>
    /// Gets or sets the heading offset from current heading (-180 to +180).
    /// </summary>
    public double HeadingOffset
    {
        get => _headingOffset;
        set
        {
            // Clamp to -180 to +180 range
            var clamped = value;
            while (clamped > 180) clamped -= 360;
            while (clamped < -180) clamped += 360;

            this.RaiseAndSetIfChanged(ref _headingOffset, clamped);
            this.RaisePropertyChanged(nameof(HeadingOffsetFormatted));

            // Update AB line heading when offset changes
            if (!UseCurrentHeading)
            {
                ABLineHeading = CurrentHeading + _headingOffset;
            }
        }
    }

    /// <summary>
    /// Gets the formatted heading offset string.
    /// </summary>
    public string HeadingOffsetFormatted => $"{(HeadingOffset >= 0 ? "+" : "")}{HeadingOffset:F1}°";

    /// <summary>
    /// Gets or sets a value indicating whether to use current GPS heading for AB line.
    /// When true, AB line follows GPS heading. When false, manual heading adjustment is enabled.
    /// </summary>
    public bool UseCurrentHeading
    {
        get => _useCurrentHeading;
        set
        {
            this.RaiseAndSetIfChanged(ref _useCurrentHeading, value);

            if (value)
            {
                ABLineHeading = CurrentHeading;
                HeadingOffset = 0;
            }
        }
    }

    /// <summary>
    /// Gets the command to use current GPS heading for AB line.
    /// </summary>
    public ICommand UseCurrentHeadingCommand { get; }

    /// <summary>
    /// Gets the command to adjust heading by a specific offset.
    /// </summary>
    public ICommand AdjustHeadingCommand { get; }

    /// <summary>
    /// Gets the command to create the AB line.
    /// </summary>
    public ICommand CreateABLineCommand { get; }

    /// <summary>
    /// Uses current GPS heading for AB line creation.
    /// </summary>
    private void OnUseCurrentHeading()
    {
        UseCurrentHeading = true;
        ABLineHeading = CurrentHeading;
        HeadingOffset = 0;
        ClearError();
    }

    /// <summary>
    /// Adjusts the AB line heading by the specified offset.
    /// </summary>
    /// <param name="offsetDelta">Angle offset to add (in degrees).</param>
    private void OnAdjustHeading(double offsetDelta)
    {
        UseCurrentHeading = false;
        HeadingOffset += offsetDelta;
        ABLineHeading = CurrentHeading + HeadingOffset;
    }

    /// <summary>
    /// Creates the AB line from current position and heading.
    /// </summary>
    private void OnCreateABLine()
    {
        if (CurrentPosition == null)
        {
            SetError("Current position not available");
            return;
        }

        try
        {
            // TODO: When AB line service is integrated, create AB line
            // Example: _abLineService?.CalculateABLineFromHeading(CurrentPosition, ABLineHeading);

            ClearError();
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Error creating AB line: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates heading offset based on current headings.
    /// </summary>
    private void UpdateHeadingOffset()
    {
        double offset = ABLineHeading - CurrentHeading;

        // Normalize to -180 to +180 range
        while (offset > 180) offset -= 360;
        while (offset < -180) offset += 360;

        _headingOffset = offset;
        this.RaisePropertyChanged(nameof(HeadingOffset));
        this.RaisePropertyChanged(nameof(HeadingOffsetFormatted));
    }

    /// <summary>
    /// Normalizes an angle to 0-360 range.
    /// </summary>
    /// <param name="angle">The angle to normalize.</param>
    /// <returns>Normalized angle (0-360).</returns>
    private double NormalizeAngle(double angle)
    {
        double normalized = angle % 360.0;
        if (normalized < 0) normalized += 360.0;
        return normalized;
    }

    /// <summary>
    /// Updates with sample GPS data for testing.
    /// </summary>
    private void UpdateSampleData()
    {
        // Sample coordinates (Iowa farm)
        CurrentPosition = new Position { Latitude = 42.0308, Longitude = -93.6319 };
        CurrentHeading = 45.0; // Northeast
        ABLineHeading = 45.0;
        HeadingOffset = 0;
    }

    /// <summary>
    /// Validates configuration before closing.
    /// </summary>
    protected override bool OnOK()
    {
        if (CurrentPosition == null)
        {
            SetError("Current position not available");
            return false;
        }

        return base.OnOK();
    }
}
