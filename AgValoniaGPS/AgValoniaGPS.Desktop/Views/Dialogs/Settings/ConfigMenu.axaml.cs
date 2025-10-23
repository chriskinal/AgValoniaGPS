using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// Placeholder view for ConfigMenu (to be implemented in future wave).
/// </summary>
public partial class ConfigMenu : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMenu"/> class.
    /// </summary>
    public ConfigMenu()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigMenu(ConfigMenuViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
