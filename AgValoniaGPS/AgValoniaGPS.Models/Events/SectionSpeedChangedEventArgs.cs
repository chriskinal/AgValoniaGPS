namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for section speed changes
/// </summary>
public class SectionSpeedChangedEventArgs : EventArgs
{
    /// <summary>
    /// ID of the section whose speed changed
    /// </summary>
    public readonly int SectionId;

    /// <summary>
    /// New speed of the section in meters per second
    /// </summary>
    public readonly double Speed;

    /// <summary>
    /// When the speed change occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of SectionSpeedChangedEventArgs
    /// </summary>
    /// <param name="sectionId">ID of the section</param>
    /// <param name="speed">New speed in m/s</param>
    public SectionSpeedChangedEventArgs(int sectionId, double speed)
    {
        if (sectionId < 0)
            throw new ArgumentOutOfRangeException(nameof(sectionId), "Section ID must be non-negative");

        SectionId = sectionId;
        Speed = speed;
        Timestamp = DateTime.UtcNow;
    }
}
