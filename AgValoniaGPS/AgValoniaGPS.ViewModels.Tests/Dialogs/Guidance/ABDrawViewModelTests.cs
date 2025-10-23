using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class ABDrawViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var vm = new ABDrawViewModel();
        Assert.Null(vm.PointA);
        Assert.Null(vm.PointB);
        Assert.False(vm.IsPointASet);
        Assert.False(vm.IsPointBSet);
        Assert.Equal("Click to set Point A", vm.Instructions);
    }

    [Fact]
    public void OnCanvasClick_SetsPointA_WhenNotSet()
    {
        var vm = new ABDrawViewModel();
        var position = new Position { Latitude = 42.0, Longitude = -93.6 };
        vm.OnCanvasClick(position);
        Assert.NotNull(vm.PointA);
        Assert.True(vm.IsPointASet);
        Assert.Equal("Click to set Point B", vm.Instructions);
    }

    [Fact]
    public void OnCanvasClick_SetsPointB_WhenPointASet()
    {
        var vm = new ABDrawViewModel();
        vm.OnCanvasClick(new Position { Latitude = 42.0, Longitude = -93.6 });
        vm.OnCanvasClick(new Position { Latitude = 42.001, Longitude = -93.601 });
        Assert.NotNull(vm.PointB);
        Assert.True(vm.IsPointBSet);
    }

    [Fact]
    public void ClearCommand_ResetsAllPoints()
    {
        var vm = new ABDrawViewModel();
        vm.OnCanvasClick(new Position { Latitude = 42.0, Longitude = -93.6 });
        vm.OnCanvasClick(new Position { Latitude = 42.001, Longitude = -93.601 });
        vm.ClearCommand.Execute(null);
        Assert.Null(vm.PointA);
        Assert.Null(vm.PointB);
        Assert.False(vm.IsPointASet);
    }

    [Fact]
    public void CreateABLineCommand_CanExecute_WhenBothPointsSet()
    {
        var vm = new ABDrawViewModel();
        vm.OnCanvasClick(new Position { Latitude = 42.0, Longitude = -93.6 });
        vm.OnCanvasClick(new Position { Latitude = 42.01, Longitude = -93.61 }); // Far enough apart
        var canExecute = vm.CreateABLineCommand.CanExecute(null);
        Assert.True(canExecute);
    }

    [Fact]
    public void LineCalculations_UpdateWhenPointsSet()
    {
        var vm = new ABDrawViewModel();
        vm.OnCanvasClick(new Position { Latitude = 42.0, Longitude = -93.6 });
        vm.OnCanvasClick(new Position { Latitude = 42.01, Longitude = -93.6 }); // Same longitude, different latitude
        Assert.True(vm.LineLength > 0);
        Assert.True(vm.LineHeading >= 0 && vm.LineHeading < 360);
    }
}
