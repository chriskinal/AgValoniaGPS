using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class BoundaryViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        var vm = new BoundaryViewModel();
        Assert.NotNull(vm.BoundaryPoints);
        Assert.Equal(0, vm.PointCount);
    }
}
