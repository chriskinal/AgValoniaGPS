namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Represents a curved guidance line defined by a sequence of GPS positions.
/// Used for following irregular field shapes and terrain contours.
/// </summary>
public class CurveLine
{
    /// <summary>
    /// Gets or sets the name of the curve line for identification.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of positions defining the curve path.
    /// Minimum of 3 points required for a valid curve.
    /// </summary>
    public List<Position> Points { get; set; } = new();

    /// <summary>
    /// Gets or sets the smoothing parameters used to process this curve.
    /// </summary>
    public SmoothingParameters? SmoothingParameters { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this curve line was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the total length of the curve in meters.
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
    /// Gets the number of points in the curve.
    /// </summary>
    public int PointCount => Points.Count;

    /// <summary>
    /// Gets the average spacing between points in meters.
    /// Returns 0 if there are fewer than 2 points.
    /// </summary>
    public double AverageSpacing
    {
        get
        {
            if (Points.Count < 2)
                return 0.0;

            return TotalLength / (Points.Count - 1);
        }
    }

    /// <summary>
    /// Validates the curve line for minimum requirements.
    /// </summary>
    /// <returns>ValidationResult indicating validity and any issues.</returns>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        // Check minimum points (>=3)
        if (Points.Count < 3)
        {
            result.AddError($"Curve must have at least 3 points. Current count: {Points.Count}");
            return result;
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

        // Check point spacing
        double minSpacing = double.MaxValue;
        double maxSpacing = 0.0;

        for (int i = 1; i < Points.Count; i++)
        {
            double dx = Points[i].Easting - Points[i - 1].Easting;
            double dy = Points[i].Northing - Points[i - 1].Northing;
            double spacing = Math.Sqrt(dx * dx + dy * dy);

            if (spacing < minSpacing)
                minSpacing = spacing;
            if (spacing > maxSpacing)
                maxSpacing = spacing;

            // Warn if points are too close
            if (spacing < 0.1)
            {
                result.AddWarning($"Points {i - 1} and {i} are very close together ({spacing:F3}m)");
            }

            // Check for duplicate consecutive points
            if (spacing < 0.01)
            {
                result.AddError($"Points {i - 1} and {i} are duplicate or nearly identical");
            }
        }

        // Warn if point spacing is too far
        if (maxSpacing > 100.0)
        {
            result.AddWarning($"Maximum point spacing is very large ({maxSpacing:F1}m)");
        }

        return result;
    }
}
