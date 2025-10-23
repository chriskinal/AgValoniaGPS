using System;
using System.IO;
using Xunit;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Pickers;

/// <summary>
/// Unit tests for LoadProfileViewModel.
/// </summary>
public class LoadProfileViewModelTests
{
    private readonly string _testProfilesDirectory;

    public LoadProfileViewModelTests()
    {
        _testProfilesDirectory = Path.Combine(Path.GetTempPath(), "AgValoniaGPS_Test_Profiles");
        Directory.CreateDirectory(_testProfilesDirectory);
    }

    [Fact]
    public void Constructor_CreatesProfilesDirectory()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid()}");

        // Act
        var viewModel = new LoadProfileViewModel(testDir);

        // Assert
        Assert.True(Directory.Exists(testDir));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public void LoadProfiles_EmptyDirectory_ShowsNoProfiles()
    {
        // Arrange
        var emptyDir = Path.Combine(Path.GetTempPath(), $"Empty_{Guid.NewGuid()}");
        Directory.CreateDirectory(emptyDir);

        // Act
        var viewModel = new LoadProfileViewModel(emptyDir);

        // Assert
        Assert.Empty(viewModel.Profiles);
        Assert.True(viewModel.HasError);

        // Cleanup
        Directory.Delete(emptyDir, true);
    }

    [Fact]
    public void LoadProfiles_WithProfiles_LoadsThem()
    {
        // Arrange
        var testFile = Path.Combine(_testProfilesDirectory, "test_profile.json");
        File.WriteAllText(testFile, "{}");

        // Act
        var viewModel = new LoadProfileViewModel(_testProfilesDirectory);

        // Assert
        Assert.NotEmpty(viewModel.Profiles);

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public void RefreshCommand_ReloadsProfiles()
    {
        // Arrange
        var viewModel = new LoadProfileViewModel(_testProfilesDirectory);
        var initialCount = viewModel.Profiles.Count;

        var testFile = Path.Combine(_testProfilesDirectory, $"new_profile_{Guid.NewGuid()}.json");
        File.WriteAllText(testFile, "{}");

        // Act
        viewModel.RefreshCommand.Execute(null);

        // Assert
        Assert.True(viewModel.Profiles.Count > initialCount);

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public void HasSelection_WhenSelected_ReturnsTrue()
    {
        // Arrange
        var testFile = Path.Combine(_testProfilesDirectory, "test_profile.json");
        File.WriteAllText(testFile, "{}");
        var viewModel = new LoadProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.SelectedProfile = viewModel.Profiles[0];

        // Assert
        Assert.True(viewModel.HasSelection);

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public void OnOK_WithoutSelection_SetsError()
    {
        // Arrange
        var viewModel = new LoadProfileViewModel(_testProfilesDirectory);

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(closeRequested);
    }

    [Fact]
    public void OnOK_WithSelection_Succeeds()
    {
        // Arrange
        var testFile = Path.Combine(_testProfilesDirectory, "test_profile.json");
        File.WriteAllText(testFile, "{}");
        var viewModel = new LoadProfileViewModel(_testProfilesDirectory);
        viewModel.SelectedProfile = viewModel.Profiles[0];

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.True(closeRequested);

        // Cleanup
        File.Delete(testFile);
    }
}
