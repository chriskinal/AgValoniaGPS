using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents an agricultural field with boundaries, AB lines, and metadata
/// </summary>
public class Field
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<Boundary> Boundaries { get; set; } = new();

    public List<ABLine> ABLines { get; set; } = new();

    /// <summary>
    /// Total area in hectares
    /// </summary>
    public double TotalArea { get; set; }

    /// <summary>
    /// Area worked in hectares
    /// </summary>
    public double WorkedArea { get; set; }

    /// <summary>
    /// Date when field was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Date when field was last modified
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Center position of the field (calculated from boundaries)
    /// </summary>
    public Position? CenterPosition { get; set; }
}