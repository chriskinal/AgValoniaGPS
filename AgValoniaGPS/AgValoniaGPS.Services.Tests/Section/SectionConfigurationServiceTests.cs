using Xunit;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Section;

namespace AgValoniaGPS.Services.Tests.Section;

/// <summary>
/// Tests for SectionConfigurationService
/// Focus: Validation, total width calculation
/// </summary>
public class SectionConfigurationServiceTests
{
    [Fact]
    public void LoadConfiguration_ValidConfiguration_AcceptsAndStores()
    {
        // Arrange
        var service = new SectionConfigurationService();
        var config = new SectionConfiguration(3, new double[] { 2.0, 2.5, 3.0 });

        // Act
        service.LoadConfiguration(config);

        // Assert
        var loaded = service.GetConfiguration();
        Assert.Equal(3, loaded.SectionCount);
        Assert.Equal(2.0, loaded.SectionWidths[0]);
        Assert.Equal(2.5, loaded.SectionWidths[1]);
        Assert.Equal(3.0, loaded.SectionWidths[2]);
    }

    [Fact]
    public void LoadConfiguration_TooManySections_ThrowsArgumentException()
    {
        // Arrange
        var service = new SectionConfigurationService();
        var widths = new double[32];
        for (int i = 0; i < 32; i++) widths[i] = 2.5;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var config = new SectionConfiguration(32, widths);
        });
    }

    [Fact]
    public void LoadConfiguration_NegativeWidth_ThrowsArgumentException()
    {
        // Arrange
        var service = new SectionConfigurationService();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var config = new SectionConfiguration(2, new double[] { 2.5, -1.0 });
        });
    }

    [Fact]
    public void GetTotalWidth_MultipleSections_CalculatesCorrectSum()
    {
        // Arrange
        var service = new SectionConfigurationService();
        var config = new SectionConfiguration(4, new double[] { 2.0, 2.5, 3.0, 1.5 });
        service.LoadConfiguration(config);

        // Act
        double totalWidth = service.GetTotalWidth();

        // Assert
        Assert.Equal(9.0, totalWidth, precision: 3);
    }

    [Fact]
    public void GetSectionWidth_ValidId_ReturnsCorrectWidth()
    {
        // Arrange
        var service = new SectionConfigurationService();
        var config = new SectionConfiguration(3, new double[] { 2.0, 2.5, 3.0 });
        service.LoadConfiguration(config);

        // Act
        double width = service.GetSectionWidth(1);

        // Assert
        Assert.Equal(2.5, width);
    }

    [Fact]
    public void GetSectionWidth_InvalidId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var service = new SectionConfigurationService();
        var config = new SectionConfiguration(3, new double[] { 2.0, 2.5, 3.0 });
        service.LoadConfiguration(config);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetSectionWidth(5));
    }

    [Fact]
    public void GetSectionOffset_SymmetricSections_CalculatesCorrectOffsets()
    {
        // Arrange
        var service = new SectionConfigurationService();
        // 3 sections of 2m each = 6m total, centered at 0
        // Left edge at -3m, sections at: [-3 to -1], [-1 to 1], [1 to 3]
        // Centers at: -2m, 0m, 2m
        var config = new SectionConfiguration(3, new double[] { 2.0, 2.0, 2.0 });
        service.LoadConfiguration(config);

        // Act
        double offset0 = service.GetSectionOffset(0); // Leftmost
        double offset1 = service.GetSectionOffset(1); // Center
        double offset2 = service.GetSectionOffset(2); // Rightmost

        // Assert
        Assert.Equal(-2.0, offset0, precision: 3);
        Assert.Equal(0.0, offset1, precision: 3);
        Assert.Equal(2.0, offset2, precision: 3);
    }
}
