using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implements U-turn pattern generation and execution for field operations.
/// Supports Omega (Dubins paths), K-turn, Wide, T-turn, and Y-turn patterns.
/// Integrates with DubinsPathService for optimal turn path generation.
/// Thread-safe implementation using lock object pattern.
/// </summary>
public class UTurnService : IUTurnService
{
    private readonly object _lockObject = new object();
    private readonly IDubinsPathService _dubinsPathService;
    private readonly IBoundaryGuidedDubinsService _boundaryGuidedDubinsService;

    // Turn configuration
    private TurnParameters _turnParameters;
    private bool _autoPause = true;

    // Legacy configuration (for backward compatibility)
    private UTurnType _legacyTurnType = UTurnType.Omega;
    private double _legacyTurningRadius = 5.0;

    // Turn state
    private bool _isInTurn = false;
    private TurnPath? _currentTurn = null;
    private Position? _turnStartPosition = null;
    private DateTime _turnStartTime;
    private double _turnProgress = 0.0;
    private int _currentWaypointIndex = 0;

    public event EventHandler<UTurnStartedEventArgs>? UTurnStarted;
    public event EventHandler<UTurnCompletedEventArgs>? UTurnCompleted;

    /// <summary>
    /// Creates a new UTurnService with DubinsPathService and boundary-guided path generation.
    /// </summary>
    /// <param name="dubinsPathService">Service for generating Dubins paths</param>
    /// <param name="boundaryGuidedDubinsService">Service for boundary-aware path generation</param>
    public UTurnService(
        IDubinsPathService dubinsPathService,
        IBoundaryGuidedDubinsService boundaryGuidedDubinsService)
    {
        _dubinsPathService = dubinsPathService ?? throw new ArgumentNullException(nameof(dubinsPathService));
        _boundaryGuidedDubinsService = boundaryGuidedDubinsService ?? throw new ArgumentNullException(nameof(boundaryGuidedDubinsService));

        // Initialize default turn parameters
        _turnParameters = new TurnParameters(
            turnStyle: TurnStyle.Omega,
            turningRadius: 5.0,
            rowSkipMode: RowSkipMode.Normal,
            rowSkipWidth: 6.0
        );
    }

    // ========== Configuration ==========

    public void ConfigureTurn(TurnParameters parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        lock (_lockObject)
        {
            if (parameters.TurningRadius <= 0)
                throw new ArgumentOutOfRangeException(nameof(parameters), "Turning radius must be positive");

            _turnParameters = parameters;
        }
    }

    [Obsolete("Use ConfigureTurn(TurnParameters) instead")]
    public void ConfigureTurn(UTurnType turnType, double turningRadius, bool autoPause)
    {
        lock (_lockObject)
        {
            if (turningRadius <= 0)
                throw new ArgumentOutOfRangeException(nameof(turningRadius), "Turning radius must be positive");

            _legacyTurnType = turnType;
            _legacyTurningRadius = turningRadius;
            _autoPause = autoPause;

            // Update main parameters for legacy compatibility
            TurnStyle style = turnType switch
            {
                UTurnType.Omega => TurnStyle.Omega,
                UTurnType.T => TurnStyle.T,
                UTurnType.Y => TurnStyle.Y,
                _ => TurnStyle.Omega
            };

            _turnParameters = new TurnParameters(
                turnStyle: style,
                turningRadius: turningRadius,
                rowSkipMode: _turnParameters.RowSkipMode,
                rowSkipWidth: _turnParameters.RowSkipWidth
            );
        }
    }

    // ========== Turn Path Generation ==========

    public TurnPath GenerateTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters? parameters = null)
    {
        var turnParams = parameters ?? _turnParameters;
        var stopwatch = Stopwatch.StartNew();

        TurnPath turnPath = turnParams.TurnStyle switch
        {
            TurnStyle.Omega => GenerateOmegaTurnDubins(entryPoint, entryHeading, exitPoint, exitHeading, turnParams),
            TurnStyle.K => GenerateKTurn(entryPoint, entryHeading, exitPoint, exitHeading, turnParams),
            TurnStyle.Wide => GenerateWideTurn(entryPoint, entryHeading, exitPoint, exitHeading, turnParams),
            TurnStyle.T => GenerateTTurnNew(entryPoint, entryHeading, exitPoint, exitHeading, turnParams),
            TurnStyle.Y => GenerateYTurnNew(entryPoint, entryHeading, exitPoint, exitHeading, turnParams),
            _ => throw new ArgumentException($"Unknown turn style: {turnParams.TurnStyle}")
        };

        stopwatch.Stop();
        turnPath.ComputationTime = stopwatch.Elapsed;

        // TODO: Apply smoothing if smoothing factor > 0
        // Smoothing implementation pending
        // if (turnParams.SmoothingFactor > 0.0 && turnPath.Waypoints.Count >= 4)
        // {
        //     turnPath = SmoothTurnPath(turnPath, turnParams.SmoothingFactor);
        // }

        return turnPath;
    }

    [Obsolete("Use GenerateTurn() instead")]
    public Position[] GenerateTurnPath(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading)
    {
        if (entryPoint == null)
            throw new ArgumentNullException(nameof(entryPoint));
        if (exitPoint == null)
            throw new ArgumentNullException(nameof(exitPoint));

        // Convert to new types and call new method
        var entryPos2D = new Position2D(entryPoint.Easting, entryPoint.Northing);
        var exitPos2D = new Position2D(exitPoint.Easting, exitPoint.Northing);
        var entryHeadingRad = entryHeading * Math.PI / 180.0;
        var exitHeadingRad = exitHeading * Math.PI / 180.0;

        var turnPath = GenerateTurn(entryPos2D, entryHeadingRad, exitPos2D, exitHeadingRad);

        // Convert waypoints back to legacy Position array
        return turnPath.Waypoints.Select(wp => new Position
        {
            Easting = wp.Easting,
            Northing = wp.Northing,
            Heading = wp.Easting, // This will be recalculated
            Latitude = entryPoint.Latitude,
            Longitude = entryPoint.Longitude,
            Zone = entryPoint.Zone,
            Hemisphere = entryPoint.Hemisphere
        }).ToArray();
    }

    public List<TurnPath> GenerateAllTurnOptions(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        List<TurnStyle>? allowedStyles = null,
        TurnParameters? parameters = null)
    {
        var turnParams = parameters ?? _turnParameters;
        var styles = allowedStyles ?? new List<TurnStyle> { TurnStyle.Omega, TurnStyle.K, TurnStyle.Wide, TurnStyle.T, TurnStyle.Y };

        var turnPaths = new List<TurnPath>();

        foreach (var style in styles)
        {
            try
            {
                var paramsClone = new TurnParameters(
                    turnStyle: style,
                    turningRadius: turnParams.TurningRadius,
                    rowSkipMode: turnParams.RowSkipMode,
                    rowSkipWidth: turnParams.RowSkipWidth
                )
                {
                    SmoothingFactor = turnParams.SmoothingFactor,
                    BoundaryMinDistance = turnParams.BoundaryMinDistance,
                    WaypointSpacing = turnParams.WaypointSpacing,
                    WideRadiusMultiplier = turnParams.WideRadiusMultiplier
                };

                var turnPath = GenerateTurn(entryPoint, entryHeading, exitPoint, exitHeading, paramsClone);
                turnPaths.Add(turnPath);
            }
            catch
            {
                // Skip styles that fail to generate
                continue;
            }
        }

        // Sort by total length (shortest first)
        return turnPaths.OrderBy(t => t.TotalLength).ToList();
    }

    // ========== Boundary Checking ==========

    public TurnBoundaryCheck CheckTurnBoundary(
        TurnPath turnPath,
        List<Position2D> fieldBoundary,
        double minDistance)
    {
        if (turnPath == null)
            throw new ArgumentNullException(nameof(turnPath));
        if (fieldBoundary == null || fieldBoundary.Count < 3)
            throw new ArgumentException("Field boundary must have at least 3 points", nameof(fieldBoundary));

        double closestDistance = double.MaxValue;
        var violationPoints = new List<Position2D>();
        int? firstViolationIndex = null;

        // Check each waypoint against boundary
        for (int i = 0; i < turnPath.Waypoints.Count; i++)
        {
            var waypoint = turnPath.Waypoints[i];
            double distanceToBoundary = CalculateDistanceToPolygon(waypoint, fieldBoundary);

            if (distanceToBoundary < closestDistance)
            {
                closestDistance = distanceToBoundary;
            }

            if (distanceToBoundary < minDistance)
            {
                violationPoints.Add(waypoint);
                firstViolationIndex ??= i;
            }
        }

        if (violationPoints.Count > 0)
        {
            return TurnBoundaryCheck.Invalid(
                $"Turn path violates boundary: {violationPoints.Count} waypoints within {minDistance}m",
                firstViolationIndex,
                violationPoints
            );
        }

        return TurnBoundaryCheck.Valid(closestDistance);
    }

    public TurnPath? GenerateBoundarySafeTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        List<Position2D> fieldBoundary,
        TurnParameters? parameters = null)
    {
        var turnParams = parameters ?? _turnParameters;

        // Step 1: Try boundary-guided Dubins for Omega turns (handles complex scenarios)
        if (turnParams.TurnStyle == TurnStyle.Omega || turnParams.TurnStyle == TurnStyle.Wide)
        {
            var boundaries = new List<List<Position2D>> { fieldBoundary };
            double effectiveRadius = turnParams.TurnStyle == TurnStyle.Wide
                ? turnParams.TurningRadius * turnParams.WideRadiusMultiplier
                : turnParams.TurningRadius;

            var guidedResult = _boundaryGuidedDubinsService.GenerateBoundaryAwarePath(
                entryPoint.Easting, entryPoint.Northing, entryHeading,
                exitPoint.Easting, exitPoint.Northing, exitHeading,
                effectiveRadius,
                boundaries,
                turnParams.BoundaryMinDistance,
                turnParams.WaypointSpacing,
                maxIterations: 8 // Performance budget: <10ms
            );

            if (guidedResult.Succeeded && guidedResult.ResultPath != null)
            {
                // Convert to TurnPath
                var turnPath = new TurnPath(turnParams.TurnStyle, entryPoint, exitPoint)
                {
                    EntryHeading = entryHeading,
                    ExitHeading = exitHeading,
                    DubinsPath = guidedResult.ResultPath,
                    Waypoints = guidedResult.ResultPath.Waypoints
                        .Select(w => new Position2D(w.Easting, w.Northing))
                        .ToList(),
                    TotalLength = guidedResult.ResultPath.TotalLength,
                    ComputationTime = guidedResult.ComputationTime,
                    BoundaryCheck = TurnBoundaryCheck.Valid(guidedResult.MinBoundaryDistance)
                };

                return turnPath;
            }
        }

        // Step 2: Try all turn styles (standard approach for non-Omega or if guided sampling failed)
        var allOptions = GenerateAllTurnOptions(entryPoint, entryHeading, exitPoint, exitHeading, null, turnParams);

        foreach (var turnPath in allOptions)
        {
            var boundaryCheck = CheckTurnBoundary(turnPath, fieldBoundary, turnParams.BoundaryMinDistance);
            if (boundaryCheck.IsValid)
            {
                turnPath.BoundaryCheck = boundaryCheck;
                return turnPath;
            }
        }

        // Step 3: Fallback - return K-turn even if it violates (caller can decide)
        return null; // No valid path found
    }

    // ========== Track Selection (Row Skip Modes) ==========

    public int? FindNextTrack(
        int currentTrackIndex,
        int totalTracks,
        HashSet<int>? workedTracks,
        RowSkipMode skipMode,
        int tracksToSkip = 1)
    {
        if (currentTrackIndex < 0 || currentTrackIndex >= totalTracks)
            throw new ArgumentOutOfRangeException(nameof(currentTrackIndex));
        if (totalTracks <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalTracks));
        if (tracksToSkip < 0)
            throw new ArgumentOutOfRangeException(nameof(tracksToSkip));

        return skipMode switch
        {
            RowSkipMode.Normal => FindNextTrackNormal(currentTrackIndex, totalTracks),
            RowSkipMode.Alternative => FindNextTrackAlternative(currentTrackIndex, totalTracks, tracksToSkip),
            RowSkipMode.IgnoreWorkedTracks => FindNextUnworkedTrack(currentTrackIndex, totalTracks, workedTracks),
            _ => null
        };
    }

    private int? FindNextTrackNormal(int currentTrackIndex, int totalTracks)
    {
        int nextIndex = currentTrackIndex + 1;
        return nextIndex < totalTracks ? nextIndex : null;
    }

    private int? FindNextTrackAlternative(int currentTrackIndex, int totalTracks, int tracksToSkip)
    {
        int nextIndex = currentTrackIndex + tracksToSkip + 1;
        return nextIndex < totalTracks ? nextIndex : null;
    }

    private int? FindNextUnworkedTrack(int currentTrackIndex, int totalTracks, HashSet<int>? workedTracks)
    {
        if (workedTracks == null)
        {
            // No worked tracks info, fall back to normal mode
            return FindNextTrackNormal(currentTrackIndex, totalTracks);
        }

        // Search forward for next unworked track
        for (int i = currentTrackIndex + 1; i < totalTracks; i++)
        {
            if (!workedTracks.Contains(i))
            {
                return i;
            }
        }

        // No unworked tracks found
        return null;
    }

    // ========== Turn Execution ==========

    public void StartTurn(Position currentPosition, double currentHeading)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lockObject)
        {
            // Calculate exit point (simple approach: assume 180° turn to parallel path)
            double exitHeading = NormalizeHeading(currentHeading + 180.0);
            Position exitPoint = CalculateExitPoint(currentPosition, currentHeading, exitHeading);

            // Generate turn path using legacy method (converts to new type internally)
            var legacyPath = GenerateTurnPath(currentPosition, currentHeading, exitPoint, exitHeading);

            _turnStartPosition = currentPosition;
            _turnStartTime = DateTime.UtcNow;
            _isInTurn = true;
            _turnProgress = 0.0;
            _currentWaypointIndex = 0;

            // Raise event with legacy path
            UTurnStarted?.Invoke(this, new UTurnStartedEventArgs(_legacyTurnType, currentPosition, legacyPath));
        }
    }

    public void UpdateTurnProgress(Position currentPosition)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lockObject)
        {
            if (!_isInTurn || _currentTurn == null || _currentTurn.Waypoints.Count == 0)
                return;

            // Find closest waypoint to current position
            double minDistance = double.MaxValue;
            int closestIndex = _currentWaypointIndex;

            var currentPos2D = new Position2D(currentPosition.Easting, currentPosition.Northing);

            for (int i = _currentWaypointIndex; i < _currentTurn.Waypoints.Count; i++)
            {
                double de = _currentTurn.Waypoints[i].Easting - currentPos2D.Easting;
                double dn = _currentTurn.Waypoints[i].Northing - currentPos2D.Northing;
                double distance = Math.Sqrt(de * de + dn * dn);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            _currentWaypointIndex = closestIndex;

            // Calculate progress as ratio of waypoints completed
            _turnProgress = (double)_currentWaypointIndex / (_currentTurn.Waypoints.Count - 1);
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

            Position endPosition = _currentTurn != null && _currentTurn.Waypoints.Count > 0
                ? new Position
                {
                    Easting = _currentTurn.Waypoints[^1].Easting,
                    Northing = _currentTurn.Waypoints[^1].Northing,
                    Heading = 0,
                    Latitude = 0,
                    Longitude = 0,
                    Zone = 0,
                    Hemisphere = 'N'
                }
                : _turnStartPosition ?? new Position();

            _isInTurn = false;
            _turnProgress = 1.0;

            // Raise event
            UTurnCompleted?.Invoke(this, new UTurnCompletedEventArgs(_legacyTurnType, endPosition, turnDuration));

            // Reset state
            _currentTurn = null;
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

    public TurnStyle? GetCurrentTurnStyle()
    {
        lock (_lockObject)
        {
            return _isInTurn ? _currentTurn?.TurnStyle : null;
        }
    }

    [Obsolete("Use GetCurrentTurnStyle() instead")]
    public UTurnType GetCurrentTurnType()
    {
        lock (_lockObject)
        {
            return _legacyTurnType;
        }
    }

    public TurnPath? GetCurrentTurn()
    {
        lock (_lockObject)
        {
            return _currentTurn;
        }
    }

    [Obsolete("Use GetCurrentTurn() instead")]
    public Position[]? GetCurrentTurnPath()
    {
        lock (_lockObject)
        {
            // Convert current turn waypoints to legacy Position array
            if (_currentTurn == null || _currentTurn.Waypoints.Count == 0)
                return null;

            return _currentTurn.Waypoints.Select(wp => new Position
            {
                Easting = wp.Easting,
                Northing = wp.Northing,
                Heading = 0.0, // Will be recalculated
                Latitude = 0.0,
                Longitude = 0.0,
                Zone = 0,
                Hemisphere = 'N'
            }).ToArray();
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
    /// Generates an Omega turn using DubinsPathService for optimal smooth paths.
    /// </summary>
    private TurnPath GenerateOmegaTurnDubins(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters parameters)
    {
        // Use Dubins path service to generate optimal turn
        var dubinsPath = _dubinsPathService.GeneratePath(
            entryPoint.Easting, entryPoint.Northing, entryHeading,
            exitPoint.Easting, exitPoint.Northing, exitHeading,
            parameters.TurningRadius,
            parameters.WaypointSpacing
        );

        var turnPath = new TurnPath(TurnStyle.Omega, entryPoint, exitPoint)
        {
            EntryHeading = entryHeading,
            ExitHeading = exitHeading,
            DubinsPath = dubinsPath
        };

        if (dubinsPath != null)
        {
            // Convert Dubins waypoints to turn waypoints
            turnPath.Waypoints = dubinsPath.Waypoints
                .Select(w => new Position2D(w.Easting, w.Northing))
                .ToList();
            turnPath.TotalLength = dubinsPath.TotalLength;
        }
        else
        {
            // Fallback: create simple semicircular arc
            turnPath.Waypoints = GenerateSimpleOmegaWaypoints(entryPoint, entryHeading, parameters.TurningRadius, parameters.WaypointSpacing);
            turnPath.TotalLength = Math.PI * parameters.TurningRadius; // Semicircle length
        }

        return turnPath;
    }

    /// <summary>
    /// Generates a K-turn (three-point turn) for tight spaces.
    /// </summary>
    private TurnPath GenerateKTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters parameters)
    {
        var turnPath = new TurnPath(TurnStyle.K, entryPoint, exitPoint)
        {
            EntryHeading = entryHeading,
            ExitHeading = exitHeading,
            RequiresReverse = true
        };

        var waypoints = new List<Position2D>();

        // K-turn: Forward, Reverse, Forward (three segments)
        double forwardDist = parameters.TurningRadius * 0.8;
        double reverseDist = parameters.TurningRadius * 0.6;

        // Segment 1: Forward at 45° angle
        double angle1 = entryHeading + Math.PI / 4.0; // 45° right
        for (double d = 0; d <= forwardDist; d += parameters.WaypointSpacing)
        {
            double e = entryPoint.Easting + d * Math.Cos(angle1);
            double n = entryPoint.Northing + d * Math.Sin(angle1);
            waypoints.Add(new Position2D(e, n));
        }

        Position2D endForward = waypoints[^1];

        // Segment 2: Reverse at -45° angle (left)
        double angle2 = angle1 - Math.PI / 2.0; // 90° left from forward direction
        for (double d = parameters.WaypointSpacing; d <= reverseDist; d += parameters.WaypointSpacing)
        {
            double e = endForward.Easting + d * Math.Cos(angle2);
            double n = endForward.Northing + d * Math.Sin(angle2);
            waypoints.Add(new Position2D(e, n));
        }

        Position2D endReverse = waypoints[^1];

        // Segment 3: Forward to exit heading
        for (double d = parameters.WaypointSpacing; d <= forwardDist; d += parameters.WaypointSpacing)
        {
            double e = endReverse.Easting + d * Math.Cos(exitHeading);
            double n = endReverse.Northing + d * Math.Sin(exitHeading);
            waypoints.Add(new Position2D(e, n));
        }

        turnPath.Waypoints = waypoints;
        turnPath.TotalLength = forwardDist * 2 + reverseDist;

        return turnPath;
    }

    /// <summary>
    /// Generates a wide turn using increased turning radius for gentle turns.
    /// Wide turns are similar to Omega turns but use a larger radius multiplier,
    /// making them suitable for large implements or when more space is available.
    /// Uses DubinsPathService with increased radius to create smooth, gradual curves.
    /// </summary>
    /// <param name="entryPoint">Starting position of the turn</param>
    /// <param name="entryHeading">Starting heading in radians</param>
    /// <param name="exitPoint">Exit position of the turn</param>
    /// <param name="exitHeading">Exit heading in radians</param>
    /// <param name="parameters">Turn configuration including WideRadiusMultiplier</param>
    /// <returns>TurnPath with waypoints forming a wide semicircular turn</returns>
    /// <remarks>
    /// Wide turn algorithm:
    /// - Applies configurable radius multiplier (default 1.5x) to base turning radius
    /// - Uses Dubins path generation for optimal smooth curves
    /// - May create two semicircle segments connected by straight line for large offsets
    /// - Handles left and right turns symmetrically
    /// - Requires more headland space than Omega but provides gentler turns
    /// Legacy source: SourceCodeLatest/GPS/Classes/CYouTurn.cs CreateCurveWideTurn/CreateABWideTurn
    /// </remarks>
    private TurnPath GenerateWideTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters parameters)
    {
        // Apply configurable wide radius multiplier
        double wideRadius = parameters.TurningRadius * parameters.WideRadiusMultiplier;

        // Create parameters with increased radius for Dubins path generation
        var wideParams = new TurnParameters(
            turnStyle: TurnStyle.Omega,
            turningRadius: wideRadius,
            rowSkipMode: parameters.RowSkipMode,
            rowSkipWidth: parameters.RowSkipWidth)
        {
            WaypointSpacing = parameters.WaypointSpacing,
            BoundaryMinDistance = parameters.BoundaryMinDistance,
            SmoothingFactor = parameters.SmoothingFactor,
            WideRadiusMultiplier = parameters.WideRadiusMultiplier
        };

        // Generate turn using Dubins path with wider radius
        var turnPath = GenerateOmegaTurnDubins(entryPoint, entryHeading, exitPoint, exitHeading, wideParams);

        // Create new TurnPath with Wide style and increased radius metadata
        var wideTurnPath = new TurnPath(TurnStyle.Wide, entryPoint, exitPoint)
        {
            EntryHeading = entryHeading,
            ExitHeading = exitHeading,
            Waypoints = turnPath.Waypoints,
            TotalLength = turnPath.TotalLength,
            DubinsPath = turnPath.DubinsPath
        };

        return wideTurnPath;
    }

    /// <summary>
    /// Generates a T-turn path (updated version returning TurnPath).
    /// </summary>
    private TurnPath GenerateTTurnNew(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters parameters)
    {
        var turnPath = new TurnPath(TurnStyle.T, entryPoint, exitPoint)
        {
            EntryHeading = entryHeading,
            ExitHeading = exitHeading,
            RequiresReverse = true
        };

        var waypoints = new List<Position2D>();
        double forwardDist = 3.0;
        double reverseDist = parameters.TurningRadius;

        // Forward segment
        for (double d = 0; d <= forwardDist; d += parameters.WaypointSpacing)
        {
            double e = entryPoint.Easting + d * Math.Cos(entryHeading);
            double n = entryPoint.Northing + d * Math.Sin(entryHeading);
            waypoints.Add(new Position2D(e, n));
        }

        Position2D endForward = waypoints[^1];

        // Reverse and turn 90°
        double reverseHeading = entryHeading + Math.PI / 2.0;
        for (double d = parameters.WaypointSpacing; d <= reverseDist; d += parameters.WaypointSpacing)
        {
            double e = endForward.Easting + d * Math.Cos(reverseHeading);
            double n = endForward.Northing + d * Math.Sin(reverseHeading);
            waypoints.Add(new Position2D(e, n));
        }

        Position2D endReverse = waypoints[^1];

        // Forward to exit
        double exitDist = reverseDist + forwardDist;
        for (double d = parameters.WaypointSpacing; d <= exitDist; d += parameters.WaypointSpacing)
        {
            double e = endReverse.Easting + d * Math.Cos(exitHeading);
            double n = endReverse.Northing + d * Math.Sin(exitHeading);
            waypoints.Add(new Position2D(e, n));
        }

        turnPath.Waypoints = waypoints;
        turnPath.TotalLength = forwardDist + reverseDist + exitDist;

        return turnPath;
    }

    /// <summary>
    /// Generates a Y-turn path (updated version returning TurnPath).
    /// </summary>
    private TurnPath GenerateYTurnNew(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters parameters)
    {
        var turnPath = new TurnPath(TurnStyle.Y, entryPoint, exitPoint)
        {
            EntryHeading = entryHeading,
            ExitHeading = exitHeading,
            RequiresReverse = true
        };

        var waypoints = new List<Position2D>();
        double forwardDist = parameters.TurningRadius * 1.5;
        double reverseDist = parameters.TurningRadius * 0.5;

        // Forward at 45° angle
        double angle1 = entryHeading + Math.PI / 4.0;
        for (double d = 0; d <= forwardDist; d += parameters.WaypointSpacing)
        {
            double e = entryPoint.Easting + d * Math.Cos(angle1);
            double n = entryPoint.Northing + d * Math.Sin(angle1);
            waypoints.Add(new Position2D(e, n));
        }

        Position2D endForward = waypoints[^1];

        // Small reverse at 90° left
        double reverseAngle = angle1 - Math.PI / 2.0;
        for (double d = parameters.WaypointSpacing; d <= reverseDist; d += parameters.WaypointSpacing)
        {
            double e = endForward.Easting + d * Math.Cos(reverseAngle);
            double n = endForward.Northing + d * Math.Sin(reverseAngle);
            waypoints.Add(new Position2D(e, n));
        }

        Position2D endReverse = waypoints[^1];

        // Forward to exit
        for (double d = parameters.WaypointSpacing; d <= forwardDist; d += parameters.WaypointSpacing)
        {
            double e = endReverse.Easting + d * Math.Cos(exitHeading);
            double n = endReverse.Northing + d * Math.Sin(exitHeading);
            waypoints.Add(new Position2D(e, n));
        }

        turnPath.Waypoints = waypoints;
        turnPath.TotalLength = forwardDist * 2 + reverseDist;

        return turnPath;
    }

    /// <summary>
    /// Generates simple Omega waypoints as fallback when Dubins path fails.
    /// </summary>
    private List<Position2D> GenerateSimpleOmegaWaypoints(Position2D entryPoint, double entryHeading, double radius, double spacing)
    {
        var waypoints = new List<Position2D>();

        // Semicircular arc
        double perpHeading = entryHeading + Math.PI / 2.0;
        double centerE = entryPoint.Easting + radius * Math.Cos(perpHeading);
        double centerN = entryPoint.Northing + radius * Math.Sin(perpHeading);

        double arcLength = Math.PI * radius;
        int numWaypoints = (int)(arcLength / spacing);

        for (int i = 0; i <= numWaypoints; i++)
        {
            double angle = (entryHeading - Math.PI / 2.0) + (Math.PI * i / numWaypoints);
            double e = centerE + radius * Math.Cos(angle);
            double n = centerN + radius * Math.Sin(angle);
            waypoints.Add(new Position2D(e, n));
        }

        return waypoints;
    }

    // ==================== Path Smoothing ====================

    /// <summary>
    /// Smooths a turn path using Catmull-Rom spline interpolation.
    /// Generates additional waypoints between existing ones for smoother turns,
    /// then recalculates headings based on fore/aft waypoint differences.
    /// </summary>
    /// <param name="turnPath">Original turn path to smooth</param>
    /// <param name="smoothingFactor">Smoothing intensity (0.0 = no smoothing, 1.0 = maximum)</param>
    /// <returns>New turn path with smoothed waypoints</returns>
    private TurnPath SmoothTurnPath(TurnPath turnPath, double smoothingFactor)
    {
        if (turnPath == null || turnPath.Waypoints.Count < 4)
        {
            return turnPath; // Not enough points for Catmull-Rom spline
        }

        if (smoothingFactor <= 0.0)
        {
            return turnPath; // No smoothing requested
        }

        // Clamp smoothing factor
        smoothingFactor = Math.Clamp(smoothingFactor, 0.0, 1.0);

        // Convert Position2D waypoints to Position for MathUtilities.CatmullRom
        var originalWaypoints = turnPath.Waypoints
            .Select(wp => new Position
            {
                Easting = wp.Easting,
                Northing = wp.Northing,
                Zone = 0,
                Hemisphere = 'N'
            })
            .ToList();

        // Calculate number of segments to insert based on smoothing factor
        // 0.0 -> 0 segments (no smoothing)
        // 0.5 -> 2 segments (moderate smoothing)
        // 1.0 -> 5 segments (maximum smoothing)
        int segmentsPerSpan = (int)Math.Round(smoothingFactor * 5.0);
        if (segmentsPerSpan < 1)
        {
            segmentsPerSpan = 1;
        }

        // Generate smoothed waypoints using Catmull-Rom spline
        var smoothedPositions = MathUtilities.CatmullRomPath(originalWaypoints, segmentsPerSpan);

        if (smoothedPositions.Count < 3)
        {
            return turnPath; // Not enough points for heading recalculation
        }

        // Recalculate headings based on fore/aft waypoint differences
        // Skip first and last waypoints (they don't have fore/aft neighbors)
        var smoothedWaypointsWithHeadings = new List<Position2D>();

        for (int i = 0; i < smoothedPositions.Count; i++)
        {
            Position2D waypoint;

            if (i == 0)
            {
                // First waypoint: use heading from first to second point
                double heading = CalculateHeadingBetweenPoints(
                    smoothedPositions[0],
                    smoothedPositions[1]
                );
                waypoint = new Position2D(smoothedPositions[0].Easting, smoothedPositions[0].Northing);
            }
            else if (i == smoothedPositions.Count - 1)
            {
                // Last waypoint: use heading from second-to-last to last point
                double heading = CalculateHeadingBetweenPoints(
                    smoothedPositions[smoothedPositions.Count - 2],
                    smoothedPositions[smoothedPositions.Count - 1]
                );
                waypoint = new Position2D(smoothedPositions[i].Easting, smoothedPositions[i].Northing);
            }
            else
            {
                // Middle waypoints: calculate heading from previous to next point (smoother)
                double heading = CalculateHeadingBetweenPoints(
                    smoothedPositions[i - 1],
                    smoothedPositions[i + 1]
                );
                waypoint = new Position2D(smoothedPositions[i].Easting, smoothedPositions[i].Northing);
            }

            smoothedWaypointsWithHeadings.Add(waypoint);
        }

        // Calculate new total length
        double totalLength = 0.0;
        for (int i = 0; i < smoothedWaypointsWithHeadings.Count - 1; i++)
        {
            double de = smoothedWaypointsWithHeadings[i + 1].Easting - smoothedWaypointsWithHeadings[i].Easting;
            double dn = smoothedWaypointsWithHeadings[i + 1].Northing - smoothedWaypointsWithHeadings[i].Northing;
            totalLength += Math.Sqrt(de * de + dn * dn);
        }

        // Create new turn path with smoothed waypoints
        var smoothedTurnPath = new TurnPath(turnPath.TurnStyle, turnPath.EntryPoint, turnPath.ExitPoint)
        {
            EntryHeading = turnPath.EntryHeading,
            ExitHeading = turnPath.ExitHeading,
            Waypoints = smoothedWaypointsWithHeadings,
            TotalLength = totalLength,
            ComputationTime = turnPath.ComputationTime,
            BoundaryCheck = turnPath.BoundaryCheck,
            DubinsPath = turnPath.DubinsPath,
            RequiresReverse = turnPath.RequiresReverse,
            NextTrackIndex = turnPath.NextTrackIndex,
            IsDirectionReversal = turnPath.IsDirectionReversal,
            TracksSkipped = turnPath.TracksSkipped
        };

        return smoothedTurnPath;
    }

    /// <summary>
    /// Calculates the heading (in radians) from one point to another using atan2.
    /// This matches the legacy implementation's heading calculation.
    /// </summary>
    /// <param name="from">Starting position</param>
    /// <param name="to">Ending position</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    private double CalculateHeadingBetweenPoints(Position from, Position to)
    {
        double heading = Math.Atan2(
            to.Easting - from.Easting,
            to.Northing - from.Northing
        );

        // Normalize to [0, 2π)
        if (heading < 0)
        {
            heading += MathConstants.TwoPI;
        }

        return heading;
    }

    // ==================== Helper Methods ====================

    /// <summary>
    /// Calculates the minimum distance from a point to a polygon boundary.
    /// </summary>
    private double CalculateDistanceToPolygon(Position2D point, List<Position2D> polygon)
    {
        double minDistance = double.MaxValue;

        // Check distance to each edge of the polygon
        for (int i = 0; i < polygon.Count; i++)
        {
            Position2D p1 = polygon[i];
            Position2D p2 = polygon[(i + 1) % polygon.Count];

            double distance = CalculateDistanceToLineSegment(point, p1, p2);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    /// <summary>
    /// Calculates the distance from a point to a line segment.
    /// </summary>
    private double CalculateDistanceToLineSegment(Position2D point, Position2D lineStart, Position2D lineEnd)
    {
        double dx = lineEnd.Easting - lineStart.Easting;
        double dy = lineEnd.Northing - lineStart.Northing;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared < 1e-10)
        {
            // Line segment is actually a point
            double de = point.Easting - lineStart.Easting;
            double dn = point.Northing - lineStart.Northing;
            return Math.Sqrt(de * de + dn * dn);
        }

        // Calculate projection parameter
        double t = ((point.Easting - lineStart.Easting) * dx + (point.Northing - lineStart.Northing) * dy) / lengthSquared;
        t = Math.Clamp(t, 0.0, 1.0);

        // Calculate closest point on line segment
        double closestE = lineStart.Easting + t * dx;
        double closestN = lineStart.Northing + t * dy;

        // Calculate distance
        double distE = point.Easting - closestE;
        double distN = point.Northing - closestN;
        return Math.Sqrt(distE * distE + distN * distN);
    }

    /// <summary>
    /// Calculates exit point for a turn assuming parallel exit path at turning radius * 2 distance
    /// </summary>
    private Position CalculateExitPoint(Position entryPoint, double entryHeading, double exitHeading)
    {
        // Simple approach: exit point is perpendicular to entry at 2x turning radius distance
        double perpHeading = NormalizeHeading(entryHeading + 90.0);
        double perpHeadingRad = perpHeading * Math.PI / 180.0;
        double offset = _turnParameters.TurningRadius * 2.0;

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
