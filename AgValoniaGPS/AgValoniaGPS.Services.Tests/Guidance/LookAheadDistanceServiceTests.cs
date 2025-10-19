using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance;

/// <summary>
/// Unit tests for LookAheadDistanceService
/// Tests adaptive look-ahead calculations for speed, cross-track error zones, curvature, and vehicle type
/// Performance target: <1ms per calculation
/// </summary>
public class LookAheadDistanceServiceTests
{
    private static VehicleConfiguration CreateTestConfiguration()
    {
        return new VehicleConfiguration
        {
            GoalPointLookAheadHold = 4.0,
            GoalPointLookAheadMult = 1.4,
            GoalPointAcquireFactor = 1.5,
            MinLookAheadDistance = 2.0
        };
    }

    [Fact]
    public void CalculateLookAheadDistance_OnLine_ReturnsHoldDistance()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var service = new LookAheadDistanceService(config);
        double speed = 3.0; // m/s
        double crossTrackError = 0.05; // meters - well within on-line threshold (0.1m)
        double curvature = 0.0; // straight line
        var vehicleType = VehicleType.Tractor;
        bool isAutoSteerActive = true;

        // Act
        double result = service.CalculateLookAheadDistance(speed, crossTrackError, curvature, vehicleType, isAutoSteerActive);

        // Assert - Should return hold distance when on-line (XTE ≤ 0.1m)
        Assert.Equal(config.GoalPointLookAheadHold, result, 2);
    }

    [Fact]
    public void CalculateLookAheadDistance_AcquireMode_ReturnsAcquireDistance()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var service = new LookAheadDistanceService(config);
        double speed = 3.0; // m/s
        double crossTrackError = 0.5; // meters - exceeds acquire threshold (0.4m)
        double curvature = 0.0; // straight line
        var vehicleType = VehicleType.Tractor;
        bool isAutoSteerActive = true;

        // Act
        double result = service.CalculateLookAheadDistance(speed, crossTrackError, curvature, vehicleType, isAutoSteerActive);

        // Assert - Should return hold * acquire factor when XTE ≥ 0.4m
        double expectedAcquireDistance = config.GoalPointLookAheadHold * config.GoalPointAcquireFactor;
        Assert.Equal(expectedAcquireDistance, result, 2);
    }

    [Fact]
    public void CalculateLookAheadDistance_TransitionZone_InterpolatesCorrectly()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var service = new LookAheadDistanceService(config);
        double speed = 3.0; // m/s
        double crossTrackError = 0.25; // meters - midpoint of transition zone (0.1-0.4m)
        double curvature = 0.0; // straight line
        var vehicleType = VehicleType.Tractor;
        bool isAutoSteerActive = true;

        // Act
        double result = service.CalculateLookAheadDistance(speed, crossTrackError, curvature, vehicleType, isAutoSteerActive);

        // Assert - Should interpolate between hold and acquire distance
        double holdDistance = config.GoalPointLookAheadHold; // 4.0
        double acquireDistance = holdDistance * config.GoalPointAcquireFactor; // 6.0
        // At midpoint (0.25), should be halfway between 4.0 and 6.0 = 5.0
        Assert.True(result > holdDistance && result < acquireDistance);
        Assert.Equal(5.0, result, 1); // ±0.1m tolerance for interpolation
    }

    [Fact]
    public void CalculateLookAheadDistance_MinimumEnforcement_NeverBelowMinimum()
    {
        // Arrange
        var config = CreateTestConfiguration();
        config.MinLookAheadDistance = 2.0;
        config.GoalPointLookAheadHold = 0.5; // Artificially low to test minimum enforcement
        var service = new LookAheadDistanceService(config);
        double speed = 0.1; // very low speed
        double crossTrackError = 0.05; // on-line
        double curvature = 0.0;
        var vehicleType = VehicleType.Tractor;
        bool isAutoSteerActive = true;

        // Act
        double result = service.CalculateLookAheadDistance(speed, crossTrackError, curvature, vehicleType, isAutoSteerActive);

        // Assert - Should never return less than minimum distance
        Assert.True(result >= config.MinLookAheadDistance);
        Assert.Equal(2.0, result, 2);
    }

    [Fact]
    public void CalculateLookAheadDistance_HarvesterVehicle_AppliesScaling()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var service = new LookAheadDistanceService(config);
        double speed = 3.0; // m/s
        double crossTrackError = 0.05; // on-line
        double curvature = 0.0; // straight line
        bool isAutoSteerActive = true;

        // Act - Test with Tractor (baseline)
        double tractorResult = service.CalculateLookAheadDistance(speed, crossTrackError, curvature, VehicleType.Tractor, isAutoSteerActive);

        // Act - Test with Harvester (10% larger)
        double harvesterResult = service.CalculateLookAheadDistance(speed, crossTrackError, curvature, VehicleType.Harvester, isAutoSteerActive);

        // Assert - Harvester should be 10% larger than tractor
        double expectedHarvesterDistance = tractorResult * 1.1;
        Assert.Equal(expectedHarvesterDistance, harvesterResult, 2);
    }

    [Fact]
    public void CalculateLookAheadDistance_TightCurve_ReducesDistance()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var service = new LookAheadDistanceService(config);
        double speed = 3.0; // m/s
        double crossTrackError = 0.05; // on-line
        var vehicleType = VehicleType.Tractor;
        bool isAutoSteerActive = true;

        // Act - Test with straight line (no curvature)
        double straightResult = service.CalculateLookAheadDistance(speed, crossTrackError, 0.0, vehicleType, isAutoSteerActive);

        // Act - Test with tight curve (curvature > 0.05)
        double curveResult = service.CalculateLookAheadDistance(speed, crossTrackError, 0.1, vehicleType, isAutoSteerActive);

        // Assert - Tight curve should reduce look-ahead by 20%
        double expectedCurveDistance = straightResult * 0.8;
        Assert.Equal(expectedCurveDistance, curveResult, 2);
        Assert.True(curveResult < straightResult);
    }
}
