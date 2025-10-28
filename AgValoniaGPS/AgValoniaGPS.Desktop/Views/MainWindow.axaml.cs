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

    // Drag support for individual floating buttons
    private bool _isDraggingBtn1, _isDraggingBtn2, _isDraggingBtn3, _isDraggingBtn4;
    private bool _isDraggingBtn5, _isDraggingBtn6, _isDraggingBtn7;
    private bool _isPressedBtn1, _isPressedBtn2, _isPressedBtn3, _isPressedBtn4;
    private bool _isPressedBtn5, _isPressedBtn6, _isPressedBtn7;
    private Avalonia.Point _dragStartBtn1, _dragStartBtn2, _dragStartBtn3, _dragStartBtn4;
    private Avalonia.Point _dragStartBtn5, _dragStartBtn6, _dragStartBtn7;
    private const double DragThreshold = 5.0; // pixels to move before considering it a drag

    // Track previous window size for proportional resize
    private double _previousWindowWidth;
    private double _previousWindowHeight;

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
        // Initialize window size tracking for proportional button resize
        _previousWindowWidth = this.Bounds.Width;
        _previousWindowHeight = this.Bounds.Height;

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
    /// Reset all floating buttons to their home positions
    /// </summary>
    private void BtnHomeButtons_Click(object? sender, RoutedEventArgs e)
    {
        // Reset all button transforms to return them to original Margin positions
        FloatingBtn1.RenderTransform = null;
        FloatingBtn2.RenderTransform = null;
        FloatingBtn3.RenderTransform = null;
        FloatingBtn4.RenderTransform = null;
        FloatingBtn5.RenderTransform = null;
        FloatingBtn6.RenderTransform = null;
        FloatingBtn7.RenderTransform = null;
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

    // ==================== Individual Floating Button Drag Handlers ====================

    // Button 1 - Navigation
    private void FloatingBtn1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn1 = true;
            _isDraggingBtn1 = false;
            _dragStartBtn1 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn1_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn1 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn1.X;
            var deltaY = currentPoint.Y - _dragStartBtn1.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // If moved more than threshold, start dragging
            if (!_isDraggingBtn1 && distance > DragThreshold)
            {
                _isDraggingBtn1 = true;
            }

            if (_isDraggingBtn1)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn1 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn1_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }

        // If we were dragging, prevent the click
        if (_isDraggingBtn1)
        {
            e.Handled = true;
        }
        else if (_isPressedBtn1)
        {
            // Was a click, not a drag - trigger the button action
            BtnNavigation_Click(sender, new RoutedEventArgs());
        }

        _isPressedBtn1 = false;
        _isDraggingBtn1 = false;
    }

    // Button 2 - Tools
    private void FloatingBtn2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn2 = true;
            _isDraggingBtn2 = false;
            _dragStartBtn2 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn2_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn2 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn2.X;
            var deltaY = currentPoint.Y - _dragStartBtn2.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtn2 && distance > DragThreshold)
            {
                _isDraggingBtn2 = true;
            }

            if (_isDraggingBtn2)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn2 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn2_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtn2)
        {
            e.Handled = true;
        }
        else if (_isPressedBtn2)
        {
            BtnTools_Click(sender, new RoutedEventArgs());
        }
        _isPressedBtn2 = false;
        _isDraggingBtn2 = false;
    }

    // Button 3 - Configuration
    private void FloatingBtn3_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn3 = true;
            _isDraggingBtn3 = false;
            _dragStartBtn3 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn3_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn3 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn3.X;
            var deltaY = currentPoint.Y - _dragStartBtn3.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtn3 && distance > DragThreshold)
            {
                _isDraggingBtn3 = true;
            }

            if (_isDraggingBtn3)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn3 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn3_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtn3)
        {
            e.Handled = true;
        }
        else if (_isPressedBtn3)
        {
            BtnConfiguration_Click(sender, new RoutedEventArgs());
        }
        _isPressedBtn3 = false;
        _isDraggingBtn3 = false;
    }

    // Button 4 - Field Manager
    private void FloatingBtn4_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn4 = true;
            _isDraggingBtn4 = false;
            _dragStartBtn4 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn4_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn4 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn4.X;
            var deltaY = currentPoint.Y - _dragStartBtn4.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtn4 && distance > DragThreshold)
            {
                _isDraggingBtn4 = true;
            }

            if (_isDraggingBtn4)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn4 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn4_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtn4) e.Handled = true;
        _isPressedBtn4 = false;
        _isDraggingBtn4 = false;
    }

    // Button 5 - Field Tools
    private void FloatingBtn5_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn5 = true;
            _isDraggingBtn5 = false;
            _dragStartBtn5 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn5_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn5 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn5.X;
            var deltaY = currentPoint.Y - _dragStartBtn5.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtn5 && distance > DragThreshold)
            {
                _isDraggingBtn5 = true;
            }

            if (_isDraggingBtn5)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn5 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn5_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtn5) e.Handled = true;
        _isPressedBtn5 = false;
        _isDraggingBtn5 = false;
    }

    // Button 6 - Auto Steer Config
    private void FloatingBtn6_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn6 = true;
            _isDraggingBtn6 = false;
            _dragStartBtn6 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn6_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn6 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn6.X;
            var deltaY = currentPoint.Y - _dragStartBtn6.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtn6 && distance > DragThreshold)
            {
                _isDraggingBtn6 = true;
            }

            if (_isDraggingBtn6)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn6 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn6_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtn6) e.Handled = true;
        _isPressedBtn6 = false;
        _isDraggingBtn6 = false;
    }

    // Button 7 - Data I/O
    private void FloatingBtn7_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtn7 = true;
            _isDraggingBtn7 = false;
            _dragStartBtn7 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtn7_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtn7 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtn7.X;
            var deltaY = currentPoint.Y - _dragStartBtn7.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtn7 && distance > DragThreshold)
            {
                _isDraggingBtn7 = true;
            }

            if (_isDraggingBtn7)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtn7 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtn7_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtn7) e.Handled = true;
        _isPressedBtn7 = false;
        _isDraggingBtn7 = false;
    }

    /// <summary>
    /// Handle window resize to move buttons proportionally
    /// </summary>
    private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Skip if previous size not initialized
        if (_previousWindowWidth == 0 || _previousWindowHeight == 0) return;

        var newWidth = this.Bounds.Width;
        var newHeight = this.Bounds.Height;
        var statusBarHeight = 60.0;
        var availableHeight = newHeight - statusBarHeight;
        var previousAvailableHeight = _previousWindowHeight - statusBarHeight;

        // Calculate size change ratios
        var widthRatio = newWidth / _previousWindowWidth;
        var heightRatio = availableHeight / previousAvailableHeight;

        // Move all floating buttons proportionally
        MoveButtonProportionally(FloatingBtn1, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn2, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn3, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn4, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn5, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn6, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn7, widthRatio, heightRatio);

        // Update previous size
        _previousWindowWidth = newWidth;
        _previousWindowHeight = newHeight;
    }

    /// <summary>
    /// Move button proportionally based on window resize ratio
    /// </summary>
    private void MoveButtonProportionally(Border button, double widthRatio, double heightRatio)
    {
        if (button == null) return;

        // Get button's base position from Margin
        var margin = button.Margin;
        var baseX = margin.Left;
        var baseY = margin.Top;

        // Get current transform (if any)
        var transform = button.RenderTransform as Avalonia.Media.TranslateTransform;
        var currentOffsetX = transform?.X ?? 0;
        var currentOffsetY = transform?.Y ?? 0;

        // Calculate current absolute position
        var currentX = baseX + currentOffsetX;
        var currentY = baseY + currentOffsetY;

        // Calculate new position based on ratio
        var newX = currentX * widthRatio;
        var newY = currentY * heightRatio;

        // Calculate new offset
        var newOffsetX = newX - baseX;
        var newOffsetY = newY - baseY;

        // Constrain to window bounds
        var constrained = GetConstrainedTransform(button, newOffsetX, newOffsetY);
        button.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);
    }

    /// <summary>
    /// Constrain a button's position to stay within the window bounds
    /// </summary>
    private void ConstrainButtonPosition(Border button)
    {
        if (button == null) return;

        // Get the button's current transform
        var transform = button.RenderTransform as Avalonia.Media.TranslateTransform;
        if (transform == null) return; // Button hasn't been moved yet

        // Get button's base position from Margin
        var margin = button.Margin;
        var baseX = margin.Left;
        var baseY = margin.Top;

        // Calculate actual position with transform
        var actualX = baseX + transform.X;
        var actualY = baseY + transform.Y;

        // Get window content bounds (accounting for status bar at top)
        var windowWidth = this.Bounds.Width;
        var windowHeight = this.Bounds.Height;
        var statusBarHeight = 60.0; // Top status bar height
        var availableHeight = windowHeight - statusBarHeight;

        // Constrain X position
        var minX = 8.0; // Minimum margin from left
        var maxX = windowWidth - button.Width - 8.0; // Maximum position (right edge)

        var constrainedX = Math.Max(minX, Math.Min(maxX, actualX));

        // Constrain Y position
        var minY = 8.0; // Minimum margin from top (within content area)
        var maxY = availableHeight - button.Height - 8.0; // Maximum position (bottom edge)

        var constrainedY = Math.Max(minY, Math.Min(maxY, actualY));

        // Calculate new transform based on constrained position
        var newTransformX = constrainedX - baseX;
        var newTransformY = constrainedY - baseY;

        // Only update if position changed
        if (Math.Abs(newTransformX - transform.X) > 0.1 || Math.Abs(newTransformY - transform.Y) > 0.1)
        {
            button.RenderTransform = new Avalonia.Media.TranslateTransform(newTransformX, newTransformY);
        }
    }

    /// <summary>
    /// Calculate constrained transform offsets during dragging
    /// </summary>
    private (double X, double Y) GetConstrainedTransform(Border button, double newOffsetX, double newOffsetY)
    {
        if (button == null) return (newOffsetX, newOffsetY);

        // Get button's base position from Margin
        var margin = button.Margin;
        var baseX = margin.Left;
        var baseY = margin.Top;

        // Calculate what the actual position would be
        var actualX = baseX + newOffsetX;
        var actualY = baseY + newOffsetY;

        // Get window content bounds
        var windowWidth = this.Bounds.Width;
        var windowHeight = this.Bounds.Height;
        var statusBarHeight = 60.0;
        var availableHeight = windowHeight - statusBarHeight;

        // Constrain X position
        var minX = 8.0;
        var maxX = windowWidth - button.Width - 8.0;
        var constrainedX = Math.Max(minX, Math.Min(maxX, actualX));

        // Constrain Y position
        var minY = 8.0;
        var maxY = availableHeight - button.Height - 8.0;
        var constrainedY = Math.Max(minY, Math.Min(maxY, actualY));

        // Return constrained offsets
        return (constrainedX - baseX, constrainedY - baseY);
    }
}
