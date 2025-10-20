using AgValoniaGPS.Models.Communication;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Unit tests for Communication domain models.
/// Tests enum values, model properties, and capability decoding.
/// </summary>
public class CommunicationModelsTests
{
    [Fact]
    public void ModuleState_Transitions_FollowExpectedFlow()
    {
        // Arrange & Act
        var states = new[]
        {
            ModuleState.Disconnected,
            ModuleState.Connecting,
            ModuleState.HelloReceived,
            ModuleState.Ready
        };

        // Assert - Verify enum values exist and can be used in state machine
        Assert.Equal(ModuleState.Disconnected, states[0]);
        Assert.Equal(ModuleState.Connecting, states[1]);
        Assert.Equal(ModuleState.HelloReceived, states[2]);
        Assert.Equal(ModuleState.Ready, states[3]);

        // Verify error states exist
        Assert.True(Enum.IsDefined(typeof(ModuleState), ModuleState.Error));
        Assert.True(Enum.IsDefined(typeof(ModuleState), ModuleState.Timeout));
    }

    [Fact]
    public void ModuleCapabilities_BitmapDecoding_V2ProtocolSupport()
    {
        // Arrange - V2 protocol bit set (bit 0)
        var capabilities = new ModuleCapabilities
        {
            RawCapabilities = new byte[] { 0x01 } // Bit 0 set
        };

        // Act & Assert
        Assert.True(capabilities.SupportsV2Protocol);
        Assert.False(capabilities.SupportsDualAntenna);
        Assert.False(capabilities.SupportsRollCompensation);
    }

    [Fact]
    public void ModuleCapabilities_BitmapDecoding_DualAntennaSupport()
    {
        // Arrange - Dual antenna bit set (bit 1)
        var capabilities = new ModuleCapabilities
        {
            RawCapabilities = new byte[] { 0x02 } // Bit 1 set
        };

        // Act & Assert
        Assert.False(capabilities.SupportsV2Protocol);
        Assert.True(capabilities.SupportsDualAntenna);
        Assert.False(capabilities.SupportsRollCompensation);
    }

    [Fact]
    public void ModuleCapabilities_BitmapDecoding_RollCompensationSupport()
    {
        // Arrange - Roll compensation bit set (bit 2)
        var capabilities = new ModuleCapabilities
        {
            RawCapabilities = new byte[] { 0x04 } // Bit 2 set
        };

        // Act & Assert
        Assert.False(capabilities.SupportsV2Protocol);
        Assert.False(capabilities.SupportsDualAntenna);
        Assert.True(capabilities.SupportsRollCompensation);
    }

    [Fact]
    public void ModuleCapabilities_BitmapDecoding_MultipleCapabilities()
    {
        // Arrange - V2 + Dual antenna + Roll compensation (0x01 | 0x02 | 0x04 = 0x07)
        var capabilities = new ModuleCapabilities
        {
            RawCapabilities = new byte[] { 0x07 }
        };

        // Act & Assert
        Assert.True(capabilities.SupportsV2Protocol);
        Assert.True(capabilities.SupportsDualAntenna);
        Assert.True(capabilities.SupportsRollCompensation);
    }

    [Fact]
    public void FirmwareVersion_ToString_FormatsCorrectly()
    {
        // Arrange
        var version = new FirmwareVersion
        {
            Major = 2,
            Minor = 5,
            Patch = 3
        };

        // Act
        var formatted = version.ToString();

        // Assert
        Assert.Equal("2.5.3", formatted);
    }

    [Fact]
    public void TransportConfiguration_Parameters_StoreCorrectly()
    {
        // Arrange
        var config = new TransportConfiguration
        {
            Module = ModuleType.AutoSteer,
            Type = TransportType.BluetoothSPP,
            Parameters = new Dictionary<string, string>
            {
                { "DeviceAddress", "00:11:22:33:44:55" },
                { "Mode", "SPP" }
            }
        };

        // Act & Assert
        Assert.Equal(ModuleType.AutoSteer, config.Module);
        Assert.Equal(TransportType.BluetoothSPP, config.Type);
        Assert.Equal("00:11:22:33:44:55", config.Parameters["DeviceAddress"]);
        Assert.Equal("SPP", config.Parameters["Mode"]);
    }
}
