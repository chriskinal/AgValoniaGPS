using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class BoundaryPlayerViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new BoundaryPlayerViewModel();
        Assert.NotNull(vm.RecordedPoints);
        Assert.False(vm.IsPlaying);
        Assert.Equal(1.0, vm.PlaybackSpeed);
    }
}
