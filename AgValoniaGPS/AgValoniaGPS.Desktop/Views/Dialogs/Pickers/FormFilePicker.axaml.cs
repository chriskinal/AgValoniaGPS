using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Pickers;

/// <summary>
/// File picker dialog that provides file system navigation and file selection.
/// </summary>
public partial class FormFilePicker : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormFilePicker"/> class.
    /// </summary>
    public FormFilePicker()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormFilePicker(FilePickerViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
