using System;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Represents a marker or flag placed on the field
/// Used for notes, obstacles, waypoints, and other points of interest
/// </summary>
public class FieldMarker
{
    /// <summary>
    /// Unique identifier for this marker
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name or title of the marker
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of marker
    /// </summary>
    public MarkerType Type { get; set; }

    /// <summary>
    /// Position where the marker is placed
    /// </summary>
    public Position Position { get; set; } = new Position();

    /// <summary>
    /// Optional note or description
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Color for rendering this marker (ARGB format)
    /// </summary>
    public uint Color { get; set; } = 0xFFFF0000; // Default: Red

    /// <summary>
    /// Icon or symbol identifier for rendering
    /// </summary>
    public string Icon { get; set; } = "flag";

    /// <summary>
    /// Date and time this marker was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time this marker was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Whether this marker is currently visible
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Field ID this marker belongs to (if any)
    /// </summary>
    public int? FieldId { get; set; }

    /// <summary>
    /// Optional category or group name for organizing markers
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets default color based on marker type
    /// </summary>
    public static uint GetDefaultColorForType(MarkerType type)
    {
        return type switch
        {
            MarkerType.Note => 0xFF0080FF,      // Blue
            MarkerType.Obstacle => 0xFFFF0000,  // Red
            MarkerType.Waypoint => 0xFF00FF00,  // Green
            MarkerType.Flag => 0xFFFFFF00,      // Yellow
            MarkerType.Reference => 0xFFFF00FF, // Magenta
            MarkerType.Warning => 0xFFFFA500,   // Orange
            _ => 0xFFFFFFFF                     // White
        };
    }

    /// <summary>
    /// Gets default icon based on marker type
    /// </summary>
    public static string GetDefaultIconForType(MarkerType type)
    {
        return type switch
        {
            MarkerType.Note => "note",
            MarkerType.Obstacle => "warning_triangle",
            MarkerType.Waypoint => "location_pin",
            MarkerType.Flag => "flag",
            MarkerType.Reference => "crosshair",
            MarkerType.Warning => "exclamation",
            _ => "marker"
        };
    }

    /// <summary>
    /// Creates a marker with default settings for the given type
    /// </summary>
    public static FieldMarker CreateDefault(MarkerType type, Position position, string name = "")
    {
        return new FieldMarker
        {
            Type = type,
            Position = position,
            Name = string.IsNullOrEmpty(name) ? $"{type} Marker" : name,
            Color = GetDefaultColorForType(type),
            Icon = GetDefaultIconForType(type)
        };
    }
}
