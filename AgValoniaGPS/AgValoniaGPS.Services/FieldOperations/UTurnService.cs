using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implements U-turn pattern generation and execution for field operations.
/// Supports Omega (Dubins path), T-turn, and Y-turn patterns with automatic section control.
/// Thread-safe implementation using lock object pattern.
/// </summary>
public class UTurnService : IUTurnService
{
    private readonly object _lockObject = new object();

    // Turn configuration
    private UTurnType _turnType = UTurnType.Omega;
    private double _turningRadius = 5.0; // meters
    private bool _autoPause = true;

    // Turn state
    private bool _isInTurn = false;
    private Position[]? _currentTurnPath = null;
    private Position? _turnStartPosition = null;
    private DateTime _turnStartTime;
    private double _turnProgress = 0.0;
    private int _currentWaypointIndex = 0;

    public event EventHandler<UTurnStartedEventArgs>? UTurnStarted;
    public event EventHandler<UTurnCompletedEventArgs>? UTurnCompleted;

    public void ConfigureTurn(UTurnType turnType, double turningRadius, bool autoPause)
    {
        lock (_lockObject)
        {
            if (turningRadius <= 0)
                throw new ArgumentOutOfRangeException(nameof(turningRadius), "Turning radius must be positive");

            _turnType = turnType;
            _turningRadius = turningRadius;
            _autoPause = autoPause;
        }
    }

    public Position[] GenerateTurnPath(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading)
    {
        if (entryPoint == null)
            throw new ArgumentNullException(nameof(entryPoint));
        if (exitPoint == null)
            throw new ArgumentNullException(nameof(exitPoint));

        lock (_lockObject)
        {
            return _turnType switch
            {
                UTurnType.Omega => GenerateOmegaTurn(entryPoint, entryHeading, exitPoint, exitHeading),
                UTurnType.T => GenerateTTurn(entryPoint, entryHeading, exitPoint, exitHeading),
                UTurnType.Y => GenerateYTurn(entryPoint, entryHeading, exitPoint, exitHeading),
                _ => throw new ArgumentException($"Unknown turn type: {_turnType}")
            };
        }
    }

    public void StartTurn(Position currentPosition, double currentHeading)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lockObject)
        {
            // Calculate exit point (simple approach: assume 180° turn to parallel path)
            double exitHeading = NormalizeHeading(currentHeading + 180.0);
            Position exitPoint = CalculateExitPoint(currentPosition, currentHeading, exitHeading);

            // Generate turn path
            _currentTurnPath = GenerateTurnPath(currentPosition, currentHeading, exitPoint, exitHeading);
            _turnStartPosition = currentPosition;
            _turnStartTime = DateTime.UtcNow;
            _isInTurn = true;
            _turnProgress = 0.0;
            _currentWaypointIndex = 0;

            // Raise event
            UTurnStarted?.Invoke(this, new UTurnStartedEventArgs(_turnType, currentPosition, _currentTurnPath));
        }
    }

    public void UpdateTurnProgress(Position currentPosition)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lockObject)
        {
            if (!_isInTurn || _currentTurnPath == null || _currentTurnPath.Length == 0)
                return;

            // Find closest waypoint to current position
            double minDistance = double.MaxValue;
            int closestIndex = _currentWaypointIndex;

            for (int i = _currentWaypointIndex; i < _currentTurnPath.Length; i++)
            {
                double distance = CalculateDistance(currentPosition, _currentTurnPath[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            _currentWaypointIndex = closestIndex;

            // Calculate progress as ratio of waypoints completed
            _turnProgress = (double)_currentWaypointIndex / (_currentTurnPath.Length - 1);
            _turnProgress = Math.Clamp(_turnProgress, 0.0, 1.0);
        }
    }

    public void CompleteTurn()
    {
        lock (_lockObject)
        {
            if (!_isInTurn)
                return;

            double turnDuration = (DateTime.UtcNow - _turnStartTime).TotalSeconds;
            Position endPosition = _currentTurnPath != null && _currentTurnPath.Length > 0
                ? _currentTurnPath[^1]
                : _turnStartPosition ?? new Position();

            _isInTurn = false;
            _turnProgress = 1.0;

            // Raise event
            UTurnCompleted?.Invoke(this, new UTurnCompletedEventArgs(_turnType, endPosition, turnDuration));

            // Reset state
            _currentTurnPath = null;
            _turnStartPosition = null;
            _currentWaypointIndex = 0;
        }
    }

    public bool IsInTurn()
    {
        lock (_lockObject)
        {
            return _isInTurn;
        }
    }

    public UTurnType GetCurrentTurnType()
    {
        lock (_lockObject)
        {
            return _turnType;
        }
    }

    public Position[]? GetCurrentTurnPath()
    {
        lock (_lockObject)
        {
            return _currentTurnPath;
        }
    }

    public double GetTurnProgress()
    {
        lock (_lockObject)
        {
            return _turnProgress;
        }
    }

    // ==================== Turn Pattern Generation ====================

    /// <summary>
    /// Generates an Omega (Ω) turn using a simplified Dubins path algorithm.
    /// Creates a smooth circular arc for 180° turn with minimum turning radius.
    /// </summary>
    private Position[] GenerateOmegaTurn(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading)
    {
        var waypoints = new List<Position>();

        // Calculate turn center point (midpoint perpendicular to entry heading)
        double entryHeadingRad = entryHeading * Math.PI / 180.0;
        double perpHeadingRad = (entryHeading + 90.0) * Math.PI / 180.0;

        // Center is at turning radius distance perpendicular from entry point
        double centerEasting = entryPoint.Easting + _turningRadius * Math.Cos(perpHeadingRad);
        double centerNorthing = entryPoint.Northing + _turningRadius * Math.Sin(perpHeadingRad);

        // Generate arc waypoints (180° turn, discretized into 18 waypoints = 10° each)
        int numWaypoints = 18;
        double startAngle = entryHeadingRad - Math.PI / 2.0; // 90° before entry heading

        for (int i = 0; i <= numWaypoints; i++)
        {
            double angle = startAngle + (Math.PI * i / numWaypoints); // 180° total arc
            double waypointEasting = centerEasting + _turningRadius * Math.Cos(angle);
            double waypointNorthing = centerNorthing + _turningRadius * Math.Sin(angle);
            double waypointHeading = NormalizeHeading((angle + Math.PI / 2.0) * 180.0 / Math.PI);

            waypoints.Add(new Position
            {
                Easting = waypointEasting,
                Northing = waypointNorthing,
                Heading = waypointHeading,
                Latitude = entryPoint.Latitude, // Simplified: preserve original lat/lon
                Longitude = entryPoint.Longitude,
                Zone = entryPoint.Zone,
                Hemisphere = entryPoint.Hemisphere
            });
        }

        return waypoints.ToArray();
    }

    /// <summary>
    /// Generates a T-turn path with forward, reverse, and forward segments.
    /// Allows for manual nudging of entry points.
    /// </summary>
    private Position[] GenerateTTurn(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading)
    {
        var waypoints = new List<Position>();

        // T-turn consists of:
        // 1. Forward segment (short distance forward)
        // 2. Reverse segment (back up while turning 90°)
        // 3. Forward segment (forward to exit at 180° from entry)

        double headingRad = entryHeading * Math.PI / 180.0;

        // Segment 1: Forward 3 meters
        double forwardDist = 3.0;
        for (int i = 0; i <= 3; i++)
        {
            double dist = forwardDist * i / 3.0;
            waypoints.Add(CreateWaypoint(entryPoint, headingRad, dist, entryHeading));
        }

        Position endForward = waypoints[^1];

        // Segment 2: Reverse and turn 90° (5 waypoints)
        double reverseHeading = NormalizeHeading(entryHeading + 90.0);
        double reverseHeadingRad = reverseHeading * Math.PI / 180.0;
        double reverseDist = _turningRadius;

        for (int i = 1; i <= 5; i++)
        {
            double dist = reverseDist * i / 5.0;
            double heading = entryHeading + (90.0 * i / 5.0);
            waypoints.Add(CreateWaypoint(endForward, reverseHeadingRad, -dist, heading));
        }

        Position endReverse = waypoints[^1];

        // Segment 3: Forward to exit at 180° heading
        double exitHeadingActual = NormalizeHeading(entryHeading + 180.0);
        double exitHeadingRad = exitHeadingActual * Math.PI / 180.0;
        double exitDist = _turningRadius + forwardDist;

        for (int i = 1; i <= 5; i++)
        {
            double dist = exitDist * i / 5.0;
            waypoints.Add(CreateWaypoint(endReverse, exitHeadingRad, dist, exitHeadingActual));
        }

        return waypoints.ToArray();
    }

    /// <summary>
    /// Generates a Y-turn path with angled forward, small reverse, and angled forward segments.
    /// Reduces reversing distance compared to T-turn.
    /// </summary>
    private Position[] GenerateYTurn(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading)
    {
        var waypoints = new List<Position>();

        // Y-turn consists of:
        // 1. Forward at 45° angle
        // 2. Short reverse segment
        // 3. Forward at 180° from entry

        double headingRad = entryHeading * Math.PI / 180.0;

        // Segment 1: Forward at 45° angle (angled right)
        double angle1 = NormalizeHeading(entryHeading + 45.0);
        double angle1Rad = angle1 * Math.PI / 180.0;
        double forwardDist = _turningRadius * 1.5;

        for (int i = 0; i <= 5; i++)
        {
            double dist = forwardDist * i / 5.0;
            waypoints.Add(CreateWaypoint(entryPoint, angle1Rad, dist, angle1));
        }

        Position endForward1 = waypoints[^1];

        // Segment 2: Small reverse at 90° to left
        double reverseAngle = NormalizeHeading(angle1 - 90.0);
        double reverseAngleRad = reverseAngle * Math.PI / 180.0;
        double reverseDist = _turningRadius * 0.5;

        for (int i = 1; i <= 3; i++)
        {
            double dist = reverseDist * i / 3.0;
            waypoints.Add(CreateWaypoint(endForward1, reverseAngleRad, -dist, reverseAngle));
        }

        Position endReverse = waypoints[^1];

        // Segment 3: Forward to exit at 180° heading
        double exitHeadingActual = NormalizeHeading(entryHeading + 180.0);
        double exitHeadingRad = exitHeadingActual * Math.PI / 180.0;
        double exitDist = forwardDist;

        for (int i = 1; i <= 5; i++)
        {
            double dist = exitDist * i / 5.0;
            waypoints.Add(CreateWaypoint(endReverse, exitHeadingRad, dist, exitHeadingActual));
        }

        return waypoints.ToArray();
    }

    // ==================== Helper Methods ====================

    /// <summary>
    /// Calculates exit point for a turn assuming parallel exit path at turning radius * 2 distance
    /// </summary>
    private Position CalculateExitPoint(Position entryPoint, double entryHeading, double exitHeading)
    {
        // Simple approach: exit point is perpendicular to entry at 2x turning radius distance
        double perpHeading = NormalizeHeading(entryHeading + 90.0);
        double perpHeadingRad = perpHeading * Math.PI / 180.0;
        double offset = _turningRadius * 2.0;

        double exitEasting = entryPoint.Easting + offset * Math.Cos(perpHeadingRad);
        double exitNorthing = entryPoint.Northing + offset * Math.Sin(perpHeadingRad);

        return new Position
        {
            Easting = exitEasting,
            Northing = exitNorthing,
            Heading = exitHeading,
            Latitude = entryPoint.Latitude,
            Longitude = entryPoint.Longitude,
            Zone = entryPoint.Zone,
            Hemisphere = entryPoint.Hemisphere
        };
    }

    /// <summary>
    /// Creates a waypoint at a given distance and direction from a base position
    /// </summary>
    private Position CreateWaypoint(Position basePosition, double headingRad, double distance, double headingDegrees)
    {
        double easting = basePosition.Easting + distance * Math.Cos(headingRad);
        double northing = basePosition.Northing + distance * Math.Sin(headingRad);

        return new Position
        {
            Easting = easting,
            Northing = northing,
            Heading = headingDegrees,
            Latitude = basePosition.Latitude,
            Longitude = basePosition.Longitude,
            Zone = basePosition.Zone,
            Hemisphere = basePosition.Hemisphere
        };
    }

    /// <summary>
    /// Calculates distance between two positions using UTM coordinates
    /// </summary>
    private double CalculateDistance(Position p1, Position p2)
    {
        double dEasting = p2.Easting - p1.Easting;
        double dNorthing = p2.Northing - p1.Northing;
        return Math.Sqrt(dEasting * dEasting + dNorthing * dNorthing);
    }

    /// <summary>
    /// Normalizes heading to 0-360 degree range
    /// </summary>
    private double NormalizeHeading(double heading)
    {
        heading = heading % 360.0;
        if (heading < 0)
            heading += 360.0;
        return heading;
    }
}
