# Specification: Wave 4 - Section Control

## Goal

Implement comprehensive section control functionality for AgValoniaGPS, providing automated and manual section on/off control, real-time coverage mapping with overlap detection, boundary-aware section management, and differential section speed calculations during turning maneuvers. This wave extracts and modernizes section control features from legacy AgOpenGPS into clean, testable, event-driven services.

## User Stories

- As a precision agriculture operator, I want sections to automatically turn off when approaching field boundaries so that I avoid overspray and overlaps
- As an operator, I want to manually override individual sections when needed so that I have full control in special situations
- As an operator, I want to see real-time coverage visualization with color-coded overlap indicators so that I can optimize field coverage and minimize waste
- As an operator, I want section control to respect my implement configuration (section count, widths, delays) so that the system matches my equipment
- As an operator, I want coverage data to persist between sessions so that I can resume work and maintain accurate field records
- As a farm manager, I want accurate coverage area calculations and overlap statistics so that I can optimize operations and reduce input costs

## Core Requirements

### Functional Requirements

**Section Configuration**
- Support 1-31 individual sections with configurable widths
- Each section can have different width (non-uniform implement support)
- Configurable turn-on delay: 1.0-15.0 seconds (hydraulic/pneumatic response time)
- Configurable turn-off delay: 1.0-15.0 seconds
- Configurable overlap tolerance: 0-50% of section width (default 10%)
- Configurable look-ahead distance: 0.5-10 meters (default 3.0m)
- Configuration persists to field directory (SectionConfig.txt)

**Section State Management**
- Four section states: Auto, ManualOn, ManualOff, Off
- Auto mode: System controls based on boundaries, coverage, and speed
- ManualOn: Operator forces section on (overrides automation)
- ManualOff: Operator forces section off (overrides automation)
- Off: Section inactive (reversing, slow speed, work switch off)
- State transitions managed by finite state machine (FSM)
- Immediate turn-off when reversing (no delay)
- Timer-based turn-on/turn-off with cancellation support

**Boundary-Aware Section Control**
- Detect when section coverage area intersects field boundary
- Look-ahead anticipation: predict boundary crossing before it occurs
- Check full section width, not just center point
- Turn off section when boundary is in middle of section coverage area
- Manual override capability for boundary detection

**Coverage Mapping**
- Triangle strip representation following vehicle path
- Two triangles per position update per active section
- Track overlap count for each covered area (1, 2, 3+ passes)
- Calculate total covered area in square meters
- Persist coverage data to Coverage.txt in field directory
- Load existing coverage when field reopened
- Session-wide tracking (no time-windowing)

**Coverage Visualization Data**
- Real-time mode: Current section states and active coverage triangles
- Historical mode: Complete coverage map with overlap color-coding
- Green: Single pass (ideal coverage)
- Yellow: 2 passes (minor overlap)
- Red: 3+ passes (significant overlap)

**Section Speed Calculation**
- Individual section speeds during turns based on turning radius
- Sections on inside of turn move slower than vehicle
- Sections on outside of turn move faster than vehicle
- Speed proportional to section's distance from turn center
- Section turns off when speed drops below threshold

**Analog Switch State Management**
- WorkSwitch: Implement engaged/disengaged (controls whether sections can activate)
- SteerSwitch: Auto-steer enabled/disabled
- LockSwitch: Section control locked/unlocked
- State changes published via events
- Services query switch state for decision making

**Manual Section Control**
- Per-section manual on/off override
- Manual overrides persist until released by operator
- UI clearly indicates manual override state
- Manual override takes precedence over automatic control

### Non-Functional Requirements

**Performance**
- Section state update cycle: <5ms
- Coverage triangle generation: <2ms per position update
- Boundary intersection check: <3ms per section
- Total section control loop: <10ms (supports 100Hz update rate)
- Parallel line generation: Efficient spatial indexing for overlap queries

**Thread Safety**
- All services thread-safe with lock-based synchronization
- Position updates may come from GPS thread
- UI updates occur on UI thread
- Lock objects protect shared state

**Reliability**
- Invalid configuration rejected with clear error messages
- Corrupted files backed up and fresh state initialized
- Missing dependencies handled gracefully (disable auto mode, allow manual)
- Service failures do not crash application

**Data Integrity**
- Configuration validation: section count 1-31, positive widths, valid ranges
- File I/O with error handling and validation
- Atomic state updates
- Coverage data consistency across save/load cycles

## Visual Design

No mockups provided. Section control integrates with existing UI patterns:
- Section control bar showing individual section states
- Coverage map overlay on field display
- Manual override buttons per section
- Configuration dialog for section setup

## Reusable Components

### Existing Code to Leverage

**Service Patterns (Waves 1-3)**
- Event-driven state publishing: `PositionUpdateService.PositionUpdated` event pattern
- Thread-safe implementation: Lock objects in `PositionUpdateService`
- Interface-based design: All services have I{ServiceName} interfaces
- Dependency injection: Registration in `ServiceCollectionExtensions.cs`
- Performance optimization: <1ms calculation targets in `VehicleKinematicsService`

**File I/O Patterns**
- Text-based file format: `BoundaryFileService`, `FieldPlaneFileService`
- Field directory structure: Fields/{FieldName}/*.txt
- Error handling and validation in file services
- Separation of concerns: dedicated FileService classes

**Event Models**
- EventArgs pattern: `ABLineChangedEventArgs` with ChangeType enum
- Timestamp tracking in event args
- Readonly fields in event args
- Null checks in constructors

**Mathematical Calculations**
- Distance calculations: Pythagorean distance in `VehicleKinematicsService`
- Heading calculations: Atan2-based in multiple services
- Coordinate transformations: Sin/Cos projections in kinematics
- Look-ahead calculations: Speed-based projection in `VehicleKinematicsService.CalculateLookAheadPosition`

**Models and Enums**
- Position models: `Position`, `Position2D`, `Position3D`, `GeoCoord`
- VehicleConfiguration: Existing model for vehicle dimensions
- Boundary models: Existing for boundary intersection checks

### New Components Required

**Section-Specific Models**
- Section state tracking model (not in existing codebase)
- Coverage triangle model (triangle strip storage)
- Coverage map data structure (spatial indexing for overlap queries)
- Section configuration model (section counts, widths, timers)

**Section-Specific Services**
- State machine for section control (FSM not present in existing code)
- Coverage map service (triangle strip tracking new to AgValoniaGPS)
- Section speed calculation (differential speeds during turning)
- Analog switch state service (multi-switch management)

**Why New Code Is Needed**
- Section control FSM is complex domain logic not present in position/guidance services
- Coverage mapping requires spatial data structures optimized for overlap detection
- Section speed calculations depend on both kinematics and section configuration
- Analog switch management is specific to machine control domain

## Technical Approach

### Database

Not applicable - AgValoniaGPS uses file-based persistence (text files in field directory).

### API

Not applicable - AgValoniaGPS is desktop application. Integration via:
- UDP communication for machine control (section on/off commands via PGN messages)
- Service events for internal communication
- File I/O for persistence

### Frontend

- Avalonia UI components for section control bar
- OpenGL rendering for coverage map visualization
- ViewModels subscribe to service events for real-time updates
- Manual override buttons bound to section state commands

### File Structure

```
AgValoniaGPS.Models/
├── Section/
│   ├── Section.cs                    # Section state model
│   ├── SectionConfiguration.cs       # Configuration model
│   ├── CoverageTriangle.cs           # Triangle strip model
│   ├── CoverageMap.cs                # Coverage map data structure
│   ├── SectionState.cs               # Enum: Auto, ManualOn, ManualOff, Off
│   ├── AnalogSwitchType.cs           # Enum: WorkSwitch, SteerSwitch, LockSwitch
│   └── SwitchState.cs                # Enum: Active, Inactive
└── Events/
    ├── SectionStateChangedEventArgs.cs
    ├── CoverageUpdatedEventArgs.cs
    ├── SectionSpeedChangedEventArgs.cs
    └── SwitchStateChangedEventArgs.cs

AgValoniaGPS.Services/
├── Section/
│   ├── SectionControlService.cs           # Main section state machine
│   ├── ISectionControlService.cs
│   ├── CoverageMapService.cs              # Coverage tracking
│   ├── ICoverageMapService.cs
│   ├── SectionSpeedService.cs             # Section speed calculations
│   ├── ISectionSpeedService.cs
│   ├── AnalogSwitchStateService.cs        # Switch state management
│   ├── IAnalogSwitchStateService.cs
│   ├── SectionConfigurationService.cs     # Configuration management
│   ├── ISectionConfigurationService.cs
│   ├── SectionControlFileService.cs       # Configuration file I/O
│   ├── ISectionControlFileService.cs
│   ├── CoverageMapFileService.cs          # Coverage data file I/O
│   └── ICoverageMapFileService.cs

AgValoniaGPS.Services.Tests/
└── Section/
    ├── SectionControlServiceTests.cs
    ├── CoverageMapServiceTests.cs
    ├── SectionSpeedServiceTests.cs
    ├── AnalogSwitchStateServiceTests.cs
    ├── SectionConfigurationServiceTests.cs
    ├── SectionControlFileServiceTests.cs
    ├── CoverageMapFileServiceTests.cs
    └── SectionControlIntegrationTests.cs
```

### Service Architecture

**SectionControlService** (Core FSM)
- Dependencies: ISectionSpeedService, ICoverageMapService, IAnalogSwitchStateService, ISectionConfigurationService, IPositionUpdateService, IBoundaryService
- Responsibilities: State machine transitions, timer management, boundary checking, manual override handling
- Events: SectionStateChanged (per section state transitions)
- Performance: <5ms per update cycle for all sections

**CoverageMapService** (Triangle Strip Tracking)
- Dependencies: None (leaf service)
- Responsibilities: Triangle generation, overlap detection, area calculation, spatial queries
- Events: CoverageUpdated (when triangles added)
- Performance: <2ms per position update
- Data Structure: Spatial index (R-tree or grid-based) for efficient overlap queries

**SectionSpeedService** (Differential Speed Calculation)
- Dependencies: IVehicleKinematicsService, ISectionConfigurationService, IPositionUpdateService
- Responsibilities: Calculate individual section speeds based on turning radius and section offset
- Events: SectionSpeedChanged (when speeds update)
- Performance: <1ms for all sections

**AnalogSwitchStateService** (Switch Management)
- Dependencies: None (leaf service)
- Responsibilities: Track work/steer/lock switch states, publish state changes
- Events: SwitchStateChanged (when any switch changes)
- Performance: Negligible (simple state tracking)

**SectionConfigurationService** (Configuration Management)
- Dependencies: None (leaf service)
- Responsibilities: Validate and manage section configuration, calculate total width
- Events: ConfigurationChanged (when configuration updated)
- Performance: Negligible (validation logic)

**SectionControlFileService** (Configuration I/O)
- Dependencies: ISectionConfigurationService
- Responsibilities: Read/write SectionConfig.txt in AgOpenGPS-compatible format
- Performance: File I/O on background thread

**CoverageMapFileService** (Coverage Persistence)
- Dependencies: ICoverageMapService
- Responsibilities: Read/write Coverage.txt, serialize triangle data
- Performance: File I/O on background thread, chunked loading for large files

### Integration Points

**Service Dependencies**
- IVehicleKinematicsService: Provides turning radius for section speed calculations
- IPositionUpdateService: Provides current position, heading, speed for coverage and state updates
- IBoundaryService: Provides boundary polygons for intersection checking
- IUdpCommunicationService: Sends section on/off PGN messages to machine control

**Event Flow**
1. PositionUpdateService.PositionUpdated → SectionControlService updates section states
2. SectionControlService.SectionStateChanged → UI updates section display
3. SectionControlService → CoverageMapService records coverage when sections on
4. CoverageMapService.CoverageUpdated → UI updates coverage visualization
5. AnalogSwitchStateService.SwitchStateChanged → SectionControlService adjusts control logic

**File I/O**
- Field directory: `Fields/{FieldName}/`
- SectionConfig.txt: Section count, widths, delays, tolerances
- Coverage.txt: Triangle vertices, section IDs, overlap counts, timestamp

### Algorithms

**1. Section Boundary Intersection Check**

```
For each section:
  1. Calculate section's current coverage polygon:
     - Center position = tool position + section lateral offset
     - Left edge = center - (section width / 2) perpendicular to heading
     - Right edge = center + (section width / 2) perpendicular to heading
     - Front edge = current position
     - Back edge = previous position

  2. Project section coverage forward by look-ahead distance:
     - Future center = current center + (heading vector * look-ahead distance)
     - Future coverage polygon using future center

  3. Check intersection with boundary polygons:
     - Use polygon-polygon intersection algorithm
     - Check all boundary polygons (outer + inner/headland)

  4. If intersection detected:
     - If not already started, start turn-off timer
     - Section state: Auto → Off (after timer expires)

  5. If no intersection and turn-off timer active:
     - Cancel turn-off timer
     - Section remains on
```

**2. Coverage Triangle Strip Generation**

```
At each position update (for each active section):

  1. Get current and previous section edge positions:
     - Current left edge = current tool position + left offset perpendicular to heading
     - Current right edge = current tool position + right offset perpendicular to heading
     - Previous left edge = previous tool position + left offset perpendicular to previous heading
     - Previous right edge = previous tool position + right offset perpendicular to previous heading

  2. Create two triangles forming a quad strip:
     - Triangle 1: [prev_left, current_left, prev_right]
     - Triangle 2: [current_left, current_right, prev_right]

  3. Check overlap with existing coverage:
     - Query spatial index for triangles near new triangles
     - For each nearby triangle:
       a. Calculate intersection area
       b. If intersection area > (section width * overlap tolerance):
          - Increment overlap count for that area

  4. Add triangles to coverage map:
     - Insert into spatial index
     - Store section ID, timestamp, initial overlap count = 1

  5. Update coverage statistics:
     - Recalculate total covered area
     - Update overlap area buckets (single, double, triple+)
```

**3. Section Speed Calculation**

```
Given:
  - Vehicle speed (m/s)
  - Vehicle heading (radians)
  - Turning radius from VehicleKinematicsService (meters, positive = right turn, negative = left turn)
  - Section configuration: section offsets from centerline (meters, negative = left side, positive = right side)

Calculate for each section:

  1. If turning radius is very large (> 1000m) or vehicle speed < 0.1 m/s:
     - All sections have same speed as vehicle (straight-line movement)
     - Return early

  2. Calculate turn center position:
     - Turn center offset = perpendicular to heading at distance = turning radius
     - Turn center = vehicle position + perpendicular offset

  3. For each section:
     a. Section center offset = section's lateral distance from vehicle centerline (meters)
        - Example: section at -2.0m is 2 meters to the left

     b. Section turning radius = |turning radius| + section center offset
        - If turning right (positive radius):
          - Left sections (negative offset): smaller radius, slower speed
          - Right sections (positive offset): larger radius, faster speed
        - If turning left (negative radius):
          - Right sections (positive offset): smaller radius, slower speed
          - Left sections (negative offset): larger radius, faster speed

     c. Section speed = vehicle speed * (section turning radius / |turning radius|)
        - Clamp to 0 if section radius becomes negative (tight inside turn)

     d. If section speed < minimum threshold (e.g., 0.1 m/s):
        - Mark section for turn-off

  4. Return array of section speeds
```

**4. Look-Ahead Boundary Anticipation**

```
Given:
  - Current position and heading
  - Vehicle speed
  - Section offset and width
  - Look-ahead distance (configurable, default 3.0m)

Calculate:

  1. Project vehicle position forward:
     - Future position = current position + (heading vector * look-ahead distance)

  2. Calculate future section position:
     - Future section center = future position + section lateral offset perpendicular to heading

  3. Create future section coverage rectangle:
     - Future left edge = future center - (section width / 2) perpendicular
     - Future right edge = future center + (section width / 2) perpendicular
     - Future front edge = future center + small forward projection (0.5m)
     - Future back edge = future center - small backward projection (0.5m)

  4. Check rectangle intersection with boundary:
     - Use polygon intersection algorithm
     - Return true if boundary intersects future coverage area
```

**5. Overlap Detection**

```
Given:
  - New coverage triangle
  - Existing coverage map with spatial index
  - Overlap tolerance percentage (default 10%)

Process:

  1. Calculate overlap threshold:
     - Threshold area = triangle area * (overlap tolerance / 100)

  2. Query spatial index for nearby triangles:
     - Bounding box query around new triangle
     - Returns candidate triangles for intersection testing

  3. For each candidate triangle:
     a. Calculate triangle-triangle intersection area using Sutherland-Hodgman algorithm

     b. If intersection area > threshold:
        - Increment overlap count for existing triangle
        - Mark new triangle with overlap count = existing overlap count + 1

     c. Track overlap statistics:
        - Single pass: overlap count = 1
        - Double pass: overlap count = 2
        - Triple+ pass: overlap count >= 3

  4. Insert new triangle into spatial index

  5. Return overlap result: NoOverlap, MinorOverlap (2 passes), SignificantOverlap (3+ passes)
```

### Testing

**Unit Test Coverage (40+ scenarios)**

SectionControlService:
- State transitions: Auto ↔ ManualOn, Auto ↔ ManualOff, Auto ↔ Off
- Turn-on timer: starts when section should activate, expires after configured delay, cancels if conditions change
- Turn-off timer: starts when section should deactivate, expires after delay, bypassed when reversing
- Manual override: ManualOn overrides boundary detection, ManualOff overrides automation, release returns to Auto
- Boundary detection: section turns off when boundary in coverage area, look-ahead anticipation working
- Work switch integration: sections turn off when work switch inactive

CoverageMapService:
- Triangle generation: correct vertices for straight movement, correct during turns, no gaps between consecutive triangles
- Overlap detection: single pass (no overlap), double pass (yellow), triple+ pass (red)
- Area calculation: total area accurate, overlap areas tracked separately
- Spatial queries: point-in-coverage check, area-covered percentage
- Performance: <2ms for triangle generation and overlap check

SectionSpeedService:
- Straight-line: all sections equal vehicle speed
- Left turn: left sections slower, right sections faster, proportional to radius
- Right turn: right sections slower, left sections faster
- Sharp turn: inside sections zero/negative speed (turned off)
- Threshold crossing: section marked for turn-off when below threshold

AnalogSwitchStateService:
- State changes: WorkSwitch, SteerSwitch, LockSwitch activation/deactivation
- Event publication: correct event args, timestamp accurate

SectionConfigurationService:
- Validation: reject >31 sections, reject negative widths, accept valid configurations
- Total width calculation: sum of section widths
- Persistence: save/load configuration file

**Integration Test Coverage (5 scenarios)**

End-to-End Section Control:
1. Field approach: sections turn off before boundary crossing, delay respected
2. Field entry: sections turn on after delay
3. Coverage overlap: sections turn off when re-entering covered area
4. Manual override during automation: operator forces section on, system respects override
5. Reversing behavior: all sections turn off immediately, re-enable when forward (with delay)

**Edge Case Coverage (10 categories)**

Boundary edge cases: curved boundaries, boundary gaps/holes, multiple polygons, boundary near field start
Speed edge cases: zero speed, very low speed, very high speed, rapid acceleration/deceleration
Turning edge cases: zero radius turn (pivot), reversing during turn, S-curve
Coverage edge cases: first pass, second pass same path, three+ passes, partial overlap, gap in coverage
Manual override edge cases: manual on near boundary, manual off in open field, switch to auto near boundary
File I/O edge cases: missing Coverage.txt, corrupted file, very large file (100k+ triangles), concurrent access
Switch state edge cases: work switch off during operation, work switch on in covered area, lock switch engaged, multiple switches changing simultaneously

**Performance Benchmarks**

All services must meet performance targets:
- Section state update: <5ms for 31 sections
- Coverage triangle generation: <2ms
- Boundary intersection check: <3ms per section
- Total loop: <10ms (100Hz capable)

Benchmarks run on each build, fail if thresholds exceeded.

## Out of Scope

**Future Enhancements (Not Wave 4)**
- Variable rate application (VRA) - deferred to future wave
- Prescription maps - future wave
- Section control based on elevation/slope - future
- Multi-implement section coordination - future
- Advanced analytics and reporting - future
- Machine learning for section optimization - future
- Integration with cloud-based field records - future
- Mobile app for remote monitoring - future

**Explicit Exclusions**
- No GUI implementation in this wave (ViewModels only)
- No OpenGL rendering code (data structure for visualization only)
- No UDP PGN message formatting (rely on existing UdpCommunicationService)
- No GPS hardware integration (rely on existing PositionUpdateService)

## Success Criteria

**Functional Success**
- All 7 services implemented with full test coverage
- 100% test pass rate (all 40+ unit tests, 5 integration tests)
- All edge cases handled gracefully
- Configuration validation prevents invalid states
- File I/O compatible with AgOpenGPS format

**Performance Success**
- All performance benchmarks met (<10ms total loop)
- No performance regression in existing services
- Memory usage reasonable for 100k+ triangle coverage maps

**Code Quality Success**
- All services follow established patterns from Waves 1-3
- XML documentation on all public APIs
- No namespace collisions (NAMING_CONVENTIONS.md followed)
- Dependency injection properly configured
- Thread-safe implementations verified

**Integration Success**
- Events published correctly, subscribers receive timely updates
- Service dependencies resolved via DI container
- File I/O does not block main thread
- Coverage data persists and loads correctly across sessions

**User Experience Success**
- Sections respond to boundary crossings with appropriate look-ahead
- Manual overrides work as expected
- Coverage visualization data accurate (green/yellow/red overlap indicators)
- Configuration changes take effect immediately
- No crashes or data loss under any test scenario
