using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// Placeholder view for ConfigHelp (to be implemented in future wave).
/// </summary>
public partial class ConfigHelp : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigHelp"/> class.
    /// </summary>
    public ConfigHelp()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigHelp(ConfigHelpViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
