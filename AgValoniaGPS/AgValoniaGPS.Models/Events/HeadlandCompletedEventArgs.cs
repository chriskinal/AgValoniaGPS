namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for headland completion events
/// </summary>
public class HeadlandCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Pass number (0-based) that was completed
    /// </summary>
    public readonly int PassNumber;

    /// <summary>
    /// Area covered in square meters
    /// </summary>
    public readonly double AreaCovered;

    /// <summary>
    /// When the completion occurred (UTC)
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of HeadlandCompletedEventArgs
    /// </summary>
    /// <param name="passNumber">Pass number (0-based)</param>
    /// <param name="areaCovered">Area covered in square meters</param>
    public HeadlandCompletedEventArgs(int passNumber, double areaCovered)
    {
        if (passNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(passNumber), "Pass number must be non-negative");
        if (areaCovered < 0)
            throw new ArgumentOutOfRangeException(nameof(areaCovered), "Area covered must be non-negative");

        PassNumber = passNumber;
        AreaCovered = areaCovered;
        Timestamp = DateTime.UtcNow;
    }
}
