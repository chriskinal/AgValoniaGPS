using AgValoniaGPS.ViewModels.Dialogs.Settings;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Settings;

/// <summary>
/// Tests for ConfigSummaryViewModel.
/// </summary>
public class ConfigSummaryViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var vm = new ConfigSummaryViewModel();

        // Assert
        Assert.NotNull(vm.VehicleName);
        Assert.NotNull(vm.VehicleType);
        Assert.NotNull(vm.UserName);
        Assert.NotNull(vm.Units);
        Assert.True(vm.VehicleWheelbase > 0);
        Assert.True(vm.MaxSteerAngle > 0);
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Arrange & Act
        var vm = new ConfigSummaryViewModel();

        // Assert
        Assert.NotNull(vm.EditVehicleCommand);
        Assert.NotNull(vm.EditUserCommand);
        Assert.NotNull(vm.RefreshCommand);
        Assert.NotNull(vm.OKCommand);
        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void VehicleName_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testName = "Test Vehicle";

        // Act
        vm.VehicleName = testName;

        // Assert
        Assert.Equal(testName, vm.VehicleName);
    }

    [Fact]
    public void VehicleWheelbase_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testValue = 200.0;

        // Act
        vm.VehicleWheelbase = testValue;

        // Assert
        Assert.Equal(testValue, vm.VehicleWheelbase);
    }

    [Fact]
    public void VehicleTrack_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testValue = 150.0;

        // Act
        vm.VehicleTrack = testValue;

        // Assert
        Assert.Equal(testValue, vm.VehicleTrack);
    }

    [Fact]
    public void MaxSteerAngle_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testValue = 50.0;

        // Act
        vm.MaxSteerAngle = testValue;

        // Assert
        Assert.Equal(testValue, vm.MaxSteerAngle);
    }

    [Fact]
    public void AntennaOffset_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testValue = 5.0;

        // Act
        vm.AntennaOffset = testValue;

        // Assert
        Assert.Equal(testValue, vm.AntennaOffset);
    }

    [Fact]
    public void VehicleType_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testType = "Harvester";

        // Act
        vm.VehicleType = testType;

        // Assert
        Assert.Equal(testType, vm.VehicleType);
    }

    [Fact]
    public void UserName_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testName = "John Doe";

        // Act
        vm.UserName = testName;

        // Assert
        Assert.Equal(testName, vm.UserName);
    }

    [Fact]
    public void Units_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testUnits = "Imperial";

        // Act
        vm.Units = testUnits;

        // Assert
        Assert.Equal(testUnits, vm.Units);
    }

    [Fact]
    public void AutoSave_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();

        // Act
        vm.AutoSave = false;

        // Assert
        Assert.False(vm.AutoSave);
    }

    [Fact]
    public void AutoSaveInterval_CanBeSet()
    {
        // Arrange
        var vm = new ConfigSummaryViewModel();
        var testInterval = 600;

        // Act
        vm.AutoSaveInterval = testInterval;

        // Assert
        Assert.Equal(testInterval, vm.AutoSaveInterval);
    }
}
