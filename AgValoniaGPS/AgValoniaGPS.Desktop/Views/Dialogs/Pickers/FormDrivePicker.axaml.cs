using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Pickers;

/// <summary>
/// Drive picker dialog that displays available drives with their information.
/// </summary>
public partial class FormDrivePicker : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormDrivePicker"/> class.
    /// </summary>
    public FormDrivePicker()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormDrivePicker(DrivePickerViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
