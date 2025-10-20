using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Session;
using AgValoniaGPS.Services.Session;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Session;

/// <summary>
/// Tests for CrashRecoveryService focusing on atomic write and file age validation.
/// </summary>
[TestFixture]
public class CrashRecoveryServiceTests
{
    private ICrashRecoveryService _crashRecoveryService = null!;

    [SetUp]
    public void SetUp()
    {
        _crashRecoveryService = new CrashRecoveryService();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up crash recovery file after each test
        await _crashRecoveryService.ClearSnapshotAsync();
    }

    [Test]
    public async Task SaveSnapshotAsync_CreatesRecoveryFile()
    {
        // Arrange
        var sessionState = new SessionState
        {
            SessionStartTime = DateTime.UtcNow,
            CurrentFieldName = "TestField",
            VehicleProfileName = "TestVehicle",
            UserProfileName = "TestUser"
        };

        // Act
        await _crashRecoveryService.SaveSnapshotAsync(sessionState);

        // Assert
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.True, "Crash recovery file should be created");
    }

    [Test]
    public async Task RestoreSnapshotAsync_RestoresCorrectData()
    {
        // Arrange
        var originalState = new SessionState
        {
            SessionStartTime = DateTime.UtcNow.AddHours(-1),
            CurrentFieldName = "NorthField",
            CurrentGuidanceLineType = GuidanceLineType.CurveLine,
            VehicleProfileName = "Deere5055e",
            UserProfileName = "John",
            WorkProgress = new WorkProgressData
            {
                AreaCovered = 10000.0,
                DistanceTravelled = 1500.0
            }
        };

        await _crashRecoveryService.SaveSnapshotAsync(originalState);

        // Act
        var result = await _crashRecoveryService.RestoreSnapshotAsync();

        // Assert
        Assert.That(result.Success, Is.True, "Restore should succeed");
        Assert.That(result.RestoredSession, Is.Not.Null, "Restored session should not be null");
        Assert.That(result.RestoredSession!.CurrentFieldName, Is.EqualTo("NorthField"), "Field name should match");
        Assert.That(result.RestoredSession.VehicleProfileName, Is.EqualTo("Deere5055e"), "Vehicle profile should match");
        Assert.That(result.RestoredSession.WorkProgress.AreaCovered, Is.EqualTo(10000.0), "Work progress should be restored");
    }

    [Test]
    public async Task SaveSnapshotAsync_PerformanceTest_CompletesIn500ms()
    {
        // Arrange
        var sessionState = new SessionState
        {
            SessionStartTime = DateTime.UtcNow,
            CurrentFieldName = "TestField",
            WorkProgress = new WorkProgressData
            {
                AreaCovered = 50000.0,
                DistanceTravelled = 5000.0,
                CoverageTrail = new System.Collections.Generic.List<Position>()
            }
        };

        // Add some coverage trail data to make save operation more realistic
        for (int i = 0; i < 1000; i++)
        {
            sessionState.WorkProgress.CoverageTrail.Add(new Position { Latitude = 1000.0 + i, Longitude = 2000.0 + i });
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        await _crashRecoveryService.SaveSnapshotAsync(sessionState);
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500),
            $"Snapshot save should complete in less than 500ms (actual: {stopwatch.ElapsedMilliseconds}ms)");
    }

    [Test]
    public async Task ClearSnapshotAsync_RemovesFile()
    {
        // Arrange
        var sessionState = new SessionState
        {
            SessionStartTime = DateTime.UtcNow,
            CurrentFieldName = "TestField"
        };
        await _crashRecoveryService.SaveSnapshotAsync(sessionState);
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.True, "File should exist before clear");

        // Act
        await _crashRecoveryService.ClearSnapshotAsync();

        // Assert
        Assert.That(_crashRecoveryService.HasCrashRecoveryFile(), Is.False, "File should be removed after clear");
    }
}
