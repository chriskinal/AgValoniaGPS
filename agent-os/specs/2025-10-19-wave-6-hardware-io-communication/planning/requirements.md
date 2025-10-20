# Spec Requirements: Wave 6 Hardware I/O & Communication

## Initial Description
Wave 6 focuses on completing the hardware communication layer for AgValoniaGPS. This includes PGN (Parameter Group Number) message handling for communication with AgOpenGPS hardware modules (AutoSteer, Machine, IMU), module connection monitoring, and bi-directional communication protocols.

This is part of an 8-wave business logic extraction plan. Waves 1-5 have been completed (Position, Guidance, Steering, Section Control, Field Operations). Some hardware communication already exists: UdpCommunicationService, NmeaParserService. Wave 6 will enhance and complete the hardware I/O layer.

## Requirements Discussion

### First Round Questions

**Q1: Hardware Module Support**
Which hardware modules need to be supported?

**Answer:** All 3 modules:
- AutoSteer module (steering motor control, wheel angle feedback)
- Machine module (section control, switches, relays, work state)
- IMU module (roll, pitch, heading data)

**Q2: Transport Architecture**
What communication transports are needed beyond the existing UDP support?

**Answer:** Multi-transport architecture required:

**Bluetooth Transport:**
- Support BOTH Bluetooth Classic SPP (Serial Port Profile) and BLE (Bluetooth Low Energy)
- User selectable between SPP and BLE
- Same PGN message format as UDP
- Primary use case: Mobile device communication
- No automatic fallback between transports

**CAN Bus Integration:**
- BOTH reading tractor data AND sending commands to tractor
- Support ISOBUS/ISO 11783 standard protocols
- Hybrid mode: Run alongside UDP (not replacing it)
- Hardware: USB CAN adapters like PCAN

**Radio Module Communication:**
- Support ALL types: LoRa, 900MHz spread spectrum, WiFi-based radios
- Expected range: 1 mile
- Target data rate: 200kbps
- Use cases: BOTH base station to vehicle AND vehicle to vehicle communication

**Q3: PGN Message Requirements**
What PGN messages need builder/parser support?

**Answer:** Complete message coverage for all 3 modules:

**AutoSteer Module:**
- Outbound: Steer settings, steer data commands (desired angle, speed)
- Inbound: Steer config responses, actual wheel angle feedback, switch states

**Machine Module:**
- Outbound: Section control commands, relay states, machine settings
- Inbound: Machine config responses, work switch state, section sensor feedback

**IMU Module:**
- Outbound: IMU configuration requests
- Inbound: Roll/pitch/heading data, calibration status

**All Modules:**
- Hello packets (module presence monitoring)
- Scan request/responses (module discovery)

**Q4: Connection Monitoring Strategy**
How should module connection state be tracked and what timeouts are appropriate?

**Answer:** Multi-level monitoring:
- Hello packets: 2-second timeout (existing implementation to be maintained)
- Data flow monitoring: 100ms for Steer/Machine, 300ms for IMU (existing implementation)
- Module "ready" state required before sending commands
- Lazy initialization: Modules initialize on-demand when first needed
- Formal handshake process with PGN-only messages (no firmware version/capabilities in hello)

**Q5: Protocol Versioning & Extensibility**
How should the protocol handle version differences and future extensions?

**Answer:** Extensible design with backward compatibility:

**Version Management:**
- Reserve new PGN numbers for v2 features (keep existing PGNs unchanged)
- Capability negotiation: Modules announce supported features during handshake
- Version field in hello packets to detect firmware versions
- Mixed version environments: Handle gracefully (modules typically on same board)

**Q6: Module Configuration & Initialization**
What initialization and configuration is needed?

**Answer:**
- Modules send PGN-only during initialization (no firmware version/capabilities announcements)
- Formal "ready" state required before application sends commands
- Lazy initialization: Modules initialize on-demand when first accessed
- Configuration persistence handled at application level (not in this wave)

**Q7: Hardware Simulator Requirements**
What simulation capabilities are needed for testing?

**Answer:** Comprehensive simulator requirements:

**Simulator Scope:**
- Support all 3 modules (AutoSteer, Machine, IMU) in one process
- GUI for manual value adjustment (defer detailed UI work to later phase)
- Scriptable interface for automated integration tests
- Realistic behavior simulation (selectable):
  - Gradual steer angle changes (not instant)
  - IMU drift over time
  - GPS jitter and noise
  - Section switch response delays

**Simulator Use Cases:**
- Development testing without hardware
- Automated CI/CD integration tests
- Stress testing (rapid changes, edge cases)
- Multi-transport testing

**Q8: Transport Selection & Configuration**
How should users configure which transport to use per module?

**Answer:**
- Per-module transport selection (e.g., AutoSteer via Bluetooth, Machine via UDP, IMU via CAN)
- Auto-detect where possible with user-configured overrides
- Transport priority: UDP is primary/default
- Mixed transport scenarios supported (different modules on different transports)

**Q9: Out of Scope Items**
What should NOT be included in this wave?

**Answer:**
- AgShare cloud integration (different wave)
- User interface for configuration screens (defer to UI wave)
- Advanced analytics or logging dashboards
- Multi-vehicle coordination
- Hardware-specific device drivers (use abstraction layer)

### Existing Code Reuse

**Similar Features Identified:**

**UDP Communication:**
- Service: `AgValoniaGPS.Services/Communication/UdpCommunicationService.cs`
- Usage: Module connection tracking, PGN message handling, hello packet monitoring
- Reuse Pattern: Extend transport abstraction to support UDP, Bluetooth, CAN, Radio

**NMEA Parsing:**
- Service: `AgValoniaGPS.Services/GPS/NmeaParserService.cs`
- Usage: Binary message parsing patterns
- Reuse Pattern: Similar state machine approach for PGN parsing

**Service Registration:**
- File: `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
- Usage: DI registration for new hardware services
- Reuse Pattern: Follow existing service registration conventions

**Service Architecture:**
- Pattern: Interface-based services with event-driven state changes
- Existing Examples: All Wave 1-5 services (PositionUpdateService, ABLineService, etc.)
- Reuse Pattern: Follow established service patterns with EventArgs for state changes

### Follow-up Questions

**Follow-up 1:** I notice this wave will create significant hardware communication infrastructure. Should we plan integration points with the completed Wave 3 (Steering Algorithms) and Wave 4 (Section Control) services?

**Answer:** Yes, critical integration points:

**Wave 3 Integration (Steering):**
- SteeringCoordinatorService outputs should feed AutoSteer module commands
- Steering angle feedback from hardware should update vehicle state
- Closed-loop control: Compare desired vs actual wheel angle

**Wave 4 Integration (Section Control):**
- Section control decisions should trigger Machine module commands
- Section sensor feedback should update coverage state
- Work switch state should enable/disable section control

**Follow-up 2:** For the hardware simulator, should it support all transport types or just UDP for initial testing?

**Answer:**
- Initial simulator: UDP only (matches existing hardware)
- Future enhancement: Support Bluetooth/CAN simulation
- Priority: Get UDP simulator working with realistic behaviors first
- Transport abstraction should make adding other transports to simulator straightforward later

## Visual Assets

### Files Provided:
No visual assets provided.

### Visual Insights:
No visual design specifications required for this backend-focused wave. The hardware communication layer is service-based without UI components in this phase.

## Requirements Summary

### Functional Requirements

**Core Hardware Communication:**
- Support 3 hardware modules: AutoSteer, Machine, IMU
- Multi-transport architecture: UDP (primary), Bluetooth SPP/BLE, CAN/ISOBUS, Radio (LoRa/900MHz/WiFi)
- Per-module transport selection with auto-detect and manual override
- Transport abstraction layer for pluggable transports

**PGN Message Handling:**
- Builder pattern for creating outbound PGN messages
- Parser for all inbound PGN message types
- Message validation with CRC checking
- Type-safe message construction (not raw byte arrays)

**Module Connection Management:**
- Hello packet monitoring (2-second timeout)
- Data flow monitoring (100ms Steer/Machine, 300ms IMU)
- Module ready state tracking
- Lazy initialization on-demand
- Connection state events for UI updates

**Protocol Versioning:**
- Reserve new PGN numbers for future features
- Capability negotiation during handshake
- Version field in hello packets
- Backward compatibility with older firmware

**Hardware Simulator:**
- Simulate all 3 modules in single process
- Manual control GUI (basic, detailed UI deferred)
- Scriptable interface for automated tests
- Realistic behaviors: gradual changes, drift, jitter, delays (selectable)
- Support for integration testing in CI/CD

**Integration with Previous Waves:**
- Wave 3 Steering: Send steering commands, receive wheel angle feedback
- Wave 4 Section Control: Send section commands, receive sensor feedback
- Closed-loop control with actual vs desired state comparison

### Reusability Opportunities

**Existing Services to Reference:**
- `UdpCommunicationService`: Extend for multi-transport abstraction
- `NmeaParserService`: Reuse parsing patterns for PGN messages
- Service architecture patterns from Waves 1-5
- Event-driven state change patterns (EventArgs)
- DI registration conventions in `ServiceCollectionExtensions.cs`

**Existing Models to Use:**
- PGN message structures (if already defined)
- Module state enums
- Connection status models

**Backend Logic Patterns:**
- Interface-based service design
- Singleton service registration
- Comprehensive unit testing with xUnit/NUnit
- State machine patterns for connection management

### Scope Boundaries

**In Scope:**
- AutoSteer, Machine, IMU module support
- UDP, Bluetooth SPP/BLE, CAN/ISOBUS, Radio transports
- PGN message builders and parsers for all message types
- Module coordinator service with connection monitoring
- Transport abstraction layer
- Protocol versioning with capability negotiation
- Hardware simulator with realistic behaviors
- Integration with Wave 3 (Steering) and Wave 4 (Section Control)
- Comprehensive unit and integration tests

**Out of Scope:**
- AgShare cloud integration (separate wave)
- User interface configuration screens (deferred to UI wave)
- Advanced analytics dashboards
- Telemetry and logging UI
- Multi-vehicle coordination features
- Hardware-specific device drivers (use abstraction)
- Mobile companion app integration
- Configuration persistence (application-level concern, not this wave)

### Technical Considerations

**Transport Architecture:**
- Abstract base transport class/interface
- Pluggable transport implementations
- UDP as primary/default transport
- Mixed transport scenarios (different modules on different transports)
- No automatic transport fallback (user selects explicitly)

**Message Protocol:**
- Binary PGN format (existing AgOpenGPS compatibility)
- CRC validation for message integrity
- Type-safe message construction (builder pattern)
- Extensible for future message types
- Maintain existing PGN numbers for backward compatibility

**Connection Management:**
- Timeout-based connection detection
- Module ready state before sending commands
- Lazy initialization for performance
- Event-driven state changes for UI reactivity
- Graceful handling of connection loss/reconnection

**Bluetooth Specifics:**
- Support BOTH Classic SPP and BLE
- User-selectable (not automatic fallback)
- Same PGN message format as UDP
- Mobile device primary use case

**CAN Bus Specifics:**
- ISOBUS/ISO 11783 standard compliance
- Bidirectional (read tractor data, send commands)
- Hybrid mode alongside UDP
- USB CAN adapter support (e.g., PCAN)

**Radio Module Specifics:**
- Support LoRa, 900MHz spread spectrum, WiFi radios
- 1-mile range target
- 200kbps data rate target
- Base station to vehicle AND vehicle to vehicle

**Hardware Simulator:**
- Single process for all modules
- Realistic behavior modes (selectable)
- Scriptable for CI/CD automated testing
- UDP initially, other transports as future enhancement

**Integration Points:**
- Wave 3 (Steering): SteeringCoordinatorService → AutoSteer commands
- Wave 4 (Section Control): Section decisions → Machine commands
- Closed-loop control with feedback from hardware
- State synchronization between services and hardware

**Testing Requirements:**
- Unit tests for message builders/parsers
- Unit tests for connection management
- Integration tests with hardware simulator
- Transport abstraction testing (mock transports)
- Cross-platform compatibility (Windows, Linux, macOS)
- Performance: Message handling <5ms latency

**Code Standards Compliance:**
- Follow MVVM/MVP patterns (agent-os/standards)
- Interface-based service design
- Comprehensive XML documentation
- Event-driven architecture with EventArgs
- Dependency injection registration
- Error handling with proper exceptions
- Input validation on all external data

**Platform Compatibility:**
- Cross-platform: Windows, Linux, macOS
- .NET 8.0 runtime
- Bluetooth: Platform-specific implementations
- CAN: USB adapter abstraction (no OS-specific drivers)
- Serial: System.IO.Ports available but not primary focus
