namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents an individual section with its state and properties
/// </summary>
public class Section
{
    /// <summary>
    /// Unique identifier for the section (0-based index)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Width of the section in meters
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Current commanded state of the section
    /// </summary>
    public SectionState State { get; set; }

    /// <summary>
    /// Actual state from section sensor feedback (Wave 6 integration).
    /// Used for coverage mapping to reflect true section state, not just commanded state.
    /// </summary>
    public bool ActualState { get; set; }

    /// <summary>
    /// Current speed of the section in meters per second
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// Whether section is under manual override control
    /// </summary>
    public bool IsManualOverride { get; set; }

    /// <summary>
    /// Turn-on timer start time (null if not active)
    /// </summary>
    public DateTime? TurnOnTimerStart { get; set; }

    /// <summary>
    /// Turn-off timer start time (null if not active)
    /// </summary>
    public DateTime? TurnOffTimerStart { get; set; }

    /// <summary>
    /// Lateral offset from vehicle centerline in meters (negative = left, positive = right)
    /// </summary>
    public double LateralOffset { get; set; }

    /// <summary>
    /// Creates a new section with the specified ID and width
    /// </summary>
    /// <param name="id">Section identifier (0-based)</param>
    /// <param name="width">Section width in meters</param>
    public Section(int id, double width)
    {
        Id = id;
        Width = width;
        State = SectionState.Off;
        ActualState = false;
        Speed = 0.0;
        IsManualOverride = false;
        TurnOnTimerStart = null;
        TurnOffTimerStart = null;
        LateralOffset = 0.0;
    }

    /// <summary>
    /// Creates a default section
    /// </summary>
    public Section() : this(0, 2.5)
    {
    }
}
