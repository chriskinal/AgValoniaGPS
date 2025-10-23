using System;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents a field flag/marker used to mark points of interest in a field
/// </summary>
public class FieldFlag
{
    /// <summary>
    /// Unique identifier for the flag
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name/label for the flag
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Position of the flag
    /// </summary>
    public Position Position { get; set; } = new Position();

    /// <summary>
    /// Flag color (stored as hex string, e.g., "#FF0000" for red)
    /// </summary>
    public string ColorHex { get; set; } = "#FF0000";

    /// <summary>
    /// Optional notes/description for the flag
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Date when the flag was created
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.Now;
}
