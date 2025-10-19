using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for managing tram lines (guidance paths for vehicle wheels)
/// Provides tram line generation, proximity detection, and pattern management
/// Performance: <5ms generation, <2ms proximity detection
/// </summary>
public class TramLineService : ITramLineService
{
    private const double MinimumSpacing = 0.5; // meters
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    private readonly object _lock = new object();
    private Position[][]? _tramLines;
    private double _spacing = 3.0; // Default spacing in meters
    private Position? _baseLineStart;
    private Position? _baseLineEnd;

    public event EventHandler<TramLineProximityEventArgs>? TramLineProximity;

    /// <summary>
    /// Generate tram lines from a base line with specified spacing
    /// Creates parallel lines on both sides of the base line
    /// </summary>
    public void GenerateTramLines(Position lineStart, Position lineEnd, double spacing, int count)
    {
        if (lineStart == null)
            throw new ArgumentNullException(nameof(lineStart));
        if (lineEnd == null)
            throw new ArgumentNullException(nameof(lineEnd));
        if (spacing < MinimumSpacing)
            throw new ArgumentException($"Spacing must be at least {MinimumSpacing} meters");
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative");

        lock (_lock)
        {
            _baseLineStart = lineStart;
            _baseLineEnd = lineEnd;
            _spacing = spacing;

            // Calculate base line heading and length
            double heading = CalculateHeading(lineStart, lineEnd);
            double lineLength = CalculateDistance(lineStart, lineEnd);

            // Generate parallel lines
            var lines = new List<Position[]>();

            // Add center line (base line)
            lines.Add(new[] { lineStart, lineEnd });

            // Generate lines on both sides
            for (int i = 1; i <= count; i++)
            {
                // Left side (negative offset)
                double leftOffset = -i * spacing;
                var leftLine = GenerateParallelLine(lineStart, lineEnd, heading, leftOffset, lineLength);
                lines.Add(leftLine);

                // Right side (positive offset)
                double rightOffset = i * spacing;
                var rightLine = GenerateParallelLine(lineStart, lineEnd, heading, rightOffset, lineLength);
                lines.Add(rightLine);
            }

            _tramLines = lines.ToArray();
        }
    }

    /// <summary>
    /// Generate tram lines from an AB line by ID
    /// Note: This is a simplified implementation. In a full integration,
    /// this would use IABLineService to get the AB line by ID
    /// </summary>
    public void GenerateFromABLine(int abLineId, double spacing)
    {
        // This is a placeholder for AB line integration
        // In a full implementation, this would:
        // 1. Inject IABLineService
        // 2. Get AB line by ID
        // 3. Generate tram lines from AB line geometry
        throw new NotImplementedException("AB line integration not yet implemented in this basic version");
    }

    /// <summary>
    /// Load tram lines from a Position array
    /// </summary>
    public void LoadTramLines(Position[][] tramLines)
    {
        if (tramLines == null)
            throw new ArgumentNullException(nameof(tramLines));

        lock (_lock)
        {
            _tramLines = tramLines;
        }
    }

    /// <summary>
    /// Clear all tram lines
    /// </summary>
    public void ClearTramLines()
    {
        lock (_lock)
        {
            _tramLines = null;
            _baseLineStart = null;
            _baseLineEnd = null;
        }
    }

    /// <summary>
    /// Get all tram lines
    /// </summary>
    public Position[][]? GetTramLines()
    {
        lock (_lock)
        {
            return _tramLines;
        }
    }

    /// <summary>
    /// Get the number of tram lines
    /// </summary>
    public int GetTramLineCount()
    {
        lock (_lock)
        {
            return _tramLines?.Length ?? 0;
        }
    }

    /// <summary>
    /// Get a specific tram line by ID
    /// </summary>
    public Position[]? GetTramLine(int tramLineId)
    {
        lock (_lock)
        {
            if (_tramLines == null || tramLineId < 0 || tramLineId >= _tramLines.Length)
                return null;

            return _tramLines[tramLineId];
        }
    }

    /// <summary>
    /// Get the distance to the nearest tram line
    /// Performance: <2ms per call
    /// </summary>
    public double GetDistanceToNearestTramLine(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            if (_tramLines == null || _tramLines.Length == 0)
                return double.MaxValue;

            double minDistance = double.MaxValue;

            foreach (var tramLine in _tramLines)
            {
                if (tramLine == null || tramLine.Length < 2)
                    continue;

                // Calculate perpendicular distance to this tram line
                double distance = CalculatePerpendicularDistance(position, tramLine[0], tramLine[1]);
                minDistance = Math.Min(minDistance, Math.Abs(distance));
            }

            return minDistance;
        }
    }

    /// <summary>
    /// Get the ID of the nearest tram line
    /// </summary>
    public int GetNearestTramLineId(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            if (_tramLines == null || _tramLines.Length == 0)
                return -1;

            double minDistance = double.MaxValue;
            int nearestId = -1;

            for (int i = 0; i < _tramLines.Length; i++)
            {
                var tramLine = _tramLines[i];
                if (tramLine == null || tramLine.Length < 2)
                    continue;

                // Calculate perpendicular distance to this tram line
                double distance = Math.Abs(CalculatePerpendicularDistance(position, tramLine[0], tramLine[1]));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestId = i;
                }
            }

            return nearestId;
        }
    }

    /// <summary>
    /// Check proximity to tram lines and raise event if within threshold
    /// </summary>
    public void CheckProximity(Position position, double threshold)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));
        if (threshold <= 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be positive");

        int nearestId = GetNearestTramLineId(position);
        if (nearestId == -1)
            return;

        double distance = GetDistanceToNearestTramLine(position);

        if (distance <= threshold)
        {
            // Calculate nearest point on the tram line
            var tramLine = GetTramLine(nearestId);
            if (tramLine != null && tramLine.Length >= 2)
            {
                var nearestPoint = GetClosestPointOnLine(position, tramLine[0], tramLine[1]);

                TramLineProximity?.Invoke(this, new TramLineProximityEventArgs(
                    nearestId,
                    distance,
                    nearestPoint
                ));
            }
        }
    }

    /// <summary>
    /// Set the spacing between tram lines
    /// </summary>
    public void SetSpacing(double spacing)
    {
        if (spacing < MinimumSpacing)
            throw new ArgumentException($"Spacing must be at least {MinimumSpacing} meters");

        lock (_lock)
        {
            _spacing = spacing;
        }
    }

    /// <summary>
    /// Get the current spacing between tram lines
    /// </summary>
    public double GetSpacing()
    {
        lock (_lock)
        {
            return _spacing;
        }
    }

    #region Helper Methods

    /// <summary>
    /// Generate a parallel line at specified offset from the base line
    /// </summary>
    private Position[] GenerateParallelLine(Position lineStart, Position lineEnd, double heading, double offset, double length)
    {
        // Calculate perpendicular direction (90 degrees right of heading for positive offset)
        double headingRadians = heading * DegreesToRadians;
        double perpHeadingRadians = headingRadians + (Math.PI / 2.0);

        // Calculate offset vector
        double offsetEasting = offset * Math.Sin(perpHeadingRadians);
        double offsetNorthing = offset * Math.Cos(perpHeadingRadians);

        // Create parallel line points
        var parallelStart = new Position
        {
            Easting = lineStart.Easting + offsetEasting,
            Northing = lineStart.Northing + offsetNorthing,
            Altitude = lineStart.Altitude,
            Latitude = lineStart.Latitude,
            Longitude = lineStart.Longitude,
            Zone = lineStart.Zone,
            Hemisphere = lineStart.Hemisphere
        };

        var parallelEnd = new Position
        {
            Easting = lineEnd.Easting + offsetEasting,
            Northing = lineEnd.Northing + offsetNorthing,
            Altitude = lineEnd.Altitude,
            Latitude = lineEnd.Latitude,
            Longitude = lineEnd.Longitude,
            Zone = lineEnd.Zone,
            Hemisphere = lineEnd.Hemisphere
        };

        return new[] { parallelStart, parallelEnd };
    }

    /// <summary>
    /// Calculate heading from point A to point B in degrees
    /// </summary>
    private double CalculateHeading(Position pointA, Position pointB)
    {
        double deltaE = pointB.Easting - pointA.Easting;
        double deltaN = pointB.Northing - pointA.Northing;
        double headingRadians = Math.Atan2(deltaE, deltaN);
        double headingDegrees = headingRadians * RadiansToDegrees;
        return NormalizeHeading(headingDegrees);
    }

    /// <summary>
    /// Normalize heading to 0-360 degrees
    /// </summary>
    private double NormalizeHeading(double heading)
    {
        heading = heading % 360.0;
        if (heading < 0) heading += 360.0;
        return heading;
    }

    /// <summary>
    /// Calculate distance between two positions
    /// </summary>
    private double CalculateDistance(Position p1, Position p2)
    {
        double deltaE = p2.Easting - p1.Easting;
        double deltaN = p2.Northing - p1.Northing;
        return Math.Sqrt(deltaE * deltaE + deltaN * deltaN);
    }

    /// <summary>
    /// Calculate perpendicular distance from position to line
    /// Positive = right of line, Negative = left of line
    /// </summary>
    private double CalculatePerpendicularDistance(Position position, Position lineStart, Position lineEnd)
    {
        // Calculate line vector
        double lineE = lineEnd.Easting - lineStart.Easting;
        double lineN = lineEnd.Northing - lineStart.Northing;
        double lineLength = Math.Sqrt(lineE * lineE + lineN * lineN);

        if (lineLength < 0.001)
            return 0.0;

        // Vector from lineStart to current position
        double deltaE = position.Easting - lineStart.Easting;
        double deltaN = position.Northing - lineStart.Northing;

        // Cross product gives signed distance
        double crossProduct = (lineN * deltaE - lineE * deltaN) / lineLength;

        return crossProduct;
    }

    /// <summary>
    /// Get closest point on line to the given position
    /// </summary>
    private Position GetClosestPointOnLine(Position position, Position lineStart, Position lineEnd)
    {
        // Calculate line vector
        double lineEasting = lineEnd.Easting - lineStart.Easting;
        double lineNorthing = lineEnd.Northing - lineStart.Northing;
        double lineLength = Math.Sqrt(lineEasting * lineEasting + lineNorthing * lineNorthing);

        if (lineLength < 0.001)
            return lineStart;

        // Normalize line vector
        double lineUnitE = lineEasting / lineLength;
        double lineUnitN = lineNorthing / lineLength;

        // Vector from lineStart to current position
        double deltaE = position.Easting - lineStart.Easting;
        double deltaN = position.Northing - lineStart.Northing;

        // Project onto line (dot product)
        double projection = deltaE * lineUnitE + deltaN * lineUnitN;

        // Calculate closest point (allow projection beyond line segment for infinite line)
        double closestE = lineStart.Easting + projection * lineUnitE;
        double closestN = lineStart.Northing + projection * lineUnitN;

        return new Position
        {
            Easting = closestE,
            Northing = closestN,
            Altitude = position.Altitude,
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            Zone = position.Zone,
            Hemisphere = position.Hemisphere
        };
    }

    #endregion
}
