using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for headline guidance operations.
/// Implements moveable guidance paths as an alternative to AB lines.
/// Thread-safe for concurrent access.
/// </summary>
public class HeadlineService : IHeadlineService
{
    private readonly object _lock = new object();
    private readonly Dictionary<int, Headline> _headlines = new();
    private int _nextHeadlineId = 1;
    private int? _activeHeadlineId = null;

    private const double MinimumPathLength = 2.0; // meters
    private const int MinimumPointCount = 4;

    public event EventHandler<HeadlineChangedEventArgs>? HeadlineChanged;

    /// <summary>
    /// Creates a new headline from a recorded path.
    /// </summary>
    public Headline CreateHeadline(List<Position> trackPoints, string name, int aPointIndex = 0)
    {
        if (trackPoints == null || trackPoints.Count < MinimumPointCount)
        {
            throw new ArgumentException($"Track points must contain at least {MinimumPointCount} points. Got {trackPoints?.Count ?? 0}.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Headline name cannot be empty.", nameof(name));
        }

        if (aPointIndex < 0 || aPointIndex >= trackPoints.Count)
        {
            throw new ArgumentException($"A-point index {aPointIndex} is out of range [0, {trackPoints.Count - 1}].", nameof(aPointIndex));
        }

        lock (_lock)
        {
            // Calculate average heading from path
            double heading = CalculateAverageHeading(trackPoints);

            var headline = new Headline
            {
                Id = _nextHeadlineId++,
                Name = name,
                TrackPoints = new List<Position>(trackPoints), // Copy list
                MoveDistance = 0.0,
                Mode = 0,
                APointIndex = aPointIndex,
                Heading = heading,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            // Validate before adding
            if (!headline.Validate())
            {
                throw new InvalidOperationException("Created headline failed validation.");
            }

            _headlines[headline.Id] = headline;

            HeadlineChanged?.Invoke(this, new HeadlineChangedEventArgs(headline, HeadlineChangeType.Created));

            return headline;
        }
    }

    /// <summary>
    /// Creates a new headline from an existing position and heading.
    /// </summary>
    public Headline CreateHeadlineFromHeading(Position startPosition, double heading, string name, double length = 1000.0)
    {
        if (startPosition == null)
        {
            throw new ArgumentNullException(nameof(startPosition));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Headline name cannot be empty.", nameof(name));
        }

        if (length <= 0)
        {
            throw new ArgumentException("Length must be positive.", nameof(length));
        }

        // Normalize heading to [0, 2π)
        heading = NormalizeHeadingRadians(heading);

        // Generate a straight path with the specified heading
        var trackPoints = new List<Position>();
        int pointCount = Math.Max(MinimumPointCount, (int)(length / 10.0)); // Point every ~10m

        for (int i = 0; i < pointCount; i++)
        {
            double distance = (i * length) / (pointCount - 1);
            double easting = startPosition.Easting + distance * Math.Sin(heading);
            double northing = startPosition.Northing + distance * Math.Cos(heading);

            trackPoints.Add(new Position
            {
                Easting = easting,
                Northing = northing,
                Altitude = startPosition.Altitude,
                Latitude = startPosition.Latitude,
                Longitude = startPosition.Longitude,
                Zone = startPosition.Zone,
                Hemisphere = startPosition.Hemisphere
            });
        }

        return CreateHeadline(trackPoints, name, aPointIndex: 0);
    }

    /// <summary>
    /// Moves (nudges) a headline by the specified distance perpendicular to its path.
    /// </summary>
    public Headline MoveHeadline(int headlineId, double offsetMeters)
    {
        lock (_lock)
        {
            if (!_headlines.TryGetValue(headlineId, out var headline))
            {
                throw new ArgumentException($"Headline with ID {headlineId} not found.", nameof(headlineId));
            }

            // Calculate perpendicular offset for each point
            // Use local heading for each segment for more accurate offset on curved paths
            var movedPoints = new List<Position>();

            for (int i = 0; i < headline.TrackPoints.Count; i++)
            {
                // Calculate local perpendicular direction
                double perpHeading = CalculateLocalPerpendicularHeading(headline.TrackPoints, i);

                // Calculate offset vector
                double offsetEasting = offsetMeters * Math.Sin(perpHeading);
                double offsetNorthing = offsetMeters * Math.Cos(perpHeading);

                // Create moved point
                var originalPoint = headline.TrackPoints[i];
                movedPoints.Add(new Position
                {
                    Easting = originalPoint.Easting + offsetEasting,
                    Northing = originalPoint.Northing + offsetNorthing,
                    Altitude = originalPoint.Altitude,
                    Latitude = originalPoint.Latitude,
                    Longitude = originalPoint.Longitude,
                    Zone = originalPoint.Zone,
                    Hemisphere = originalPoint.Hemisphere
                });
            }

            // Update headline with new points
            headline.TrackPoints = movedPoints;
            headline.MoveDistance += offsetMeters;

            HeadlineChanged?.Invoke(this, new HeadlineChangedEventArgs(headline, HeadlineChangeType.Moved));

            return headline;
        }
    }

    /// <summary>
    /// Sets the specified headline as the active guidance line.
    /// </summary>
    public bool SetActiveHeadline(int headlineId)
    {
        lock (_lock)
        {
            if (!_headlines.ContainsKey(headlineId))
            {
                return false;
            }

            // Deactivate previous headline
            if (_activeHeadlineId.HasValue && _headlines.TryGetValue(_activeHeadlineId.Value, out var previousHeadline))
            {
                previousHeadline.IsActive = false;
                HeadlineChanged?.Invoke(this, new HeadlineChangedEventArgs(previousHeadline, HeadlineChangeType.Deactivated));
            }

            // Activate new headline
            var headline = _headlines[headlineId];
            headline.IsActive = true;
            _activeHeadlineId = headlineId;

            HeadlineChanged?.Invoke(this, new HeadlineChangedEventArgs(headline, HeadlineChangeType.Activated));

            return true;
        }
    }

    /// <summary>
    /// Gets the currently active headline.
    /// </summary>
    public Headline? GetActiveHeadline()
    {
        lock (_lock)
        {
            if (_activeHeadlineId.HasValue && _headlines.TryGetValue(_activeHeadlineId.Value, out var headline))
            {
                return headline;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets a headline by its ID.
    /// </summary>
    public Headline? GetHeadline(int headlineId)
    {
        lock (_lock)
        {
            return _headlines.TryGetValue(headlineId, out var headline) ? headline : null;
        }
    }

    /// <summary>
    /// Gets all headlines in the collection.
    /// </summary>
    public List<Headline> ListHeadlines()
    {
        lock (_lock)
        {
            return _headlines.Values.ToList();
        }
    }

    /// <summary>
    /// Deletes a headline from the collection.
    /// </summary>
    public bool DeleteHeadline(int headlineId)
    {
        lock (_lock)
        {
            if (!_headlines.TryGetValue(headlineId, out var headline))
            {
                return false;
            }

            // Deactivate if this was the active headline
            if (_activeHeadlineId == headlineId)
            {
                headline.IsActive = false;
                _activeHeadlineId = null;
            }

            _headlines.Remove(headlineId);

            HeadlineChanged?.Invoke(this, new HeadlineChangedEventArgs(headline, HeadlineChangeType.Deleted));

            return true;
        }
    }

    /// <summary>
    /// Clears all headlines from the collection.
    /// </summary>
    public void ClearAllHeadlines()
    {
        lock (_lock)
        {
            _headlines.Clear();
            _activeHeadlineId = null;
            _nextHeadlineId = 1;
        }
    }

    /// <summary>
    /// Calculates guidance information for the current position relative to the active headline.
    /// </summary>
    public GuidanceLineResult? CalculateGuidance(Position currentPosition, double currentHeading)
    {
        var activeHeadline = GetActiveHeadline();
        if (activeHeadline == null)
        {
            return null;
        }

        var closestPointResult = GetClosestPoint(currentPosition, activeHeadline);

        // Get heading at closest point
        double pathHeading = CalculatePathHeadingAtIndex(activeHeadline.TrackPoints, closestPointResult.Index);

        // Calculate cross-track error (distance from path)
        double xte = CalculateSignedCrossTrackError(currentPosition, closestPointResult.Position, pathHeading);

        // Calculate heading error
        double headingError = CalculateHeadingError(currentHeading, pathHeading);

        return new GuidanceLineResult
        {
            CrossTrackError = xte,
            ClosestPoint = closestPointResult.Position,
            HeadingError = headingError,
            DistanceToLine = Math.Abs(xte)
        };
    }

    /// <summary>
    /// Finds the closest point on a headline to the given position.
    /// </summary>
    public ClosestPointResult GetClosestPoint(Position position, Headline headline)
    {
        if (headline.TrackPoints == null || headline.TrackPoints.Count < 2)
        {
            throw new ArgumentException("Headline must have at least 2 track points.");
        }

        double minDistance = double.MaxValue;
        Position? closestPoint = null;
        double closestHeading = 0.0;
        int closestSegmentIndex = 0;

        // Check each line segment of the path
        for (int i = 0; i < headline.TrackPoints.Count - 1; i++)
        {
            var p1 = headline.TrackPoints[i];
            var p2 = headline.TrackPoints[i + 1];

            // Find closest point on this segment
            var result = GetClosestPointOnSegment(position, p1, p2);

            if (result.Distance < minDistance)
            {
                minDistance = result.Distance;
                closestPoint = result.Point;
                closestHeading = result.Heading;
                closestSegmentIndex = i;
            }
        }

        return new ClosestPointResult
        {
            Position = closestPoint ?? headline.TrackPoints[0],
            Distance = minDistance,
            Index = closestSegmentIndex
        };
    }

    /// <summary>
    /// Validates a headline for correctness.
    /// </summary>
    public ValidationResult ValidateHeadline(Headline headline)
    {
        var result = new ValidationResult { IsValid = true };

        if (headline == null)
        {
            result.AddError("Headline cannot be null");
            return result;
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(headline.Name))
        {
            result.AddError("Headline name cannot be empty");
        }

        // Validate point count
        if (headline.TrackPoints == null || headline.TrackPoints.Count < MinimumPointCount)
        {
            result.AddError($"Headline must have at least {MinimumPointCount} track points. Got {headline.TrackPoints?.Count ?? 0}.");
        }

        // Validate A-point index
        if (headline.TrackPoints != null)
        {
            if (headline.APointIndex < 0 || headline.APointIndex >= headline.TrackPoints.Count)
            {
                result.AddError($"A-point index {headline.APointIndex} is out of range [0, {headline.TrackPoints.Count - 1}]");
            }
        }

        // Validate points are not NaN or Infinity
        if (headline.TrackPoints != null)
        {
            for (int i = 0; i < headline.TrackPoints.Count; i++)
            {
                var point = headline.TrackPoints[i];
                if (!IsValidPosition(point))
                {
                    result.AddError($"Track point {i} contains invalid values (NaN or Infinity)");
                }
            }
        }

        // Validate heading
        if (double.IsNaN(headline.Heading) || double.IsInfinity(headline.Heading))
        {
            result.AddError("Heading contains invalid value (NaN or Infinity)");
        }

        return result;
    }

    #region Helper Methods

    /// <summary>
    /// Calculate average heading from a path of points.
    /// </summary>
    private double CalculateAverageHeading(List<Position> points)
    {
        if (points == null || points.Count < 2)
        {
            return 0.0;
        }

        double sumX = 0.0;
        double sumY = 0.0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            double dx = points[i + 1].Easting - points[i].Easting;
            double dy = points[i + 1].Northing - points[i].Northing;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length > 0.001)
            {
                sumX += dx / length;
                sumY += dy / length;
            }
        }

        return Math.Atan2(sumX, sumY);
    }

    /// <summary>
    /// Calculate local perpendicular heading for a point in the path.
    /// Used for accurate offset calculations on curved paths.
    /// </summary>
    private double CalculateLocalPerpendicularHeading(List<Position> points, int index)
    {
        double localHeading;

        if (index == 0)
        {
            // Use heading from first to second point
            localHeading = CalculateHeadingBetweenPoints(points[0], points[1]);
        }
        else if (index == points.Count - 1)
        {
            // Use heading from second-to-last to last point
            localHeading = CalculateHeadingBetweenPoints(points[index - 1], points[index]);
        }
        else
        {
            // Average heading from previous and next segments
            double h1 = CalculateHeadingBetweenPoints(points[index - 1], points[index]);
            double h2 = CalculateHeadingBetweenPoints(points[index], points[index + 1]);
            localHeading = AverageHeadings(h1, h2);
        }

        // Return perpendicular heading (90 degrees left for positive offset)
        return localHeading - (Math.PI / 2.0);
    }

    /// <summary>
    /// Calculate heading between two points in radians.
    /// </summary>
    private double CalculateHeadingBetweenPoints(Position p1, Position p2)
    {
        double dx = p2.Easting - p1.Easting;
        double dy = p2.Northing - p1.Northing;
        return Math.Atan2(dx, dy);
    }

    /// <summary>
    /// Average two heading angles accounting for circular nature of angles.
    /// </summary>
    private double AverageHeadings(double h1, double h2)
    {
        // Convert to unit vectors, average, then back to angle
        double x = (Math.Cos(h1) + Math.Cos(h2)) / 2.0;
        double y = (Math.Sin(h1) + Math.Sin(h2)) / 2.0;
        return Math.Atan2(y, x);
    }

    /// <summary>
    /// Find closest point on a line segment.
    /// </summary>
    private (Position Point, double Distance, double Heading) GetClosestPointOnSegment(Position position, Position segmentStart, Position segmentEnd)
    {
        // Vector from start to end
        double segmentDx = segmentEnd.Easting - segmentStart.Easting;
        double segmentDy = segmentEnd.Northing - segmentStart.Northing;
        double segmentLengthSquared = segmentDx * segmentDx + segmentDy * segmentDy;

        if (segmentLengthSquared < 0.000001)
        {
            // Degenerate segment, return start point
            double dist = CalculateDistance(position, segmentStart);
            return (segmentStart, dist, 0.0);
        }

        // Vector from start to position
        double dx = position.Easting - segmentStart.Easting;
        double dy = position.Northing - segmentStart.Northing;

        // Project position onto segment (dot product)
        double t = (dx * segmentDx + dy * segmentDy) / segmentLengthSquared;

        // Clamp t to [0, 1] to stay on segment
        t = Math.Max(0.0, Math.Min(1.0, t));

        // Calculate closest point
        Position closestPoint = new Position
        {
            Easting = segmentStart.Easting + t * segmentDx,
            Northing = segmentStart.Northing + t * segmentDy,
            Altitude = position.Altitude,
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            Zone = position.Zone,
            Hemisphere = position.Hemisphere
        };

        double distance = CalculateDistance(position, closestPoint);
        double heading = Math.Atan2(segmentDx, segmentDy);

        return (closestPoint, distance, heading);
    }

    /// <summary>
    /// Calculate signed cross-track error (positive = right of path, negative = left).
    /// </summary>
    private double CalculateSignedCrossTrackError(Position position, Position closestPoint, double pathHeading)
    {
        // Vector from closest point to current position
        double dx = position.Easting - closestPoint.Easting;
        double dy = position.Northing - closestPoint.Northing;

        // Project onto perpendicular direction
        double perpHeading = pathHeading + (Math.PI / 2.0);
        double xte = dx * Math.Sin(perpHeading) + dy * Math.Cos(perpHeading);

        return xte;
    }

    /// <summary>
    /// Calculate path heading at a given segment index.
    /// </summary>
    private double CalculatePathHeadingAtIndex(List<Position> points, int segmentIndex)
    {
        if (segmentIndex >= 0 && segmentIndex < points.Count - 1)
        {
            return CalculateHeadingBetweenPoints(points[segmentIndex], points[segmentIndex + 1]);
        }
        else if (segmentIndex == points.Count - 1 && points.Count >= 2)
        {
            return CalculateHeadingBetweenPoints(points[segmentIndex - 1], points[segmentIndex]);
        }
        else
        {
            return 0.0;
        }
    }

    /// <summary>
    /// Calculate heading error in radians [-π, π].
    /// </summary>
    private double CalculateHeadingError(double currentHeading, double pathHeading)
    {
        double error = currentHeading - pathHeading;

        // Normalize to [-π, π]
        while (error > Math.PI) error -= 2.0 * Math.PI;
        while (error < -Math.PI) error += 2.0 * Math.PI;

        return error;
    }

    /// <summary>
    /// Normalize heading to [0, 2π) radians.
    /// </summary>
    private double NormalizeHeadingRadians(double heading)
    {
        heading = heading % (2.0 * Math.PI);
        if (heading < 0) heading += 2.0 * Math.PI;
        return heading;
    }

    /// <summary>
    /// Calculate distance between two positions.
    /// </summary>
    private double CalculateDistance(Position p1, Position p2)
    {
        double dx = p2.Easting - p1.Easting;
        double dy = p2.Northing - p1.Northing;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Check if position contains valid values.
    /// </summary>
    private bool IsValidPosition(Position position)
    {
        return !double.IsNaN(position.Easting) &&
               !double.IsInfinity(position.Easting) &&
               !double.IsNaN(position.Northing) &&
               !double.IsInfinity(position.Northing);
    }

    #endregion
}
