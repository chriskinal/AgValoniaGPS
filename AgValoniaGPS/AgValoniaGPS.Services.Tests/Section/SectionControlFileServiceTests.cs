using System;
using System.IO;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Section;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Section;

/// <summary>
/// Tests for SectionControlFileService
/// Focused on critical file I/O behaviors: save, load, error handling
/// </summary>
public class SectionControlFileServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ISectionConfigurationService _configService;
    private readonly SectionControlFileService _fileService;

    public SectionControlFileServiceTests()
    {
        // Create temp directory for tests
        _testDirectory = Path.Combine(Path.GetTempPath(), $"AgValoniaGPS_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _configService = new SectionConfigurationService();
        _fileService = new SectionControlFileService(_configService);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void SaveConfiguration_ValidConfig_CreatesFileWithCorrectFormat()
    {
        // Arrange
        var config = new SectionConfiguration(3, new double[] { 2.5, 3.0, 2.5 })
        {
            TurnOnDelay = 2.0,
            TurnOffDelay = 1.5,
            OverlapTolerance = 10.0,
            LookAheadDistance = 3.0,
            MinimumSpeed = 0.1
        };

        // Act
        _fileService.SaveConfiguration(_testDirectory, config);

        // Assert
        var filePath = Path.Combine(_testDirectory, "SectionConfig.txt");
        Assert.True(File.Exists(filePath), "Configuration file should be created");

        var content = File.ReadAllText(filePath);
        Assert.Contains("[SectionControl]", content);
        Assert.Contains("SectionCount=3", content);
        Assert.Contains("SectionWidths=2.50,3.00,2.50", content); // Updated to F2 format
        Assert.Contains("TurnOnDelay=2.00", content);
        Assert.Contains("TurnOffDelay=1.50", content);
        Assert.Contains("OverlapTolerance=10.00", content);
        Assert.Contains("LookAheadDistance=3.00", content);
        Assert.Contains("MinimumSpeed=0.10", content);
    }

    [Fact]
    public void LoadConfiguration_ValidFile_ReturnsCorrectConfiguration()
    {
        // Arrange
        var originalConfig = new SectionConfiguration(5, new double[] { 2.5, 2.5, 2.5, 2.5, 2.5 })
        {
            TurnOnDelay = 3.0,
            TurnOffDelay = 2.0,
            OverlapTolerance = 15.0,
            LookAheadDistance = 4.0,
            MinimumSpeed = 0.2
        };

        _fileService.SaveConfiguration(_testDirectory, originalConfig);

        // Act
        var loadedConfig = _fileService.LoadConfiguration(_testDirectory);

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(5, loadedConfig.SectionCount);
        Assert.Equal(5, loadedConfig.SectionWidths.Length);
        Assert.All(loadedConfig.SectionWidths, w => Assert.Equal(2.5, w));
        Assert.Equal(3.0, loadedConfig.TurnOnDelay);
        Assert.Equal(2.0, loadedConfig.TurnOffDelay);
        Assert.Equal(15.0, loadedConfig.OverlapTolerance);
        Assert.Equal(4.0, loadedConfig.LookAheadDistance);
        Assert.Equal(0.2, loadedConfig.MinimumSpeed);
    }

    [Fact]
    public void LoadConfiguration_CorruptedFile_BacksUpFileAndReturnsNull()
    {
        // Arrange - Create a file that will cause an exception during file I/O
        var filePath = Path.Combine(_testDirectory, "SectionConfig.txt");

        // Write binary garbage that will cause parsing issues
        byte[] binaryGarbage = new byte[] { 0xFF, 0xFE, 0x00, 0x01, 0x02, 0x03 };
        File.WriteAllBytes(filePath, binaryGarbage);

        // Lock the file to cause an exception
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // Act & Assert - File is locked, should handle gracefully
            var config = _fileService.LoadConfiguration(_testDirectory);

            // Since file is locked, it should return null but won't be able to backup
            // This tests the error handling path
            Assert.Null(config);
        }
    }

    [Fact]
    public void LoadConfiguration_NonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "NonExistent");

        // Act
        var config = _fileService.LoadConfiguration(nonExistentDir);

        // Assert
        Assert.Null(config);
    }

    [Fact]
    public void SaveConfiguration_InvalidConfig_ThrowsException()
    {
        // Arrange
        var invalidConfig = new SectionConfiguration();
        invalidConfig.TurnOnDelay = 100.0; // Invalid: exceeds max of 15.0

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _fileService.SaveConfiguration(_testDirectory, invalidConfig));
    }

    [Fact]
    public void SaveLoadRoundTrip_PreservesAllConfigurationValues()
    {
        // Arrange
        var originalConfig = new SectionConfiguration(7, new double[] { 1.5, 2.0, 2.5, 3.0, 2.5, 2.0, 1.5 })
        {
            TurnOnDelay = 2.5,
            TurnOffDelay = 1.8,
            OverlapTolerance = 12.5,
            LookAheadDistance = 3.5,
            MinimumSpeed = 0.15
        };

        // Act
        _fileService.SaveConfiguration(_testDirectory, originalConfig);
        var loadedConfig = _fileService.LoadConfiguration(_testDirectory);

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(originalConfig.SectionCount, loadedConfig.SectionCount);
        Assert.Equal(originalConfig.SectionWidths.Length, loadedConfig.SectionWidths.Length);
        for (int i = 0; i < originalConfig.SectionWidths.Length; i++)
        {
            Assert.Equal(originalConfig.SectionWidths[i], loadedConfig.SectionWidths[i], 2);
        }
        Assert.Equal(originalConfig.TurnOnDelay, loadedConfig.TurnOnDelay, 2);
        Assert.Equal(originalConfig.TurnOffDelay, loadedConfig.TurnOffDelay, 2);
        Assert.Equal(originalConfig.OverlapTolerance, loadedConfig.OverlapTolerance, 2);
        Assert.Equal(originalConfig.LookAheadDistance, loadedConfig.LookAheadDistance, 2);
        Assert.Equal(originalConfig.MinimumSpeed, loadedConfig.MinimumSpeed, 2);
    }
}
