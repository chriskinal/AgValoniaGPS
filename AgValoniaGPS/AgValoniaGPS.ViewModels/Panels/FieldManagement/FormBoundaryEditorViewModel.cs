using AgValoniaGPS.Models;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.FieldManagement;

/// <summary>
/// ViewModel for the Boundary Editor panel allowing users to draw and edit field boundaries.
/// Provides drawing mode, point capture, simplification, and area calculation.
/// </summary>
public partial class FormBoundaryEditorViewModel : PanelViewModelBase
{
    private readonly IBoundaryManagementService _boundaryService;
    private readonly IPositionUpdateService _positionService;

    private bool _isDrawingMode;
    private int _pointCount;
    private double _boundaryAreaHectares;
    private double _simplificationTolerance = 1.0;

    public FormBoundaryEditorViewModel(
        IBoundaryManagementService boundaryService,
        IPositionUpdateService positionService)
    {
        _boundaryService = boundaryService ?? throw new ArgumentNullException(nameof(boundaryService));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));

        Title = "Boundary Editor";

        BoundaryPoints = new ObservableCollection<Position>();

        // Commands
        StartDrawingCommand = ReactiveCommand.Create(OnStartDrawing, this.WhenAnyValue(x => x.IsDrawingMode, drawing => !drawing));
        StopDrawingCommand = ReactiveCommand.Create(OnStopDrawing, this.WhenAnyValue(x => x.IsDrawingMode));
        AddPointCommand = ReactiveCommand.Create(OnAddPoint, this.WhenAnyValue(x => x.IsDrawingMode));
        UndoLastPointCommand = ReactiveCommand.Create(OnUndoLastPoint, this.WhenAnyValue(x => x.PointCount, count => count > 0));
        ClearBoundaryCommand = ReactiveCommand.Create(OnClearBoundary);
        SimplifyBoundaryCommand = ReactiveCommand.Create(OnSimplifyBoundary, this.WhenAnyValue(x => x.PointCount, count => count > 3));
        SaveBoundaryCommand = ReactiveCommand.Create(OnSaveBoundary, this.WhenAnyValue(x => x.PointCount, count => count >= 3));

        // Load existing boundary if available
        LoadBoundary();
    }

    public string Title { get; } = "Boundary Editor";

    /// <summary>
    /// Collection of boundary points
    /// </summary>
    public ObservableCollection<Position> BoundaryPoints { get; }

    /// <summary>
    /// Whether the editor is in drawing mode
    /// </summary>
    public bool IsDrawingMode
    {
        get => _isDrawingMode;
        set => this.RaiseAndSetIfChanged(ref _isDrawingMode, value);
    }

    /// <summary>
    /// Number of points in the boundary
    /// </summary>
    public int PointCount
    {
        get => _pointCount;
        set => this.RaiseAndSetIfChanged(ref _pointCount, value);
    }

    /// <summary>
    /// Calculated boundary area in hectares
    /// </summary>
    public double BoundaryAreaHectares
    {
        get => _boundaryAreaHectares;
        set => this.RaiseAndSetIfChanged(ref _boundaryAreaHectares, value);
    }

    /// <summary>
    /// Simplification tolerance in meters (Douglas-Peucker)
    /// </summary>
    public double SimplificationTolerance
    {
        get => _simplificationTolerance;
        set => this.RaiseAndSetIfChanged(ref _simplificationTolerance, Math.Max(0.1, value));
    }

    public ICommand StartDrawingCommand { get; }
    public ICommand StopDrawingCommand { get; }
    public ICommand AddPointCommand { get; }
    public ICommand UndoLastPointCommand { get; }
    public ICommand ClearBoundaryCommand { get; }
    public ICommand SimplifyBoundaryCommand { get; }
    public ICommand SaveBoundaryCommand { get; }

    private void OnStartDrawing()
    {
        IsDrawingMode = true;
        ClearError();
    }

    private void OnStopDrawing()
    {
        IsDrawingMode = false;
        CalculateArea();
        ClearError();
    }

    private void OnAddPoint()
    {
        try
        {
            var geoCoord = _positionService.GetCurrentPosition();
            if (geoCoord == null)
            {
                SetError("No GPS position available");
                return;
            }

            var position = new Position
            {
                Easting = geoCoord.Easting,
                Northing = geoCoord.Northing,
                Altitude = geoCoord.Altitude
            };

            BoundaryPoints.Add(position);
            PointCount = BoundaryPoints.Count;

            if (PointCount >= 3)
            {
                CalculateArea();
            }

            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to add point: {ex.Message}");
        }
    }

    private void OnUndoLastPoint()
    {
        if (BoundaryPoints.Count > 0)
        {
            BoundaryPoints.RemoveAt(BoundaryPoints.Count - 1);
            PointCount = BoundaryPoints.Count;

            if (PointCount >= 3)
            {
                CalculateArea();
            }
            else
            {
                BoundaryAreaHectares = 0;
            }

            ClearError();
        }
    }

    private void OnClearBoundary()
    {
        BoundaryPoints.Clear();
        PointCount = 0;
        BoundaryAreaHectares = 0;
        IsDrawingMode = false;
        _boundaryService.ClearBoundary();
        ClearError();
    }

    private void OnSimplifyBoundary()
    {
        try
        {
            if (BoundaryPoints.Count < 3)
            {
                SetError("Need at least 3 points to simplify");
                return;
            }

            var simplified = _boundaryService.SimplifyBoundary(SimplificationTolerance);

            BoundaryPoints.Clear();
            foreach (var point in simplified)
            {
                BoundaryPoints.Add(point);
            }

            PointCount = BoundaryPoints.Count;
            CalculateArea();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to simplify boundary: {ex.Message}");
        }
    }

    private void OnSaveBoundary()
    {
        try
        {
            if (BoundaryPoints.Count < 3)
            {
                SetError("Need at least 3 points to save boundary");
                return;
            }

            var boundaryArray = BoundaryPoints.ToArray();
            _boundaryService.LoadBoundary(boundaryArray);

            IsDrawingMode = false;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save boundary: {ex.Message}");
        }
    }

    private void LoadBoundary()
    {
        try
        {
            var boundary = _boundaryService.GetCurrentBoundary();
            if (boundary != null && boundary.Length > 0)
            {
                BoundaryPoints.Clear();
                foreach (var point in boundary)
                {
                    BoundaryPoints.Add(point);
                }
                PointCount = BoundaryPoints.Count;
                CalculateArea();
            }
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load boundary: {ex.Message}");
        }
    }

    private void CalculateArea()
    {
        try
        {
            if (BoundaryPoints.Count >= 3)
            {
                // Load boundary temporarily to calculate area
                var tempBoundary = BoundaryPoints.ToArray();
                _boundaryService.LoadBoundary(tempBoundary);

                var areaSquareMeters = _boundaryService.CalculateArea();
                BoundaryAreaHectares = areaSquareMeters / 10000.0; // Convert to hectares
            }
            else
            {
                BoundaryAreaHectares = 0;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to calculate area: {ex.Message}");
        }
    }
}
