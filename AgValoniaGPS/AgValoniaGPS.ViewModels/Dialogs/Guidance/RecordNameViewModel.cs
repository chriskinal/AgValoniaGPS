using System;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Guidance;

/// <summary>
/// ViewModel for recording naming dialog (FormRecordName).
/// Provides interface for naming and describing recorded guidance paths.
/// </summary>
public class RecordNameViewModel : DialogViewModelBase
{
    private string _recordingName = string.Empty;
    private string _description = string.Empty;
    private DateTime _recordedDate;
    private int _pointCount;
    private double _totalDistance;
    private bool _isValid;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNameViewModel"/> class.
    /// </summary>
    /// <param name="pointCount">Number of recorded points.</param>
    /// <param name="totalDistance">Total distance recorded in meters.</param>
    public RecordNameViewModel(int pointCount = 0, double totalDistance = 0.0)
    {
        _pointCount = pointCount;
        _totalDistance = totalDistance;
        _recordedDate = DateTime.Now;

        SaveRecordingCommand = ReactiveCommand.Create(OnSaveRecording,
            this.WhenAnyValue(x => x.IsValid).Select(valid => valid));

        // Validate whenever recording name changes
        this.WhenAnyValue(x => x.RecordingName)
            .Subscribe(_ => ValidateRecordingName());
    }

    /// <summary>
    /// Gets or sets the name for the recording (required).
    /// Must be 1-50 characters, no special characters.
    /// </summary>
    public string RecordingName
    {
        get => _recordingName;
        set => this.RaiseAndSetIfChanged(ref _recordingName, value);
    }

    /// <summary>
    /// Gets or sets the optional description for the recording.
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Gets or sets the recording date and time.
    /// </summary>
    public DateTime RecordedDate
    {
        get => _recordedDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _recordedDate, value);
            this.RaisePropertyChanged(nameof(RecordedDateFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted recording date string.
    /// </summary>
    public string RecordedDateFormatted => RecordedDate.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Gets or sets the number of recorded points.
    /// </summary>
    public int PointCount
    {
        get => _pointCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _pointCount, value);
            this.RaisePropertyChanged(nameof(PointCountDisplay));
        }
    }

    /// <summary>
    /// Gets the point count display text.
    /// </summary>
    public string PointCountDisplay => $"{PointCount} points";

    /// <summary>
    /// Gets or sets the total distance recorded in meters.
    /// </summary>
    public double TotalDistance
    {
        get => _totalDistance;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalDistance, value);
            this.RaisePropertyChanged(nameof(TotalDistanceFormatted));
        }
    }

    /// <summary>
    /// Gets the formatted total distance string.
    /// </summary>
    public string TotalDistanceFormatted => $"{TotalDistance:F1} m";

    /// <summary>
    /// Gets or sets a value indicating whether the recording name is valid.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        private set => this.RaiseAndSetIfChanged(ref _isValid, value);
    }

    /// <summary>
    /// Gets the command to save the recording with the provided name.
    /// </summary>
    public ICommand SaveRecordingCommand { get; }

    /// <summary>
    /// Validates the recording name.
    /// Name must be 1-50 characters and contain only alphanumeric characters, spaces, hyphens, and underscores.
    /// </summary>
    private void ValidateRecordingName()
    {
        if (string.IsNullOrWhiteSpace(RecordingName))
        {
            IsValid = false;
            SetError("Recording name is required");
            return;
        }

        if (RecordingName.Length > 50)
        {
            IsValid = false;
            SetError("Recording name must be 50 characters or less");
            return;
        }

        // Check for invalid characters (allow alphanumeric, spaces, hyphens, underscores)
        if (!System.Text.RegularExpressions.Regex.IsMatch(RecordingName, @"^[a-zA-Z0-9 _-]+$"))
        {
            IsValid = false;
            SetError("Recording name can only contain letters, numbers, spaces, hyphens, and underscores");
            return;
        }

        // All validation passed
        IsValid = true;
        ClearError();
    }

    /// <summary>
    /// Saves the recording with the provided name.
    /// </summary>
    private void OnSaveRecording()
    {
        if (!IsValid)
        {
            return;
        }

        // TODO: When recording services are integrated, save the recording
        // Example: _recordingService?.SaveRecording(RecordingName, Description, RecordedDate);

        RequestClose(true);
    }

    /// <summary>
    /// Override OnOK to validate before closing.
    /// </summary>
    protected override void OnOK()
    {
        if (!IsValid)
        {
            return; // Keep dialog open if validation fails
        }

        base.OnOK();
    }
}
