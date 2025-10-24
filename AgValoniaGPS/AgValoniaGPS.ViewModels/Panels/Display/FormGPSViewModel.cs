using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.ViewModels.Base;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.ViewModels.Panels.Display;

/// <summary>
/// ViewModel for the main GPS field view (FormGPS).
/// Manages the central map display and provides overlay controls for vehicle status,
/// guidance, section control, and camera manipulation.
/// </summary>
public partial class FormGPSViewModel : PanelViewModelBase
{
    private readonly IPositionUpdateService? _positionService;
    private readonly IGuidanceService? _guidanceService;
    private readonly IVehicleKinematicsService? _vehicleKinematicsService;

    #region Observable Properties

    [ObservableProperty]
    private double _currentSpeed;

    [ObservableProperty]
    private double _currentHeading;

    [ObservableProperty]
    private double _crossTrackError;

    [ObservableProperty]
    private double _distanceToTurn;

    [ObservableProperty]
    private bool _isGuidanceActive;

    [ObservableProperty]
    private bool _isAutoSteerActive;

    [ObservableProperty]
    private double _zoomLevel = 1.0;

    [ObservableProperty]
    private bool _is3DMode;

    [ObservableProperty]
    private double _cameraTilt;

    [ObservableProperty]
    private double _brightness = 1.0;

    [ObservableProperty]
    private bool _isDayMode = true;

    [ObservableProperty]
    private bool _showGrid = true;

    [ObservableProperty]
    private bool _followVehicle = true;

    // Section control states (up to 16 sections)
    [ObservableProperty]
    private bool _section1Enabled = true;

    [ObservableProperty]
    private bool _section2Enabled = true;

    [ObservableProperty]
    private bool _section3Enabled = true;

    [ObservableProperty]
    private bool _section4Enabled = true;

    [ObservableProperty]
    private bool _section5Enabled = true;

    [ObservableProperty]
    private bool _section6Enabled = true;

    [ObservableProperty]
    private bool _section7Enabled = true;

    [ObservableProperty]
    private bool _section8Enabled = true;

    #endregion

    public FormGPSViewModel(
        IPositionUpdateService? positionService = null,
        IGuidanceService? guidanceService = null,
        IVehicleKinematicsService? vehicleKinematicsService = null)
    {
        _positionService = positionService;
        _guidanceService = guidanceService;
        _vehicleKinematicsService = vehicleKinematicsService;

        // Subscribe to service events if services are available
        if (_positionService != null)
        {
            _positionService.PositionUpdated += OnPositionUpdated;
        }

        if (_guidanceService != null)
        {
            _guidanceService.GuidanceUpdated += OnGuidanceUpdated;
        }
    }

    #region Event Handlers

    private void OnPositionUpdated(object? sender, EventArgs e)
    {
        if (_positionService == null) return;

        CurrentSpeed = _positionService.GetCurrentSpeed() * 3.6; // m/s to km/h
        CurrentHeading = _positionService.GetCurrentHeading() * 180.0 / Math.PI; // radians to degrees
    }

    private void OnGuidanceUpdated(object? sender, GuidanceData e)
    {
        if (_guidanceService == null) return;

        CrossTrackError = _guidanceService.CrossTrackError;
        DistanceToTurn = _guidanceService.LookaheadDistance;
        IsGuidanceActive = _guidanceService.IsActive;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ToggleGuidance()
    {
        if (_guidanceService == null) return;

        if (_guidanceService.IsActive)
        {
            _guidanceService.Stop();
        }
        else
        {
            _guidanceService.Start();
        }

        IsGuidanceActive = _guidanceService.IsActive;
    }

    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(20.0, ZoomLevel * 1.2);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(0.1, ZoomLevel / 1.2);
    }

    [RelayCommand]
    private void Toggle3DMode()
    {
        Is3DMode = !Is3DMode;
    }

    [RelayCommand]
    private void TiltUp()
    {
        CameraTilt = Math.Min(90.0, CameraTilt + 5.0);
    }

    [RelayCommand]
    private void TiltDown()
    {
        CameraTilt = Math.Max(0.0, CameraTilt - 5.0);
    }

    [RelayCommand]
    private void BrightnessUp()
    {
        Brightness = Math.Min(2.0, Brightness + 0.1);
    }

    [RelayCommand]
    private void BrightnessDown()
    {
        Brightness = Math.Max(0.1, Brightness - 0.1);
    }

    [RelayCommand]
    private void ToggleDayNightMode()
    {
        IsDayMode = !IsDayMode;
    }

    [RelayCommand]
    private void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
    }

    [RelayCommand]
    private void ToggleFollowVehicle()
    {
        FollowVehicle = !FollowVehicle;
    }

    [RelayCommand]
    private void ResetCamera()
    {
        ZoomLevel = 1.0;
        CameraTilt = 0.0;
        FollowVehicle = true;
        Is3DMode = false;
    }

    [RelayCommand]
    private void ToggleSection1() => Section1Enabled = !Section1Enabled;

    [RelayCommand]
    private void ToggleSection2() => Section2Enabled = !Section2Enabled;

    [RelayCommand]
    private void ToggleSection3() => Section3Enabled = !Section3Enabled;

    [RelayCommand]
    private void ToggleSection4() => Section4Enabled = !Section4Enabled;

    [RelayCommand]
    private void ToggleSection5() => Section5Enabled = !Section5Enabled;

    [RelayCommand]
    private void ToggleSection6() => Section6Enabled = !Section6Enabled;

    [RelayCommand]
    private void ToggleSection7() => Section7Enabled = !Section7Enabled;

    [RelayCommand]
    private void ToggleSection8() => Section8Enabled = !Section8Enabled;

    #endregion
}
