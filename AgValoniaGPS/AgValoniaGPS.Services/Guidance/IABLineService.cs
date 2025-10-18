using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service interface for AB Line guidance operations
/// Provides functionality for creating, manipulating, and calculating guidance from straight AB lines
/// </summary>
public interface IABLineService
{
    /// <summary>
    /// Event fired when an AB line is created, modified, activated, or nudged
    /// </summary>
    event EventHandler<ABLineChangedEventArgs>? ABLineChanged;

    /// <summary>
    /// Create AB line from two GPS positions (Point A and Point B)
    /// </summary>
    /// <param name="pointA">First point defining the line</param>
    /// <param name="pointB">Second point defining the line</param>
    /// <param name="name">Name for the AB line</param>
    /// <returns>Created AB line</returns>
    /// <exception cref="ArgumentException">Thrown if points are identical or too close</exception>
    ABLine CreateFromPoints(Position pointA, Position pointB, string name);

    /// <summary>
    /// Create AB line from single point and heading angle
    /// </summary>
    /// <param name="origin">Starting point for the line</param>
    /// <param name="headingDegrees">Heading angle in degrees (0-360)</param>
    /// <param name="name">Name for the AB line</param>
    /// <param name="length">Length of the line in meters (default: 1000m)</param>
    /// <returns>Created AB line</returns>
    ABLine CreateFromHeading(Position origin, double headingDegrees, string name, double length = 1000.0);

    /// <summary>
    /// Calculate cross-track error and guidance information for current position
    /// </summary>
    /// <param name="currentPosition">Current vehicle position</param>
    /// <param name="currentHeading">Current vehicle heading in degrees</param>
    /// <param name="line">AB line to calculate guidance from</param>
    /// <returns>Guidance calculation results</returns>
    GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, ABLine line);

    /// <summary>
    /// Find the closest point on the AB line to current position
    /// </summary>
    /// <param name="currentPosition">Current position</param>
    /// <param name="line">AB line</param>
    /// <returns>Closest point on the line</returns>
    Position GetClosestPoint(Position currentPosition, ABLine line);

    /// <summary>
    /// Nudge (offset) an AB line perpendicular to its heading
    /// </summary>
    /// <param name="line">Original AB line</param>
    /// <param name="offsetMeters">Offset distance in meters (positive = right, negative = left)</param>
    /// <returns>New AB line with nudged position</returns>
    ABLine NudgeLine(ABLine line, double offsetMeters);

    /// <summary>
    /// Generate parallel AB lines at specified spacing
    /// </summary>
    /// <param name="referenceLine">Reference AB line to generate parallels from</param>
    /// <param name="spacing">Spacing between lines</param>
    /// <param name="count">Number of lines to generate on each side</param>
    /// <param name="units">Unit system for spacing (Metric or Imperial)</param>
    /// <returns>List of parallel AB lines</returns>
    List<ABLine> GenerateParallelLines(ABLine referenceLine, double spacing, int count, UnitSystem units);

    /// <summary>
    /// Validate an AB line for correctness
    /// </summary>
    /// <param name="line">AB line to validate</param>
    /// <returns>Validation result with any errors or warnings</returns>
    ValidationResult ValidateLine(ABLine line);
}

/// <summary>
/// Event arguments for AB line changes
/// </summary>
public class ABLineChangedEventArgs : EventArgs
{
    public ABLine Line { get; }
    public ABLineChangeType ChangeType { get; }
    public DateTime Timestamp { get; }

    public ABLineChangedEventArgs(ABLine line, ABLineChangeType changeType)
    {
        Line = line;
        ChangeType = changeType;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Type of change to an AB line
/// </summary>
public enum ABLineChangeType
{
    Created,
    Modified,
    Activated,
    Nudged
}
