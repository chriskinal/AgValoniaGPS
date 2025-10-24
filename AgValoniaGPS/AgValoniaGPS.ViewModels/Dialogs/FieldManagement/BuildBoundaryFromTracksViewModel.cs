using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// Simple class to represent a GPS track for display
/// </summary>
public class GPSTrack
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int PointCount { get; set; }
    public double LengthMeters { get; set; }
}

/// <summary>
/// ViewModel for converting GPS tracks to boundary (FormBuildBoundaryFromTracks)
/// </summary>
public class BuildBoundaryFromTracksViewModel : DialogViewModelBase
{
    private readonly IBoundaryManagementService? _boundaryService;
    private GPSTrack? _selectedTrack;
    private double _bufferDistance = 5.0; // 5 meters default
    private double _simplifyTolerance = 1.0; // 1 meter default
    private int _resultPointCount;
    private bool _hasPreview;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildBoundaryFromTracksViewModel"/> class.
    /// </summary>
    /// <param name="boundaryService">Optional boundary service.</param>
    public BuildBoundaryFromTracksViewModel(IBoundaryManagementService? boundaryService = null)
    {
        _boundaryService = boundaryService;

        Tracks = new ObservableCollection<GPSTrack>();

        SelectTrackCommand = new RelayCommand<GPSTrack>(OnSelectTrack);
        GenerateCommand = new RelayCommand(OnGenerate);
        PreviewCommand = new RelayCommand(OnPreview);
    }

    /// <summary>
    /// Gets the available GPS tracks.
    /// </summary>
    public ObservableCollection<GPSTrack> Tracks { get; }

    /// <summary>
    /// Gets or sets the selected track to convert.
    /// </summary>
    public GPSTrack? SelectedTrack
    {
        get => _selectedTrack;
        set
        {
            SetProperty(ref _selectedTrack, value);
            HasPreview = false;
        }
    }

    /// <summary>
    /// Gets or sets the buffer distance around the track in meters.
    /// </summary>
    public double BufferDistance
    {
        get => _bufferDistance;
        set
        {
            if (value >= 0.5 && value <= 50.0)
            {
                SetProperty(ref _bufferDistance, value);
                HasPreview = false; // Invalidate preview
            }
        }
    }

    /// <summary>
    /// Gets or sets the simplification tolerance in meters.
    /// </summary>
    public double SimplifyTolerance
    {
        get => _simplifyTolerance;
        set
        {
            if (value >= 0.1 && value <= 10.0)
            {
                SetProperty(ref _simplifyTolerance, value);
                HasPreview = false; // Invalidate preview
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of points in the generated boundary.
    /// </summary>
    public int ResultPointCount
    {
        get => _resultPointCount;
        set => SetProperty(ref _resultPointCount, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a preview has been generated.
    /// </summary>
    public bool HasPreview
    {
        get => _hasPreview;
        set => SetProperty(ref _hasPreview, value);
    }

    /// <summary>
    /// Gets the command to select a track.
    /// </summary>
    public ICommand SelectTrackCommand { get; }

    /// <summary>
    /// Gets the command to generate the boundary.
    /// </summary>
    public ICommand GenerateCommand { get; }

    /// <summary>
    /// Gets the command to preview the boundary.
    /// </summary>
    public ICommand PreviewCommand { get; }

    /// <summary>
    /// Loads available tracks.
    /// </summary>
    /// <param name="tracks">The tracks to load.</param>
    public void LoadTracks(GPSTrack[] tracks)
    {
        Tracks.Clear();
        foreach (var track in tracks)
        {
            Tracks.Add(track);
        }
    }

    /// <summary>
    /// Selects a track.
    /// </summary>
    private void OnSelectTrack(GPSTrack track)
    {
        SelectedTrack = track;
    }

    /// <summary>
    /// Generates the boundary from the selected track.
    /// </summary>
    private void OnGenerate()
    {
        if (SelectedTrack == null)
        {
            SetError("Please select a track.");
            return;
        }

        try
        {
            // In a real implementation, would:
            // 1. Load track points
            // 2. Create buffer polygon around track
            // 3. Simplify using SimplifyTolerance
            // 4. Return as boundary

            // For now, simulate result
            ResultPointCount = (int)(SelectedTrack.PointCount * 0.3); // Simplified estimate

            DialogResult = true;
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Error generating boundary: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a preview of the boundary.
    /// </summary>
    private void OnPreview()
    {
        if (SelectedTrack == null)
        {
            return;
        }

        try
        {
            // In a real implementation, would generate and display preview
            ResultPointCount = (int)(SelectedTrack.PointCount * 0.3); // Simplified estimate
            HasPreview = true;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Error generating preview: {ex.Message}");
        }
    }
}
