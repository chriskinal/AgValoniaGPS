using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class EnterFlagViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var vm = new EnterFlagViewModel();

        // Assert
        Assert.Equal(string.Empty, vm.FlagName);
        Assert.Equal(0, vm.Latitude);
        Assert.Equal(0, vm.Longitude);
        Assert.Equal("#FF0000", vm.FlagColorHex);
        Assert.NotNull(vm.PickColorCommand);
        Assert.NotNull(vm.UseCurrentPositionCommand);
    }

    [Fact]
    public void Constructor_WithFlag_LoadsData()
    {
        // Arrange
        var flag = new FieldFlag
        {
            Name = "Test Flag",
            Position = new Position { Latitude = 45.5, Longitude = -122.3 },
            ColorHex = "#00FF00",
            Notes = "Test notes"
        };

        // Act
        var vm = new EnterFlagViewModel(flag);

        // Assert
        Assert.Equal("Test Flag", vm.FlagName);
        Assert.Equal(45.5, vm.Latitude);
        Assert.Equal(-122.3, vm.Longitude);
        Assert.Equal("#00FF00", vm.FlagColorHex);
        Assert.Equal("Test notes", vm.Notes);
    }

    [Fact]
    public void SetFlagName_UpdatesProperty()
    {
        // Arrange
        var vm = new EnterFlagViewModel();

        // Act
        vm.FlagName = "New Flag";

        // Assert
        Assert.Equal("New Flag", vm.FlagName);
    }

    [Fact]
    public void SetLatitude_UpdatesProperty()
    {
        // Arrange
        var vm = new EnterFlagViewModel();

        // Act
        vm.Latitude = 45.123456;

        // Assert
        Assert.Equal(45.123456, vm.Latitude);
    }

    [Fact]
    public void ToFieldFlag_CreatesCorrectFlag()
    {
        // Arrange
        var vm = new EnterFlagViewModel
        {
            FlagName = "Test Flag",
            Latitude = 45.5,
            Longitude = -122.3,
            FlagColorHex = "#0000FF",
            Notes = "Test notes"
        };

        // Act
        var flag = vm.ToFieldFlag();

        // Assert
        Assert.Equal("Test Flag", flag.Name);
        Assert.Equal(45.5, flag.Position.Latitude);
        Assert.Equal(-122.3, flag.Position.Longitude);
        Assert.Equal("#0000FF", flag.ColorHex);
        Assert.Equal("Test notes", flag.Notes);
    }

    [Fact]
    public void UseCurrentPosition_WithValidPosition_UpdatesCoordinates()
    {
        // Arrange
        var vm = new EnterFlagViewModel
        {
            CurrentPosition = new Position { Latitude = 50.0, Longitude = -100.0 }
        };

        // Act
        vm.UseCurrentPositionCommand.Execute(null);

        // Assert
        Assert.Equal(50.0, vm.Latitude);
        Assert.Equal(-100.0, vm.Longitude);
    }

    [Fact]
    public void UseCurrentPosition_WithNullPosition_ShowsError()
    {
        // Arrange
        var vm = new EnterFlagViewModel
        {
            CurrentPosition = null
        };

        // Act
        vm.UseCurrentPositionCommand.Execute(null);

        // Assert
        Assert.True(vm.HasError);
    }
}
