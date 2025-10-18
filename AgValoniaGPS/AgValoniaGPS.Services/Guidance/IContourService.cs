using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service interface for Contour guidance operations.
/// Provides functionality for recording, locking, and calculating guidance from contour lines.
/// </summary>
public interface IContourService
{
    /// <summary>
    /// Event fired when contour state changes (recording started, point added, locked, unlocked)
    /// </summary>
    event EventHandler<ContourStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Gets whether a contour is currently being recorded
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Gets whether the current contour is locked for guidance use
    /// </summary>
    bool IsLocked { get; }

    /// <summary>
    /// Start recording a new contour line from a starting position
    /// </summary>
    /// <param name="startPosition">Initial position for the contour</param>
    /// <param name="minDistanceMeters">Minimum distance threshold in meters for adding points (runtime configurable)</param>
    /// <exception cref="InvalidOperationException">Thrown if already recording</exception>
    /// <exception cref="ArgumentNullException">Thrown if startPosition is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if minDistanceMeters is negative</exception>
    void StartRecording(Position startPosition, double minDistanceMeters);

    /// <summary>
    /// Add a new point to the recording contour with offset applied
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="offset">Perpendicular offset to apply to the recorded position in meters</param>
    /// <remarks>
    /// Point will only be added if distance from last point exceeds minimum distance threshold.
    /// Offset is applied perpendicular to the vehicle's heading.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if not currently recording</exception>
    /// <exception cref="ArgumentNullException">Thrown if currentPosition is null</exception>
    void AddPoint(Position currentPosition, double offset);

    /// <summary>
    /// Lock the current contour for guidance use
    /// </summary>
    /// <param name="name">Name for the contour line</param>
    /// <returns>Locked ContourLine ready for guidance</returns>
    /// <exception cref="InvalidOperationException">Thrown if not recording or already locked</exception>
    /// <exception cref="ArgumentException">Thrown if contour has insufficient points (less than 10)</exception>
    ContourLine LockContour(string name);

    /// <summary>
    /// Stop recording the current contour without locking it
    /// </summary>
    /// <remarks>
    /// This clears the recording state without creating a usable contour.
    /// Use LockContour to finalize the contour for guidance use.
    /// </remarks>
    void StopRecording();

    /// <summary>
    /// Calculate guidance information from a contour line
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="currentHeading">Current vehicle heading in degrees</param>
    /// <param name="contour">Contour line to calculate guidance from</param>
    /// <returns>Guidance calculation results with cross-track error and look-ahead point</returns>
    /// <exception cref="ArgumentNullException">Thrown if parameters are null</exception>
    /// <exception cref="ArgumentException">Thrown if contour is not locked or invalid</exception>
    /// <remarks>
    /// Execution time must be less than 5ms for 20-25 Hz operation.
    /// Uses look-ahead smoothing for stable following behavior.
    /// </remarks>
    GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, ContourLine contour);

    /// <summary>
    /// Calculate the perpendicular offset from current position to contour path
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="contour">Contour line to calculate offset from</param>
    /// <returns>Signed offset in meters (positive = right, negative = left)</returns>
    /// <exception cref="ArgumentNullException">Thrown if parameters are null</exception>
    double CalculateOffset(Position currentPosition, ContourLine contour);

    /// <summary>
    /// Set the locked state of the current contour
    /// </summary>
    /// <param name="locked">True to lock, false to unlock</param>
    /// <remarks>
    /// This allows toggling the lock state without creating a new contour.
    /// </remarks>
    void SetLocked(bool locked);

    /// <summary>
    /// Validate a contour line for correctness
    /// </summary>
    /// <param name="contour">Contour line to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    /// <exception cref="ArgumentNullException">Thrown if contour is null</exception>
    ValidationResult ValidateContour(ContourLine contour);
}
