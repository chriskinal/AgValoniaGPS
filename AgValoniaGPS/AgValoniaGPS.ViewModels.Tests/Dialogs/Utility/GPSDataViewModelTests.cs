using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class GPSDataViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithSampleData()
    {
        var vm = new GPSDataViewModel();
        Assert.NotEqual(0, vm.Latitude);
        Assert.NotEqual(0, vm.Longitude);
        Assert.True(vm.IsConnected);
    }

    [Fact]
    public void FormattedProperties_FormatCorrectly()
    {
        var vm = new GPSDataViewModel();
        vm.Latitude = 42.123456789;
        
        Assert.Contains("42.1234568", vm.LatitudeFormatted);
    }

    [Fact]
    public void ConnectionStatus_ReflectsState()
    {
        var vm = new GPSDataViewModel();
        vm.IsConnected = true;
        Assert.Equal("Connected", vm.ConnectionStatus);
        
        vm.IsConnected = false;
        Assert.Equal("Disconnected", vm.ConnectionStatus);
    }
}
