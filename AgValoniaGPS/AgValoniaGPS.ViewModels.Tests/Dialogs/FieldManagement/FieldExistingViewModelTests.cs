using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class FieldExistingViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        var vm = new FieldExistingViewModel();
        Assert.NotNull(vm.Fields);
    }
}
