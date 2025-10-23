using System;
using System.IO;
using Xunit;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Pickers;

/// <summary>
/// Unit tests for NewProfileViewModel.
/// </summary>
public class NewProfileViewModelTests
{
    private readonly string _testProfilesDirectory;

    public NewProfileViewModelTests()
    {
        _testProfilesDirectory = Path.Combine(Path.GetTempPath(), "AgValoniaGPS_Test_Profiles");
        Directory.CreateDirectory(_testProfilesDirectory);
    }

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Assert
        Assert.Equal(string.Empty, viewModel.ProfileName);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.Equal(ProfileTypeEnum.Vehicle, viewModel.ProfileType);
        Assert.False(viewModel.IsValid);
    }

    [Fact]
    public void ProfileName_Empty_SetsError()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.ProfileName = "";

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.IsValid);
    }

    [Fact]
    public void ProfileName_Valid_ClearsError()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.ProfileName = "ValidProfile";

        // Assert
        Assert.False(viewModel.HasError);
        Assert.True(viewModel.IsValid);
    }

    [Fact]
    public void ProfileName_InvalidCharacters_SetsError()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.ProfileName = "Invalid/Name";

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("invalid characters", viewModel.ErrorMessage);
        Assert.False(viewModel.IsValid);
    }

    [Fact]
    public void ProfileName_Duplicate_SetsError()
    {
        // Arrange
        var existingFile = Path.Combine(_testProfilesDirectory, "ExistingProfile.json");
        File.WriteAllText(existingFile, "{}");

        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.ProfileName = "ExistingProfile";

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("already exists", viewModel.ErrorMessage);
        Assert.False(viewModel.IsValid);

        // Cleanup
        File.Delete(existingFile);
    }

    [Fact]
    public void IsVehicleProfile_SetTrue_SetsProfileType()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);
        viewModel.ProfileType = ProfileTypeEnum.User;

        // Act
        viewModel.IsVehicleProfile = true;

        // Assert
        Assert.Equal(ProfileTypeEnum.Vehicle, viewModel.ProfileType);
        Assert.True(viewModel.IsVehicleProfile);
        Assert.False(viewModel.IsUserProfile);
    }

    [Fact]
    public void IsUserProfile_SetTrue_SetsProfileType()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.IsUserProfile = true;

        // Assert
        Assert.Equal(ProfileTypeEnum.User, viewModel.ProfileType);
        Assert.True(viewModel.IsUserProfile);
        Assert.False(viewModel.IsVehicleProfile);
    }

    [Fact]
    public void OnOK_InvalidForm_ReturnsFalse()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);
        // Don't set profile name - invalid

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.False(closeRequested);
    }

    [Fact]
    public void OnOK_ValidForm_ReturnsTrue()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);
        viewModel.ProfileName = "NewValidProfile";

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.True(closeRequested);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public void ProfileName_Trimmed_RemovesWhitespace()
    {
        // Arrange
        var viewModel = new NewProfileViewModel(_testProfilesDirectory);

        // Act
        viewModel.ProfileName = "  SpacedName  ";

        // Assert
        Assert.Equal("SpacedName", viewModel.ProfileName);
    }
}
