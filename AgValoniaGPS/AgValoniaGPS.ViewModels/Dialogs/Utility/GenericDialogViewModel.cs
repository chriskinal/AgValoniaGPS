using System;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// Generic dialog ViewModel for displaying simple messages or prompts with customizable title and content.
/// Can be used as a wrapper for simple dialogs without creating custom ViewModels.
/// </summary>
public class GenericDialogViewModel : DialogViewModelBase
{
    private string _title = "Dialog";
    private string _message = string.Empty;
    private string _okButtonText = "OK";
    private string _cancelButtonText = "Cancel";
    private bool _showCancelButton = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericDialogViewModel"/> class.
    /// </summary>
    public GenericDialogViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance with specified title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    public GenericDialogViewModel(string title, string message) : this()
    {
        Title = title;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance with full customization.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="okButtonText">Text for the OK button.</param>
    /// <param name="cancelButtonText">Text for the Cancel button.</param>
    /// <param name="showCancelButton">Whether to show the Cancel button.</param>
    public GenericDialogViewModel(string title, string message, string okButtonText, string cancelButtonText = "Cancel", bool showCancelButton = true) : this()
    {
        Title = title;
        Message = message;
        OKButtonText = okButtonText;
        CancelButtonText = cancelButtonText;
        ShowCancelButton = showCancelButton;
    }

    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the dialog message content.
    /// </summary>
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    /// <summary>
    /// Gets or sets the text for the OK button.
    /// </summary>
    public string OKButtonText
    {
        get => _okButtonText;
        set => this.RaiseAndSetIfChanged(ref _okButtonText, value);
    }

    /// <summary>
    /// Gets or sets the text for the Cancel button.
    /// </summary>
    public string CancelButtonText
    {
        get => _cancelButtonText;
        set => this.RaiseAndSetIfChanged(ref _cancelButtonText, value);
    }

    /// <summary>
    /// Gets or sets whether the Cancel button should be shown.
    /// </summary>
    public bool ShowCancelButton
    {
        get => _showCancelButton;
        set => this.RaiseAndSetIfChanged(ref _showCancelButton, value);
    }
}
