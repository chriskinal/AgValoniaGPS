using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for boundary management dialog (FormBoundary)
/// Create, edit, delete, load, and save boundaries
/// </summary>
public class BoundaryViewModel : DialogViewModelBase
{
    private readonly IBoundaryManagementService? _boundaryService;
    private readonly IBoundaryFileService? _boundaryFileService;
    private double _boundaryArea;
    private bool _isValid;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundaryViewModel"/> class.
    /// </summary>
    /// <param name="boundaryService">Optional boundary management service.</param>
    /// <param name="boundaryFileService">Optional boundary file service.</param>
    public BoundaryViewModel(
        IBoundaryManagementService? boundaryService = null,
        IBoundaryFileService? boundaryFileService = null)
    {
        _boundaryService = boundaryService;
        _boundaryFileService = boundaryFileService;

        BoundaryPoints = new ObservableCollection<Position>();

        NewBoundaryCommand = ReactiveCommand.Create(OnNewBoundary);
        LoadBoundaryCommand = ReactiveCommand.Create(OnLoadBoundary);
        SaveBoundaryCommand = ReactiveCommand.Create(OnSaveBoundary,
            this.WhenAnyValue(x => x.IsValid).Select(valid => valid));
        SimplifyCommand = ReactiveCommand.Create(OnSimplify,
            this.WhenAnyValue(x => x.PointCount).Select(count => count > 3));
        ClearCommand = ReactiveCommand.Create(OnClear);

        UpdateCalculations();
    }

    /// <summary>
    /// Gets the boundary points collection.
    /// </summary>
    public ObservableCollection<Position> BoundaryPoints { get; }

    /// <summary>
    /// Gets the number of boundary points.
    /// </summary>
    public int PointCount => BoundaryPoints.Count;

    /// <summary>
    /// Gets or sets the calculated boundary area in hectares.
    /// </summary>
    public double BoundaryArea
    {
        get => _boundaryArea;
        set => this.RaiseAndSetIfChanged(ref _boundaryArea, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the boundary forms a valid closed polygon.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        set => this.RaiseAndSetIfChanged(ref _isValid, value);
    }

    /// <summary>
    /// Gets the command to clear and start a new boundary.
    /// </summary>
    public ICommand NewBoundaryCommand { get; }

    /// <summary>
    /// Gets the command to load a boundary from file.
    /// </summary>
    public ICommand LoadBoundaryCommand { get; }

    /// <summary>
    /// Gets the command to save the boundary to file.
    /// </summary>
    public ICommand SaveBoundaryCommand { get; }

    /// <summary>
    /// Gets the command to simplify the boundary (reduce points).
    /// </summary>
    public ICommand SimplifyCommand { get; }

    /// <summary>
    /// Gets the command to clear all points.
    /// </summary>
    public ICommand ClearCommand { get; }

    /// <summary>
    /// Adds a point to the boundary.
    /// </summary>
    /// <param name="position">The position to add.</param>
    public void AddPoint(Position position)
    {
        BoundaryPoints.Add(position);
        UpdateCalculations();
    }

    /// <summary>
    /// Removes a point from the boundary.
    /// </summary>
    /// <param name="position">The position to remove.</param>
    public void RemovePoint(Position position)
    {
        BoundaryPoints.Remove(position);
        UpdateCalculations();
    }

    /// <summary>
    /// Clears and starts a new boundary.
    /// </summary>
    private void OnNewBoundary()
    {
        BoundaryPoints.Clear();
        UpdateCalculations();
    }

    /// <summary>
    /// Loads a boundary from a file.
    /// </summary>
    private async void OnLoadBoundary()
    {
        if (_boundaryFileService == null)
        {
            SetError("Boundary file service not available.");
            return;
        }

        try
        {
            // In a real implementation, would show file picker
            // For now, placeholder
        }
        catch (Exception ex)
        {
            SetError($"Error loading boundary: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves the boundary to a file.
    /// </summary>
    private async void OnSaveBoundary()
    {
        if (!IsValid)
        {
            SetError("Cannot save invalid boundary.");
            return;
        }

        if (_boundaryFileService == null)
        {
            SetError("Boundary file service not available.");
            return;
        }

        try
        {
            // In a real implementation, would show save file dialog
            // For now, placeholder
        }
        catch (Exception ex)
        {
            SetError($"Error saving boundary: {ex.Message}");
        }
    }

    /// <summary>
    /// Simplifies the boundary using Douglas-Peucker algorithm.
    /// </summary>
    private void OnSimplify()
    {
        if (_boundaryService == null || BoundaryPoints.Count < 3)
        {
            return;
        }

        try
        {
            var simplified = _boundaryService.SimplifyBoundary(1.0); // 1 meter tolerance

            BoundaryPoints.Clear();
            foreach (var point in simplified)
            {
                BoundaryPoints.Add(point);
            }

            UpdateCalculations();
        }
        catch (Exception ex)
        {
            SetError($"Error simplifying boundary: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears all boundary points.
    /// </summary>
    private void OnClear()
    {
        BoundaryPoints.Clear();
        UpdateCalculations();
    }

    /// <summary>
    /// Updates boundary calculations (area, validity).
    /// </summary>
    private void UpdateCalculations()
    {
        this.RaisePropertyChanged(nameof(PointCount));

        // A valid boundary needs at least 3 points and should be closed
        IsValid = BoundaryPoints.Count >= 3;

        if (IsValid && _boundaryService != null)
        {
            try
            {
                // Load into service for area calculation
                _boundaryService.LoadBoundary(BoundaryPoints.ToArray());
                BoundaryArea = _boundaryService.CalculateArea() / 10000.0; // Convert m² to hectares
            }
            catch
            {
                BoundaryArea = 0;
            }
        }
        else
        {
            BoundaryArea = 0;
        }
    }
}
