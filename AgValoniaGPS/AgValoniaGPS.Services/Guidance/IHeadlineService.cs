using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service interface for headline guidance operations.
/// Headlines are moveable guidance paths that provide an alternative to AB lines.
/// </summary>
public interface IHeadlineService
{
    /// <summary>
    /// Event raised when a headline is created, modified, or deleted.
    /// </summary>
    event EventHandler<HeadlineChangedEventArgs>? HeadlineChanged;

    /// <summary>
    /// Creates a new headline from a recorded path.
    /// </summary>
    /// <param name="trackPoints">Points defining the headline path.</param>
    /// <param name="name">Name for the headline.</param>
    /// <param name="aPointIndex">Index of the A-point in the track points.</param>
    /// <returns>The created headline.</returns>
    Headline CreateHeadline(List<Position> trackPoints, string name, int aPointIndex = 0);

    /// <summary>
    /// Creates a new headline from an existing position and heading.
    /// </summary>
    /// <param name="startPosition">Starting position for the headline.</param>
    /// <param name="heading">Heading angle in radians.</param>
    /// <param name="name">Name for the headline.</param>
    /// <param name="length">Length of the headline in meters (default: 1000m).</param>
    /// <returns>The created headline.</returns>
    Headline CreateHeadlineFromHeading(Position startPosition, double heading, string name, double length = 1000.0);

    /// <summary>
    /// Moves (nudges) a headline by the specified distance perpendicular to its path.
    /// </summary>
    /// <param name="headlineId">ID of the headline to move.</param>
    /// <param name="offsetMeters">Distance to move in meters (positive = right, negative = left).</param>
    /// <returns>The modified headline with updated positions.</returns>
    Headline MoveHeadline(int headlineId, double offsetMeters);

    /// <summary>
    /// Sets the specified headline as the active guidance line.
    /// </summary>
    /// <param name="headlineId">ID of the headline to activate.</param>
    /// <returns>True if successful, false if headline not found.</returns>
    bool SetActiveHeadline(int headlineId);

    /// <summary>
    /// Gets the currently active headline.
    /// </summary>
    /// <returns>The active headline, or null if none is active.</returns>
    Headline? GetActiveHeadline();

    /// <summary>
    /// Gets a headline by its ID.
    /// </summary>
    /// <param name="headlineId">ID of the headline to retrieve.</param>
    /// <returns>The headline if found, null otherwise.</returns>
    Headline? GetHeadline(int headlineId);

    /// <summary>
    /// Gets all headlines in the collection.
    /// </summary>
    /// <returns>List of all headlines.</returns>
    List<Headline> ListHeadlines();

    /// <summary>
    /// Deletes a headline from the collection.
    /// </summary>
    /// <param name="headlineId">ID of the headline to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    bool DeleteHeadline(int headlineId);

    /// <summary>
    /// Clears all headlines from the collection.
    /// </summary>
    void ClearAllHeadlines();

    /// <summary>
    /// Calculates guidance information for the current position relative to the active headline.
    /// </summary>
    /// <param name="currentPosition">Current vehicle position.</param>
    /// <param name="currentHeading">Current vehicle heading in radians.</param>
    /// <returns>Guidance result with cross-track error and heading information.</returns>
    GuidanceLineResult? CalculateGuidance(Position currentPosition, double currentHeading);

    /// <summary>
    /// Finds the closest point on a headline to the given position.
    /// </summary>
    /// <param name="position">Position to check.</param>
    /// <param name="headline">Headline to check against.</param>
    /// <returns>The closest point on the headline path.</returns>
    ClosestPointResult GetClosestPoint(Position position, Headline headline);

    /// <summary>
    /// Validates a headline for correctness.
    /// </summary>
    /// <param name="headline">Headline to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    ValidationResult ValidateHeadline(Headline headline);
}

/// <summary>
/// Event arguments for headline change notifications.
/// </summary>
public class HeadlineChangedEventArgs : EventArgs
{
    public Headline Headline { get; }
    public HeadlineChangeType ChangeType { get; }

    public HeadlineChangedEventArgs(Headline headline, HeadlineChangeType changeType)
    {
        Headline = headline;
        ChangeType = changeType;
    }
}

/// <summary>
/// Types of changes that can occur to a headline.
/// </summary>
public enum HeadlineChangeType
{
    Created,
    Modified,
    Moved,
    Activated,
    Deactivated,
    Deleted
}
