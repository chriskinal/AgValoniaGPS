using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for managing tram lines (guidance paths for vehicle wheels)
/// Provides tram line generation, proximity detection, and file I/O
/// </summary>
public interface ITramLineService
{
    /// <summary>
    /// Event raised when vehicle is within proximity threshold of a tram line
    /// </summary>
    event EventHandler<TramLineProximityEventArgs>? TramLineProximity;

    /// <summary>
    /// Generate tram lines from a base line with specified spacing
    /// </summary>
    /// <param name="lineStart">Start position of base line</param>
    /// <param name="lineEnd">End position of base line</param>
    /// <param name="spacing">Spacing between tram lines in meters</param>
    /// <param name="count">Number of tram lines to generate on each side</param>
    void GenerateTramLines(Position lineStart, Position lineEnd, double spacing, int count);

    /// <summary>
    /// Generate tram lines from an AB line with specified spacing
    /// </summary>
    /// <param name="abLine">The AB line to generate tram lines from</param>
    /// <param name="spacing">Spacing between tram lines in meters</param>
    /// <param name="count">Number of tram lines to generate on each side of the AB line</param>
    /// <param name="unitSystem">Unit system for spacing (Metric or Imperial)</param>
    void GenerateFromABLine(ABLine abLine, double spacing, int count, UnitSystem unitSystem = UnitSystem.Metric);

    /// <summary>
    /// Load tram lines from a Position array
    /// </summary>
    /// <param name="tramLines">Array of tram lines, each line is an array of positions</param>
    void LoadTramLines(Position[][] tramLines);

    /// <summary>
    /// Clear all tram lines
    /// </summary>
    void ClearTramLines();

    /// <summary>
    /// Get all tram lines
    /// </summary>
    /// <returns>Array of tram lines, or null if no tram lines loaded</returns>
    Position[][]? GetTramLines();

    /// <summary>
    /// Get the number of tram lines
    /// </summary>
    /// <returns>Number of tram lines</returns>
    int GetTramLineCount();

    /// <summary>
    /// Get a specific tram line by ID
    /// </summary>
    /// <param name="tramLineId">ID of the tram line</param>
    /// <returns>Array of positions representing the tram line, or null if not found</returns>
    Position[]? GetTramLine(int tramLineId);

    /// <summary>
    /// Get the distance to the nearest tram line
    /// </summary>
    /// <param name="position">Current position</param>
    /// <returns>Distance to nearest tram line in meters</returns>
    double GetDistanceToNearestTramLine(Position position);

    /// <summary>
    /// Get the ID of the nearest tram line
    /// </summary>
    /// <param name="position">Current position</param>
    /// <returns>ID of nearest tram line, or -1 if no tram lines</returns>
    int GetNearestTramLineId(Position position);

    /// <summary>
    /// Check proximity to tram lines and raise event if within threshold
    /// </summary>
    /// <param name="position">Current position</param>
    /// <param name="threshold">Distance threshold in meters</param>
    void CheckProximity(Position position, double threshold);

    /// <summary>
    /// Set the spacing between tram lines
    /// </summary>
    /// <param name="spacing">Spacing in meters</param>
    void SetSpacing(double spacing);

    /// <summary>
    /// Get the current spacing between tram lines
    /// </summary>
    /// <returns>Spacing in meters</returns>
    double GetSpacing();
}
