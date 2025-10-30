using System;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Interfaces;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Heading;

[TestFixture]
public class HeadingCalculatorServiceTests
{
    private HeadingCalculatorService _service;
    private const double Tolerance = 0.001; // 1 millirad (~0.057 degrees)

    [SetUp]
    public void SetUp()
    {
        _service = new HeadingCalculatorService();
    }

    #region FixToFix Heading Tests

    [Test]
    public void CalculateFixToFixHeading_NorthMovement_ReturnsZeroRadians()
    {
        // Arrange - moving north
        var data = new FixToFixHeadingData
        {
            CurrentEasting = 500000,
            CurrentNorthing = 4500010,
            PreviousEasting = 500000,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0
        };

        // Act
        double heading = _service.CalculateFixToFixHeading(data);

        // Assert - north is 0 radians
        Assert.That(heading, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void CalculateFixToFixHeading_EastMovement_ReturnsPiOver2()
    {
        // Arrange - moving east
        var data = new FixToFixHeadingData
        {
            CurrentEasting = 500010,
            CurrentNorthing = 4500000,
            PreviousEasting = 500000,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0
        };

        // Act
        double heading = _service.CalculateFixToFixHeading(data);

        // Assert - east is π/2 radians
        Assert.That(heading, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void CalculateFixToFixHeading_SouthMovement_ReturnsPi()
    {
        // Arrange - moving south
        var data = new FixToFixHeadingData
        {
            CurrentEasting = 500000,
            CurrentNorthing = 4500000,
            PreviousEasting = 500000,
            PreviousNorthing = 4500010,
            MinimumDistance = 1.0
        };

        // Act
        double heading = _service.CalculateFixToFixHeading(data);

        // Assert - south is π radians
        Assert.That(heading, Is.EqualTo(Math.PI).Within(Tolerance));
    }

    [Test]
    public void CalculateFixToFixHeading_WestMovement_Returns3PiOver2()
    {
        // Arrange - moving west
        var data = new FixToFixHeadingData
        {
            CurrentEasting = 500000,
            CurrentNorthing = 4500000,
            PreviousEasting = 500010,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0
        };

        // Act
        double heading = _service.CalculateFixToFixHeading(data);

        // Assert - west is 3π/2 radians
        Assert.That(heading, Is.EqualTo(3 * Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void CalculateFixToFixHeading_InsufficientMovement_ReturnsLastHeading()
    {
        // Arrange - first establish a heading
        var firstMove = new FixToFixHeadingData
        {
            CurrentEasting = 500010,
            CurrentNorthing = 4500000,
            PreviousEasting = 500000,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0
        };
        _service.CalculateFixToFixHeading(firstMove);

        // Now move less than minimum distance
        var tinyMove = new FixToFixHeadingData
        {
            CurrentEasting = 500010.1,
            CurrentNorthing = 4500000,
            PreviousEasting = 500010,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0 // Only moved 0.1m
        };

        // Act
        double heading = _service.CalculateFixToFixHeading(tinyMove);

        // Assert - should return previous heading (east = π/2)
        Assert.That(heading, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void CalculateFixToFixHeading_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.CalculateFixToFixHeading(null));
    }

    [Test]
    public void CalculateFixToFixHeading_FiresHeadingChangedEvent()
    {
        // Arrange
        HeadingUpdate? capturedUpdate = null;
        _service.HeadingChanged += (sender, update) => capturedUpdate = update;

        var data = new FixToFixHeadingData
        {
            CurrentEasting = 500010,
            CurrentNorthing = 4500000,
            PreviousEasting = 500000,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0
        };

        // Act
        _service.CalculateFixToFixHeading(data);

        // Assert
        Assert.That(capturedUpdate, Is.Not.Null);
        Assert.That(capturedUpdate.Source, Is.EqualTo(HeadingSource.FixToFix));
        Assert.That(capturedUpdate.Heading, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    #endregion

    #region VTG Heading Tests

    [Test]
    public void ProcessVtgHeading_NorthHeading_ConvertsCorrectly()
    {
        // Arrange - 0 degrees (north)
        double vtgDegrees = 0;

        // Act
        double heading = _service.ProcessVtgHeading(vtgDegrees);

        // Assert
        Assert.That(heading, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void ProcessVtgHeading_EastHeading_ConvertsCorrectly()
    {
        // Arrange - 90 degrees (east)
        double vtgDegrees = 90;

        // Act
        double heading = _service.ProcessVtgHeading(vtgDegrees);

        // Assert - 90° = π/2 radians
        Assert.That(heading, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void ProcessVtgHeading_360Degrees_NormalizesTo0()
    {
        // Arrange
        double vtgDegrees = 360;

        // Act
        double heading = _service.ProcessVtgHeading(vtgDegrees);

        // Assert - 360° should normalize to 0
        Assert.That(heading, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void ProcessVtgHeading_FiresHeadingChangedEvent()
    {
        // Arrange
        HeadingUpdate? capturedUpdate = null;
        _service.HeadingChanged += (sender, update) => capturedUpdate = update;

        // Act
        _service.ProcessVtgHeading(45);

        // Assert
        Assert.That(capturedUpdate, Is.Not.Null);
        Assert.That(capturedUpdate.Source, Is.EqualTo(HeadingSource.VTG));
    }

    #endregion

    #region Dual Antenna Heading Tests

    [Test]
    public void ProcessDualAntennaHeading_ConvertsDegreesToRadians()
    {
        // Arrange
        double dualDegrees = 180;

        // Act
        double heading = _service.ProcessDualAntennaHeading(dualDegrees);

        // Assert - 180° = π radians
        Assert.That(heading, Is.EqualTo(Math.PI).Within(Tolerance));
    }

    [Test]
    public void ProcessDualAntennaHeading_FiresHeadingChangedEvent()
    {
        // Arrange
        HeadingUpdate? capturedUpdate = null;
        _service.HeadingChanged += (sender, update) => capturedUpdate = update;

        // Act
        _service.ProcessDualAntennaHeading(270);

        // Assert
        Assert.That(capturedUpdate, Is.Not.Null);
        Assert.That(capturedUpdate.Source, Is.EqualTo(HeadingSource.DualAntenna));
        Assert.That(capturedUpdate.Heading, Is.EqualTo(3 * Math.PI / 2).Within(Tolerance));
    }

    #endregion

    #region IMU Fusion Tests

    [Test]
    public void FuseImuHeading_InitialFusion_ReturnsBlendedHeading()
    {
        // Arrange - GPS says north (0), IMU says east (90°)
        double gpsHeading = 0;
        double imuDegrees = 90;
        double fusionWeight = 0.1; // Low weight = trust IMU more

        // Act
        double fusedHeading = _service.FuseImuHeading(gpsHeading, imuDegrees, fusionWeight);

        // Assert - should be closer to IMU (east) due to low GPS weight
        // With initial fusion, expect heading near π/2 (east)
        Assert.That(fusedHeading, Is.GreaterThan(0));
        Assert.That(fusedHeading, Is.LessThan(Math.PI));
    }

    [Test]
    public void FuseImuHeading_InvalidFusionWeight_ThrowsException()
    {
        // Arrange
        double gpsHeading = 0;
        double imuDegrees = 0;

        // Act & Assert - fusion weight < 0
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.FuseImuHeading(gpsHeading, imuDegrees, -0.1));

        // Act & Assert - fusion weight > 1
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.FuseImuHeading(gpsHeading, imuDegrees, 1.5));
    }

    [Test]
    public void FuseImuHeading_HighGpsWeight_ConvergesToGps()
    {
        // Arrange - Run fusion multiple times with high GPS weight
        double gpsHeading = 0; // North
        double imuDegrees = 90; // East
        double fusionWeight = 0.9; // High weight = trust GPS more

        // Act - fuse multiple times to converge
        double fusedHeading = gpsHeading;
        for (int i = 0; i < 10; i++)
        {
            fusedHeading = _service.FuseImuHeading(gpsHeading, imuDegrees, fusionWeight);
        }

        // Assert - should converge towards GPS heading (north)
        // After multiple iterations, offset should bring IMU closer to GPS
        Assert.That(_service.ImuGpsOffset, Is.Not.EqualTo(0));
    }

    [Test]
    public void FuseImuHeading_FiresHeadingChangedEvent()
    {
        // Arrange
        HeadingUpdate? capturedUpdate = null;
        _service.HeadingChanged += (sender, update) => capturedUpdate = update;

        // Act
        _service.FuseImuHeading(0, 0, 0.5);

        // Assert
        Assert.That(capturedUpdate, Is.Not.Null);
        Assert.That(capturedUpdate.Source, Is.EqualTo(HeadingSource.Fused));
    }

    [Test]
    public void FuseImuHeading_WraparoundCase_HandlesCorrectly()
    {
        // Arrange - GPS near 0° (north), IMU near 360° (also north)
        double gpsHeading = 0.1; // Slightly east of north
        double imuDegrees = 359; // Slightly west of north
        double fusionWeight = 0.5;

        // Act
        double fusedHeading = _service.FuseImuHeading(gpsHeading, imuDegrees, fusionWeight);

        // Assert - should be close to 0/2π (north), not π (south)
        Assert.That(fusedHeading < 0.5 || fusedHeading > 6.0, Is.True,
            "Fused heading should be near 0/2π, not in the middle");
    }

    #endregion

    #region Roll Correction Tests

    [Test]
    public void CalculateRollCorrectionDistance_NoRoll_ReturnsZero()
    {
        // Arrange
        double heading = 0;
        double rollDegrees = 0;
        double antennaHeight = 3.0;

        // Act
        double correction = _service.CalculateRollCorrectionDistance(heading, rollDegrees, antennaHeight);

        // Assert
        Assert.That(correction, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void CalculateRollCorrectionDistance_RightRoll_ReturnsNegativeCorrection()
    {
        // Arrange - rolling right (positive roll)
        double heading = 0;
        double rollDegrees = 10; // 10° right roll
        double antennaHeight = 3.0;

        // Act
        double correction = _service.CalculateRollCorrectionDistance(heading, rollDegrees, antennaHeight);

        // Assert - should be negative (antenna shifts left relative to pivot)
        Assert.That(correction, Is.LessThan(0));
    }

    [Test]
    public void CalculateRollCorrectionDistance_LeftRoll_ReturnsPositiveCorrection()
    {
        // Arrange - rolling left (negative roll)
        double heading = 0;
        double rollDegrees = -10; // 10° left roll
        double antennaHeight = 3.0;

        // Act
        double correction = _service.CalculateRollCorrectionDistance(heading, rollDegrees, antennaHeight);

        // Assert - should be positive (antenna shifts right relative to pivot)
        Assert.That(correction, Is.GreaterThan(0));
    }

    [Test]
    public void CalculateRollCorrectionDistance_ZeroAntennaHeight_ReturnsZero()
    {
        // Arrange
        double heading = 0;
        double rollDegrees = 10;
        double antennaHeight = 0;

        // Act
        double correction = _service.CalculateRollCorrectionDistance(heading, rollDegrees, antennaHeight);

        // Assert
        Assert.That(correction, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void CalculateRollCorrectionDistance_SmallRoll_ReturnsZero()
    {
        // Arrange - very small roll (below threshold)
        double heading = 0;
        double rollDegrees = 0.005; // Below 0.01° threshold
        double antennaHeight = 3.0;

        // Act
        double correction = _service.CalculateRollCorrectionDistance(heading, rollDegrees, antennaHeight);

        // Assert
        Assert.That(correction, Is.EqualTo(0).Within(Tolerance));
    }

    #endregion

    #region Optimal Source Selection Tests

    [Test]
    public void DetermineOptimalSource_DualAntennaAvailable_ReturnsDualAntenna()
    {
        // Arrange
        double speed = 5.0;
        bool hasDual = true;
        bool hasImu = true;

        // Act
        var source = _service.DetermineOptimalSource(speed, hasDual, hasImu);

        // Assert - dual antenna is most accurate
        Assert.That(source, Is.EqualTo(HeadingSource.DualAntenna));
    }

    [Test]
    public void DetermineOptimalSource_ImuAvailable_ReturnsFused()
    {
        // Arrange
        double speed = 5.0;
        bool hasDual = false;
        bool hasImu = true;

        // Act
        var source = _service.DetermineOptimalSource(speed, hasDual, hasImu);

        // Assert - IMU fusion is second best
        Assert.That(source, Is.EqualTo(HeadingSource.Fused));
    }

    [Test]
    public void DetermineOptimalSource_OnlyGps_ReturnsFixToFix()
    {
        // Arrange
        double speed = 5.0;
        bool hasDual = false;
        bool hasImu = false;

        // Act
        var source = _service.DetermineOptimalSource(speed, hasDual, hasImu);

        // Assert - fall back to fix-to-fix
        Assert.That(source, Is.EqualTo(HeadingSource.FixToFix));
    }

    [Test]
    public void DetermineOptimalSource_LowSpeed_ReturnsFixToFix()
    {
        // Arrange - very slow speed
        double speed = 0.5;
        bool hasDual = false;
        bool hasImu = false;

        // Act
        var source = _service.DetermineOptimalSource(speed, hasDual, hasImu);

        // Assert
        Assert.That(source, Is.EqualTo(HeadingSource.FixToFix));
    }

    #endregion

    #region Angle Normalization Tests

    [Test]
    public void NormalizeAngle_PositiveAngle_RemainsUnchanged()
    {
        // Arrange
        double angle = Math.PI / 2; // 90°

        // Act
        double normalized = _service.NormalizeAngle(angle);

        // Assert
        Assert.That(normalized, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void NormalizeAngle_NegativeAngle_WrapsTo2Pi()
    {
        // Arrange
        double angle = -Math.PI / 2; // -90°

        // Act
        double normalized = _service.NormalizeAngle(angle);

        // Assert - should wrap to 3π/2 (270°)
        Assert.That(normalized, Is.EqualTo(3 * Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void NormalizeAngle_GreaterThan2Pi_WrapsTo0_2Pi()
    {
        // Arrange
        double angle = 3 * Math.PI; // 540° (1.5 rotations)

        // Act
        double normalized = _service.NormalizeAngle(angle);

        // Assert - should wrap to π (180°)
        Assert.That(normalized, Is.EqualTo(Math.PI).Within(Tolerance));
    }

    [Test]
    public void NormalizeAngle_Zero_RemainsZero()
    {
        // Arrange
        double angle = 0;

        // Act
        double normalized = _service.NormalizeAngle(angle);

        // Assert
        Assert.That(normalized, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void NormalizeAngle_TwoPi_WrapsToZero()
    {
        // Arrange
        double angle = 2 * Math.PI;

        // Act
        double normalized = _service.NormalizeAngle(angle);

        // Assert
        Assert.That(normalized, Is.EqualTo(0).Within(Tolerance));
    }

    #endregion

    #region Angular Delta Tests

    [Test]
    public void CalculateAngularDelta_SameAngles_ReturnsZero()
    {
        // Arrange
        double angle1 = Math.PI / 2;
        double angle2 = Math.PI / 2;

        // Act
        double delta = _service.CalculateAngularDelta(angle1, angle2);

        // Assert
        Assert.That(delta, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void CalculateAngularDelta_90DegreesDifference_ReturnsPiOver2()
    {
        // Arrange
        double angle1 = 0; // North
        double angle2 = Math.PI / 2; // East

        // Act
        double delta = _service.CalculateAngularDelta(angle1, angle2);

        // Assert
        Assert.That(delta, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void CalculateAngularDelta_WrapAroundCase_ReturnsShortestPath()
    {
        // Arrange - from 350° to 10° (should be +20°, not -340°)
        double angle1 = 350 * Math.PI / 180; // Near 0
        double angle2 = 10 * Math.PI / 180; // Just past 0

        // Act
        double delta = _service.CalculateAngularDelta(angle1, angle2);

        // Assert - should be small positive angle (~20°)
        Assert.That(delta, Is.GreaterThan(0));
        Assert.That(delta, Is.LessThan(Math.PI / 4)); // Less than 45°
    }

    [Test]
    public void CalculateAngularDelta_OppositeDirections_ReturnsNearPi()
    {
        // Arrange
        double angle1 = 0; // North
        double angle2 = Math.PI; // South

        // Act
        double delta = _service.CalculateAngularDelta(angle1, angle2);

        // Assert - could be +π or -π (both shortest paths)
        Assert.That(Math.Abs(delta), Is.EqualTo(Math.PI).Within(Tolerance));
    }

    [Test]
    public void CalculateAngularDelta_NegativeAngles_NormalizesCorrectly()
    {
        // Arrange
        double angle1 = -Math.PI / 4; // Should normalize to 7π/4
        double angle2 = Math.PI / 4;

        // Act
        double delta = _service.CalculateAngularDelta(angle1, angle2);

        // Assert - should find shortest path (~π/2)
        Assert.That(Math.Abs(delta), Is.LessThan(Math.PI)); // Shortest path is less than 180°
    }

    #endregion

    #region Steer Angle Compensation Tests (Section 6E)

    [Test]
    public void ApplySteerAngleCompensation_LowSpeedWithSteerAngle_AppliesCompensation()
    {
        // Arrange - low speed (0.5 m/s), significant steer angle (10°), forward motion
        double heading = Math.PI / 2; // East (90°)
        double steerAngleDegrees = 10.0;
        double speed = 0.5; // Low speed
        bool isReversing = false;
        double antennaPivotDistance = 2.0; // 2 meters
        double forwardCompensation = 1.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing,
            antennaPivotDistance, forwardCompensation);

        // Assert - compensation should be applied (heading should change)
        Assert.That(compensated, Is.Not.EqualTo(heading));

        // Compensation formula: heading -= toRadians(antennaPivot * steerAngle * forwardComp)
        // Expected: 2.0 * 10.0 * 1.0 = 20 degrees = ~0.349 radians
        double expectedCompensation = 20.0 * Math.PI / 180.0;
        double expectedHeading = heading - expectedCompensation;
        if (expectedHeading < 0) expectedHeading += 2 * Math.PI;

        Assert.That(compensated, Is.EqualTo(expectedHeading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_HighSpeed_NoCompensation()
    {
        // Arrange - high speed (2.0 m/s), compensation should NOT be applied
        double heading = Math.PI / 2; // East
        double steerAngleDegrees = 10.0;
        double speed = 2.0; // High speed (>= 1.0 m/s)
        bool isReversing = false;
        double antennaPivotDistance = 2.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - heading should remain unchanged
        Assert.That(compensated, Is.EqualTo(heading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_SmallSteerAngle_NoCompensation()
    {
        // Arrange - low speed but steer angle below threshold (0.1°)
        double heading = Math.PI / 2;
        double steerAngleDegrees = 0.05; // Below 0.1° threshold
        double speed = 0.5;
        bool isReversing = false;
        double antennaPivotDistance = 2.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - heading should remain unchanged
        Assert.That(compensated, Is.EqualTo(heading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_ZeroAntennaPivot_NoCompensation()
    {
        // Arrange - antenna pivot distance is zero (no physical swing)
        double heading = Math.PI / 2;
        double steerAngleDegrees = 10.0;
        double speed = 0.5;
        bool isReversing = false;
        double antennaPivotDistance = 0.0; // No pivot distance

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - heading should remain unchanged
        Assert.That(compensated, Is.EqualTo(heading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_ReverseMotion_UsesReverseCompensation()
    {
        // Arrange - reversing with different compensation factor
        double heading = Math.PI; // South
        double steerAngleDegrees = 10.0;
        double speed = 0.5;
        bool isReversing = true;
        double antennaPivotDistance = 2.0;
        double forwardCompensation = 1.0;
        double reverseCompensation = 1.5; // Higher compensation in reverse

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing,
            antennaPivotDistance, forwardCompensation, reverseCompensation);

        // Assert - should use reverse compensation factor
        // Expected: 2.0 * 10.0 * 1.5 = 30 degrees = ~0.524 radians
        double expectedCompensation = 30.0 * Math.PI / 180.0;
        double expectedHeading = heading - expectedCompensation;
        if (expectedHeading < 0) expectedHeading += 2 * Math.PI;

        Assert.That(compensated, Is.EqualTo(expectedHeading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_NegativeSteerAngle_AppliesCorrectly()
    {
        // Arrange - negative steer angle (left turn)
        double heading = 0; // North
        double steerAngleDegrees = -15.0; // Left turn
        double speed = 0.5;
        bool isReversing = false;
        double antennaPivotDistance = 1.5;
        double forwardCompensation = 1.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing,
            antennaPivotDistance, forwardCompensation);

        // Assert - compensation should be applied with negative value
        // Expected: 1.5 * -15.0 * 1.0 = -22.5 degrees = ~-0.393 radians
        double expectedCompensation = -22.5 * Math.PI / 180.0;
        double expectedHeading = heading - expectedCompensation;
        while (expectedHeading < 0) expectedHeading += 2 * Math.PI;
        while (expectedHeading >= 2 * Math.PI) expectedHeading -= 2 * Math.PI;

        Assert.That(compensated, Is.EqualTo(expectedHeading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_WraparoundCase_NormalizesCorrectly()
    {
        // Arrange - heading near 0, large positive compensation wraps around
        double heading = 0.1; // Just past north
        double steerAngleDegrees = 30.0; // Large steer angle
        double speed = 0.8;
        bool isReversing = false;
        double antennaPivotDistance = 1.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - result should be normalized to 0-2π range
        Assert.That(compensated, Is.GreaterThanOrEqualTo(0));
        Assert.That(compensated, Is.LessThan(2 * Math.PI));
    }

    [Test]
    public void ApplySteerAngleCompensation_SpeedAtThreshold_NoCompensation()
    {
        // Arrange - speed exactly at 1.0 m/s (threshold boundary)
        double heading = Math.PI / 4;
        double steerAngleDegrees = 10.0;
        double speed = 1.0; // Exactly at threshold
        bool isReversing = false;
        double antennaPivotDistance = 2.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - no compensation at threshold (speed >= 1.0)
        Assert.That(compensated, Is.EqualTo(heading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_SpeedJustBelowThreshold_AppliesCompensation()
    {
        // Arrange - speed just below 1.0 m/s threshold
        double heading = Math.PI / 4;
        double steerAngleDegrees = 10.0;
        double speed = 0.99; // Just below threshold
        bool isReversing = false;
        double antennaPivotDistance = 2.0;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - compensation should be applied
        Assert.That(compensated, Is.Not.EqualTo(heading));
    }

    [Test]
    public void ApplySteerAngleCompensation_LargeSteerAngle_AppliesLargeCompensation()
    {
        // Arrange - very large steer angle (near max for agricultural equipment)
        double heading = Math.PI / 2; // East
        double steerAngleDegrees = 35.0; // Large angle
        double speed = 0.3; // Very slow
        bool isReversing = false;
        double antennaPivotDistance = 2.5;
        double forwardCompensation = 1.2;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing,
            antennaPivotDistance, forwardCompensation);

        // Assert - large compensation should be applied
        // Expected: 2.5 * 35.0 * 1.2 = 105 degrees = ~1.833 radians
        double expectedCompensation = 105.0 * Math.PI / 180.0;
        double expectedHeading = heading - expectedCompensation;
        while (expectedHeading < 0) expectedHeading += 2 * Math.PI;

        Assert.That(compensated, Is.EqualTo(expectedHeading).Within(Tolerance));
    }

    [Test]
    public void ApplySteerAngleCompensation_VeryLowSpeed_AppliesCompensation()
    {
        // Arrange - very slow speed (creeping forward)
        double heading = 3 * Math.PI / 2; // West
        double steerAngleDegrees = 5.0;
        double speed = 0.1; // Very slow
        bool isReversing = false;
        double antennaPivotDistance = 1.8;

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing, antennaPivotDistance);

        // Assert - compensation should still apply at very low speeds
        Assert.That(compensated, Is.Not.EqualTo(heading));
    }

    [Test]
    public void ApplySteerAngleCompensation_CompensationFactorZero_NoCompensation()
    {
        // Arrange - compensation factor set to 0 (disabled)
        double heading = Math.PI;
        double steerAngleDegrees = 10.0;
        double speed = 0.5;
        bool isReversing = false;
        double antennaPivotDistance = 2.0;
        double forwardCompensation = 0.0; // Compensation disabled

        // Act
        double compensated = _service.ApplySteerAngleCompensation(
            heading, steerAngleDegrees, speed, isReversing,
            antennaPivotDistance, forwardCompensation);

        // Assert - heading should remain unchanged when factor is 0
        Assert.That(compensated, Is.EqualTo(heading).Within(Tolerance));
    }

    #endregion

    #region State Tracking Tests

    [Test]
    public void CurrentHeading_AfterCalculation_UpdatesCorrectly()
    {
        // Arrange
        var data = new FixToFixHeadingData
        {
            CurrentEasting = 500010,
            CurrentNorthing = 4500000,
            PreviousEasting = 500000,
            PreviousNorthing = 4500000,
            MinimumDistance = 1.0
        };

        // Act
        _service.CalculateFixToFixHeading(data);

        // Assert
        Assert.That(_service.CurrentHeading, Is.EqualTo(Math.PI / 2).Within(Tolerance));
    }

    [Test]
    public void ImuGpsOffset_InitialValue_IsZero()
    {
        // Assert
        Assert.That(_service.ImuGpsOffset, Is.EqualTo(0).Within(Tolerance));
    }

    [Test]
    public void ImuGpsOffset_AfterFusion_UpdatesCorrectly()
    {
        // Arrange & Act
        _service.FuseImuHeading(0, 90, 0.5);

        // Assert - offset should have changed
        Assert.That(_service.ImuGpsOffset, Is.Not.EqualTo(0));
    }

    #endregion
}
