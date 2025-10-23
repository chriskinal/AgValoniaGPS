using AgValoniaGPS.Models;
using AgValoniaGPS.Services.UI;
using FluentAssertions;
using Xunit;

namespace AgValoniaGPS.Services.Tests.UI;

/// <summary>
/// Unit tests for DialogService class.
/// Note: These tests are limited because DialogService requires an Avalonia application context.
/// Full integration tests should be performed manually or in a UI test harness.
/// </summary>
public class DialogServiceTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new DialogService();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IDialogService>();
    }

    [Fact]
    public void ShowDialogAsync_WithNullViewModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = new DialogService();

        // Act
        Func<Task> act = async () => await service.ShowDialogAsync<object, bool>(null!);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void ShowFilePickerAsync_WithNullTitle_ShouldHandleGracefully()
    {
        // Arrange
        var service = new DialogService();

        // Act
        Func<Task> act = async () => await service.ShowFilePickerAsync(null!);

        // Assert - Should not throw, but will return null due to no application context
        act.Should().NotThrowAsync();
    }

    [Fact]
    public void ShowFolderPickerAsync_WithNullTitle_ShouldHandleGracefully()
    {
        // Arrange
        var service = new DialogService();

        // Act
        Func<Task> act = async () => await service.ShowFolderPickerAsync(null!);

        // Assert - Should not throw, but will return null due to no application context
        act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShowMessageAsync_WithValidParameters_ShouldComplete()
    {
        // Arrange
        var service = new DialogService();

        // Act
        Func<Task> act = async () => await service.ShowMessageAsync("Test message", "Test title", MessageType.Information);

        // Assert - Should not throw, will return immediately due to no application context
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithValidParameters_ShouldComplete()
    {
        // Arrange
        var service = new DialogService();

        // Act
        var result = await service.ShowConfirmationAsync("Test message", "Test title");

        // Assert - Returns false when no application context available
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(MessageType.Information)]
    [InlineData(MessageType.Warning)]
    [InlineData(MessageType.Error)]
    [InlineData(MessageType.Success)]
    public async Task ShowMessageAsync_WithDifferentMessageTypes_ShouldComplete(MessageType messageType)
    {
        // Arrange
        var service = new DialogService();

        // Act
        Func<Task> act = async () => await service.ShowMessageAsync("Test", "Title", messageType);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShowFilePickerAsync_WithFileTypes_ShouldComplete()
    {
        // Arrange
        var service = new DialogService();
        var fileTypes = new[] { "txt", "json", "xml" };

        // Act
        var result = await service.ShowFilePickerAsync("Select File", "/default/path", fileTypes);

        // Assert - Returns null when no application context available
        result.Should().BeNull();
    }

    [Fact]
    public async Task ShowFolderPickerAsync_WithDefaultPath_ShouldComplete()
    {
        // Arrange
        var service = new DialogService();

        // Act
        var result = await service.ShowFolderPickerAsync("Select Folder", "/default/path");

        // Assert - Returns null when no application context available
        result.Should().BeNull();
    }

    // Note: Full UI integration tests should test:
    // - Dialog window creation and display
    // - ViewModel to View mapping
    // - User interaction and result handling
    // - File/folder picker platform integration
    // - Multiple concurrent dialogs
    // These require an Avalonia application context and are best done as manual or automated UI tests.
}
