using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class HelpViewModelTests
{
    [Fact]
    public void Constructor_LoadsHelpTopics()
    {
        var vm = new HelpViewModel();
        Assert.NotEmpty(vm.HelpTopics);
        Assert.True(vm.HelpTopics.Count > 0);
    }

    [Fact]
    public void TopicContent_ShowsDefaultWhenNoneSelected()
    {
        var vm = new HelpViewModel();
        Assert.Contains("Select a topic", vm.TopicContent);
    }

    [Fact]
    public void SelectedTopic_UpdatesTopicContent()
    {
        var vm = new HelpViewModel();
        var firstTopic = vm.HelpTopics[0];
        
        vm.SelectedTopic = firstTopic;
        
        Assert.Equal(firstTopic.Content, vm.TopicContent);
    }
}
