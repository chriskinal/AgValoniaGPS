using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Coordinates steering calculations and PGN output.
/// Routes calculations to active steering algorithm (Stanley or Pure Pursuit)
/// and transmits steering commands via PGN 254 messages over UDP.
/// Performance target: <5ms for Update() execution.
/// </summary>
public class SteeringCoordinatorService : ISteeringCoordinatorService
{
    private readonly IStanleySteeringService _stanleyService;
    private readonly IPurePursuitService _purePursuitService;
    private readonly ILookAheadDistanceService _lookAheadService;
    private readonly IUdpCommunicationService _udpService;
    private readonly VehicleConfiguration _vehicleConfig;

    private SteeringAlgorithm _activeAlgorithm = SteeringAlgorithm.Stanley;
    private readonly object _algorithmLock = new object();

    private double _currentSteeringAngle;
    private double _currentCrossTrackError;
    private double _currentLookAheadDistance;

    public event EventHandler<SteeringUpdateEventArgs>? SteeringUpdated;

    public SteeringCoordinatorService(
        IStanleySteeringService stanleyService,
        IPurePursuitService purePursuitService,
        ILookAheadDistanceService lookAheadService,
        IUdpCommunicationService udpService,
        VehicleConfiguration vehicleConfig)
    {
        _stanleyService = stanleyService ?? throw new ArgumentNullException(nameof(stanleyService));
        _purePursuitService = purePursuitService ?? throw new ArgumentNullException(nameof(purePursuitService));
        _lookAheadService = lookAheadService ?? throw new ArgumentNullException(nameof(lookAheadService));
        _udpService = udpService ?? throw new ArgumentNullException(nameof(udpService));
        _vehicleConfig = vehicleConfig ?? throw new ArgumentNullException(nameof(vehicleConfig));
    }

    public SteeringAlgorithm ActiveAlgorithm
    {
        get
        {
            lock (_algorithmLock)
            {
                return _activeAlgorithm;
            }
        }
        set
        {
            lock (_algorithmLock)
            {
                if (_activeAlgorithm != value)
                {
                    _activeAlgorithm = value;
                    // Reset integrals in both services when switching algorithms
                    _stanleyService.ResetIntegral();
                    _purePursuitService.ResetIntegral();
                }
            }
        }
    }

    public double CurrentSteeringAngle => _currentSteeringAngle;
    public double CurrentCrossTrackError => _currentCrossTrackError;
    public double CurrentLookAheadDistance => _currentLookAheadDistance;

    public void Update(
        Position3D pivotPosition,
        Position3D steerPosition,
        GuidanceLineResult guidanceResult,
        double speed,
        double heading,
        bool isAutoSteerActive)
    {
        // Store current cross-track error
        _currentCrossTrackError = guidanceResult.CrossTrackError;

        // Calculate look-ahead distance (used by Pure Pursuit)
        _currentLookAheadDistance = _lookAheadService.CalculateLookAheadDistance(
            speed,
            Math.Abs(guidanceResult.CrossTrackError),
            0.0, // TODO: Get actual curvature from guidance line
            _vehicleConfig.Type,
            isAutoSteerActive
        );

        // Calculate pivot distance error for integral control
        // This is the cross-track error at the pivot point
        double pivotDistanceError = guidanceResult.CrossTrackError;

        // Route to appropriate algorithm
        double steeringAngle;
        SteeringAlgorithm currentAlgorithm;

        lock (_algorithmLock)
        {
            currentAlgorithm = _activeAlgorithm;

            if (currentAlgorithm == SteeringAlgorithm.Stanley)
            {
                // Stanley algorithm: uses cross-track error and heading error
                steeringAngle = _stanleyService.CalculateSteeringAngle(
                    guidanceResult.CrossTrackError,
                    guidanceResult.HeadingError * (Math.PI / 180.0), // Convert degrees to radians
                    speed,
                    pivotDistanceError,
                    false // TODO: Get reverse status from vehicle state
                );
            }
            else // Pure Pursuit
            {
                // Pure Pursuit: calculate goal point on guidance line
                Position3D goalPoint = CalculateGoalPoint(
                    steerPosition,
                    guidanceResult,
                    _currentLookAheadDistance
                );

                steeringAngle = _purePursuitService.CalculateSteeringAngle(
                    steerPosition,
                    goalPoint,
                    speed,
                    pivotDistanceError
                );
            }
        }

        // Store calculated steering angle
        _currentSteeringAngle = steeringAngle;

        // Send PGN 254 message if AutoSteer is active
        if (isAutoSteerActive)
        {
            SendPGN254Message(speed, steeringAngle, guidanceResult.CrossTrackError);
        }

        // Raise steering updated event
        SteeringUpdated?.Invoke(this, new SteeringUpdateEventArgs
        {
            SteeringAngle = steeringAngle,
            CrossTrackError = guidanceResult.CrossTrackError,
            LookAheadDistance = _currentLookAheadDistance,
            Algorithm = currentAlgorithm,
            Timestamp = DateTime.UtcNow,
            IsAutoSteerActive = isAutoSteerActive
        });
    }

    /// <summary>
    /// Calculate goal point on guidance line at look-ahead distance from steer axle
    /// </summary>
    private Position3D CalculateGoalPoint(
        Position3D steerPosition,
        GuidanceLineResult guidanceResult,
        double lookAheadDistance)
    {
        // Get closest point on guidance line
        var closestPoint = guidanceResult.ClosestPoint;

        // Calculate guidance line heading (perpendicular to cross-track error direction)
        // For now, use a simplified approach: project look-ahead distance along line heading
        // The heading can be derived from the vehicle heading and heading error
        double lineHeadingRadians = guidanceResult.HeadingError * (Math.PI / 180.0);

        // Calculate goal point position
        double goalEasting = closestPoint.Easting + lookAheadDistance * Math.Sin(lineHeadingRadians);
        double goalNorthing = closestPoint.Northing + lookAheadDistance * Math.Cos(lineHeadingRadians);

        return new Position3D(goalEasting, goalNorthing, lineHeadingRadians);
    }

    /// <summary>
    /// Construct and send PGN 254 (AutoSteer Data) message.
    /// Format:
    /// Byte 0-1: Header (0x80, 0x81)
    /// Byte 2: Source (0x7F)
    /// Byte 3: PGN (254 / 0xFE)
    /// Byte 4: Length (10 bytes)
    /// Byte 5-6: speedHi/Lo - speed * 10 (uint16, big-endian)
    /// Byte 7: status - 0=off, 1=on
    /// Byte 8-9: steerAngleHi/Lo - steer angle * 100 (int16, big-endian)
    /// Byte 10-11: distanceHi/Lo - cross-track error in mm (int16, big-endian)
    /// Byte 12-13: Reserved (0x00)
    /// Byte 14: CRC
    /// </summary>
    private void SendPGN254Message(double speed, double steeringAngle, double crossTrackError)
    {
        // Create PGN message with 10 data bytes
        var pgn = new PgnMessage
        {
            Source = 0x7F,
            PGN = PgnNumbers.AUTOSTEER_DATA2, // 254
            Length = 10,
            Data = new byte[10]
        };

        // Byte 0-1: Speed * 10 (uint16, big-endian)
        ushort speedValue = (ushort)Math.Clamp(speed * 10.0, 0, 65535);
        pgn.Data[0] = (byte)(speedValue >> 8);   // High byte
        pgn.Data[1] = (byte)(speedValue & 0xFF); // Low byte

        // Byte 2: Status (1 = AutoSteer on)
        pgn.Data[2] = 1;

        // Byte 3-4: Steering angle * 100 (int16, big-endian)
        short steerValue = (short)Math.Clamp(steeringAngle * 100.0, -32768, 32767);
        pgn.Data[3] = (byte)(steerValue >> 8);   // High byte
        pgn.Data[4] = (byte)(steerValue & 0xFF); // Low byte

        // Byte 5-6: Cross-track error in mm (int16, big-endian)
        short xteValue = (short)Math.Clamp(crossTrackError * 1000.0, -32768, 32767);
        pgn.Data[5] = (byte)(xteValue >> 8);     // High byte
        pgn.Data[6] = (byte)(xteValue & 0xFF);   // Low byte

        // Byte 7-8: Reserved
        pgn.Data[7] = 0x00;
        pgn.Data[8] = 0x00;

        // Byte 9: Reserved
        pgn.Data[9] = 0x00;

        // Convert to bytes (includes CRC calculation)
        byte[] message = pgn.ToBytes();

        // Send via UDP to modules
        _udpService.SendToModules(message);
    }
}
