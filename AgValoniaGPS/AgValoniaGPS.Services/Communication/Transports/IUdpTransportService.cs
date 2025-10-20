namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// UDP-specific transport service interface.
    /// Extends ITransport with UDP-specific configuration properties.
    /// </summary>
    /// <remarks>
    /// Provides UDP broadcast-based communication for AgOpenGPS hardware modules.
    /// Each module type uses a specific port:
    /// - AutoSteer: 8888
    /// - Machine: 9999
    /// - IMU: 7777
    /// Broadcasts to 255.255.255.255 for local network module discovery.
    /// </remarks>
    public interface IUdpTransportService : ITransport
    {
        /// <summary>
        /// Gets the local port number this transport is bound to.
        /// Port assignment based on module type:
        /// - AutoSteer: 8888
        /// - Machine: 9999
        /// - IMU: 7777
        /// </summary>
        int LocalPort { get; }

        /// <summary>
        /// Gets the broadcast address used for sending to modules.
        /// Default: 255.255.255.255 (local network broadcast)
        /// </summary>
        string BroadcastAddress { get; }
    }
}
