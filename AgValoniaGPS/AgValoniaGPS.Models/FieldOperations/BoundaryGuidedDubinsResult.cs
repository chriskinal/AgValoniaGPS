using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Result from boundary-guided Dubins path generation.
/// Contains the path and metadata about how it was generated.
/// </summary>
public class BoundaryGuidedDubinsResult
{
    public BoundaryGuidedDubinsResult(bool succeeded)
    {
        Succeeded = succeeded;
        IntermediateWaypoints = new List<Position2D>();
        DubinsSegments = new List<DubinsPath>();
    }

    /// <summary>
    /// Whether a valid path was found.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// The complete path as a single DubinsPath (if succeeded).
    /// </summary>
    public DubinsPath? ResultPath { get; set; }

    /// <summary>
    /// Intermediate waypoints sampled to avoid boundaries.
    /// Empty for standard Dubins paths.
    /// </summary>
    public List<Position2D> IntermediateWaypoints { get; set; }

    /// <summary>
    /// Individual Dubins segments connecting waypoints.
    /// </summary>
    public List<DubinsPath> DubinsSegments { get; set; }

    /// <summary>
    /// Number of sampling iterations performed.
    /// 0 = standard Dubins worked
    /// 1-10 = guided sampling iterations
    /// </summary>
    public int IterationCount { get; set; }

    /// <summary>
    /// Time taken to compute this path.
    /// </summary>
    public TimeSpan ComputationTime { get; set; }

    /// <summary>
    /// Strategy that succeeded.
    /// </summary>
    public PathGenerationStrategy Strategy { get; set; }

    /// <summary>
    /// Minimum distance to boundary along the path.
    /// </summary>
    public double MinBoundaryDistance { get; set; }

    /// <summary>
    /// Whether this path required boundary-guided sampling.
    /// </summary>
    public bool UsedGuidedSampling => IterationCount > 0;
}

/// <summary>
/// Strategy used to generate the path.
/// </summary>
public enum PathGenerationStrategy
{
    /// <summary>
    /// Standard Dubins path (6 types, pick shortest).
    /// </summary>
    StandardDubins,

    /// <summary>
    /// Boundary-guided sampling with intermediate waypoints.
    /// </summary>
    GuidedSampling,

    /// <summary>
    /// Fallback K-turn with reverse segments.
    /// </summary>
    FallbackKTurn
}
