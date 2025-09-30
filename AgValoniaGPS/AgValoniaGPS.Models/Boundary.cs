using System.Collections.Generic;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents a field boundary consisting of a series of points
/// </summary>
public class Boundary
{
    public string Name { get; set; } = string.Empty;

    public List<Position> Points { get; set; } = new();

    /// <summary>
    /// Area in hectares
    /// </summary>
    public double Area { get; set; }

    /// <summary>
    /// Whether this boundary is turned on for display/guidance
    /// </summary>
    public bool IsActive { get; set; }
}