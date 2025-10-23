using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Pickers;

/// <summary>
/// New profile creation dialog with validation.
/// </summary>
public partial class FormNewProfile : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormNewProfile"/> class.
    /// </summary>
    public FormNewProfile()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormNewProfile(NewProfileViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
