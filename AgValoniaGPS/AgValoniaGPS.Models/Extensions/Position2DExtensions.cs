using System;

namespace AgValoniaGPS.Models.Extensions;

/// <summary>
/// Extension methods for Position2D to support vector operations.
/// Used extensively in geometric calculations like Dubins paths.
/// </summary>
public static class Position2DExtensions
{
    /// <summary>
    /// Subtracts two positions to get a direction vector.
    /// </summary>
    public static Position2D Subtract(this Position2D a, Position2D b)
    {
        return new Position2D(a.Easting - b.Easting, a.Northing - b.Northing);
    }

    /// <summary>
    /// Adds two positions/vectors.
    /// </summary>
    public static Position2D Add(this Position2D a, Position2D b)
    {
        return new Position2D(a.Easting + b.Easting, a.Northing + b.Northing);
    }

    /// <summary>
    /// Multiplies a position/vector by a scalar.
    /// </summary>
    public static Position2D Multiply(this Position2D pos, double scalar)
    {
        return new Position2D(pos.Easting * scalar, pos.Northing * scalar);
    }

    /// <summary>
    /// Calculates the length (magnitude) of the vector.
    /// </summary>
    public static double GetLength(this Position2D pos)
    {
        return Math.Sqrt(pos.Easting * pos.Easting + pos.Northing * pos.Northing);
    }

    /// <summary>
    /// Calculates the squared length of the vector (faster, use for comparisons).
    /// </summary>
    public static double GetLengthSquared(this Position2D pos)
    {
        return pos.Easting * pos.Easting + pos.Northing * pos.Northing;
    }

    /// <summary>
    /// Returns a normalized (unit length) version of this vector.
    /// </summary>
    public static Position2D Normalize(this Position2D pos)
    {
        double length = pos.GetLength();
        if (length < 1e-10)
        {
            return new Position2D(0, 0);
        }
        return new Position2D(pos.Easting / length, pos.Northing / length);
    }
}
