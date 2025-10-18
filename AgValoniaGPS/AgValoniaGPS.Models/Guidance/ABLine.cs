namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Represents a straight AB guidance line defined by two points or a heading angle.
/// Used for precision agriculture guidance to create parallel passes across a field.
/// </summary>
public class ABLine
{
    /// <summary>
    /// Gets or sets the name of the AB line for identification.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first point (Point A) defining the AB line.
    /// </summary>
    public Position PointA { get; set; } = new();

    /// <summary>
    /// Gets or sets the second point (Point B) defining the AB line.
    /// </summary>
    public Position PointB { get; set; } = new();

    /// <summary>
    /// Gets or sets the heading angle of the AB line in radians.
    /// Range: 0 to 2π (0 to 360 degrees).
    /// </summary>
    public double Heading { get; set; }

    /// <summary>
    /// Gets or sets the perpendicular offset (nudge) applied to the line in meters.
    /// Positive values offset to the right, negative values to the left.
    /// </summary>
    public double NudgeOffset { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this AB line was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the calculated length of the AB line in meters.
    /// </summary>
    public double Length
    {
        get
        {
            double dx = PointB.Easting - PointA.Easting;
            double dy = PointB.Northing - PointA.Northing;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// Gets the midpoint position between Point A and Point B.
    /// </summary>
    public Position MidPoint
    {
        get
        {
            return new Position
            {
                Easting = (PointA.Easting + PointB.Easting) / 2.0,
                Northing = (PointA.Northing + PointB.Northing) / 2.0,
                Altitude = (PointA.Altitude + PointB.Altitude) / 2.0,
                Latitude = (PointA.Latitude + PointB.Latitude) / 2.0,
                Longitude = (PointA.Longitude + PointB.Longitude) / 2.0,
                Zone = PointA.Zone,
                Hemisphere = PointA.Hemisphere
            };
        }
    }

    /// <summary>
    /// Gets the unit vector representing the direction of the AB line.
    /// Returns (cos(Heading), sin(Heading)).
    /// </summary>
    public (double X, double Y) UnitVector
    {
        get
        {
            return (Math.Cos(Heading), Math.Sin(Heading));
        }
    }

    /// <summary>
    /// Validates the AB line for minimum requirements.
    /// </summary>
    /// <returns>True if the line is valid, false otherwise.</returns>
    public bool Validate()
    {
        // Check minimum length (must be > 0.5m)
        if (Length < 0.5)
            return false;

        // Check for valid points (no NaN or infinity)
        if (double.IsNaN(PointA.Easting) || double.IsNaN(PointA.Northing) ||
            double.IsNaN(PointB.Easting) || double.IsNaN(PointB.Northing) ||
            double.IsInfinity(PointA.Easting) || double.IsInfinity(PointA.Northing) ||
            double.IsInfinity(PointB.Easting) || double.IsInfinity(PointB.Northing))
            return false;

        // Check for valid heading (0 to 2π)
        if (double.IsNaN(Heading) || double.IsInfinity(Heading) || Heading < 0 || Heading > 2 * Math.PI)
            return false;

        return true;
    }
}
