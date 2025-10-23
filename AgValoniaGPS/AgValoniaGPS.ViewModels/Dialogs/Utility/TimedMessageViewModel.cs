using System;
using System.Reactive;
using System.Reactive.Linq;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for a timed notification that automatically closes after a countdown.
/// Displays remaining time and allows early cancellation.
/// </summary>
public class TimedMessageViewModel : DialogViewModelBase
{
    private string _message = string.Empty;
    private int _secondsRemaining;
    private int _totalSeconds;
    private IDisposable? _timerSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedMessageViewModel"/> class.
    /// </summary>
    public TimedMessageViewModel()
    {
        CloseNowCommand = ReactiveCommand.Create(OnCloseNow);
    }

    /// <summary>
    /// Initializes a new instance with message and duration.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="durationSeconds">Duration in seconds before auto-close.</param>
    public TimedMessageViewModel(string message, int durationSeconds) : this()
    {
        Message = message;
        TotalSeconds = durationSeconds;
        SecondsRemaining = durationSeconds;
        StartTimer();
    }

    /// <summary>
    /// Gets or sets the message to display.
    /// </summary>
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    /// <summary>
    /// Gets or sets the seconds remaining until auto-close.
    /// </summary>
    public int SecondsRemaining
    {
        get => _secondsRemaining;
        set
        {
            this.RaiseAndSetIfChanged(ref _secondsRemaining, value);
            this.RaisePropertyChanged(nameof(CountdownText));
        }
    }

    /// <summary>
    /// Gets or sets the total duration in seconds.
    /// </summary>
    public int TotalSeconds
    {
        get => _totalSeconds;
        set => this.RaiseAndSetIfChanged(ref _totalSeconds, value);
    }

    /// <summary>
    /// Gets the countdown text to display.
    /// </summary>
    public string CountdownText => $"Closing in {SecondsRemaining} second{(SecondsRemaining != 1 ? "s" : "")}...";

    /// <summary>
    /// Gets the command to close the dialog immediately.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CloseNowCommand { get; }

    /// <summary>
    /// Starts the countdown timer.
    /// </summary>
    public void StartTimer()
    {
        // Stop existing timer if any
        StopTimer();

        // Create a timer that ticks every second
        _timerSubscription = Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => OnTimerTick());
    }

    /// <summary>
    /// Stops the countdown timer.
    /// </summary>
    public void StopTimer()
    {
        _timerSubscription?.Dispose();
        _timerSubscription = null;
    }

    /// <summary>
    /// Called on each timer tick.
    /// </summary>
    private void OnTimerTick()
    {
        SecondsRemaining--;

        if (SecondsRemaining <= 0)
        {
            StopTimer();
            OnAutoClose();
        }
    }

    /// <summary>
    /// Called when the close now button is clicked.
    /// </summary>
    private void OnCloseNow()
    {
        StopTimer();
        DialogResult = false;
        RequestClose(false);
    }

    /// <summary>
    /// Called when the timer expires and dialog auto-closes.
    /// </summary>
    private void OnAutoClose()
    {
        DialogResult = true;
        RequestClose(true);
    }

    /// <summary>
    /// Cleanup when the ViewModel is disposed.
    /// </summary>
    protected override void OnCancel()
    {
        StopTimer();
        base.OnCancel();
    }
}
