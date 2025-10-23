using System;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents metadata information about a field for directory listings and selection dialogs
/// </summary>
public class FieldInfo
{
    /// <summary>
    /// Field name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full path to the field directory
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Field area in hectares
    /// </summary>
    public double AreaHectares { get; set; }

    /// <summary>
    /// Date when field was last modified
    /// </summary>
    public DateTime DateModified { get; set; }

    /// <summary>
    /// Date when field was created
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Number of boundary points in the field
    /// </summary>
    public int BoundaryPointCount { get; set; }

    /// <summary>
    /// Whether the field has a valid boundary
    /// </summary>
    public bool HasBoundary { get; set; }
}
