using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.Services.Vehicle;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.Configuration;
using AgValoniaGPS.Services.Session;
using AgValoniaGPS.Services.Profile;
using AgValoniaGPS.Services.UI;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.Desktop.Services;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Profile;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.ViewModels.Panels.Display;
using AgValoniaGPS.ViewModels.Panels.FieldManagement;
using AgValoniaGPS.ViewModels.Panels.FieldOperations;
using AgValoniaGPS.ViewModels.Panels.Guidance;
using AgValoniaGPS.ViewModels.Panels.Configuration;

namespace AgValoniaGPS.Desktop.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgValoniaServices(this IServiceCollection services)
    {
        // Communication Services
        services.AddSingleton<IUdpCommunicationService, UdpCommunicationService>();
        services.AddSingleton<IGpsService, GpsService>();
        services.AddSingleton<INtripClientService, NtripClientService>();

        // Position & Kinematics Services (Wave 1)
        services.AddSingleton<IPositionUpdateService, PositionUpdateService>();
        services.AddSingleton<IHeadingCalculatorService, HeadingCalculatorService>();
        services.AddSingleton<IVehicleKinematicsService, VehicleKinematicsService>();

        // Guidance Services (Wave 2)
        services.AddSingleton<IABLineService, ABLineService>();
        services.AddSingleton<ICurveLineService, CurveLineService>();
        services.AddSingleton<IGuidanceService, GuidanceService>();

        // Section Control Services (Wave 4)
        services.AddSingleton<ISectionControlService, SectionControlService>();

        // Field Operations Services (Wave 5)
        services.AddSingleton<IFieldService, FieldService>();
        services.AddSingleton<IFieldStatisticsService, FieldStatisticsService>();
        services.AddSingleton<ITramLineService, TramLineService>();

        // Steering Services (Wave 5)
        services.AddSingleton<ISteeringCoordinatorService, SteeringCoordinatorService>();

        // State Management Services (Wave 8)
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ISessionManagementService, SessionManagementService>();
        services.AddSingleton<ICrashRecoveryService, CrashRecoveryService>();
        services.AddSingleton<IProfileProvider<VehicleProfile>, VehicleProfileProvider>();
        services.AddSingleton<IProfileProvider<UserProfile>, UserProfileProvider>();
        services.AddSingleton<IProfileManagementService, ProfileManagementService>();

        // UI Services (Wave 9)
        services.AddSingleton<IDialogService, DialogService>();

        // Vehicle Configuration
        services.AddSingleton<VehicleConfiguration>();

        // ViewModels - Main
        services.AddSingleton<MainViewModel>();

        // ViewModels - Wave 10 Task Group 1: Field Operations Panels
        services.AddSingleton<FormFieldDataViewModel>();
        services.AddSingleton<FormGPSDataViewModel>();
        services.AddSingleton<FormTramLineViewModel>();
        services.AddSingleton<FormQuickABViewModel>();

        // ViewModels - Wave 10 Task Group 2: Configuration Panels
        services.AddSingleton<FormSteerViewModel>();
        services.AddSingleton<FormConfigViewModel>();
        services.AddSingleton<FormDiagnosticsViewModel>();
        services.AddSingleton<FormRollCorrectionViewModel>();
        services.AddSingleton<FormVehicleConfigViewModel>();

        // ViewModels - Wave 10 Task Group 3: Field Management Panels
        services.AddSingleton<FormFlagsViewModel>();
        services.AddSingleton<FormCameraViewModel>();
        services.AddSingleton<FormBoundaryEditorViewModel>();
        services.AddSingleton<FormFieldToolsViewModel>();
        services.AddSingleton<FormFieldFileManagerViewModel>();

        return services;
    }
}
