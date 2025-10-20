# Task Breakdown: Wave 6 Hardware I/O & Communication

This document breaks down the Wave 6 specification into parallelizable task groups for agent implementation.

## Task Execution Order (Dependency-Based)

```
Layer 1 (No Dependencies):
- Task Group 1: Models & EventArgs

Layer 2 (Depends on Layer 1):
- Task Group 2: PGN Message Builder Service
- Task Group 3: PGN Message Parser Service

Layer 3 (Depends on Layers 1-2):
- Task Group 4: Transport Abstraction Interface
- Task Group 5: UDP Transport Service (enhance existing)

Layer 4 (Depends on Layers 1-3):
- Task Group 6: Module Coordinator Service

Layer 5 (Depends on Layers 1-4):
- Task Group 7: AutoSteer Communication Service
- Task Group 8: Machine Communication Service
- Task Group 9: IMU Communication Service

Layer 6 (Depends on all previous):
- Task Group 10: Hardware Simulator Service
- Task Group 11: Wave 3/4 Integration
- Task Group 12: Bluetooth Transport Service
- Task Group 13: CAN Bus Transport Service
- Task Group 14: Radio Transport Service
```

---

## Implementation Status Legend
- [ ] Not started
- [x] Complete
- [~] In progress
- [!] Blocked (dependencies incomplete)

---

### Task Group 1: Models & EventArgs
**Assigned implementer:** api-engineer
**Dependencies:** None
**Estimated Effort:** 2-3 hours
**Complexity:** Low

Status: COMPLETE (implemented in prior session)

---

### Task Group 2: PGN Message Builder Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 (models)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

Status: COMPLETE (implemented in prior session)

---

### Task Group 3: PGN Message Parser Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 (models)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

Status: COMPLETE (implemented in prior session)

---

### Task Group 4: Transport Abstraction Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 (models)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

Status: COMPLETE (implemented in prior session)

---

### Task Group 5: UDP Transport Service (Enhancement)
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1, 4 (models, transport abstraction)
**Estimated Effort:** 2-3 hours
**Complexity:** Low-Medium

Status: COMPLETE (implemented in prior session)

---

### Task Group 6: Module Coordinator Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1, 2, 3, 4 (models, builder, parser, transport)
**Estimated Effort:** 4-5 hours
**Complexity:** Medium-High

Status: COMPLETE (implemented in prior session)

---

### Task Group 7: AutoSteer Communication Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 2, 3, 4, 6 (builder, parser, transport, coordinator)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

Status: COMPLETE (implemented in prior session)

---

### Task Group 8: Machine Communication Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 2, 3, 4, 6 (builder, parser, transport, coordinator)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

Status: COMPLETE (implemented in prior session)

---

### Task Group 9: Hardware Simulator Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 2, 3, 5 (builder, parser, UDP transport)
**Estimated Effort:** 5-6 hours
**Complexity:** Medium-High

- [x] 9.0 Complete Hardware Simulator Service implementation
  - [x] 9.1 Write 4-6 focused tests for hardware simulator
    - Test simulator start/stop lifecycle
    - Test realistic steering response (gradual angle change, not instant)
    - Test section sensor feedback with delay
    - Test IMU drift over time when realistic behavior enabled
    - Test scriptable scenario execution (load script, verify outputs)
    - Test 10Hz update rate sustained (100ms cycles)
    - Test realistic vs instant mode behavior difference
    - Test performance - update cycle <30ms for all 3 modules
  - [x] 9.2 Create IHardwareSimulatorService interface
    - Lifecycle: Task StartAsync(), Task StopAsync(), void EnableRealisticBehavior(bool enabled)
    - Manual control: SetSteeringAngle(angle), SetSectionSensor(section, active), SetImuRoll(roll), SetImuPitch(pitch)
    - Scripting: LoadScriptAsync(scriptPath), ExecuteScriptAsync()
    - Realism: SetSteeringResponseTime(seconds), SetImuDriftRate(degreesPerMinute), SetGpsJitterMagnitude(meters)
    - Events: EventHandler<SimulatorStateChangedEventArgs> StateChanged
  - [x] 9.3 Implement HardwareSimulatorService class
    - Location: `AgValoniaGPS.Services/Communication/HardwareSimulatorService.cs`
    - Inject: IPgnMessageBuilderService, IPgnMessageParserService
    - Simulate all 3 modules: AutoSteer, Machine, IMU
    - 10Hz update timer (100ms cycle) to send periodic data
  - [x] 9.4 Implement AutoSteer module simulation
    - Receive PGN 254 commands (steering angle, speed, XTE)
    - Simulate gradual steering response: actual angle += (target - actual) * responseRate * dt
    - Response time: Configurable (default 0.5 seconds to target)
    - Send PGN 253 feedback: Actual wheel angle, switch states (random or scripted)
    - Send PGN 126 hello packet every 1 second
  - [x] 9.5 Implement Machine module simulation
    - Receive PGN 239 commands (section states, relay states)
    - Simulate section sensor feedback: Matches commanded state with configurable delay (default 50ms)
    - Send PGN 234 work switch state (toggleable or scripted)
    - Send PGN 123 hello packet every 1 second
  - [x] 9.6 Implement IMU module simulation
    - Receive PGN 218 configuration commands
    - Simulate roll/pitch/heading data: Constant values + drift + noise
    - Drift simulation: Roll/pitch drift at configurable rate (default 0.1 deg/min)
    - GPS jitter: Add random noise to heading (configurable magnitude)
    - Send PGN 219 IMU data every 100ms (10Hz)
    - Send PGN 121 hello packet every 1 second
  - [x] 9.7 Implement realistic behavior modes
    - Realistic mode ON: Gradual steering, sensor delays, IMU drift, GPS jitter
    - Realistic mode OFF: Instant response, no delays, no drift/jitter
    - Configurable parameters: SteeringResponseTime, SectionSensorDelay, ImuDriftRate, GpsJitterMagnitude
  - [x] 9.8 Implement scriptable interface
    - LoadScriptAsync: Parse JSON or text file with timed commands
    - Script format: Timestamp, command type, parameters (e.g., "5.0s: SetSteeringAngle(10.0)")
    - ExecuteScriptAsync: Play back script, execute commands at specified times
    - Use for integration tests (e.g., simulate U-turn scenario)
  - [x] 9.9 Add 10Hz update loop
    - Background timer (Task.Delay-based, 100ms interval)
    - Send hello packets (1Hz for each module)
    - Send data packets (10Hz AutoSteer, 10Hz Machine, 3Hz IMU per spec)
    - Update internal state (steering response, drift accumulation)
    - Stop timer on StopAsync()
  - [x] 9.10 Ensure hardware simulator tests pass
    - Run ONLY the 4-6 tests written in 9.1
    - Verify 10Hz sustained update rate (performance test)
    - Verify realistic behavior (gradual steering, drift)
    - Verify scriptable scenarios work
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 8 tests written in 9.1 pass ✅
- All 3 modules simulated (AutoSteer, Machine, IMU) ✅
- 10Hz update rate sustained (verified with performance test) ✅
- Realistic behavior modes work (gradual steering, drift, jitter) ✅
- Scriptable interface allows automated test scenarios ✅
- Manual control methods work for UI integration ✅
- Performance: <30ms per update cycle for all 3 modules ✅

---

### Task Group 10: Wave 3/4 Integration (Closed-Loop Control)
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 6, 7, 8 (coordinator, module services), Wave 3, Wave 4
**Estimated Effort:** 4-5 hours
**Complexity:** Medium-High

- [x] 10.0 Complete Wave 3/4 Integration for Closed-Loop Control
  - [x] 10.1 Write 6 focused tests for closed-loop integration
    - Test steering closed-loop: SteeringCoordinatorService sends command, AutoSteerCommunicationService receives feedback
    - Test section control closed-loop: SectionControlService sends command, MachineCommunicationService receives feedback
    - Test work switch integration: Machine work switch state affects section control
    - Test module ready state: Commands only sent when module is ready
    - Test feedback error tracking: Log error between desired vs actual wheel angle
    - Test section sensor mismatch: Coverage map updated based on actual section state (not commanded)
  - [x] 10.2 Integrate SteeringCoordinatorService with AutoSteerCommunicationService
    - Subscribe to FeedbackReceived event in SteeringCoordinatorService
    - Send steering command via AutoSteerCommunicationService.SendSteeringCommand()
    - Pass speedMph, steerAngle, xteErrorMm, isActive parameters
    - Track desired vs actual wheel angle for closed-loop control
    - Log error if |desired - actual| > 2.0 degrees
  - [x] 10.3 Integrate SectionControlService with MachineCommunicationService
    - Subscribe to FeedbackReceived event in SectionControlService
    - Send section states via MachineCommunicationService.SendSectionStates()
    - Subscribe to WorkSwitchChanged event
    - Disable all sections when work switch released
    - Enable auto control when work switch pressed
  - [x] 10.4 Add ActualState property to Section model for coverage mapping
    - Add ActualState bool property to Section model
    - Update actual state from section sensor feedback
    - Prevent coverage gaps if section fails to activate
    - Log warning if commanded != actual (mismatch detection)
  - [x] 10.5 Module ready state checks implemented in AutoSteerCommunicationService and MachineCommunicationService
    - Check ModuleCoordinatorService.IsModuleReady() before sending commands
    - Silently drop commands if module not ready
    - ModuleCoordinator handles reconnection if needed
  - [x] 10.6 Register Wave 6 services in DI container
    - Add AddWave6CommunicationServices() method to ServiceCollectionExtensions
    - Register all Wave 6 communication services as Singleton
    - Update Wave 3 and Wave 4 service descriptions to mention Wave 6 integration

**Acceptance Criteria:**
- Integration tests written (6 tests covering all integration scenarios) ✅
- SteeringCoordinatorService sends commands via AutoSteerCommunicationService ✅
- SectionControlService sends commands via MachineCommunicationService ✅
- Feedback events update application state (actual angles, section sensors) ✅
- Work switch integration functional ✅
- Module ready state enforced before sending commands ✅
- Wave 6 services registered in DI container ✅

---

### Task Group 11: Bluetooth Transport Service
**Assigned implementer:** system-engineer
**Dependencies:** Task Groups 1, 4 (models, transport abstraction)
**Estimated Effort:** 6-8 hours (platform-specific)
**Complexity:** High

- [ ] 11.0 Complete Bluetooth Transport Service implementation
  - [ ] 11.1 Write 4-6 focused tests for Bluetooth transport
    - Test Bluetooth SPP connection establishment
    - Test BLE mode connection
    - Test data send/receive over Bluetooth
    - Test disconnection handling
    - Test auto-reconnect (if implemented)
    - Test platform-specific implementations (Windows, Linux, macOS)
  - [ ] 11.2 Create IBluetoothTransportService interface
    - Extends ITransport
    - Additional properties: BluetoothMode (SPP or BLE)
    - Methods: StartAsync(), StopAsync(), Send()
    - Events: DataReceived, ConnectionChanged
  - [ ] 11.3 Implement BluetoothTransportService class
    - Platform detection (Windows: WinRT, Linux: BlueZ, macOS: CoreBluetooth)
    - SPP mode: Classic Bluetooth Serial Port Profile
    - BLE mode: Bluetooth Low Energy GATT characteristics
    - Handle pairing/bonding if required
  - [ ] 11.4 Platform-specific implementations
    - Windows: Use WinRT Bluetooth APIs (Windows.Devices.Bluetooth)
    - Linux: Use BlueZ D-Bus APIs or BluetoothSocket
    - macOS: Use CoreBluetooth framework (may require Xamarin/P/Invoke)
    - Graceful degradation if platform doesn't support Bluetooth
  - [ ] 11.5 Add error handling and reconnection
    - Detect Bluetooth adapter availability
    - Handle connection drops (raise ConnectionChanged event)
    - Auto-reconnect option (configurable)
    - Timeout handling for connection attempts
  - [ ] 11.6 Ensure Bluetooth transport tests pass
    - Run ONLY the 4-6 tests written in 11.1
    - Verify SPP and BLE modes work
    - Verify platform-specific implementations
    - Manual test with real Bluetooth hardware recommended

**Acceptance Criteria:**
- Bluetooth transport tests pass
- SPP mode functional (classic Bluetooth)
- BLE mode functional (Bluetooth Low Energy)
- Platform-specific implementations for Windows, Linux, macOS
- Connection/disconnection events raised correctly
- Send/receive data works reliably

---

### Task Group 12: CAN Bus Transport Service
**Assigned implementer:** system-engineer
**Dependencies:** Task Groups 1, 4 (models, transport abstraction)
**Estimated Effort:** 6-8 hours
**Complexity:** High

- [ ] 12.0 Complete CAN Bus Transport Service implementation
  - [ ] 12.1 Write 4-6 focused tests for CAN transport
    - Test CAN bus connection via USB adapter
    - Test CAN frame send/receive
    - Test ISOBUS protocol integration (if applicable)
    - Test hybrid mode (CAN + UDP simultaneously)
    - Test disconnection handling
    - Test adapter detection (PCAN, SocketCAN, etc.)
  - [ ] 12.2 Create ICanBusTransportService interface
    - Extends ITransport
    - Additional properties: AdapterPath (e.g., /dev/pcanusb0)
    - Methods: StartAsync(), StopAsync(), Send()
    - Events: DataReceived, ConnectionChanged
  - [ ] 12.3 Implement CanBusTransportService class
    - Support common adapters: PCAN-USB, SocketCAN (Linux), CANAL (Windows)
    - CAN frame format: Convert PGN messages to CAN frames
    - CAN arbitration ID: Map PGN to CAN ID (standard or extended)
    - Receive filtering: Accept only relevant CAN IDs
  - [ ] 12.4 ISOBUS integration (optional advanced feature)
    - ISOBUS address claiming (if implementing full ISOBUS stack)
    - Transport Protocol (TP) for multi-frame messages >8 bytes
    - Parameter Group Number (PGN) routing
    - Defer full ISOBUS to future wave if too complex
  - [ ] 12.5 Hybrid mode (CAN + UDP)
    - Allow CAN and UDP transports active simultaneously
    - Route messages based on module configuration
    - Example: IMU via CAN, AutoSteer via UDP
    - Coordinate with TransportAbstractionService for routing
  - [ ] 12.6 Ensure CAN transport tests pass
    - Run ONLY the 4-6 tests written in 12.1
    - Verify CAN adapter detection
    - Verify send/receive works
    - Manual test with real CAN hardware recommended

**Acceptance Criteria:**
- CAN transport tests pass
- CAN USB adapter support functional
- PGN to CAN frame conversion correct
- Hybrid mode (CAN + UDP) works
- Adapter detection and error handling robust

---

### Task Group 13: Radio Transport Service
**Assigned implementer:** system-engineer
**Dependencies:** Task Groups 1, 4 (models, transport abstraction)
**Estimated Effort:** 5-7 hours
**Complexity:** Medium-High

- [ ] 13.0 Complete Radio Transport Service implementation
  - [ ] 13.1 Write 4-6 focused tests for Radio transport
    - Test LoRa module connection
    - Test 900MHz spread spectrum radio
    - Test WiFi-based radio (e.g., ESP32 bridge)
    - Test long-range transmission (mock or real field test)
    - Test packet loss handling
    - Test transmit power adjustment
  - [ ] 13.2 Create IRadioTransportService interface
    - Extends ITransport
    - Additional properties: RadioType (LoRa, 900MHz, WiFi), TransmitPower
    - Methods: StartAsync(), StopAsync(), Send()
    - Events: DataReceived, ConnectionChanged, SignalStrengthChanged
  - [ ] 13.3 Implement RadioTransportService class
    - LoRa: Support LoRa modules via UART/SPI (e.g., RFM95)
    - 900MHz: Support XBee, RFD900 modules
    - WiFi: Support ESP32/ESP8266 bridge mode
    - Serial communication: Use System.IO.Ports.SerialPort
    - Packet framing: Add start/stop markers for reliable parsing
  - [ ] 13.4 Range and power management
    - Configurable transmit power (respect regulatory limits)
    - Signal strength monitoring (RSSI)
    - Adaptive retry logic for packet loss
    - Beacon mode for maintaining link (optional)
  - [ ] 13.5 Ensure Radio transport tests pass
    - Run ONLY the 4-6 tests written in 13.1
    - Verify each radio type works
    - Verify transmit power control
    - Manual field test recommended for range validation

**Acceptance Criteria:**
- Radio transport tests pass
- LoRa, 900MHz, WiFi radio modes functional
- Transmit power control works
- Packet loss handling robust
- Signal strength monitoring works

---

## Summary Statistics

**Total Task Groups:** 13
**Completed:** 10 (Tasks 1-10)
**Remaining:** 3 (Tasks 11-13)

**Total Estimated Effort:** 50-65 hours
**Completed Effort:** ~42-50 hours
**Remaining Effort:** ~8-15 hours

**Complexity Breakdown:**
- Low: 1 task
- Low-Medium: 1 task
- Medium: 5 tasks
- Medium-High: 4 tasks
- High: 2 tasks

**Parallelization Potential:**
- Layer 1-6: Completed (sequential dependencies)
- Tasks 11-13 can run in parallel (no interdependencies)

**Recommended Next Steps:**
1. ✅ Complete Task 10 (Wave 3/4 Integration) - DONE
2. Start Tasks 11-13 (Bluetooth, CAN, Radio) in parallel - system-engineer

---

## Notes

- **Testing Strategy**: Each task group includes 4-6 focused tests. Run ONLY those tests, not the entire suite, to avoid unrelated test failures.
- **Wave 3/4 Integration Complete**: SteeringCoordinatorService and SectionControlService now use AutoSteerCommunicationService and MachineCommunicationService for closed-loop control with hardware feedback.
- **Performance Requirements**: All services must meet latency requirements (<10ms message handling, <30ms simulator update cycle).
- **Cross-Platform**: UDP and Bluetooth prioritized for cross-platform support. CAN and Radio may be platform-specific initially.
