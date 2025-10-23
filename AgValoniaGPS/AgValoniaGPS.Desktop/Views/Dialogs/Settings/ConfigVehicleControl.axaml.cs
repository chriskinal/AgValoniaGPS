using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// UserControl for comprehensive vehicle configuration with tabbed interface.
/// </summary>
public partial class ConfigVehicleControl : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigVehicleControl"/> class.
    /// </summary>
    public ConfigVehicleControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigVehicleControl(ConfigVehicleControlViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
