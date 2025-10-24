using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Settings;

/// <summary>
/// Placeholder ViewModel for ConfigData (to be implemented in future wave).
/// </summary>
public class ConfigDataViewModel : DialogViewModelBase
{
    private string _placeholderMessage = "This configuration panel will be implemented in a future release.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigDataViewModel"/> class.
    /// </summary>
    public ConfigDataViewModel()
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
