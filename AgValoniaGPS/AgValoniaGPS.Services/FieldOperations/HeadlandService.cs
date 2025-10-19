using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for headland generation, tracking, and management.
/// Provides real-time headland entry/exit detection and completion tracking.
/// </summary>
public class HeadlandService : IHeadlandService
{
    private readonly IPointInPolygonService _pointInPolygonService;
    private readonly object _lock = new();

    private Position[][]? _headlands;
    private HashSet<int> _completedPasses = new();
    private bool _wasInHeadland = false;
    private int _lastPassNumber = -1;
    private HeadlandMode _mode = HeadlandMode.Auto;

    /// <inheritdoc/>
    public event EventHandler<HeadlandEntryEventArgs>? HeadlandEntry;

    /// <inheritdoc/>
    public event EventHandler<HeadlandExitEventArgs>? HeadlandExit;

    /// <inheritdoc/>
    public event EventHandler<HeadlandCompletedEventArgs>? HeadlandCompleted;

    /// <summary>
    /// Creates a new instance of HeadlandService
    /// </summary>
    /// <param name="pointInPolygonService">Service for point-in-polygon checks</param>
    public HeadlandService(IPointInPolygonService pointInPolygonService)
    {
        _pointInPolygonService = pointInPolygonService ?? throw new ArgumentNullException(nameof(pointInPolygonService));
    }

    /// <inheritdoc/>
    public void GenerateHeadlands(Position[] boundary, double passWidth, int passCount)
    {
        if (boundary == null || boundary.Length < 3)
            throw new ArgumentException("Boundary must have at least 3 points", nameof(boundary));
        if (passWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(passWidth), "Pass width must be positive");
        if (passCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(passCount), "Pass count must be positive");

        lock (_lock)
        {
            var headlands = new List<Position[]>();

            for (int pass = 0; pass < passCount; pass++)
            {
                double offsetDistance = passWidth * (pass + 1);
                var offsetPath = GenerateOffsetPolygon(boundary, offsetDistance);

                if (offsetPath != null && offsetPath.Length >= 3)
                {
                    headlands.Add(offsetPath);
                }
            }

            _headlands = headlands.ToArray();
            _completedPasses.Clear();
            _wasInHeadland = false;
            _lastPassNumber = -1;
        }
    }

    /// <inheritdoc/>
    public void LoadHeadlands(Position[][] headlands)
    {
        if (headlands == null)
            throw new ArgumentNullException(nameof(headlands));

        lock (_lock)
        {
            _headlands = headlands;
            _completedPasses.Clear();
            _wasInHeadland = false;
            _lastPassNumber = -1;
        }
    }

    /// <inheritdoc/>
    public void ClearHeadlands()
    {
        lock (_lock)
        {
            _headlands = null;
            _completedPasses.Clear();
            _wasInHeadland = false;
            _lastPassNumber = -1;
        }
    }

    /// <inheritdoc/>
    public Position[][]? GetHeadlands()
    {
        lock (_lock)
        {
            return _headlands;
        }
    }

    /// <inheritdoc/>
    public bool IsInHeadland(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            if (_headlands == null || _headlands.Length == 0)
                return false;

            // Check all headland passes
            foreach (var headland in _headlands)
            {
                if (_pointInPolygonService.IsPointInside(position, headland))
                    return true;
            }

            return false;
        }
    }

    /// <inheritdoc/>
    public int GetCurrentPass(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            if (_headlands == null || _headlands.Length == 0)
                return -1;

            // Check passes from innermost to outermost (higher pass numbers first)
            for (int i = _headlands.Length - 1; i >= 0; i--)
            {
                if (_pointInPolygonService.IsPointInside(position, _headlands[i]))
                    return i;
            }

            return -1;
        }
    }

    /// <inheritdoc/>
    public void MarkPassCompleted(int passNumber)
    {
        if (passNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(passNumber), "Pass number must be non-negative");

        lock (_lock)
        {
            if (!_completedPasses.Contains(passNumber))
            {
                _completedPasses.Add(passNumber);

                // Calculate area (simplified - use headland polygon area)
                double area = 0;
                if (_headlands != null && passNumber < _headlands.Length)
                {
                    area = CalculatePolygonArea(_headlands[passNumber]);
                }

                HeadlandCompleted?.Invoke(this, new HeadlandCompletedEventArgs(passNumber, area));
            }
        }
    }

    /// <inheritdoc/>
    public bool IsPassCompleted(int passNumber)
    {
        if (passNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(passNumber), "Pass number must be non-negative");

        lock (_lock)
        {
            return _completedPasses.Contains(passNumber);
        }
    }

    /// <inheritdoc/>
    public void CheckPosition(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        if (_mode != HeadlandMode.Auto)
            return;

        lock (_lock)
        {
            bool isInHeadland = IsInHeadland(position);
            int currentPass = GetCurrentPass(position);

            // Detect entry
            if (!_wasInHeadland && isInHeadland)
            {
                _wasInHeadland = true;
                _lastPassNumber = currentPass;
                HeadlandEntry?.Invoke(this, new HeadlandEntryEventArgs(currentPass, position));
            }
            // Detect exit
            else if (_wasInHeadland && !isInHeadland)
            {
                _wasInHeadland = false;
                HeadlandExit?.Invoke(this, new HeadlandExitEventArgs(_lastPassNumber, position));
                _lastPassNumber = -1;
            }
            // Update pass number if moved to different pass
            else if (_wasInHeadland && currentPass != _lastPassNumber)
            {
                _lastPassNumber = currentPass;
            }
        }
    }

    /// <inheritdoc/>
    public void SetMode(HeadlandMode mode)
    {
        lock (_lock)
        {
            _mode = mode;
        }
    }

    /// <inheritdoc/>
    public HeadlandMode GetMode()
    {
        lock (_lock)
        {
            return _mode;
        }
    }

    /// <summary>
    /// Generate offset polygon from boundary (simplified algorithm)
    /// Uses uniform inward scaling for simplicity - full parallel offset is complex
    /// </summary>
    private Position[] GenerateOffsetPolygon(Position[] boundary, double offsetDistance)
    {
        // Calculate centroid
        double centerEasting = 0, centerNorthing = 0;
        foreach (var point in boundary)
        {
            centerEasting += point.Easting;
            centerNorthing += point.Northing;
        }
        centerEasting /= boundary.Length;
        centerNorthing /= boundary.Length;

        // Calculate average distance from centroid
        double avgDistance = 0;
        foreach (var point in boundary)
        {
            double dx = point.Easting - centerEasting;
            double dy = point.Northing - centerNorthing;
            avgDistance += Math.Sqrt(dx * dx + dy * dy);
        }
        avgDistance /= boundary.Length;

        // Calculate scale factor to move inward by offsetDistance
        double scaleFactor = Math.Max(0.1, (avgDistance - offsetDistance) / avgDistance);

        // Generate offset points by scaling toward centroid
        var offsetPoints = new Position[boundary.Length];
        for (int i = 0; i < boundary.Length; i++)
        {
            double dx = boundary[i].Easting - centerEasting;
            double dy = boundary[i].Northing - centerNorthing;

            offsetPoints[i] = boundary[i] with
            {
                Easting = centerEasting + dx * scaleFactor,
                Northing = centerNorthing + dy * scaleFactor
            };
        }

        return offsetPoints;
    }

    /// <summary>
    /// Calculate polygon area using Shoelace formula
    /// </summary>
    private double CalculatePolygonArea(Position[] polygon)
    {
        if (polygon == null || polygon.Length < 3)
            return 0;

        double area = 0;
        int n = polygon.Length;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += polygon[i].Easting * polygon[j].Northing;
            area -= polygon[j].Easting * polygon[i].Northing;
        }

        return Math.Abs(area) / 2.0;
    }
}
