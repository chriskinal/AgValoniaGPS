using System.Threading.Tasks;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.StateManagement;

/// <summary>
/// Interface for services that need to receive state change notifications.
/// Services implement this interface to participate in coordinated state management via the Mediator pattern.
/// </summary>
public interface IStateAwareService
{
    /// <summary>
    /// Called when application settings change.
    /// Services should update their internal state based on the new settings.
    /// </summary>
    /// <param name="category">The category of settings that changed</param>
    /// <param name="newSettings">The new settings object</param>
    Task OnSettingsChangedAsync(SettingsCategory category, object newSettings);

    /// <summary>
    /// Called when the active profile is switched.
    /// Services should reload their configuration based on the new profile.
    /// </summary>
    /// <param name="profileType">The type of profile that switched (Vehicle or User)</param>
    /// <param name="profileName">The name of the new active profile</param>
    Task OnProfileSwitchedAsync(ProfileType profileType, string profileName);

    /// <summary>
    /// Called when session state changes.
    /// Services can update their behavior based on session state (start, end, field change, etc.).
    /// </summary>
    /// <param name="changeType">The type of session state change</param>
    /// <param name="stateData">Optional state data associated with the change</param>
    Task OnSessionStateChangedAsync(SessionStateChangeType changeType, object? stateData);
}
