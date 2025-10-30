using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents an elevation grid for terrain elevation tracking
/// Uses grid-based storage with configurable resolution
/// </summary>
public class ElevationGrid
{
    /// <summary>
    /// Grid resolution in meters (default 5.0m)
    /// Determines the spacing between grid cells
    /// </summary>
    public double GridResolution { get; set; } = 5.0;

    /// <summary>
    /// Minimum elevation in meters
    /// </summary>
    public double MinElevation { get; private set; } = double.MaxValue;

    /// <summary>
    /// Maximum elevation in meters
    /// </summary>
    public double MaxElevation { get; private set; } = double.MinValue;

    /// <summary>
    /// Average elevation in meters
    /// </summary>
    public double AverageElevation { get; private set; }

    /// <summary>
    /// Elevation data stored by grid cell
    /// Key: GridCell (X, Y indices)
    /// Value: Elevation in meters
    /// </summary>
    public Dictionary<GridCell, double> ElevationPoints { get; } = new();

    /// <summary>
    /// Minimum easting (X) coordinate in meters
    /// </summary>
    public double MinEasting { get; private set; } = double.MaxValue;

    /// <summary>
    /// Maximum easting (X) coordinate in meters
    /// </summary>
    public double MaxEasting { get; private set; } = double.MinValue;

    /// <summary>
    /// Minimum northing (Y) coordinate in meters
    /// </summary>
    public double MinNorthing { get; private set; } = double.MaxValue;

    /// <summary>
    /// Maximum northing (Y) coordinate in meters
    /// </summary>
    public double MaxNorthing { get; private set; } = double.MinValue;

    /// <summary>
    /// Total number of elevation points in the grid
    /// </summary>
    public int PointCount => ElevationPoints.Count;

    /// <summary>
    /// Updates the statistics (min, max, average) based on current elevation data
    /// Should be called after adding or removing points
    /// </summary>
    public void UpdateStatistics()
    {
        if (ElevationPoints.Count == 0)
        {
            MinElevation = double.MaxValue;
            MaxElevation = double.MinValue;
            AverageElevation = 0;
            return;
        }

        MinElevation = double.MaxValue;
        MaxElevation = double.MinValue;
        double sum = 0;

        foreach (var elevation in ElevationPoints.Values)
        {
            if (elevation < MinElevation) MinElevation = elevation;
            if (elevation > MaxElevation) MaxElevation = elevation;
            sum += elevation;
        }

        AverageElevation = sum / ElevationPoints.Count;
    }

    /// <summary>
    /// Updates the bounding box based on a new position
    /// </summary>
    public void UpdateBoundingBox(Position2D position)
    {
        if (position.Easting < MinEasting) MinEasting = position.Easting;
        if (position.Easting > MaxEasting) MaxEasting = position.Easting;
        if (position.Northing < MinNorthing) MinNorthing = position.Northing;
        if (position.Northing > MaxNorthing) MaxNorthing = position.Northing;
    }

    /// <summary>
    /// Clears all elevation data
    /// </summary>
    public void Clear()
    {
        ElevationPoints.Clear();
        MinElevation = double.MaxValue;
        MaxElevation = double.MinValue;
        AverageElevation = 0;
        MinEasting = double.MaxValue;
        MaxEasting = double.MinValue;
        MinNorthing = double.MaxValue;
        MaxNorthing = double.MinValue;
    }
}
