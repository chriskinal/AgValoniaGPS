namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Represents a contour guidance line recorded in real-time during field operations.
/// Used for maintaining consistent offset distances while recording or following a path.
/// </summary>
public class ContourLine
{
    /// <summary>
    /// Gets or sets the name of the contour line for identification.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of positions defining the contour path.
    /// Minimum of 10 points required for reliable following.
    /// </summary>
    public List<Position> Points { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the contour is locked and finalized for guidance use.
    /// When locked, no more points can be added to the contour.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets the minimum distance threshold in meters for adding new points.
    /// Points closer than this distance to the last recorded point will be skipped.
    /// </summary>
    public double MinDistanceThreshold { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the date and time when this contour line was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the total length of the contour in meters.
    /// </summary>
    public double TotalLength
    {
        get
        {
            if (Points.Count < 2)
                return 0.0;

            double totalLength = 0.0;
            for (int i = 1; i < Points.Count; i++)
            {
                double dx = Points[i].Easting - Points[i - 1].Easting;
                double dy = Points[i].Northing - Points[i - 1].Northing;
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }
            return totalLength;
        }
    }

    /// <summary>
    /// Gets the number of points in the contour.
    /// </summary>
    public int PointCount => Points.Count;

    /// <summary>
    /// Gets whether the contour is currently being recorded.
    /// A contour is recording if it has points but is not yet locked.
    /// </summary>
    public bool IsRecording => Points.Count > 0 && !IsLocked;

    /// <summary>
    /// Validates the contour line for minimum requirements.
    /// </summary>
    /// <returns>ValidationResult indicating validity and any issues.</returns>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        // Check minimum points for locking (>=10)
        if (IsLocked && Points.Count < 10)
        {
            result.AddError($"Locked contour must have at least 10 points for reliable following. Current count: {Points.Count}");
            return result;
        }

        // Check minimum total length (>10m) for locked contours
        if (IsLocked && TotalLength < 10.0)
        {
            result.AddError($"Locked contour must be at least 10m long. Current length: {TotalLength:F1}m");
        }

        // Check for NaN or infinity in points
        for (int i = 0; i < Points.Count; i++)
        {
            var point = Points[i];
            if (double.IsNaN(point.Easting) || double.IsNaN(point.Northing) ||
                double.IsInfinity(point.Easting) || double.IsInfinity(point.Northing))
            {
                result.AddError($"Point {i} contains invalid values (NaN or Infinity)");
            }
        }

        // Check for duplicate consecutive points
        for (int i = 1; i < Points.Count; i++)
        {
            double dx = Points[i].Easting - Points[i - 1].Easting;
            double dy = Points[i].Northing - Points[i - 1].Northing;
            double spacing = Math.Sqrt(dx * dx + dy * dy);

            if (spacing < 0.01)
            {
                result.AddError($"Points {i - 1} and {i} are duplicate or nearly identical");
            }
        }

        // Check point spacing consistency
        if (Points.Count >= 2)
        {
            List<double> spacings = new();
            for (int i = 1; i < Points.Count; i++)
            {
                double dx = Points[i].Easting - Points[i - 1].Easting;
                double dy = Points[i].Northing - Points[i - 1].Northing;
                spacings.Add(Math.Sqrt(dx * dx + dy * dy));
            }

            double avgSpacing = spacings.Average();
            double maxDeviation = spacings.Max(s => Math.Abs(s - avgSpacing));

            if (maxDeviation > avgSpacing * 3)
            {
                result.AddWarning($"Point spacing is inconsistent (max deviation: {maxDeviation:F2}m from average {avgSpacing:F2}m)");
            }
        }

        // Check for self-intersection (basic check)
        if (IsLocked && CheckForSelfIntersection())
        {
            result.AddWarning("Contour may contain self-intersecting segments");
        }

        return result;
    }

    /// <summary>
    /// Performs a basic check for self-intersecting contour segments.
    /// </summary>
    /// <returns>True if potential self-intersection detected, false otherwise.</returns>
    private bool CheckForSelfIntersection()
    {
        if (Points.Count < 4)
            return false;

        // Simple check: if any point is very close to a non-adjacent segment, flag it
        for (int i = 0; i < Points.Count - 1; i++)
        {
            for (int j = i + 2; j < Points.Count - 1; j++)
            {
                // Skip adjacent segments
                if (Math.Abs(i - j) <= 1)
                    continue;

                // Check if segments are suspiciously close
                double minDist = MinDistanceToSegment(Points[i], Points[j], Points[j + 1]);
                if (minDist < 0.5) // 0.5m threshold for potential intersection
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculates minimum distance from a point to a line segment.
    /// </summary>
    private double MinDistanceToSegment(Position point, Position segStart, Position segEnd)
    {
        double dx = segEnd.Easting - segStart.Easting;
        double dy = segEnd.Northing - segStart.Northing;
        double lengthSq = dx * dx + dy * dy;

        if (lengthSq < 0.0001) // Segment is essentially a point
            return Math.Sqrt(
                Math.Pow(point.Easting - segStart.Easting, 2) +
                Math.Pow(point.Northing - segStart.Northing, 2));

        double t = ((point.Easting - segStart.Easting) * dx +
                    (point.Northing - segStart.Northing) * dy) / lengthSq;
        t = Math.Max(0, Math.Min(1, t));

        double projX = segStart.Easting + t * dx;
        double projY = segStart.Northing + t * dy;

        return Math.Sqrt(
            Math.Pow(point.Easting - projX, 2) +
            Math.Pow(point.Northing - projY, 2));
    }
}
