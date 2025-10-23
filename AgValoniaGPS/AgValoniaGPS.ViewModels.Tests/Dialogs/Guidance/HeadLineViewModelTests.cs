using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class HeadLineViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var vm = new HeadLineViewModel();
        Assert.Equal(10.0, vm.DistanceFromBoundary);
        Assert.Equal(1, vm.NumberOfPasses);
        Assert.False(vm.HasHeadland);
    }

    [Fact]
    public void LoadBoundary_UpdatesHasBoundary()
    {
        var vm = new HeadLineViewModel();
        var boundary = new List<Position>
        {
            new Position { Latitude = 42.0, Longitude = -93.6 },
            new Position { Latitude = 42.001, Longitude = -93.6 },
            new Position { Latitude = 42.001, Longitude = -93.601 }
        };
        vm.LoadBoundary(boundary);
        Assert.True(vm.HasBoundary);
    }

    [Fact]
    public void NumberOfPasses_ClampsToValidRange()
    {
        var vm = new HeadLineViewModel();
        vm.NumberOfPasses = 0;
        Assert.Equal(1, vm.NumberOfPasses);
        vm.NumberOfPasses = 15;
        Assert.Equal(10, vm.NumberOfPasses);
    }
}
