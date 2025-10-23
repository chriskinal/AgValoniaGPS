using System;
using System.Reactive;
using System.Reflection;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for the About dialog displaying application version, copyright, and license information.
/// </summary>
public class AboutViewModel : DialogViewModelBase
{
    private string _applicationName = "AgValoniaGPS";
    private string _version = "1.0.0";
    private string _copyright = "Copyright © 2025";
    private string _description = "Precision agriculture guidance software";
    private string _license = "GNU General Public License v3.0";
    private string _website = "https://github.com/farmerbriantee/AgOpenGPS";
    private string _credits = "Based on AgOpenGPS by Brian Tischler";

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutViewModel"/> class.
    /// </summary>
    public AboutViewModel()
    {
        OpenWebsiteCommand = ReactiveCommand.Create(OnOpenWebsite);

        // Try to load version from assembly
        LoadVersionInfo();
    }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string ApplicationName
    {
        get => _applicationName;
        set => this.RaiseAndSetIfChanged(ref _applicationName, value);
    }

    /// <summary>
    /// Gets or sets the application version.
    /// </summary>
    public string Version
    {
        get => _version;
        set => this.RaiseAndSetIfChanged(ref _version, value);
    }

    /// <summary>
    /// Gets or sets the copyright information.
    /// </summary>
    public string Copyright
    {
        get => _copyright;
        set => this.RaiseAndSetIfChanged(ref _copyright, value);
    }

    /// <summary>
    /// Gets or sets the application description.
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Gets or sets the license information.
    /// </summary>
    public string License
    {
        get => _license;
        set => this.RaiseAndSetIfChanged(ref _license, value);
    }

    /// <summary>
    /// Gets or sets the website URL.
    /// </summary>
    public string Website
    {
        get => _website;
        set => this.RaiseAndSetIfChanged(ref _website, value);
    }

    /// <summary>
    /// Gets or sets the credits text.
    /// </summary>
    public string Credits
    {
        get => _credits;
        set => this.RaiseAndSetIfChanged(ref _credits, value);
    }

    /// <summary>
    /// Gets the command to open the website.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenWebsiteCommand { get; }

    /// <summary>
    /// Gets the full version string including build information.
    /// </summary>
    public string FullVersionString => $"Version {Version}";

    /// <summary>
    /// Loads version information from the executing assembly.
    /// </summary>
    private void LoadVersionInfo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            if (version != null)
            {
                Version = $"{version.Major}.{version.Minor}.{version.Build}";
            }

            // Try to get copyright from assembly attributes
            var copyrightAttr = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            if (copyrightAttr != null)
            {
                Copyright = copyrightAttr.Copyright;
            }

            // Try to get description from assembly attributes
            var descriptionAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (descriptionAttr != null)
            {
                Description = descriptionAttr.Description;
            }
        }
        catch
        {
            // If loading fails, keep default values
        }
    }

    /// <summary>
    /// Opens the website in the default browser.
    /// </summary>
    private void OnOpenWebsite()
    {
        try
        {
            // Platform-specific URL opening
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = Website,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            SetError($"Failed to open website: {ex.Message}");
        }
    }
}
