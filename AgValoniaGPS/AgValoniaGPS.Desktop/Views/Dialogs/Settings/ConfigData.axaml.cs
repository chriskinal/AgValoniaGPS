using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// Placeholder view for ConfigData (to be implemented in future wave).
/// </summary>
public partial class ConfigData : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigData"/> class.
    /// </summary>
    public ConfigData()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigData(ConfigDataViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
