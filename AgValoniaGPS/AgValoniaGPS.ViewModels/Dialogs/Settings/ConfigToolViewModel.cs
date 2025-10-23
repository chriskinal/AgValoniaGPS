using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Settings;

/// <summary>
/// Placeholder ViewModel for ConfigTool (to be implemented in future wave).
/// </summary>
public class ConfigToolViewModel : DialogViewModelBase
{
    private string _placeholderMessage = "This configuration panel will be implemented in a future release.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigToolViewModel"/> class.
    /// </summary>
    public ConfigToolViewModel()
    {
        // Minimal initialization
    }

    /// <summary>
    /// Gets or sets the placeholder message.
    /// </summary>
    public string PlaceholderMessage
    {
        get => _placeholderMessage;
        set => this.RaiseAndSetIfChanged(ref _placeholderMessage, value);
    }
}
