using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for boundary editing tools dialog (FormBndTool)
/// Provides tools for drawing, erasing, simplifying, and moving boundary points
/// </summary>
public class BndToolViewModel : DialogViewModelBase
{
    private BoundaryToolMode _currentMode = BoundaryToolMode.Draw;
    private int _drawingPointCount;
    private double _simplifyTolerance = 1.0; // Default 1 meter tolerance

    /// <summary>
    /// Initializes a new instance of the <see cref="BndToolViewModel"/> class.
    /// </summary>
    public BndToolViewModel()
    {
        SetDrawModeCommand = new RelayCommand(() => SetMode(BoundaryToolMode.Draw));
        SetEraseModeCommand = new RelayCommand(() => SetMode(BoundaryToolMode.Erase));
        SetSimplifyModeCommand = new RelayCommand(() => SetMode(BoundaryToolMode.Simplify));
        SetMoveModeCommand = new RelayCommand(() => SetMode(BoundaryToolMode.Move));
        UndoCommand = new RelayCommand(OnUndo);
        ClearCommand = new RelayCommand(OnClear);
    }

    /// <summary>
    /// Gets or sets the current tool mode.
    /// </summary>
    public BoundaryToolMode CurrentMode
    {
        get => _currentMode;
        set => SetProperty(ref _currentMode, value);
    }

    /// <summary>
    /// Gets or sets the number of points drawn in the current session.
    /// </summary>
    public int DrawingPointCount
    {
        get => _drawingPointCount;
        set => SetProperty(ref _drawingPointCount, value);
    }

    /// <summary>
    /// Gets or sets the simplify tolerance in meters.
    /// Higher values remove more points but may lose accuracy.
    /// </summary>
    public double SimplifyTolerance
    {
        get => _simplifyTolerance;
        set
        {
            if (value >= 0.1 && value <= 10.0)
            {
                SetProperty(ref _simplifyTolerance, value);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current mode is Draw.
    /// </summary>
    public bool IsDrawMode => CurrentMode == BoundaryToolMode.Draw;

    /// <summary>
    /// Gets a value indicating whether the current mode is Erase.
    /// </summary>
    public bool IsEraseMode => CurrentMode == BoundaryToolMode.Erase;

    /// <summary>
    /// Gets a value indicating whether the current mode is Simplify.
    /// </summary>
    public bool IsSimplifyMode => CurrentMode == BoundaryToolMode.Simplify;

    /// <summary>
    /// Gets a value indicating whether the current mode is Move.
    /// </summary>
    public bool IsMoveMode => CurrentMode == BoundaryToolMode.Move;

    /// <summary>
    /// Gets the command to set Draw mode.
    /// </summary>
    public ICommand SetDrawModeCommand { get; }

    /// <summary>
    /// Gets the command to set Erase mode.
    /// </summary>
    public ICommand SetEraseModeCommand { get; }

    /// <summary>
    /// Gets the command to set Simplify mode.
    /// </summary>
    public ICommand SetSimplifyModeCommand { get; }

    /// <summary>
    /// Gets the command to set Move mode.
    /// </summary>
    public ICommand SetMoveModeCommand { get; }

    /// <summary>
    /// Gets the command to undo the last point/operation.
    /// </summary>
    public ICommand UndoCommand { get; }

    /// <summary>
    /// Gets the command to clear all points.
    /// </summary>
    public ICommand ClearCommand { get; }

    /// <summary>
    /// Event raised when the tool mode changes.
    /// </summary>
    public event EventHandler<BoundaryToolMode>? ModeChanged;

    /// <summary>
    /// Event raised when undo is requested.
    /// </summary>
    public event EventHandler? UndoRequested;

    /// <summary>
    /// Event raised when clear all is requested.
    /// </summary>
    public event EventHandler? ClearRequested;

    /// <summary>
    /// Sets the current tool mode and notifies subscribers.
    /// </summary>
    private void SetMode(BoundaryToolMode mode)
    {
        CurrentMode = mode;
        OnPropertyChanged(nameof(IsDrawMode));
        OnPropertyChanged(nameof(IsEraseMode));
        OnPropertyChanged(nameof(IsSimplifyMode));
        OnPropertyChanged(nameof(IsMoveMode));
        ModeChanged?.Invoke(this, mode);
    }

    /// <summary>
    /// Called when Undo command is executed.
    /// </summary>
    private void OnUndo()
    {
        if (DrawingPointCount > 0)
        {
            DrawingPointCount--;
        }
        UndoRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when Clear command is executed.
    /// </summary>
    private void OnClear()
    {
        DrawingPointCount = 0;
        ClearRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Increments the drawing point count (called externally when a point is added).
    /// </summary>
    public void IncrementPointCount()
    {
        DrawingPointCount++;
    }
}
