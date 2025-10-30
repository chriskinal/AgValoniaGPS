using System;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a grid cell in an elevation grid
/// Used for indexing elevation data by grid coordinates
/// </summary>
public readonly struct GridCell : IEquatable<GridCell>
{
    /// <summary>
    /// X index in the grid
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// Y index in the grid
    /// </summary>
    public int Y { get; init; }

    public GridCell(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(GridCell other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is GridCell other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(GridCell left, GridCell right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridCell left, GridCell right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"GridCell({X}, {Y})";
    }
}
