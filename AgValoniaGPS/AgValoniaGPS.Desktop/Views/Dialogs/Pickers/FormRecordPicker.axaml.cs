using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Pickers;

/// <summary>
/// Generic record picker dialog that displays a searchable list of records.
/// </summary>
public partial class FormRecordPicker : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormRecordPicker"/> class.
    /// </summary>
    public FormRecordPicker()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormRecordPicker(DialogViewModelBase viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
