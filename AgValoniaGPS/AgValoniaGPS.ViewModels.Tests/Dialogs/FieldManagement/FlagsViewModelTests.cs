using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class FlagsViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        // Arrange & Act
        var vm = new FlagsViewModel();

        // Assert
        Assert.NotNull(vm.Flags);
        Assert.NotNull(vm.FilteredFlags);
        Assert.Equal(0, vm.FlagCount);
    }

    [Fact]
    public void AddFlag_AddsToCollection()
    {
        // Arrange
        var vm = new FlagsViewModel();
        var flag = new FieldFlag { Name = "Test Flag" };

        // Act
        vm.AddFlag(flag);

        // Assert
        Assert.Equal(1, vm.FlagCount);
        Assert.Contains(flag, vm.Flags);
    }

    [Fact]
    public void SearchText_FiltersFlags()
    {
        // Arrange
        var vm = new FlagsViewModel();
        vm.AddFlag(new FieldFlag { Name = "Test Flag 1" });
        vm.AddFlag(new FieldFlag { Name = "Test Flag 2" });
        vm.AddFlag(new FieldFlag { Name = "Other Flag" });

        // Act
        vm.SearchText = "Test";

        // Assert
        Assert.Equal(2, vm.FilteredFlags.Count);
    }

    [Fact]
    public void SearchText_Empty_ShowsAllFlags()
    {
        // Arrange
        var vm = new FlagsViewModel();
        vm.AddFlag(new FieldFlag { Name = "Flag 1" });
        vm.AddFlag(new FieldFlag { Name = "Flag 2" });

        // Act
        vm.SearchText = "";

        // Assert
        Assert.Equal(2, vm.FilteredFlags.Count);
    }

    [Fact]
    public void DeleteFlag_RemovesFromCollection()
    {
        // Arrange
        var vm = new FlagsViewModel();
        var flag = new FieldFlag { Name = "Test Flag" };
        vm.AddFlag(flag);
        vm.SelectedFlag = flag;

        // Act
        vm.DeleteFlagCommand.Execute(null);

        // Assert
        Assert.Equal(0, vm.FlagCount);
        Assert.DoesNotContain(flag, vm.Flags);
    }
}
