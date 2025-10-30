using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Manages a collection of guidance tracks with auto-switching, nudging, and worked track tracking.
/// Thread-safe for concurrent access.
/// </summary>
public class TrackManagementService : ITrackManagementService
{
    private readonly object _lock = new object();
    private readonly Dictionary<int, GuidanceTrack> _tracks = new();
    private readonly IABLineService _abLineService;
    private readonly ICurveLineService _curveLineService;
    private readonly IContourService _contourService;
    private readonly IHeadlineService _headlineService;

    private int _nextTrackId = 1;
    private int? _activeTrackId = null;

    public event EventHandler<TrackChangedEventArgs>? ActiveTrackChanged;
    public event EventHandler<TrackChangedEventArgs>? TrackAdded;
    public event EventHandler<TrackChangedEventArgs>? TrackRemoved;

    public TrackManagementService(
        IABLineService abLineService,
        ICurveLineService curveLineService,
        IContourService contourService,
        IHeadlineService headlineService)
    {
        _abLineService = abLineService ?? throw new ArgumentNullException(nameof(abLineService));
        _curveLineService = curveLineService ?? throw new ArgumentNullException(nameof(curveLineService));
        _contourService = contourService ?? throw new ArgumentNullException(nameof(contourService));
        _headlineService = headlineService ?? throw new ArgumentNullException(nameof(headlineService));
    }

    public GuidanceTrack AddTrack(ABLine abLine)
    {
        if (abLine == null)
            throw new ArgumentNullException(nameof(abLine));

        lock (_lock)
        {
            var track = GuidanceTrack.FromABLine(_nextTrackId++, abLine);
            _tracks[track.Id] = track;

            TrackAdded?.Invoke(this, new TrackChangedEventArgs(track, TrackChangeType.Added));
            return track;
        }
    }

    public GuidanceTrack AddTrack(CurveLine curveLine)
    {
        if (curveLine == null)
            throw new ArgumentNullException(nameof(curveLine));

        lock (_lock)
        {
            var track = GuidanceTrack.FromCurveLine(_nextTrackId++, curveLine);
            _tracks[track.Id] = track;

            TrackAdded?.Invoke(this, new TrackChangedEventArgs(track, TrackChangeType.Added));
            return track;
        }
    }

    public GuidanceTrack AddTrack(ContourLine contourLine)
    {
        if (contourLine == null)
            throw new ArgumentNullException(nameof(contourLine));

        lock (_lock)
        {
            var track = GuidanceTrack.FromContourLine(_nextTrackId++, contourLine);
            _tracks[track.Id] = track;

            TrackAdded?.Invoke(this, new TrackChangedEventArgs(track, TrackChangeType.Added));
            return track;
        }
    }

    public GuidanceTrack AddTrack(Headline headline)
    {
        if (headline == null)
            throw new ArgumentNullException(nameof(headline));

        lock (_lock)
        {
            var track = GuidanceTrack.FromHeadline(_nextTrackId++, headline);
            _tracks[track.Id] = track;

            TrackAdded?.Invoke(this, new TrackChangedEventArgs(track, TrackChangeType.Added));
            return track;
        }
    }

    public bool RemoveTrack(int trackId)
    {
        lock (_lock)
        {
            if (!_tracks.TryGetValue(trackId, out var track))
                return false;

            // Deactivate if this was the active track
            if (_activeTrackId == trackId)
            {
                _activeTrackId = null;
                track.IsActive = false;
                ActiveTrackChanged?.Invoke(this, new TrackChangedEventArgs(track, TrackChangeType.Deactivated));
            }

            _tracks.Remove(trackId);
            TrackRemoved?.Invoke(this, new TrackChangedEventArgs(track, TrackChangeType.Removed));
            return true;
        }
    }

    public void ClearTracks()
    {
        lock (_lock)
        {
            var activeTrack = GetActiveTrack();
            if (activeTrack != null)
            {
                activeTrack.IsActive = false;
                ActiveTrackChanged?.Invoke(this, new TrackChangedEventArgs(activeTrack, TrackChangeType.Deactivated));
            }

            _tracks.Clear();
            _activeTrackId = null;
            _nextTrackId = 1;
        }
    }

    public List<GuidanceTrack> GetAllTracks()
    {
        lock (_lock)
        {
            return _tracks.Values.ToList();
        }
    }

    public GuidanceTrack? GetTrack(int trackId)
    {
        lock (_lock)
        {
            return _tracks.TryGetValue(trackId, out var track) ? track : null;
        }
    }

    public GuidanceTrack? GetActiveTrack()
    {
        lock (_lock)
        {
            if (_activeTrackId == null)
                return null;

            return _tracks.TryGetValue(_activeTrackId.Value, out var track) ? track : null;
        }
    }

    public bool SetActiveTrack(int trackId)
    {
        lock (_lock)
        {
            if (!_tracks.ContainsKey(trackId))
                return false;

            // Deactivate current track
            if (_activeTrackId != null && _tracks.TryGetValue(_activeTrackId.Value, out var currentTrack))
            {
                currentTrack.IsActive = false;
                ActiveTrackChanged?.Invoke(this, new TrackChangedEventArgs(currentTrack, TrackChangeType.Deactivated));
            }

            // Activate new track
            var newTrack = _tracks[trackId];
            newTrack.IsActive = true;
            newTrack.LastUsedDate = DateTime.UtcNow;
            _activeTrackId = trackId;

            ActiveTrackChanged?.Invoke(this, new TrackChangedEventArgs(newTrack, TrackChangeType.Activated));
            return true;
        }
    }

    public GuidanceTrack? SwitchToClosestTrack(Position currentPosition, double maxDistance = 50.0)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lock)
        {
            if (_tracks.Count == 0)
                return null;

            // Update all track distances
            UpdateTrackDistancesInternal(currentPosition);

            // Find closest track within maxDistance
            var closestTrack = _tracks.Values
                .Where(t => t.DistanceFromVehicle <= maxDistance)
                .OrderBy(t => t.DistanceFromVehicle)
                .FirstOrDefault();

            if (closestTrack != null)
            {
                SetActiveTrack(closestTrack.Id);
                return closestTrack;
            }

            return null;
        }
    }

    public GuidanceTrack? CycleTrack(bool forward = true)
    {
        lock (_lock)
        {
            if (_tracks.Count == 0)
                return null;

            var trackList = _tracks.Values.OrderBy(t => t.Id).ToList();

            if (_activeTrackId == null)
            {
                // No active track, activate first or last
                var track = forward ? trackList.First() : trackList.Last();
                SetActiveTrack(track.Id);
                return track;
            }

            // Find current index
            var currentIndex = trackList.FindIndex(t => t.Id == _activeTrackId.Value);
            if (currentIndex == -1)
            {
                // Active track not found, activate first
                SetActiveTrack(trackList.First().Id);
                return trackList.First();
            }

            // Calculate next index
            int nextIndex;
            if (forward)
            {
                nextIndex = (currentIndex + 1) % trackList.Count;
            }
            else
            {
                nextIndex = currentIndex - 1;
                if (nextIndex < 0)
                    nextIndex = trackList.Count - 1;
            }

            var nextTrack = trackList[nextIndex];
            SetActiveTrack(nextTrack.Id);
            return nextTrack;
        }
    }

    public bool NudgeActiveTrack(double offsetMeters)
    {
        lock (_lock)
        {
            var activeTrack = GetActiveTrack();
            if (activeTrack == null)
                return false;

            // Apply nudge based on track mode
            switch (activeTrack.Mode)
            {
                case TrackMode.ABLine:
                    if (activeTrack.ABLine != null)
                    {
                        var nudgedLine = _abLineService.NudgeLine(activeTrack.ABLine, offsetMeters);
                        activeTrack.ABLine = nudgedLine;
                        activeTrack.NudgeOffset += offsetMeters;
                    }
                    break;

                case TrackMode.CurveLine:
                    // TODO: Implement curve nudging - requires offsetting each point perpendicular to local tangent
                    // This is more complex than straight line nudging
                    return false;

                case TrackMode.Contour:
                    // Contour lines typically don't support nudging
                    return false;

                case TrackMode.Headline:
                    if (activeTrack.Headline != null)
                    {
                        var nudgedHeadline = _headlineService.MoveHeadline(activeTrack.Headline.Id, offsetMeters);
                        activeTrack.Headline = nudgedHeadline;
                        activeTrack.NudgeOffset += offsetMeters;
                    }
                    break;

                default:
                    return false;
            }

            ActiveTrackChanged?.Invoke(this, new TrackChangedEventArgs(activeTrack, TrackChangeType.Nudged));
            return true;
        }
    }

    public bool SetTrackWorked(int trackId, bool worked)
    {
        lock (_lock)
        {
            if (!_tracks.TryGetValue(trackId, out var track))
                return false;

            track.IsWorked = worked;

            var changeType = worked ? TrackChangeType.MarkedWorked : TrackChangeType.MarkedNotWorked;
            ActiveTrackChanged?.Invoke(this, new TrackChangedEventArgs(track, changeType));
            return true;
        }
    }

    public HashSet<int> GetWorkedTrackIds()
    {
        lock (_lock)
        {
            return _tracks.Values
                .Where(t => t.IsWorked)
                .Select(t => t.Id)
                .ToHashSet();
        }
    }

    public GuidanceTrack? FindNextUnworkedTrack(Position currentPosition)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lock)
        {
            // Update distances
            UpdateTrackDistancesInternal(currentPosition);

            // Find closest unworked track
            var nextTrack = _tracks.Values
                .Where(t => !t.IsWorked)
                .OrderBy(t => t.DistanceFromVehicle)
                .FirstOrDefault();

            if (nextTrack != null)
            {
                SetActiveTrack(nextTrack.Id);
            }

            return nextTrack;
        }
    }

    public List<GuidanceTrack> GetTracksByMode(TrackMode mode)
    {
        lock (_lock)
        {
            return _tracks.Values
                .Where(t => t.Mode == mode)
                .ToList();
        }
    }

    public int GetTrackCount()
    {
        lock (_lock)
        {
            return _tracks.Count;
        }
    }

    public void UpdateTrackDistances(Position currentPosition)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lock)
        {
            UpdateTrackDistancesInternal(currentPosition);
        }
    }

    /// <summary>
    /// Internal method to update track distances (assumes lock is held)
    /// </summary>
    private void UpdateTrackDistancesInternal(Position currentPosition)
    {
        foreach (var track in _tracks.Values)
        {
            track.DistanceFromVehicle = CalculateTrackDistance(track, currentPosition);
        }
    }

    /// <summary>
    /// Calculate perpendicular distance from position to track
    /// </summary>
    private double CalculateTrackDistance(GuidanceTrack track, Position currentPosition)
    {
        switch (track.Mode)
        {
            case TrackMode.ABLine:
                if (track.ABLine != null)
                {
                    var result = _abLineService.CalculateGuidance(currentPosition, currentPosition.Heading, track.ABLine);
                    return Math.Abs(result.CrossTrackError);
                }
                break;

            case TrackMode.CurveLine:
                if (track.CurveLine != null)
                {
                    var result = _curveLineService.CalculateGuidance(currentPosition, currentPosition.Heading, track.CurveLine);
                    return Math.Abs(result.CrossTrackError);
                }
                break;

            case TrackMode.Contour:
                if (track.ContourLine != null)
                {
                    var result = _contourService.CalculateGuidance(currentPosition, currentPosition.Heading, track.ContourLine);
                    return Math.Abs(result.CrossTrackError);
                }
                break;

            case TrackMode.Headline:
                if (track.Headline != null)
                {
                    var closestPoint = _headlineService.GetClosestPoint(currentPosition, track.Headline);
                    return closestPoint.Distance;
                }
                break;
        }

        return double.MaxValue;
    }
}
