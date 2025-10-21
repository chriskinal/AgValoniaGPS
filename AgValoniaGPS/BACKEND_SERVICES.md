# AgValoniaGPS Backend Services

This document catalogs all backend services in AgValoniaGPS, organized by functional area and development wave.

## Wave 1: Position & Kinematics (Foundation)

### GPS Services
- **GpsService** - Core GPS data processing and NMEA sentence handling
- **PositionUpdateService** - Real-time position updates with filtering and smoothing
- **NmeaParserService** - Parses NMEA 0183 sentences (GGA, VTG, RMC, HDT, etc.)
- **HeadingCalculatorService** - Computes vehicle heading from GPS positions and IMU data
- **NtripClientService** - NTRIP client for RTK correction data streaming

### Vehicle Services
- **VehicleKinematicsService** - Vehicle kinematics modeling (wheelbase, track, Ackermann steering)

### Communication Foundation
- **UdpCommunicationService** - UDP socket communication on port 9999 for Teensy modules

---

## Wave 2: Guidance Line Core

### Guidance Line Services
- **ABLineService** - A-B straight line guidance (create, edit, switch between lines)
- **CurveLineService** - Curved guidance lines with smooth transitions
- **ContourService** - Contour-based guidance following terrain features
- **GuidanceService** - High-level guidance orchestration and line management

### Guidance File I/O
- **ABLineFileService** - Load/save A-B lines in AgOpenGPS format
- **CurveLineFileService** - Load/save curve lines with point data
- **ContourLineFileService** - Load/save contour lines

---

## Wave 3: Guidance Steering Algorithms

### Steering Algorithm Services
- **LookAheadDistanceService** - Dynamic look-ahead distance calculation based on speed
- **PurePursuitService** - Pure Pursuit steering algorithm for curved paths
- **StanleySteeringService** - Stanley steering algorithm for cross-track error correction
- **SteeringCoordinatorService** - Coordinates steering algorithms and switches between Pure Pursuit/Stanley

---

## Wave 4: Section Control

### Section Management Services
- **AnalogSwitchStateService** - Processes analog switch inputs for section control
- **SectionConfigurationService** - Manages section configuration (width, overlap, positions)
- **SectionSpeedService** - Speed-based section on/off control with delays
- **SectionControlService** - Main section control logic with auto/manual modes
- **CoverageMapService** - Generates coverage maps showing applied/skipped areas

### Section File I/O
- **SectionControlFileService** - Load/save section configuration
- **CoverageMapFileService** - Load/save coverage map data

---

## Wave 5: Field Operations

### Geometric & Spatial Services
- **PointInPolygonService** - Ray-casting algorithm for point-in-polygon tests
- **BoundaryManagementService** - Field boundary management with Douglas-Peucker simplification

### Field Operation Services
- **HeadlandService** - Multi-pass headland generation with parallel offset algorithm
- **UTurnService** - U-turn path generation (Omega/T/Y patterns) with Dubins paths
- **TramLineService** - Parallel tram line generation with proximity detection

### Field & Boundary File I/O
- **FieldService** - Field data management (name, area, boundaries, background images)
- **BoundaryFileService** - Load/save field boundaries (AgOpenGPS, GeoJSON, KML formats)
- **HeadlandFileService** - Load/save headland paths
- **TramLineFileService** - Load/save tram line data
- **FieldPlaneFileService** - Field plane coordinate transformations
- **BackgroundImageFileService** - Load/save georeferenced field background images

---

## Wave 6: Hardware I/O Communication

### PGN Protocol Services
- **PgnMessageBuilderService** - Builds binary PGN messages for module communication
- **PgnMessageParserService** - Parses incoming PGN messages from modules

### Transport Layer Services
- **TransportAbstractionService** - Abstraction layer for multiple transport types
- **UdpTransportService** - UDP transport for module communication
- **BluetoothTransportService** - Bluetooth transport (stub for future implementation)
- **CanBusTransportService** - CAN bus transport (stub for future implementation)
- **RadioTransportService** - Radio transport (stub for future implementation)

### Module Communication Services
- **ModuleCoordinatorService** - Coordinates communication with all hardware modules
- **AutoSteerCommunicationService** - Handles AutoSteer module communication (PGN 250-254)
- **MachineCommunicationService** - Handles Machine module communication (work/steer switches)
- **ImuCommunicationService** - Handles IMU module communication (roll, pitch, yaw data)

### Simulation Services
- **HardwareSimulatorService** - Simulates hardware modules for testing without physical hardware

---

## Wave 7: Display & Visualization

### Display Services
- **DisplayFormatterService** - Formats data for display (units, precision, localization)
- **FieldStatisticsService** - Calculates field statistics (area worked, efficiency, etc.)

---

## Wave 8: State Management

### Configuration Services
- **ConfigurationService** - Application configuration management with dual-format (JSON+XML) persistence
  - Loads v6.x XML settings from legacy AgOpenGPS
  - Saves settings in JSON (primary) and XML (legacy compatibility)
  - Manages vehicle, guidance, section, display, and communication settings

### Session Management Services
- **SessionManagementService** - Manages work sessions with duration tracking and state persistence
  - Tracks active field, vehicle profile, user profile
  - Records session start time, duration, work area
  - Provides session state snapshots

### Profile Management Services
- **ProfileManagementService** - Manages vehicle and user profiles
  - Creates, updates, deletes, switches profiles
  - Validates profile data
  - Provides profile discovery and enumeration

### State Coordination Services
- **StateMediatorService** - Mediates state changes across services with validation and notifications
- **ValidationService** - Validates configuration and state changes
- **CrashRecoveryService** - Auto-save and crash recovery with configurable intervals
- **UndoRedoService** - Undo/redo functionality for reversible operations

### Setup Services
- **SetupWizardService** - First-run setup wizard for initial configuration

---

## Service Count by Wave

| Wave | Services | Description |
|------|----------|-------------|
| Wave 1 | 6 | Position & Kinematics Foundation |
| Wave 2 | 7 | Guidance Line Core |
| Wave 3 | 4 | Steering Algorithms |
| Wave 4 | 7 | Section Control |
| Wave 5 | 11 | Field Operations |
| Wave 6 | 13 | Hardware I/O Communication |
| Wave 7 | 2 | Display & Visualization |
| Wave 8 | 8 | State Management |
| **Total** | **58** | **Complete Backend** |

---

## Service Architecture Patterns

### Dependency Injection
All services are registered in `ServiceCollectionExtensions.cs` using Microsoft.Extensions.DependencyInjection with singleton lifetime.

### Interface-First Design
Every service has a corresponding interface (e.g., `IConfigurationService`, `ISessionManagementService`) for testability and loose coupling.

### Event-Driven Communication
Services use the EventArgs pattern for state change notifications:
- `PositionUpdatedEventArgs` - GPS position changes
- `GuidanceLineChangedEventArgs` - Guidance line switches
- `SectionStateChangedEventArgs` - Section on/off state changes
- `ModuleConnectedEventArgs` - Hardware module connections
- `ConfigurationChangedEventArgs` - Configuration updates
- And many more...

### File Format Support
- **AgOpenGPS v6.x XML** - Legacy format for backward compatibility
- **JSON** - Modern primary format with better structure
- **GeoJSON** - Standard geospatial format for boundaries
- **KML** - Google Earth compatible format
- **Binary PGN** - Efficient hardware communication protocol

### Performance Targets
- **Target Update Rate**: 20-25 Hz (40-50ms cycle time)
- **Guidance calculations**: <5ms (AB line, curve, contour)
- **Section control updates**: <5ms (state calculation + coverage mapping)
- **Point-in-polygon tests**: <1ms (boundary checks)
- **U-turn generation**: <10ms (Omega/T/Y paths)
- **Position updates**: 10 Hz GPS input, interpolated to 20-25 Hz output
- **Steering calculations**: <5ms (Pure Pursuit + Stanley coordinator)

---

## Testing Coverage

All services have comprehensive unit tests in `AgValoniaGPS.Services.Tests/`:
- **Total Tests**: 350+
- **Pass Rate**: 100%
- **Test Pattern**: AAA (Arrange-Act-Assert)
- **Frameworks**: xUnit and NUnit (being standardized to xUnit)
- **Coverage**: All services, integration scenarios, edge cases, error handling

---

## Documentation

Each service includes:
- XML documentation comments for all public methods
- Interface contracts defining behavior
- Implementation reports in `agent-os/specs/` for development waves
- Integration test documentation showing cross-service workflows

---

*Last Updated: 2025-10-21*
*AgValoniaGPS Version: 0.1.0 (Pre-release)*
