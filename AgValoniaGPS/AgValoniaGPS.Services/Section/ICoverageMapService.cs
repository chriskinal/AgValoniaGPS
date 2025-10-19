using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Service interface for coverage mapping with triangle strip tracking and overlap detection
/// Provides spatial indexing for efficient overlap queries
/// </summary>
public interface ICoverageMapService
{
    /// <summary>
    /// Event fired when coverage map is updated with new triangles
    /// </summary>
    event EventHandler<CoverageMapUpdatedEventArgs>? CoverageUpdated;

    /// <summary>
    /// Adds coverage triangles to the map and detects overlaps
    /// </summary>
    /// <param name="triangles">Triangles to add</param>
    void AddCoverageTriangles(IEnumerable<CoverageTriangle> triangles);

    /// <summary>
    /// Gets coverage information at a specific position
    /// </summary>
    /// <param name="position">Position to query</param>
    /// <returns>Overlap count at position (0 = not covered, 1 = single pass, 2+ = overlap)</returns>
    int GetCoverageAt(Position position);

    /// <summary>
    /// Gets the total covered area in square meters
    /// </summary>
    /// <returns>Total covered area</returns>
    double GetTotalCoveredArea();

    /// <summary>
    /// Gets overlap statistics (single pass, double pass, triple+ pass areas)
    /// </summary>
    /// <returns>Dictionary with overlap counts as keys and areas as values</returns>
    Dictionary<int, double> GetOverlapStatistics();

    /// <summary>
    /// Gets all triangles in the coverage map
    /// </summary>
    /// <returns>Collection of all coverage triangles</returns>
    IReadOnlyList<CoverageTriangle> GetAllTriangles();

    /// <summary>
    /// Clears all coverage data
    /// </summary>
    void Clear();
}
