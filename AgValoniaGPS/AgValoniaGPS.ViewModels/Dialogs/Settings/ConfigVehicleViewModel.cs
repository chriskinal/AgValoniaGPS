using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Settings;

/// <summary>
/// Placeholder ViewModel for ConfigVehicle (to be implemented in future wave).
/// Note: This is different from ConfigVehicleControlViewModel which provides full vehicle configuration.
/// </summary>
public class ConfigVehicleViewModel : DialogViewModelBase
{
    private string _placeholderMessage = "This configuration panel will be implemented in a future release.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigVehicleViewModel"/> class.
    /// </summary>
    public ConfigVehicleViewModel()
    {
        // Minimal initialization
    }

    /// <summary>
    /// Gets or sets the placeholder message.
    /// </summary>
    public string PlaceholderMessage
    {
        get => _placeholderMessage;
        set => SetProperty(ref _placeholderMessage, value);
    }
}
