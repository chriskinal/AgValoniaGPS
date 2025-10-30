using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a complete recorded path with metadata.
/// </summary>
public class RecordedPath
{
    /// <summary>
    /// Creates a new recorded path.
    /// </summary>
    /// <param name="name">Name of the recorded path</param>
    public RecordedPath(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Points = new List<RecordedPathPoint>();
        RecordedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Name of this recorded path.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// List of points in the recorded path.
    /// </summary>
    public List<RecordedPathPoint> Points { get; set; }

    /// <summary>
    /// When this path was recorded (UTC).
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Total length of the path in meters.
    /// </summary>
    public double TotalLength
    {
        get
        {
            double length = 0;
            for (int i = 0; i < Points.Count - 1; i++)
            {
                double de = Points[i + 1].Easting - Points[i].Easting;
                double dn = Points[i + 1].Northing - Points[i].Northing;
                length += Math.Sqrt(de * de + dn * dn);
            }
            return length;
        }
    }

    /// <summary>
    /// Number of points in the path.
    /// </summary>
    public int PointCount => Points.Count;

    /// <summary>
    /// Whether this path has any points.
    /// </summary>
    public bool IsEmpty => Points.Count == 0;

    /// <summary>
    /// Clears all points from the path.
    /// </summary>
    public void Clear()
    {
        Points.Clear();
    }

    /// <summary>
    /// Adds a point to the path.
    /// </summary>
    public void AddPoint(RecordedPathPoint point)
    {
        Points.Add(point);
    }

    /// <summary>
    /// Adds a point to the path with explicit parameters.
    /// </summary>
    public void AddPoint(double easting, double northing, double heading, double speed, bool isSectionAutoOn)
    {
        Points.Add(new RecordedPathPoint(easting, northing, heading, speed, isSectionAutoOn));
    }
}
