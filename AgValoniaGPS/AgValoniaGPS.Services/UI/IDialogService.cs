using System.Threading.Tasks;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.UI;

/// <summary>
/// Service interface for showing dialogs and modal windows across the application.
/// Provides a consistent way to display dialogs, message boxes, and file pickers.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a modal dialog with the specified ViewModel and returns the dialog result.
    /// The dialog view is located by convention: [ViewModelName] → [ViewName] (ViewModel suffix removed).
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel to display.</typeparam>
    /// <typeparam name="TResult">The type of result expected from the dialog.</typeparam>
    /// <param name="viewModel">The ViewModel instance to display in the dialog.</param>
    /// <returns>The result from the dialog, or null if the dialog was cancelled or closed.</returns>
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel) where TViewModel : class;

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The dialog title. Defaults to "Confirm".</param>
    /// <returns>True if the user clicked Yes, false if No or cancelled.</returns>
    Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");

    /// <summary>
    /// Shows a message dialog with an OK button.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The dialog title. Defaults to "Message".</param>
    /// <param name="type">The type of message (affects icon and styling).</param>
    /// <returns>A task that completes when the dialog is closed.</returns>
    Task ShowMessageAsync(string message, string title = "Message", MessageType type = MessageType.Information);

    /// <summary>
    /// Shows a file picker dialog for selecting a file.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="defaultPath">The default directory path to start in.</param>
    /// <param name="fileTypes">Array of file extensions to filter (e.g., ["txt", "json"]). Null shows all files.</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    Task<string?> ShowFilePickerAsync(string title, string? defaultPath = null, string[]? fileTypes = null);

    /// <summary>
    /// Shows a folder picker dialog for selecting a directory.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="defaultPath">The default directory path to start in.</param>
    /// <returns>The selected folder path, or null if cancelled.</returns>
    Task<string?> ShowFolderPickerAsync(string title, string? defaultPath = null);
}
