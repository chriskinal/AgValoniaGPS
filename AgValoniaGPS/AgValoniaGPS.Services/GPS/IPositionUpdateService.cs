using System;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.GPS
{
    /// <summary>
    /// Processes GPS position updates and maintains position history at 10Hz update rate.
    /// Provides smoothing, speed calculation, and reverse detection capabilities.
    /// </summary>
    public interface IPositionUpdateService
    {
        /// <summary>
        /// Raised when a new position has been processed and validated.
        /// </summary>
        event EventHandler<PositionUpdateEventArgs>? PositionUpdated;

        /// <summary>
        /// Processes incoming GPS position data with optional IMU fusion.
        /// Updates current position, speed, heading, and history buffer.
        /// </summary>
        /// <param name="gpsData">GPS data from NMEA parser containing fix, speed, and altitude</param>
        /// <param name="imuData">Optional IMU data for sensor fusion and roll correction</param>
        /// <remarks>
        /// This method should be called at the GPS update rate (typically 10Hz).
        /// Thread-safe for concurrent access.
        /// </remarks>
        void ProcessGpsPosition(GpsData gpsData, ImuData? imuData);

        /// <summary>
        /// Gets the current vehicle position in UTM coordinates.
        /// </summary>
        /// <returns>Current position with easting, northing, and altitude</returns>
        GeoCoord GetCurrentPosition();

        /// <summary>
        /// Gets the current heading in radians (0 to 2π).
        /// </summary>
        /// <returns>Current heading in radians, 0 = North, π/2 = East</returns>
        double GetCurrentHeading();

        /// <summary>
        /// Gets the current speed in meters per second.
        /// </summary>
        /// <returns>Speed in m/s, calculated from position deltas</returns>
        double GetCurrentSpeed();

        /// <summary>
        /// Determines if the vehicle is currently reversing.
        /// </summary>
        /// <returns>True if heading change indicates reverse direction</returns>
        bool IsReversing();

        /// <summary>
        /// Gets the position history buffer for trail display and calculations.
        /// </summary>
        /// <param name="count">Number of historical positions to retrieve (max 20)</param>
        /// <returns>Array of historical positions, most recent first</returns>
        /// <remarks>
        /// History maintained in circular buffer with maximum 20 positions.
        /// Used for fix-to-fix heading calculation and smoothing.
        /// </remarks>
        GeoCoord[] GetPositionHistory(int count);

        /// <summary>
        /// Gets the GPS update frequency in Hertz.
        /// </summary>
        /// <returns>Current GPS update rate (typically 10Hz)</returns>
        double GetGpsFrequency();

        /// <summary>
        /// Resets the position service state (heading, history, etc.).
        /// </summary>
        /// <remarks>
        /// Used when re-initializing heading or clearing history.
        /// </remarks>
        void Reset();
    }

    /// <summary>
    /// Event arguments for position update events.
    /// </summary>
    public class PositionUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The updated position in UTM coordinates.
        /// </summary>
        public GeoCoord Position { get; set; }

        /// <summary>
        /// Current heading in radians.
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        /// Current speed in meters per second.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Distance travelled since last update in meters.
        /// </summary>
        public double DistanceTravelled { get; set; }

        /// <summary>
        /// True if vehicle is reversing.
        /// </summary>
        public bool IsReversing { get; set; }

        /// <summary>
        /// Timestamp of this position update.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public PositionUpdateEventArgs()
        {
            Position = new GeoCoord();
            Timestamp = DateTime.UtcNow;
        }
    }
}
