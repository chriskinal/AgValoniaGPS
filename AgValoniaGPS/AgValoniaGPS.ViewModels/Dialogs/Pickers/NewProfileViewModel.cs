using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Pickers;

/// <summary>
/// ViewModel for new profile creation dialog with validation.
/// </summary>
public class NewProfileViewModel : DialogViewModelBase
{
    private string _profileName = string.Empty;
    private string _description = string.Empty;
    private ProfileTypeEnum _profileType = ProfileTypeEnum.Vehicle;
    private string _profilesDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewProfileViewModel"/> class.
    /// </summary>
    /// <param name="profilesDirectory">The directory where profiles are stored.</param>
    public NewProfileViewModel(string profilesDirectory)
    {
        _profilesDirectory = profilesDirectory;

        // Subscribe to property changes for validation
        this.WhenAnyValue(x => x.ProfileName)
            .Subscribe(_ => ValidateProfileName());
    }

    /// <summary>
    /// Gets or sets the profile name.
    /// </summary>
    public string ProfileName
    {
        get => _profileName;
        set => this.RaiseAndSetIfChanged(ref _profileName, value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the profile description.
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the profile type.
    /// </summary>
    public ProfileTypeEnum ProfileType
    {
        get => _profileType;
        set => this.RaiseAndSetIfChanged(ref _profileType, value);
    }

    /// <summary>
    /// Gets a value indicating whether the form is valid.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(ProfileName) && !HasError;

    /// <summary>
    /// Gets a value indicating whether the profile is a vehicle profile.
    /// </summary>
    public bool IsVehicleProfile
    {
        get => ProfileType == ProfileTypeEnum.Vehicle;
        set
        {
            if (value)
                ProfileType = ProfileTypeEnum.Vehicle;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the profile is a user profile.
    /// </summary>
    public bool IsUserProfile
    {
        get => ProfileType == ProfileTypeEnum.User;
        set
        {
            if (value)
                ProfileType = ProfileTypeEnum.User;
        }
    }

    /// <summary>
    /// Validates the profile name.
    /// </summary>
    private void ValidateProfileName()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(ProfileName))
        {
            SetError("Profile name is required.");
            this.RaisePropertyChanged(nameof(IsValid));
            return;
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (ProfileName.Any(c => invalidChars.Contains(c)))
        {
            SetError("Profile name contains invalid characters.");
            this.RaisePropertyChanged(nameof(IsValid));
            return;
        }

        // Check for duplicate
        if (!string.IsNullOrWhiteSpace(_profilesDirectory) && Directory.Exists(_profilesDirectory))
        {
            var existingFile = Path.Combine(_profilesDirectory, $"{ProfileName}.json");
            if (File.Exists(existingFile))
            {
                SetError("A profile with this name already exists.");
                this.RaisePropertyChanged(nameof(IsValid));
                return;
            }
        }

        this.RaisePropertyChanged(nameof(IsValid));
    }

    /// <summary>
    /// Validates the form before closing.
    /// </summary>
    protected override void OnOK()
    {
        ValidateProfileName();

        if (!IsValid)
        {
            return;
        }

        ClearError();
        base.OnOK();
    }
}

/// <summary>
/// Enumeration of profile types.
/// </summary>
public enum ProfileTypeEnum
{
    /// <summary>
    /// Vehicle/tractor profile.
    /// </summary>
    Vehicle,

    /// <summary>
    /// User/operator profile.
    /// </summary>
    User
}
