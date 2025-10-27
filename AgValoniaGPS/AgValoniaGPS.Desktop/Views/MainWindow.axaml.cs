using System;
using Avalonia.Controls;
using Avalonia.Input;
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

    // Drag support for Navigation panel
    private bool _isDraggingNavigationPanel;
    private Avalonia.Point _dragStartPointNavigation;

    // Drag support for Tools panel
    private bool _isDraggingToolsPanel;
    private Avalonia.Point _dragStartPoint;

    // Drag support for Configuration panel
    private bool _isDraggingConfigurationPanel;
    private Avalonia.Point _dragStartPointConfiguration;

    // Drag support for Field Tools panel
    private bool _isDraggingFieldToolsPanel;
    private Avalonia.Point _dragStartPointFieldTools;

    // Drag support for Floating Toolbar
    private bool _isDraggingFloatingToolbar;
    private Avalonia.Point _dragStartPointFloatingToolbar;

    // Rotation state for Floating Toolbar (0, 90, 180, 270 degrees)
    private int _toolbarRotation = 0;

    // Track toolbar position separately from transform
    private double _toolbarOffsetX = 0;
    private double _toolbarOffsetY = 0;

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

        // Subscribe to SizeChanged event to keep toolbar in bounds
        this.SizeChanged += MainWindow_SizeChanged;
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

    /// <summary>
    /// Toggle Navigation Panel visibility when button #1 is clicked
    /// </summary>
    private void BtnNavigation_Click(object? sender, RoutedEventArgs e)
    {
        // If Navigation panel is already open, just close it
        if (PanelNavigation.IsVisible)
        {
            PanelNavigation.IsVisible = false;
        }
        else
        {
            // Close all other panels first
            CloseAllOverlayPanels();
            // Open Navigation panel
            PanelNavigation.IsVisible = true;
        }
    }

    /// <summary>
    /// Handle pointer pressed on Navigation panel header to start dragging
    /// </summary>
    private void PanelNavigationHeader_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDraggingNavigationPanel = true;
            // Store the initial mouse position relative to the window
            _dragStartPointNavigation = e.GetPosition(this);
        }
    }

    /// <summary>
    /// Handle pointer moved to drag the Navigation panel
    /// </summary>
    private void PanelNavigationHeader_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingNavigationPanel)
        {
            // Get current mouse position relative to the window
            var currentPoint = e.GetPosition(this);

            // Calculate offset from the start point
            var offsetX = currentPoint.X - _dragStartPointNavigation.X;
            var offsetY = currentPoint.Y - _dragStartPointNavigation.Y;

            // Get existing transform or create new one
            var existingTransform = PanelNavigation.RenderTransform as Avalonia.Media.TranslateTransform;
            var currentOffsetX = existingTransform?.X ?? 0;
            var currentOffsetY = existingTransform?.Y ?? 0;

            // Apply cumulative offset
            PanelNavigation.RenderTransform = new Avalonia.Media.TranslateTransform(
                currentOffsetX + offsetX,
                currentOffsetY + offsetY
            );

            // Update drag start point for next move
            _dragStartPointNavigation = currentPoint;
        }
    }

    /// <summary>
    /// Handle pointer released to stop dragging Navigation panel
    /// </summary>
    private void PanelNavigationHeader_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingNavigationPanel = false;
    }

    /// <summary>
    /// Close all overlay panels
    /// </summary>
    private void CloseAllOverlayPanels()
    {
        PanelNavigation.IsVisible = false;
        PanelTools.IsVisible = false;
        PanelConfiguration.IsVisible = false;
        PanelFieldTools.IsVisible = false;
    }

    /// <summary>
    /// Toggle Tools Panel visibility when button #2 is clicked
    /// </summary>
    private void BtnTools_Click(object? sender, RoutedEventArgs e)
    {
        // If Tools panel is already open, just close it
        if (PanelTools.IsVisible)
        {
            PanelTools.IsVisible = false;
        }
        else
        {
            // Close all other panels first
            CloseAllOverlayPanels();
            // Open Tools panel
            PanelTools.IsVisible = true;
        }
    }

    /// <summary>
    /// Button #4: Field Manager menu
    /// </summary>
    private void BtnFieldManager_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement Field Manager menu (later wave)
        // Will show field management options (New Field, Open Field, etc.)
    }

    /// <summary>
    /// Button #6: Auto Steer Configuration panel
    /// </summary>
    private void BtnAutoSteerConfig_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement Auto Steer Configuration panel (later wave)
        // Will show dockable panel with steer settings, gains, etc.
    }

    /// <summary>
    /// Button #7: Data I/O Status panel
    /// </summary>
    private void BtnDataIO_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement Data I/O Status panel (later wave)
        // Will show AgIO connection status, module status, RTCM data, etc.
    }

    /// <summary>
    /// Handle pointer pressed on Tools panel header to start dragging
    /// </summary>
    private void PanelToolsHeader_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDraggingToolsPanel = true;
            // Store the initial mouse position relative to the window
            _dragStartPoint = e.GetPosition(this);
        }
    }

    /// <summary>
    /// Handle pointer moved to drag the Tools panel
    /// </summary>
    private void PanelToolsHeader_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingToolsPanel)
        {
            // Get current mouse position relative to the window
            var currentPoint = e.GetPosition(this);

            // Calculate offset from the start point
            var offsetX = currentPoint.X - _dragStartPoint.X;
            var offsetY = currentPoint.Y - _dragStartPoint.Y;

            // Get existing transform or create new one
            var existingTransform = PanelTools.RenderTransform as Avalonia.Media.TranslateTransform;
            var currentOffsetX = existingTransform?.X ?? 0;
            var currentOffsetY = existingTransform?.Y ?? 0;

            // Apply cumulative offset
            PanelTools.RenderTransform = new Avalonia.Media.TranslateTransform(
                currentOffsetX + offsetX,
                currentOffsetY + offsetY
            );

            // Update drag start point for next move
            _dragStartPoint = currentPoint;
        }
    }

    /// <summary>
    /// Handle pointer released to stop dragging
    /// </summary>
    private void PanelToolsHeader_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingToolsPanel = false;
    }

    /// <summary>
    /// Toggle Configuration Panel visibility when button #3 is clicked
    /// </summary>
    private void BtnConfiguration_Click(object? sender, RoutedEventArgs e)
    {
        // If Configuration panel is already open, just close it
        if (PanelConfiguration.IsVisible)
        {
            PanelConfiguration.IsVisible = false;
        }
        else
        {
            // Close all other panels first
            CloseAllOverlayPanels();
            // Open Configuration panel
            PanelConfiguration.IsVisible = true;
        }
    }

    /// <summary>
    /// Handle pointer pressed on Configuration panel header to start dragging
    /// </summary>
    private void PanelConfigurationHeader_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDraggingConfigurationPanel = true;
            // Store the initial mouse position relative to the window
            _dragStartPointConfiguration = e.GetPosition(this);
        }
    }

    /// <summary>
    /// Handle pointer moved to drag the Configuration panel
    /// </summary>
    private void PanelConfigurationHeader_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingConfigurationPanel)
        {
            // Get current mouse position relative to the window
            var currentPoint = e.GetPosition(this);

            // Calculate offset from the start point
            var offsetX = currentPoint.X - _dragStartPointConfiguration.X;
            var offsetY = currentPoint.Y - _dragStartPointConfiguration.Y;

            // Get existing transform or create new one
            var existingTransform = PanelConfiguration.RenderTransform as Avalonia.Media.TranslateTransform;
            var currentOffsetX = existingTransform?.X ?? 0;
            var currentOffsetY = existingTransform?.Y ?? 0;

            // Apply cumulative offset
            PanelConfiguration.RenderTransform = new Avalonia.Media.TranslateTransform(
                currentOffsetX + offsetX,
                currentOffsetY + offsetY
            );

            // Update drag start point for next move
            _dragStartPointConfiguration = currentPoint;
        }
    }

    /// <summary>
    /// Handle pointer released to stop dragging Configuration panel
    /// </summary>
    private void PanelConfigurationHeader_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingConfigurationPanel = false;
    }

    /// <summary>
    /// Toggle Field Tools Panel visibility when button #5 is clicked
    /// </summary>
    private void BtnFieldTools_Click(object? sender, RoutedEventArgs e)
    {
        // If Field Tools panel is already open, just close it
        if (PanelFieldTools.IsVisible)
        {
            PanelFieldTools.IsVisible = false;
        }
        else
        {
            // Close all other panels first
            CloseAllOverlayPanels();
            // Open Field Tools panel
            PanelFieldTools.IsVisible = true;
        }
    }

    /// <summary>
    /// Handle pointer pressed on Field Tools panel header to start dragging
    /// </summary>
    private void PanelFieldToolsHeader_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDraggingFieldToolsPanel = true;
            // Store the initial mouse position relative to the window
            _dragStartPointFieldTools = e.GetPosition(this);
        }
    }

    /// <summary>
    /// Handle pointer moved to drag the Field Tools panel
    /// </summary>
    private void PanelFieldToolsHeader_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingFieldToolsPanel)
        {
            // Get current mouse position relative to the window
            var currentPoint = e.GetPosition(this);

            // Calculate offset from the start point
            var offsetX = currentPoint.X - _dragStartPointFieldTools.X;
            var offsetY = currentPoint.Y - _dragStartPointFieldTools.Y;

            // Get existing transform or create new one
            var existingTransform = PanelFieldTools.RenderTransform as Avalonia.Media.TranslateTransform;
            var currentOffsetX = existingTransform?.X ?? 0;
            var currentOffsetY = existingTransform?.Y ?? 0;

            // Apply cumulative offset
            PanelFieldTools.RenderTransform = new Avalonia.Media.TranslateTransform(
                currentOffsetX + offsetX,
                currentOffsetY + offsetY
            );

            // Update drag start point for next move
            _dragStartPointFieldTools = currentPoint;
        }
    }

    /// <summary>
    /// Handle pointer released to stop dragging Field Tools panel
    /// </summary>
    private void PanelFieldToolsHeader_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingFieldToolsPanel = false;
    }

    /// <summary>
    /// Handle pointer pressed on Floating Toolbar header to start dragging
    /// </summary>
    private void FloatingToolbarHeader_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDraggingFloatingToolbar = true;
            _dragStartPointFloatingToolbar = e.GetPosition(this);
        }
    }

    /// <summary>
    /// Handle pointer moved to drag the Floating Toolbar
    /// </summary>
    private void FloatingToolbarHeader_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingFloatingToolbar)
        {
            var currentPoint = e.GetPosition(this);
            var offsetX = currentPoint.X - _dragStartPointFloatingToolbar.X;
            var offsetY = currentPoint.Y - _dragStartPointFloatingToolbar.Y;

            _toolbarOffsetX += offsetX;
            _toolbarOffsetY += offsetY;

            _dragStartPointFloatingToolbar = currentPoint;

            // Rebuild combined transform
            UpdateToolbarTransform();
        }
    }

    /// <summary>
    /// Handle pointer released to stop dragging Floating Toolbar
    /// </summary>
    private void FloatingToolbarHeader_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingFloatingToolbar = false;
    }

    /// <summary>
    /// Toggle the floating toolbar between vertical and horizontal
    /// </summary>
    private void RotateToolbar_Click(object? sender, RoutedEventArgs e)
    {
        // Toggle between vertical (0) and horizontal (90)
        _toolbarRotation = _toolbarRotation == 0 ? 90 : 0;

        // Update layout based on rotation
        UpdateToolbarLayout();
    }

    /// <summary>
    /// Update the toolbar layout based on current rotation state
    /// Vertical (0°): Tall and narrow
    /// Horizontal (90°): Wide and short
    /// </summary>
    private void UpdateToolbarLayout()
    {
        // Change layout orientation and dimensions based on rotation
        if (_toolbarRotation == 0)
        {
            // Vertical layout: StackPanel with vertical orientation
            PanelLeft.Orientation = Avalonia.Layout.Orientation.Vertical;
            FloatingToolbar.Width = 110;
            FloatingToolbar.Height = double.NaN; // Auto height
            FloatingToolbar.MaxHeight = 600;
            FloatingToolbar.MaxWidth = double.PositiveInfinity;
        }
        else
        {
            // Horizontal layout: StackPanel with horizontal orientation
            PanelLeft.Orientation = Avalonia.Layout.Orientation.Horizontal;
            FloatingToolbar.Width = double.NaN; // Auto width
            FloatingToolbar.Height = double.NaN; // Auto height
            FloatingToolbar.MaxWidth = 700;
            FloatingToolbar.MaxHeight = 100;
        }

        // Update the combined transform
        UpdateToolbarTransform();
    }

    /// <summary>
    /// Apply combined translation transform to toolbar (no visual rotation)
    /// </summary>
    private void UpdateToolbarTransform()
    {
        // Only apply translation, not rotation
        // Rotation is handled by changing the StackPanel orientation
        FloatingToolbar.RenderTransform = new Avalonia.Media.TranslateTransform(_toolbarOffsetX, _toolbarOffsetY);
    }

    /// <summary>
    /// Handle window resize to keep toolbar within visible bounds
    /// </summary>
    private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Only check if toolbar exists
        if (FloatingToolbar == null)
            return;

        // Get window dimensions
        var windowWidth = e.NewSize.Width;
        var windowHeight = e.NewSize.Height;

        // Get toolbar dimensions (accounting for rotation)
        var toolbarWidth = FloatingToolbar.Bounds.Width;
        var toolbarHeight = FloatingToolbar.Bounds.Height;

        // Calculate toolbar's current position (initial margin + transform offset)
        var currentX = 8 + _toolbarOffsetX; // 8 is the initial margin
        var currentY = 8 + _toolbarOffsetY;

        // Check if toolbar is out of bounds and adjust
        var maxX = windowWidth - toolbarWidth - 8; // Leave 8px margin
        var maxY = windowHeight - toolbarHeight - 8;

        var needsUpdate = false;

        if (currentX < 8)
        {
            _toolbarOffsetX = 0;
            needsUpdate = true;
        }
        else if (currentX > maxX)
        {
            _toolbarOffsetX = maxX - 8;
            needsUpdate = true;
        }

        if (currentY < 8)
        {
            _toolbarOffsetY = 0;
            needsUpdate = true;
        }
        else if (currentY > maxY)
        {
            _toolbarOffsetY = maxY - 8;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            UpdateToolbarTransform();
        }
    }
}
