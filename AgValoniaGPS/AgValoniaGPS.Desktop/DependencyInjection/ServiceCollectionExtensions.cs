using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Desktop.DependencyInjection;

public static class ServiceCollectionExtensions
{
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

        return services;
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