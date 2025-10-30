using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for managing a collection of guidance tracks (AB lines, curves, contours).
/// Provides auto-switching, nudging, and worked track management.
/// </summary>
public interface ITrackManagementService
{
    /// <summary>
    /// Event raised when the active track changes
    /// </summary>
    event EventHandler<TrackChangedEventArgs>? ActiveTrackChanged;

    /// <summary>
    /// Event raised when a track is added to the collection
    /// </summary>
    event EventHandler<TrackChangedEventArgs>? TrackAdded;

    /// <summary>
    /// Event raised when a track is removed from the collection
    /// </summary>
    event EventHandler<TrackChangedEventArgs>? TrackRemoved;

    /// <summary>
    /// Add a track from an ABLine
    /// </summary>
    /// <param name="abLine">The AB line to add as a track</param>
    /// <returns>The created GuidanceTrack</returns>
    GuidanceTrack AddTrack(ABLine abLine);

    /// <summary>
    /// Add a track from a CurveLine
    /// </summary>
    /// <param name="curveLine">The curve line to add as a track</param>
    /// <returns>The created GuidanceTrack</returns>
    GuidanceTrack AddTrack(CurveLine curveLine);

    /// <summary>
    /// Add a track from a ContourLine
    /// </summary>
    /// <param name="contourLine">The contour line to add as a track</param>
    /// <returns>The created GuidanceTrack</returns>
    GuidanceTrack AddTrack(ContourLine contourLine);

    /// <summary>
    /// Remove a track by ID
    /// </summary>
    /// <param name="trackId">ID of track to remove</param>
    /// <returns>True if track was removed</returns>
    bool RemoveTrack(int trackId);

    /// <summary>
    /// Clear all tracks
    /// </summary>
    void ClearTracks();

    /// <summary>
    /// Get all tracks in the collection
    /// </summary>
    /// <returns>List of all tracks</returns>
    List<GuidanceTrack> GetAllTracks();

    /// <summary>
    /// Get a track by ID
    /// </summary>
    /// <param name="trackId">Track ID</param>
    /// <returns>The track, or null if not found</returns>
    GuidanceTrack? GetTrack(int trackId);

    /// <summary>
    /// Get the currently active track
    /// </summary>
    /// <returns>Active track, or null if none</returns>
    GuidanceTrack? GetActiveTrack();

    /// <summary>
    /// Set the active track by ID
    /// </summary>
    /// <param name="trackId">ID of track to activate</param>
    /// <returns>True if track was activated</returns>
    bool SetActiveTrack(int trackId);

    /// <summary>
    /// Find and activate the closest track to current position
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="maxDistance">Maximum distance to consider (meters)</param>
    /// <returns>The activated track, or null if none within range</returns>
    GuidanceTrack? SwitchToClosestTrack(Position currentPosition, double maxDistance = 50.0);

    /// <summary>
    /// Cycle to the next track in the collection
    /// </summary>
    /// <param name="forward">True to cycle forward, false to cycle backward</param>
    /// <returns>The newly activated track, or null if no tracks</returns>
    GuidanceTrack? CycleTrack(bool forward = true);

    /// <summary>
    /// Nudge (offset) the active track perpendicular to its heading
    /// </summary>
    /// <param name="offsetMeters">Offset distance in meters (positive = right, negative = left)</param>
    /// <returns>True if nudge was applied</returns>
    bool NudgeActiveTrack(double offsetMeters);

    /// <summary>
    /// Mark a track as worked
    /// </summary>
    /// <param name="trackId">ID of track to mark</param>
    /// <param name="worked">True to mark as worked, false to mark as not worked</param>
    /// <returns>True if track status was updated</returns>
    bool SetTrackWorked(int trackId, bool worked);

    /// <summary>
    /// Get all worked track IDs
    /// </summary>
    /// <returns>Set of worked track IDs</returns>
    HashSet<int> GetWorkedTrackIds();

    /// <summary>
    /// Find the next unworked track starting from current track
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <returns>Next unworked track, or null if all tracks are worked</returns>
    GuidanceTrack? FindNextUnworkedTrack(Position currentPosition);

    /// <summary>
    /// Get tracks filtered by mode
    /// </summary>
    /// <param name="mode">Track mode to filter by</param>
    /// <returns>List of tracks matching the mode</returns>
    List<GuidanceTrack> GetTracksByMode(TrackMode mode);

    /// <summary>
    /// Get the total number of tracks
    /// </summary>
    /// <returns>Track count</returns>
    int GetTrackCount();

    /// <summary>
    /// Update track distances from current position
    /// Should be called frequently during operation
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    void UpdateTrackDistances(Position currentPosition);
}

/// <summary>
/// Event arguments for track changes
/// </summary>
public class TrackChangedEventArgs : EventArgs
{
    public GuidanceTrack Track { get; }
    public TrackChangeType ChangeType { get; }
    public DateTime Timestamp { get; }

    public TrackChangedEventArgs(GuidanceTrack track, TrackChangeType changeType)
    {
        Track = track;
        ChangeType = changeType;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Type of track change event
/// </summary>
public enum TrackChangeType
{
    Added,
    Removed,
    Activated,
    Deactivated,
    Nudged,
    MarkedWorked,
    MarkedNotWorked
}
