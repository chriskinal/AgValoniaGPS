using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Session;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.Session;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Session;

/// <summary>
/// Focused tests for SessionManagementService.
/// Tests only critical operations: session start/end, crash recovery, and state updates.
/// </summary>
[TestFixture]
public class SessionManagementServiceTests
{
    private ICrashRecoveryService _crashRecoveryService = null!;
    private ISessionManagementService _sessionManagementService = null!;

    [SetUp]
    public void SetUp()
    {
        _crashRecoveryService = new CrashRecoveryService();
        _sessionManagementService = new SessionManagementService(_crashRecoveryService);
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up crash recovery file after each test
        await _crashRecoveryService.ClearSnapshotAsync();
    }

    [Test]
    public async Task StartSessionAsync_CreatesNewSessionState()
    {
        // Arrange
        SessionStateChangedEventArgs? eventArgs = null;
        _sessionManagementService.SessionStateChanged += (sender, e) => eventArgs = e;

        // Act
        await _sessionManagementService.StartSessionAsync();
        var sessionState = _sessionManagementService.GetCurrentSessionState();

        // Assert
        Assert.That(sessionState, Is.Not.Null, "Session state should be created");
        Assert.That(sessionState!.SessionStartTime, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)), "Session start time should be recent");
        Assert.That(eventArgs, Is.Not.Null, "SessionStateChanged event should be raised");
        Assert.That(eventArgs!.ChangeType, Is.EqualTo(SessionStateChangeType.SessionStarted), "Event type should be SessionStarted");
    }

    [Test]
    public async Task EndSessionAsync_ClearsSessionState()
    {
        // Arrange
        await _sessionManagementService.StartSessionAsync();
        SessionStateChangedEventArgs? eventArgs = null;
        _sessionManagementService.SessionStateChanged += (sender, e) => eventArgs = e;

        // Act
        await _sessionManagementService.EndSessionAsync();
        var sessionState = _sessionManagementService.GetCurrentSessionState();

        // Assert
        Assert.That(sessionState, Is.Null, "Session state should be cleared after end");
        Assert.That(eventArgs, Is.Not.Null, "SessionStateChanged event should be raised");
        Assert.That(eventArgs!.ChangeType, Is.EqualTo(SessionStateChangeType.SessionEnded), "Event type should be SessionEnded");
    }

    [Test]
    public async Task SaveSessionSnapshotAsync_SavesStateToFile()
    {
        // Arrange
        await _sessionManagementService.StartSessionAsync();
        _sessionManagementService.UpdateCurrentField("TestField");

        // Act
        await _sessionManagementService.SaveSessionSnapshotAsync();

        // Assert - verify crash recovery file exists
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.True, "Crash recovery file should exist after snapshot");
    }

    [Test]
    public async Task RestoreLastSessionAsync_RestoresSavedSession()
    {
        // Arrange - create and save a session
        await _sessionManagementService.StartSessionAsync();
        _sessionManagementService.UpdateCurrentField("TestField");
        _sessionManagementService.UpdateCurrentGuidanceLine(GuidanceLineType.ABLine, new { PointA = "test" });
        await _sessionManagementService.SaveSessionSnapshotAsync();

        // Act - restore the session
        var restoreResult = await _sessionManagementService.RestoreLastSessionAsync();

        // Assert
        Assert.That(restoreResult.Success, Is.True, "Restore should succeed");
        Assert.That(restoreResult.RestoredSession, Is.Not.Null, "Restored session should not be null");
        Assert.That(restoreResult.RestoredSession!.CurrentFieldName, Is.EqualTo("TestField"), "Field name should be restored");
        Assert.That(restoreResult.RestoredSession.CurrentGuidanceLineType, Is.EqualTo(GuidanceLineType.ABLine), "Guidance line type should be restored");
    }

    [Test]
    public async Task UpdateCurrentField_UpdatesSessionState()
    {
        // Arrange
        await _sessionManagementService.StartSessionAsync();
        SessionStateChangedEventArgs? eventArgs = null;
        _sessionManagementService.SessionStateChanged += (sender, e) => eventArgs = e;

        // Act
        _sessionManagementService.UpdateCurrentField("NorthField");
        var sessionState = _sessionManagementService.GetCurrentSessionState();

        // Assert
        Assert.That(sessionState!.CurrentFieldName, Is.EqualTo("NorthField"), "Field name should be updated");
        Assert.That(eventArgs, Is.Not.Null, "SessionStateChanged event should be raised");
        Assert.That(eventArgs!.ChangeType, Is.EqualTo(SessionStateChangeType.FieldUpdated), "Event type should be FieldUpdated");
    }

    [Test]
    public async Task UpdateWorkProgress_UpdatesSessionState()
    {
        // Arrange
        await _sessionManagementService.StartSessionAsync();
        var progressData = new WorkProgressData
        {
            AreaCovered = 12500.0,
            DistanceTravelled = 2500.0,
            TimeWorked = TimeSpan.FromHours(2.5)
        };

        // Act
        _sessionManagementService.UpdateWorkProgress(progressData);
        var sessionState = _sessionManagementService.GetCurrentSessionState();

        // Assert
        Assert.That(sessionState!.WorkProgress.AreaCovered, Is.EqualTo(12500.0), "Area covered should be updated");
        Assert.That(sessionState.WorkProgress.DistanceTravelled, Is.EqualTo(2500.0), "Distance travelled should be updated");
        Assert.That(sessionState.WorkProgress.TimeWorked, Is.EqualTo(TimeSpan.FromHours(2.5)), "Time worked should be updated");
    }

    [Test]
    public async Task ClearCrashRecoveryAsync_RemovesCrashRecoveryFile()
    {
        // Arrange - create a crash recovery file
        await _sessionManagementService.StartSessionAsync();
        await _sessionManagementService.SaveSessionSnapshotAsync();
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.True, "Crash recovery file should exist");

        // Act
        await _sessionManagementService.ClearCrashRecoveryAsync();

        // Assert
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.False, "Crash recovery file should be removed");
    }
}
