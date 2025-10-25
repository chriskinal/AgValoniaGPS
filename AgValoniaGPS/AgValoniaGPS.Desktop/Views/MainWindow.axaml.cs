using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.Desktop.Services;
using AgValoniaGPS.Desktop.Views.Controls.DockButtons;

namespace AgValoniaGPS.Desktop.Views;

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private readonly IPanelHostingService _panelHostingService;

    public MainWindow()
    {
        InitializeComponent();

        // Set DataContext from DI
        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
            _panelHostingService = App.Services.GetRequiredService<IPanelHostingService>();
        }
        else
        {
            throw new InvalidOperationException("App.Services is null. DI container not initialized.");
        }

        // Subscribe to ViewModel property changes if needed
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        // Subscribe to Loaded event to initialize PanelHostingService
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        // Initialize PanelHostingService with panel containers
        // Note: All three bars (Left, Right, Bottom) now have static icons only
        // Create hidden containers for Wave 10 panel hosting
        var leftPanelContainer = new StackPanel
        {
            IsVisible = false,
            Background = Avalonia.Media.Brushes.Transparent
        };

        var rightPanelContainer = new StackPanel
        {
            IsVisible = false,
            Background = Avalonia.Media.Brushes.Transparent
        };

        var bottomPanelContainer = new StackPanel
        {
            IsVisible = false,
            Background = Avalonia.Media.Brushes.Transparent,
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };

        _panelHostingService.Initialize(
            leftPanelContainer,   // Use hidden container instead of PanelLeft
            rightPanelContainer,  // Use hidden container instead of PanelRight
            bottomPanelContainer, // Use hidden container instead of PanelBottom
            PanelNavigation
        );

        // Register all Wave 10 panels with their default dock locations
        RegisterPanels();
    }

    /// <summary>
    /// Registers all 15 Wave 10 panels with the PanelHostingService.
    /// Sets default dock locations and initial visibility states as per Appendix C.
    /// </summary>
    private void RegisterPanels()
    {
        // Left Panel (5 panels)
        // FormQuickAB - Default visible
        var quickABButton = new FormQuickABButton();
        _panelHostingService.RegisterPanel("quickAB", PanelDockLocation.Left, quickABButton);

        var flagsButton = new FormFlagsButton();
        _panelHostingService.RegisterPanel("flags", PanelDockLocation.Left, flagsButton);

        var boundaryEditorButton = new FormBoundaryEditorButton();
        _panelHostingService.RegisterPanel("boundaryEditor", PanelDockLocation.Left, boundaryEditorButton);

        var fieldToolsButton = new FormFieldToolsButton();
        _panelHostingService.RegisterPanel("fieldTools", PanelDockLocation.Left, fieldToolsButton);

        var fieldFileManagerButton = new FormFieldFileManagerButton();
        _panelHostingService.RegisterPanel("fieldFileManager", PanelDockLocation.Left, fieldFileManagerButton);

        // Right Panel (7 panels)
        var tramLineButton = new FormTramLineButton();
        _panelHostingService.RegisterPanel("tramLine", PanelDockLocation.Right, tramLineButton);

        var steerButton = new FormSteerButton();
        _panelHostingService.RegisterPanel("steer", PanelDockLocation.Right, steerButton);

        var configButton = new FormConfigButton();
        _panelHostingService.RegisterPanel("config", PanelDockLocation.Right, configButton);

        var diagnosticsButton = new FormDiagnosticsButton();
        _panelHostingService.RegisterPanel("diagnostics", PanelDockLocation.Right, diagnosticsButton);

        var rollCorrectionButton = new FormRollCorrectionButton();
        _panelHostingService.RegisterPanel("rollCorrection", PanelDockLocation.Right, rollCorrectionButton);

        var vehicleConfigButton = new FormVehicleConfigButton();
        _panelHostingService.RegisterPanel("vehicleConfig", PanelDockLocation.Right, vehicleConfigButton);

        // FormCamera - Default visible
        var cameraButton = new FormCameraButton();
        _panelHostingService.RegisterPanel("camera", PanelDockLocation.Right, cameraButton);

        // Bottom Panel (2 panels)
        var fieldDataButton = new FormFieldDataButton();
        _panelHostingService.RegisterPanel("fieldData", PanelDockLocation.Bottom, fieldDataButton);

        // FormGPSData - Default visible
        var gpsDataButton = new FormGPSDataButton();
        _panelHostingService.RegisterPanel("gpsData", PanelDockLocation.Bottom, gpsDataButton);

        // Navigation Panel (1 panel)
        var navigationButton = new NavigationButton();
        _panelHostingService.RegisterPanel("navigation", PanelDockLocation.Navigation, navigationButton);

        // Show default-visible panels (as per Appendix C)
        // Default visibility: FormGPSData, FormQuickAB, FormCamera
        _panelHostingService.ShowPanel("gpsData");
        _panelHostingService.ShowPanel("quickAB");
        _panelHostingService.ShowPanel("camera");
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Handle property changes from ViewModel if needed
        // For now, this is a placeholder for future functionality
    }
}
