using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.Profile;
using AgValoniaGPS.Services.Configuration;

namespace AgValoniaGPS.Services.Setup;

/// <summary>
/// Implements first-time setup wizard for new installations.
/// Guides users through vehicle configuration or provides quick default setup.
/// </summary>
public class SetupWizardService : ISetupWizardService
{
    private readonly IProfileManagementService _profileService;
    private readonly IConfigurationService _configurationService;
    private readonly Dictionary<int, object> _completedSteps = new();

    public SetupWizardService(
        IProfileManagementService profileService,
        IConfigurationService configurationService)
    {
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Checks if first-time setup is needed by verifying profile existence.
    /// </summary>
    public bool IsFirstTimeSetup()
    {
        var vehicleProfiles = _profileService.GetVehicleProfilesAsync().Result;
        var userProfiles = _profileService.GetUserProfilesAsync().Result;

        return vehicleProfiles.Length == 0 || userProfiles.Length == 0;
    }

    /// <summary>
    /// Gets the wizard steps for guided setup.
    /// </summary>
    public SetupWizardStep[] GetWizardSteps()
    {
        return new[]
        {
            new SetupWizardStep
            {
                StepIndex = 0,
                Title = "Vehicle Selection",
                Description = "Select your vehicle type and provide a name",
                IsRequired = true
            },
            new SetupWizardStep
            {
                StepIndex = 1,
                Title = "Physical Configuration",
                Description = "Configure vehicle dimensions (wheelbase, track, antenna position)",
                IsRequired = true
            },
            new SetupWizardStep
            {
                StepIndex = 2,
                Title = "GPS Setup",
                Description = "Configure GPS settings (update rate, heading source)",
                IsRequired = false
            },
            new SetupWizardStep
            {
                StepIndex = 3,
                Title = "Section Control",
                Description = "Configure section control (number of sections, tool width)",
                IsRequired = false
            }
        };
    }

    /// <summary>
    /// Completes a wizard step with user-provided data.
    /// </summary>
    public async Task<StepCompletionResult> CompleteStepAsync(int stepIndex, object stepData)
    {
        if (stepData == null)
        {
            return new StepCompletionResult
            {
                Success = false,
                ErrorMessage = "Step data cannot be null"
            };
        }

        _completedSteps[stepIndex] = stepData;

        return new StepCompletionResult
        {
            Success = true,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Skips the wizard and creates a default configuration.
    /// </summary>
    public async Task<SetupResult> SkipWizardAndUseDefaultsAsync()
    {
        try
        {
            // Get default settings
            var defaultSettings = DefaultSettingsProvider.GetDefaultSettings();

            // Create default vehicle profile
            var vehicleResult = await _profileService.CreateVehicleProfileAsync("DefaultVehicle", defaultSettings);
            if (!vehicleResult.Success)
            {
                return new SetupResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to create vehicle profile: {vehicleResult.ErrorMessage}"
                };
            }

            // Create default user profile
            var defaultUserPreferences = DefaultSettingsProvider.GetDefaultUserPreferences();
            var userResult = await _profileService.CreateUserProfileAsync("DefaultUser", defaultUserPreferences);
            if (!userResult.Success)
            {
                return new SetupResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to create user profile: {userResult.ErrorMessage}"
                };
            }

            // Switch to the new profiles
            await _profileService.SwitchVehicleProfileAsync("DefaultVehicle", carryOverSession: false);
            await _profileService.SwitchUserProfileAsync("DefaultUser");

            return new SetupResult
            {
                Success = true,
                ErrorMessage = null,
                VehicleProfileName = "DefaultVehicle",
                UserProfileName = "DefaultUser"
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                ErrorMessage = $"Setup failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Completes the entire wizard and creates the initial profile.
    /// </summary>
    public async Task<SetupResult> CompleteWizardAsync()
    {
        try
        {
            // Validate all required steps completed
            var steps = GetWizardSteps();
            var requiredSteps = steps.Where(s => s.IsRequired).ToArray();

            foreach (var step in requiredSteps)
            {
                if (!_completedSteps.ContainsKey(step.StepIndex))
                {
                    return new SetupResult
                    {
                        Success = false,
                        ErrorMessage = $"Required step not completed: {step.Title}"
                    };
                }
            }

            // Get vehicle name from step 0
            var vehicleName = _completedSteps.ContainsKey(0) && _completedSteps[0] is string name
                ? name
                : "MyVehicle";

            // Build settings from completed steps
            var settings = DefaultSettingsProvider.GetDefaultSettings();

            // Apply vehicle configuration from step 1 if provided
            if (_completedSteps.ContainsKey(1) && _completedSteps[1] is VehicleSettings vehicleSettings)
            {
                settings.Vehicle = vehicleSettings;
            }

            // Apply GPS settings from step 2 if provided
            if (_completedSteps.ContainsKey(2) && _completedSteps[2] is GpsSettings gpsSettings)
            {
                settings.Gps = gpsSettings;
            }

            // Apply section control settings from step 3 if provided
            if (_completedSteps.ContainsKey(3) && _completedSteps[3] is SectionControlSettings sectionSettings)
            {
                settings.SectionControl = sectionSettings;
            }

            // Create vehicle profile
            var vehicleResult = await _profileService.CreateVehicleProfileAsync(vehicleName, settings);
            if (!vehicleResult.Success)
            {
                return new SetupResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to create vehicle profile: {vehicleResult.ErrorMessage}"
                };
            }

            // Create default user profile
            var defaultUserPreferences = DefaultSettingsProvider.GetDefaultUserPreferences();
            var userResult = await _profileService.CreateUserProfileAsync("User1", defaultUserPreferences);
            if (!userResult.Success)
            {
                return new SetupResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to create user profile: {userResult.ErrorMessage}"
                };
            }

            // Switch to the new profiles
            await _profileService.SwitchVehicleProfileAsync(vehicleName, carryOverSession: false);
            await _profileService.SwitchUserProfileAsync("User1");

            // Clear completed steps
            _completedSteps.Clear();

            return new SetupResult
            {
                Success = true,
                ErrorMessage = null,
                VehicleProfileName = vehicleName,
                UserProfileName = "User1"
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                ErrorMessage = $"Setup failed: {ex.Message}"
            };
        }
    }
}
