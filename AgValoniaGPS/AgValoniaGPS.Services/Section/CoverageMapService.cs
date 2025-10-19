using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Implements thread-safe coverage mapping with grid-based spatial indexing
/// Tracks coverage triangles, detects overlaps, calculates areas
/// </summary>
/// <remarks>
/// Performance Target: Less than 2ms per position update for triangle generation + overlap check
/// Thread Safety: Uses lock object pattern for concurrent access
/// Data Structure: Grid-based spatial index (100x100m cells) for efficient overlap queries
/// Algorithm: Triangle strip generation (2 triangles per position update per section)
/// </remarks>
public class CoverageMapService : ICoverageMapService
{
    private const double GridCellSize = 100.0; // meters

    private readonly object _lockObject = new object();
    private readonly List<CoverageTriangle> _triangles;
    private readonly Dictionary<(int, int), List<CoverageTriangle>> _spatialIndex; // Grid-based index
    private double _totalCoveredArea;

    /// <summary>
    /// Event fired when coverage map is updated with new triangles
    /// </summary>
    public event EventHandler<CoverageMapUpdatedEventArgs>? CoverageUpdated;

    /// <summary>
    /// Creates a new instance of CoverageMapService with empty coverage map
    /// </summary>
    public CoverageMapService()
    {
        _triangles = new List<CoverageTriangle>();
        _spatialIndex = new Dictionary<(int, int), List<CoverageTriangle>>();
        _totalCoveredArea = 0.0;
    }

    /// <summary>
    /// Adds coverage triangles to the map and detects overlaps
    /// Updates spatial index and calculates total covered area
    /// </summary>
    /// <param name="triangles">Triangles to add</param>
    public void AddCoverageTriangles(IEnumerable<CoverageTriangle> triangles)
    {
        if (triangles == null)
            throw new ArgumentNullException(nameof(triangles));

        var triangleList = triangles.ToList();
        if (triangleList.Count == 0)
            return;

        lock (_lockObject)
        {
            foreach (var triangle in triangleList)
            {
                // Detect overlaps with existing triangles
                DetectAndMarkOverlaps(triangle);

                // Add to main list
                _triangles.Add(triangle);

                // Add to spatial index
                AddToSpatialIndex(triangle);

                // Update total area
                _totalCoveredArea += triangle.CalculateArea();
            }
        }

        // Fire event outside of lock
        RaiseCoverageUpdated(triangleList.Count, _totalCoveredArea);
    }

    /// <summary>
    /// Gets coverage information at a specific position
    /// </summary>
    /// <param name="position">Position to query</param>
    /// <returns>Maximum overlap count at position (0 = not covered, 1 = single pass, 2+ = overlap)</returns>
    public int GetCoverageAt(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lockObject)
        {
            var gridCell = GetGridCell(position.Easting, position.Northing);

            if (!_spatialIndex.TryGetValue(gridCell, out var candidateTriangles))
                return 0;

            int maxOverlapCount = 0;
            foreach (var triangle in candidateTriangles)
            {
                if (IsPointInTriangle(position, triangle))
                {
                    maxOverlapCount = Math.Max(maxOverlapCount, triangle.OverlapCount);
                }
            }

            return maxOverlapCount;
        }
    }

    /// <summary>
    /// Gets the total covered area in square meters
    /// </summary>
    /// <returns>Total covered area</returns>
    public double GetTotalCoveredArea()
    {
        lock (_lockObject)
        {
            return _totalCoveredArea;
        }
    }

    /// <summary>
    /// Gets overlap statistics (single pass, double pass, triple+ pass areas)
    /// </summary>
    /// <returns>Dictionary with overlap counts as keys and areas as values</returns>
    public Dictionary<int, double> GetOverlapStatistics()
    {
        lock (_lockObject)
        {
            var statistics = new Dictionary<int, double>();

            foreach (var triangle in _triangles)
            {
                int overlapKey = Math.Min(triangle.OverlapCount, 3); // Group 3+ together
                double area = triangle.CalculateArea();

                if (statistics.ContainsKey(overlapKey))
                    statistics[overlapKey] += area;
                else
                    statistics[overlapKey] = area;
            }

            return statistics;
        }
    }

    /// <summary>
    /// Gets all triangles in the coverage map (returns defensive copy)
    /// </summary>
    /// <returns>Read-only list of all coverage triangles</returns>
    public IReadOnlyList<CoverageTriangle> GetAllTriangles()
    {
        lock (_lockObject)
        {
            return _triangles.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Clears all coverage data
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _triangles.Clear();
            _spatialIndex.Clear();
            _totalCoveredArea = 0.0;
        }

        // Fire event with zero coverage
        RaiseCoverageUpdated(0, 0.0);
    }

    private void DetectAndMarkOverlaps(CoverageTriangle newTriangle)
    {
        // Get grid cells that this triangle might overlap with
        var gridCells = GetGridCellsForTriangle(newTriangle);

        int maxOverlapCount = 0;

        foreach (var gridCell in gridCells)
        {
            if (!_spatialIndex.TryGetValue(gridCell, out var candidateTriangles))
                continue;

            foreach (var existingTriangle in candidateTriangles)
            {
                // Check if triangles overlap
                if (TrianglesOverlap(newTriangle, existingTriangle))
                {
                    maxOverlapCount = Math.Max(maxOverlapCount, existingTriangle.OverlapCount);
                }
            }
        }

        // Set new triangle's overlap count (1 if first pass, existing+1 if overlap)
        newTriangle.OverlapCount = maxOverlapCount > 0 ? maxOverlapCount + 1 : 1;
    }

    private void AddToSpatialIndex(CoverageTriangle triangle)
    {
        var gridCells = GetGridCellsForTriangle(triangle);

        foreach (var gridCell in gridCells)
        {
            if (!_spatialIndex.ContainsKey(gridCell))
                _spatialIndex[gridCell] = new List<CoverageTriangle>();

            _spatialIndex[gridCell].Add(triangle);
        }
    }

    private (int, int) GetGridCell(double easting, double northing)
    {
        int cellX = (int)Math.Floor(easting / GridCellSize);
        int cellY = (int)Math.Floor(northing / GridCellSize);
        return (cellX, cellY);
    }

    private List<(int, int)> GetGridCellsForTriangle(CoverageTriangle triangle)
    {
        var cells = new HashSet<(int, int)>();

        // Add grid cells for each vertex
        foreach (var vertex in triangle.Vertices)
        {
            cells.Add(GetGridCell(vertex.Easting, vertex.Northing));
        }

        // Add grid cells for bounding box to catch triangles spanning multiple cells
        double minEasting = triangle.Vertices.Min(v => v.Easting);
        double maxEasting = triangle.Vertices.Max(v => v.Easting);
        double minNorthing = triangle.Vertices.Min(v => v.Northing);
        double maxNorthing = triangle.Vertices.Max(v => v.Northing);

        int minCellX = (int)Math.Floor(minEasting / GridCellSize);
        int maxCellX = (int)Math.Floor(maxEasting / GridCellSize);
        int minCellY = (int)Math.Floor(minNorthing / GridCellSize);
        int maxCellY = (int)Math.Floor(maxNorthing / GridCellSize);

        for (int x = minCellX; x <= maxCellX; x++)
        {
            for (int y = minCellY; y <= maxCellY; y++)
            {
                cells.Add((x, y));
            }
        }

        return cells.ToList();
    }

    private bool TrianglesOverlap(CoverageTriangle t1, CoverageTriangle t2)
    {
        // Simple overlap detection: check if any vertex of one triangle is inside the other
        // Or if any edges intersect
        // For performance, we use a simplified check: bounding box overlap + vertex containment

        // Quick bounding box check first
        if (!BoundingBoxesOverlap(t1, t2))
            return false;

        // Check if any vertex of t1 is inside t2
        foreach (var vertex in t1.Vertices)
        {
            if (IsPointInTriangle(vertex, t2))
                return true;
        }

        // Check if any vertex of t2 is inside t1
        foreach (var vertex in t2.Vertices)
        {
            if (IsPointInTriangle(vertex, t1))
                return true;
        }

        // For more robust detection, we'd also check edge intersections
        // But for performance target (<2ms), vertex checks are sufficient
        return false;
    }

    private bool BoundingBoxesOverlap(CoverageTriangle t1, CoverageTriangle t2)
    {
        double t1MinE = t1.Vertices.Min(v => v.Easting);
        double t1MaxE = t1.Vertices.Max(v => v.Easting);
        double t1MinN = t1.Vertices.Min(v => v.Northing);
        double t1MaxN = t1.Vertices.Max(v => v.Northing);

        double t2MinE = t2.Vertices.Min(v => v.Easting);
        double t2MaxE = t2.Vertices.Max(v => v.Easting);
        double t2MinN = t2.Vertices.Min(v => v.Northing);
        double t2MaxN = t2.Vertices.Max(v => v.Northing);

        return !(t1MaxE < t2MinE || t2MaxE < t1MinE || t1MaxN < t2MinN || t2MaxN < t1MinN);
    }

    private bool IsPointInTriangle(Position point, CoverageTriangle triangle)
    {
        // Use barycentric coordinate method for point-in-triangle test
        var v0 = triangle.Vertices[0];
        var v1 = triangle.Vertices[1];
        var v2 = triangle.Vertices[2];

        double dX = point.Easting - v2.Easting;
        double dY = point.Northing - v2.Northing;
        double dX21 = v2.Easting - v1.Easting;
        double dY12 = v1.Northing - v2.Northing;
        double D = dY12 * (v0.Easting - v2.Easting) + dX21 * (v0.Northing - v2.Northing);
        double s = dY12 * dX + dX21 * dY;
        double t = (v2.Northing - v0.Northing) * dX + (v0.Easting - v2.Easting) * dY;

        if (D < 0)
            return s <= 0 && t <= 0 && s + t >= D;
        return s >= 0 && t >= 0 && s + t <= D;
    }

    private void RaiseCoverageUpdated(int addedTrianglesCount, double totalCoveredArea)
    {
        CoverageUpdated?.Invoke(this, new CoverageMapUpdatedEventArgs(addedTrianglesCount, totalCoveredArea));
    }
}
