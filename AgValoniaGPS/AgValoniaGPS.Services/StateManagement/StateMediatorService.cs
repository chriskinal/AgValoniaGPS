using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgValoniaGPS.Models.StateManagement;
using Microsoft.Extensions.Logging;

namespace AgValoniaGPS.Services.StateManagement;

/// <summary>
/// Implementation of the State Mediator pattern for cross-service coordination.
/// Maintains weak references to registered services to prevent memory leaks.
/// Broadcasts state change notifications to all registered IStateAwareService instances.
/// Exception handling ensures one service failure doesn't affect others.
/// Performance target: Complete notification cycle in less than 10ms.
/// </summary>
public class StateMediatorService : IStateMediatorService
{
    private readonly List<WeakReference<IStateAwareService>> _registeredServices;
    private readonly object _servicesLock = new object();
    private readonly ILogger<StateMediatorService>? _logger;

    /// <summary>
    /// Creates a new StateMediatorService instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics</param>
    public StateMediatorService(ILogger<StateMediatorService>? logger = null)
    {
        _registeredServices = new List<WeakReference<IStateAwareService>>();
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task NotifySettingsChangedAsync(SettingsCategory category, object newSettings)
    {
        if (newSettings == null)
        {
            throw new ArgumentNullException(nameof(newSettings));
        }

        _logger?.LogDebug("Notifying settings change: {Category}", category);

        var services = GetActiveServices();
        var tasks = new List<Task>();

        foreach (var service in services)
        {
            tasks.Add(NotifyServiceSafelyAsync(
                service,
                async s => await s.OnSettingsChangedAsync(category, newSettings),
                $"OnSettingsChangedAsync({category})"));
        }

        await Task.WhenAll(tasks);

        _logger?.LogDebug("Settings change notification completed for {Count} services", services.Count);
    }

    /// <inheritdoc/>
    public async Task NotifyProfileSwitchAsync(ProfileType profileType, string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ArgumentException("Profile name cannot be null or empty", nameof(profileName));
        }

        _logger?.LogDebug("Notifying profile switch: {ProfileType} -> {ProfileName}", profileType, profileName);

        var services = GetActiveServices();
        var tasks = new List<Task>();

        foreach (var service in services)
        {
            tasks.Add(NotifyServiceSafelyAsync(
                service,
                async s => await s.OnProfileSwitchedAsync(profileType, profileName),
                $"OnProfileSwitchedAsync({profileType}, {profileName})"));
        }

        await Task.WhenAll(tasks);

        _logger?.LogDebug("Profile switch notification completed for {Count} services", services.Count);
    }

    /// <inheritdoc/>
    public async Task NotifySessionStateChangedAsync(SessionStateChangeType changeType, object? stateData = null)
    {
        _logger?.LogDebug("Notifying session state change: {ChangeType}", changeType);

        var services = GetActiveServices();
        var tasks = new List<Task>();

        foreach (var service in services)
        {
            tasks.Add(NotifyServiceSafelyAsync(
                service,
                async s => await s.OnSessionStateChangedAsync(changeType, stateData),
                $"OnSessionStateChangedAsync({changeType})"));
        }

        await Task.WhenAll(tasks);

        _logger?.LogDebug("Session state change notification completed for {Count} services", services.Count);
    }

    /// <inheritdoc/>
    public void RegisterServiceForNotifications(IStateAwareService service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        lock (_servicesLock)
        {
            // Check if service is already registered
            var activeServices = GetActiveServices();
            if (activeServices.Any(s => ReferenceEquals(s, service)))
            {
                _logger?.LogWarning("Service {ServiceType} is already registered", service.GetType().Name);
                return;
            }

            _registeredServices.Add(new WeakReference<IStateAwareService>(service));
            _logger?.LogDebug("Registered service: {ServiceType} (Total: {Count})",
                service.GetType().Name,
                GetActiveServices().Count);
        }
    }

    /// <inheritdoc/>
    public void UnregisterService(IStateAwareService service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        lock (_servicesLock)
        {
            var toRemove = _registeredServices
                .Where(wr =>
                {
                    if (wr.TryGetTarget(out var target))
                    {
                        return ReferenceEquals(target, service);
                    }
                    return false;
                })
                .ToList();

            foreach (var weakRef in toRemove)
            {
                _registeredServices.Remove(weakRef);
            }

            if (toRemove.Any())
            {
                _logger?.LogDebug("Unregistered service: {ServiceType} (Total: {Count})",
                    service.GetType().Name,
                    GetActiveServices().Count);
            }

            // Clean up dead references while we're at it
            CleanupDeadReferences();
        }
    }

    /// <inheritdoc/>
    public int GetRegisteredServiceCount()
    {
        lock (_servicesLock)
        {
            return GetActiveServices().Count;
        }
    }

    /// <summary>
    /// Gets all currently active (alive) services from weak references.
    /// Automatically cleans up dead references.
    /// </summary>
    /// <returns>List of active IStateAwareService instances</returns>
    private List<IStateAwareService> GetActiveServices()
    {
        lock (_servicesLock)
        {
            var activeServices = new List<IStateAwareService>();

            foreach (var weakRef in _registeredServices)
            {
                if (weakRef.TryGetTarget(out var service))
                {
                    activeServices.Add(service);
                }
            }

            // Clean up dead references if we found any
            var deadCount = _registeredServices.Count - activeServices.Count;
            if (deadCount > 0)
            {
                CleanupDeadReferences();
                _logger?.LogDebug("Cleaned up {Count} dead service references", deadCount);
            }

            return activeServices;
        }
    }

    /// <summary>
    /// Removes weak references that no longer point to active objects.
    /// Must be called within a lock on _servicesLock.
    /// </summary>
    private void CleanupDeadReferences()
    {
        _registeredServices.RemoveAll(wr => !wr.TryGetTarget(out _));
    }

    /// <summary>
    /// Safely notifies a service, catching and logging any exceptions.
    /// Ensures that one service's failure doesn't affect other services.
    /// </summary>
    /// <param name="service">The service to notify</param>
    /// <param name="notificationAction">The async notification action to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <returns>Task representing the async operation</returns>
    private async Task NotifyServiceSafelyAsync(
        IStateAwareService service,
        Func<IStateAwareService, Task> notificationAction,
        string operationName)
    {
        try
        {
            await notificationAction(service);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error notifying service {ServiceType} via {Operation}",
                service.GetType().Name,
                operationName);
        }
    }
}
