using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Pickers;

/// <summary>
/// Color picker dialog that allows users to select colors via palette, RGB sliders, or hex input.
/// </summary>
public partial class FormColorPicker : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormColorPicker"/> class.
    /// </summary>
    public FormColorPicker()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public FormColorPicker(ColorPickerViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
