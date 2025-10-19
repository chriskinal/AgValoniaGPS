using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Provides point-in-polygon containment checks using ray-casting algorithm with optional R-tree spatial indexing.
/// Thread-safe service optimized for 10Hz GPS updates with <1ms per check performance target.
/// </summary>
/// <remarks>
/// Implements ray-casting algorithm for point containment checks with edge case handling.
/// Uses R-tree spatial indexing for polygons with >500 vertices for performance optimization.
/// All methods are thread-safe for concurrent access.
/// </remarks>
public interface IPointInPolygonService
{
    /// <summary>
    /// Determines if a point is inside a polygon using ray-casting algorithm.
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <param name="polygon">Array of polygon vertices in order</param>
    /// <returns>True if point is inside or on the edge of the polygon, false otherwise</returns>
    /// <remarks>
    /// Points on polygon edges or vertices are considered inside.
    /// Time complexity: O(n) for simple check, O(log n) with spatial index.
    /// </remarks>
    bool IsPointInside(Position point, Position[] polygon);

    /// <summary>
    /// Determines if a point is inside a polygon with holes (inner boundaries).
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <param name="outerBoundary">Outer polygon boundary vertices</param>
    /// <param name="holes">Optional array of hole polygons (inner boundaries)</param>
    /// <returns>True if point is inside outer boundary and outside all holes, false otherwise</returns>
    /// <remarks>
    /// Used for fields with inner boundaries (excluded areas).
    /// Point must be inside outer boundary AND outside all holes to return true.
    /// </remarks>
    bool IsPointInside(Position point, Position[] outerBoundary, Position[][] holes);

    /// <summary>
    /// Classifies a point's location relative to a polygon boundary.
    /// </summary>
    /// <param name="point">The point to classify</param>
    /// <param name="polygon">Array of polygon vertices in order</param>
    /// <returns>PointLocation indicating Inside, Outside, or OnBoundary</returns>
    /// <remarks>
    /// Provides more detailed location information than boolean IsPointInside.
    /// Useful for boundary proximity detection and validation.
    /// </remarks>
    PointLocation ClassifyPoint(Position point, Position[] polygon);

    /// <summary>
    /// Builds an R-tree spatial index for a polygon to optimize repeated point-in-polygon checks.
    /// </summary>
    /// <param name="polygon">The polygon to index</param>
    /// <remarks>
    /// Only beneficial for polygons with >500 vertices or when performing many checks.
    /// Index is cached until ClearSpatialIndex() is called or a new polygon is indexed.
    /// Thread-safe: only one polygon can be indexed at a time.
    /// </remarks>
    void BuildSpatialIndex(Position[] polygon);

    /// <summary>
    /// Clears the current spatial index, freeing memory.
    /// </summary>
    /// <remarks>
    /// Call when done with a polygon or when switching to a different polygon.
    /// Thread-safe operation.
    /// </remarks>
    void ClearSpatialIndex();

    /// <summary>
    /// Gets the duration of the last point-in-polygon check in milliseconds.
    /// </summary>
    /// <returns>Duration in milliseconds, or 0 if no checks have been performed</returns>
    /// <remarks>
    /// Used for performance monitoring and benchmarking.
    /// Should be <1ms for typical operations.
    /// </remarks>
    double GetLastCheckDurationMs();

    /// <summary>
    /// Gets the total number of point-in-polygon checks performed since service creation.
    /// </summary>
    /// <returns>Total check count</returns>
    /// <remarks>
    /// Thread-safe counter for statistics and monitoring.
    /// Useful for performance profiling and optimization decisions.
    /// </remarks>
    long GetTotalChecksPerformed();
}

/// <summary>
/// Specifies the location of a point relative to a polygon boundary
/// </summary>
public enum PointLocation
{
    /// <summary>
    /// Point is inside the polygon
    /// </summary>
    Inside,

    /// <summary>
    /// Point is outside the polygon
    /// </summary>
    Outside,

    /// <summary>
    /// Point is on the polygon boundary (edge or vertex)
    /// </summary>
    OnBoundary
}
