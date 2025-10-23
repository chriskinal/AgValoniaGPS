using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Utility;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Utility;

/// <summary>
/// Generic dialog for displaying simple messages or prompts.
/// </summary>
public partial class FormDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormDialog"/> class.
    /// </summary>
    public FormDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormDialog(GenericDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
