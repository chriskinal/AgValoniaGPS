using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for generating and executing U-turn patterns at field boundaries.
/// Supports Omega (Ω), T-turn, and Y-turn patterns with automatic section pause/resume.
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

    /// <summary>
    /// Configures turn parameters
    /// </summary>
    /// <param name="turnType">Type of turn to execute</param>
    /// <param name="turningRadius">Turning radius in meters</param>
    /// <param name="autoPause">Whether to automatically pause sections during turn</param>
    void ConfigureTurn(UTurnType turnType, double turningRadius, bool autoPause);

    /// <summary>
    /// Generates a turn path between entry and exit points
    /// </summary>
    /// <param name="entryPoint">Entry point position</param>
    /// <param name="entryHeading">Entry heading in degrees</param>
    /// <param name="exitPoint">Exit point position</param>
    /// <param name="exitHeading">Exit heading in degrees</param>
    /// <returns>Array of waypoints defining the turn path</returns>
    Position[] GenerateTurnPath(Position entryPoint, double entryHeading, Position exitPoint, double exitHeading);

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
    /// Gets the current turn type
    /// </summary>
    /// <returns>Current turn type, or null if not in turn</returns>
    UTurnType GetCurrentTurnType();

    /// <summary>
    /// Gets the current turn path waypoints
    /// </summary>
    /// <returns>Turn path, or null if not in turn</returns>
    Position[]? GetCurrentTurnPath();

    /// <summary>
    /// Gets the current turn progress as a value from 0.0 to 1.0
    /// </summary>
    /// <returns>Progress value (0.0 = start, 1.0 = complete)</returns>
    double GetTurnProgress();
}
