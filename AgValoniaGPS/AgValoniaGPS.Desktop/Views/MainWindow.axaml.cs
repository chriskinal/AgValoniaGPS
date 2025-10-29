using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.Desktop.Services;
using AgValoniaGPS.Desktop.Views.Controls.DockButtons;
using AgValoniaGPS.Services.Profile;
using AgValoniaGPS.Models.Profile;

namespace AgValoniaGPS.Desktop.Views;

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private readonly IPanelHostingService _panelHostingService;
    private readonly IProfileManagementService _profileService;
    private readonly IProfileProvider<UserProfile> _userProfileProvider;

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

    // Drag support for individual floating buttons - Left Panel
    private bool _isDraggingBtn1, _isDraggingBtn2, _isDraggingBtn3, _isDraggingBtn4;
    private bool _isDraggingBtn5, _isDraggingBtn6, _isDraggingBtn7;
    private bool _isPressedBtn1, _isPressedBtn2, _isPressedBtn3, _isPressedBtn4;
    private bool _isPressedBtn5, _isPressedBtn6, _isPressedBtn7;
    private Avalonia.Point _dragStartBtn1, _dragStartBtn2, _dragStartBtn3, _dragStartBtn4;
    private Avalonia.Point _dragStartBtn5, _dragStartBtn6, _dragStartBtn7;

    // Drag support for Right Panel floating buttons (R1-R9)
    private bool _isDraggingBtnR1, _isDraggingBtnR2, _isDraggingBtnR3, _isDraggingBtnR4, _isDraggingBtnR5;
    private bool _isDraggingBtnR6, _isDraggingBtnR7, _isDraggingBtnR8, _isDraggingBtnR9;
    private bool _isPressedBtnR1, _isPressedBtnR2, _isPressedBtnR3, _isPressedBtnR4, _isPressedBtnR5;
    private bool _isPressedBtnR6, _isPressedBtnR7, _isPressedBtnR8, _isPressedBtnR9;
    private Avalonia.Point _dragStartBtnR1, _dragStartBtnR2, _dragStartBtnR3, _dragStartBtnR4, _dragStartBtnR5;
    private Avalonia.Point _dragStartBtnR6, _dragStartBtnR7, _dragStartBtnR8, _dragStartBtnR9;

    // Drag support for Bottom Panel floating buttons (B1-B11)
    private bool _isDraggingBtnB1, _isDraggingBtnB2, _isDraggingBtnB3, _isDraggingBtnB4, _isDraggingBtnB5, _isDraggingBtnB6;
    private bool _isDraggingBtnB7, _isDraggingBtnB8, _isDraggingBtnB9, _isDraggingBtnB10, _isDraggingBtnB11;
    private bool _isPressedBtnB1, _isPressedBtnB2, _isPressedBtnB3, _isPressedBtnB4, _isPressedBtnB5, _isPressedBtnB6;
    private bool _isPressedBtnB7, _isPressedBtnB8, _isPressedBtnB9, _isPressedBtnB10, _isPressedBtnB11;
    private Avalonia.Point _dragStartBtnB1, _dragStartBtnB2, _dragStartBtnB3, _dragStartBtnB4, _dragStartBtnB5, _dragStartBtnB6;
    private Avalonia.Point _dragStartBtnB7, _dragStartBtnB8, _dragStartBtnB9, _dragStartBtnB10, _dragStartBtnB11;

    private const double DragThreshold = 5.0; // pixels to move before considering it a drag

    // Track previous window size for proportional resize
    private double _previousWindowWidth;
    private double _previousWindowHeight;
    private bool _isWindowInitialized;
    private bool _hasLoadedPositions;

    public MainWindow()
    {
        InitializeComponent();

        // Set DataContext from DI
        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
            _panelHostingService = App.Services.GetRequiredService<IPanelHostingService>();
            _profileService = App.Services.GetRequiredService<IProfileManagementService>();
            _userProfileProvider = App.Services.GetRequiredService<IProfileProvider<UserProfile>>();
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

        // Subscribe to Opened event to load profile and window size BEFORE window is shown
        this.Opened += MainWindow_Opened;

        // Subscribe to Loaded event to initialize PanelHostingService
        this.Loaded += MainWindow_Loaded;

        // Subscribe to Closing event to save window size
        this.Closing += MainWindow_Closing;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        // Load profile and apply window size as soon as window opens
        try
        {
            var result = await _profileService.SwitchUserProfileAsync("Default");
            if (result.Success)
            {
                // Apply saved window size from profile
                var profile = _profileService.GetCurrentUserProfile();
                if (profile != null)
                {
                    var displayPrefs = profile.Preferences.DisplayPreferences;

                    if (!displayPrefs.WindowMaximized)
                    {
                        this.WindowState = Avalonia.Controls.WindowState.Normal;
                        this.Width = displayPrefs.WindowWidth;
                        this.Height = displayPrefs.WindowHeight;
                    }
                    else
                    {
                        this.WindowState = Avalonia.Controls.WindowState.Maximized;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainWindow] Error loading profile: {ex.Message}");
        }
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        // Don't initialize size tracking yet - let Window_SizeChanged handle it after layout
        // This prevents a bogus resize event with incorrect initial bounds
        _isWindowInitialized = false;
        _hasLoadedPositions = false;

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

        // Load button positions after window is fully loaded
        // Profile and window size were already loaded in constructor
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            // Brief delay for layout to complete
            await System.Threading.Tasks.Task.Delay(100);

            // Load button positions
            LoadButtonPositions();
            _hasLoadedPositions = true;
        }, Avalonia.Threading.DispatcherPriority.Background);
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
    /// Reset all floating buttons to their home positions (Left, Right, and Bottom panels)
    /// </summary>
    private void BtnHomeButtons_Click(object? sender, RoutedEventArgs e)
    {
        ResetButtonPositions("Left");
        ResetButtonPositions("Right");
        ResetButtonPositions("Bottom");
        SaveButtonPositions();
    }

    /// <summary>
    /// Reset floating buttons to their home positions for a specific panel
    /// </summary>
    /// <param name="panel">"Left", "Right", or "Bottom"</param>
    private void ResetButtonPositions(string panel)
    {
        Console.WriteLine($"[ButtonPosition] Resetting {panel} panel buttons to home positions");

        switch (panel)
        {
            case "Left":
                FloatingBtn1.RenderTransform = null;
                FloatingBtn2.RenderTransform = null;
                FloatingBtn3.RenderTransform = null;
                FloatingBtn4.RenderTransform = null;
                FloatingBtn5.RenderTransform = null;
                FloatingBtn6.RenderTransform = null;
                FloatingBtn7.RenderTransform = null;
                break;

            case "Right":
                // Right panel buttons - aligned vertically on right edge, evenly spaced
                // Position relative to window width, R9 at top (Y=8), R1 at bottom
                // All buttons have Margin="8,8,0,0", so RenderTransform is offset from that position
                // Gap from edge = 8px (same as left panel)
                {
                    double rightX = this.Bounds.Width - 8 - 70 - 8; // gap(8) + button(70) + margin(8) = Width - 86
                    FloatingBtnR1.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 624); // Y offset = 624, visual Y = 8 + 624 = 632
                    FloatingBtnR2.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 546); // Y offset = 546, visual Y = 8 + 546 = 554
                    FloatingBtnR3.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 468); // Y offset = 468, visual Y = 8 + 468 = 476
                    FloatingBtnR4.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 390); // Y offset = 390, visual Y = 8 + 390 = 398
                    FloatingBtnR5.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 312); // Y offset = 312, visual Y = 8 + 312 = 320
                    FloatingBtnR6.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 234); // Y offset = 234, visual Y = 8 + 234 = 242
                    FloatingBtnR7.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 156); // Y offset = 156, visual Y = 8 + 156 = 164
                    FloatingBtnR8.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 78);  // Y offset = 78, visual Y = 8 + 78 = 86
                    FloatingBtnR9.RenderTransform = new Avalonia.Media.TranslateTransform(rightX, 0);   // Y offset = 0, visual Y = 8 + 0 = 8 (top)
                }
                break;

            case "Bottom":
                // Bottom panel buttons - centered horizontally on bottom edge, evenly spaced
                // Spacing: 78px between buttons (same as left panel: 70px button + 8px gap)
                // Position relative to window height, B11 at left, B1 at right
                // Buttons are in Grid.Row="1" which starts after 60px status bar
                // All buttons have Margin="8,8,0,0" within Grid.Row="1"
                // All buttons are now 70x70 (same as Right panel)
                // Gap from edge = 8px (same as left panel)
                {
                    double bottomY = this.Bounds.Height - 8 - 70 - 60 - 8; // gap(8) + button(70) + statusBar(60) + margin(8) = Height - 146

                    // Center the buttons horizontally
                    // Total width: 11 buttons × 70px + 10 gaps × 8px = 850px
                    double totalRowWidth = 11 * 70 + 10 * 8; // 850px
                    double startX = (this.Bounds.Width - totalRowWidth) / 2 - 8; // Subtract initial margin

                    // Position buttons with 78px spacing (70px button + 8px gap)
                    FloatingBtnB11.RenderTransform = new Avalonia.Media.TranslateTransform(startX, bottomY);
                    FloatingBtnB10.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 78, bottomY);
                    FloatingBtnB9.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 156, bottomY);
                    FloatingBtnB8.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 234, bottomY);
                    FloatingBtnB7.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 312, bottomY);
                    FloatingBtnB6.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 390, bottomY);
                    FloatingBtnB5.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 468, bottomY);
                    FloatingBtnB4.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 546, bottomY);
                    FloatingBtnB3.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 624, bottomY);
                    FloatingBtnB2.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 702, bottomY);
                    FloatingBtnB1.RenderTransform = new Avalonia.Media.TranslateTransform(startX + 780, bottomY);
                }
                break;
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

        // If we were dragging, prevent the click and save position
        if (_isDraggingBtn1)
        {
            e.Handled = true;
            SaveButtonPositions();
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
            SaveButtonPositions();
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
            SaveButtonPositions();
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
        if (_isDraggingBtn4)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
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
        if (_isDraggingBtn5)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
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
        if (_isDraggingBtn6)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
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
        if (_isDraggingBtn7)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtn7 = false;
        _isDraggingBtn7 = false;
    }

    // ===== RIGHT PANEL FLOATING BUTTONS (R1-R9) =====

    // Button R1 - Auto Steer
    private void FloatingBtnR1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR1 = true;
            _isDraggingBtnR1 = false;
            _dragStartBtnR1 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR1_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR1 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR1.X;
            var deltaY = currentPoint.Y - _dragStartBtnR1.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR1 && distance > DragThreshold)
            {
                _isDraggingBtnR1 = true;
            }

            if (_isDraggingBtnR1)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR1 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR1_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR1)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR1 = false;
        _isDraggingBtnR1 = false;
    }

    // Button R2 - Auto YouTurn
    private void FloatingBtnR2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR2 = true;
            _isDraggingBtnR2 = false;
            _dragStartBtnR2 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR2_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR2 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR2.X;
            var deltaY = currentPoint.Y - _dragStartBtnR2.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR2 && distance > DragThreshold)
            {
                _isDraggingBtnR2 = true;
            }

            if (_isDraggingBtnR2)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR2 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR2_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR2)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR2 = false;
        _isDraggingBtnR2 = false;
    }

    // Button R3 - Section Master Auto
    private void FloatingBtnR3_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR3 = true;
            _isDraggingBtnR3 = false;
            _dragStartBtnR3 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR3_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR3 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR3.X;
            var deltaY = currentPoint.Y - _dragStartBtnR3.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR3 && distance > DragThreshold)
            {
                _isDraggingBtnR3 = true;
            }

            if (_isDraggingBtnR3)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR3 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR3_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR3)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR3 = false;
        _isDraggingBtnR3 = false;
    }

    // Button R4 - Section Master Manual
    private void FloatingBtnR4_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR4 = true;
            _isDraggingBtnR4 = false;
            _dragStartBtnR4 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR4_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR4 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR4.X;
            var deltaY = currentPoint.Y - _dragStartBtnR4.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR4 && distance > DragThreshold)
            {
                _isDraggingBtnR4 = true;
            }

            if (_isDraggingBtnR4)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR4 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR4_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR4)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR4 = false;
        _isDraggingBtnR4 = false;
    }

    // Button R5 - Auto Track
    private void FloatingBtnR5_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR5 = true;
            _isDraggingBtnR5 = false;
            _dragStartBtnR5 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR5_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR5 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR5.X;
            var deltaY = currentPoint.Y - _dragStartBtnR5.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR5 && distance > DragThreshold)
            {
                _isDraggingBtnR5 = true;
            }

            if (_isDraggingBtnR5)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR5 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR5_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR5)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR5 = false;
        _isDraggingBtnR5 = false;
    }

    // Button R6 - Cycle Lines Backward
    private void FloatingBtnR6_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR6 = true;
            _isDraggingBtnR6 = false;
            _dragStartBtnR6 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR6_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR6 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR6.X;
            var deltaY = currentPoint.Y - _dragStartBtnR6.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR6 && distance > DragThreshold)
            {
                _isDraggingBtnR6 = true;
            }

            if (_isDraggingBtnR6)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR6 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR6_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR6)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR6 = false;
        _isDraggingBtnR6 = false;
    }

    // Button R7 - Cycle Lines Forward
    private void FloatingBtnR7_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR7 = true;
            _isDraggingBtnR7 = false;
            _dragStartBtnR7 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR7_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR7 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR7.X;
            var deltaY = currentPoint.Y - _dragStartBtnR7.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR7 && distance > DragThreshold)
            {
                _isDraggingBtnR7 = true;
            }

            if (_isDraggingBtnR7)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR7 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR7_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR7)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR7 = false;
        _isDraggingBtnR7 = false;
    }

    // Button R8 - Contour Mode
    private void FloatingBtnR8_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR8 = true;
            _isDraggingBtnR8 = false;
            _dragStartBtnR8 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR8_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR8 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR8.X;
            var deltaY = currentPoint.Y - _dragStartBtnR8.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR8 && distance > DragThreshold)
            {
                _isDraggingBtnR8 = true;
            }

            if (_isDraggingBtnR8)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR8 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR8_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR8)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR8 = false;
        _isDraggingBtnR8 = false;
    }

    // Button R9 - Contour Lock
    private void FloatingBtnR9_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnR9 = true;
            _isDraggingBtnR9 = false;
            _dragStartBtnR9 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnR9_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnR9 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnR9.X;
            var deltaY = currentPoint.Y - _dragStartBtnR9.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnR9 && distance > DragThreshold)
            {
                _isDraggingBtnR9 = true;
            }

            if (_isDraggingBtnR9)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                // Constrain to window bounds
                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnR9 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnR9_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnR9)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnR9 = false;
        _isDraggingBtnR9 = false;
    }

    // Bottom Panel floating button handlers (B1-B11)

    private void FloatingBtnB1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB1 = true;
            _isDraggingBtnB1 = false;
            _dragStartBtnB1 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB1_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB1 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB1.X;
            var deltaY = currentPoint.Y - _dragStartBtnB1.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB1 && distance > DragThreshold)
            {
                _isDraggingBtnB1 = true;
            }

            if (_isDraggingBtnB1)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB1 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB1_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB1)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB1 = false;
        _isDraggingBtnB1 = false;
    }

    private void FloatingBtnB2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB2 = true;
            _isDraggingBtnB2 = false;
            _dragStartBtnB2 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB2_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB2 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB2.X;
            var deltaY = currentPoint.Y - _dragStartBtnB2.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB2 && distance > DragThreshold)
            {
                _isDraggingBtnB2 = true;
            }

            if (_isDraggingBtnB2)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB2 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB2_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB2)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB2 = false;
        _isDraggingBtnB2 = false;
    }

    private void FloatingBtnB3_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB3 = true;
            _isDraggingBtnB3 = false;
            _dragStartBtnB3 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB3_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB3 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB3.X;
            var deltaY = currentPoint.Y - _dragStartBtnB3.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB3 && distance > DragThreshold)
            {
                _isDraggingBtnB3 = true;
            }

            if (_isDraggingBtnB3)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB3 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB3_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB3)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB3 = false;
        _isDraggingBtnB3 = false;
    }

    private void FloatingBtnB4_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB4 = true;
            _isDraggingBtnB4 = false;
            _dragStartBtnB4 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB4_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB4 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB4.X;
            var deltaY = currentPoint.Y - _dragStartBtnB4.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB4 && distance > DragThreshold)
            {
                _isDraggingBtnB4 = true;
            }

            if (_isDraggingBtnB4)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB4 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB4_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB4)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB4 = false;
        _isDraggingBtnB4 = false;
    }

    private void FloatingBtnB5_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB5 = true;
            _isDraggingBtnB5 = false;
            _dragStartBtnB5 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB5_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB5 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB5.X;
            var deltaY = currentPoint.Y - _dragStartBtnB5.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB5 && distance > DragThreshold)
            {
                _isDraggingBtnB5 = true;
            }

            if (_isDraggingBtnB5)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB5 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB5_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB5)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB5 = false;
        _isDraggingBtnB5 = false;
    }

    private void FloatingBtnB6_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB6 = true;
            _isDraggingBtnB6 = false;
            _dragStartBtnB6 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB6_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB6 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB6.X;
            var deltaY = currentPoint.Y - _dragStartBtnB6.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB6 && distance > DragThreshold)
            {
                _isDraggingBtnB6 = true;
            }

            if (_isDraggingBtnB6)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB6 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB6_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB6)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB6 = false;
        _isDraggingBtnB6 = false;
    }

    private void FloatingBtnB7_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB7 = true;
            _isDraggingBtnB7 = false;
            _dragStartBtnB7 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB7_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB7 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB7.X;
            var deltaY = currentPoint.Y - _dragStartBtnB7.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB7 && distance > DragThreshold)
            {
                _isDraggingBtnB7 = true;
            }

            if (_isDraggingBtnB7)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB7 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB7_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB7)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB7 = false;
        _isDraggingBtnB7 = false;
    }

    private void FloatingBtnB8_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB8 = true;
            _isDraggingBtnB8 = false;
            _dragStartBtnB8 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB8_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB8 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB8.X;
            var deltaY = currentPoint.Y - _dragStartBtnB8.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB8 && distance > DragThreshold)
            {
                _isDraggingBtnB8 = true;
            }

            if (_isDraggingBtnB8)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB8 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB8_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB8)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB8 = false;
        _isDraggingBtnB8 = false;
    }

    private void FloatingBtnB9_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB9 = true;
            _isDraggingBtnB9 = false;
            _dragStartBtnB9 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB9_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB9 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB9.X;
            var deltaY = currentPoint.Y - _dragStartBtnB9.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB9 && distance > DragThreshold)
            {
                _isDraggingBtnB9 = true;
            }

            if (_isDraggingBtnB9)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB9 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB9_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB9)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB9 = false;
        _isDraggingBtnB9 = false;
    }

    private void FloatingBtnB10_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB10 = true;
            _isDraggingBtnB10 = false;
            _dragStartBtnB10 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB10_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB10 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB10.X;
            var deltaY = currentPoint.Y - _dragStartBtnB10.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB10 && distance > DragThreshold)
            {
                _isDraggingBtnB10 = true;
            }

            if (_isDraggingBtnB10)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB10 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB10_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB10)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB10 = false;
        _isDraggingBtnB10 = false;
    }

    private void FloatingBtnB11_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Border border)
        {
            _isPressedBtnB11 = true;
            _isDraggingBtnB11 = false;
            _dragStartBtnB11 = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void FloatingBtnB11_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressedBtnB11 && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartBtnB11.X;
            var deltaY = currentPoint.Y - _dragStartBtnB11.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (!_isDraggingBtnB11 && distance > DragThreshold)
            {
                _isDraggingBtnB11 = true;
            }

            if (_isDraggingBtnB11)
            {
                var existingTransform = border.RenderTransform as Avalonia.Media.TranslateTransform;
                var currentOffsetX = existingTransform?.X ?? 0;
                var currentOffsetY = existingTransform?.Y ?? 0;

                var newOffsetX = currentOffsetX + deltaX;
                var newOffsetY = currentOffsetY + deltaY;

                var constrained = GetConstrainedTransform(border, newOffsetX, newOffsetY);
                border.RenderTransform = new Avalonia.Media.TranslateTransform(constrained.X, constrained.Y);

                _dragStartBtnB11 = currentPoint;
                e.Handled = true;
            }
        }
    }

    private void FloatingBtnB11_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border)
        {
            e.Pointer.Capture(null);
        }
        if (_isDraggingBtnB11)
        {
            e.Handled = true;
            SaveButtonPositions();
        }
        _isPressedBtnB11 = false;
        _isDraggingBtnB11 = false;
    }

    /// <summary>
    /// Handle window resize to move buttons proportionally
    /// </summary>
    private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var newWidth = this.Bounds.Width;
        var newHeight = this.Bounds.Height;

        // On first resize event after load, just record the size and skip resizing
        if (!_isWindowInitialized)
        {
            Console.WriteLine($"[ButtonPosition] Window initial size: {newWidth:F0}x{newHeight:F0}");
            _previousWindowWidth = newWidth;
            _previousWindowHeight = newHeight;
            _isWindowInitialized = true;
            return;
        }

        // Skip if size hasn't actually changed
        if (_previousWindowWidth == newWidth && _previousWindowHeight == newHeight)
        {
            Console.WriteLine($"[ButtonPosition] Size unchanged, skipping: {newWidth:F0}x{newHeight:F0}");
            return;
        }

        // IMPORTANT: Don't resize buttons until positions have been loaded
        // This prevents scaling saved positions during app startup
        if (!_hasLoadedPositions)
        {
            Console.WriteLine($"[ButtonPosition] Skipping resize before load: {_previousWindowWidth:F0}x{_previousWindowHeight:F0} -> {newWidth:F0}x{newHeight:F0}");
            _previousWindowWidth = newWidth;
            _previousWindowHeight = newHeight;
            return;
        }

        var statusBarHeight = 60.0;
        var availableHeight = newHeight - statusBarHeight;
        var previousAvailableHeight = _previousWindowHeight - statusBarHeight;

        // Calculate size change ratios
        var widthRatio = newWidth / _previousWindowWidth;
        var heightRatio = availableHeight / previousAvailableHeight;

        Console.WriteLine($"[ButtonPosition] Window resized: {_previousWindowWidth:F0}x{_previousWindowHeight:F0} -> {newWidth:F0}x{newHeight:F0} (ratios: {widthRatio:F3}, {heightRatio:F3})");

        // Move all floating buttons proportionally (Left panel)
        MoveButtonProportionally(FloatingBtn1, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn2, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn3, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn4, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn5, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn6, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtn7, widthRatio, heightRatio);

        // Move Right panel buttons proportionally
        MoveButtonProportionally(FloatingBtnR1, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR2, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR3, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR4, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR5, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR6, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR7, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR8, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnR9, widthRatio, heightRatio);

        // Move Bottom panel buttons proportionally
        MoveButtonProportionally(FloatingBtnB1, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB2, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB3, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB4, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB5, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB6, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB7, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB8, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB9, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB10, widthRatio, heightRatio);
        MoveButtonProportionally(FloatingBtnB11, widthRatio, heightRatio);

        // Save button positions after resize
        SaveButtonPositions();

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

    /// <summary>
    /// Load button positions from the current user profile
    /// </summary>
    private void LoadButtonPositions()
    {
        try
        {
            var currentUserProfile = _profileService.GetCurrentUserProfile();
            if (currentUserProfile == null)
            {
                Console.WriteLine("[ButtonPosition] No current user profile found");
                return;
            }

            var buttonPositions = currentUserProfile.Preferences.DisplayPreferences.ButtonPositions;
            if (buttonPositions == null || buttonPositions.Count == 0)
            {
                Console.WriteLine("[ButtonPosition] No saved button positions found");
                return;
            }

            Console.WriteLine($"[ButtonPosition] Loading {buttonPositions.Count} button positions at WindowSize: {this.Bounds.Width:F0}x{this.Bounds.Height:F0}");

            // Map button IDs to Border controls
            var buttonMap = new System.Collections.Generic.Dictionary<string, Border>
            {
                // Left panel buttons
                { "FloatingBtn1", FloatingBtn1 },
                { "FloatingBtn2", FloatingBtn2 },
                { "FloatingBtn3", FloatingBtn3 },
                { "FloatingBtn4", FloatingBtn4 },
                { "FloatingBtn5", FloatingBtn5 },
                { "FloatingBtn6", FloatingBtn6 },
                { "FloatingBtn7", FloatingBtn7 },
                // Right panel buttons
                { "FloatingBtnR1", FloatingBtnR1 },
                { "FloatingBtnR2", FloatingBtnR2 },
                { "FloatingBtnR3", FloatingBtnR3 },
                { "FloatingBtnR4", FloatingBtnR4 },
                { "FloatingBtnR5", FloatingBtnR5 },
                { "FloatingBtnR6", FloatingBtnR6 },
                { "FloatingBtnR7", FloatingBtnR7 },
                { "FloatingBtnR8", FloatingBtnR8 },
                { "FloatingBtnR9", FloatingBtnR9 },
                // Bottom panel buttons
                { "FloatingBtnB1", FloatingBtnB1 },
                { "FloatingBtnB2", FloatingBtnB2 },
                { "FloatingBtnB3", FloatingBtnB3 },
                { "FloatingBtnB4", FloatingBtnB4 },
                { "FloatingBtnB5", FloatingBtnB5 },
                { "FloatingBtnB6", FloatingBtnB6 },
                { "FloatingBtnB7", FloatingBtnB7 },
                { "FloatingBtnB8", FloatingBtnB8 },
                { "FloatingBtnB9", FloatingBtnB9 },
                { "FloatingBtnB10", FloatingBtnB10 },
                { "FloatingBtnB11", FloatingBtnB11 }
            };

            // Apply saved positions to buttons immediately (we're already on UI thread via Dispatcher)
            foreach (var position in buttonPositions)
            {
                if (buttonMap.TryGetValue(position.ButtonId, out var button))
                {
                    button.RenderTransform = new Avalonia.Media.TranslateTransform(position.OffsetX, position.OffsetY);
                    button.IsVisible = position.IsVisible;
                    Console.WriteLine($"[ButtonPosition] Applied {position.ButtonId}: X={position.OffsetX:F2}, Y={position.OffsetY:F2}");

                    // Verify it was applied
                    var transform = button.RenderTransform as Avalonia.Media.TranslateTransform;
                    Console.WriteLine($"[ButtonPosition] Verified {position.ButtonId}: X={transform?.X:F2}, Y={transform?.Y:F2}");
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash - buttons will just use default positions
            Console.WriteLine($"[ButtonPosition] Error loading button positions: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error loading button positions: {ex.Message}");
        }
    }

    /// <summary>
    /// Save current button positions to the user profile
    /// </summary>
    private async void SaveButtonPositions()
    {
        try
        {
            var currentUserProfile = _profileService.GetCurrentUserProfile();
            if (currentUserProfile == null) return;

            // Get current positions from all buttons
            var buttons = new[]
            {
                // Left panel buttons
                (Id: "FloatingBtn1", Button: FloatingBtn1),
                (Id: "FloatingBtn2", Button: FloatingBtn2),
                (Id: "FloatingBtn3", Button: FloatingBtn3),
                (Id: "FloatingBtn4", Button: FloatingBtn4),
                (Id: "FloatingBtn5", Button: FloatingBtn5),
                (Id: "FloatingBtn6", Button: FloatingBtn6),
                (Id: "FloatingBtn7", Button: FloatingBtn7),
                // Right panel buttons
                (Id: "FloatingBtnR1", Button: FloatingBtnR1),
                (Id: "FloatingBtnR2", Button: FloatingBtnR2),
                (Id: "FloatingBtnR3", Button: FloatingBtnR3),
                (Id: "FloatingBtnR4", Button: FloatingBtnR4),
                (Id: "FloatingBtnR5", Button: FloatingBtnR5),
                (Id: "FloatingBtnR6", Button: FloatingBtnR6),
                (Id: "FloatingBtnR7", Button: FloatingBtnR7),
                (Id: "FloatingBtnR8", Button: FloatingBtnR8),
                (Id: "FloatingBtnR9", Button: FloatingBtnR9),
                // Bottom panel buttons
                (Id: "FloatingBtnB1", Button: FloatingBtnB1),
                (Id: "FloatingBtnB2", Button: FloatingBtnB2),
                (Id: "FloatingBtnB3", Button: FloatingBtnB3),
                (Id: "FloatingBtnB4", Button: FloatingBtnB4),
                (Id: "FloatingBtnB5", Button: FloatingBtnB5),
                (Id: "FloatingBtnB6", Button: FloatingBtnB6),
                (Id: "FloatingBtnB7", Button: FloatingBtnB7),
                (Id: "FloatingBtnB8", Button: FloatingBtnB8),
                (Id: "FloatingBtnB9", Button: FloatingBtnB9),
                (Id: "FloatingBtnB10", Button: FloatingBtnB10),
                (Id: "FloatingBtnB11", Button: FloatingBtnB11)
            };

            var buttonPositions = new System.Collections.Generic.List<ButtonPosition>();

            Console.WriteLine("[ButtonPosition] Saving button positions:");
            foreach (var (id, button) in buttons)
            {
                var transform = button.RenderTransform as Avalonia.Media.TranslateTransform;
                var offsetX = transform?.X ?? 0;
                var offsetY = transform?.Y ?? 0;

                buttonPositions.Add(new ButtonPosition
                {
                    ButtonId = id,
                    OffsetX = offsetX,
                    OffsetY = offsetY,
                    IsVisible = button.IsVisible
                });

                Console.WriteLine($"[ButtonPosition]   {id}: X={offsetX:F2}, Y={offsetY:F2}");
            }

            // Update profile and save
            currentUserProfile.Preferences.DisplayPreferences.ButtonPositions = buttonPositions;
            var saved = await _userProfileProvider.SaveAsync(currentUserProfile);
            Console.WriteLine($"[ButtonPosition] Save result: {saved}");
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            Console.WriteLine($"[ButtonPosition] Error saving: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error saving button positions: {ex.Message}");
        }
    }

    /// <summary>
    /// Save window size when closing
    /// </summary>
    private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            Console.WriteLine($"[WindowSize] Window closing - Current size: {this.Width:F0}x{this.Height:F0}, State={this.WindowState}");

            var currentUserProfile = _profileService.GetCurrentUserProfile();
            if (currentUserProfile == null)
            {
                Console.WriteLine($"[WindowSize] No profile to save to!");
                return;
            }

            var displayPrefs = currentUserProfile.Preferences.DisplayPreferences;

            // Save current window state
            if (this.WindowState == Avalonia.Controls.WindowState.Maximized)
            {
                displayPrefs.WindowMaximized = true;
                Console.WriteLine($"[WindowSize] Saving maximized state");
            }
            else
            {
                displayPrefs.WindowMaximized = false;
                displayPrefs.WindowWidth = this.Width;
                displayPrefs.WindowHeight = this.Height;
                Console.WriteLine($"[WindowSize] Saving normal window size: {this.Width:F0}x{this.Height:F0}");
            }

            var saved = await _userProfileProvider.SaveAsync(currentUserProfile);
            Console.WriteLine($"[WindowSize] Save result: {saved}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WindowSize] Error saving window size: {ex.Message}");
        }
    }
}
