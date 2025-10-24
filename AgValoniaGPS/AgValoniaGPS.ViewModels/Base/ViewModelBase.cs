using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Runtime.CompilerServices;

namespace AgValoniaGPS.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels providing common functionality for property change notification
/// and state management using CommunityToolkit.Mvvm.
/// </summary>
public abstract class ViewModelBase : ObservableObject
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
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets the current error message, if any.
    /// Empty string indicates no error.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets a value indicating whether there is an error message to display.
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Helper method to set a property value and raise property changed notification.
    /// This uses CommunityToolkit.Mvvm's SetProperty from ObservableObject base class.
    /// Note: This method is inherited from ObservableObject and doesn't need to be redefined,
    /// but we keep this documentation for clarity about its availability.
    /// </summary>
    /// <remarks>
    /// SetProperty is provided by ObservableObject base class.
    /// Usage: SetProperty(ref _field, value);
    /// </remarks>

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
