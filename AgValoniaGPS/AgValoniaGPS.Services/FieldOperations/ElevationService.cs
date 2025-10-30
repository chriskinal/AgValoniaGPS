using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for managing elevation data in a grid-based format
/// Thread-safe implementation with grid-based storage and bilinear interpolation
/// </summary>
public class ElevationService : IElevationService
{
    private readonly object _lock = new();
    private readonly ElevationGrid _grid = new();

    public event EventHandler? ElevationDataCleared;
    public event EventHandler<ElevationPoint>? ElevationPointAdded;

    public ElevationGrid ElevationGrid
    {
        get
        {
            lock (_lock)
            {
                return _grid;
            }
        }
    }

    public double MinElevation
    {
        get
        {
            lock (_lock)
            {
                return _grid.MinElevation;
            }
        }
    }

    public double MaxElevation
    {
        get
        {
            lock (_lock)
            {
                return _grid.MaxElevation;
            }
        }
    }

    public double AverageElevation
    {
        get
        {
            lock (_lock)
            {
                return _grid.AverageElevation;
            }
        }
    }

    public int PointCount
    {
        get
        {
            lock (_lock)
            {
                return _grid.PointCount;
            }
        }
    }

    public double GridResolution
    {
        get
        {
            lock (_lock)
            {
                return _grid.GridResolution;
            }
        }
    }

    public void SetGridResolution(double resolution)
    {
        if (resolution <= 0)
        {
            throw new ArgumentException("Grid resolution must be greater than zero", nameof(resolution));
        }

        lock (_lock)
        {
            _grid.GridResolution = resolution;
        }
    }

    public void AddElevationPoint(Position2D position, double elevation)
    {
        lock (_lock)
        {
            var cell = PositionToGridCell(position);
            _grid.ElevationPoints[cell] = elevation;
            _grid.UpdateBoundingBox(position);
            _grid.UpdateStatistics();
        }

        ElevationPointAdded?.Invoke(this, new ElevationPoint(position, elevation));
    }

    public double? GetElevationAt(Position2D position)
    {
        lock (_lock)
        {
            var cell = PositionToGridCell(position);

            // If we have exact grid cell data, return it
            if (_grid.ElevationPoints.TryGetValue(cell, out var elevation))
            {
                return elevation;
            }

            // Otherwise, try interpolation
            return InterpolateBilinear(position);
        }
    }

    public double? InterpolateBilinear(Position2D position)
    {
        lock (_lock)
        {
            // Get the four surrounding grid cells
            double gridX = position.Easting / _grid.GridResolution;
            double gridY = position.Northing / _grid.GridResolution;

            int x0 = (int)Math.Floor(gridX);
            int y0 = (int)Math.Floor(gridY);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            // Get the four corner elevations
            var cell00 = new GridCell(x0, y0);
            var cell10 = new GridCell(x1, y0);
            var cell01 = new GridCell(x0, y1);
            var cell11 = new GridCell(x1, y1);

            if (!_grid.ElevationPoints.TryGetValue(cell00, out var elev00)) return null;
            if (!_grid.ElevationPoints.TryGetValue(cell10, out var elev10)) return null;
            if (!_grid.ElevationPoints.TryGetValue(cell01, out var elev01)) return null;
            if (!_grid.ElevationPoints.TryGetValue(cell11, out var elev11)) return null;

            // Calculate interpolation weights
            double tx = gridX - x0; // 0 to 1 between x0 and x1
            double ty = gridY - y0; // 0 to 1 between y0 and y1

            // Bilinear interpolation
            // First interpolate along X axis
            double elev0 = elev00 * (1 - tx) + elev10 * tx; // Bottom edge
            double elev1 = elev01 * (1 - tx) + elev11 * tx; // Top edge

            // Then interpolate along Y axis
            double elevation = elev0 * (1 - ty) + elev1 * ty;

            return elevation;
        }
    }

    public GridCell PositionToGridCell(Position2D position)
    {
        lock (_lock)
        {
            int x = (int)Math.Floor(position.Easting / _grid.GridResolution);
            int y = (int)Math.Floor(position.Northing / _grid.GridResolution);
            return new GridCell(x, y);
        }
    }

    public Position2D GridCellToPosition(GridCell cell)
    {
        lock (_lock)
        {
            double easting = cell.X * _grid.GridResolution;
            double northing = cell.Y * _grid.GridResolution;
            return new Position2D(easting, northing);
        }
    }

    public void ClearElevationData()
    {
        lock (_lock)
        {
            _grid.Clear();
        }

        ElevationDataCleared?.Invoke(this, EventArgs.Empty);
    }
}
