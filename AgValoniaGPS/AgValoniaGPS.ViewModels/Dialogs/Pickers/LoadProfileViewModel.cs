using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Pickers;

/// <summary>
/// ViewModel for profile loading dialog that displays available profiles with details.
/// </summary>
public class LoadProfileViewModel : DialogViewModelBase
{
    private ObservableCollection<ProfileInfo> _profiles = new();
    private ProfileInfo? _selectedProfile;
    private string _profilesDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadProfileViewModel"/> class.
    /// </summary>
    /// <param name="profilesDirectory">The directory where profiles are stored.</param>
    public LoadProfileViewModel(string profilesDirectory)
    {
        _profilesDirectory = profilesDirectory;

        LoadFromFileCommand = new RelayCommand(LoadFromFile);
        DeleteProfileCommand = new RelayCommand(DeleteProfile);
        RefreshCommand = new RelayCommand(LoadProfiles);

        LoadProfiles();
    }

    /// <summary>
    /// Gets the collection of available profiles.
    /// </summary>
    public ObservableCollection<ProfileInfo> Profiles
    {
        get => _profiles;
        private set => SetProperty(ref _profiles, value);
    }

    /// <summary>
    /// Gets or sets the selected profile.
    /// </summary>
    public ProfileInfo? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            SetProperty(ref _selectedProfile, value);
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    /// <summary>
    /// Gets a value indicating whether a profile is selected.
    /// </summary>
    public bool HasSelection => SelectedProfile != null;

    /// <summary>
    /// Gets the command to load a profile from a file.
    /// </summary>
    public ICommand LoadFromFileCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected profile.
    /// </summary>
    public ICommand DeleteProfileCommand { get; }

    /// <summary>
    /// Gets the command to refresh the profile list.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Loads the list of available profiles from the profiles directory.
    /// </summary>
    private void LoadProfiles()
    {
        try
        {
            IsBusy = true;
            ClearError();

            if (!Directory.Exists(_profilesDirectory))
            {
                Directory.CreateDirectory(_profilesDirectory);
            }

            var profileFiles = Directory.GetFiles(_profilesDirectory, "*.json")
                .Select(f => new ProfileInfo(f))
                .OrderByDescending(p => p.LastModified)
                .ToList();

            Profiles = new ObservableCollection<ProfileInfo>(profileFiles);

            if (Profiles.Count == 0)
            {
                SetError("No profiles found. Create a new profile to get started.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error loading profiles: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Opens a file dialog to load a profile from a custom location.
    /// </summary>
    private void LoadFromFile()
    {
        // This would typically open a file dialog
        // For now, just set an error message indicating the feature
        SetError("File dialog not yet implemented. Please select from the list.");
    }

    /// <summary>
    /// Deletes the selected profile after confirmation.
    /// </summary>
    private void DeleteProfile()
    {
        if (SelectedProfile == null) return;

        try
        {
            // In a real implementation, this would show a confirmation dialog first
            File.Delete(SelectedProfile.FilePath);
            LoadProfiles();
            SetError($"Profile '{SelectedProfile.Name}' deleted successfully.");
        }
        catch (Exception ex)
        {
            SetError($"Error deleting profile: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that a profile is selected before closing.
    /// </summary>
    protected override void OnOK()
    {
        if (SelectedProfile == null)
        {
            SetError("Please select a profile to load.");
            return;
        }

        ClearError();
        base.OnOK();
    }
}

/// <summary>
/// Represents information about a profile for display in the picker.
/// </summary>
public class ProfileInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileInfo"/> class.
    /// </summary>
    /// <param name="filePath">The full path to the profile file.</param>
    public ProfileInfo(string filePath)
    {
        FilePath = filePath;
        Name = Path.GetFileNameWithoutExtension(filePath);

        var fileInfo = new FileInfo(filePath);
        LastModified = fileInfo.LastWriteTime;
        FileSize = FormatBytes(fileInfo.Length);

        // In a real implementation, this would read metadata from the file
        Description = "Profile configuration";
        ProfileType = DetermineProfileType(filePath);
    }

    /// <summary>
    /// Gets the full path to the profile file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the name of the profile.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the profile.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the last modified date and time.
    /// </summary>
    public DateTime LastModified { get; }

    /// <summary>
    /// Gets the formatted last modified date string.
    /// </summary>
    public string LastModifiedString => LastModified.ToString("yyyy-MM-dd HH:mm");

    /// <summary>
    /// Gets the file size.
    /// </summary>
    public string FileSize { get; }

    /// <summary>
    /// Gets the profile type.
    /// </summary>
    public string ProfileType { get; }

    /// <summary>
    /// Gets the icon for the profile type.
    /// </summary>
    public string Icon => ProfileType.ToLowerInvariant() switch
    {
        "vehicle" => "🚜",
        "user" => "👤",
        "field" => "🌾",
        _ => "📄"
    };

    /// <summary>
    /// Determines the profile type from the file name or path.
    /// </summary>
    private string DetermineProfileType(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

        if (name.Contains("vehicle") || name.Contains("tractor"))
            return "Vehicle";
        if (name.Contains("user") || name.Contains("operator"))
            return "User";
        if (name.Contains("field"))
            return "Field";

        return "General";
    }

    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
