using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.Setup;

/// <summary>
/// Provides first-time setup wizard functionality for new installations.
/// Guides users through initial vehicle configuration with sensible defaults.
/// </summary>
public interface ISetupWizardService
{
    /// <summary>
    /// Checks if first-time setup is needed (no existing profiles).
    /// </summary>
    /// <returns>True if no profiles exist, false otherwise</returns>
    bool IsFirstTimeSetup();

    /// <summary>
    /// Gets the wizard steps for guided setup.
    /// </summary>
    /// <returns>Array of setup wizard steps</returns>
    SetupWizardStep[] GetWizardSteps();

    /// <summary>
    /// Completes a wizard step with user-provided data.
    /// </summary>
    /// <param name="stepIndex">The index of the step to complete</param>
    /// <param name="stepData">The data provided by the user for this step</param>
    /// <returns>Result indicating success or failure</returns>
    Task<StepCompletionResult> CompleteStepAsync(int stepIndex, object stepData);

    /// <summary>
    /// Skips the wizard and creates a default configuration.
    /// Uses sensible defaults from DefaultSettingsProvider.
    /// </summary>
    /// <returns>Result indicating success or failure</returns>
    Task<SetupResult> SkipWizardAndUseDefaultsAsync();

    /// <summary>
    /// Completes the entire wizard and creates the initial profile.
    /// </summary>
    /// <returns>Result indicating success or failure</returns>
    Task<SetupResult> CompleteWizardAsync();
}
