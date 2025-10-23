using System;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels providing common functionality for property change notification
/// and state management using ReactiveUI.
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the ViewModel is currently performing an operation.
    /// Use this to show loading indicators or disable UI controls during async operations.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets the current error message, if any.
    /// Empty string indicates no error.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets a value indicating whether there is an error message to display.
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Helper method to set a property value and raise property changed notification.
    /// This is a convenience wrapper around ReactiveUI's RaiseAndSetIfChanged.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">Reference to the backing field.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The name of the property (automatically captured via CallerMemberName).</param>
    /// <returns>True if the value changed, false otherwise.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this.RaisePropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Clears any error message.
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Sets an error message.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    protected void SetError(string message)
    {
        ErrorMessage = message ?? string.Empty;
    }
}
