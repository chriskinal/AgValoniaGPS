using System;
using System.Collections.Generic;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Guidance;
using Position = AgValoniaGPS.Models.Position;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for recording, smoothing, and calculating guidance from curved paths.
/// Supports real-time recording, cubic spline smoothing, and efficient closest-point search.
/// </summary>
public interface ICurveLineService
{
    /// <summary>
    /// Event fired when curve recording state changes or curve is modified.
    /// </summary>
    event EventHandler<CurveLineChangedEventArgs>? CurveChanged;

    /// <summary>
    /// Start recording a new curved path from initial position.
    /// </summary>
    /// <param name="startPosition">Initial position to begin recording</param>
    /// <remarks>
    /// Initializes empty point list and sets recording state.
    /// Recording continues until FinishRecording is called.
    /// </remarks>
    void StartRecording(Position startPosition);

    /// <summary>
    /// Add a point to the curve being recorded.
    /// </summary>
    /// <param name="point">Position to add to curve</param>
    /// <param name="minDistanceMeters">Minimum distance from last point to add new point (meters)</param>
    /// <remarks>
    /// Point is only added if distance from last recorded point exceeds minDistanceMeters.
    /// Typical minimum distance is 0.5-2.0 meters to avoid excessive point density.
    /// </remarks>
    void AddPoint(Position point, double minDistanceMeters);

    /// <summary>
    /// Finish recording and create final CurveLine.
    /// </summary>
    /// <param name="name">Name for the recorded curve</param>
    /// <returns>CurveLine with recorded points (minimum 3 points required)</returns>
    /// <exception cref="InvalidOperationException">Thrown if fewer than 3 points recorded</exception>
    /// <remarks>
    /// Validates that curve has sufficient points (>=3) before returning.
    /// Stops recording and emits CurveChanged event.
    /// </remarks>
    CurveLine FinishRecording(string name);

    /// <summary>
    /// Calculate guidance from vehicle position to curve.
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="currentHeading">Current vehicle heading in radians</param>
    /// <param name="curve">Curve to calculate guidance from</param>
    /// <param name="findGlobal">If true, search entire curve; if false, use local search from last index</param>
    /// <returns>GuidanceLineResult with cross-track error, closest point, and heading error</returns>
    /// <remarks>
    /// Local search (findGlobal=false) is optimized for sequential calls when vehicle follows curve.
    /// Global search (findGlobal=true) is slower but accurate for initial approach to curve.
    /// Execution time target: <5ms for 20-25 Hz operation.
    /// </remarks>
    GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, CurveLine curve, bool findGlobal = false);

    /// <summary>
    /// Find closest point on curve to given position.
    /// </summary>
    /// <param name="currentPosition">Position to find closest point from</param>
    /// <param name="curve">Curve to search</param>
    /// <param name="searchStartIndex">Index to start local search from (-1 for global search)</param>
    /// <returns>Closest position on curve with index information</returns>
    /// <remarks>
    /// Use searchStartIndex from previous call for efficient local search.
    /// Returns both position and index for use in subsequent calls.
    /// </remarks>
    ClosestPointResult GetClosestPoint(Position currentPosition, CurveLine curve, int searchStartIndex = -1);

    /// <summary>
    /// Get heading (tangent direction) at specific point on curve.
    /// </summary>
    /// <param name="point">Position on or near curve</param>
    /// <param name="curve">Curve to calculate heading from</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    /// <remarks>
    /// Calculates tangent vector between nearest curve points.
    /// Used for heading error calculation and steering guidance.
    /// </remarks>
    double GetHeadingAtPoint(Position point, CurveLine curve);

    /// <summary>
    /// Validate curve quality and parameters.
    /// </summary>
    /// <param name="curve">Curve to validate</param>
    /// <returns>ValidationResult with errors and warnings</returns>
    /// <remarks>
    /// Checks: minimum points (>=3), point spacing (0.1m-100m), no duplicates, smoothness.
    /// </remarks>
    ValidationResult ValidateCurve(CurveLine curve);

    /// <summary>
    /// Gets whether curve recording is currently active.
    /// </summary>
    bool IsRecording { get; }
}
