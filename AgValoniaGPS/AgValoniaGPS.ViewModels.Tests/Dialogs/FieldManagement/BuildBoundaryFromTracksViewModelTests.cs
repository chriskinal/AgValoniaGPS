using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class BuildBoundaryFromTracksViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new BuildBoundaryFromTracksViewModel();
        Assert.NotNull(vm.Tracks);
        Assert.Equal(5.0, vm.BufferDistance);
        Assert.Equal(1.0, vm.SimplifyTolerance);
    }
}
