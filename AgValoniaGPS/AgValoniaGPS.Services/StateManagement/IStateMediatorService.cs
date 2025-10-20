using System.Threading.Tasks;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.StateManagement;

/// <summary>
/// Mediator service for coordinating state changes across multiple services.
/// Implements the Mediator pattern to prevent tight coupling between services and state management.
/// Broadcasts notifications to registered IStateAwareService instances.
/// Performance target: Notification cycle must complete in less than 10ms.
/// </summary>
public interface IStateMediatorService
{
    /// <summary>
    /// Notifies all registered services that application settings have changed.
    /// </summary>
    /// <param name="category">The category of settings that changed</param>
    /// <param name="newSettings">The new settings object</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifySettingsChangedAsync(SettingsCategory category, object newSettings);

    /// <summary>
    /// Notifies all registered services that the active profile has been switched.
    /// </summary>
    /// <param name="profileType">The type of profile that switched (Vehicle or User)</param>
    /// <param name="profileName">The name of the new active profile</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifyProfileSwitchAsync(ProfileType profileType, string profileName);

    /// <summary>
    /// Notifies all registered services that session state has changed.
    /// </summary>
    /// <param name="changeType">The type of session state change</param>
    /// <param name="stateData">Optional state data associated with the change</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifySessionStateChangedAsync(SessionStateChangeType changeType, object? stateData = null);

    /// <summary>
    /// Registers a service to receive state change notifications.
    /// Services are stored using weak references to prevent memory leaks.
    /// </summary>
    /// <param name="service">The service to register for notifications</param>
    void RegisterServiceForNotifications(IStateAwareService service);

    /// <summary>
    /// Unregisters a service from receiving state change notifications.
    /// </summary>
    /// <param name="service">The service to unregister</param>
    void UnregisterService(IStateAwareService service);

    /// <summary>
    /// Gets the count of currently registered services.
    /// Useful for diagnostics and testing.
    /// </summary>
    /// <returns>Number of registered services</returns>
    int GetRegisteredServiceCount();
}
