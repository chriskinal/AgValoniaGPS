using System;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a waypoint along a Dubins path with position and heading.
/// </summary>
public readonly struct DubinsPathWaypoint : IEquatable<DubinsPathWaypoint>
{
    /// <summary>
    /// Creates a new Dubins path waypoint.
    /// </summary>
    /// <param name="easting">Easting coordinate in meters</param>
    /// <param name="northing">Northing coordinate in meters</param>
    /// <param name="heading">Heading in radians</param>
    public DubinsPathWaypoint(double easting, double northing, double heading)
    {
        Easting = easting;
        Northing = northing;
        Heading = heading;
    }

    /// <summary>
    /// Easting coordinate in meters.
    /// </summary>
    public double Easting { get; }

    /// <summary>
    /// Northing coordinate in meters.
    /// </summary>
    public double Northing { get; }

    /// <summary>
    /// Heading in radians (0 = North, clockwise positive).
    /// </summary>
    public double Heading { get; }

    public bool Equals(DubinsPathWaypoint other)
    {
        return Easting.Equals(other.Easting) &&
               Northing.Equals(other.Northing) &&
               Heading.Equals(other.Heading);
    }

    public override bool Equals(object? obj)
    {
        return obj is DubinsPathWaypoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Easting, Northing, Heading);
    }

    public static bool operator ==(DubinsPathWaypoint left, DubinsPathWaypoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DubinsPathWaypoint left, DubinsPathWaypoint right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"E: {Easting:F3}, N: {Northing:F3}, Hdg: {Heading:F3} rad";
    }
}
