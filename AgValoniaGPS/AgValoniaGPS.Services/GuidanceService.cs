using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services;

/// <summary>
/// Implementation of guidance calculation service
/// </summary>
public class GuidanceService : IGuidanceService
{
    public event EventHandler<GuidanceData>? GuidanceUpdated;

    public double CrossTrackError { get; private set; }

    public double LookaheadDistance { get; private set; }

    public bool IsActive { get; private set; }

    public void CalculateGuidance(Position currentPosition, ABLine abLine, Vehicle vehicle)
    {
        if (!IsActive)
            return;

        // TODO: Implement pure pursuit algorithm for guidance calculations
        // This will calculate cross track error and lookahead point

        var guidanceData = new GuidanceData
        {
            CrossTrackError = CrossTrackError,
            LookaheadDistance = LookaheadDistance,
            SteerAngle = 0.0,
            IsOnLine = Math.Abs(CrossTrackError) < 0.1 // Within 10cm
        };

        GuidanceUpdated?.Invoke(this, guidanceData);
    }

    public void Start()
    {
        IsActive = true;
    }

    public void Stop()
    {
        IsActive = false;
    }
}