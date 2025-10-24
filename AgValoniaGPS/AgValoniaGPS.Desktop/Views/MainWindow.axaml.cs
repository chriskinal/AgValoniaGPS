using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.ViewModels;
using AgValoniaGPS.Services;

namespace AgValoniaGPS.Desktop.Views;

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private bool _isDraggingSection = false;
    private bool _isDraggingPanel = false;
    private Control? _draggingPanel = null;
    private Avalonia.Point _dragStartPoint;

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

        // Add keyboard shortcut for 3D mode toggle (F3)
        this.KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F3 && MapControl != null)
        {
            MapControl.Toggle3DMode();
            e.Handled = true;
        }
        else if (e.Key == Key.PageUp && MapControl != null)
        {
            // Increase pitch (tilt camera up)
            MapControl.SetPitch(0.05);
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown && MapControl != null)
        {
            // Decrease pitch (tilt camera down)
            MapControl.SetPitch(-0.05);
            e.Handled = true;
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

    private void Btn3DToggle_Click(object? sender, RoutedEventArgs e)
    {
        if (MapControl != null)
        {
            MapControl.Toggle3DMode();
        }
    }

    private void BtnSettings_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog
        {
            DataContext = ViewModel
        };
        dialog.ShowDialog(this);
    }

    private async void BtnFields_Click(object? sender, RoutedEventArgs e)
    {
        if (App.Services == null) return;

        var fieldService = App.Services.GetRequiredService<IFieldService>();

        // Get or set default fields directory
        string fieldsDir = ViewModel?.FieldsRootDirectory ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldsDir))
        {
            // Default to Documents/AgOpenGPS/Fields
            fieldsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "AgOpenGPS",
                "Fields");
        }

        // Show folder picker first to let user choose fields directory
        var folderDialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select AgOpenGPS Fields Directory",
            AllowMultiple = false
        };

        var storageProvider = this.StorageProvider;
        if (storageProvider != null)
        {
            // Try to set suggested start location to the current fields directory
            if (Directory.Exists(fieldsDir))
            {
                try
                {
                    // Convert local path to absolute Uri for Avalonia StorageProvider
                    var absolutePath = Path.GetFullPath(fieldsDir);
                    var uri = new Uri(absolutePath, UriKind.Absolute);
                    var startFolder = await storageProvider.TryGetFolderFromPathAsync(uri);
                    if (startFolder != null)
                    {
                        folderDialog.SuggestedStartLocation = startFolder;
                    }
                }
                catch { }
            }

            var folders = await storageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = "Select AgOpenGPS Fields Directory",
                AllowMultiple = false,
                SuggestedStartLocation = folderDialog.SuggestedStartLocation
            });

            if (folders != null && folders.Count > 0)
            {
                // User selected a directory
                var selectedPath = folders[0].Path.LocalPath;
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    fieldsDir = selectedPath;
                    if (ViewModel != null)
                    {
                        ViewModel.FieldsRootDirectory = fieldsDir;
                    }
                }
            }
            else
            {
                // User cancelled the folder picker
                return;
            }
        }

        var dialog = new FieldSelectionDialog(fieldService, fieldsDir);
        var result = await dialog.ShowDialog<bool>(this);

        if (result && dialog.SelectedField != null && ViewModel != null)
        {
            // Update ViewModel with selected field directory
            ViewModel.FieldsRootDirectory = fieldsDir;

            // Pass boundary to MapControl for rendering and center camera on it
            if (MapControl != null && dialog.SelectedField.Boundary != null)
            {
                MapControl.SetBoundary(dialog.SelectedField.Boundary);

                // Center camera on boundary
                var boundary = dialog.SelectedField.Boundary;
                if (boundary.OuterBoundary != null && boundary.OuterBoundary.Points.Count > 0)
                {
                    // Calculate boundary center
                    double sumE = 0, sumN = 0;
                    foreach (var point in boundary.OuterBoundary.Points)
                    {
                        sumE += point.Easting;
                        sumN += point.Northing;
                    }
                    double centerE = sumE / boundary.OuterBoundary.Points.Count;
                    double centerN = sumN / boundary.OuterBoundary.Points.Count;

                    // Pan camera to boundary center
                    MapControl.Pan(centerE, centerN);
                }
            }
        }
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

    // Map overlay event handlers that forward to MapControl
    private void MapOverlay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (MapControl != null)
        {
            // Forward event to MapControl's internal handler
            var point = e.GetCurrentPoint(this);

            if (point.Properties.IsLeftButtonPressed)
            {
                MapControl.StartPan(point.Position);
                e.Handled = true;
            }
            else if (point.Properties.IsRightButtonPressed)
            {
                MapControl.StartRotate(point.Position);
                e.Handled = true;
            }
        }
    }

    private void MapOverlay_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (MapControl != null)
        {
            var point = e.GetCurrentPoint(this);
            MapControl.UpdateMouse(point.Position);
            e.Handled = true;
        }
    }

    private void MapOverlay_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (MapControl != null)
        {
            MapControl.EndPanRotate();
            e.Handled = true;
        }
    }

    private void MapOverlay_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (MapControl != null)
        {
            double zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;
            MapControl.Zoom(zoomFactor);
            e.Handled = true;
        }
    }

    // Drag functionality for operational panels
    private void Panel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control panel)
        {
            _isDraggingPanel = true;
            _draggingPanel = panel;
            _dragStartPoint = e.GetPosition(this);
            e.Pointer.Capture(panel);
            e.Handled = true;
        }
    }

    private void Panel_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingPanel && sender is Control panel && panel == _draggingPanel)
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _dragStartPoint;

            // Get current position
            double currentLeft = Canvas.GetLeft(panel);
            double currentTop = Canvas.GetTop(panel);

            if (double.IsNaN(currentLeft)) currentLeft = 0;
            if (double.IsNaN(currentTop)) currentTop = 0;

            // Calculate new position
            double newLeft = currentLeft + delta.X;
            double newTop = currentTop + delta.Y;

            // Constrain to window bounds
            double maxLeft = Bounds.Width - panel.Bounds.Width;
            double maxTop = Bounds.Height - panel.Bounds.Height;

            newLeft = Math.Clamp(newLeft, 0, Math.Max(0, maxLeft));
            newTop = Math.Clamp(newTop, 0, Math.Max(0, maxTop));

            // Update position
            Canvas.SetLeft(panel, newLeft);
            Canvas.SetTop(panel, newTop);

            _dragStartPoint = currentPoint;
            e.Handled = true;
        }
    }

    private void Panel_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDraggingPanel)
        {
            _isDraggingPanel = false;
            if (sender is Control panel)
            {
                e.Pointer.Capture(null);
            }
            _draggingPanel = null;
            e.Handled = true;
        }
    }
}