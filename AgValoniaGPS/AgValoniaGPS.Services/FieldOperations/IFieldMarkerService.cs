using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for managing field markers (flags, notes, obstacles, waypoints)
/// </summary>
public interface IFieldMarkerService
{
    /// <summary>
    /// Event raised when a marker is added
    /// </summary>
    event EventHandler<MarkerChangedEventArgs>? MarkerAdded;

    /// <summary>
    /// Event raised when a marker is removed
    /// </summary>
    event EventHandler<MarkerChangedEventArgs>? MarkerRemoved;

    /// <summary>
    /// Event raised when a marker is updated
    /// </summary>
    event EventHandler<MarkerChangedEventArgs>? MarkerUpdated;

    /// <summary>
    /// Add a new marker to the collection
    /// </summary>
    /// <param name="marker">The marker to add</param>
    /// <returns>The added marker with assigned ID</returns>
    FieldMarker AddMarker(FieldMarker marker);

    /// <summary>
    /// Add a new marker at the specified position
    /// </summary>
    /// <param name="type">Type of marker to create</param>
    /// <param name="position">Position to place the marker</param>
    /// <param name="name">Optional name for the marker</param>
    /// <param name="note">Optional note text</param>
    /// <returns>The created marker</returns>
    FieldMarker AddMarker(MarkerType type, Position position, string name = "", string note = "");

    /// <summary>
    /// Remove a marker by ID
    /// </summary>
    /// <param name="markerId">ID of marker to remove</param>
    /// <returns>True if marker was removed</returns>
    bool RemoveMarker(int markerId);

    /// <summary>
    /// Update an existing marker
    /// </summary>
    /// <param name="marker">The marker with updated properties</param>
    /// <returns>True if marker was updated</returns>
    bool UpdateMarker(FieldMarker marker);

    /// <summary>
    /// Clear all markers
    /// </summary>
    void ClearMarkers();

    /// <summary>
    /// Clear all markers for a specific field
    /// </summary>
    /// <param name="fieldId">ID of field whose markers to clear</param>
    void ClearMarkersForField(int fieldId);

    /// <summary>
    /// Get all markers in the collection
    /// </summary>
    /// <returns>List of all markers</returns>
    List<FieldMarker> GetAllMarkers();

    /// <summary>
    /// Get a marker by ID
    /// </summary>
    /// <param name="markerId">Marker ID</param>
    /// <returns>The marker, or null if not found</returns>
    FieldMarker? GetMarker(int markerId);

    /// <summary>
    /// Get all markers for a specific field
    /// </summary>
    /// <param name="fieldId">Field ID</param>
    /// <returns>List of markers for the field</returns>
    List<FieldMarker> GetMarkersForField(int fieldId);

    /// <summary>
    /// Get markers filtered by type
    /// </summary>
    /// <param name="type">Marker type to filter by</param>
    /// <returns>List of markers matching the type</returns>
    List<FieldMarker> GetMarkersByType(MarkerType type);

    /// <summary>
    /// Get markers filtered by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <returns>List of markers in the category</returns>
    List<FieldMarker> GetMarkersByCategory(string category);

    /// <summary>
    /// Get markers within a certain distance of a position
    /// </summary>
    /// <param name="position">Reference position</param>
    /// <param name="maxDistanceMeters">Maximum distance in meters</param>
    /// <returns>List of markers within range, sorted by distance</returns>
    List<FieldMarker> GetMarkersNearPosition(Position position, double maxDistanceMeters);

    /// <summary>
    /// Get the nearest marker to a position
    /// </summary>
    /// <param name="position">Reference position</param>
    /// <param name="maxDistanceMeters">Maximum distance to search (meters)</param>
    /// <returns>Nearest marker, or null if none within range</returns>
    FieldMarker? GetNearestMarker(Position position, double maxDistanceMeters = double.MaxValue);

    /// <summary>
    /// Toggle marker visibility
    /// </summary>
    /// <param name="markerId">ID of marker to toggle</param>
    /// <returns>True if marker visibility was toggled</returns>
    bool ToggleMarkerVisibility(int markerId);

    /// <summary>
    /// Set visibility for all markers of a specific type
    /// </summary>
    /// <param name="type">Marker type</param>
    /// <param name="visible">True to show, false to hide</param>
    void SetTypeVisibility(MarkerType type, bool visible);

    /// <summary>
    /// Get the total number of markers
    /// </summary>
    /// <returns>Marker count</returns>
    int GetMarkerCount();

    /// <summary>
    /// Get count of markers by type
    /// </summary>
    /// <param name="type">Marker type</param>
    /// <returns>Count of markers of that type</returns>
    int GetMarkerCountByType(MarkerType type);

    /// <summary>
    /// Search markers by name or note content
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="caseSensitive">Whether search is case sensitive</param>
    /// <returns>List of matching markers</returns>
    List<FieldMarker> SearchMarkers(string searchText, bool caseSensitive = false);
}

/// <summary>
/// Event arguments for marker changes
/// </summary>
public class MarkerChangedEventArgs : EventArgs
{
    public FieldMarker Marker { get; }
    public MarkerChangeType ChangeType { get; }
    public DateTime Timestamp { get; }

    public MarkerChangedEventArgs(FieldMarker marker, MarkerChangeType changeType)
    {
        Marker = marker;
        ChangeType = changeType;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Type of marker change event
/// </summary>
public enum MarkerChangeType
{
    Added,
    Removed,
    Updated,
    VisibilityChanged
}
