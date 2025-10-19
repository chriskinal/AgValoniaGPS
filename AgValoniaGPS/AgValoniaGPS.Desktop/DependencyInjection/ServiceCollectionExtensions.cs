using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Vehicle;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Desktop.DependencyInjection;

/// <summary>
/// Extension methods for registering AgValoniaGPS services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all AgValoniaGPS services, including Wave 1 (Position & Kinematics), Wave 2 (Guidance Line Core), Wave 3 (Steering Algorithms), Wave 4 (Section Control), and Wave 5 (Field Operations) services.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddAgValoniaServices(this IServiceCollection services)
    {
        // Register ViewModels
        services.AddTransient<MainViewModel>();

        // Register Models (AOG_Dev integration)
        services.AddSingleton(sp => CreateDefaultVehicleConfiguration());

        // Register Services
        services.AddSingleton<IUdpCommunicationService, UdpCommunicationService>();
        services.AddSingleton<IGpsService, GpsService>();
        services.AddSingleton<IFieldService, FieldService>();
        services.AddSingleton<IGuidanceService, GuidanceService>();
        services.AddSingleton<INtripClientService, NtripClientService>();
        services.AddSingleton<FieldStatisticsService>();

        // Wave 1: Position & Kinematics Services
        services.AddSingleton<IPositionUpdateService, PositionUpdateService>();
        services.AddSingleton<IHeadingCalculatorService, HeadingCalculatorService>();
        services.AddSingleton<IVehicleKinematicsService, VehicleKinematicsService>();

        // Wave 2: Guidance Line Core Services
        AddWave2GuidanceServices(services);

        // Wave 3: Steering Algorithms
        AddWave3SteeringServices(services);

        // Wave 4: Section Control Services
        AddWave4SectionControlServices(services);

        // Wave 5: Field Operations Services
        AddWave5FieldOperationsServices(services);

        return services;
    }

    /// <summary>
    /// Registers Wave 2 guidance line services (AB Line, Curve Line, and Contour) with Scoped lifetime.
    /// These services are registered as Scoped to allow per-request/per-operation isolation while maintaining
    /// state during a single field operation session.
    /// </summary>
    /// <param name="services">The service collection to add Wave 2 services to</param>
    /// <remarks>
    /// Wave 2 services provide core guidance line functionality:
    /// - IABLineService: Straight AB line guidance with parallel line generation
    /// - ICurveLineService: Curved path guidance with advanced smoothing algorithms
    /// - IContourService: Real-time contour recording and following
    ///
    /// All services are optimized for 20-25 Hz operation and support metric/imperial units.
    /// </remarks>
    private static void AddWave2GuidanceServices(IServiceCollection services)
    {
        // AB Line Service - Provides straight line guidance, parallel line generation, and cross-track error calculation
        services.AddScoped<IABLineService, ABLineService>();

        // Curve Line Service - Provides curved path guidance with cubic spline smoothing using MathNet.Numerics
        services.AddScoped<ICurveLineService, CurveLineService>();

        // Contour Service - Provides real-time contour recording, locking, and guidance following
        services.AddScoped<IContourService, ContourService>();
    }

    /// <summary>
    /// Registers Wave 3 steering algorithm services (Look-Ahead Distance, Stanley, Pure Pursuit, and Steering Coordinator) with Singleton lifetime.
    /// These services are registered as Singleton for optimal performance in the 100Hz guidance loop.
    /// </summary>
    /// <param name="services">The service collection to add Wave 3 services to</param>
    /// <remarks>
    /// Wave 3 services provide steering control algorithms:
    /// - ILookAheadDistanceService: Adaptive look-ahead distance calculation based on speed, cross-track error, and curvature
    /// - IStanleySteeringService: Stanley steering algorithm (cross-track error + heading error based)
    /// - IPurePursuitService: Pure Pursuit steering algorithm (look-ahead point based)
    /// - ISteeringCoordinatorService: Coordinates between algorithms, manages PGN output, and handles real-time algorithm switching
    ///
    /// All services are thread-safe, optimized for 100Hz operation, and include integral control for steady-state error elimination.
    /// </remarks>
    private static void AddWave3SteeringServices(IServiceCollection services)
    {
        // Look-Ahead Distance Service - Provides adaptive look-ahead distance calculation
        services.AddSingleton<ILookAheadDistanceService, LookAheadDistanceService>();

        // Stanley Steering Service - Stanley algorithm implementation with integral control
        services.AddSingleton<IStanleySteeringService, StanleySteeringService>();

        // Pure Pursuit Service - Pure Pursuit algorithm implementation with goal point calculation
        services.AddSingleton<IPurePursuitService, PurePursuitService>();

        // Steering Coordinator Service - Algorithm coordinator with PGN output and real-time algorithm switching
        services.AddSingleton<ISteeringCoordinatorService, SteeringCoordinatorService>();
    }

    /// <summary>
    /// Registers Wave 4 section control services with Singleton lifetime.
    /// These services are registered as Singleton for optimal performance and state management.
    /// </summary>
    /// <param name="services">The service collection to add Wave 4 services to</param>
    /// <remarks>
    /// Wave 4 services provide section control functionality:
    /// - IAnalogSwitchStateService: Manages work/steer/lock switch states
    /// - ISectionConfigurationService: Validates and manages section configuration
    /// - ICoverageMapService: Triangle strip tracking with overlap detection
    /// - ISectionSpeedService: Calculates individual section speeds during turns
    /// - ISectionControlService: State machine for section on/off control
    /// - ISectionControlFileService: Read/write SectionConfig.txt
    /// - ICoverageMapFileService: Read/write Coverage.txt
    ///
    /// All services are thread-safe and optimized for <10ms total loop time.
    /// </remarks>
    private static void AddWave4SectionControlServices(IServiceCollection services)
    {
        // Leaf Services (no service dependencies)
        services.AddSingleton<IAnalogSwitchStateService, AnalogSwitchStateService>();
        services.AddSingleton<ISectionConfigurationService, SectionConfigurationService>();
        services.AddSingleton<ICoverageMapService, CoverageMapService>();

        // Dependent Services (require leaf services)
        services.AddSingleton<ISectionSpeedService, SectionSpeedService>();
        services.AddSingleton<ISectionControlService, SectionControlService>();

        // File I/O Services
        services.AddSingleton<ISectionControlFileService, SectionControlFileService>();
        services.AddSingleton<ICoverageMapFileService, CoverageMapFileService>();
    }

    /// <summary>
    /// Registers Wave 5 field operations services with Singleton lifetime.
    /// These services are registered as Singleton for optimal performance and thread-safe access.
    /// </summary>
    /// <param name="services">The service collection to add Wave 5 services to</param>
    /// <remarks>
    /// Wave 5 services provide field operations functionality:
    /// - IPointInPolygonService: Ray-casting algorithm for point containment checks with R-tree spatial indexing
    /// - IBoundaryManagementService: Boundary loading, validation, simplification, and violation detection
    /// - IBoundaryFileService: Multi-format boundary file I/O (AgOpenGPS .txt, GeoJSON, KML)
    /// - IHeadlandService: Headland generation, tracking, and real-time entry/exit detection
    /// - IHeadlandFileService: Multi-format headland file I/O (AgOpenGPS .txt, GeoJSON, KML)
    /// - IUTurnService: U-turn path generation and execution with automatic section pause/resume
    /// - ITramLineService: Tram line generation, proximity detection, and pattern management
    /// - ITramLineFileService: Multi-format tram line file I/O (AgOpenGPS .txt, GeoJSON, KML)
    ///
    /// All services are thread-safe and optimized for real-time field operations.
    /// </remarks>
    private static void AddWave5FieldOperationsServices(IServiceCollection services)
    {
        // Point-in-Polygon Service - Foundation service for geometric containment checks
        services.AddSingleton<IPointInPolygonService, PointInPolygonService>();

        // Boundary Management Service - Boundary loading, validation, simplification, and violation detection
        services.AddSingleton<IBoundaryManagementService, BoundaryManagementService>();

        // Boundary File Service - Multi-format boundary file I/O
        services.AddSingleton<IBoundaryFileService, BoundaryFileService>();

        // Headland Service - Headland generation and real-time tracking
        services.AddSingleton<IHeadlandService, HeadlandService>();

        // Headland File Service - Multi-format headland file I/O
        services.AddSingleton<IHeadlandFileService, HeadlandFileService>();

        // U-Turn Service - Turn path generation and execution
        services.AddSingleton<IUTurnService, UTurnService>();

        // Tram Line Service - Tram line generation and proximity detection
        services.AddSingleton<ITramLineService, TramLineService>();

        // Tram Line File Service - Multi-format tram line file I/O
        services.AddSingleton<ITramLineFileService, TramLineFileService>();
    }

    private static VehicleConfiguration CreateDefaultVehicleConfiguration()
    {
        return new VehicleConfiguration
        {
            // Physical dimensions - reasonable defaults for a medium tractor
            AntennaHeight = 3.0,
            AntennaPivot = 0.6,
            AntennaOffset = 0.0,
            Wheelbase = 2.5,
            TrackWidth = 1.8,

            // Vehicle type
            Type = VehicleType.Tractor,

            // Steering limits
            MaxSteerAngle = 35.0,
            MaxAngularVelocity = 35.0,

            // Goal point look-ahead parameters (from AOG_Dev)
            GoalPointLookAheadHold = 4.0,
            GoalPointLookAheadMult = 1.4,
            GoalPointAcquireFactor = 1.5,
            MinLookAheadDistance = 2.0,

            // Stanley steering algorithm parameters
            StanleyDistanceErrorGain = 0.8,
            StanleyHeadingErrorGain = 1.0,
            StanleyIntegralGainAB = 0.0,
            StanleyIntegralDistanceAwayTriggerAB = 0.3,

            // Pure Pursuit algorithm parameters
            PurePursuitIntegralGain = 0.0,

            // Heading dead zone
            DeadZoneHeading = 0.5,
            DeadZoneDelay = 10,

            // U-turn compensation
            UTurnCompensation = 1.0,

            // Hydraulic lift look-ahead distances
            HydLiftLookAheadDistanceLeft = 1.0,
            HydLiftLookAheadDistanceRight = 1.0
        };
    }
}
