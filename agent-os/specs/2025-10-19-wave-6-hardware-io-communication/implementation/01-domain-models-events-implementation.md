# Task 1: Domain Models & Events

## Overview
**Task Reference:** Task #1 from `agent-os/specs/2025-10-19-wave-6-hardware-io-communication/tasks.md`
**Implemented By:** api-engineer
**Date:** 2025-10-19
**Status:** ✅ Complete

### Task Description
Create comprehensive domain models and events for Wave 6 Hardware I/O & Communication infrastructure. This includes enums for module types and states, PGN response models for AutoSteer/Machine/IMU modules, configuration models, and all EventArgs for communication events.

## Implementation Summary
Implemented the complete foundation layer for Wave 6 by creating 5 enums, 10 domain models, and 12 EventArgs classes following existing Wave 1-5 patterns. All models are organized in the `Communication/` directory (per NAMING_CONVENTIONS.md) to avoid namespace collisions. The implementation provides type-safe representations of hardware module states, transport configurations, and PGN message responses, with proper capability bitmap decoding and firmware version formatting.

All 7 focused tests pass, validating enum transitions, bitmap decoding, version formatting, and parameter storage. The models follow established patterns from existing waves using readonly fields in EventArgs, DateTime.UtcNow timestamps, and property-based capability helpers.

## Files Changed/Created

### New Files
- `AgValoniaGPS.Models/Communication/ModuleType.cs` - Enum for AutoSteer, Machine, and IMU module types
- `AgValoniaGPS.Models/Communication/ModuleState.cs` - Enum for module connection state machine (Disconnected → Connecting → HelloReceived → Ready)
- `AgValoniaGPS.Models/Communication/TransportType.cs` - Enum for UDP, Bluetooth (SPP/BLE), CAN, and Radio transport types
- `AgValoniaGPS.Models/Communication/BluetoothMode.cs` - Enum for ClassicSPP and BLE Bluetooth modes
- `AgValoniaGPS.Models/Communication/RadioType.cs` - Enum for LoRa, Spread900MHz, and WiFi radio types
- `AgValoniaGPS.Models/Communication/AutoSteerConfigResponse.cs` - Model for AutoSteer module configuration (version, capabilities, angle limits)
- `AgValoniaGPS.Models/Communication/AutoSteerFeedback.cs` - Model for AutoSteer feedback data (actual wheel angle, switch states, timestamp)
- `AgValoniaGPS.Models/Communication/MachineConfigResponse.cs` - Model for Machine module configuration (version, max sections/zones)
- `AgValoniaGPS.Models/Communication/MachineFeedback.cs` - Model for Machine feedback data (work switch, section sensors, timestamp)
- `AgValoniaGPS.Models/Communication/ImuData.cs` - Model for IMU data (roll, pitch, heading, yaw rate, calibration, timestamp)
- `AgValoniaGPS.Models/Communication/ScanResponse.cs` - Model for module scan/discovery responses
- `AgValoniaGPS.Models/Communication/HelloResponse.cs` - Model for hello packet responses with connection monitoring
- `AgValoniaGPS.Models/Communication/TransportConfiguration.cs` - Model for per-module transport configuration with parameters dictionary
- `AgValoniaGPS.Models/Communication/ModuleCapabilities.cs` - Model for capability bitmap decoding with property helpers (V2 protocol, dual antenna, roll compensation)
- `AgValoniaGPS.Models/Communication/FirmwareVersion.cs` - Model for firmware version (major.minor.patch) with ToString override
- `AgValoniaGPS.Models/Events/ModuleConnectedEventArgs.cs` - EventArgs for module connection events
- `AgValoniaGPS.Models/Events/ModuleDisconnectedEventArgs.cs` - EventArgs for module disconnection events
- `AgValoniaGPS.Models/Events/ModuleReadyEventArgs.cs` - EventArgs for module ready state events
- `AgValoniaGPS.Models/Events/PgnMessageReceivedEventArgs.cs` - EventArgs for PGN message reception
- `AgValoniaGPS.Models/Events/TransportStateChangedEventArgs.cs` - EventArgs for transport state changes
- `AgValoniaGPS.Models/Events/TransportDataReceivedEventArgs.cs` - EventArgs for transport data reception
- `AgValoniaGPS.Models/Events/AutoSteerFeedbackEventArgs.cs` - EventArgs for AutoSteer feedback
- `AgValoniaGPS.Models/Events/MachineFeedbackEventArgs.cs` - EventArgs for Machine feedback
- `AgValoniaGPS.Models/Events/ImuDataReceivedEventArgs.cs` - EventArgs for IMU data reception
- `AgValoniaGPS.Models/Events/WorkSwitchChangedEventArgs.cs` - EventArgs for work switch state changes
- `AgValoniaGPS.Models/Events/ImuCalibrationChangedEventArgs.cs` - EventArgs for IMU calibration changes
- `AgValoniaGPS.Models/Events/SimulatorStateChangedEventArgs.cs` - EventArgs for hardware simulator state changes
- `AgValoniaGPS.Services.Tests/Communication/CommunicationModelsTests.cs` - 7 focused tests for domain models validation

### Modified Files
None - all new files created in appropriate directories.

### Deleted Files
None

## Key Implementation Details

### Module Enums
**Location:** `AgValoniaGPS.Models/Communication/`

Created 5 enums following spec requirements:
- **ModuleType**: AutoSteer, Machine, IMU (3 hardware module types)
- **ModuleState**: Disconnected, Connecting, HelloReceived, Ready, Error, Timeout (state machine transitions)
- **TransportType**: UDP, BluetoothSPP, BluetoothBLE, CAN, Radio (5 transport options)
- **BluetoothMode**: ClassicSPP, BLE (Bluetooth variants)
- **RadioType**: LoRa, Spread900MHz, WiFi (radio module types)

**Rationale:** Enums provide type-safe module identification and state tracking. The ModuleState enum supports the connection state machine described in spec (Disconnected → Connecting → HelloReceived → Ready), with Error and Timeout for error handling.

### PGN Response Models
**Location:** `AgValoniaGPS.Models/Communication/`

Created 7 models for PGN message responses:
- **AutoSteerConfigResponse**: Version, Capabilities, MaxSteerAngle, MinSteerAngle
- **AutoSteerFeedback**: ActualWheelAngle, SwitchStates, StatusFlags, Timestamp (for closed-loop control)
- **MachineConfigResponse**: Version, MaxSections, MaxZones
- **MachineFeedback**: WorkSwitchActive, SectionSensors, StatusFlags, Timestamp (for section control)
- **ImuData**: Roll, Pitch, Heading, YawRate, IsCalibrated, Timestamp
- **ScanResponse**: ModuleType, Version, Capabilities (for module discovery)
- **HelloResponse**: ModuleType, Version, ReceivedAt (for keepalive monitoring)

**Rationale:** These models provide strongly-typed representations of PGN message payloads. Timestamps enable timeout detection, feedback models support closed-loop control with Wave 3/4 integration.

### Configuration Models
**Location:** `AgValoniaGPS.Models/Communication/`

Created 3 configuration models:
- **TransportConfiguration**: Module, Type, Parameters (Dictionary<string, string>) for flexible per-module transport settings
- **ModuleCapabilities**: RawCapabilities byte array with property helpers for bitmap decoding (SupportsV2Protocol, SupportsDualAntenna, SupportsRollCompensation)
- **FirmwareVersion**: Major, Minor, Patch with ToString override formatting as "major.minor.patch"

**Rationale:** TransportConfiguration allows per-module transport selection with custom parameters (e.g., Bluetooth device address, CAN adapter path). ModuleCapabilities provides type-safe access to capability bits per spec protocol versioning. FirmwareVersion formats consistently for display.

### Connection Event Args
**Location:** `AgValoniaGPS.Models/Events/`

Created 6 connection-related EventArgs:
- **ModuleConnectedEventArgs**: ModuleType, Transport, Version, ConnectedAt
- **ModuleDisconnectedEventArgs**: ModuleType, Reason, DisconnectedAt
- **ModuleReadyEventArgs**: ModuleType, Capabilities, ReadyAt
- **PgnMessageReceivedEventArgs**: PgnNumber, Data, SourceModule, ReceivedAt
- **TransportStateChangedEventArgs**: Module, Transport, IsConnected, StateMessage
- **TransportDataReceivedEventArgs**: Module, Data, ReceivedAt

**Rationale:** Following Wave 1-5 patterns with readonly fields and DateTime timestamps. These events enable ModuleCoordinatorService to track connection lifecycle and publish state changes to subscribers.

### Module-Specific Event Args
**Location:** `AgValoniaGPS.Models/Events/`

Created 6 module-specific EventArgs:
- **AutoSteerFeedbackEventArgs**: Feedback (AutoSteerFeedback)
- **MachineFeedbackEventArgs**: Feedback (MachineFeedback)
- **ImuDataReceivedEventArgs**: Data (ImuData)
- **WorkSwitchChangedEventArgs**: IsActive, ChangedAt
- **ImuCalibrationChangedEventArgs**: IsCalibrated, ChangedAt
- **SimulatorStateChangedEventArgs**: IsRunning, RealisticBehaviorEnabled

**Rationale:** Module-specific events allow services to subscribe to relevant data updates. Work switch and calibration events provide distinct notifications for important state changes in Wave 4 and IMU integration.

## Database Changes
No database changes required - all models are in-memory domain objects.

## Dependencies
No new dependencies added - all implementations use standard .NET 8 libraries.

### Configuration Changes
None

## Testing

### Test Files Created/Updated
- `AgValoniaGPS.Services.Tests/Communication/CommunicationModelsTests.cs` - 7 focused tests for domain model validation

### Test Coverage
- Unit tests: ✅ Complete (7/7 tests passing)
- Integration tests: N/A (deferred to Task Group 11)
- Edge cases covered:
  - ModuleState enum transitions following expected state machine flow
  - ModuleCapabilities bitmap decoding for each capability bit (V2, dual antenna, roll compensation)
  - ModuleCapabilities multiple capabilities combined (0x07 = all three bits set)
  - FirmwareVersion ToString formatting
  - TransportConfiguration parameter storage and retrieval

### Manual Testing Performed
Built AgValoniaGPS.Models project successfully. Ran focused tests using:
```
dotnet test AgValoniaGPS.Services.Tests/AgValoniaGPS.Services.Tests.csproj --filter "FullyQualifiedName~CommunicationModelsTests"
```

Results: **Test Run Successful. Total tests: 7, Passed: 7, Total time: 1.6247 Seconds**

## User Standards & Preferences Compliance

### NAMING_CONVENTIONS.md
**File Reference:** `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/NAMING_CONVENTIONS.md`

**How Implementation Complies:**
- Used `Communication/` directory (functional area) instead of `Module/` or `Hardware/` as per critical naming rules
- Avoided namespace collision with potential Model classes by using functional area naming
- Followed pattern: `AgValoniaGPS.Models.Communication` for domain models
- Reused existing `Events/` directory following established pattern from Wave 1-5
- EventArgs classes follow naming pattern: `{Entity}{Action}EventArgs`

**Deviations:** None

### agent-os/standards/global/coding-style.md
**File Reference:** `agent-os/standards/global/coding-style.md`

**How Implementation Complies:**
- XML documentation comments on all public types and members
- Readonly fields in EventArgs classes for immutability
- Properties use expression-bodied members where appropriate (ModuleCapabilities)
- Consistent brace style and indentation
- Enum members have XML doc comments explaining their purpose

**Deviations:** None

### agent-os/standards/global/commenting.md
**File Reference:** `agent-os/standards/global/commenting.md`

**How Implementation Complies:**
- Comprehensive XML documentation on all public classes, properties, and fields
- Summary tags explain purpose and usage
- EventArgs constructors document parameters
- Property comments explain units and ranges (e.g., "Roll angle in degrees", "0 = North, 90 = East")
- Comments explain business logic (e.g., capability bitmap bit positions)

**Deviations:** None

### agent-os/standards/global/conventions.md
**File Reference:** `agent-os/standards/global/conventions.md`

**How Implementation Complies:**
- Properties use PascalCase (ModuleType, IsConnected, ReceivedAt)
- Readonly fields in EventArgs use PascalCase
- Enum values use PascalCase (AutoSteer, Disconnected, BluetoothSPP)
- DateTime properties consistently use UTC (DateTime.UtcNow)
- Boolean properties use "Is" prefix (IsActive, IsCalibrated, IsConnected)
- Array properties initialized to Array.Empty<T>() to prevent null reference issues

**Deviations:** None

### agent-os/standards/global/validation.md
**File Reference:** `agent-os/standards/global/validation.md`

**How Implementation Complies:**
- EventArgs constructors validate critical parameters with ArgumentNullException (Version, Capabilities, Feedback, Data)
- ModuleCapabilities safely checks array bounds before accessing RawCapabilities[0]
- Properties defensively initialize collections to empty arrays
- Timestamp properties auto-initialize to DateTime.UtcNow preventing invalid dates

**Deviations:** None - validation in EventArgs constructors aligns with immutability pattern. Model validation deferred to service layer per existing Wave 1-5 pattern.

### Wave 1-5 EventArgs Patterns
**File Reference:** `AgValoniaGPS.Models/Events/ABLineChangedEventArgs.cs`, `SectionStateChangedEventArgs.cs`

**How Implementation Complies:**
- Readonly fields for immutability (e.g., `public readonly ModuleType ModuleType`)
- Constructor validation with ArgumentNullException
- DateTime timestamp fields auto-initialized in constructor
- Inheritance from EventArgs base class
- Followed pattern of including context object in EventArgs (e.g., Feedback, Data)

**Deviations:** None - exactly followed existing Wave patterns.

## Integration Points

### APIs/Endpoints
Not applicable - domain models only, no API endpoints.

### External Services
None

### Internal Dependencies
- **Wave 1-5 Services** will reference these models for hardware communication
- **Task Group 2 (PGN Builder)** will use models to build messages
- **Task Group 3 (PGN Parser)** will populate these models from byte arrays
- **Task Group 6 (Module Coordinator)** will use enums and events for state management
- **Task Group 7 (Module Communication Services)** will publish EventArgs and maintain model state

## Known Issues & Limitations

### Issues
None

### Limitations
1. **ModuleCapabilities bitmap limited to first byte**
   - Description: Only RawCapabilities[0] is currently decoded, additional capability bits 8-15 not yet defined
   - Reason: Spec only defines 3 capability bits (V2 protocol, dual antenna, roll compensation)
   - Future Consideration: Additional property helpers can be added when spec defines more capability bits

## Performance Considerations
- Enum comparisons are O(1)
- ModuleCapabilities property helpers use bitwise operations (very fast, <1ns)
- EventArgs are lightweight value containers with minimal allocation overhead
- FirmwareVersion.ToString() uses string interpolation (efficient for display)

## Security Considerations
- No sensitive data in models
- Validation in EventArgs constructors prevents null reference attacks
- No user input directly mapped to models (service layer will validate)

## Dependencies for Other Tasks
- **Task Group 2 (PGN Builder)**: Depends on AutoSteerConfigResponse, MachineConfigResponse, ImuData models for building PGN messages
- **Task Group 3 (PGN Parser)**: Depends on all PGN response models for parsing incoming messages
- **Task Group 4 (Transport Abstraction)**: Depends on TransportType, ModuleType enums and Transport*EventArgs
- **Task Group 5 (UDP Transport)**: Depends on TransportType, ModuleType enums
- **Task Group 6 (Module Coordinator)**: Depends on ModuleState, ModuleType enums and Module*EventArgs
- **Task Group 7 (Module Communication)**: Depends on all models and events for feedback processing
- **Task Group 9 (Hardware Simulator)**: Depends on all models for simulating hardware responses
- **Task Group 10 (Wave 3/4 Integration)**: Depends on AutoSteerFeedback, MachineFeedback, ImuData for closed-loop control

## Notes
- All 7 tests passed on first run, validating correctness of enum transitions, bitmap decoding, version formatting, and parameter storage
- Implementation strictly follows NAMING_CONVENTIONS.md using `Communication/` directory to avoid namespace collisions
- Models are designed for immutability where appropriate (EventArgs readonly fields) and mutability where needed (PGN response models updated by parsers)
- Capability bitmap decoding uses property helpers for type-safe access while preserving raw byte array for extensibility
- Timestamp properties consistently use UTC to avoid timezone issues in distributed systems
- All XML documentation provides context for hardware communication domain (e.g., "2-second timeout", "PGN number", "roll compensation")
