using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System.Reactive.Linq;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for boundary playback/animation dialog (FormBoundaryPlayer)
/// </summary>
public class BoundaryPlayerViewModel : DialogViewModelBase
{
    private int _currentPointIndex;
    private double _playbackSpeed = 1.0;
    private bool _isPlaying;
    private TimeSpan _totalDuration;
    private TimeSpan _currentTime;
    private CancellationTokenSource? _playbackCancellation;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundaryPlayerViewModel"/> class.
    /// </summary>
    public BoundaryPlayerViewModel()
    {
        RecordedPoints = new ObservableCollection<Position>();

        PlayCommand = ReactiveCommand.Create(OnPlay,
            this.WhenAnyValue(x => x.IsPlaying, x => x.RecordedPoints.Count,
                (playing, count) => !playing && count > 0));
        PauseCommand = ReactiveCommand.Create(OnPause,
            this.WhenAnyValue(x => x.IsPlaying).Select(playing => playing));
        StopCommand = ReactiveCommand.Create(OnStop,
            this.WhenAnyValue(x => x.IsPlaying).Select(playing => playing));
        SeekCommand = ReactiveCommand.Create<int>(OnSeek);
    }

    /// <summary>
    /// Gets the recorded boundary points.
    /// </summary>
    public ObservableCollection<Position> RecordedPoints { get; }

    /// <summary>
    /// Gets or sets the current playback point index.
    /// </summary>
    public int CurrentPointIndex
    {
        get => _currentPointIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPointIndex, value);
            UpdateCurrentTime();
        }
    }

    /// <summary>
    /// Gets or sets the playback speed multiplier (0.5x to 4x).
    /// </summary>
    public double PlaybackSpeed
    {
        get => _playbackSpeed;
        set
        {
            if (value >= 0.5 && value <= 4.0)
            {
                this.RaiseAndSetIfChanged(ref _playbackSpeed, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether playback is active.
    /// </summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    /// <summary>
    /// Gets or sets the total recording duration.
    /// </summary>
    public TimeSpan TotalDuration
    {
        get => _totalDuration;
        set => this.RaiseAndSetIfChanged(ref _totalDuration, value);
    }

    /// <summary>
    /// Gets or sets the current playback time.
    /// </summary>
    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }

    /// <summary>
    /// Gets the total number of points.
    /// </summary>
    public int TotalPoints => RecordedPoints.Count;

    /// <summary>
    /// Gets the playback progress (0-100).
    /// </summary>
    public double Progress => TotalPoints > 0 ? (CurrentPointIndex * 100.0) / TotalPoints : 0;

    /// <summary>
    /// Gets the command to start playback.
    /// </summary>
    public ICommand PlayCommand { get; }

    /// <summary>
    /// Gets the command to pause playback.
    /// </summary>
    public ICommand PauseCommand { get; }

    /// <summary>
    /// Gets the command to stop playback and reset.
    /// </summary>
    public ICommand StopCommand { get; }

    /// <summary>
    /// Gets the command to seek to a specific point.
    /// </summary>
    public ICommand SeekCommand { get; }

    /// <summary>
    /// Loads recorded points for playback.
    /// </summary>
    /// <param name="points">The points to load.</param>
    /// <param name="duration">Total recording duration.</param>
    public void LoadRecording(Position[] points, TimeSpan duration)
    {
        RecordedPoints.Clear();
        foreach (var point in points)
        {
            RecordedPoints.Add(point);
        }

        TotalDuration = duration;
        CurrentPointIndex = 0;
        UpdateCurrentTime();
        this.RaisePropertyChanged(nameof(TotalPoints));
    }

    /// <summary>
    /// Starts playback.
    /// </summary>
    private async void OnPlay()
    {
        IsPlaying = true;
        _playbackCancellation = new CancellationTokenSource();

        try
        {
            await PlaybackLoop(_playbackCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Playback was cancelled
        }
        finally
        {
            IsPlaying = false;
        }
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    private void OnPause()
    {
        _playbackCancellation?.Cancel();
        IsPlaying = false;
    }

    /// <summary>
    /// Stops playback and resets to beginning.
    /// </summary>
    private void OnStop()
    {
        _playbackCancellation?.Cancel();
        IsPlaying = false;
        CurrentPointIndex = 0;
    }

    /// <summary>
    /// Seeks to a specific point index.
    /// </summary>
    private void OnSeek(int pointIndex)
    {
        if (pointIndex >= 0 && pointIndex < TotalPoints)
        {
            CurrentPointIndex = pointIndex;
        }
    }

    /// <summary>
    /// Playback loop that advances through points.
    /// </summary>
    private async Task PlaybackLoop(CancellationToken cancellationToken)
    {
        while (CurrentPointIndex < TotalPoints - 1 && !cancellationToken.IsCancellationRequested)
        {
            // Calculate delay based on playback speed
            var baseDelay = TotalDuration.TotalMilliseconds / TotalPoints;
            var delay = (int)(baseDelay / PlaybackSpeed);

            await Task.Delay(delay, cancellationToken);

            CurrentPointIndex++;
            this.RaisePropertyChanged(nameof(Progress));
        }

        // Reached end
        IsPlaying = false;
    }

    /// <summary>
    /// Updates the current time based on current point index.
    /// </summary>
    private void UpdateCurrentTime()
    {
        if (TotalPoints > 0)
        {
            CurrentTime = TimeSpan.FromMilliseconds(
                TotalDuration.TotalMilliseconds * CurrentPointIndex / TotalPoints);
        }
        else
        {
            CurrentTime = TimeSpan.Zero;
        }
    }
}
