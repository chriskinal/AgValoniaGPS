using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implementation of boundary management service with support for loading, validation, simplification, and violation detection
/// </summary>
public class BoundaryManagementService : IBoundaryManagementService
{
    private readonly IPointInPolygonService _pointInPolygonService;
    private readonly object _lock = new();
    private Position[]? _currentBoundary;

    public event EventHandler<BoundaryViolationEventArgs>? BoundaryViolation;

    /// <summary>
    /// Creates a new instance of BoundaryManagementService
    /// </summary>
    /// <param name="pointInPolygonService">Point-in-polygon service for boundary checks</param>
    public BoundaryManagementService(IPointInPolygonService pointInPolygonService)
    {
        _pointInPolygonService = pointInPolygonService ?? throw new ArgumentNullException(nameof(pointInPolygonService));
    }

    public void LoadBoundary(Position[] boundary)
    {
        lock (_lock)
        {
            if (boundary == null || boundary.Length == 0)
            {
                _currentBoundary = null;
                _pointInPolygonService.ClearSpatialIndex();
                return;
            }

            _currentBoundary = boundary;

            // Build spatial index if boundary is large enough
            if (boundary.Length > 500)
            {
                _pointInPolygonService.BuildSpatialIndex(boundary);
            }
        }
    }

    public void ClearBoundary()
    {
        lock (_lock)
        {
            _currentBoundary = null;
            _pointInPolygonService.ClearSpatialIndex();
        }
    }

    public Position[]? GetCurrentBoundary()
    {
        lock (_lock)
        {
            return _currentBoundary?.ToArray(); // Return copy to prevent external modification
        }
    }

    public bool HasBoundary()
    {
        lock (_lock)
        {
            return _currentBoundary != null && _currentBoundary.Length > 0;
        }
    }

    public bool IsInsideBoundary(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            if (_currentBoundary == null || _currentBoundary.Length < 3)
                return false;

            return _pointInPolygonService.IsPointInside(position, _currentBoundary);
        }
    }

    public double CalculateArea()
    {
        lock (_lock)
        {
            if (_currentBoundary == null || _currentBoundary.Length < 3)
                return 0.0;

            return CalculateAreaShoelace(_currentBoundary);
        }
    }

    public Position[] SimplifyBoundary(double tolerance)
    {
        if (tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be non-negative");

        lock (_lock)
        {
            if (_currentBoundary == null || _currentBoundary.Length < 3)
                return Array.Empty<Position>();

            return DouglasPeucker(_currentBoundary, tolerance);
        }
    }

    public void CheckPosition(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        if (!HasBoundary())
            return;

        bool isInside = IsInsideBoundary(position);

        if (!isInside)
        {
            // Calculate distance from boundary
            double distance = CalculateDistanceToBoundary(position);
            BoundaryViolation?.Invoke(this, new BoundaryViolationEventArgs(position, distance));
        }
    }

    /// <summary>
    /// Calculates polygon area using Shoelace formula
    /// </summary>
    /// <remarks>
    /// Formula: Area = 0.5 * |Σ(xi * yi+1 - xi+1 * yi)|
    /// Uses UTM coordinates (Easting/Northing) for accurate area in square meters
    /// </remarks>
    private double CalculateAreaShoelace(Position[] polygon)
    {
        if (polygon.Length < 3)
            return 0.0;

        double area = 0.0;
        int n = polygon.Length;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += polygon[i].Easting * polygon[j].Northing;
            area -= polygon[j].Easting * polygon[i].Northing;
        }

        return Math.Abs(area) / 2.0;
    }

    /// <summary>
    /// Simplifies a polygon using Douglas-Peucker algorithm
    /// </summary>
    /// <remarks>
    /// Recursive algorithm that finds the point with maximum perpendicular distance from the line segment
    /// and splits the polygon at that point if distance exceeds tolerance.
    /// Time complexity: O(n log n) average, O(n²) worst case
    /// </remarks>
    private Position[] DouglasPeucker(Position[] points, double tolerance)
    {
        if (points.Length < 3)
            return points;

        // Find the point with maximum distance from line segment (first to last)
        double maxDistance = 0.0;
        int maxIndex = 0;

        for (int i = 1; i < points.Length - 1; i++)
        {
            double distance = PerpendicularDistance(points[i], points[0], points[points.Length - 1]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        // If max distance is greater than tolerance, recursively simplify
        if (maxDistance > tolerance)
        {
            // Recursively simplify left and right segments
            Position[] leftSegment = new Position[maxIndex + 1];
            Array.Copy(points, 0, leftSegment, 0, maxIndex + 1);
            Position[] leftResult = DouglasPeucker(leftSegment, tolerance);

            Position[] rightSegment = new Position[points.Length - maxIndex];
            Array.Copy(points, maxIndex, rightSegment, 0, points.Length - maxIndex);
            Position[] rightResult = DouglasPeucker(rightSegment, tolerance);

            // Combine results (excluding duplicate point at join)
            Position[] result = new Position[leftResult.Length + rightResult.Length - 1];
            Array.Copy(leftResult, 0, result, 0, leftResult.Length);
            Array.Copy(rightResult, 1, result, leftResult.Length, rightResult.Length - 1);

            return result;
        }
        else
        {
            // All points between first and last can be removed
            return new Position[] { points[0], points[points.Length - 1] };
        }
    }

    /// <summary>
    /// Calculates perpendicular distance from a point to a line segment
    /// </summary>
    private double PerpendicularDistance(Position point, Position lineStart, Position lineEnd)
    {
        double x0 = point.Easting;
        double y0 = point.Northing;
        double x1 = lineStart.Easting;
        double y1 = lineStart.Northing;
        double x2 = lineEnd.Easting;
        double y2 = lineEnd.Northing;

        // Calculate line segment length squared
        double dx = x2 - x1;
        double dy = y2 - y1;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
        {
            // Line segment is a point
            return Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
        }

        // Calculate perpendicular distance using cross product
        double numerator = Math.Abs((y2 - y1) * x0 - (x2 - x1) * y0 + x2 * y1 - y2 * x1);
        return numerator / Math.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Calculates minimum distance from a point to the boundary
    /// </summary>
    private double CalculateDistanceToBoundary(Position point)
    {
        if (_currentBoundary == null || _currentBoundary.Length < 2)
            return double.MaxValue;

        double minDistance = double.MaxValue;

        for (int i = 0; i < _currentBoundary.Length; i++)
        {
            int j = (i + 1) % _currentBoundary.Length;
            double distance = DistanceToSegment(point, _currentBoundary[i], _currentBoundary[j]);
            minDistance = Math.Min(minDistance, distance);
        }

        return minDistance;
    }

    /// <summary>
    /// Calculates distance from a point to a line segment
    /// </summary>
    private double DistanceToSegment(Position point, Position segmentStart, Position segmentEnd)
    {
        double x = point.Easting;
        double y = point.Northing;
        double x1 = segmentStart.Easting;
        double y1 = segmentStart.Northing;
        double x2 = segmentEnd.Easting;
        double y2 = segmentEnd.Northing;

        double dx = x2 - x1;
        double dy = y2 - y1;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
        {
            // Segment is a point
            return Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
        }

        // Calculate parameter t for projection of point onto line
        double t = Math.Max(0, Math.Min(1, ((x - x1) * dx + (y - y1) * dy) / lengthSquared));

        // Calculate closest point on segment
        double closestX = x1 + t * dx;
        double closestY = y1 + t * dy;

        // Return distance to closest point
        return Math.Sqrt((x - closestX) * (x - closestX) + (y - closestY) * (y - closestY));
    }
}
