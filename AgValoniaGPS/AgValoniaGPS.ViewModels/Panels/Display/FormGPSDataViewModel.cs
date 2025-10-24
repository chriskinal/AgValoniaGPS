using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;

namespace AgValoniaGPS.ViewModels.Panels.Display;

/// <summary>
/// ViewModel for the GPS Data panel showing detailed GPS status and position information.
/// Displays fix quality, position coordinates, speed, heading, and NTRIP connection status.
/// </summary>
public partial class FormGPSDataViewModel : PanelViewModelBase
{
    private readonly IPositionUpdateService _positionService;
    private readonly INtripClientService _ntripService;

    private string _fixQualityText = "No Fix";
    private double _latitude;
    private double _longitude;
    private double _altitude;
    private double _speedKmh;
    private double _headingDegrees;
    private double _rollAngleDegrees;
    private int _satelliteCount;
    private double _hdop = 99.9;
    private double _ageOfCorrection;
    private bool _ntripConnected;

    public FormGPSDataViewModel(
        IPositionUpdateService positionService,
        INtripClientService ntripService)
    {
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        _ntripService = ntripService ?? throw new ArgumentNullException(nameof(ntripService));

        Title = "GPS Data";

        // Subscribe to position updates
        _positionService.PositionUpdated += OnPositionUpdated;

        // Subscribe to NTRIP status changes (if service supports it)
        // _ntripService.ConnectionStateChanged += OnNtripConnectionStateChanged;

        // Initialize with current values
        UpdateGPSData();
    }

    public string Title { get; } = "GPS Data";

    /// <summary>
    /// GPS fix quality status as text
    /// </summary>
    public string FixQualityText
    {
        get => _fixQualityText;
        set => this.RaiseAndSetIfChanged(ref _fixQualityText, value);
    }

    /// <summary>
    /// Latitude in decimal degrees
    /// </summary>
    public double Latitude
    {
        get => _latitude;
        set => this.RaiseAndSetIfChanged(ref _latitude, value);
    }

    /// <summary>
    /// Longitude in decimal degrees
    /// </summary>
    public double Longitude
    {
        get => _longitude;
        set => this.RaiseAndSetIfChanged(ref _longitude, value);
    }

    /// <summary>
    /// Altitude in meters
    /// </summary>
    public double Altitude
    {
        get => _altitude;
        set => this.RaiseAndSetIfChanged(ref _altitude, value);
    }

    /// <summary>
    /// Speed in km/h
    /// </summary>
    public double SpeedKmh
    {
        get => _speedKmh;
        set => this.RaiseAndSetIfChanged(ref _speedKmh, value);
    }

    /// <summary>
    /// Heading in degrees (0-360)
    /// </summary>
    public double HeadingDegrees
    {
        get => _headingDegrees;
        set => this.RaiseAndSetIfChanged(ref _headingDegrees, value);
    }

    /// <summary>
    /// Roll angle in degrees
    /// </summary>
    public double RollAngleDegrees
    {
        get => _rollAngleDegrees;
        set => this.RaiseAndSetIfChanged(ref _rollAngleDegrees, value);
    }

    /// <summary>
    /// Number of satellites being tracked
    /// </summary>
    public int SatelliteCount
    {
        get => _satelliteCount;
        set => this.RaiseAndSetIfChanged(ref _satelliteCount, value);
    }

    /// <summary>
    /// Horizontal Dilution of Precision
    /// </summary>
    public double HDOP
    {
        get => _hdop;
        set => this.RaiseAndSetIfChanged(ref _hdop, value);
    }

    /// <summary>
    /// Age of differential correction in seconds
    /// </summary>
    public double AgeOfCorrection
    {
        get => _ageOfCorrection;
        set => this.RaiseAndSetIfChanged(ref _ageOfCorrection, value);
    }

    /// <summary>
    /// NTRIP connection status
    /// </summary>
    public bool NTRIPConnected
    {
        get => _ntripConnected;
        set => this.RaiseAndSetIfChanged(ref _ntripConnected, value);
    }

    private void OnPositionUpdated(object? sender, PositionUpdateEventArgs e)
    {
        UpdateGPSData();
    }

    private void UpdateGPSData()
    {
        var currentPosition = _positionService.GetCurrentPosition();
        var currentHeading = _positionService.GetCurrentHeading();
        var currentSpeed = _positionService.GetCurrentSpeed();

        if (currentPosition != null)
        {
            // For now, display UTM coordinates as lat/lon (actual conversion would use UTM library)
            // TODO: Implement proper UTM to Lat/Lon conversion
            Latitude = currentPosition.Northing / 111320.0; // Approximate conversion
            Longitude = currentPosition.Easting / 111320.0; // Approximate conversion
            Altitude = currentPosition.Altitude;

            // Convert heading from radians to degrees
            HeadingDegrees = currentHeading * 180.0 / Math.PI;

            // Convert speed from m/s to km/h
            SpeedKmh = currentSpeed * 3.6;

            // Placeholder values for now
            RollAngleDegrees = 0;  // Would come from IMU data
            SatelliteCount = 12;   // Placeholder
            HDOP = 1.2;           // Placeholder
            AgeOfCorrection = 1.0; // Placeholder
            FixQualityText = "RTK Fixed"; // Placeholder
        }

        // Update NTRIP status
        NTRIPConnected = _ntripService?.IsConnected ?? false;
    }
}
