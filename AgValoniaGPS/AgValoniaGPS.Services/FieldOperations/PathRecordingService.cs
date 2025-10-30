using AgValoniaGPS.Models.FieldOperations;
using System;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implementation of path recording service.
/// Records vehicle paths for later playback or conversion to guidance lines.
/// </summary>
public class PathRecordingService : IPathRecordingService
{
    private RecordingState _currentState = RecordingState.Stopped;
    private RecordedPath? _currentPath;

    public event EventHandler<RecordingStateChangedEventArgs>? RecordingStateChanged;
    public event EventHandler<PathPointRecordedEventArgs>? PathPointRecorded;

    public RecordingState CurrentState => _currentState;
    public RecordedPath? CurrentPath => _currentPath;
    public int PointCount => _currentPath?.PointCount ?? 0;

    public void StartRecording(string pathName)
    {
        if (string.IsNullOrWhiteSpace(pathName))
        {
            throw new ArgumentException("Path name cannot be empty", nameof(pathName));
        }

        if (_currentState != RecordingState.Stopped)
        {
            throw new InvalidOperationException($"Cannot start recording while in state: {_currentState}");
        }

        _currentPath = new RecordedPath(pathName);
        SetState(RecordingState.Recording);
    }

    public RecordedPath? StopRecording()
    {
        if (_currentState == RecordingState.Stopped)
        {
            return null;
        }

        var completedPath = _currentPath;
        SetState(RecordingState.Stopped);

        return completedPath;
    }

    public void PauseRecording()
    {
        if (_currentState != RecordingState.Recording)
        {
            throw new InvalidOperationException($"Cannot pause recording while in state: {_currentState}");
        }

        SetState(RecordingState.Paused);
    }

    public void ResumeRecording()
    {
        if (_currentState != RecordingState.Paused)
        {
            throw new InvalidOperationException($"Cannot resume recording while in state: {_currentState}");
        }

        SetState(RecordingState.Recording);
    }

    public void RecordPoint(double easting, double northing, double heading, double speed, bool isSectionAutoOn)
    {
        if (_currentState != RecordingState.Recording)
        {
            return; // Silently ignore points when not actively recording
        }

        if (_currentPath == null)
        {
            throw new InvalidOperationException("Current path is null while recording");
        }

        var point = new RecordedPathPoint(easting, northing, heading, speed, isSectionAutoOn);
        _currentPath.AddPoint(point);

        PathPointRecorded?.Invoke(this, new PathPointRecordedEventArgs(point, _currentPath.PointCount - 1));
    }

    public void ClearRecording()
    {
        var oldState = _currentState;
        _currentPath = null;
        SetState(RecordingState.Stopped);
    }

    public RecordedPath SmoothPath(RecordedPath path, double tolerance = 0.5)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentException("Tolerance must be positive", nameof(tolerance));
        }

        if (path.Points.Count < 3)
        {
            // No smoothing needed for very short paths
            return path;
        }

        var smoothedPoints = PathSmoothingUtility.DouglasPeucker(path.Points, tolerance);

        var smoothedPath = new RecordedPath(path.Name + " (Smoothed)")
        {
            Points = smoothedPoints,
            RecordedAt = path.RecordedAt
        };

        return smoothedPath;
    }

    public RecordedPath FilterPathByDistance(RecordedPath path, double minDistance)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (minDistance <= 0)
        {
            throw new ArgumentException("Min distance must be positive", nameof(minDistance));
        }

        if (path.Points.Count < 2)
        {
            return path;
        }

        var filteredPoints = PathSmoothingUtility.FilterByDistance(path.Points, minDistance);

        var filteredPath = new RecordedPath(path.Name + " (Filtered)")
        {
            Points = filteredPoints,
            RecordedAt = path.RecordedAt
        };

        return filteredPath;
    }

    private void SetState(RecordingState newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        var oldState = _currentState;
        _currentState = newState;

        RecordingStateChanged?.Invoke(this, new RecordingStateChangedEventArgs(oldState, newState));
    }
}
