using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Represents a headline guidance path with moveable guidance lines.
/// Headlines are an alternative to AB lines for specific workflows, allowing
/// a path-based guidance system with adjustable position offsets.
/// </summary>
public class Headline
{
    /// <summary>
    /// Gets or sets the unique identifier for this headline.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the headline for identification.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of points defining the headline path.
    /// Each point represents a position along the guidance path.
    /// </summary>
    public List<Position> TrackPoints { get; set; } = new();

    /// <summary>
    /// Gets or sets the move distance offset in meters.
    /// Used to shift the headline laterally while maintaining path shape.
    /// </summary>
    public double MoveDistance { get; set; }

    /// <summary>
    /// Gets or sets the mode/type of the headline.
    /// 0 = Standard mode
    /// </summary>
    public int Mode { get; set; }

    /// <summary>
    /// Gets or sets the index of the A-point in the track points list.
    /// Used to identify the starting reference point for guidance.
    /// </summary>
    public int APointIndex { get; set; }

    /// <summary>
    /// Gets or sets the heading angle in radians for this headline.
    /// Represents the overall direction of the path.
    /// </summary>
    public double Heading { get; set; }

    /// <summary>
    /// Gets or sets whether this headline is currently active for guidance.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this headline was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the calculated length of the headline path in meters.
    /// </summary>
    public double Length
    {
        get
        {
            if (TrackPoints == null || TrackPoints.Count < 2)
                return 0.0;

            double totalLength = 0.0;
            for (int i = 0; i < TrackPoints.Count - 1; i++)
            {
                double dx = TrackPoints[i + 1].Easting - TrackPoints[i].Easting;
                double dy = TrackPoints[i + 1].Northing - TrackPoints[i].Northing;
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }
            return totalLength;
        }
    }

    /// <summary>
    /// Validates the headline for minimum requirements.
    /// </summary>
    /// <returns>True if the headline is valid, false otherwise.</returns>
    public bool Validate()
    {
        // Must have a name
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        // Must have at least 4 points (minimum for path-based guidance)
        if (TrackPoints == null || TrackPoints.Count < 4)
            return false;

        // A-point index must be within bounds
        if (APointIndex < 0 || APointIndex >= TrackPoints.Count)
            return false;

        // Check for valid points (no NaN or infinity)
        foreach (var point in TrackPoints)
        {
            if (double.IsNaN(point.Easting) || double.IsNaN(point.Northing) ||
                double.IsInfinity(point.Easting) || double.IsInfinity(point.Northing))
                return false;
        }

        // Check for valid heading (0 to 2π)
        if (double.IsNaN(Heading) || double.IsInfinity(Heading) || Heading < 0 || Heading > 2 * Math.PI)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the A-point position if index is valid.
    /// </summary>
    public Position? GetAPoint()
    {
        if (APointIndex >= 0 && APointIndex < TrackPoints.Count)
            return TrackPoints[APointIndex];
        return null;
    }

    /// <summary>
    /// Gets the average heading of the path by sampling key segments.
    /// </summary>
    public double CalculateAverageHeading()
    {
        if (TrackPoints == null || TrackPoints.Count < 2)
            return 0.0;

        double sumX = 0.0;
        double sumY = 0.0;

        for (int i = 0; i < TrackPoints.Count - 1; i++)
        {
            double dx = TrackPoints[i + 1].Easting - TrackPoints[i].Easting;
            double dy = TrackPoints[i + 1].Northing - TrackPoints[i].Northing;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length > 0.001)
            {
                sumX += dx / length;
                sumY += dy / length;
            }
        }

        return Math.Atan2(sumX, sumY);
    }
}
