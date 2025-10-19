using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Provides boundary management services including loading, validation, simplification, and violation detection.
/// Thread-safe service optimized for real-time position checking with <2ms performance target.
/// </summary>
/// <remarks>
/// Supports multiple file formats: AgOpenGPS Boundary.txt, GeoJSON, and KML.
/// Uses PointInPolygonService for efficient boundary violation checks.
/// Implements Douglas-Peucker simplification algorithm for boundary optimization.
/// All methods are thread-safe for concurrent access.
/// </remarks>
public interface IBoundaryManagementService
{
    /// <summary>
    /// Event raised when a position violates the boundary (outside the boundary)
    /// </summary>
    event EventHandler<BoundaryViolationEventArgs>? BoundaryViolation;

    /// <summary>
    /// Loads a boundary from an array of positions
    /// </summary>
    /// <param name="boundary">Array of positions defining the boundary</param>
    /// <remarks>
    /// Replaces any existing boundary. Clears boundary if null or empty array provided.
    /// Thread-safe operation.
    /// </remarks>
    void LoadBoundary(Position[] boundary);

    /// <summary>
    /// Clears the current boundary
    /// </summary>
    /// <remarks>
    /// Thread-safe operation. Subsequent boundary checks will return false.
    /// </remarks>
    void ClearBoundary();

    /// <summary>
    /// Gets the current boundary
    /// </summary>
    /// <returns>Array of positions defining the boundary, or null if no boundary loaded</returns>
    /// <remarks>
    /// Thread-safe operation. Returns a copy to prevent external modification.
    /// </remarks>
    Position[]? GetCurrentBoundary();

    /// <summary>
    /// Checks if a boundary is currently loaded
    /// </summary>
    /// <returns>True if a boundary is loaded, false otherwise</returns>
    /// <remarks>
    /// Thread-safe operation.
    /// </remarks>
    bool HasBoundary();

    /// <summary>
    /// Checks if a position is inside the current boundary
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if inside boundary, false if outside or no boundary loaded</returns>
    /// <remarks>
    /// Uses PointInPolygonService for efficient checking.
    /// Thread-safe operation with <2ms performance target.
    /// </remarks>
    bool IsInsideBoundary(Position position);

    /// <summary>
    /// Calculates the area of the current boundary using Shoelace formula
    /// </summary>
    /// <returns>Area in square meters, or 0 if no boundary loaded</returns>
    /// <remarks>
    /// Uses UTM coordinates for accurate area calculation.
    /// O(n) time complexity where n is number of vertices.
    /// </remarks>
    double CalculateArea();

    /// <summary>
    /// Simplifies the current boundary using Douglas-Peucker algorithm
    /// </summary>
    /// <param name="tolerance">Tolerance in meters for simplification</param>
    /// <returns>Simplified boundary as array of positions</returns>
    /// <remarks>
    /// Higher tolerance removes more points but may lose accuracy.
    /// Typical tolerance: 0.5-2.0 meters for agricultural applications.
    /// Time complexity: O(n log n) average case.
    /// </remarks>
    Position[] SimplifyBoundary(double tolerance);

    /// <summary>
    /// Checks a position and raises BoundaryViolation event if outside boundary
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <remarks>
    /// Use this method for automated violation detection during operations.
    /// No event raised if position is inside boundary or no boundary loaded.
    /// Thread-safe operation.
    /// </remarks>
    void CheckPosition(Position position);
}
