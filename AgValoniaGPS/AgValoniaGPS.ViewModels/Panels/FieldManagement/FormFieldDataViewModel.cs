using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.Session;
using AgValoniaGPS.ViewModels.Base;
using Avalonia.Threading;
using System;
using System.Linq;

namespace AgValoniaGPS.ViewModels.Panels.FieldManagement;

/// <summary>
/// ViewModel for the Field Statistics panel showing real-time field operation data.
/// Displays field information, area coverage, distance traveled, and work statistics.
/// </summary>
public partial class FormFieldDataViewModel : PanelViewModelBase
{
    private readonly IFieldStatisticsService _fieldStatistics;
    private readonly ISessionManagementService _session;
    private readonly ISectionControlService _sectionControl;

    private string _fieldName = "No Field";
    private double _fieldAreaHectares;
    private double _workAreaHectares;
    private double _coveragePercent;
    private double _distanceTraveledKm;
    private TimeSpan _timeElapsed;
    private double _averageSpeedKmh;
    private int _sectionsAppliedCount;
    private double _applicationRate;
    private bool _workSwitchOn;

    public FormFieldDataViewModel(
        IFieldStatisticsService fieldStatistics,
        ISessionManagementService session,
        ISectionControlService sectionControl)
    {
        _fieldStatistics = fieldStatistics ?? throw new ArgumentNullException(nameof(fieldStatistics));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _sectionControl = sectionControl ?? throw new ArgumentNullException(nameof(sectionControl));

        Title = "Field Statistics";

        // Subscribe to service events
        _session.SessionStateChanged += OnSessionChanged;
        _sectionControl.SectionStateChanged += OnSectionStateChanged;

        // Initialize with current values
        UpdateFieldData();
    }

    public string Title { get; } = "Field Statistics";

    /// <summary>
    /// Current field name
    /// </summary>
    public string FieldName
    {
        get => _fieldName;
        set => SetProperty(ref _fieldName, value);
    }

    /// <summary>
    /// Total field area in hectares
    /// </summary>
    public double FieldAreaHectares
    {
        get => _fieldAreaHectares;
        set => SetProperty(ref _fieldAreaHectares, value);
    }

    /// <summary>
    /// Area worked in hectares
    /// </summary>
    public double WorkAreaHectares
    {
        get => _workAreaHectares;
        set => SetProperty(ref _workAreaHectares, value);
    }

    /// <summary>
    /// Coverage percentage (0-100)
    /// </summary>
    public double CoveragePercent
    {
        get => _coveragePercent;
        set => SetProperty(ref _coveragePercent, value);
    }

    /// <summary>
    /// Distance traveled in kilometers
    /// </summary>
    public double DistanceTraveledKm
    {
        get => _distanceTraveledKm;
        set => SetProperty(ref _distanceTraveledKm, value);
    }

    /// <summary>
    /// Time elapsed since field operation started
    /// </summary>
    public TimeSpan TimeElapsed
    {
        get => _timeElapsed;
        set => SetProperty(ref _timeElapsed, value);
    }

    /// <summary>
    /// Average speed in km/h
    /// </summary>
    public double AverageSpeedKmh
    {
        get => _averageSpeedKmh;
        set => SetProperty(ref _averageSpeedKmh, value);
    }

    /// <summary>
    /// Number of sections currently applied/active
    /// </summary>
    public int SectionsAppliedCount
    {
        get => _sectionsAppliedCount;
        set => SetProperty(ref _sectionsAppliedCount, value);
    }

    /// <summary>
    /// Application rate (e.g., liters per hectare)
    /// </summary>
    public double ApplicationRate
    {
        get => _applicationRate;
        set => SetProperty(ref _applicationRate, value);
    }

    /// <summary>
    /// Work switch state (on/off)
    /// </summary>
    public bool WorkSwitchOn
    {
        get => _workSwitchOn;
        set => SetProperty(ref _workSwitchOn, value);
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        // Marshal to UI thread to prevent cross-thread exceptions
        Dispatcher.UIThread.Post(() => UpdateFieldData());
    }

    private void OnSectionStateChanged(object? sender, EventArgs e)
    {
        // Marshal to UI thread to prevent cross-thread exceptions
        Dispatcher.UIThread.Post(() => UpdateSectionData());
    }

    private void UpdateFieldData()
    {
        // Get field name from statistics service
        FieldName = _fieldStatistics.GetCurrentFieldName();

        // Convert square meters to hectares (1 hectare = 10,000 m²)
        FieldAreaHectares = _fieldStatistics.BoundaryAreaSquareMeters / 10000.0;
        WorkAreaHectares = _fieldStatistics.ActualAreaCovered / 10000.0;

        // Calculate coverage percentage
        if (FieldAreaHectares > 0)
        {
            CoveragePercent = (WorkAreaHectares / FieldAreaHectares) * 100.0;
        }
        else
        {
            CoveragePercent = 0;
        }

        // Convert meters to kilometers
        DistanceTraveledKm = _fieldStatistics.UserDistance / 1000.0;

        // Get time from session (use current session state)
        var sessionState = _session.GetCurrentSessionState();
        if (sessionState != null)
        {
            TimeElapsed = DateTime.Now - sessionState.SessionStartTime;
        }
        else
        {
            TimeElapsed = TimeSpan.Zero;
        }

        // Calculate average speed (avoid division by zero)
        if (TimeElapsed.TotalHours > 0)
        {
            AverageSpeedKmh = DistanceTraveledKm / TimeElapsed.TotalHours;
        }
        else
        {
            AverageSpeedKmh = 0;
        }

        UpdateSectionData();
    }

    private void UpdateSectionData()
    {
        // Count active sections (Auto state means actively working)
        var sectionStates = _sectionControl.GetAllSectionStates();
        SectionsAppliedCount = sectionStates.Count(s => s == SectionState.Auto);

        // Work switch is considered "on" if any sections are active
        WorkSwitchOn = SectionsAppliedCount > 0;

        // Application rate calculation (placeholder - actual implementation may vary)
        // This would typically come from implement configuration
        ApplicationRate = 0; // To be implemented when implement configuration is available
    }
}
