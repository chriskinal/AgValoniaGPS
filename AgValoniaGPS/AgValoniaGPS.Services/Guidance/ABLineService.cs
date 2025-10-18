using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for AB Line guidance operations
/// Implements core AB line functionality including creation, guidance calculations,
/// nudging, and parallel line generation with ±2cm accuracy
/// </summary>
public class ABLineService : IABLineService
{
    private const double MinimumLineLength = 0.5; // meters
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    public event EventHandler<ABLineChangedEventArgs>? ABLineChanged;

    /// <summary>
    /// Create AB line from two GPS positions
    /// </summary>
    public ABLine CreateFromPoints(Position pointA, Position pointB, string name)
    {
        // Validate points are not identical
        double distance = CalculateDistance(pointA, pointB);
        if (distance < MinimumLineLength)
        {
            throw new ArgumentException($"Points are too close together (distance: {distance:F2}m). Minimum distance is {MinimumLineLength}m.");
        }

        // Calculate heading between points
        double headingDegrees = CalculateHeading(pointA, pointB);

        var line = new ABLine
        {
            Name = name,
            PointA = pointA,
            PointB = pointB,
            Heading = headingDegrees,
            NudgeOffset = 0.0
        };

        // Emit event
        ABLineChanged?.Invoke(this, new ABLineChangedEventArgs(line, ABLineChangeType.Created));

        return line;
    }

    /// <summary>
    /// Create AB line from single point and heading angle
    /// </summary>
    public ABLine CreateFromHeading(Position origin, double headingDegrees, string name, double length = 1000.0)
    {
        // Validate heading
        if (double.IsNaN(headingDegrees) || double.IsInfinity(headingDegrees))
        {
            throw new ArgumentException("Heading must be a valid number");
        }

        // Normalize heading to 0-360
        headingDegrees = NormalizeHeading(headingDegrees);

        // Calculate endpoint from origin and heading
        double headingRadians = headingDegrees * DegreesToRadians;
        double deltaEasting = length * Math.Sin(headingRadians);
        double deltaNorthing = length * Math.Cos(headingRadians);

        var pointB = new Position
        {
            Easting = origin.Easting + deltaEasting,
            Northing = origin.Northing + deltaNorthing,
            Altitude = origin.Altitude,
            Latitude = origin.Latitude,
            Longitude = origin.Longitude,
            Zone = origin.Zone,
            Hemisphere = origin.Hemisphere
        };

        var line = new ABLine
        {
            Name = name,
            PointA = origin,
            PointB = pointB,
            Heading = headingDegrees,
            NudgeOffset = 0.0
        };

        // Emit event
        ABLineChanged?.Invoke(this, new ABLineChangedEventArgs(line, ABLineChangeType.Created));

        return line;
    }

    /// <summary>
    /// Calculate cross-track error and guidance information
    /// Optimized for <5ms execution time
    /// </summary>
    public GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, ABLine line)
    {
        // Find closest point on line
        var closestPoint = GetClosestPoint(currentPosition, line);

        // Calculate cross-track error (perpendicular distance to line)
        double xte = CalculateCrossTrackError(currentPosition, line);

        // Calculate heading error
        double headingError = CalculateHeadingError(currentHeading, line.Heading);

        // Calculate distance to line
        double distance = Math.Abs(xte);

        return new GuidanceLineResult
        {
            CrossTrackError = xte,
            ClosestPoint = closestPoint,
            HeadingError = headingError,
            DistanceToLine = distance
        };
    }

    /// <summary>
    /// Find closest point on AB line to current position
    /// </summary>
    public Position GetClosestPoint(Position currentPosition, ABLine line)
    {
        // Calculate line vector
        double lineEasting = line.PointB.Easting - line.PointA.Easting;
        double lineNorthing = line.PointB.Northing - line.PointA.Northing;
        double lineLength = Math.Sqrt(lineEasting * lineEasting + lineNorthing * lineNorthing);

        if (lineLength < 0.001)
        {
            // Degenerate line, return PointA
            return line.PointA;
        }

        // Normalize line vector
        double lineUnitE = lineEasting / lineLength;
        double lineUnitN = lineNorthing / lineLength;

        // Vector from PointA to current position
        double deltaE = currentPosition.Easting - line.PointA.Easting;
        double deltaN = currentPosition.Northing - line.PointA.Northing;

        // Project onto line (dot product)
        double projection = deltaE * lineUnitE + deltaN * lineUnitN;

        // Calculate closest point (allow projection beyond line segment for infinite line)
        double closestE = line.PointA.Easting + projection * lineUnitE;
        double closestN = line.PointA.Northing + projection * lineUnitN;

        return new Position
        {
            Easting = closestE,
            Northing = closestN,
            Altitude = currentPosition.Altitude,
            Latitude = currentPosition.Latitude,
            Longitude = currentPosition.Longitude,
            Zone = currentPosition.Zone,
            Hemisphere = currentPosition.Hemisphere
        };
    }

    /// <summary>
    /// Nudge AB line perpendicular to its heading
    /// Creates new line with offset while preserving heading
    /// </summary>
    public ABLine NudgeLine(ABLine line, double offsetMeters)
    {
        // Calculate perpendicular direction (90 degrees left of heading for positive offset)
        double headingRadians = line.Heading * DegreesToRadians;
        double perpHeadingRadians = headingRadians - (Math.PI / 2.0);

        // Calculate offset vector
        double offsetEasting = offsetMeters * Math.Sin(perpHeadingRadians);
        double offsetNorthing = offsetMeters * Math.Cos(perpHeadingRadians);

        // Create nudged points
        var nudgedPointA = new Position
        {
            Easting = line.PointA.Easting + offsetEasting,
            Northing = line.PointA.Northing + offsetNorthing,
            Altitude = line.PointA.Altitude,
            Latitude = line.PointA.Latitude,
            Longitude = line.PointA.Longitude,
            Zone = line.PointA.Zone,
            Hemisphere = line.PointA.Hemisphere
        };

        var nudgedPointB = new Position
        {
            Easting = line.PointB.Easting + offsetEasting,
            Northing = line.PointB.Northing + offsetNorthing,
            Altitude = line.PointB.Altitude,
            Latitude = line.PointB.Latitude,
            Longitude = line.PointB.Longitude,
            Zone = line.PointB.Zone,
            Hemisphere = line.PointB.Hemisphere
        };

        // Create new line with updated offset
        var nudgedLine = new ABLine
        {
            Name = line.Name,
            PointA = nudgedPointA,
            PointB = nudgedPointB,
            Heading = line.Heading, // Preserve heading
            NudgeOffset = line.NudgeOffset + offsetMeters // Accumulate offset
        };

        // Emit event
        ABLineChanged?.Invoke(this, new ABLineChangedEventArgs(nudgedLine, ABLineChangeType.Nudged));

        return nudgedLine;
    }

    /// <summary>
    /// Generate parallel AB lines at specified spacing
    /// Maintains exact spacing (±2cm accuracy) and completes in <50ms for 10 lines
    /// </summary>
    public List<ABLine> GenerateParallelLines(ABLine referenceLine, double spacing, int count, UnitSystem units)
    {
        // Convert spacing to meters (internal representation)
        double spacingMeters = UnitSystemExtensions.ToMeters(spacing, units);

        var parallelLines = new List<ABLine>(count * 2);

        // Generate lines on left side (negative offsets)
        for (int i = 1; i <= count; i++)
        {
            double offset = -i * spacingMeters;
            var leftLine = NudgeLine(referenceLine, offset);
            leftLine.Name = GenerateParallelLineName(referenceLine.Name, -i);
            parallelLines.Add(leftLine);
        }

        // Generate lines on right side (positive offsets)
        for (int i = 1; i <= count; i++)
        {
            double offset = i * spacingMeters;
            var rightLine = NudgeLine(referenceLine, offset);
            rightLine.Name = GenerateParallelLineName(referenceLine.Name, i);
            parallelLines.Add(rightLine);
        }

        return parallelLines;
    }

    /// <summary>
    /// Validate an AB line for correctness
    /// </summary>
    public ValidationResult ValidateLine(ABLine line)
    {
        var result = new ValidationResult { IsValid = true };

        if (line == null)
        {
            result.AddError("AB line cannot be null");
            return result;
        }

        // Validate minimum length
        double length = CalculateDistance(line.PointA, line.PointB);
        if (length < MinimumLineLength)
        {
            result.AddError($"AB line is too short ({length:F2}m). Minimum length is {MinimumLineLength}m.");
        }

        // Validate points are not NaN or Infinity
        if (!IsValidPosition(line.PointA))
        {
            result.AddError("Point A contains invalid values (NaN or Infinity)");
        }

        if (!IsValidPosition(line.PointB))
        {
            result.AddError("Point B contains invalid values (NaN or Infinity)");
        }

        // Validate heading
        if (double.IsNaN(line.Heading) || double.IsInfinity(line.Heading))
        {
            result.AddError("Heading contains invalid value (NaN or Infinity)");
        }

        return result;
    }

    #region Helper Methods

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
    /// Calculate cross-track error (perpendicular distance from position to line)
    /// Positive = right of line, Negative = left of line
    /// </summary>
    private double CalculateCrossTrackError(Position position, ABLine line)
    {
        // Calculate line vector
        double lineE = line.PointB.Easting - line.PointA.Easting;
        double lineN = line.PointB.Northing - line.PointA.Northing;
        double lineLength = Math.Sqrt(lineE * lineE + lineN * lineN);

        if (lineLength < 0.001)
        {
            return 0.0;
        }

        // Vector from PointA to current position
        double deltaE = position.Easting - line.PointA.Easting;
        double deltaN = position.Northing - line.PointA.Northing;

        // Cross product gives signed distance
        // Positive = right of line, Negative = left
        // Negate to match convention: right = positive, left = negative
        double crossProduct = (lineN * deltaE - lineE * deltaN) / lineLength;

        return crossProduct;
    }

    /// <summary>
    /// Calculate heading error (difference between vehicle and line heading)
    /// Returns value in range [-180, 180]
    /// </summary>
    private double CalculateHeadingError(double currentHeading, double lineHeading)
    {
        double error = currentHeading - lineHeading;

        // Normalize to [-180, 180]
        while (error > 180.0) error -= 360.0;
        while (error < -180.0) error += 360.0;

        return error;
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
    /// Check if position contains valid values
    /// </summary>
    private bool IsValidPosition(Position position)
    {
        return !double.IsNaN(position.Easting) &&
               !double.IsInfinity(position.Easting) &&
               !double.IsNaN(position.Northing) &&
               !double.IsInfinity(position.Northing);
    }

    /// <summary>
    /// Generate name for parallel line
    /// </summary>
    private string GenerateParallelLineName(string referenceName, int offset)
    {
        string sign = offset > 0 ? "+" : "";
        return $"{referenceName} {sign}{offset}";
    }

    #endregion
}
