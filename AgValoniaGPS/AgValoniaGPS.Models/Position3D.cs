using System;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents a 3D position with heading in UTM/local plane coordinates.
/// Immutable struct for thread-safe position representation with orientation.
/// </summary>
public readonly struct Position3D : IEquatable<Position3D>
{
    /// <summary>
    /// Creates a new 3D position with heading.
    /// </summary>
    /// <param name="easting">Easting coordinate in meters</param>
    /// <param name="northing">Northing coordinate in meters</param>
    /// <param name="heading">Heading in radians (0 = North, increases clockwise)</param>
    public Position3D(double easting, double northing, double heading)
    {
        Easting = easting;
        Northing = northing;
        Heading = heading;
    }

    /// <summary>
    /// Easting coordinate in meters (X-axis, increases eastward).
    /// </summary>
    public double Easting { get; }

    /// <summary>
    /// Northing coordinate in meters (Y-axis, increases northward).
    /// </summary>
    public double Northing { get; }

    /// <summary>
    /// Heading in radians. 0 = North, PI/2 = East, PI = South, 3*PI/2 = West.
    /// Range: [0, 2*PI)
    /// </summary>
    public double Heading { get; }

    /// <summary>
    /// Gets the 2D position component (easting, northing).
    /// </summary>
    public Position2D Position2D => new Position2D(Easting, Northing);

    /// <summary>
    /// Calculates squared distance to another position (faster than Distance, use for comparisons).
    /// </summary>
    public double DistanceSquared(Position3D other)
    {
        double de = Easting - other.Easting;
        double dn = Northing - other.Northing;
        return de * de + dn * dn;
    }

    /// <summary>
    /// Calculates distance to another position in meters (ignores heading).
    /// </summary>
    public double Distance(Position3D other)
    {
        return Math.Sqrt(DistanceSquared(other));
    }

    /// <summary>
    /// Calculates distance to a 2D position in meters.
    /// </summary>
    public double Distance(Position2D other)
    {
        double de = Easting - other.Easting;
        double dn = Northing - other.Northing;
        return Math.Sqrt(de * de + dn * dn);
    }

    /// <summary>
    /// Creates a new Position3D from a 2D position and heading.
    /// </summary>
    public static Position3D FromPosition2D(Position2D position, double heading)
    {
        return new Position3D(position.Easting, position.Northing, heading);
    }

    public bool Equals(Position3D other)
    {
        return Easting.Equals(other.Easting) &&
               Northing.Equals(other.Northing) &&
               Heading.Equals(other.Heading);
    }

    public override bool Equals(object? obj)
    {
        return obj is Position3D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Easting, Northing, Heading);
    }

    public static bool operator ==(Position3D left, Position3D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Position3D left, Position3D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"E: {Easting:F3}, N: {Northing:F3}, H: {Heading:F4} rad";
    }
}
