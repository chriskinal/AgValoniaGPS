using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for generating and executing U-turn patterns at field boundaries.
/// Supports Omega (Dubins paths), K-turn, Wide, T-turn, and Y-turn patterns.
/// Integrates with DubinsPathService for optimal turn path generation.
/// Performance target: &lt;10ms for turn path generation, &lt;5ms for turn detection.
/// </summary>
public interface IUTurnService
{
    /// <summary>
    /// Event raised when a U-turn is started
    /// </summary>
    event EventHandler<UTurnStartedEventArgs>? UTurnStarted;

    /// <summary>
    /// Event raised when a U-turn is completed
    /// </summary>
    event EventHandler<UTurnCompletedEventArgs>? UTurnCompleted;

    // ========== Configuration ==========

    /// <summary>
    /// Configures turn parameters
    /// </summary>
    /// <param name="parameters">Complete turn configuration</param>
    void ConfigureTurn(TurnParameters parameters);

    /// <summary>
    /// Configures turn parameters (legacy method for backward compatibility)
    /// </summary>
    /// <param name="turnType">Type of turn to execute</param>
    /// <param name="turningRadius">Turning radius in meters</param>
    /// <param name="autoPause">Whether to automatically pause sections during turn</param>
    [Obsolete("Use ConfigureTurn(TurnParameters) instead")]
    void ConfigureTurn(UTurnType turnType, double turningRadius, bool autoPause);

    // ========== Turn Path Generation ==========

    /// <summary>
    /// Generates a turn path using configured parameters and Dubins integration.
    /// </summary>
    /// <param name="entryPoint">Entry point (end of current track)</param>
    /// <param name="entryHeading">Entry heading in radians</param>
    /// <param name="exitPoint">Exit point (start of next track)</param>
    /// <param name="exitHeading">Exit heading in radians</param>
    /// <param name="parameters">Turn parameters (uses configured if null)</param>
    /// <returns>Complete turn path with waypoints and metadata</returns>
    TurnPath GenerateTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        TurnParameters? parameters = null);

    /// <summary>
    /// Generates a turn path (legacy method for backward compatibility)
    /// </summary>
    [Obsolete("Use GenerateTurn() instead")]
    Position[] GenerateTurnPath(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading);

    /// <summary>
    /// Generates multiple turn path options and returns the shortest valid path.
    /// Tries different turn styles and selects optimal solution.
    /// </summary>
    /// <param name="entryPoint">Entry point (end of current track)</param>
    /// <param name="entryHeading">Entry heading in radians</param>
    /// <param name="exitPoint">Exit point (start of next track)</param>
    /// <param name="exitHeading">Exit heading in radians</param>
    /// <param name="allowedStyles">Turn styles to consider (null = all)</param>
    /// <param name="parameters">Turn parameters (uses configured if null)</param>
    /// <returns>List of turn paths sorted by total length (shortest first)</returns>
    List<TurnPath> GenerateAllTurnOptions(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        List<TurnStyle>? allowedStyles = null,
        TurnParameters? parameters = null);

    // ========== Boundary Checking ==========

    /// <summary>
    /// Checks if a turn path violates field boundaries.
    /// </summary>
    /// <param name="turnPath">Turn path to validate</param>
    /// <param name="fieldBoundary">Field boundary points (closed polygon)</param>
    /// <param name="minDistance">Minimum required distance from boundary</param>
    /// <returns>Boundary check result with collision details</returns>
    TurnBoundaryCheck CheckTurnBoundary(
        TurnPath turnPath,
        List<Position2D> fieldBoundary,
        double minDistance);

    /// <summary>
    /// Generates a turn path that avoids boundary collisions.
    /// Automatically adjusts turn style if needed.
    /// </summary>
    /// <param name="entryPoint">Entry point (end of current track)</param>
    /// <param name="entryHeading">Entry heading in radians</param>
    /// <param name="exitPoint">Exit point (start of next track)</param>
    /// <param name="exitHeading">Exit heading in radians</param>
    /// <param name="fieldBoundary">Field boundary points (closed polygon)</param>
    /// <param name="parameters">Turn parameters (uses configured if null)</param>
    /// <returns>Valid turn path, or null if no valid path exists</returns>
    TurnPath? GenerateBoundarySafeTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        List<Position2D> fieldBoundary,
        TurnParameters? parameters = null);

    // ========== Track Selection (Row Skip Modes) ==========

    /// <summary>
    /// Finds the next track to work based on row skip mode.
    /// </summary>
    /// <param name="currentTrackIndex">Index of current track</param>
    /// <param name="totalTracks">Total number of tracks in field</param>
    /// <param name="workedTracks">Set of already-worked track indices (for IgnoreWorkedTracks mode)</param>
    /// <param name="skipMode">Row skip mode to use</param>
    /// <param name="tracksToSkip">Number of tracks to skip (for Alternative mode)</param>
    /// <returns>Index of next track to work, or null if no tracks available</returns>
    int? FindNextTrack(
        int currentTrackIndex,
        int totalTracks,
        HashSet<int>? workedTracks,
        RowSkipMode skipMode,
        int tracksToSkip = 1);

    // ========== Turn Execution ==========

    /// <summary>
    /// Starts a U-turn from the current position
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="currentHeading">Current vehicle heading in degrees</param>
    void StartTurn(Position currentPosition, double currentHeading);

    /// <summary>
    /// Updates turn progress based on current position
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    void UpdateTurnProgress(Position currentPosition);

    /// <summary>
    /// Completes the current turn
    /// </summary>
    void CompleteTurn();

    /// <summary>
    /// Checks if currently executing a turn
    /// </summary>
    /// <returns>True if in turn, false otherwise</returns>
    bool IsInTurn();

    /// <summary>
    /// Gets the current turn style
    /// </summary>
    /// <returns>Current turn style, or null if not in turn</returns>
    TurnStyle? GetCurrentTurnStyle();

    /// <summary>
    /// Gets the current turn type (legacy method)
    /// </summary>
    [Obsolete("Use GetCurrentTurnStyle() instead")]
    UTurnType GetCurrentTurnType();

    /// <summary>
    /// Gets the current turn path
    /// </summary>
    /// <returns>Turn path, or null if not in turn</returns>
    TurnPath? GetCurrentTurn();

    /// <summary>
    /// Gets the current turn path waypoints (legacy method)
    /// </summary>
    [Obsolete("Use GetCurrentTurn() instead")]
    Position[]? GetCurrentTurnPath();

    /// <summary>
    /// Gets the current turn progress as a value from 0.0 to 1.0
    /// </summary>
    /// <returns>Progress value (0.0 = start, 1.0 = complete)</returns>
    double GetTurnProgress();
}
