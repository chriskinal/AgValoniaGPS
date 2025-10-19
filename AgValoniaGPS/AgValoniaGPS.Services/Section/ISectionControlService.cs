using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Main section control service managing state machine transitions, timer management,
/// boundary checking, and manual override handling.
/// </summary>
/// <remarks>
/// <para>
/// Implements a finite state machine for section control with the following states:
/// - Auto: System controls based on boundaries, coverage, and speed
/// - ManualOn: Operator forces section on (overrides automation)
/// - ManualOff: Operator forces section off (overrides automation)
/// - Off: Section inactive (reversing, slow speed, work switch off)
/// </para>
/// <para>
/// Thread-safe: All methods use lock-based synchronization.
/// Performance: &lt;5ms per update cycle for all 31 sections.
/// </para>
/// </remarks>
public interface ISectionControlService
{
    /// <summary>
    /// Raised when a section's state changes.
    /// </summary>
    event EventHandler<SectionStateChangedEventArgs>? SectionStateChanged;

    /// <summary>
    /// Updates section states based on current position, heading, speed, and system conditions.
    /// This is the main update loop called from position updates.
    /// </summary>
    /// <param name="position">Current vehicle position</param>
    /// <param name="heading">Current heading in radians</param>
    /// <param name="speed">Current speed in meters per second</param>
    /// <remarks>
    /// <para>
    /// Performs the following operations:
    /// - Checks work switch state (all sections off if inactive)
    /// - Detects reversing (immediate turn-off, no delay)
    /// - Checks boundary intersections with look-ahead anticipation
    /// - Manages turn-on and turn-off timers
    /// - Respects manual overrides
    /// - Enforces minimum speed threshold
    /// </para>
    /// <para>
    /// Performance target: &lt;5ms for all 31 sections
    /// </para>
    /// </remarks>
    void UpdateSectionStates(GeoCoord position, double heading, double speed);

    /// <summary>
    /// Sets manual override for a specific section.
    /// </summary>
    /// <param name="sectionId">Section identifier (0-based)</param>
    /// <param name="state">Desired manual override state (ManualOn, ManualOff, or Auto to release)</param>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    /// <exception cref="ArgumentException">If state is not ManualOn, ManualOff, or Auto</exception>
    /// <remarks>
    /// Manual overrides take precedence over automatic control.
    /// Setting state to Auto releases the manual override.
    /// </remarks>
    void SetManualOverride(int sectionId, SectionState state);

    /// <summary>
    /// Gets the current state of a specific section.
    /// </summary>
    /// <param name="sectionId">Section identifier (0-based)</param>
    /// <returns>Current section state</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    SectionState GetSectionState(int sectionId);

    /// <summary>
    /// Gets all section states.
    /// </summary>
    /// <returns>Array of section states indexed by section ID</returns>
    SectionState[] GetAllSectionStates();

    /// <summary>
    /// Gets whether a section is under manual override.
    /// </summary>
    /// <param name="sectionId">Section identifier (0-based)</param>
    /// <returns>True if section is manually overridden</returns>
    /// <exception cref="ArgumentOutOfRangeException">If section ID is invalid</exception>
    bool IsManualOverride(int sectionId);

    /// <summary>
    /// Resets all sections to Auto state and clears all timers.
    /// </summary>
    void ResetAllSections();
}
