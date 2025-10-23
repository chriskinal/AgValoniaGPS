using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Pickers;

/// <summary>
/// Profile loading dialog that displays available profiles with preview and details.
/// </summary>
public partial class FormLoadProfile : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormLoadProfile"/> class.
    /// </summary>
    public FormLoadProfile()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormLoadProfile(LoadProfileViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
