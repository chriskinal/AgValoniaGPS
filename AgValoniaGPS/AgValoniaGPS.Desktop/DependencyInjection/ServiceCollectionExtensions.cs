using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Vehicle;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Desktop.DependencyInjection;

/// <summary>
/// Extension methods for registering AgValoniaGPS services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all AgValoniaGPS services, including Wave 1 (Position & Kinematics) and Wave 2 (Guidance Line Core) services.
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
