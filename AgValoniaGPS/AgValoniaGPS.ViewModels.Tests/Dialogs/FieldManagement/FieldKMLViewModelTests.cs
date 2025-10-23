using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class FieldKMLViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        var vm = new FieldKMLViewModel();
        Assert.NotNull(vm.Features);
    }
}
