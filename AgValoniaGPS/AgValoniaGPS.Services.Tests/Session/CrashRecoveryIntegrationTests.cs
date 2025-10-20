using System;
using System.Collections.Generic;
using System.Diagnostics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Session;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.Session;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Session;

/// <summary>
/// Integration tests for crash recovery end-to-end workflow.
/// Tests critical scenarios:
/// - Full crash simulation (save, crash, restore)
/// - Performance requirements for snapshot saves
/// - Stale recovery file handling
/// </summary>
[TestFixture]
public class CrashRecoveryIntegrationTests
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
        await _crashRecoveryService.ClearSnapshotAsync();
    }

    [Test]
    public async Task CrashRecoveryEndToEnd_SaveCrashRestore_RestoresFullState()
    {
        // Arrange - Create a full session with all state
        await _sessionManagementService.StartSessionAsync();

        _sessionManagementService.UpdateCurrentField("North Field");
        _sessionManagementService.UpdateCurrentGuidanceLine(GuidanceLineType.ABLine, new
        {
            PointA = new { Easting = 500000.0, Northing = 4500000.0 },
            PointB = new { Easting = 500100.0, Northing = 4500100.0 },
            Heading = 45.0
        });

        var workProgress = new WorkProgressData
        {
            AreaCovered = 15000.0,
            DistanceTravelled = 3500.0,
            TimeWorked = TimeSpan.FromHours(3.5),
            SectionStates = new bool[] { true, true, false }
        };
        _sessionManagementService.UpdateWorkProgress(workProgress);

        // Act - Save snapshot (simulate periodic snapshot before crash)
        await _sessionManagementService.SaveSessionSnapshotAsync();

        // Simulate application crash - do NOT call EndSession
        // Instead, dispose and recreate service (simulates app restart)
        var crashRecoveryService2 = new CrashRecoveryService();
        var sessionService2 = new SessionManagementService(crashRecoveryService2);

        // Act - Restore after "crash"
        var restoreResult = await sessionService2.RestoreLastSessionAsync();

        // Assert - All state should be restored
        Assert.That(restoreResult.Success, Is.True, "Restore should succeed");
        Assert.That(restoreResult.RestoredSession, Is.Not.Null, "Restored session should not be null");
        Assert.That(restoreResult.RestoredSession!.CurrentFieldName, Is.EqualTo("North Field"), "Field name should be restored");
        Assert.That(restoreResult.RestoredSession.CurrentGuidanceLineType, Is.EqualTo(GuidanceLineType.ABLine), "Guidance line type should be restored");
        Assert.That(restoreResult.RestoredSession.WorkProgress, Is.Not.Null, "Work progress should be restored");
        Assert.That(restoreResult.RestoredSession.WorkProgress.AreaCovered, Is.EqualTo(15000.0), "Area covered should be restored");
        Assert.That(restoreResult.RestoredSession.WorkProgress.DistanceTravelled, Is.EqualTo(3500.0), "Distance travelled should be restored");
        Assert.That(restoreResult.RestoredSession.WorkProgress.TimeWorked, Is.EqualTo(TimeSpan.FromHours(3.5)), "Time worked should be restored");

        // Cleanup
        await crashRecoveryService2.ClearSnapshotAsync();
    }

    [Test]
    public async Task CrashRecoverySnapshot_PerformanceTest_CompletesUnder500ms()
    {
        // Arrange - Create a session with substantial data
        await _sessionManagementService.StartSessionAsync();

        _sessionManagementService.UpdateCurrentField("Large Field");

        // Create large work progress data to simulate realistic scenario
        var coverageTrail = new List<Position>();
        for (int i = 0; i < 1000; i++)
        {
            coverageTrail.Add(new Position { Latitude = 500000.0 + i, Longitude = 4500000.0 + i });
        }

        var workProgress = new WorkProgressData
        {
            AreaCovered = 50000.0,
            DistanceTravelled = 10000.0,
            TimeWorked = TimeSpan.FromHours(10),
            CoverageTrail = coverageTrail,
            SectionStates = new bool[32] // Maximum sections
        };
        _sessionManagementService.UpdateWorkProgress(workProgress);

        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _sessionManagementService.SaveSessionSnapshotAsync();
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500),
            $"Crash recovery snapshot took {stopwatch.ElapsedMilliseconds}ms, expected <500ms");
    }

    [Test]
    public async Task CleanShutdown_RemovesCrashRecoveryFile()
    {
        // Arrange
        await _sessionManagementService.StartSessionAsync();
        _sessionManagementService.UpdateCurrentField("Test Field");
        await _sessionManagementService.SaveSessionSnapshotAsync();

        // Verify crash recovery file exists
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.True, "Crash recovery file should exist after snapshot");

        // Act - Clean shutdown
        await _sessionManagementService.EndSessionAsync();
        await _sessionManagementService.ClearCrashRecoveryAsync();

        // Assert
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.False, "Crash recovery file should be removed after clean shutdown");
    }
}
