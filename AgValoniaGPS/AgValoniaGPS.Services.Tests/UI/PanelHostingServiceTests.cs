using System.Linq;
using Avalonia.Controls;
using AgValoniaGPS.Desktop.Services;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.UI;

/// <summary>
/// Unit tests for PanelHostingService covering:
/// - Panel registration
/// - Show/hide functionality
/// - Toggle functionality
/// - Visibility state tracking
/// - Panels in location queries
/// - Event raising
/// </summary>
[TestFixture]
public class PanelHostingServiceTests
{
    [Test]
    public void RegisterPanel_ShouldAddPanelToRegistry()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid();
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();

        // Act
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Assert
        Assert.That(service.GetPanelsInLocation(PanelDockLocation.Left).Count(), Is.EqualTo(1));
        Assert.That(service.GetPanelsInLocation(PanelDockLocation.Left).First(), Is.EqualTo("test"));
    }

    [Test]
    public void ShowPanel_ShouldAddControlToGridContainer()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Act
        service.ShowPanel("test");

        // Assert
        Assert.That(panelLeft.Children.Contains(panel), Is.True);
        Assert.That(service.IsPanelVisible("test"), Is.True);
    }

    [Test]
    public void ShowPanel_ShouldAddControlToStackPanelContainer()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid();
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Right, panel);

        // Act
        service.ShowPanel("test");

        // Assert
        Assert.That(panelRight.Children.Contains(panel), Is.True);
        Assert.That(service.IsPanelVisible("test"), Is.True);
    }

    [Test]
    public void HidePanel_ShouldRemoveControlFromContainer()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);
        service.ShowPanel("test");

        // Act
        service.HidePanel("test");

        // Assert
        Assert.That(panelLeft.Children.Contains(panel), Is.False);
        Assert.That(service.IsPanelVisible("test"), Is.False);
    }

    [Test]
    public void TogglePanel_ShouldInvertVisibility()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Act & Assert - First toggle (show)
        service.TogglePanel("test");
        Assert.That(service.IsPanelVisible("test"), Is.True);
        Assert.That(panelLeft.Children.Contains(panel), Is.True);

        // Act & Assert - Second toggle (hide)
        service.TogglePanel("test");
        Assert.That(service.IsPanelVisible("test"), Is.False);
        Assert.That(panelLeft.Children.Contains(panel), Is.False);
    }

    [Test]
    public void IsPanelVisible_ReturnsCorrectState()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Assert - Initially hidden
        Assert.That(service.IsPanelVisible("test"), Is.False);

        // Act - Show panel
        service.ShowPanel("test");

        // Assert - Now visible
        Assert.That(service.IsPanelVisible("test"), Is.True);
    }

    [Test]
    public void GetPanelsInLocation_ReturnsPanelsForLocation()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel1 = new Button();
        var panel2 = new Button();
        var panel3 = new Button();

        // Act
        service.RegisterPanel("left1", PanelDockLocation.Left, panel1);
        service.RegisterPanel("left2", PanelDockLocation.Left, panel2);
        service.RegisterPanel("right1", PanelDockLocation.Right, panel3);

        // Assert
        var leftPanels = service.GetPanelsInLocation(PanelDockLocation.Left).ToList();
        var rightPanels = service.GetPanelsInLocation(PanelDockLocation.Right).ToList();

        Assert.That(leftPanels.Count, Is.EqualTo(2));
        Assert.That(leftPanels, Contains.Item("left1"));
        Assert.That(leftPanels, Contains.Item("left2"));

        Assert.That(rightPanels.Count, Is.EqualTo(1));
        Assert.That(rightPanels, Contains.Item("right1"));
    }

    [Test]
    public void PanelVisibilityChanged_EventFires()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        PanelVisibilityChangedEventArgs? eventArgs = null;
        service.PanelVisibilityChanged += (sender, args) =>
        {
            eventArgs = args;
        };

        // Act
        service.ShowPanel("test");

        // Assert
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.PanelId, Is.EqualTo("test"));
        Assert.That(eventArgs.IsVisible, Is.True);
        Assert.That(eventArgs.Location, Is.EqualTo(PanelDockLocation.Left));
    }

    [Test]
    public void ShowPanel_GridRowAssignment_UsesFirstAvailableRow()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel1 = new Button();
        var panel2 = new Button();
        service.RegisterPanel("test1", PanelDockLocation.Left, panel1);
        service.RegisterPanel("test2", PanelDockLocation.Left, panel2);

        // Act
        service.ShowPanel("test1");
        service.ShowPanel("test2");

        // Assert
        Assert.That(Grid.GetRow(panel1), Is.EqualTo(0)); // First available row
        Assert.That(Grid.GetRow(panel2), Is.EqualTo(1)); // Second available row
    }

    [Test]
    public void ShowPanel_WhenAlreadyVisible_DoesNothing()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);
        service.ShowPanel("test");

        int eventCount = 0;
        service.PanelVisibilityChanged += (sender, args) => eventCount++;

        // Act
        service.ShowPanel("test");

        // Assert
        Assert.That(eventCount, Is.EqualTo(0)); // Event should not fire for already visible panel
        Assert.That(panelLeft.Children.Count, Is.EqualTo(1)); // Panel not added twice
    }

    [Test]
    public void HidePanel_WhenAlreadyHidden_DoesNothing()
    {
        // Arrange
        var service = new PanelHostingService();
        var panelLeft = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*,*") // 8 rows
        };
        var panelRight = new StackPanel();
        var panelBottom = new StackPanel();
        var panelNavigation = new Border();
        service.Initialize(panelLeft, panelRight, panelBottom, panelNavigation);

        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        int eventCount = 0;
        service.PanelVisibilityChanged += (sender, args) => eventCount++;

        // Act
        service.HidePanel("test");

        // Assert
        Assert.That(eventCount, Is.EqualTo(0)); // Event should not fire for already hidden panel
    }
}
