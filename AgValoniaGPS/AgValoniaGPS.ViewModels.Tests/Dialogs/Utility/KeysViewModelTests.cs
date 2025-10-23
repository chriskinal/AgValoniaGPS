using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;
using System.Linq;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class KeysViewModelTests
{
    [Fact]
    public void Constructor_LoadsShortcuts()
    {
        var vm = new KeysViewModel();
        Assert.NotEmpty(vm.Categories);
        Assert.NotEmpty(vm.FilteredShortcuts);
    }

    [Fact]
    public void Categories_ContainsExpectedGroups()
    {
        var vm = new KeysViewModel();
        var categoryNames = vm.Categories.Select(c => c.Name).ToList();
        
        Assert.Contains("File", categoryNames);
        Assert.Contains("Edit", categoryNames);
        Assert.Contains("View", categoryNames);
        Assert.Contains("Guidance", categoryNames);
    }

    [Fact]
    public void SearchText_FiltersShortcuts()
    {
        var vm = new KeysViewModel();
        var initialCount = vm.FilteredShortcuts.Count;
        
        vm.SearchText = "save";
        
        Assert.True(vm.FilteredShortcuts.Count < initialCount);
        Assert.All(vm.FilteredShortcuts, s => 
            Assert.True(s.Key.ToLower().Contains("save") || s.Description.ToLower().Contains("save")));
    }
}
