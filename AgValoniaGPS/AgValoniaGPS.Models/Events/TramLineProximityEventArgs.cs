namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for tram line proximity events
/// </summary>
public class TramLineProximityEventArgs : EventArgs
{
    /// <summary>
    /// ID of the nearest tram line
    /// </summary>
    public readonly int TramLineId;

    /// <summary>
    /// Distance to the nearest tram line in meters
    /// </summary>
    public readonly double Distance;

    /// <summary>
    /// Nearest point on the tram line
    /// </summary>
    public readonly Position NearestPoint;

    /// <summary>
    /// When the proximity check occurred (UTC)
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of TramLineProximityEventArgs
    /// </summary>
    /// <param name="tramLineId">ID of the nearest tram line</param>
    /// <param name="distance">Distance to the nearest tram line in meters</param>
    /// <param name="nearestPoint">Nearest point on the tram line</param>
    public TramLineProximityEventArgs(int tramLineId, double distance, Position nearestPoint)
    {
        if (tramLineId < 0)
            throw new ArgumentOutOfRangeException(nameof(tramLineId), "Tram line ID must be non-negative");
        if (distance < 0)
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be non-negative");

        TramLineId = tramLineId;
        Distance = distance;
        NearestPoint = nearestPoint ?? throw new ArgumentNullException(nameof(nearestPoint));
        Timestamp = DateTime.UtcNow;
    }
}
