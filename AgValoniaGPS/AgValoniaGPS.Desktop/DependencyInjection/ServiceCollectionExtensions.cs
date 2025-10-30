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
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.Services.Display;
using AgValoniaGPS.Services.UndoRedo;
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

        // Machine Communication Services (Wave 4)
        services.AddSingleton<IMachineCommunicationService, MachineCommunicationService>();
        services.AddSingleton<IAutoSteerCommunicationService, AutoSteerCommunicationService>();
        services.AddSingleton<IImuCommunicationService, ImuCommunicationService>();
        services.AddSingleton<IPgnMessageBuilderService, PgnMessageBuilderService>();
        services.AddSingleton<IPgnMessageParserService, PgnMessageParserService>();
        services.AddSingleton<ITransportAbstractionService, TransportAbstractionService>();
        services.AddSingleton<IModuleCoordinatorService, ModuleCoordinatorService>();
        services.AddSingleton<IHardwareSimulatorService, HardwareSimulatorService>();
        services.AddSingleton<IIsobusCommunicationService, IsobusCommunicationService>(); // Section 6A: ISOBUS protocol support

        // Position & Kinematics Services (Wave 1)
        services.AddSingleton<IPositionUpdateService, PositionUpdateService>();
        services.AddSingleton<IHeadingCalculatorService, HeadingCalculatorService>();
        services.AddSingleton<IVehicleKinematicsService, VehicleKinematicsService>();

        // Guidance Services (Wave 2)
        services.AddSingleton<IABLineService, ABLineService>();
        services.AddSingleton<ICurveLineService, CurveLineService>();
        services.AddSingleton<IContourService, ContourService>();
        services.AddSingleton<IGuidanceService, GuidanceService>();
        services.AddSingleton<ITrackManagementService, TrackManagementService>();

        // Guidance File Services (Wave 2)
        services.AddSingleton<ABLineFileService>();
        services.AddSingleton<CurveLineFileService>();
        services.AddSingleton<ContourLineFileService>();

        // Section Control Services (Wave 4)
        services.AddSingleton<IAnalogSwitchStateService, AnalogSwitchStateService>();
        services.AddSingleton<ISectionConfigurationService, SectionConfigurationService>();
        services.AddSingleton<ISectionSpeedService, SectionSpeedService>();
        services.AddSingleton<ICoverageMapService, CoverageMapService>();
        services.AddSingleton<ISectionControlFileService, SectionControlFileService>();
        services.AddSingleton<ICoverageMapFileService, CoverageMapFileService>();
        services.AddSingleton<ISectionGeometryService, SectionGeometryService>(); // Section 6B: Triangle generation
        services.AddSingleton<ISectionControlService, SectionControlService>();

        // Field Operations Services (Wave 5)
        services.AddSingleton<IFieldService, FieldService>();
        services.AddSingleton<IFieldStatisticsService, FieldStatisticsService>();
        services.AddSingleton<ITramLineService, TramLineService>();
        services.AddSingleton<IFieldMarkerService, FieldMarkerService>();
        services.AddSingleton<IPointInPolygonService, PointInPolygonService>();
        services.AddSingleton<IBoundaryManagementService, BoundaryManagementService>();
        services.AddSingleton<IHeadlandService, HeadlandService>();
        services.AddSingleton<IUTurnService, UTurnService>();
        services.AddSingleton<IDubinsPathService, DubinsPathService>(); // Critical for U-turn path generation
        services.AddSingleton<IBoundaryGuidedDubinsService, BoundaryGuidedDubinsService>(); // Enhanced Dubins with boundary-guided sampling
        services.AddSingleton<IPathRecordingService, PathRecordingService>(); // Path recording and smoothing
        services.AddSingleton<IRecordedPathFileService, RecordedPathFileService>(); // Path file I/O
        services.AddSingleton<IElevationService, ElevationService>(); // Section 6C: Elevation mapping
        services.AddSingleton<IElevationFileService, ElevationFileService>(); // Section 6C: Elevation file I/O

        // Field Operations File Services (Wave 5)
        services.AddSingleton<IBoundaryFileService, BoundaryFileService>();
        services.AddSingleton<IHeadlandFileService, HeadlandFileService>();
        services.AddSingleton<ITramLineFileService, TramLineFileService>();
        services.AddSingleton<FieldMarkerFileService>();

        // Steering Services (Wave 5)
        services.AddSingleton<IStanleySteeringService, StanleySteeringService>();
        services.AddSingleton<IPurePursuitService, PurePursuitService>();
        services.AddSingleton<ILookAheadDistanceService, LookAheadDistanceService>();
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
        services.AddSingleton<IDisplayFormatterService, DisplayFormatterService>();
        services.AddSingleton<IAudioNotificationService, AudioNotificationService>();

        // UI Services (Wave 10.5) - Panel Docking System
        services.AddSingleton<IPanelHostingService, PanelHostingService>();

        // Undo/Redo Service (Wave 8)
        services.AddSingleton<IUndoRedoService, UndoRedoService>();

        // Vehicle Configuration
        services.AddSingleton<VehicleConfiguration>();

        // ViewModels - Main
        services.AddSingleton<MainViewModel>();

        // ViewModels - Wave 10 Task Group 1: Field Operations Panels
        services.AddSingleton<FormGPSViewModel>(); // Main GPS view with overlay controls
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
