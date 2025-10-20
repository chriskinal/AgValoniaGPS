using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.StateManagement;
using Xunit;

namespace AgValoniaGPS.Services.Tests.StateManagement;

/// <summary>
/// Focused tests for StateMediatorService.
/// Tests critical operations: service registration, notification routing, exception handling, and performance.
/// Limited to 8 highly focused tests as per task requirements.
/// </summary>
public class StateMediatorServiceTests
{
    [Fact]
    public void RegisterServiceForNotifications_AddsServiceToRegisteredList()
    {
        // Arrange
        var mediator = new StateMediatorService();
        var mockService = new MockStateAwareService();

        // Act
        mediator.RegisterServiceForNotifications(mockService);

        // Assert
        Assert.Equal(1, mediator.GetRegisteredServiceCount());
    }

    [Fact]
    public async Task NotifySettingsChangedAsync_CallsOnSettingsChangedOnAllRegisteredServices()
    {
        // Arrange
        var mediator = new StateMediatorService();
        var service1 = new MockStateAwareService();
        var service2 = new MockStateAwareService();
        mediator.RegisterServiceForNotifications(service1);
        mediator.RegisterServiceForNotifications(service2);

        var newSettings = new { TestSetting = "value" };

        // Act
        await mediator.NotifySettingsChangedAsync(SettingsCategory.Vehicle, newSettings);

        // Assert
        Assert.Equal(1, service1.SettingsChangedCount);
        Assert.Equal(SettingsCategory.Vehicle, service1.LastSettingsCategory);
        Assert.Same(newSettings, service1.LastSettings);

        Assert.Equal(1, service2.SettingsChangedCount);
        Assert.Equal(SettingsCategory.Vehicle, service2.LastSettingsCategory);
        Assert.Same(newSettings, service2.LastSettings);
    }

    [Fact]
    public async Task NotifyProfileSwitchAsync_CallsOnProfileSwitchedOnAllRegisteredServices()
    {
        // Arrange
        var mediator = new StateMediatorService();
        var service = new MockStateAwareService();
        mediator.RegisterServiceForNotifications(service);

        // Act
        await mediator.NotifyProfileSwitchAsync(ProfileType.Vehicle, "Deere5055e");

        // Assert
        Assert.Equal(1, service.ProfileSwitchedCount);
        Assert.Equal(ProfileType.Vehicle, service.LastProfileType);
        Assert.Equal("Deere5055e", service.LastProfileName);
    }

    [Fact]
    public async Task NotifySessionStateChangedAsync_CallsOnSessionStateChangedOnAllRegisteredServices()
    {
        // Arrange
        var mediator = new StateMediatorService();
        var service = new MockStateAwareService();
        mediator.RegisterServiceForNotifications(service);

        var stateData = new { FieldName = "North40" };

        // Act
        await mediator.NotifySessionStateChangedAsync(SessionStateChangeType.FieldUpdated, stateData);

        // Assert
        Assert.Equal(1, service.SessionStateChangedCount);
        Assert.Equal(SessionStateChangeType.FieldUpdated, service.LastSessionChangeType);
        Assert.Same(stateData, service.LastSessionStateData);
    }

    [Fact]
    public async Task NotifySettingsChangedAsync_OneServiceFailure_DoesNotAffectOtherServices()
    {
        // Arrange
        var mediator = new StateMediatorService();
        var service1 = new MockStateAwareService { ThrowOnSettingsChanged = true };
        var service2 = new MockStateAwareService();
        mediator.RegisterServiceForNotifications(service1);
        mediator.RegisterServiceForNotifications(service2);

        var newSettings = new { TestSetting = "value" };

        // Act
        await mediator.NotifySettingsChangedAsync(SettingsCategory.Steering, newSettings);

        // Assert - service2 should still be notified even though service1 threw
        Assert.Equal(1, service2.SettingsChangedCount);
        Assert.Equal(SettingsCategory.Steering, service2.LastSettingsCategory);
    }

    [Fact]
    public void UnregisterService_RemovesServiceFromRegisteredList()
    {
        // Arrange
        var mediator = new StateMediatorService();
        var service1 = new MockStateAwareService();
        var service2 = new MockStateAwareService();
        mediator.RegisterServiceForNotifications(service1);
        mediator.RegisterServiceForNotifications(service2);

        // Act
        mediator.UnregisterService(service1);

        // Assert
        Assert.Equal(1, mediator.GetRegisteredServiceCount());
    }

    [Fact]
    public void WeakReferences_ServiceGarbageCollected_AutomaticallyRemovedFromList()
    {
        // Arrange
        var mediator = new StateMediatorService();
        RegisterAndDiscardService(mediator);

        // Force garbage collection to clean up weak references
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act - trigger cleanup by getting active services
        var count = mediator.GetRegisteredServiceCount();

        // Assert - weak reference should be cleaned up
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task NotifySettingsChangedAsync_CompletesWithinPerformanceTarget()
    {
        // Arrange
        var mediator = new StateMediatorService();

        // Register 10 services to simulate realistic scenario
        for (int i = 0; i < 10; i++)
        {
            mediator.RegisterServiceForNotifications(new MockStateAwareService());
        }

        var newSettings = new { TestSetting = "value" };
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await mediator.NotifySettingsChangedAsync(SettingsCategory.Guidance, newSettings);
        stopwatch.Stop();

        // Assert - must complete in less than 10ms (performance requirement)
        Assert.True(stopwatch.ElapsedMilliseconds < 10,
            $"Notification took {stopwatch.ElapsedMilliseconds}ms, expected < 10ms");
    }

    /// <summary>
    /// Helper method to create and discard a service for weak reference testing.
    /// The service is created in a separate scope to ensure it can be garbage collected.
    /// </summary>
    private void RegisterAndDiscardService(StateMediatorService mediator)
    {
        var service = new MockStateAwareService();
        mediator.RegisterServiceForNotifications(service);
        // service goes out of scope here and becomes eligible for GC
    }
}

/// <summary>
/// Mock implementation of IStateAwareService for testing.
/// Tracks notification calls and allows simulating failures.
/// </summary>
internal class MockStateAwareService : IStateAwareService
{
    public int SettingsChangedCount { get; private set; }
    public SettingsCategory? LastSettingsCategory { get; private set; }
    public object? LastSettings { get; private set; }

    public int ProfileSwitchedCount { get; private set; }
    public ProfileType? LastProfileType { get; private set; }
    public string? LastProfileName { get; private set; }

    public int SessionStateChangedCount { get; private set; }
    public SessionStateChangeType? LastSessionChangeType { get; private set; }
    public object? LastSessionStateData { get; private set; }

    public bool ThrowOnSettingsChanged { get; set; }
    public bool ThrowOnProfileSwitched { get; set; }
    public bool ThrowOnSessionStateChanged { get; set; }

    public Task OnSettingsChangedAsync(SettingsCategory category, object newSettings)
    {
        if (ThrowOnSettingsChanged)
        {
            throw new InvalidOperationException("Test exception from OnSettingsChangedAsync");
        }

        SettingsChangedCount++;
        LastSettingsCategory = category;
        LastSettings = newSettings;
        return Task.CompletedTask;
    }

    public Task OnProfileSwitchedAsync(ProfileType profileType, string profileName)
    {
        if (ThrowOnProfileSwitched)
        {
            throw new InvalidOperationException("Test exception from OnProfileSwitchedAsync");
        }

        ProfileSwitchedCount++;
        LastProfileType = profileType;
        LastProfileName = profileName;
        return Task.CompletedTask;
    }

    public Task OnSessionStateChangedAsync(SessionStateChangeType changeType, object? stateData)
    {
        if (ThrowOnSessionStateChanged)
        {
            throw new InvalidOperationException("Test exception from OnSessionStateChangedAsync");
        }

        SessionStateChangedCount++;
        LastSessionChangeType = changeType;
        LastSessionStateData = stateData;
        return Task.CompletedTask;
    }
}
