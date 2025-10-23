using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Settings;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Settings;

/// <summary>
/// UserControl for displaying configuration summary overview.
/// </summary>
public partial class ConfigSummaryControl : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigSummaryControl"/> class.
    /// </summary>
    public ConfigSummaryControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel for this view.</param>
    public ConfigSummaryControl(ConfigSummaryViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
