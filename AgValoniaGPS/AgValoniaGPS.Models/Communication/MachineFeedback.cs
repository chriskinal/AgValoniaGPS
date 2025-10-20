namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents feedback data from a Machine module.
/// Contains work switch state and section sensor feedback for closed-loop section control.
/// </summary>
public class MachineFeedback
{
    /// <summary>
    /// Gets or sets whether the work switch is active.
    /// When false, section control should be disabled.
    /// </summary>
    public bool WorkSwitchActive { get; set; }

    /// <summary>
    /// Gets or sets the section sensor states.
    /// Each byte represents actual on/off state of a section for coverage verification.
    /// </summary>
    public byte[] SectionSensors { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the status flags from the module.
    /// </summary>
    public byte StatusFlags { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this feedback was received.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
