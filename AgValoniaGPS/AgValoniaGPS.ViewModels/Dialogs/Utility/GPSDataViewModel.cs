using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for real-time GPS data display.
/// Shows latitude, longitude, altitude, speed, heading, fix quality, and satellite information.
/// Updates at 1 Hz frequency.
/// </summary>
public class GPSDataViewModel : DialogViewModelBase
{
    private double _latitude;
    private double _longitude;
    private double _altitude;
    private double _speed;
    private double _heading;
    private string _fixQuality = "No Fix";
    private int _satelliteCount;
    private double _hdop;
    private double _ageOfCorrection;
    private bool _isConnected;
    private DateTime _lastUpdate;

    /// <summary>
    /// Initializes a new instance of the <see cref="GPSDataViewModel"/> class.
    /// </summary>
    public GPSDataViewModel()
    {
        // TODO: Inject IPositionUpdateService and INmeaParserService from Wave 1
        // For now, we'll show placeholder data

        RefreshCommand = new RelayCommand(OnRefresh);

        // Initialize with sample data
        UpdateSampleData();
    }

    /// <summary>
    /// Gets or sets the latitude in decimal degrees.
    /// </summary>
    public double Latitude
    {
        get => _latitude;
        set
        {
            SetProperty(ref _latitude, value);
            OnPropertyChanged(nameof(LatitudeFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted latitude string with 7 decimal places.
    /// </summary>
    public string LatitudeFormatted => $"{Latitude:F7}°";

    /// <summary>
    /// Gets or sets the longitude in decimal degrees.
    /// </summary>
    public double Longitude
    {
        get => _longitude;
        set
        {
            SetProperty(ref _longitude, value);
            OnPropertyChanged(nameof(LongitudeFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted longitude string with 7 decimal places.
    /// </summary>
    public string LongitudeFormatted => $"{Longitude:F7}°";

    /// <summary>
    /// Gets or sets the altitude in meters.
    /// </summary>
    public double Altitude
    {
        get => _altitude;
        set
        {
            SetProperty(ref _altitude, value);
            OnPropertyChanged(nameof(AltitudeFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted altitude string.
    /// </summary>
    public string AltitudeFormatted => $"{Altitude:F1} m";

    /// <summary>
    /// Gets or sets the speed in km/h.
    /// </summary>
    public double Speed
    {
        get => _speed;
        set
        {
            SetProperty(ref _speed, value);
            OnPropertyChanged(nameof(SpeedFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted speed string.
    /// </summary>
    public string SpeedFormatted => $"{Speed:F1} km/h";

    /// <summary>
    /// Gets or sets the heading in degrees (0-360).
    /// </summary>
    public double Heading
    {
        get => _heading;
        set
        {
            SetProperty(ref _heading, value);
            OnPropertyChanged(nameof(HeadingFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted heading string.
    /// </summary>
    public string HeadingFormatted => $"{Heading:F1}°";

    /// <summary>
    /// Gets or sets the GPS fix quality description.
    /// </summary>
    public string FixQuality
    {
        get => _fixQuality;
        set => SetProperty(ref _fixQuality, value);
    }

    /// <summary>
    /// Gets or sets the number of satellites in use.
    /// </summary>
    public int SatelliteCount
    {
        get => _satelliteCount;
        set => SetProperty(ref _satelliteCount, value);
    }

    /// <summary>
    /// Gets or sets the Horizontal Dilution of Precision.
    /// </summary>
    public double HDOP
    {
        get => _hdop;
        set
        {
            SetProperty(ref _hdop, value);
            OnPropertyChanged(nameof(HDOPFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted HDOP string.
    /// </summary>
    public string HDOPFormatted => $"{HDOP:F2}";

    /// <summary>
    /// Gets or sets the age of differential correction in seconds.
    /// </summary>
    public double AgeOfCorrection
    {
        get => _ageOfCorrection;
        set
        {
            SetProperty(ref _ageOfCorrection, value);
            OnPropertyChanged(nameof(AgeOfCorrectionFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted age of correction string.
    /// </summary>
    public string AgeOfCorrectionFormatted => $"{AgeOfCorrection:F1} s";

    /// <summary>
    /// Gets or sets whether GPS is connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            SetProperty(ref _isConnected, value);
            OnPropertyChanged(nameof(ConnectionStatus));
        }
    }

    /// <summary>
    /// Gets the connection status text.
    /// </summary>
    public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            SetProperty(ref _lastUpdate, value);
            OnPropertyChanged(nameof(LastUpdateFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted last update string.
    /// </summary>
    public string LastUpdateFormatted => LastUpdate.ToString("HH:mm:ss");

    /// <summary>
    /// Gets the command to manually refresh GPS data.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Updates with sample GPS data for testing.
    /// </summary>
    private void UpdateSampleData()
    {
        // Sample coordinates (somewhere in Iowa, USA - farm country)
        Latitude = 42.0308341;
        Longitude = -93.6319350;
        Altitude = 312.5;
        Speed = 8.5;
        Heading = 45.0;
        FixQuality = "RTK Fixed";
        SatelliteCount = 12;
        HDOP = 0.8;
        AgeOfCorrection = 1.2;
        IsConnected = true;
        LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Refreshes the GPS data display.
    /// </summary>
    private void OnRefresh()
    {
        // TODO: When GPS services are integrated, refresh from actual GPS data
        UpdateSampleData();
    }

    /// <summary>
    /// Starts real-time GPS data updates.
    /// </summary>
    public void StartUpdates()
    {
        // TODO: Subscribe to IPositionUpdateService events
        // Update properties when GPS data arrives
        // Example:
        // _positionUpdateService.PositionUpdated += OnPositionUpdated;
    }

    /// <summary>
    /// Stops real-time GPS data updates.
    /// </summary>
    public void StopUpdates()
    {
        // TODO: Unsubscribe from GPS service events
        // _positionUpdateService.PositionUpdated -= OnPositionUpdated;
    }

    /// <summary>
    /// Cleanup when dialog closes.
    /// </summary>
    protected override void OnCancel()
    {
        StopUpdates();
        base.OnCancel();
    }
}
