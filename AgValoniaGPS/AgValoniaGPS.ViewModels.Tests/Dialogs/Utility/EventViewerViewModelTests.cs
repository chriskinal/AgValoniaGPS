using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class EventViewerViewModelTests
{
    [Fact]
    public void Constructor_LoadsEvents()
    {
        var vm = new EventViewerViewModel();
        Assert.NotEmpty(vm.Events);
        Assert.NotEmpty(vm.FilteredEvents);
    }

    [Fact]
    public void FilterLevel_FiltersEvents()
    {
        var vm = new EventViewerViewModel();
        var allCount = vm.FilteredEvents.Count;
        
        vm.FilterLevel = EventLevel.Error;
        
        Assert.True(vm.FilteredEvents.Count <= allCount);
    }

    [Fact]
    public void ClearCommand_RemovesAllEvents()
    {
        var vm = new EventViewerViewModel();
        Assert.True(vm.Events.Count > 0);
        
        vm.ClearCommand.Execute().Subscribe();
        
        Assert.Empty(vm.Events);
        Assert.Empty(vm.FilteredEvents);
    }

    [Fact]
    public void AddEvent_AddsToCollection()
    {
        var vm = new EventViewerViewModel();
        var initialCount = vm.Events.Count;
        
        vm.AddEvent(EventLevel.Info, "Test", "Test message");
        
        Assert.Equal(initialCount + 1, vm.Events.Count);
    }
}
