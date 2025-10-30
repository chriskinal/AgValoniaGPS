using System;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a single point in a recorded path with position, heading, speed, and section state.
/// </summary>
public readonly struct RecordedPathPoint : IEquatable<RecordedPathPoint>
{
    /// <summary>
    /// Creates a new recorded path point.
    /// </summary>
    /// <param name="easting">Easting coordinate in meters</param>
    /// <param name="northing">Northing coordinate in meters</param>
    /// <param name="heading">Heading in radians (0 = North, clockwise positive)</param>
    /// <param name="speed">Speed in meters per second</param>
    /// <param name="isSectionAutoOn">Whether section control was in auto mode at this point</param>
    public RecordedPathPoint(double easting, double northing, double heading, double speed, bool isSectionAutoOn)
    {
        Easting = easting;
        Northing = northing;
        Heading = heading;
        Speed = speed;
        IsSectionAutoOn = isSectionAutoOn;
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

    /// <summary>
    /// Speed in meters per second at this point.
    /// </summary>
    public double Speed { get; }

    /// <summary>
    /// Whether section control was in auto mode when this point was recorded.
    /// </summary>
    public bool IsSectionAutoOn { get; }

    public bool Equals(RecordedPathPoint other)
    {
        return Easting.Equals(other.Easting) &&
               Northing.Equals(other.Northing) &&
               Heading.Equals(other.Heading) &&
               Speed.Equals(other.Speed) &&
               IsSectionAutoOn == other.IsSectionAutoOn;
    }

    public override bool Equals(object? obj)
    {
        return obj is RecordedPathPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Easting, Northing, Heading, Speed, IsSectionAutoOn);
    }

    public static bool operator ==(RecordedPathPoint left, RecordedPathPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RecordedPathPoint left, RecordedPathPoint right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"E: {Easting:F3}, N: {Northing:F3}, Hdg: {Heading:F3} rad, Spd: {Speed:F2} m/s, Auto: {IsSectionAutoOn}";
    }
}
