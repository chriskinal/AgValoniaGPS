using System;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for displaying a save progress indicator with spinner.
/// This is typically shown as a non-modal dialog during save operations.
/// </summary>
public class SavingViewModel : ViewModelBase
{
    private string _message = "Saving...";
    private double _progress;
    private bool _isIndeterminate = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="SavingViewModel"/> class.
    /// </summary>
    public SavingViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public SavingViewModel(string message) : this()
    {
        Message = message;
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
    /// Gets or sets the progress value (0-100).
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            this.RaiseAndSetIfChanged(ref _progress, value);
            IsIndeterminate = false;
        }
    }

    /// <summary>
    /// Gets or sets whether the progress is indeterminate (spinner mode).
    /// </summary>
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _isIndeterminate, value);
    }

    /// <summary>
    /// Updates the progress and message.
    /// </summary>
    /// <param name="progress">Progress percentage (0-100).</param>
    /// <param name="message">Optional message to display.</param>
    public void UpdateProgress(double progress, string? message = null)
    {
        Progress = progress;
        if (message != null)
        {
            Message = message;
        }
    }
}
