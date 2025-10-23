using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class FieldISOXMLViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        var vm = new FieldISOXMLViewModel();
        Assert.NotNull(vm.ISOFields);
    }
}
