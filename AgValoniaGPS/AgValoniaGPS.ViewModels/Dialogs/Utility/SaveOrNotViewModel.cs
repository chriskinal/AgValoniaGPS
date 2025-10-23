using System;
using System.Reactive;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for "Save or Not" dialog that prompts user to save, discard, or cancel changes.
/// DialogResult: true = Save, false = Don't Save, null = Cancel
/// </summary>
public class SaveOrNotViewModel : DialogViewModelBase
{
    private string _message = "Do you want to save your changes?";
    private string _title = "Save Changes";

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveOrNotViewModel"/> class.
    /// </summary>
    public SaveOrNotViewModel()
    {
        SaveCommand = ReactiveCommand.Create(OnSave);
        DontSaveCommand = ReactiveCommand.Create(OnDontSave);
    }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="message">The prompt message to display.</param>
    public SaveOrNotViewModel(string message) : this()
    {
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance with custom title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The prompt message to display.</param>
    public SaveOrNotViewModel(string title, string message) : this()
    {
        Title = title;
        Message = message;
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
    /// Gets or sets the message to display.
    /// </summary>
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    /// <summary>
    /// Gets the command to save changes and close.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    /// <summary>
    /// Gets the command to discard changes and close.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DontSaveCommand { get; }

    /// <summary>
    /// Called when Save button is clicked.
    /// Sets DialogResult to true and closes.
    /// </summary>
    private void OnSave()
    {
        DialogResult = true;
        RequestClose(true);
    }

    /// <summary>
    /// Called when Don't Save button is clicked.
    /// Sets DialogResult to false and closes.
    /// </summary>
    private void OnDontSave()
    {
        DialogResult = false;
        RequestClose(false);
    }
}
