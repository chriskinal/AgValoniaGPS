namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Defines how the U-turn system selects the next track to work.
/// Compatible with legacy AgOpenGPS SkipMode.
/// </summary>
public enum RowSkipMode
{
    /// <summary>
    /// Normal mode: Proceed to the next adjacent track.
    /// Most common mode for continuous field work.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Alternative mode: Skip every other track.
    /// Useful for multi-pass operations or avoiding wheel tracks.
    /// </summary>
    Alternative = 1,

    /// <summary>
    /// Ignore worked tracks: Find the next unworked track.
    /// Skips over already-worked sections automatically.
    /// Requires worked section tracking to be enabled.
    /// </summary>
    IgnoreWorkedTracks = 2
}
