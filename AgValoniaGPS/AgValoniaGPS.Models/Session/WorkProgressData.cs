using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models.Session;

/// <summary>
/// Represents work progress data for the current session including area coverage,
/// distance traveled, and section states.
/// </summary>
public class WorkProgressData
{
    /// <summary>
    /// Gets or sets the total area covered in square meters.
    /// </summary>
    public double AreaCovered { get; set; }

    /// <summary>
    /// Gets or sets the total distance traveled in meters.
    /// </summary>
    public double DistanceTravelled { get; set; }

    /// <summary>
    /// Gets or sets the total time spent working (sections active).
    /// </summary>
    public TimeSpan TimeWorked { get; set; }

    /// <summary>
    /// Gets or sets the coverage trail of positions for coverage map visualization.
    /// Limited to recent positions to prevent excessive memory usage.
    /// </summary>
    public List<Position> CoverageTrail { get; set; } = new();

    /// <summary>
    /// Gets or sets the current state of each section (true = on, false = off).
    /// Array length matches the number of sections configured.
    /// </summary>
    public bool[] SectionStates { get; set; } = Array.Empty<bool>();
}
