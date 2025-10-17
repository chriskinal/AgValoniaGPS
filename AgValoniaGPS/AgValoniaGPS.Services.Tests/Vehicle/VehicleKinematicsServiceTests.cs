using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Vehicle;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Vehicle;

[TestFixture]
public class VehicleKinematicsServiceTests
{
    private VehicleKinematicsService _service;
    private const double Tolerance = 0.001; // 1mm tolerance for position calculations

    [SetUp]
    public void SetUp()
    {
        _service = new VehicleKinematicsService();
    }

    #region Pivot Position Tests

    [Test]
    public void CalculatePivotPosition_NorthHeading_CorrectlyOffsetsAntenna()
    {
        // Arrange
        var gpsPosition = new Position2D(easting: 500000, northing: 4500000);
        double heading = 0; // North
        double antennaPivot = 2.5; // Antenna 2.5m ahead of pivot

        // Act
        var pivotPosition = _service.CalculatePivotPosition(gpsPosition, heading, antennaPivot);

        // Assert - pivot should be 2.5m south of GPS (northing reduced)
        Assert.That(pivotPosition.Easting, Is.EqualTo(500000).Within(Tolerance));
        Assert.That(pivotPosition.Northing, Is.EqualTo(4499997.5).Within(Tolerance));
        Assert.That(pivotPosition.Heading, Is.EqualTo(0));
    }

    [Test]
    public void CalculatePivotPosition_EastHeading_CorrectlyOffsetsAntenna()
    {
        // Arrange
        var gpsPosition = new Position2D(easting: 500000, northing: 4500000);
        double heading = Math.PI / 2; // East
        double antennaPivot = 2.5;

        // Act
        var pivotPosition = _service.CalculatePivotPosition(gpsPosition, heading, antennaPivot);

        // Assert - pivot should be 2.5m west of GPS (easting reduced)
        Assert.That(pivotPosition.Easting, Is.EqualTo(499997.5).Within(Tolerance));
        Assert.That(pivotPosition.Northing, Is.EqualTo(4500000).Within(Tolerance));
        Assert.That(pivotPosition.Heading, Is.EqualTo(Math.PI / 2));
    }

    [Test]
    public void CalculatePivotPosition_NegativeOffset_AntennaBehindPivot()
    {
        // Arrange - negative offset means antenna is behind pivot
        var gpsPosition = new Position2D(easting: 500000, northing: 4500000);
        double heading = 0; // North
        double antennaPivot = -1.0; // Antenna 1m behind pivot

        // Act
        var pivotPosition = _service.CalculatePivotPosition(gpsPosition, heading, antennaPivot);

        // Assert - pivot should be 1m north of GPS (northing increased)
        Assert.That(pivotPosition.Northing, Is.EqualTo(4500001.0).Within(Tolerance));
    }

    #endregion

    #region Steer Axle Tests

    [Test]
    public void CalculateSteerAxlePosition_NorthHeading_CorrectlyCalculates()
    {
        // Arrange
        var pivotPosition = new Position3D(easting: 500000, northing: 4500000, heading: 0);
        double wheelbase = 3.0; // 3m wheelbase

        // Act
        var steerPosition = _service.CalculateSteerAxlePosition(pivotPosition, pivotPosition.Heading, wheelbase);

        // Assert - steer axle should be 3m north of pivot
        Assert.That(steerPosition.Easting, Is.EqualTo(500000).Within(Tolerance));
        Assert.That(steerPosition.Northing, Is.EqualTo(4500003.0).Within(Tolerance));
        Assert.That(steerPosition.Heading, Is.EqualTo(0));
    }

    [Test]
    public void CalculateSteerAxlePosition_SouthWestHeading_CorrectlyCalculates()
    {
        // Arrange
        var pivotPosition = new Position3D(easting: 500000, northing: 4500000, heading: Math.PI * 1.25); // Southwest
        double wheelbase = 2.5;

        // Act
        var steerPosition = _service.CalculateSteerAxlePosition(pivotPosition, pivotPosition.Heading, wheelbase);

        // Assert - steer should be southwest of pivot
        Assert.That(steerPosition.Easting, Is.LessThan(500000)); // West
        Assert.That(steerPosition.Northing, Is.LessThan(4500000)); // South
    }

    #endregion

    #region Hitch Position Tests

    [Test]
    public void CalculateHitchPosition_StandardTractor_CorrectlyCalculates()
    {
        // Arrange
        var gpsPosition = new Position2D(easting: 500000, northing: 4500000);
        double heading = 0; // North
        double hitchLength = -1.5; // Hitch 1.5m behind pivot
        double antennaPivot = 2.0; // Antenna 2m ahead of pivot

        // Act
        var hitchPosition = _service.CalculateHitchPosition(gpsPosition, heading, hitchLength, antennaPivot);

        // Assert - hitch should be 3.5m south of GPS (-1.5 - 2.0)
        Assert.That(hitchPosition.Northing, Is.EqualTo(4499996.5).Within(Tolerance));
    }

    #endregion

    #region Rigid Tool Tests

    [Test]
    public void CalculateRigidToolPosition_CopiesHitchPositionAndHeading()
    {
        // Arrange
        var hitchPosition = new Position2D(easting: 500000, northing: 4500000);
        double heading = Math.PI / 4; // Northeast

        // Act
        var toolPosition = _service.CalculateRigidToolPosition(hitchPosition, heading);

        // Assert
        Assert.That(toolPosition.Easting, Is.EqualTo(hitchPosition.Easting));
        Assert.That(toolPosition.Northing, Is.EqualTo(hitchPosition.Northing));
        Assert.That(toolPosition.Heading, Is.EqualTo(heading));
    }

    #endregion

    #region Trailing Tool Tests

    [Test]
    public void CalculateTrailingToolPosition_NoMovement_ReturnsPreviousPosition()
    {
        // Arrange
        var hitchPosition = new Position2D(easting: 500000, northing: 4500000);
        var previousToolPosition = new Position3D(easting: 499998, northing: 4500000, heading: 0);
        double trailingHitchLength = 3.0;
        double distanceMoved = 0; // No movement
        double vehicleHeading = 0;

        // Act
        var toolPosition = _service.CalculateTrailingToolPosition(
            hitchPosition, previousToolPosition, trailingHitchLength, distanceMoved, vehicleHeading, null);

        // Assert - should return unchanged position
        Assert.That(toolPosition.Easting, Is.EqualTo(previousToolPosition.Easting));
        Assert.That(toolPosition.Northing, Is.EqualTo(previousToolPosition.Northing));
        Assert.That(toolPosition.Heading, Is.EqualTo(previousToolPosition.Heading));
    }

    [Test]
    public void CalculateTrailingToolPosition_NormalMovement_UpdatesHeadingAndPosition()
    {
        // Arrange - vehicle moving north, tool trailing behind
        var hitchPosition = new Position2D(easting: 500000, northing: 4500003);
        var previousToolPosition = new Position3D(easting: 500000, northing: 4500000, heading: 0);
        double trailingHitchLength = 3.0;
        double distanceMoved = 1.0; // Moved 1m
        double vehicleHeading = 0; // North

        // Act
        var toolPosition = _service.CalculateTrailingToolPosition(
            hitchPosition, previousToolPosition, trailingHitchLength, distanceMoved, vehicleHeading, null);

        // Assert - tool should update position behind hitch
        Assert.That(toolPosition.Easting, Is.EqualTo(500000).Within(Tolerance));
        Assert.That(toolPosition.Northing, Is.LessThan(hitchPosition.Northing)); // Behind hitch
    }

    [Test]
    public void CalculateTrailingToolPosition_Jackknifed_ForcesAlignment()
    {
        // Arrange - tool at extreme angle (jackknifed)
        var hitchPosition = new Position2D(easting: 500000, northing: 4500000);
        var previousToolPosition = new Position3D(easting: 500002.5, northing: 4499999, heading: Math.PI / 2); // Perpendicular
        double trailingHitchLength = 3.0;
        double distanceMoved = 1.0;
        double vehicleHeading = 0; // North

        // Act
        var toolPosition = _service.CalculateTrailingToolPosition(
            hitchPosition, previousToolPosition, trailingHitchLength, distanceMoved, vehicleHeading, null);

        // Assert - tool should be forced to align with vehicle heading
        Assert.That(toolPosition.Heading, Is.EqualTo(vehicleHeading).Within(0.1));
    }

    #endregion

    #region Tank Position Tests (TBT)

    [Test]
    public void CalculateTankPosition_NormalMovement_UpdatesCorrectly()
    {
        // Arrange
        var hitchPosition = new Position2D(easting: 500000, northing: 4500002);
        var previousTankPosition = new Position3D(easting: 500000, northing: 4500000, heading: 0);
        double tankHitchLength = 2.0;
        double distanceMoved = 1.0;
        double vehicleHeading = 0;

        // Act
        var tankPosition = _service.CalculateTankPosition(
            hitchPosition, previousTankPosition, tankHitchLength, distanceMoved, vehicleHeading);

        // Assert
        Assert.That(tankPosition.Northing, Is.LessThan(hitchPosition.Northing)); // Tank behind hitch
    }

    [Test]
    public void CalculateTankPosition_Jackknifed_UsesHigherThreshold()
    {
        // Arrange - Tank jackknife threshold is 2.0 radians (higher than tool's 1.9)
        var hitchPosition = new Position2D(easting: 500000, northing: 4500000);
        var previousTankPosition = new Position3D(easting: 500001.8, northing: 4500000, heading: Math.PI / 2);
        double tankHitchLength = 2.0;
        double distanceMoved = 0.5;
        double vehicleHeading = 0;

        // Act
        var tankPosition = _service.CalculateTankPosition(
            hitchPosition, previousTankPosition, tankHitchLength, distanceMoved, vehicleHeading);

        // Assert - should align due to jackknife
        Assert.That(tankPosition.Heading, Is.EqualTo(vehicleHeading).Within(0.1));
    }

    #endregion

    #region Jackknife Detection Tests

    [Test]
    public void IsJackknifed_SmallAngle_ReturnsFalse()
    {
        // Arrange
        double implementHeading = 0.1; // ~5.7 degrees
        double vehicleHeading = 0;
        double threshold = 1.9;

        // Act
        bool isJackknifed = _service.IsJackknifed(implementHeading, vehicleHeading, threshold);

        // Assert
        Assert.That(isJackknifed, Is.False);
    }

    [Test]
    public void IsJackknifed_AngleAtThreshold_ReturnsFalse()
    {
        // Arrange
        double implementHeading = 1.89; // Just under threshold
        double vehicleHeading = 0;
        double threshold = 1.9;

        // Act
        bool isJackknifed = _service.IsJackknifed(implementHeading, vehicleHeading, threshold);

        // Assert
        Assert.That(isJackknifed, Is.False);
    }

    [Test]
    public void IsJackknifed_AngleBeyondThreshold_ReturnsTrue()
    {
        // Arrange
        double implementHeading = 2.0; // Beyond threshold
        double vehicleHeading = 0;
        double threshold = 1.9;

        // Act
        bool isJackknifed = _service.IsJackknifed(implementHeading, vehicleHeading, threshold);

        // Assert
        Assert.That(isJackknifed, Is.True);
    }

    [Test]
    public void IsJackknifed_OppositeDirection_ReturnsTrue()
    {
        // Arrange - implement facing opposite direction
        double implementHeading = Math.PI; // 180 degrees
        double vehicleHeading = 0;
        double threshold = 1.9;

        // Act
        bool isJackknifed = _service.IsJackknifed(implementHeading, vehicleHeading, threshold);

        // Assert
        Assert.That(isJackknifed, Is.True);
    }

    #endregion

    #region Look Ahead Tests

    [Test]
    public void CalculateLookAheadPosition_LowSpeed_UsesToolWidth()
    {
        // Arrange
        var pivotPosition = new Position3D(easting: 500000, northing: 4500000, heading: 0);
        double toolWidth = 10.0; // 10m wide tool
        double speed = 1.0; // 1 m/s (slow)
        double lookAheadTime = 2.0; // 2 seconds

        // Act
        var lookAhead = _service.CalculateLookAheadPosition(pivotPosition, pivotPosition.Heading, toolWidth, speed, lookAheadTime);

        // Assert - should use toolWidth/2 = 5m (greater than speed*time = 2m)
        double distance = pivotPosition.Distance(lookAhead);
        Assert.That(distance, Is.EqualTo(5.0).Within(Tolerance));
    }

    [Test]
    public void CalculateLookAheadPosition_HighSpeed_UsesSpeedBased()
    {
        // Arrange
        var pivotPosition = new Position3D(easting: 500000, northing: 4500000, heading: 0);
        double toolWidth = 6.0; // 6m wide tool
        double speed = 5.0; // 5 m/s (fast - 18 km/h)
        double lookAheadTime = 2.0; // 2 seconds

        // Act
        var lookAhead = _service.CalculateLookAheadPosition(pivotPosition, pivotPosition.Heading, toolWidth, speed, lookAheadTime);

        // Assert - should use speed*time = 10m (greater than toolWidth/2 = 3m)
        double distance = pivotPosition.Distance(lookAhead);
        Assert.That(distance, Is.EqualTo(10.0).Within(Tolerance));
    }

    [Test]
    public void CalculateLookAheadPosition_EastHeading_CorrectDirection()
    {
        // Arrange
        var pivotPosition = new Position3D(easting: 500000, northing: 4500000, heading: Math.PI / 2); // East
        double toolWidth = 6.0;
        double speed = 3.0;
        double lookAheadTime = 1.0;

        // Act
        var lookAhead = _service.CalculateLookAheadPosition(pivotPosition, pivotPosition.Heading, toolWidth, speed, lookAheadTime);

        // Assert - should be east of pivot
        Assert.That(lookAhead.Easting, Is.GreaterThan(pivotPosition.Easting));
        Assert.That(lookAhead.Northing, Is.EqualTo(pivotPosition.Northing).Within(Tolerance));
    }

    #endregion

    #region TBT Tool Position Tests

    [Test]
    public void CalculateTBTToolPosition_ReturnsToolPivotAndWorkingPosition()
    {
        // Arrange
        var tankPosition = new Position3D(easting: 500000, northing: 4500000, heading: 0);
        var previousToolPosition = new Position3D(easting: 499999, northing: 4499997, heading: 0);
        double trailingHitchLength = 4.0;
        double toolToPivotLength = 1.0;
        double distanceMoved = 0.5;

        // Act
        var (toolPivot, toolWorking) = _service.CalculateTBTToolPosition(
            tankPosition, previousToolPosition, trailingHitchLength, toolToPivotLength, distanceMoved, tankPosition.Heading);

        // Assert
        Assert.That(toolPivot.Northing, Is.LessThan(tankPosition.Northing)); // Behind tank
        Assert.That(toolWorking.Northing, Is.GreaterThan(toolPivot.Northing)); // Working position ahead of pivot
        Assert.That(toolWorking.Northing, Is.LessThan(tankPosition.Northing)); // But still behind tank
    }

    #endregion
}
