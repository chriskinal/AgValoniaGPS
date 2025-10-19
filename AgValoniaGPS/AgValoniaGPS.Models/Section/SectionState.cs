namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents the operational state of a section
/// </summary>
public enum SectionState
{
    /// <summary>
    /// Section is under automatic control based on boundaries, coverage, and speed
    /// </summary>
    Auto,

    /// <summary>
    /// Section is manually forced on by operator (overrides automation)
    /// </summary>
    ManualOn,

    /// <summary>
    /// Section is manually forced off by operator (overrides automation)
    /// </summary>
    ManualOff,

    /// <summary>
    /// Section is inactive (reversing, slow speed, or work switch off)
    /// </summary>
    Off
}
