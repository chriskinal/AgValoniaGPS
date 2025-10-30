using System;

namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents an ISOBUS section control command for turning sections on/off.
/// Used for precision agriculture section control via ISO 11783 standard.
/// </summary>
public class IsobusSectionControlCommand
{
    /// <summary>
    /// Section control enabled flag
    /// </summary>
    public bool SectionControlEnabled { get; set; }

    /// <summary>
    /// Number of sections
    /// </summary>
    public int NumberOfSections { get; set; }

    /// <summary>
    /// Section states (true = on, false = off)
    /// Array length must match NumberOfSections
    /// </summary>
    public bool[] SectionStates { get; set; } = Array.Empty<bool>();

    /// <summary>
    /// Creates a new ISOBUS section control command
    /// </summary>
    public IsobusSectionControlCommand()
    {
    }

    /// <summary>
    /// Creates a new ISOBUS section control command with specified states
    /// </summary>
    public IsobusSectionControlCommand(bool enabled, bool[] sectionStates)
    {
        SectionControlEnabled = enabled;
        SectionStates = sectionStates ?? Array.Empty<bool>();
        NumberOfSections = SectionStates.Length;
    }

    /// <summary>
    /// Validates that the command is properly formed
    /// </summary>
    public bool IsValid()
    {
        return NumberOfSections > 0 &&
               NumberOfSections <= 31 &&
               SectionStates.Length == NumberOfSections;
    }
}
