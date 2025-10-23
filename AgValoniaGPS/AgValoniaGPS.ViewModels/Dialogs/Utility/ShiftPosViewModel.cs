using System;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for position shift tool that allows user to adjust X/Y offsets
/// for correcting field position errors.
/// </summary>
public class ShiftPosViewModel : DialogViewModelBase
{
    private double _offsetX;
    private double _offsetY;
    private double _originalX;
    private double _originalY;
    private string _units = "meters";

    /// <summary>
    /// Initializes a new instance of the <see cref="ShiftPosViewModel"/> class.
    /// </summary>
    public ShiftPosViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance with the current position.
    /// </summary>
    /// <param name="currentX">Current X position.</param>
    /// <param name="currentY">Current Y position.</param>
    public ShiftPosViewModel(double currentX, double currentY) : this()
    {
        OriginalX = currentX;
        OriginalY = currentY;
    }

    /// <summary>
    /// Gets or sets the X offset in meters.
    /// </summary>
    public double OffsetX
    {
        get => _offsetX;
        set
        {
            this.RaiseAndSetIfChanged(ref _offsetX, value);
            this.RaisePropertyChanged(nameof(NewX));
        }
    }

    /// <summary>
    /// Gets or sets the Y offset in meters.
    /// </summary>
    public double OffsetY
    {
        get => _offsetY;
        set
        {
            this.RaiseAndSetIfChanged(ref _offsetY, value);
            this.RaisePropertyChanged(nameof(NewY));
        }
    }

    /// <summary>
    /// Gets or sets the original X position.
    /// </summary>
    public double OriginalX
    {
        get => _originalX;
        set
        {
            this.RaiseAndSetIfChanged(ref _originalX, value);
            this.RaisePropertyChanged(nameof(NewX));
        }
    }

    /// <summary>
    /// Gets or sets the original Y position.
    /// </summary>
    public double OriginalY
    {
        get => _originalY;
        set
        {
            this.RaiseAndSetIfChanged(ref _originalY, value);
            this.RaisePropertyChanged(nameof(NewY));
        }
    }

    /// <summary>
    /// Gets or sets the measurement units.
    /// </summary>
    public string Units
    {
        get => _units;
        set => this.RaiseAndSetIfChanged(ref _units, value);
    }

    /// <summary>
    /// Gets the new X position after applying offset.
    /// </summary>
    public double NewX => OriginalX + OffsetX;

    /// <summary>
    /// Gets the new Y position after applying offset.
    /// </summary>
    public double NewY => OriginalY + OffsetY;

    /// <summary>
    /// Gets the total shift distance.
    /// </summary>
    public double TotalShift => Math.Sqrt(OffsetX * OffsetX + OffsetY * OffsetY);

    /// <summary>
    /// Gets the shift angle in degrees from north.
    /// </summary>
    public double ShiftAngle
    {
        get
        {
            if (Math.Abs(OffsetX) < 0.001 && Math.Abs(OffsetY) < 0.001)
                return 0;

            var angle = Math.Atan2(OffsetX, OffsetY) * 180.0 / Math.PI;
            return angle < 0 ? angle + 360 : angle;
        }
    }

    /// <summary>
    /// Validates the input before accepting.
    /// </summary>
    protected override bool OnOK()
    {
        // Validate that offsets are reasonable (within ±1000 meters)
        if (Math.Abs(OffsetX) > 1000 || Math.Abs(OffsetY) > 1000)
        {
            SetError("Offset values must be between -1000 and 1000 meters.");
            return false;
        }

        ClearError();
        return base.OnOK();
    }

    /// <summary>
    /// Resets the offsets to zero.
    /// </summary>
    public void ResetOffsets()
    {
        OffsetX = 0;
        OffsetY = 0;
    }
}
