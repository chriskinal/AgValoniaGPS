using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for AB line smoothing dialog (FormSmoothAB).
/// Smooths AB lines using Douglas-Peucker algorithm with adjustable tolerance.
/// </summary>
public class SmoothABViewModel : DialogViewModelBase
{
    private readonly List<Position> _originalPoints;
    private double _smoothingTolerance = 1.0;
    private int _originalPointCount;
    private int _smoothedPointCount;
    private bool _hasPreview;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmoothABViewModel"/> class.
    /// </summary>
    /// <param name="recordedPoints">Recorded track points to smooth.</param>
    public SmoothABViewModel(List<Position>? recordedPoints = null)
    {
        _originalPoints = recordedPoints ?? new List<Position>();
        _originalPointCount = _originalPoints.Count;
        _smoothedPointCount = _originalPointCount;

        RecordedPoints = new ObservableCollection<Position>(_originalPoints);
        SmoothedPoints = new ObservableCollection<Position>();

        LoadTrackCommand = new RelayCommand(OnLoadTrack);
        PreviewSmoothingCommand = new RelayCommand(OnPreviewSmoothing);
        ApplySmoothingCommand = new RelayCommand(OnApplySmoothing);
        ResetCommand = new RelayCommand(OnReset);
    }

    /// <summary>
    /// Gets the recorded points collection.
    /// </summary>
    public ObservableCollection<Position> RecordedPoints { get; }

    /// <summary>
    /// Gets the smoothed points collection for preview.
    /// </summary>
    public ObservableCollection<Position> SmoothedPoints { get; }

    /// <summary>
    /// Gets or sets the smoothing tolerance in meters (0.1-10m).
    /// Higher values remove more points for smoother lines.
    /// </summary>
    public double SmoothingTolerance
    {
        get => _smoothingTolerance;
        set
        {
            SetProperty(ref _smoothingTolerance, Math.Clamp(value, 0.1, 10.0));
            _hasPreview = false; // Clear preview when tolerance changes
            OnPropertyChanged(nameof(HasPreview));
        }
    }

    /// <summary>
    /// Gets or sets the original point count before smoothing.
    /// </summary>
    public int OriginalPointCount
    {
        get => _originalPointCount;
        set
        {
            SetProperty(ref _originalPointCount, value);
            OnPropertyChanged(nameof(OriginalPointCountDisplay));
            UpdateReductionPercentage();
        }
    }

    /// <summary>
    /// Gets the original point count display text.
    /// </summary>
    public string OriginalPointCountDisplay => $"{OriginalPointCount} points";

    /// <summary>
    /// Gets or sets the smoothed point count after smoothing.
    /// </summary>
    public int SmoothedPointCount
    {
        get => _smoothedPointCount;
        set
        {
            SetProperty(ref _smoothedPointCount, value);
            OnPropertyChanged(nameof(SmoothedPointCountDisplay));
            UpdateReductionPercentage();
        }
    }

    /// <summary>
    /// Gets the smoothed point count display text.
    /// </summary>
    public string SmoothedPointCountDisplay => $"{SmoothedPointCount} points";

    /// <summary>
    /// Gets the percentage of points removed by smoothing.
    /// </summary>
    public double ReductionPercentage { get; private set; }

    /// <summary>
    /// Gets the reduction percentage display text.
    /// </summary>
    public string ReductionPercentageDisplay => $"{ReductionPercentage:F1}% reduction";

    /// <summary>
    /// Gets or sets a value indicating whether a preview is available.
    /// </summary>
    public bool HasPreview
    {
        get => _hasPreview;
        private set => SetProperty(ref _hasPreview, value);
    }

    /// <summary>
    /// Gets the command to load a recorded track.
    /// </summary>
    public ICommand LoadTrackCommand { get; }

    /// <summary>
    /// Gets the command to preview smoothing result.
    /// </summary>
    public ICommand PreviewSmoothingCommand { get; }

    /// <summary>
    /// Gets the command to apply smoothing to AB line.
    /// </summary>
    public ICommand ApplySmoothingCommand { get; }

    /// <summary>
    /// Gets the command to reset to original points.
    /// </summary>
    public ICommand ResetCommand { get; }

    /// <summary>
    /// Loads a recorded track from file or service.
    /// </summary>
    private void OnLoadTrack()
    {
        // TODO: Integrate with track recording service
        // For now, use existing points
        ClearError();
    }

    /// <summary>
    /// Previews the smoothing result without applying it.
    /// </summary>
    private void OnPreviewSmoothing()
    {
        if (RecordedPoints.Count < 3)
        {
            SetError("Need at least 3 points to smooth");
            return;
        }

        try
        {
            var smoothed = DouglasPeuckerSmoothing(RecordedPoints.ToList(), SmoothingTolerance);

            SmoothedPoints.Clear();
            foreach (var point in smoothed)
            {
                SmoothedPoints.Add(point);
            }

            SmoothedPointCount = smoothed.Count;
            HasPreview = true;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Error smoothing: {ex.Message}");
            HasPreview = false;
        }
    }

    /// <summary>
    /// Applies smoothing to the AB line.
    /// </summary>
    private void OnApplySmoothing()
    {
        if (!HasPreview)
        {
            SetError("Preview smoothing before applying");
            return;
        }

        // TODO: When AB line service is integrated, update the AB line
        // Example: _abLineService?.UpdateLinePoints(SmoothedPoints.ToList());

        RequestClose(true);
    }

    /// <summary>
    /// Resets to original points without smoothing.
    /// </summary>
    private void OnReset()
    {
        RecordedPoints.Clear();
        foreach (var point in _originalPoints)
        {
            RecordedPoints.Add(point);
        }

        SmoothedPoints.Clear();
        SmoothedPointCount = OriginalPointCount;
        HasPreview = false;
        ClearError();
    }

    /// <summary>
    /// Updates the reduction percentage calculation.
    /// </summary>
    private void UpdateReductionPercentage()
    {
        if (OriginalPointCount > 0)
        {
            ReductionPercentage = ((OriginalPointCount - SmoothedPointCount) / (double)OriginalPointCount) * 100.0;
        }
        else
        {
            ReductionPercentage = 0;
        }

        OnPropertyChanged(nameof(ReductionPercentage));
        OnPropertyChanged(nameof(ReductionPercentageDisplay));
    }

    /// <summary>
    /// Douglas-Peucker line simplification algorithm.
    /// </summary>
    /// <param name="points">Points to simplify.</param>
    /// <param name="tolerance">Tolerance in meters.</param>
    /// <returns>Simplified point list.</returns>
    private List<Position> DouglasPeuckerSmoothing(List<Position> points, double tolerance)
    {
        if (points.Count < 3)
        {
            return new List<Position>(points);
        }

        // Find the point with maximum distance from line between first and last points
        double maxDistance = 0;
        int maxIndex = 0;

        var first = points[0];
        var last = points[points.Count - 1];

        for (int i = 1; i < points.Count - 1; i++)
        {
            double distance = PerpendicularDistance(points[i], first, last);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        // If max distance is greater than tolerance, recursively simplify
        if (maxDistance > tolerance)
        {
            var left = DouglasPeuckerSmoothing(points.GetRange(0, maxIndex + 1), tolerance);
            var right = DouglasPeuckerSmoothing(points.GetRange(maxIndex, points.Count - maxIndex), tolerance);

            // Combine results (remove duplicate middle point)
            var result = new List<Position>(left);
            result.AddRange(right.Skip(1));
            return result;
        }
        else
        {
            // All points between first and last can be removed
            return new List<Position> { first, last };
        }
    }

    /// <summary>
    /// Calculates perpendicular distance from a point to a line.
    /// </summary>
    private double PerpendicularDistance(Position point, Position lineStart, Position lineEnd)
    {
        // Convert to local Cartesian coordinates (approximate for short distances)
        double x = point.Longitude;
        double y = point.Latitude;
        double x1 = lineStart.Longitude;
        double y1 = lineStart.Latitude;
        double x2 = lineEnd.Longitude;
        double y2 = lineEnd.Latitude;

        double A = x - x1;
        double B = y - y1;
        double C = x2 - x1;
        double D = y2 - y1;

        double dot = A * C + B * D;
        double lenSq = C * C + D * D;

        if (lenSq == 0)
        {
            return Math.Sqrt(A * A + B * B) * 111320.0; // Approximate meters per degree
        }

        double param = dot / lenSq;

        double xx, yy;

        if (param < 0)
        {
            xx = x1;
            yy = y1;
        }
        else if (param > 1)
        {
            xx = x2;
            yy = y2;
        }
        else
        {
            xx = x1 + param * C;
            yy = y1 + param * D;
        }

        double dx = x - xx;
        double dy = y - yy;

        // Convert to meters (approximate)
        return Math.Sqrt(dx * dx + dy * dy) * 111320.0;
    }
}
