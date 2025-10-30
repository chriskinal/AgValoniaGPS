using AgValoniaGPS.Models.FieldOperations;
using System;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for recording vehicle paths and converting them to guidance lines.
/// </summary>
public interface IPathRecordingService
{
    /// <summary>
    /// Raised when the recording state changes.
    /// </summary>
    event EventHandler<RecordingStateChangedEventArgs>? RecordingStateChanged;

    /// <summary>
    /// Raised when a new point is added to the recording.
    /// </summary>
    event EventHandler<PathPointRecordedEventArgs>? PathPointRecorded;

    /// <summary>
    /// Current recording state.
    /// </summary>
    RecordingState CurrentState { get; }

    /// <summary>
    /// The path currently being recorded (or last recorded path).
    /// </summary>
    RecordedPath? CurrentPath { get; }

    /// <summary>
    /// Number of points in the current recording.
    /// </summary>
    int PointCount { get; }

    /// <summary>
    /// Starts recording a new path.
    /// </summary>
    /// <param name="pathName">Name for the new path</param>
    void StartRecording(string pathName);

    /// <summary>
    /// Stops recording and finalizes the current path.
    /// </summary>
    /// <returns>The completed recorded path</returns>
    RecordedPath? StopRecording();

    /// <summary>
    /// Pauses the current recording (can be resumed).
    /// </summary>
    void PauseRecording();

    /// <summary>
    /// Resumes a paused recording.
    /// </summary>
    void ResumeRecording();

    /// <summary>
    /// Records a point along the path.
    /// </summary>
    /// <param name="easting">Easting coordinate</param>
    /// <param name="northing">Northing coordinate</param>
    /// <param name="heading">Heading in radians</param>
    /// <param name="speed">Speed in m/s</param>
    /// <param name="isSectionAutoOn">Section control auto state</param>
    void RecordPoint(double easting, double northing, double heading, double speed, bool isSectionAutoOn);

    /// <summary>
    /// Clears the current recording without saving.
    /// </summary>
    void ClearRecording();

    /// <summary>
    /// Smooths a recorded path using the Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="path">Path to smooth</param>
    /// <param name="tolerance">Smoothing tolerance in meters (default 0.5m)</param>
    /// <returns>Smoothed path</returns>
    RecordedPath SmoothPath(RecordedPath path, double tolerance = 0.5);

    /// <summary>
    /// Filters a path by minimum distance between points.
    /// </summary>
    /// <param name="path">Path to filter</param>
    /// <param name="minDistance">Minimum distance between points in meters</param>
    /// <returns>Filtered path</returns>
    RecordedPath FilterPathByDistance(RecordedPath path, double minDistance);
}

/// <summary>
/// Event args for recording state changes.
/// </summary>
public class RecordingStateChangedEventArgs : EventArgs
{
    public RecordingStateChangedEventArgs(RecordingState oldState, RecordingState newState)
    {
        OldState = oldState;
        NewState = newState;
    }

    public RecordingState OldState { get; }
    public RecordingState NewState { get; }
}

/// <summary>
/// Event args for when a path point is recorded.
/// </summary>
public class PathPointRecordedEventArgs : EventArgs
{
    public PathPointRecordedEventArgs(RecordedPathPoint point, int pointIndex)
    {
        Point = point;
        PointIndex = pointIndex;
    }

    public RecordedPathPoint Point { get; }
    public int PointIndex { get; }
}
