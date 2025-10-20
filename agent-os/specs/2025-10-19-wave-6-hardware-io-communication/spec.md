# Specification: Wave 6 Hardware I/O & Communication

## Goal
Extract and modernize hardware communication infrastructure from AgOpenGPS legacy codebase into a robust, multi-transport communication layer that supports AutoSteer, Machine, and IMU modules across UDP, Bluetooth, CAN, and Radio transports, with comprehensive PGN message handling, module connection monitoring, and realistic hardware simulation for testing.

## User Stories
- As a tractor operator, I want reliable communication with AutoSteer, Machine, and IMU modules so that guidance and section control work consistently
- As a mobile user, I want to connect via Bluetooth from my tablet/phone so that I can operate without requiring a wired network
- As a developer, I want to test hardware integration without physical modules so that I can develop and validate features in CI/CD
- As a field technician, I want the system to detect module disconnections within 2 seconds so that I can troubleshoot connectivity issues quickly
- As an advanced user, I want to integrate with tractor CAN bus systems so that I can read implement data and send commands directly to the tractor
- As a remote operations manager, I want long-range radio communication (1 mile) so that I can monitor multiple vehicles from a base station
- As a developer, I want clean separation between transport layers and message protocols so that adding new transports doesn't require changing message handling code

## Core Requirements

### Functional Requirements
- Parse inbound PGN messages from all three module types (AutoSteer, Machine, IMU)
- Build outbound PGN messages with correct format and CRC validation
- Monitor module connection state using hello packets (2-second timeout)
- Track data flow from each module (100ms for AutoSteer/Machine, 300ms for IMU)
- Support UDP transport (primary, existing implementation to enhance)
- Support Bluetooth Classic (SPP) and BLE transports (user selectable, no auto-fallback)
- Support CAN/ISOBUS bidirectional communication (hybrid mode alongside UDP)
- Support Radio modules (LoRa, 900MHz spread spectrum, WiFi-based)
- Enable per-module transport selection (e.g., AutoSteer via Bluetooth, Machine via UDP, IMU via CAN)
- Implement transport abstraction layer for pluggable transport types
- Provide hardware simulator for all three modules with realistic behaviors
- Integrate with Wave 3 steering services (send steering commands, receive wheel angle feedback)
- Integrate with Wave 4 section control services (send section commands, receive sensor feedback)
- Support formal module "ready" state before sending commands
- Implement lazy initialization (modules initialize on-demand when first accessed)
- Handle protocol versioning with capability negotiation
- Maintain backward compatibility with existing firmware

### Non-Functional Requirements
- **Performance**: Message latency <10ms from send to receive
- **Connection Detection**: Module connection/disconnection detected within 2 seconds
- **Data Flow Monitoring**: Real-time tracking with 100-300ms update intervals
- **Thread Safety**: All services must be thread-safe for concurrent access
- **Simulator Performance**: 10Hz update rate with realistic data generation
- **Transport Switching**: Transition between transports in <1 second
- **Reliability**: CRC validation for all messages with error detection
- **Extensibility**: New PGN message types can be added without breaking changes
- **Cross-Platform**: Support Windows, Linux, macOS (with platform-specific transport implementations)

## Visual Design
No visual assets provided. Hardware communication layer is backend-focused without UI components in this wave. UI configuration screens for transport selection and module monitoring deferred to future UI wave.

## Reusable Components

### Existing Code to Leverage

**Communication Services:**
- `UdpCommunicationService` - UDP socket management, hello packet handling, module connection tracking
  - Reuse pattern: Extend into transport abstraction with UDP as one implementation
  - Already implements: 2-second hello timeout, data flow tracking, PGN detection
  - Location: `AgValoniaGPS.Services/UdpCommunicationService.cs`

- `NmeaParserService` - Binary message parsing patterns, checksum validation, state machine approach
  - Reuse pattern: Similar validation and parsing logic for PGN messages
  - Location: `AgValoniaGPS.Services/NmeaParserService.cs`

**Models:**
- `PgnMessage` - Base PGN message structure with ToBytes()/FromBytes() methods
  - Reuse pattern: Extend with specific message type builders
  - Location: `AgValoniaGPS.Models/PgnMessage.cs`

- `PgnNumbers` - Constants for existing PGN identifiers (Hello, AutoSteer, Machine, IMU)
  - Reuse pattern: Add new PGN numbers for v2 protocol features
  - Location: `AgValoniaGPS.Models/PgnMessage.cs`

**Event Patterns:**
- Wave 1-5 EventArgs patterns for state changes
  - Examples: `SteeringUpdateEventArgs`, `SectionStateChangedEventArgs`
  - Reuse pattern: Create hardware-specific EventArgs (ModuleConnectedEventArgs, etc.)

**Service Registration:**
- `ServiceCollectionExtensions.cs` - DI registration patterns
  - Reuse pattern: Register new hardware services following existing conventions
  - Location: `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`

**Integration Points:**
- Wave 3 `SteeringCoordinatorService` - Outputs steering angle commands
  - Integration: Connect to AutoSteer PGN builder

- Wave 4 `SectionControlService` - Outputs section on/off commands
  - Integration: Connect to Machine PGN builder

### New Components Required

**PgnMessageBuilderService** - Type-safe PGN message construction
- Existing `PgnMessage.ToBytes()` provides basic serialization but lacks type-safe builders for specific message types
- Need dedicated builders for AutoSteer settings, Machine commands, IMU configuration
- Why new: Current approach requires manual byte array manipulation; need fluent API

**PgnMessageParserService** - Parse all inbound PGN message types
- Existing `UdpCommunicationService.ProcessReceivedData()` detects PGN but doesn't parse content
- Need structured parsing for AutoSteer config responses, Machine feedback, IMU data
- Why new: No existing service extracts data fields from PGN payloads

**ModuleCoordinatorService** - Module lifecycle and orchestration
- Existing `UdpCommunicationService` has basic hello tracking but no formal ready state
- Need coordination across multiple transports, lazy initialization, ready state management
- Why new: Current implementation is transport-specific (UDP only); need multi-transport orchestration

**TransportAbstractionService** - Pluggable transport layer
- Existing `UdpCommunicationService` is UDP-specific with socket management
- Need abstract transport interface that UDP, Bluetooth, CAN, Radio can implement
- Why new: Cannot reuse UDP-specific socket code for other transport types

**HardwareSimulatorService** - Realistic hardware simulation
- No existing simulator - requires physical hardware for testing
- Need all three modules with scriptable behaviors for automated tests
- Why new: Critical for CI/CD and development without hardware

**Module-Specific Communication Services:**
- `AutoSteerCommunicationService` - AutoSteer module protocol
- `MachineCommunicationService` - Machine module protocol
- `ImuCommunicationService` - IMU module protocol
- Why new: Each module has unique message formats and state tracking requirements

**Transport Implementations:**
- `BluetoothTransportService` - Bluetooth SPP/BLE (platform-specific)
- `CanBusTransportService` - CAN/ISOBUS integration (USB adapters like PCAN)
- `RadioTransportService` - LoRa/900MHz/WiFi radios
- Why new: No existing Bluetooth, CAN, or Radio implementations

## Technical Approach

### Database
No database changes required. Transport configuration and module settings persistence handled at application level (out of scope for this wave - deferred to configuration management wave).

### Service Architecture

**Communication Layer Hierarchy:**
```
Application Layer (Wave 3/4 Services)
    ↓
Module Communication Services (AutoSteer, Machine, IMU)
    ↓
Module Coordinator Service (Connection tracking, ready state)
    ↓
PGN Message Builder/Parser Services (Protocol layer)
    ↓
Transport Abstraction Service (Multi-transport routing)
    ↓
Transport Implementations (UDP, Bluetooth, CAN, Radio)
```

### API / Service Interfaces

**IPgnMessageBuilderService**
```csharp
public interface IPgnMessageBuilderService
{
    // AutoSteer outbound messages
    byte[] BuildAutoSteerSettings(
        byte pwmDrive,
        byte minPwm,
        float proportionalGain,
        byte highPwm,
        float lowSpeedPwm);

    byte[] BuildAutoSteerData(
        double speedMph,
        byte status,
        double steerAngle,
        int crossTrackErrorMm);

    // Machine outbound messages
    byte[] BuildMachineData(
        byte[] relayLo,
        byte[] relayHi,
        byte tramLine,
        byte[] sectionState);

    byte[] BuildMachineConfig(
        ushort sections,
        ushort zones,
        ushort totalWidth);

    // IMU outbound messages
    byte[] BuildImuConfig(byte configFlags);

    // Module discovery
    byte[] BuildScanRequest();
    byte[] BuildHelloPacket();
}
```

**IPgnMessageParserService**
```csharp
public interface IPgnMessageParserService
{
    // Parse generic PGN
    PgnMessage? ParseMessage(byte[] data);

    // AutoSteer inbound messages
    AutoSteerConfigResponse? ParseAutoSteerConfig(byte[] data);
    AutoSteerFeedback? ParseAutoSteerData(byte[] data);

    // Machine inbound messages
    MachineConfigResponse? ParseMachineConfig(byte[] data);
    MachineFeedback? ParseMachineData(byte[] data);

    // IMU inbound messages
    ImuData? ParseImuData(byte[] data);

    // Module discovery
    ScanResponse? ParseScanResponse(byte[] data);
    HelloResponse? ParseHelloPacket(byte[] data);

    // Validation
    bool ValidateCrc(byte[] data);
    byte CalculateCrc(byte[] data);
}
```

**ITransportAbstractionService**
```csharp
public interface ITransportAbstractionService
{
    // Transport management
    Task StartTransportAsync(ModuleType module, TransportType transport);
    Task StopTransportAsync(ModuleType module);
    void SendMessage(ModuleType module, byte[] data);

    // Transport selection
    TransportType GetActiveTransport(ModuleType module);
    void SetPreferredTransport(ModuleType module, TransportType transport);

    // Events
    event EventHandler<TransportDataReceivedEventArgs> DataReceived;
    event EventHandler<TransportStateChangedEventArgs> TransportStateChanged;
}
```

**IModuleCoordinatorService**
```csharp
public interface IModuleCoordinatorService
{
    // Module state
    ModuleState GetModuleState(ModuleType module);
    bool IsModuleReady(ModuleType module);
    DateTime GetLastHelloTime(ModuleType module);
    DateTime GetLastDataTime(ModuleType module);

    // Lifecycle
    Task InitializeModuleAsync(ModuleType module);
    void ResetModule(ModuleType module);

    // Events
    event EventHandler<ModuleConnectedEventArgs> ModuleConnected;
    event EventHandler<ModuleDisconnectedEventArgs> ModuleDisconnected;
    event EventHandler<ModuleReadyEventArgs> ModuleReady;
}
```

**IAutoSteerCommunicationService**
```csharp
public interface IAutoSteerCommunicationService
{
    // Commands
    void SendSteeringCommand(double speedMph, double steerAngle, int xteErrorMm, bool isActive);
    void SendSettings(AutoSteerSettings settings);
    void SendConfiguration(AutoSteerConfig config);

    // State
    AutoSteerFeedback? CurrentFeedback { get; }
    double ActualWheelAngle { get; }
    byte[] SwitchStates { get; }

    // Events
    event EventHandler<AutoSteerFeedbackEventArgs> FeedbackReceived;
    event EventHandler<SwitchStateChangedEventArgs> SwitchStateChanged;
}
```

**IMachineCommunicationService**
```csharp
public interface IMachineCommunicationService
{
    // Commands
    void SendSectionStates(byte[] sectionStates);
    void SendRelayStates(byte[] relayLo, byte[] relayHi);
    void SendConfiguration(MachineConfig config);

    // State
    MachineFeedback? CurrentFeedback { get; }
    bool WorkSwitchActive { get; }
    byte[] SectionSensors { get; }

    // Events
    event EventHandler<MachineFeedbackEventArgs> FeedbackReceived;
    event EventHandler<WorkSwitchChangedEventArgs> WorkSwitchChanged;
}
```

**IImuCommunicationService**
```csharp
public interface IImuCommunicationService
{
    // Commands
    void SendConfiguration(ImuConfig config);
    void RequestCalibration();

    // State
    ImuData? CurrentData { get; }
    double Roll { get; }
    double Pitch { get; }
    double Heading { get; }
    bool IsCalibrated { get; }

    // Events
    event EventHandler<ImuDataReceivedEventArgs> DataReceived;
    event EventHandler<ImuCalibrationChangedEventArgs> CalibrationChanged;
}
```

**IHardwareSimulatorService**
```csharp
public interface IHardwareSimulatorService
{
    // Simulator control
    Task StartAsync();
    Task StopAsync();
    void EnableRealisticBehavior(bool enabled);

    // Manual value control
    void SetSteeringAngle(double angle);
    void SetSectionSensor(int section, bool active);
    void SetImuRoll(double roll);
    void SetImuPitch(double pitch);

    // Scripted behaviors
    void LoadScript(string scriptPath);
    void ExecuteScript();

    // Realism modes
    void SetSteeringResponseTime(double seconds);
    void SetImuDriftRate(double degreesPerMinute);
    void SetGpsJitterMagnitude(double meters);

    // Events
    event EventHandler<SimulatorStateChangedEventArgs> StateChanged;
}
```

**Transport Interfaces:**
```csharp
public interface ITransport
{
    TransportType Type { get; }
    bool IsConnected { get; }

    Task StartAsync();
    Task StopAsync();
    void Send(byte[] data);

    event EventHandler<byte[]> DataReceived;
    event EventHandler<bool> ConnectionChanged;
}

public interface IUdpTransportService : ITransport { }
public interface IBluetoothTransportService : ITransport
{
    BluetoothMode Mode { get; set; } // SPP or BLE
}
public interface ICanBusTransportService : ITransport
{
    string AdapterPath { get; set; } // e.g., /dev/pcanusb0
}
public interface IRadioTransportService : ITransport
{
    RadioType RadioType { get; set; } // LoRa, 900MHz, WiFi
    int TransmitPower { get; set; }
}
```

### Domain Models & Enums

**Models (AgValoniaGPS.Models/):**

```csharp
// Module Types
public enum ModuleType
{
    AutoSteer,
    Machine,
    IMU
}

// Module Connection States
public enum ModuleState
{
    Disconnected,
    Connecting,
    HelloReceived,
    Ready,
    Error,
    Timeout
}

// Transport Types
public enum TransportType
{
    UDP,
    BluetoothSPP,
    BluetoothBLE,
    CAN,
    Radio
}

// Bluetooth Modes
public enum BluetoothMode
{
    ClassicSPP,
    BLE
}

// Radio Types
public enum RadioType
{
    LoRa,
    Spread900MHz,
    WiFi
}

// PGN Message Responses
public class AutoSteerConfigResponse
{
    public byte Version { get; set; }
    public byte[] Capabilities { get; set; }
    public double MaxSteerAngle { get; set; }
    public double MinSteerAngle { get; set; }
}

public class AutoSteerFeedback
{
    public double ActualWheelAngle { get; set; }
    public byte[] SwitchStates { get; set; }
    public byte StatusFlags { get; set; }
    public DateTime Timestamp { get; set; }
}

public class MachineConfigResponse
{
    public byte Version { get; set; }
    public ushort MaxSections { get; set; }
    public ushort MaxZones { get; set; }
}

public class MachineFeedback
{
    public bool WorkSwitchActive { get; set; }
    public byte[] SectionSensors { get; set; }
    public byte StatusFlags { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ImuData
{
    public double Roll { get; set; }
    public double Pitch { get; set; }
    public double Heading { get; set; }
    public double YawRate { get; set; }
    public bool IsCalibrated { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ScanResponse
{
    public ModuleType ModuleType { get; set; }
    public byte Version { get; set; }
    public byte[] Capabilities { get; set; }
}

public class HelloResponse
{
    public ModuleType ModuleType { get; set; }
    public byte Version { get; set; }
    public DateTime ReceivedAt { get; set; }
}

// Transport Configuration
public class TransportConfiguration
{
    public ModuleType Module { get; set; }
    public TransportType Type { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}

// Hardware Capabilities
public class ModuleCapabilities
{
    public byte[] RawCapabilities { get; set; }

    public bool SupportsV2Protocol => (RawCapabilities[0] & 0x01) != 0;
    public bool SupportsDualAntenna => (RawCapabilities[0] & 0x02) != 0;
    public bool SupportsRollCompensation => (RawCapabilities[0] & 0x04) != 0;
}

// Firmware Version
public class FirmwareVersion
{
    public byte Major { get; set; }
    public byte Minor { get; set; }
    public byte Patch { get; set; }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
```

**EventArgs (AgValoniaGPS.Models/Events/):**

```csharp
public class ModuleConnectedEventArgs : EventArgs
{
    public ModuleType ModuleType { get; set; }
    public TransportType Transport { get; set; }
    public FirmwareVersion Version { get; set; }
    public DateTime ConnectedAt { get; set; }
}

public class ModuleDisconnectedEventArgs : EventArgs
{
    public ModuleType ModuleType { get; set; }
    public string Reason { get; set; }
    public DateTime DisconnectedAt { get; set; }
}

public class ModuleReadyEventArgs : EventArgs
{
    public ModuleType ModuleType { get; set; }
    public ModuleCapabilities Capabilities { get; set; }
    public DateTime ReadyAt { get; set; }
}

public class PgnMessageReceivedEventArgs : EventArgs
{
    public byte PgnNumber { get; set; }
    public byte[] Data { get; set; }
    public ModuleType SourceModule { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public class TransportStateChangedEventArgs : EventArgs
{
    public ModuleType Module { get; set; }
    public TransportType Transport { get; set; }
    public bool IsConnected { get; set; }
    public string StateMessage { get; set; }
}

public class AutoSteerFeedbackEventArgs : EventArgs
{
    public AutoSteerFeedback Feedback { get; set; }
}

public class MachineFeedbackEventArgs : EventArgs
{
    public MachineFeedback Feedback { get; set; }
}

public class ImuDataReceivedEventArgs : EventArgs
{
    public ImuData Data { get; set; }
}

public class WorkSwitchChangedEventArgs : EventArgs
{
    public bool IsActive { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class ImuCalibrationChangedEventArgs : EventArgs
{
    public bool IsCalibrated { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class SimulatorStateChangedEventArgs : EventArgs
{
    public bool IsRunning { get; set; }
    public bool RealisticBehaviorEnabled { get; set; }
}

public class TransportDataReceivedEventArgs : EventArgs
{
    public ModuleType Module { get; set; }
    public byte[] Data { get; set; }
    public DateTime ReceivedAt { get; set; }
}
```

### PGN Message Specifications

**Extended PGN Numbers (add to PgnNumbers class):**

```csharp
// V2 Protocol PGNs (reserved for future features)
public const byte AUTOSTEER_SETTINGS_V2 = 240;
public const byte MACHINE_DATA_V2 = 237;
public const byte IMU_DATA_V2 = 220;

// IMU PGNs
public const byte IMU_DATA = 219;
public const byte IMU_CONFIG = 218;
public const byte IMU_CALIBRATION_REQUEST = 217;
public const byte IMU_CALIBRATION_STATUS = 216;

// Machine extended PGNs
public const byte MACHINE_RELAY_STATES = 235;
public const byte WORK_SWITCH_STATE = 234;
public const byte SECTION_SENSOR_FEEDBACK = 233;

// AutoSteer extended PGNs
public const byte AUTOSTEER_SWITCH_STATES = 249;
public const byte AUTOSTEER_WHEEL_ANGLE = 248;

// Configuration handshake
public const byte CAPABILITY_REQUEST = 210;
public const byte CAPABILITY_RESPONSE = 211;
```

**AutoSteer PGN Messages:**

**PGN 254 - AutoSteer Data (Outbound):**
```
Header: 0x80, 0x81
Source: 0x7F
PGN: 254 (0xFE)
Length: 10
Data:
  [0-1]: speedHi/speedLo - Speed * 10 (uint16, big-endian)
  [2]: status - 0=off, 1=on, 2=error
  [3-4]: steerAngleHi/steerAngleLo - Angle * 100 (int16, big-endian, degrees)
  [5-6]: distanceHi/distanceLo - XTE in mm (int16, big-endian)
  [7-8]: Reserved
CRC: Sum of bytes 2 through 14
```

**PGN 253 - AutoSteer Data Alternate (Inbound):**
```
Header: 0x80, 0x81
Source: 0x7E (AutoSteer module)
PGN: 253 (0xFD)
Length: 8
Data:
  [0-1]: actualAngleHi/actualAngleLo - Actual wheel angle * 100 (int16)
  [2]: switchStates - Bitmap of switch inputs
  [3]: statusFlags - Module status flags
  [4-7]: Reserved
CRC: Sum of bytes 2 through length+4
```

**PGN 252 - Steer Settings (Outbound):**
```
Header: 0x80, 0x81
Source: 0x7F
PGN: 252 (0xFC)
Length: 12
Data:
  [0]: pwmDrive - PWM drive level (0-255)
  [1]: minPwm - Minimum PWM threshold
  [2-3]: proportionalGain * 100 (uint16)
  [4]: highPwm - High PWM limit
  [5-6]: lowSpeedPwm * 100 (uint16)
  [7-11]: Reserved for additional PID parameters
CRC: Sum of bytes
```

**Machine PGN Messages:**

**PGN 239 - Machine Data (Outbound):**
```
Header: 0x80, 0x81
Source: 0x7F
PGN: 239 (0xEF)
Length: Variable (depends on section count)
Data:
  [0-7]: relayLo - Low 8 relay states (1 bit per relay)
  [8-15]: relayHi - High 8 relay states
  [16]: tramLine - Tram line guidance bits
  [17-N]: sectionState - 1 byte per section (0=off, 1=on, 2=auto)
CRC: Sum of bytes
```

**PGN 238 - Machine Config (Outbound):**
```
Header: 0x80, 0x81
Source: 0x7F
PGN: 238 (0xEE)
Length: 6
Data:
  [0-1]: sections - Number of sections (uint16)
  [2-3]: zones - Number of zones (uint16)
  [4-5]: totalWidth - Total implement width in cm (uint16)
CRC: Sum of bytes
```

**PGN 234 - Work Switch State (Inbound):**
```
Header: 0x80, 0x81
Source: 0x7B (Machine module)
PGN: 234 (0xEA)
Length: 2
Data:
  [0]: workSwitchActive - 0=inactive, 1=active
  [1]: statusFlags - Module status
CRC: Sum of bytes
```

**IMU PGN Messages:**

**PGN 219 - IMU Data (Inbound):**
```
Header: 0x80, 0x81
Source: 0x79 (IMU module)
PGN: 219 (0xDB)
Length: 16
Data:
  [0-1]: roll * 100 (int16, degrees)
  [2-3]: pitch * 100 (int16, degrees)
  [4-5]: heading * 100 (uint16, degrees)
  [6-7]: yawRate * 100 (int16, degrees/sec)
  [8]: isCalibrated - 0=uncalibrated, 1=calibrated
  [9-15]: Reserved
CRC: Sum of bytes
```

**PGN 218 - IMU Config (Outbound):**
```
Header: 0x80, 0x81
Source: 0x7F
PGN: 218 (0xDA)
Length: 4
Data:
  [0]: configFlags - Configuration bitmap
  [1-3]: Reserved
CRC: Sum of bytes
```

**Hello Packets (Existing):**

**PGN 200 - Hello from AgIO (Outbound):**
```
Current implementation: [0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, CRC]
Keep existing format for backward compatibility
```

**PGN 126 - Hello from AutoSteer (Inbound):**
**PGN 123 - Hello from Machine (Inbound):**
**PGN 121 - Hello from IMU (Inbound):**
```
Existing format - maintain compatibility
Add version field in data payload for capability negotiation
```

### Protocol Versioning & Extensibility

**Version Management Strategy:**

1. **Hello Packet Version Field:**
   - Byte 5 of hello packet data = firmware version (major.minor)
   - Example: 0x12 = version 1.2
   - Backward compatible: Version 0 = legacy firmware (no version field)

2. **Capability Negotiation:**
   - After hello exchange, send PGN 210 (Capability Request)
   - Module responds with PGN 211 (Capability Response)
   - Response includes bitmap of supported features:
     - Bit 0: V2 protocol support
     - Bit 1: Dual antenna support
     - Bit 2: Roll compensation
     - Bit 3: Extended section count (>16 sections)
     - Bits 4-7: Reserved

3. **Reserved PGN Numbers:**
   - V2 AutoSteer: 240-247
   - V2 Machine: 230-237
   - V2 IMU: 215-220
   - Existing PGNs (>240 AutoSteer, >230 Machine, >215 IMU) remain unchanged

4. **Mixed Version Support:**
   - Application detects module versions during handshake
   - Use V1 PGNs for legacy modules, V2 PGNs for updated modules
   - Graceful degradation: Advanced features disabled if module doesn't support

### File Structure

**Service Organization (following NAMING_CONVENTIONS.md):**

```
AgValoniaGPS.Services/Communication/
├── PgnMessageBuilderService.cs
├── IPgnMessageBuilderService.cs
├── PgnMessageParserService.cs
├── IPgnMessageParserService.cs
├── ModuleCoordinatorService.cs
├── IModuleCoordinatorService.cs
├── TransportAbstractionService.cs
├── ITransportAbstractionService.cs
├── AutoSteerCommunicationService.cs
├── IAutoSteerCommunicationService.cs
├── MachineCommunicationService.cs
├── IMachineCommunicationService.cs
├── ImuCommunicationService.cs
├── IImuCommunicationService.cs
├── HardwareSimulatorService.cs
├── IHardwareSimulatorService.cs

AgValoniaGPS.Services/Communication/Transports/
├── ITransport.cs (base interface)
├── UdpTransportService.cs (refactor existing UdpCommunicationService)
├── IUdpTransportService.cs
├── BluetoothTransportService.cs
├── IBluetoothTransportService.cs
├── CanBusTransportService.cs
├── ICanBusTransportService.cs
├── RadioTransportService.cs
├── IRadioTransportService.cs

AgValoniaGPS.Models/Communication/
├── ModuleType.cs (enum)
├── ModuleState.cs (enum)
├── TransportType.cs (enum)
├── BluetoothMode.cs (enum)
├── RadioType.cs (enum)
├── AutoSteerConfigResponse.cs
├── AutoSteerFeedback.cs
├── MachineConfigResponse.cs
├── MachineFeedback.cs
├── ImuData.cs
├── ScanResponse.cs
├── HelloResponse.cs
├── TransportConfiguration.cs
├── ModuleCapabilities.cs
├── FirmwareVersion.cs

AgValoniaGPS.Models/Events/
├── ModuleConnectedEventArgs.cs
├── ModuleDisconnectedEventArgs.cs
├── ModuleReadyEventArgs.cs
├── PgnMessageReceivedEventArgs.cs
├── TransportStateChangedEventArgs.cs
├── AutoSteerFeedbackEventArgs.cs
├── MachineFeedbackEventArgs.cs
├── ImuDataReceivedEventArgs.cs
├── WorkSwitchChangedEventArgs.cs
├── ImuCalibrationChangedEventArgs.cs
├── SimulatorStateChangedEventArgs.cs
├── TransportDataReceivedEventArgs.cs

AgValoniaGPS.Services.Tests/Communication/
├── PgnMessageBuilderServiceTests.cs
├── PgnMessageParserServiceTests.cs
├── ModuleCoordinatorServiceTests.cs
├── TransportAbstractionServiceTests.cs
├── AutoSteerCommunicationServiceTests.cs
├── MachineCommunicationServiceTests.cs
├── ImuCommunicationServiceTests.cs
├── HardwareSimulatorServiceTests.cs
├── UdpTransportServiceTests.cs
├── BluetoothTransportServiceTests.cs
├── CanBusTransportServiceTests.cs
├── RadioTransportServiceTests.cs
├── CommunicationIntegrationTests.cs
```

**IMPORTANT:** Follow naming convention - use `Communication/` directory (functional area) not `Module/` or `Hardware/` (avoid namespace collision with potential Model classes).

### Integration Points

**Wave 3 Integration (Steering Algorithms):**

**Outbound - SteeringCoordinatorService → AutoSteer Module:**
```csharp
// In SteeringCoordinatorService.Update() method:
if (isAutoSteerActive)
{
    double speedMph = speed * 2.23694; // m/s to mph
    int xteMm = (int)(CurrentCrossTrackError * 1000); // meters to mm

    _autoSteerComm.SendSteeringCommand(
        speedMph,
        CurrentSteeringAngle,
        xteMm,
        isAutoSteerActive);
}
```

**Inbound - AutoSteer Feedback → Steering Services:**
```csharp
// AutoSteerCommunicationService raises FeedbackReceived event
// SteeringCoordinatorService subscribes to compare desired vs actual:
_autoSteerComm.FeedbackReceived += (sender, e) =>
{
    double desiredAngle = CurrentSteeringAngle;
    double actualAngle = e.Feedback.ActualWheelAngle;
    double error = desiredAngle - actualAngle;

    // Log for diagnostics or adjust integral control
    if (Math.Abs(error) > 2.0)
    {
        // Large error - possible mechanical issue
    }
};
```

**Wave 4 Integration (Section Control):**

**Outbound - SectionControlService → Machine Module:**
```csharp
// In SectionControlService when section states change:
public void UpdateSectionStates(Section[] sections)
{
    byte[] sectionStates = new byte[sections.Length];
    for (int i = 0; i < sections.Length; i++)
    {
        sectionStates[i] = sections[i].State switch
        {
            SectionState.Off => 0,
            SectionState.On => 1,
            SectionState.Auto => 2,
            _ => 0
        };
    }

    _machineComm.SendSectionStates(sectionStates);
}
```

**Inbound - Machine Feedback → Section Control:**
```csharp
// MachineCommunicationService raises WorkSwitchChanged event
// SectionControlService subscribes to enable/disable control:
_machineComm.WorkSwitchChanged += (sender, e) =>
{
    if (!e.IsActive)
    {
        // Work switch released - turn off all sections
        DisableAllSections();
    }
    else
    {
        // Work switch pressed - resume auto control
        EnableAutoControl();
    }
};

// Section sensor feedback updates coverage state
_machineComm.FeedbackReceived += (sender, e) =>
{
    for (int i = 0; i < e.Feedback.SectionSensors.Length; i++)
    {
        bool isActuallyOn = (e.Feedback.SectionSensors[i] & 0x01) != 0;
        _sections[i].ActualState = isActuallyOn;
    }
};
```

**Closed-Loop Control Examples:**

**Steering Closed-Loop:**
- Desired angle from SteeringCoordinatorService
- Command sent via AutoSteerCommunicationService
- Actual angle received in AutoSteerFeedback
- Error logged/displayed for operator feedback
- Integral control adjusts for steady-state error

**Section Control Closed-Loop:**
- Section command sent via MachineCommunicationService
- Sensor feedback confirms actual section state
- Coverage map updated based on actual state (not commanded state)
- Prevents coverage gaps if section fails to activate

### Testing Strategy

**Unit Tests:**

**PGN Message Building:**
- Test each message type builds correct byte array
- Test CRC calculation matches expected values
- Test edge cases: max values, zero values, negative values
- Test V1 vs V2 message format differences

**PGN Message Parsing:**
- Test each message type parses correctly from byte array
- Test CRC validation rejects corrupted messages
- Test partial message handling (incomplete data)
- Test unknown PGN numbers (graceful skip)

**Module Connection Management:**
- Test hello timeout detection (2-second threshold)
- Test data timeout detection (100ms AutoSteer, 300ms IMU)
- Test ready state transitions
- Test lazy initialization triggers

**Transport Abstraction:**
- Mock transport implementation
- Test multi-transport routing (AutoSteer→BT, Machine→UDP)
- Test transport switching
- Test send/receive through abstraction layer

**Integration Tests:**

**Module Communication Flow:**
```csharp
[Test]
public async Task FullAutoSteerCommunicationLoop()
{
    // Arrange: Start simulator
    await _simulator.StartAsync();

    // Act: Send steering command
    _autoSteerComm.SendSteeringCommand(
        speedMph: 10.0,
        steerAngle: 5.0,
        xteErrorMm: 100,
        isActive: true);

    // Wait for feedback
    AutoSteerFeedback? feedback = null;
    _autoSteerComm.FeedbackReceived += (s, e) => feedback = e.Feedback;
    await Task.Delay(200); // Allow simulator to respond

    // Assert: Feedback received with realistic angle
    Assert.That(feedback, Is.Not.Null);
    Assert.That(feedback.ActualWheelAngle, Is.InRange(4.5, 5.5)); // Realistic response
}
```

**Multi-Transport Switching:**
```csharp
[Test]
public async Task SwitchAutoSteerFromUdpToBluetooth()
{
    // Arrange: Start on UDP
    await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);
    Assert.That(_transport.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.UDP));

    // Act: Switch to Bluetooth
    await _transport.StopTransportAsync(ModuleType.AutoSteer);
    await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.BluetoothSPP);

    // Assert: Now on Bluetooth
    Assert.That(_transport.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.BluetoothSPP));
}
```

**Closed-Loop Integration with Wave 3:**
```csharp
[Test]
public async Task SteeringClosedLoopWithHardwareSimulator()
{
    // Arrange: Setup steering coordinator and simulator
    await _simulator.StartAsync();
    _simulator.EnableRealisticBehavior(true);

    var guidanceResult = new GuidanceLineResult
    {
        CrossTrackError = 0.5, // 0.5m off line
        Heading = 90.0
    };

    // Act: Run steering update
    _steeringCoordinator.Update(
        pivotPosition: new Position3D(0, 0, 0),
        steerPosition: new Position3D(1, 0, 0),
        guidanceResult: guidanceResult,
        speed: 5.0,
        heading: 85.0, // 5 degree heading error
        isAutoSteerActive: true);

    // Wait for feedback loop
    await Task.Delay(300);

    // Assert: Actual angle approaches desired angle
    double desiredAngle = _steeringCoordinator.CurrentSteeringAngle;
    double actualAngle = _autoSteerComm.ActualWheelAngle;
    Assert.That(actualAngle, Is.InRange(desiredAngle - 2.0, desiredAngle + 2.0));
}
```

**Simulator Realism Tests:**
```csharp
[Test]
public async Task SimulatorGradualSteeringResponse()
{
    // Arrange: Enable realistic behavior
    await _simulator.StartAsync();
    _simulator.EnableRealisticBehavior(true);
    _simulator.SetSteeringResponseTime(0.5); // 500ms response

    // Act: Command instant angle change
    _autoSteerComm.SendSteeringCommand(10.0, 20.0, 0, true);

    // Assert: Angle changes gradually, not instantly
    await Task.Delay(100);
    Assert.That(_autoSteerComm.ActualWheelAngle, Is.LessThan(10.0)); // Partway

    await Task.Delay(400);
    Assert.That(_autoSteerComm.ActualWheelAngle, Is.InRange(18.0, 22.0)); // Near target
}
```

**Performance Tests:**
- Message build time: <5ms per message
- Message parse time: <5ms per message
- Full send/receive loop: <10ms total latency
- Simulator update rate: 10Hz sustained (100ms updates)
- 1000 messages processed without memory growth (no leaks)

**Real Hardware Validation:**
- Connect to physical AutoSteer/Machine/IMU modules
- Verify hello packets received within 2 seconds
- Verify data flow at expected rates (50Hz AutoSteer, 10Hz IMU)
- Test CRC validation with real hardware messages
- Test transport switching with physical devices

### Performance Requirements

**Message Handling:**
- PGN build: <5ms per message
- PGN parse: <5ms per message
- CRC calculation: <1ms
- Message send: <10ms total latency (build + transport)

**Connection Monitoring:**
- Hello timeout: Exactly 2000ms ±50ms
- Data timeout: AutoSteer/Machine 100ms ±10ms, IMU 300ms ±20ms
- State change events: Fire within 50ms of detection

**Simulator Performance:**
- Update rate: 10Hz (100ms cycle) sustained
- All three modules: <30ms total processing time per cycle
- Realistic behaviors: No frame drops or timing glitches

**Transport Switching:**
- Disconnect: <500ms to close transport
- Connect: <1000ms to establish new transport
- Total switch time: <1500ms

**Memory & Resources:**
- No memory growth over 1000+ messages (verify with profiler)
- Socket/handle cleanup: All resources released on Stop()
- Thread safety: No race conditions under concurrent access

## Out of Scope

**Explicitly Excluded from Wave 6:**
- AgShare cloud integration (separate wave)
- User interface configuration screens (deferred to UI wave)
- Advanced analytics dashboards
- Telemetry and logging UI
- Multi-vehicle coordination features
- Hardware-specific device drivers (use abstraction layer)
- Mobile companion app integration
- Configuration persistence (application-level concern, not this wave)
- Serial port communication (legacy, UDP/BT preferred)
- Real-time diagnostics UI (service-level diagnostics only)

## Success Criteria

**Core Functionality:**
- All 8 core services implemented with full interfaces
- All 4 transport implementations functional (UDP, Bluetooth, CAN, Radio)
- All PGN message types can be built and parsed correctly
- Module connection tracking works with 2-second hello timeout
- Data flow monitoring detects timeouts at specified intervals
- Hardware simulator runs all three modules with realistic behaviors

**Integration:**
- Wave 3 integration: Steering commands sent, wheel angle feedback received
- Wave 4 integration: Section commands sent, work switch feedback received
- Closed-loop control validated in integration tests

**Testing:**
- Unit tests achieve >90% code coverage
- Integration tests validate full communication loops
- Performance benchmarks meet all latency requirements
- Simulator realism validated with manual testing

**Protocol Compliance:**
- Backward compatibility: Works with existing AgOpenGPS firmware
- V2 protocol: Capability negotiation functional
- CRC validation: All messages validated, corruption detected
- Mixed versions: Application handles mixed firmware versions gracefully

**Performance:**
- Message latency <10ms
- Connection detection <2 seconds
- Simulator 10Hz sustained rate
- No memory leaks over extended operation

**Cross-Platform:**
- UDP transport: Works on Windows, Linux, macOS
- Bluetooth transport: Platform-specific implementations tested
- CAN transport: Works with USB adapters on all platforms
- Radio transport: Works with supported radio modules
