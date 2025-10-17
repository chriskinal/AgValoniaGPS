using System;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents a 2D position in UTM/local plane coordinates (easting, northing).
/// Immutable struct for thread-safe position representation.
/// </summary>
public readonly struct Position2D : IEquatable<Position2D>
{
    /// <summary>
    /// Creates a new 2D position.
    /// </summary>
    /// <param name="easting">Easting coordinate in meters</param>
    /// <param name="northing">Northing coordinate in meters</param>
    public Position2D(double easting, double northing)
    {
        Easting = easting;
        Northing = northing;
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
    /// Calculates squared distance to another position (faster than Distance, use for comparisons).
    /// </summary>
    public double DistanceSquared(Position2D other)
    {
        double de = Easting - other.Easting;
        double dn = Northing - other.Northing;
        return de * de + dn * dn;
    }

    /// <summary>
    /// Calculates distance to another position in meters.
    /// </summary>
    public double Distance(Position2D other)
    {
        return Math.Sqrt(DistanceSquared(other));
    }

    public bool Equals(Position2D other)
    {
        return Easting.Equals(other.Easting) && Northing.Equals(other.Northing);
    }

    public override bool Equals(object? obj)
    {
        return obj is Position2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Easting, Northing);
    }

    public static bool operator ==(Position2D left, Position2D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Position2D left, Position2D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"E: {Easting:F3}, N: {Northing:F3}";
    }
}
