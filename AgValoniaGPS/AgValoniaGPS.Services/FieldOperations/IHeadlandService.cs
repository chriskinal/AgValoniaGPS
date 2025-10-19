using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for headland generation, tracking, and management.
/// Provides real-time headland entry/exit detection and completion tracking.
/// </summary>
public interface IHeadlandService
{
    /// <summary>
    /// Raised when vehicle enters a headland area
    /// </summary>
    event EventHandler<HeadlandEntryEventArgs>? HeadlandEntry;

    /// <summary>
    /// Raised when vehicle exits a headland area
    /// </summary>
    event EventHandler<HeadlandExitEventArgs>? HeadlandExit;

    /// <summary>
    /// Raised when a headland pass is completed
    /// </summary>
    event EventHandler<HeadlandCompletedEventArgs>? HeadlandCompleted;

    /// <summary>
    /// Generate headlands from field boundary with specified parameters
    /// </summary>
    /// <param name="boundary">Field boundary points (UTM coordinates)</param>
    /// <param name="passWidth">Width of each headland pass in meters</param>
    /// <param name="passCount">Number of headland passes to generate</param>
    void GenerateHeadlands(Position[] boundary, double passWidth, int passCount);

    /// <summary>
    /// Load pre-generated headlands
    /// </summary>
    /// <param name="headlands">Array of headland passes, each containing position points</param>
    void LoadHeadlands(Position[][] headlands);

    /// <summary>
    /// Clear all headlands
    /// </summary>
    void ClearHeadlands();

    /// <summary>
    /// Get all generated headland passes
    /// </summary>
    /// <returns>Array of headland passes, or null if no headlands loaded</returns>
    Position[][]? GetHeadlands();

    /// <summary>
    /// Check if a position is within any headland area
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if position is in headland, false otherwise</returns>
    bool IsInHeadland(Position position);

    /// <summary>
    /// Get the current headland pass number for a position
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>Pass number (0-based) or -1 if not in headland</returns>
    int GetCurrentPass(Position position);

    /// <summary>
    /// Mark a headland pass as completed
    /// </summary>
    /// <param name="passNumber">Pass number (0-based) to mark complete</param>
    void MarkPassCompleted(int passNumber);

    /// <summary>
    /// Check if a headland pass is completed
    /// </summary>
    /// <param name="passNumber">Pass number (0-based) to check</param>
    /// <returns>True if pass is completed, false otherwise</returns>
    bool IsPassCompleted(int passNumber);

    /// <summary>
    /// Check current position and raise entry/exit events if crossing headland boundary
    /// </summary>
    /// <param name="position">Current position to check</param>
    void CheckPosition(Position position);

    /// <summary>
    /// Set headland operation mode (Auto or Manual)
    /// </summary>
    /// <param name="mode">Mode to set</param>
    void SetMode(HeadlandMode mode);

    /// <summary>
    /// Get current headland operation mode
    /// </summary>
    /// <returns>Current mode</returns>
    HeadlandMode GetMode();
}
