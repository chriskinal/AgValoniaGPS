using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.FieldManagement;

/// <summary>
/// ViewModel for the Camera/View Controls panel providing zoom, pan, and camera mode controls.
/// Manages the map view state and camera positioning.
/// </summary>
public partial class FormCameraViewModel : PanelViewModelBase
{
    private double _zoomLevel = 5.0;
    private string _cameraMode = "Auto";
    private double _panOffsetX;
    private double _panOffsetY;
    private bool _isFollowingVehicle = true;

    public FormCameraViewModel()
    {
        Title = "Camera Controls";

        // Commands
        ZoomInCommand = ReactiveCommand.Create(OnZoomIn);
        ZoomOutCommand = ReactiveCommand.Create(OnZoomOut);
        ZoomToFitCommand = ReactiveCommand.Create(OnZoomToFit);
        ResetCameraCommand = ReactiveCommand.Create(OnResetCamera);
        ToggleFollowCommand = ReactiveCommand.Create(OnToggleFollow);
    }

    public string Title { get; } = "Camera Controls";

    /// <summary>
    /// Current zoom level (1.0-20.0)
    /// </summary>
    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            var clampedValue = Math.Clamp(value, 1.0, 20.0);
            this.RaiseAndSetIfChanged(ref _zoomLevel, clampedValue);
        }
    }

    /// <summary>
    /// Camera mode: Auto, Manual, North-Up, Heading-Up
    /// </summary>
    public string CameraMode
    {
        get => _cameraMode;
        set => this.RaiseAndSetIfChanged(ref _cameraMode, value);
    }

    /// <summary>
    /// Horizontal pan offset
    /// </summary>
    public double PanOffsetX
    {
        get => _panOffsetX;
        set => this.RaiseAndSetIfChanged(ref _panOffsetX, value);
    }

    /// <summary>
    /// Vertical pan offset
    /// </summary>
    public double PanOffsetY
    {
        get => _panOffsetY;
        set => this.RaiseAndSetIfChanged(ref _panOffsetY, value);
    }

    /// <summary>
    /// Whether camera is following the vehicle
    /// </summary>
    public bool IsFollowingVehicle
    {
        get => _isFollowingVehicle;
        set
        {
            this.RaiseAndSetIfChanged(ref _isFollowingVehicle, value);
            if (value)
            {
                CameraMode = "Auto";
            }
        }
    }

    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ZoomToFitCommand { get; }
    public ICommand ResetCameraCommand { get; }
    public ICommand ToggleFollowCommand { get; }

    private void OnZoomIn()
    {
        ZoomLevel += 1.0;
        ClearError();
    }

    private void OnZoomOut()
    {
        ZoomLevel -= 1.0;
        ClearError();
    }

    private void OnZoomToFit()
    {
        // Zoom to fit field boundary
        // This will be implemented when integrated with map view
        ZoomLevel = 5.0;
        PanOffsetX = 0;
        PanOffsetY = 0;
        ClearError();
    }

    private void OnResetCamera()
    {
        // Reset to default view centered on vehicle
        ZoomLevel = 5.0;
        PanOffsetX = 0;
        PanOffsetY = 0;
        IsFollowingVehicle = true;
        CameraMode = "Auto";
        ClearError();
    }

    private void OnToggleFollow()
    {
        IsFollowingVehicle = !IsFollowingVehicle;
        ClearError();
    }
}
