using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for managing elevation data in a grid-based format
/// Supports terrain elevation tracking and interpolation
/// </summary>
public interface IElevationService
{
    /// <summary>
    /// Raised when the elevation grid is cleared
    /// </summary>
    event EventHandler? ElevationDataCleared;

    /// <summary>
    /// Raised when a new elevation point is added
    /// </summary>
    event EventHandler<ElevationPoint>? ElevationPointAdded;

    /// <summary>
    /// Gets the current elevation grid
    /// </summary>
    ElevationGrid ElevationGrid { get; }

    /// <summary>
    /// Adds an elevation point to the grid
    /// </summary>
    /// <param name="position">Position in local coordinates (meters)</param>
    /// <param name="elevation">Elevation in meters</param>
    void AddElevationPoint(Position2D position, double elevation);

    /// <summary>
    /// Gets the elevation at a specific position
    /// If the position is not exactly on a grid point, bilinear interpolation is used
    /// </summary>
    /// <param name="position">Position in local coordinates (meters)</param>
    /// <returns>Elevation in meters, or null if no data available</returns>
    double? GetElevationAt(Position2D position);

    /// <summary>
    /// Gets interpolated elevation using bilinear interpolation between grid points
    /// </summary>
    /// <param name="position">Position in local coordinates (meters)</param>
    /// <returns>Interpolated elevation in meters, or null if insufficient data</returns>
    double? InterpolateBilinear(Position2D position);

    /// <summary>
    /// Converts a position to grid cell coordinates
    /// </summary>
    /// <param name="position">Position in local coordinates (meters)</param>
    /// <returns>Grid cell coordinates</returns>
    GridCell PositionToGridCell(Position2D position);

    /// <summary>
    /// Converts grid cell coordinates to position
    /// </summary>
    /// <param name="cell">Grid cell coordinates</param>
    /// <returns>Position in local coordinates (meters)</returns>
    Position2D GridCellToPosition(GridCell cell);

    /// <summary>
    /// Gets the minimum elevation in the grid
    /// </summary>
    double MinElevation { get; }

    /// <summary>
    /// Gets the maximum elevation in the grid
    /// </summary>
    double MaxElevation { get; }

    /// <summary>
    /// Gets the average elevation in the grid
    /// </summary>
    double AverageElevation { get; }

    /// <summary>
    /// Gets the total number of elevation points
    /// </summary>
    int PointCount { get; }

    /// <summary>
    /// Clears all elevation data
    /// </summary>
    void ClearElevationData();

    /// <summary>
    /// Sets the grid resolution (spacing between grid points in meters)
    /// </summary>
    /// <param name="resolution">Grid resolution in meters</param>
    void SetGridResolution(double resolution);

    /// <summary>
    /// Gets the current grid resolution in meters
    /// </summary>
    double GridResolution { get; }
}
