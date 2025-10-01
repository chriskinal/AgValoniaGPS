using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.ViewModels;

namespace AgValoniaGPS.Desktop.Views;

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private bool _isDraggingSection = false;
    private Avalonia.Point _dragStartPoint;

    // Camera control state
    private bool _isPanningCamera = false;
    private bool _isRotatingCamera = false;
    private Point _lastCameraMousePosition;

    public MainWindow()
    {
        InitializeComponent();

        // Set DataContext from DI
        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
        }

        // Handle window resize to keep section control in bounds
        this.PropertyChanged += MainWindow_PropertyChanged;

        // Subscribe to GPS position changes
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void MainWindow_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(Bounds) && SectionControlPanel != null)
        {
            // Constrain section control to new window bounds
            double currentLeft = Canvas.GetLeft(SectionControlPanel);
            double currentTop = Canvas.GetTop(SectionControlPanel);

            if (double.IsNaN(currentLeft)) currentLeft = 900; // Default initial position
            if (double.IsNaN(currentTop)) currentTop = 600;

            double maxLeft = Bounds.Width - SectionControlPanel.Bounds.Width;
            double maxTop = Bounds.Height - SectionControlPanel.Bounds.Height;

            double newLeft = Math.Clamp(currentLeft, 0, Math.Max(0, maxLeft));
            double newTop = Math.Clamp(currentTop, 0, Math.Max(0, maxTop));

            Canvas.SetLeft(SectionControlPanel, newLeft);
            Canvas.SetTop(SectionControlPanel, newTop);
        }
    }

    private async void BtnNtripConnect_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.ConnectToNtripAsync();
        }
    }

    private async void BtnNtripDisconnect_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.DisconnectFromNtripAsync();
        }
    }

    private void BtnDataIO_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new DataIODialog
        {
            DataContext = ViewModel
        };
        dialog.ShowDialog(this);
    }

    // Drag functionality for Section Control
    private void SectionControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border)
        {
            _isDraggingSection = true;
            _dragStartPoint = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void SectionControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingSection && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _dragStartPoint;

            // Calculate new position
            double newLeft = Canvas.GetLeft(border) + delta.X;
            double newTop = Canvas.GetTop(border) + delta.Y;

            // Constrain to window bounds
            double maxLeft = Bounds.Width - border.Bounds.Width;
            double maxTop = Bounds.Height - border.Bounds.Height;

            newLeft = Math.Clamp(newLeft, 0, maxLeft);
            newTop = Math.Clamp(newTop, 0, maxTop);

            // Update position
            Canvas.SetLeft(border, newLeft);
            Canvas.SetTop(border, newTop);

            _dragStartPoint = currentPoint;
        }
    }

    private void SectionControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDraggingSection)
        {
            _isDraggingSection = false;
            if (sender is Border border)
            {
                e.Pointer.Capture(null);
            }
        }
    }

    // Map input overlay event handlers for camera control
    private void MapInputOverlay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPanningCamera = true;
            _lastCameraMousePosition = point.Position;
            e.Pointer.Capture(MapInputOverlay);
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _isRotatingCamera = true;
            _lastCameraMousePosition = point.Position;
            e.Pointer.Capture(MapInputOverlay);
        }
    }

    private void MapInputOverlay_PointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var currentPos = point.Position;

        if (_isPanningCamera && MapControl != null)
        {
            double deltaX = currentPos.X - _lastCameraMousePosition.X;
            double deltaY = currentPos.Y - _lastCameraMousePosition.Y;

            // Convert screen space delta to world space
            double aspect = Bounds.Width / Bounds.Height;
            double worldDeltaX = -deltaX * (200.0 * aspect / MapControl.GetZoom()) / Bounds.Width;
            double worldDeltaY = -deltaY * (200.0 / MapControl.GetZoom()) / Bounds.Height;

            MapControl.Pan(worldDeltaX, worldDeltaY);
            _lastCameraMousePosition = currentPos;
        }
        else if (_isRotatingCamera && MapControl != null)
        {
            double deltaX = currentPos.X - _lastCameraMousePosition.X;
            double rotationDelta = deltaX * 0.01;

            MapControl.Rotate(rotationDelta);
            _lastCameraMousePosition = currentPos;
        }
    }

    private void MapInputOverlay_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanningCamera || _isRotatingCamera)
        {
            _isPanningCamera = false;
            _isRotatingCamera = false;
            e.Pointer.Capture(null);
        }
    }

    private void MapInputOverlay_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (MapControl != null)
        {
            double zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;
            MapControl.Zoom(zoomFactor);
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.Easting) ||
            e.PropertyName == nameof(MainViewModel.Northing) ||
            e.PropertyName == nameof(MainViewModel.Heading))
        {
            if (ViewModel != null && MapControl != null)
            {
                // Convert heading from degrees to radians
                double headingRadians = ViewModel.Heading * Math.PI / 180.0;
                MapControl.SetVehiclePosition(ViewModel.Easting, ViewModel.Northing, headingRadians);
            }
        }
    }
}