using System;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a single elevation measurement at a specific position
/// </summary>
public readonly struct ElevationPoint
{
    /// <summary>
    /// Position in local coordinates (meters)
    /// </summary>
    public Position2D Position { get; init; }

    /// <summary>
    /// Elevation in meters
    /// </summary>
    public double Elevation { get; init; }

    /// <summary>
    /// Timestamp when this elevation was recorded
    /// </summary>
    public DateTime Timestamp { get; init; }

    public ElevationPoint(Position2D position, double elevation, DateTime timestamp)
    {
        Position = position;
        Elevation = elevation;
        Timestamp = timestamp;
    }

    public ElevationPoint(Position2D position, double elevation)
        : this(position, elevation, DateTime.UtcNow)
    {
    }

    public override string ToString()
    {
        return $"ElevationPoint(Pos: {Position}, Elev: {Elevation:F2}m)";
    }
}
