using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Coordinates steering calculations and AutoSteer hardware communication.
/// Routes calculations to active steering algorithm (Stanley or Pure Pursuit)
/// and transmits steering commands via AutoSteerCommunicationService.
/// Implements closed-loop control with feedback error tracking.
/// Performance target: <5ms for Update() execution.
/// </summary>
public class SteeringCoordinatorService : ISteeringCoordinatorService
{
    private readonly IStanleySteeringService _stanleyService;
    private readonly IPurePursuitService _purePursuitService;
    private readonly ILookAheadDistanceService _lookAheadService;
    private readonly IAutoSteerCommunicationService _autoSteerComm;
    private readonly VehicleConfiguration _vehicleConfig;
    private readonly ILogger<SteeringCoordinatorService>? _logger;

    private SteeringAlgorithm _activeAlgorithm = SteeringAlgorithm.Stanley;
    private readonly object _algorithmLock = new object();

    private double _currentSteeringAngle;
    private double _currentCrossTrackError;
    private double _currentLookAheadDistance;

    private const double SteeringErrorWarningThreshold = 2.0; // degrees

    public event EventHandler<SteeringUpdateEventArgs>? SteeringUpdated;

    /// <summary>
    /// Creates a new SteeringCoordinatorService with Wave 6 AutoSteer integration.
    /// </summary>
    /// <param name="stanleyService">Stanley steering algorithm service</param>
    /// <param name="purePursuitService">Pure Pursuit steering algorithm service</param>
    /// <param name="lookAheadService">Look-ahead distance calculation service</param>
    /// <param name="autoSteerComm">AutoSteer hardware communication service (Wave 6)</param>
    /// <param name="vehicleConfig">Vehicle configuration</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    public SteeringCoordinatorService(
        IStanleySteeringService stanleyService,
        IPurePursuitService purePursuitService,
        ILookAheadDistanceService lookAheadService,
        IAutoSteerCommunicationService autoSteerComm,
        VehicleConfiguration vehicleConfig,
        ILogger<SteeringCoordinatorService>? logger = null)
    {
        _stanleyService = stanleyService ?? throw new ArgumentNullException(nameof(stanleyService));
        _purePursuitService = purePursuitService ?? throw new ArgumentNullException(nameof(purePursuitService));
        _lookAheadService = lookAheadService ?? throw new ArgumentNullException(nameof(lookAheadService));
        _autoSteerComm = autoSteerComm ?? throw new ArgumentNullException(nameof(autoSteerComm));
        _vehicleConfig = vehicleConfig ?? throw new ArgumentNullException(nameof(vehicleConfig));
        _logger = logger;

        // Subscribe to AutoSteer feedback for closed-loop control
        _autoSteerComm.FeedbackReceived += OnAutoSteerFeedbackReceived;
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

        // Send steering command via AutoSteerCommunicationService if AutoSteer is active
        if (isAutoSteerActive)
        {
            // Convert speed to MPH (AutoSteer expects MPH)
            double speedMph = speed * 2.23694; // m/s to mph

            // Convert cross-track error to millimeters
            int xteErrorMm = (int)(guidanceResult.CrossTrackError * 1000.0); // meters to mm

            // Send command via Wave 6 AutoSteer communication service
            _autoSteerComm.SendSteeringCommand(speedMph, steeringAngle, xteErrorMm, isAutoSteerActive);
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
    /// Handles AutoSteer feedback received from hardware module.
    /// Implements closed-loop control: compares desired vs actual wheel angle and logs errors.
    /// </summary>
    private void OnAutoSteerFeedbackReceived(object? sender, Models.Events.AutoSteerFeedbackEventArgs e)
    {
        double desiredAngle = _currentSteeringAngle;
        double actualAngle = e.Feedback.ActualWheelAngle;
        double steeringError = Math.Abs(desiredAngle - actualAngle);

        // Log warning if steering error exceeds threshold (possible mechanical issue)
        if (steeringError > SteeringErrorWarningThreshold)
        {
            _logger?.LogWarning(
                "Steering error exceeds threshold: Desired={Desired:F2}°, Actual={Actual:F2}°, Error={Error:F2}° (Threshold={Threshold:F2}°)",
                desiredAngle,
                actualAngle,
                steeringError,
                SteeringErrorWarningThreshold);
        }
        else
        {
            _logger?.LogDebug(
                "Steering feedback: Desired={Desired:F2}°, Actual={Actual:F2}°, Error={Error:F2}°",
                desiredAngle,
                actualAngle,
                steeringError);
        }

        // TODO: Could adjust integral control based on persistent errors
        // TODO: Could publish diagnostic event for UI display
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
}
