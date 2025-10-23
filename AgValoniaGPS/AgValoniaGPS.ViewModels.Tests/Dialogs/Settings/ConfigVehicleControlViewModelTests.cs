using System.Reactive;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.ViewModels.Dialogs.Settings;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Settings;

/// <summary>
/// Tests for ConfigVehicleControlViewModel.
/// </summary>
public class ConfigVehicleControlViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var vm = new ConfigVehicleControlViewModel();

        // Assert
        Assert.NotNull(vm.VehicleName);
        Assert.True(vm.VehicleWheelbase > 0);
        Assert.True(vm.VehicleTrackWidth > 0);
        Assert.True(vm.VehicleWidth > 0);
        Assert.True(vm.MaxSteerAngle > 0);
        Assert.True(vm.ImplementWidth > 0);
        Assert.True(vm.NumberOfSections > 0);
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Arrange & Act
        var vm = new ConfigVehicleControlViewModel();

        // Assert
        Assert.NotNull(vm.SaveConfigCommand);
        Assert.NotNull(vm.ResetCommand);
        Assert.NotNull(vm.LoadPresetCommand);
        Assert.NotNull(vm.OKCommand);
        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void Constructor_InitializesVehicleTypes()
    {
        // Arrange & Act
        var vm = new ConfigVehicleControlViewModel();

        // Assert
        Assert.NotNull(vm.VehicleTypes);
        Assert.NotEmpty(vm.VehicleTypes);
    }

    [Fact]
    public void VehicleName_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testName = "Test Tractor";

        // Act
        vm.VehicleName = testName;

        // Assert
        Assert.Equal(testName, vm.VehicleName);
    }

    [Fact]
    public void VehicleWheelbase_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 200.0;

        // Act
        vm.VehicleWheelbase = testValue;

        // Assert
        Assert.Equal(testValue, vm.VehicleWheelbase);
    }

    [Fact]
    public void VehicleType_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testType = VehicleType.Harvester;

        // Act
        vm.VehicleType = testType;

        // Assert
        Assert.Equal(testType, vm.VehicleType);
    }

    [Fact]
    public void MaxSteerAngle_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 50.0;

        // Act
        vm.MaxSteerAngle = testValue;

        // Assert
        Assert.Equal(testValue, vm.MaxSteerAngle);
    }

    [Fact]
    public void MinSteerAngle_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = -50.0;

        // Act
        vm.MinSteerAngle = testValue;

        // Assert
        Assert.Equal(testValue, vm.MinSteerAngle);
    }

    [Fact]
    public void AckermannPercentage_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 80.0;

        // Act
        vm.AckermannPercentage = testValue;

        // Assert
        Assert.Equal(testValue, vm.AckermannPercentage);
    }

    [Fact]
    public void SteeringDeadband_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 1.0;

        // Act
        vm.SteeringDeadband = testValue;

        // Assert
        Assert.Equal(testValue, vm.SteeringDeadband);
    }

    [Fact]
    public void AntennaHeight_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 60.0;

        // Act
        vm.AntennaHeight = testValue;

        // Assert
        Assert.Equal(testValue, vm.AntennaHeight);
    }

    [Fact]
    public void AntennaOffset_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 10.0;

        // Act
        vm.AntennaOffset = testValue;

        // Assert
        Assert.Equal(testValue, vm.AntennaOffset);
    }

    [Fact]
    public void ImplementWidth_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 400.0;

        // Act
        vm.ImplementWidth = testValue;

        // Assert
        Assert.Equal(testValue, vm.ImplementWidth);
    }

    [Fact]
    public void ImplementOffset_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 15.0;

        // Act
        vm.ImplementOffset = testValue;

        // Assert
        Assert.Equal(testValue, vm.ImplementOffset);
    }

    [Fact]
    public void NumberOfSections_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 8;

        // Act
        vm.NumberOfSections = testValue;

        // Assert
        Assert.Equal(testValue, vm.NumberOfSections);
    }

    [Fact]
    public void IsTrailing_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();

        // Act
        vm.IsTrailing = false;

        // Assert
        Assert.False(vm.IsTrailing);
    }

    [Fact]
    public void MinLookAhead_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 5.0;

        // Act
        vm.MinLookAhead = testValue;

        // Assert
        Assert.Equal(testValue, vm.MinLookAhead);
    }

    [Fact]
    public void MaxLookAhead_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 15.0;

        // Act
        vm.MaxLookAhead = testValue;

        // Assert
        Assert.Equal(testValue, vm.MaxLookAhead);
    }

    [Fact]
    public void LookAheadSpeedGain_CanBeSet()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        var testValue = 2.0;

        // Act
        vm.LookAheadSpeedGain = testValue;

        // Assert
        Assert.Equal(testValue, vm.LookAheadSpeedGain);
    }

    [Fact]
    public void ResetCommand_ResetsToDefaults()
    {
        // Arrange
        var vm = new ConfigVehicleControlViewModel();
        vm.VehicleName = "Modified";
        vm.VehicleWheelbase = 999;

        // Act
        vm.ResetCommand.Execute(Unit.Default);

        // Assert
        Assert.Equal("Default Vehicle", vm.VehicleName);
        Assert.Equal(180.0, vm.VehicleWheelbase);
    }
}
