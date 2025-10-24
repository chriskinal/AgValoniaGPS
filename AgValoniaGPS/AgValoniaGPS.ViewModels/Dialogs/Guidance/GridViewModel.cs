using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for grid guidance setup dialog (FormGrid).
/// Configures grid guidance with spacing, angle, and origin point.
/// </summary>
public class GridViewModel : DialogViewModelBase
{
    private double _gridSpacing = 10.0;
    private double _gridAngle;
    private Position? _gridOrigin;
    private bool _showGrid = true;
    private int _numberOfLines = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridViewModel"/> class.
    /// </summary>
    public GridViewModel()
    {
        SetOriginCommand = new RelayCommand(OnSetOrigin);
        RotateGridCommand = new RelayCommand<double>(OnRotateGrid);
        ApplyGridCommand = new RelayCommand(OnApplyGrid);

        // Initialize with default origin (will be replaced by current position)
        _gridOrigin = new Position { Latitude = 0, Longitude = 0 };
    }

    /// <summary>
    /// Gets or sets the grid spacing (distance between grid lines) in meters.
    /// Valid range: 1-100 meters.
    /// </summary>
    public double GridSpacing
    {
        get => _gridSpacing;
        set
        {
            SetProperty(ref _gridSpacing, Math.Clamp(value, 1.0, 100.0));
            OnPropertyChanged(nameof(GridSpacingFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted grid spacing string.
    /// </summary>
    public string GridSpacingFormatted => $"{GridSpacing:F1} m";

    /// <summary>
    /// Gets or sets the grid rotation angle in degrees (0-360).
    /// 0 degrees = North, 90 degrees = East.
    /// </summary>
    public double GridAngle
    {
        get => _gridAngle;
        set
        {
            // Normalize to 0-360 range
            var normalized = value % 360.0;
            if (normalized < 0) normalized += 360.0;

            SetProperty(ref _gridAngle, normalized);
            OnPropertyChanged(nameof(GridAngleFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted grid angle string.
    /// </summary>
    public string GridAngleFormatted => $"{GridAngle:F1}°";

    /// <summary>
    /// Gets or sets the grid origin (center point).
    /// </summary>
    public Position? GridOrigin
    {
        get => _gridOrigin;
        set
        {
            SetProperty(ref _gridOrigin, value);
            OnPropertyChanged(nameof(GridOriginFormatted));
            OnPropertyChanged(nameof(HasOrigin));
        }
    }

    /// <summary>
    /// Gets the formatted grid origin string.
    /// </summary>
    public string GridOriginFormatted =>
        GridOrigin != null
            ? $"{GridOrigin.Latitude:F6}°, {GridOrigin.Longitude:F6}°"
            : "No origin set";

    /// <summary>
    /// Gets a value indicating whether a grid origin has been set.
    /// </summary>
    public bool HasOrigin => GridOrigin != null;

    /// <summary>
    /// Gets or sets a value indicating whether to show the grid.
    /// </summary>
    public bool ShowGrid
    {
        get => _showGrid;
        set => SetProperty(ref _showGrid, value);
    }

    /// <summary>
    /// Gets or sets the number of parallel lines to display (1-20).
    /// This is the number of lines on each side of the origin.
    /// </summary>
    public int NumberOfLines
    {
        get => _numberOfLines;
        set
        {
            SetProperty(ref _numberOfLines, Math.Clamp(value, 1, 20));
            OnPropertyChanged(nameof(TotalLinesDisplay));
        }
    }

    /// <summary>
    /// Gets the total number of lines display text.
    /// </summary>
    public string TotalLinesDisplay => $"{NumberOfLines * 2 + 1} lines total";

    /// <summary>
    /// Gets the command to set the grid origin from current position.
    /// </summary>
    public ICommand SetOriginCommand { get; }

    /// <summary>
    /// Gets the command to rotate the grid by a specific angle.
    /// </summary>
    public ICommand RotateGridCommand { get; }

    /// <summary>
    /// Gets the command to apply the grid configuration.
    /// </summary>
    public ICommand ApplyGridCommand { get; }

    /// <summary>
    /// Sets the grid origin from current GPS position.
    /// </summary>
    private void OnSetOrigin()
    {
        // TODO: When position service is integrated, get current position
        // Example: GridOrigin = _positionUpdateService?.CurrentPosition;

        // For now, use placeholder
        GridOrigin = new Position { Latitude = 42.0308, Longitude = -93.6319 };
        ClearError();
    }

    /// <summary>
    /// Rotates the grid by the specified angle.
    /// </summary>
    /// <param name="angleDelta">Angle to add to current grid angle (in degrees).</param>
    private void OnRotateGrid(double angleDelta)
    {
        GridAngle += angleDelta;
    }

    /// <summary>
    /// Applies the grid configuration and generates grid lines.
    /// </summary>
    private void OnApplyGrid()
    {
        if (GridOrigin == null)
        {
            SetError("Grid origin must be set before applying");
            return;
        }

        try
        {
            // TODO: When AB line service is integrated, generate grid lines
            // Example: _abLineService?.GenerateGrid(GridOrigin, GridSpacing, GridAngle, NumberOfLines);

            ClearError();
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Error applying grid: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates grid configuration before closing.
    /// </summary>
    protected override void OnOK()
    {
        if (GridOrigin == null)
        {
            SetError("Grid origin must be set");
            return;
        }

        if (GridSpacing < 1.0 || GridSpacing > 100.0)
        {
            SetError("Grid spacing must be between 1 and 100 meters");
            return;
        }

        base.OnOK();
    }
}
