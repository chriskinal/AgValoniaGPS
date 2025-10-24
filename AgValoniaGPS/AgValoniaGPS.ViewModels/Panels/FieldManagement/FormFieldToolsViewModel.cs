using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.ViewModels.Base;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.FieldManagement;

/// <summary>
/// ViewModel for the Field Tools panel providing headland generation and field utilities.
/// Manages headland configuration, area calculations, and field data export.
/// </summary>
public partial class FormFieldToolsViewModel : PanelViewModelBase
{
    private readonly IFieldService _fieldService;
    private readonly IBoundaryManagementService _boundaryService;
    private readonly IHeadlandService _headlandService;

    private int _headlandPasses = 2;
    private double _headlandSpacing = 12.0;
    private double _headlandAreaHectares;
    private double _innerBoundaryAreaHectares;

    public FormFieldToolsViewModel(
        IFieldService fieldService,
        IBoundaryManagementService boundaryService,
        IHeadlandService headlandService)
    {
        _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
        _boundaryService = boundaryService ?? throw new ArgumentNullException(nameof(boundaryService));
        _headlandService = headlandService ?? throw new ArgumentNullException(nameof(headlandService));

        Title = "Field Tools";

        // Commands
        GenerateHeadlandCommand = new RelayCommand(OnGenerateHeadland);
        ClearHeadlandCommand = new RelayCommand(OnClearHeadland);
        CalculateFieldAreaCommand = new RelayCommand(OnCalculateFieldArea);
        ExportFieldDataCommand = new RelayCommand(OnExportFieldData);

        // Initialize values
        UpdateAreaCalculations();
    }

    public string Title { get; } = "Field Tools";

    /// <summary>
    /// Number of headland passes (1-10)
    /// </summary>
    public int HeadlandPasses
    {
        get => _headlandPasses;
        set
        {
            var clampedValue = Math.Clamp(value, 1, 10);
            SetProperty(ref _headlandPasses, clampedValue);
            UpdateAreaCalculations();
        }
    }

    /// <summary>
    /// Headland spacing in meters (typically implement width)
    /// </summary>
    public double HeadlandSpacing
    {
        get => _headlandSpacing;
        set
        {
            var clampedValue = Math.Max(1.0, value);
            SetProperty(ref _headlandSpacing, clampedValue);
            UpdateAreaCalculations();
        }
    }

    /// <summary>
    /// Calculated headland area in hectares
    /// </summary>
    public double HeadlandAreaHectares
    {
        get => _headlandAreaHectares;
        set => SetProperty(ref _headlandAreaHectares, value);
    }

    /// <summary>
    /// Inner boundary area (field area minus headland) in hectares
    /// </summary>
    public double InnerBoundaryAreaHectares
    {
        get => _innerBoundaryAreaHectares;
        set => SetProperty(ref _innerBoundaryAreaHectares, value);
    }

    public ICommand GenerateHeadlandCommand { get; }
    public ICommand ClearHeadlandCommand { get; }
    public ICommand CalculateFieldAreaCommand { get; }
    public ICommand ExportFieldDataCommand { get; }

    private void OnGenerateHeadland()
    {
        try
        {
            var boundary = _boundaryService.GetCurrentBoundary();
            if (boundary == null || boundary.Length < 3)
            {
                SetError("No valid boundary loaded. Please create a boundary first.");
                return;
            }

            _headlandService.GenerateHeadlands(boundary, HeadlandSpacing, HeadlandPasses);

            UpdateAreaCalculations();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to generate headland: {ex.Message}");
        }
    }

    private void OnClearHeadland()
    {
        try
        {
            _headlandService.ClearHeadlands();
            HeadlandAreaHectares = 0;
            UpdateAreaCalculations();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to clear headland: {ex.Message}");
        }
    }

    private void OnCalculateFieldArea()
    {
        try
        {
            UpdateAreaCalculations();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to calculate field area: {ex.Message}");
        }
    }

    private void OnExportFieldData()
    {
        try
        {
            // Export field data will be implemented when file service is available
            SetError("Export field data not yet implemented");
        }
        catch (Exception ex)
        {
            SetError($"Failed to export field data: {ex.Message}");
        }
    }

    private void UpdateAreaCalculations()
    {
        try
        {
            var boundary = _boundaryService.GetCurrentBoundary();
            if (boundary != null && boundary.Length >= 3)
            {
                var totalAreaSquareMeters = _boundaryService.CalculateArea();
                var totalAreaHectares = totalAreaSquareMeters / 10000.0;

                // Estimate headland area (simplified calculation)
                // Real calculation would use actual headland polygons
                var boundaryPerimeter = CalculatePerimeter(boundary);
                var headlandWidth = HeadlandPasses * HeadlandSpacing;
                var estimatedHeadlandAreaSquareMeters = boundaryPerimeter * headlandWidth;

                HeadlandAreaHectares = estimatedHeadlandAreaSquareMeters / 10000.0;
                InnerBoundaryAreaHectares = Math.Max(0, totalAreaHectares - HeadlandAreaHectares);
            }
            else
            {
                HeadlandAreaHectares = 0;
                InnerBoundaryAreaHectares = 0;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to update area calculations: {ex.Message}");
        }
    }

    private double CalculatePerimeter(Models.Position[] boundary)
    {
        double perimeter = 0;
        for (int i = 0; i < boundary.Length; i++)
        {
            var p1 = boundary[i];
            var p2 = boundary[(i + 1) % boundary.Length];

            var dx = p2.Easting - p1.Easting;
            var dy = p2.Northing - p1.Northing;
            perimeter += Math.Sqrt(dx * dx + dy * dy);
        }
        return perimeter;
    }
}
