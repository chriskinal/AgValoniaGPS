using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.Interfaces;
using AgValoniaGPS.Services.GPS;
using Moq;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Section;

[TestFixture]
public class SectionSpeedServiceTests
{
    private SectionSpeedService _service;
    private Mock<ISectionConfigurationService> _mockConfigService;
    private Mock<IVehicleKinematicsService> _mockKinematicsService;
    private Mock<IPositionUpdateService> _mockPositionService;
    private SectionConfiguration _testConfig;

    private const double Tolerance = 0.001;

    [SetUp]
    public void SetUp()
    {
        // Create test configuration with 5 sections, 2.5m each
        _testConfig = new SectionConfiguration(5, new double[] { 2.5, 2.5, 2.5, 2.5, 2.5 });

        _mockConfigService = new Mock<ISectionConfigurationService>();
        _mockConfigService.Setup(x => x.GetConfiguration()).Returns(_testConfig);

        _mockKinematicsService = new Mock<IVehicleKinematicsService>();
        _mockPositionService = new Mock<IPositionUpdateService>();

        _service = new SectionSpeedService(
            _mockConfigService.Object,
            _mockKinematicsService.Object,
            _mockPositionService.Object);
    }

    [Test]
    public void CalculateSectionSpeeds_StraightMovement_AllSectionsEqualVehicleSpeed()
    {
        // Arrange
        double vehicleSpeed = 5.0; // m/s
        double turningRadius = 5000.0; // Very large radius = straight line
        double heading = 0.0;

        // Act
        _service.CalculateSectionSpeeds(vehicleSpeed, turningRadius, heading);

        // Assert - all sections should have same speed as vehicle
        var speeds = _service.GetAllSectionSpeeds();
        Assert.That(speeds.Length, Is.EqualTo(5));
        foreach (var speed in speeds)
        {
            Assert.That(speed, Is.EqualTo(vehicleSpeed).Within(Tolerance));
        }
    }

    [Test]
    public void CalculateSectionSpeeds_RightTurn_LeftSectionsSlowerRightSectionsFaster()
    {
        // Arrange - turning right
        double vehicleSpeed = 5.0; // m/s
        double turningRadius = 10.0; // 10m radius, right turn (positive)
        double heading = 0.0;

        // Act
        _service.CalculateSectionSpeeds(vehicleSpeed, turningRadius, heading);

        // Assert
        var speeds = _service.GetAllSectionSpeeds();

        // Left sections (0, 1) should be slower than vehicle
        Assert.That(speeds[0], Is.LessThan(vehicleSpeed));
        Assert.That(speeds[1], Is.LessThan(vehicleSpeed));

        // Center section (2) should be close to vehicle speed
        Assert.That(speeds[2], Is.EqualTo(vehicleSpeed).Within(0.5));

        // Right sections (3, 4) should be faster than vehicle
        Assert.That(speeds[3], Is.GreaterThan(vehicleSpeed));
        Assert.That(speeds[4], Is.GreaterThan(vehicleSpeed));
    }

    [Test]
    public void CalculateSectionSpeeds_LeftTurn_RightSectionsSlowerLeftSectionsFaster()
    {
        // Arrange - turning left
        double vehicleSpeed = 5.0; // m/s
        double turningRadius = -10.0; // 10m radius, left turn (negative)
        double heading = 0.0;

        // Act
        _service.CalculateSectionSpeeds(vehicleSpeed, turningRadius, heading);

        // Assert
        var speeds = _service.GetAllSectionSpeeds();

        // Right sections (3, 4) should be slower than vehicle
        Assert.That(speeds[3], Is.LessThan(vehicleSpeed));
        Assert.That(speeds[4], Is.LessThan(vehicleSpeed));

        // Center section (2) should be close to vehicle speed
        Assert.That(speeds[2], Is.EqualTo(vehicleSpeed).Within(0.5));

        // Left sections (0, 1) should be faster than vehicle
        Assert.That(speeds[0], Is.GreaterThan(vehicleSpeed));
        Assert.That(speeds[1], Is.GreaterThan(vehicleSpeed));
    }

    [Test]
    public void CalculateSectionSpeeds_SharpTurn_InsideSectionClampsToZero()
    {
        // Arrange - very sharp turn where inside sections would have negative radius
        double vehicleSpeed = 5.0; // m/s
        double turningRadius = 5.0; // Very tight 5m radius right turn
        double heading = 0.0;

        // Act
        _service.CalculateSectionSpeeds(vehicleSpeed, turningRadius, heading);

        // Assert - leftmost section should be clamped to zero (inside of tight turn)
        var speeds = _service.GetAllSectionSpeeds();
        Assert.That(speeds[0], Is.EqualTo(0.0).Within(Tolerance));
    }

    [Test]
    public void GetSectionSpeed_ValidSectionId_ReturnsCorrectSpeed()
    {
        // Arrange
        double vehicleSpeed = 5.0;
        double turningRadius = 5000.0;
        double heading = 0.0;
        _service.CalculateSectionSpeeds(vehicleSpeed, turningRadius, heading);

        // Act
        double speed = _service.GetSectionSpeed(2); // Center section

        // Assert
        Assert.That(speed, Is.EqualTo(vehicleSpeed).Within(Tolerance));
    }

    [Test]
    public void GetSectionSpeed_InvalidSectionId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetSectionSpeed(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetSectionSpeed(10));
    }
}
