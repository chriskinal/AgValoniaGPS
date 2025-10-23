using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// Placeholder view for ConfigModule (to be implemented in future wave).
/// </summary>
public partial class ConfigModule : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigModule"/> class.
    /// </summary>
    public ConfigModule()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigModule(ConfigModuleViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
