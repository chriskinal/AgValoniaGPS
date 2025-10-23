using System;
using System.Windows.Input;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Base;

/// <summary>
/// Base class for dialog ViewModels that provides OK/Cancel command pattern
/// and dialog result handling.
/// </summary>
public abstract class DialogViewModelBase : ViewModelBase
{
    private bool? _dialogResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogViewModelBase"/> class.
    /// </summary>
    protected DialogViewModelBase()
    {
        OKCommand = ReactiveCommand.Create(OnOK);
        CancelCommand = ReactiveCommand.Create(OnCancel);
    }

    /// <summary>
    /// Gets or sets the dialog result.
    /// True = OK/Accept, False = Cancel, Null = No result yet.
    /// </summary>
    public bool? DialogResult
    {
        get => _dialogResult;
        set => this.RaiseAndSetIfChanged(ref _dialogResult, value);
    }

    /// <summary>
    /// Gets the command to execute when OK is pressed.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Gets the command to execute when Cancel is pressed.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Event raised when the dialog should be closed.
    /// Subscribers should close the dialog window when this event is raised.
    /// </summary>
    public event EventHandler<bool?>? CloseRequested;

    /// <summary>
    /// Called when the OK command is executed.
    /// Override this method to add validation or custom logic before closing.
    /// </summary>
    /// <returns>True if the dialog should close, false to keep it open (e.g., validation failed).</returns>
    protected virtual bool OnOK()
    {
        DialogResult = true;
        RequestClose(true);
        return true;
    }

    /// <summary>
    /// Called when the Cancel command is executed.
    /// Override this method to add cleanup logic if needed.
    /// </summary>
    protected virtual void OnCancel()
    {
        DialogResult = false;
        RequestClose(false);
    }

    /// <summary>
    /// Requests the dialog to close with the specified result.
    /// </summary>
    /// <param name="result">The dialog result.</param>
    protected void RequestClose(bool? result)
    {
        DialogResult = result;
        CloseRequested?.Invoke(this, result);
    }
}
