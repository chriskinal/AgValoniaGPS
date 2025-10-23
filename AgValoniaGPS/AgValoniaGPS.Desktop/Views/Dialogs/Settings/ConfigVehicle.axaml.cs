using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// Placeholder view for ConfigVehicle (to be implemented in future wave).
/// Note: This is different from ConfigVehicleControl which provides full vehicle configuration.
/// </summary>
public partial class ConfigVehicle : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigVehicle"/> class.
    /// </summary>
    public ConfigVehicle()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigVehicle(ConfigVehicleViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
