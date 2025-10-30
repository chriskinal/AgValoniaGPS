using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for managing field markers (flags, notes, obstacles, waypoints)
/// Thread-safe for concurrent access.
/// </summary>
public class FieldMarkerService : IFieldMarkerService
{
    private readonly object _lock = new object();
    private readonly Dictionary<int, FieldMarker> _markers = new();
    private int _nextMarkerId = 1;

    public event EventHandler<MarkerChangedEventArgs>? MarkerAdded;
    public event EventHandler<MarkerChangedEventArgs>? MarkerRemoved;
    public event EventHandler<MarkerChangedEventArgs>? MarkerUpdated;

    public FieldMarker AddMarker(FieldMarker marker)
    {
        if (marker == null)
            throw new ArgumentNullException(nameof(marker));

        lock (_lock)
        {
            // Assign new ID
            marker.Id = _nextMarkerId++;
            marker.CreatedDate = DateTime.UtcNow;
            marker.ModifiedDate = null;

            _markers[marker.Id] = marker;

            MarkerAdded?.Invoke(this, new MarkerChangedEventArgs(marker, MarkerChangeType.Added));
            return marker;
        }
    }

    public FieldMarker AddMarker(MarkerType type, Position position, string name = "", string note = "")
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        var marker = FieldMarker.CreateDefault(type, position, name);
        marker.Note = note;

        return AddMarker(marker);
    }

    public bool RemoveMarker(int markerId)
    {
        lock (_lock)
        {
            if (!_markers.TryGetValue(markerId, out var marker))
                return false;

            _markers.Remove(markerId);
            MarkerRemoved?.Invoke(this, new MarkerChangedEventArgs(marker, MarkerChangeType.Removed));
            return true;
        }
    }

    public bool UpdateMarker(FieldMarker marker)
    {
        if (marker == null)
            throw new ArgumentNullException(nameof(marker));

        lock (_lock)
        {
            if (!_markers.ContainsKey(marker.Id))
                return false;

            marker.ModifiedDate = DateTime.UtcNow;
            _markers[marker.Id] = marker;

            MarkerUpdated?.Invoke(this, new MarkerChangedEventArgs(marker, MarkerChangeType.Updated));
            return true;
        }
    }

    public void ClearMarkers()
    {
        lock (_lock)
        {
            _markers.Clear();
            _nextMarkerId = 1;
        }
    }

    public void ClearMarkersForField(int fieldId)
    {
        lock (_lock)
        {
            var markersToRemove = _markers.Values
                .Where(m => m.FieldId == fieldId)
                .Select(m => m.Id)
                .ToList();

            foreach (var markerId in markersToRemove)
            {
                if (_markers.TryGetValue(markerId, out var marker))
                {
                    _markers.Remove(markerId);
                    MarkerRemoved?.Invoke(this, new MarkerChangedEventArgs(marker, MarkerChangeType.Removed));
                }
            }
        }
    }

    public List<FieldMarker> GetAllMarkers()
    {
        lock (_lock)
        {
            return _markers.Values.ToList();
        }
    }

    public FieldMarker? GetMarker(int markerId)
    {
        lock (_lock)
        {
            return _markers.TryGetValue(markerId, out var marker) ? marker : null;
        }
    }

    public List<FieldMarker> GetMarkersForField(int fieldId)
    {
        lock (_lock)
        {
            return _markers.Values
                .Where(m => m.FieldId == fieldId)
                .ToList();
        }
    }

    public List<FieldMarker> GetMarkersByType(MarkerType type)
    {
        lock (_lock)
        {
            return _markers.Values
                .Where(m => m.Type == type)
                .ToList();
        }
    }

    public List<FieldMarker> GetMarkersByCategory(string category)
    {
        lock (_lock)
        {
            return _markers.Values
                .Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public List<FieldMarker> GetMarkersNearPosition(Position position, double maxDistanceMeters)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            return _markers.Values
                .Select(m => new
                {
                    Marker = m,
                    Distance = CalculateDistance(position, m.Position)
                })
                .Where(x => x.Distance <= maxDistanceMeters)
                .OrderBy(x => x.Distance)
                .Select(x => x.Marker)
                .ToList();
        }
    }

    public FieldMarker? GetNearestMarker(Position position, double maxDistanceMeters = double.MaxValue)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        lock (_lock)
        {
            if (_markers.Count == 0)
                return null;

            return _markers.Values
                .Select(m => new
                {
                    Marker = m,
                    Distance = CalculateDistance(position, m.Position)
                })
                .Where(x => x.Distance <= maxDistanceMeters)
                .OrderBy(x => x.Distance)
                .FirstOrDefault()?.Marker;
        }
    }

    public bool ToggleMarkerVisibility(int markerId)
    {
        lock (_lock)
        {
            if (!_markers.TryGetValue(markerId, out var marker))
                return false;

            marker.IsVisible = !marker.IsVisible;
            marker.ModifiedDate = DateTime.UtcNow;

            MarkerUpdated?.Invoke(this, new MarkerChangedEventArgs(marker, MarkerChangeType.VisibilityChanged));
            return true;
        }
    }

    public void SetTypeVisibility(MarkerType type, bool visible)
    {
        lock (_lock)
        {
            var markersOfType = _markers.Values
                .Where(m => m.Type == type)
                .ToList();

            foreach (var marker in markersOfType)
            {
                marker.IsVisible = visible;
                marker.ModifiedDate = DateTime.UtcNow;
                MarkerUpdated?.Invoke(this, new MarkerChangedEventArgs(marker, MarkerChangeType.VisibilityChanged));
            }
        }
    }

    public int GetMarkerCount()
    {
        lock (_lock)
        {
            return _markers.Count;
        }
    }

    public int GetMarkerCountByType(MarkerType type)
    {
        lock (_lock)
        {
            return _markers.Values.Count(m => m.Type == type);
        }
    }

    public List<FieldMarker> SearchMarkers(string searchText, bool caseSensitive = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<FieldMarker>();

        lock (_lock)
        {
            var comparison = caseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            return _markers.Values
                .Where(m =>
                    m.Name.Contains(searchText, comparison) ||
                    m.Note.Contains(searchText, comparison) ||
                    m.Category.Contains(searchText, comparison))
                .ToList();
        }
    }

    /// <summary>
    /// Calculate distance between two positions using UTM coordinates
    /// </summary>
    private double CalculateDistance(Position pos1, Position pos2)
    {
        // Simple Pythagorean distance for same zone
        // TODO: Handle cross-zone distance calculations if needed
        double deltaEasting = pos2.Easting - pos1.Easting;
        double deltaNorthing = pos2.Northing - pos1.Northing;
        return Math.Sqrt(deltaEasting * deltaEasting + deltaNorthing * deltaNorthing);
    }
}
