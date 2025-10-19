# Spec Requirements: Wave 4 - Section Control

## Initial Description
Wave 4 implements section control functionality for AgValoniaGPS, including:
- Section state management (on/off control)
- Coverage mapping and tracking
- Section speed calculations
- Boundary-aware section control
- Overlap detection and prevention
- Manual and automatic control modes

This is part of the feature extraction roadmap from AgOpenGPS, specifically Wave 4: Section Control which has HIGH complexity and includes:
1. SectionControlService - Section state machine with boundary checking, look-ahead, and manual overrides
2. CoverageMapService - Coverage recording, area calculation, and patch management
3. SectionSpeedService - Individual section speed calculation based on tool width and turning

Dependencies: Waves 1-3 (Position/Kinematics, Guidance Lines, Steering Algorithms) - all complete

## Requirements Discussion

### First Round Questions

**Q1: What's the maximum number of sections the system should support?**
**Answer:** Up to 31 sections (standard in precision agriculture).

**Q2: Should sections have configurable widths, or are they derived from total implement width divided by section count?**
**Answer:** Configurable individual section widths. Total implement width is the sum of all section widths. Each section can have a different width.

**Q3: For coverage mapping, what granularity should we track? (e.g., grid-based cells, triangle strips following vehicle path, etc.)**
**Answer:** Triangle strips following vehicle path, similar to AgOpenGPS. This provides smooth, accurate coverage representation that follows actual vehicle movement rather than discrete grid cells.

**Q4: How should the system handle section state during sharp turns or reversing?**
**Answer:**
- Sharp turns: Calculate individual section speeds using tool width and turning radius. Sections on the inside of the turn move slower, outside sections move faster. Sections stop when speed drops below threshold.
- Reversing: Sections should turn off immediately when reversing is detected. No coverage should be recorded when moving backward.

**Q5: Should section control respect field boundaries automatically, or require manual override?**
**Answer:** Automatic boundary awareness with manual override capability. Sections should automatically turn off when approaching boundaries to prevent overlap/overspray, but operators can manually force sections on/off if needed.

**Q6: What's the primary data structure for storing coverage history? In-memory only, or persist to disk?**
**Answer:** Persist to disk. Coverage data should be saved to field directory and loaded when field is reopened. Use similar text-based format as other field files (e.g., Coverage.txt or SectionData.txt) for consistency with AgOpenGPS field format.

**Q7: For overlap detection, what tolerance should we use? (e.g., 10% of section width, 50cm absolute distance)**
**Answer:** 10% of section width for overlap tolerance. This is configurable per-field since different operations (spraying vs fertilizing) may have different precision requirements.

**Q8: Should the UI show real-time coverage visualization, historical coverage, or both?**
**Answer:** Both. Real-time display shows current section states and actively drawing coverage triangles. Historical mode shows complete coverage map with color-coded overlaps (green = single pass, yellow = 2 passes, red = 3+ passes).

**Q9: Are there any features that should explicitly NOT be included in this wave?**
**Answer:**
- Variable rate application (VRA) - deferred to future wave
- Prescription maps - future wave
- Section control based on elevation/slope - future
- Multi-implement section coordination - future
Focus purely on basic section on/off control, coverage tracking, and boundary awareness.

### Follow-up Questions

**Follow-up 1: What should be the configurable range for section turn-on and turn-off delay timers?**
**Answer:** Range from 1.0 to 15.0 seconds, configurable via a form in the UI. These are significant delays for hydraulic/pneumatic systems that need time to respond.

**Follow-up 2: For section boundary logic, should we turn off a section when the boundary line is anywhere within that section's width, or only when the section's center point crosses the boundary?**
**Answer:** Turn off the section when the boundary is anywhere in the middle of the section (i.e., when the section's coverage area intersects with boundaries). Use look-ahead logic to anticipate boundary crossings rather than reacting after crossing.

**Follow-up 3: Should we include a WorkSwitch/MachineControl service in Wave 4 to track implement engagement state (work switch, hitch position)?**
**Answer:** YES, include in Wave 4. Better named as "Analog Switch State" service. Multiple analog switches exist: work switch, steer switch, lock switch. Other services may need to know or update the state of the hitch-mounted implement.

**Follow-up 4: Should section control state (which sections are on/off) be managed internally within SectionControlService, or shared via a separate state management service that other modules can access?**
**Answer:** Section control should internally manage its state, but make state available to other modules via events/properties. This confirms the pattern from previous waves (services own their state but publish changes via events).

**Follow-up 5: For coverage mapping, should we track coverage for the entire field session, or implement time-windowed coverage (e.g., last 10 minutes) to manage memory?**
**Answer:** Track entire field session. Do not use time-windowing. Persist all coverage data for the session. This allows complete field coverage analysis and record-keeping.

## Existing Code to Reference

**Similar Features Identified:**
- Feature: Position and GPS filtering - Path: `AgValoniaGPS/AgValoniaGPS.Services/GPS/`
  - Pattern: Services with event-driven state changes
  - Example: PositionUpdateService publishes PositionChanged events

- Feature: Guidance line services - Path: `AgValoniaGPS/AgValoniaGPS.Services/Guidance/`
  - Pattern: Service registration in DI container
  - Example: ABLineService, CurveLineService with file I/O separation

- Feature: Vehicle kinematics - Path: `AgValoniaGPS/AgValoniaGPS.Services/Vehicle/`
  - Pattern: Calculation services with configuration
  - Example: VehicleKinematicsService calculates turning dynamics

- Feature: Field file I/O - Path: `AgValoniaGPS/AgValoniaGPS.Services/`
  - Pattern: Text-based file format parsing and serialization
  - Example: BoundaryFileService, FieldPlaneFileService for reading/writing field data

**Architectural Patterns to Follow:**
- Dependency Injection: Register all services in `ServiceCollectionExtensions.cs`
- Event-Driven: Use EventHandler<TEventArgs> for state change notifications
- Interface-based: Define I{ServiceName} interfaces for testability
- Service Organization: Place in `AgValoniaGPS.Services/Section/` directory (follows naming conventions)
- File I/O: Separate file services (e.g., SectionControlFileService, CoverageMapFileService)

## Visual Assets

No visual assets provided.

## Requirements Summary

### Functional Requirements

#### 1. Section Configuration
- Support up to 31 individual sections
- Each section has configurable width (sections can have different widths)
- Total implement width = sum of all section widths
- Section configuration persists with field data

#### 2. Section State Management
- Each section has independent state: Auto, Manual On, Manual Off, Off
- State machine transitions:
  - Auto mode: System controls section based on boundaries, coverage, speed
  - Manual On: Section forced on regardless of automation
  - Manual Off: Section forced off regardless of automation
  - Off: Section inactive (speed below threshold, reversing, etc.)
- Turn-on delay timer: configurable 1.0-15.0 seconds
- Turn-off delay timer: configurable 1.0-15.0 seconds
- Immediate turn-off when reversing (no delay)

#### 3. Section Speed Calculation
- Individual section speed calculated based on:
  - Vehicle speed and heading
  - Tool width and section position relative to vehicle center
  - Turning radius (from vehicle kinematics)
  - Section offset from centerline
- Sections on inside of turn move slower
- Sections on outside of turn move faster
- Section turns off when speed drops below configurable threshold

#### 4. Boundary-Aware Section Control
- Automatic section turn-off when section coverage area intersects field boundary
- Look-ahead anticipation: Turn off section before boundary crossing
- Look-ahead distance configurable (e.g., 2-5 meters)
- Check entire section width, not just center point
- Manual override capability: Operator can force sections on even near boundaries

#### 5. Overlap Detection and Prevention
- Overlap tolerance: 10% of section width (configurable per field)
- Turn off sections when entering previously covered area
- Different operations may require different tolerance settings

#### 6. Coverage Mapping
- Triangle strip representation following vehicle path
- Real-time coverage drawing as vehicle moves
- Persist coverage data to disk in field directory
- File format: Text-based (Coverage.txt) for consistency with AgOpenGPS
- Track complete field session (no time-windowing)
- Area calculation for covered regions
- Overlap detection: Track how many times each area is covered

#### 7. Coverage Visualization
- Real-time mode: Show current section states and actively drawing coverage
- Historical mode: Show complete coverage map with overlap color-coding
  - Green: Single pass (ideal coverage)
  - Yellow: 2 passes (minor overlap)
  - Red: 3+ passes (significant overlap)
- Toggle between real-time and historical views

#### 8. Analog Switch State Management
- Track multiple analog switch states:
  - Work switch: Implement engaged/disengaged
  - Steer switch: Auto-steer enabled/disabled
  - Lock switch: Section control locked/unlocked
- State available to other services via events/properties
- Work switch controls whether sections can activate
- Hitch-mounted implement state affects section control

#### 9. Manual Section Control
- Operator can manually override individual sections
- Manual on: Force section on regardless of automation
- Manual off: Force section off regardless of automation
- Manual overrides persist until operator releases them or conditions change dramatically
- UI clearly indicates manual override state

### Reusability Opportunities

**Service Patterns from Waves 1-3:**
- Event-driven state publishing (PositionUpdateService pattern)
- Interface-based design for testability (all services have interfaces)
- File I/O separation (dedicated FileService classes)
- Dependency injection registration in ServiceCollectionExtensions

**Models and Enums:**
- Leverage existing Position model for vehicle location
- Use VehicleConfiguration for implement dimensions
- Reference Boundary model for boundary intersection checks

**File I/O Patterns:**
- Follow text-based format used by BoundaryFileService, FieldPlaneFileService
- Use same error handling and validation patterns
- Consistent file naming in field directory structure

**Testing Patterns:**
- xUnit test framework (being standardized across codebase)
- AAA pattern (Arrange, Act, Assert)
- Comprehensive test coverage for all services
- Performance benchmarks for critical calculations

### Scope Boundaries

**In Scope:**
- Section state management (auto/manual modes with FSM)
- Section configuration (up to 31 sections, individual widths)
- Turn-on/turn-off delay timers (1.0-15.0 seconds)
- Section speed calculation for turning (individual section speeds)
- Boundary-aware section control with look-ahead anticipation
- Overlap detection and prevention (10% tolerance)
- Coverage mapping with triangle strips following vehicle path
- Coverage persistence to disk (Coverage.txt format)
- Coverage visualization (real-time and historical modes)
- Overlap visualization (color-coded: green/yellow/red)
- Analog switch state service (work switch, steer switch, lock switch)
- Manual section overrides
- Immediate section turn-off when reversing

**Out of Scope (Future Waves):**
- Variable rate application (VRA)
- Prescription maps
- Section control based on elevation/slope
- Multi-implement section coordination
- Advanced analytics and reporting
- Machine learning for section optimization

### Technical Considerations

#### Service Architecture
**Services to Implement:**

1. **SectionControlService**
   - Manages section state machine (Auto, Manual On, Manual Off, Off)
   - Implements turn-on/turn-off delay timers
   - Handles manual overrides
   - Publishes SectionStateChanged events
   - Dependencies: ISectionSpeedService, ICoverageMapService, IAnalogSwitchStateService

2. **CoverageMapService**
   - Records coverage using triangle strips
   - Tracks overlap counts per area
   - Calculates covered area
   - Provides coverage query methods (has area been covered?)
   - Publishes CoverageUpdated events
   - Dependencies: None (leaf service)

3. **SectionSpeedService**
   - Calculates individual section speeds based on vehicle kinematics
   - Accounts for turning radius and section offset
   - Determines if section speed is above/below threshold
   - Publishes SectionSpeedChanged events
   - Dependencies: IVehicleKinematicsService, ISectionConfigurationService

4. **AnalogSwitchStateService**
   - Manages work switch, steer switch, lock switch states
   - Publishes SwitchStateChanged events
   - Provides current state queries
   - Dependencies: None (leaf service)

5. **SectionConfigurationService**
   - Manages section count and individual widths
   - Validates section configuration (max 31 sections)
   - Calculates total implement width
   - Publishes ConfigurationChanged events
   - Dependencies: None (leaf service)

6. **SectionControlFileService**
   - Reads/writes section configuration from/to field directory
   - File format: Text-based for AgOpenGPS compatibility
   - Dependencies: ISectionConfigurationService

7. **CoverageMapFileService**
   - Reads/writes coverage data from/to field directory (Coverage.txt)
   - Serializes triangle strip data
   - Dependencies: ICoverageMapService

#### Models and Enums

**SectionState Enum:**
```
- Auto: System controls section
- ManualOn: Operator forced on
- ManualOff: Operator forced off
- Off: Inactive (reversing, slow speed, etc.)
```

**SectionMode Enum:**
```
- Automatic: Normal operation with boundary/overlap checking
- ManualOverride: Operator has manual control
```

**AnalogSwitchType Enum:**
```
- WorkSwitch: Implement engaged/disengaged
- SteerSwitch: Auto-steer enabled/disabled
- LockSwitch: Section control locked/unlocked
```

**SwitchState Enum:**
```
- Active: Switch is on/engaged
- Inactive: Switch is off/disengaged
```

**Section Model:**
```
- Id: Section identifier (0-30)
- Width: Section width in meters
- State: Current SectionState
- Speed: Current calculated speed
- IsManualOverride: Boolean flag
- TurnOnDelayRemaining: Time remaining on turn-on timer
- TurnOffDelayRemaining: Time remaining on turn-off timer
```

**SectionConfiguration Model:**
```
- SectionCount: Number of sections (1-31)
- SectionWidths: Array of individual widths
- TotalWidth: Computed total implement width
- TurnOnDelay: Seconds (1.0-15.0)
- TurnOffDelay: Seconds (1.0-15.0)
- OverlapTolerance: Percentage of section width (default 10%)
- LookAheadDistance: Meters (default 3.0)
- MinimumSpeed: Speed threshold for section activation
```

**CoverageTriangle Model:**
```
- Vertices: Array of 3 Position points
- SectionId: Which section created this triangle
- Timestamp: When triangle was recorded
- OverlapCount: How many times this area has been covered
```

**CoverageMap Model:**
```
- Triangles: List of CoverageTriangle
- TotalCoveredArea: Calculated area in square meters
- OverlapAreas: Dictionary<int, double> (overlap count -> area)
```

#### Algorithms

**1. Section Boundary Intersection**
```
For each section:
  1. Calculate section's current coverage polygon (based on vehicle position, heading, section offset, section width)
  2. Project section's coverage forward by look-ahead distance
  3. Check if projected coverage intersects field boundary
  4. If intersection detected:
     - Start turn-off timer (if not already started)
  5. If no intersection and timer active:
     - Cancel turn-off timer
```

**2. Look-Ahead Boundary Anticipation**
```
Given:
  - Vehicle position and heading
  - Vehicle speed
  - Section offset from centerline
  - Look-ahead distance (configurable, default 3m)

Calculate:
  1. Future vehicle position = current position + (heading * look-ahead distance)
  2. Future section position = future vehicle position + section offset perpendicular to heading
  3. Future section polygon = rectangle centered on future section position
  4. Check future section polygon intersection with boundary
  5. Return true if intersection predicted
```

**3. Coverage Map Triangle Strip Generation**
```
At each position update:
  For each active section:
    1. Get previous position and current position for this section
    2. Calculate section's left and right edge positions (perpendicular to heading, +/- width/2)
    3. Create two triangles forming a strip:
       - Triangle 1: [prev_left, prev_right, curr_left]
       - Triangle 2: [prev_right, curr_right, curr_left]
    4. Check if triangles overlap existing coverage
    5. Increment overlap count for overlapping areas
    6. Add triangles to coverage map
    7. Update total covered area calculation
```

**4. Section Speed Calculation for Turning**
```
Given:
  - Vehicle speed (m/s)
  - Vehicle heading
  - Turning radius (from VehicleKinematicsService)
  - Section offset from vehicle centerline (meters)

Calculate:
  1. If vehicle moving straight (large turning radius):
     - All sections have same speed as vehicle
  2. If vehicle turning:
     - Turn center = calculate from vehicle position, heading, and turning radius
     - For each section:
       a. Section center offset = section's lateral distance from vehicle centerline
       b. Section radius = turning radius + section center offset
          (positive offset = outside of turn, negative = inside)
       c. Section speed = vehicle speed * (section radius / turning radius)
       d. If section speed < minimum threshold:
          - Mark section for turn-off
  3. Return array of section speeds
```

**5. Overlap Detection**
```
Given:
  - New coverage triangle
  - Existing coverage map
  - Overlap tolerance (percentage of section width)

Check:
  1. Query coverage map for triangles in proximity to new triangle
  2. For each nearby existing triangle:
     a. Calculate intersection area
     b. If intersection area > (section width * overlap tolerance):
        - Increment overlap count
        - Add to overlap areas tracking
     c. Determine overlap severity:
        - 1 pass: No action needed
        - 2 passes: Mark as yellow overlap
        - 3+ passes: Mark as red overlap
  3. Return overlap result and updated coverage map
```

#### Integration Points

**Dependencies on Existing Services:**
- **IVehicleKinematicsService**: Section speed calculations need turning radius
- **IPositionUpdateService**: Coverage tracking requires current vehicle position
- **IGuidanceService**: May influence when sections should be active
- **Boundary models**: Boundary intersection checking

**Services Depending on Section Control:**
- Machine control module (UDP PGN messages for section solenoid activation)
- Display/UI services (real-time section state visualization)
- Data logging services (coverage statistics and reporting)

**File I/O Integration:**
- Field directory structure: `Fields/{FieldName}/Coverage.txt`
- Section configuration: `Fields/{FieldName}/SectionConfig.txt`
- Consistent with existing FieldFileService patterns

#### Performance Requirements

**Critical Performance Targets:**
- Section state updates: <5ms per update cycle
- Coverage triangle generation: <2ms per position update
- Boundary intersection check: <3ms per section
- Overall section control loop: <10ms total (allows 100Hz update rate)

**Memory Considerations:**
- Coverage map size: Expect 10,000-100,000 triangles per field session
- Efficient spatial indexing for overlap queries (consider R-tree or grid-based indexing)
- Lazy loading of historical coverage data when not actively displayed

#### Error Handling

**Failure Scenarios:**
- Invalid section configuration (>31 sections, negative widths)
  - Action: Reject configuration, log error, use previous valid configuration
- File I/O errors (corrupted Coverage.txt)
  - Action: Log error, start fresh coverage map, backup corrupted file
- Boundary data missing
  - Action: Disable automatic boundary-aware control, allow manual mode only
- Service dependency failure (VehicleKinematicsService unavailable)
  - Action: Disable auto mode, allow manual section control only

**Data Validation:**
- Section widths: Must be positive, reasonable range (0.1m - 20m)
- Section count: 1-31 only
- Timer values: 1.0-15.0 seconds
- Overlap tolerance: 0-50% (default 10%)
- Look-ahead distance: 0.5-10 meters

### Test Scenarios and Edge Cases

#### Unit Test Scenarios

**SectionControlService Tests:**
1. State machine transitions
   - Auto → ManualOn → Auto
   - Auto → ManualOff → Auto
   - Auto → Off (reversing) → Auto
2. Turn-on delay timer behavior
   - Timer starts when section should activate
   - Section activates only after timer expires
   - Timer cancels if conditions change before expiry
3. Turn-off delay timer behavior
   - Timer starts when section should deactivate
   - Section deactivates only after timer expires
   - Immediate turn-off when reversing (bypass timer)
4. Manual override precedence
   - Manual on overrides boundary detection
   - Manual off overrides automation
   - Manual override release returns to auto mode

**SectionSpeedService Tests:**
1. Straight-line movement
   - All sections have equal speed (vehicle speed)
2. Left turn
   - Left sections slower than vehicle
   - Right sections faster than vehicle
   - Speed proportional to radius difference
3. Right turn
   - Right sections slower than vehicle
   - Left sections faster than vehicle
4. Sharp turn
   - Inside sections may have zero or negative speed
   - Outside sections compensate with higher speed
5. Speed threshold crossing
   - Section marks for turn-off when below threshold
   - Section ready for turn-on when above threshold

**CoverageMapService Tests:**
1. Triangle strip generation
   - Correct vertices for straight movement
   - Correct vertices during turns
   - No gaps between consecutive triangles
2. Overlap detection
   - Single pass (no overlap)
   - Double pass (yellow overlap)
   - Triple+ pass (red overlap)
3. Area calculation
   - Total covered area accurate
   - Overlap areas tracked separately
4. Coverage queries
   - Point-in-coverage check
   - Area-covered percentage

**AnalogSwitchStateService Tests:**
1. Switch state changes
   - WorkSwitch activation/deactivation
   - SteerSwitch activation/deactivation
   - LockSwitch activation/deactivation
2. Event publication
   - SwitchStateChanged events fire correctly
   - Event data includes switch type and new state

**SectionConfigurationService Tests:**
1. Configuration validation
   - Reject >31 sections
   - Reject negative widths
   - Accept valid configurations (1-31 sections, positive widths)
2. Total width calculation
   - Sum of individual section widths
3. Configuration persistence
   - Save configuration to file
   - Load configuration from file
   - Handle corrupted file gracefully

#### Integration Test Scenarios

**End-to-End Section Control:**
1. Field approach
   - Vehicle approaches field boundary
   - Sections turn off before crossing boundary
   - Turn-off delay respected
2. Field entry
   - Vehicle enters field
   - Sections turn on after delay
   - Turn-on delay respected
3. Coverage overlap
   - Vehicle re-enters covered area
   - Sections turn off to prevent overlap
4. Manual override during automation
   - Operator forces section on near boundary
   - System respects override
   - System returns to auto when override released
5. Reversing behavior
   - All sections turn off immediately when reversing
   - Sections re-enable when moving forward again (with turn-on delay)

**Coverage Map Persistence:**
1. Save coverage during session
   - Coverage data written to Coverage.txt periodically
2. Load coverage on field open
   - Coverage.txt loaded and triangles reconstructed
   - Overlap counts restored
   - Historical visualization shows correct coverage
3. Append to existing coverage
   - New session appends to existing coverage map
   - Previous coverage preserved

#### Edge Cases

**Boundary Edge Cases:**
1. **Curved boundaries**
   - Look-ahead correctly predicts boundary crossing on curves
   - Section polygon intersection accurate for non-linear boundaries
2. **Boundary gaps/holes**
   - Interior boundaries (islands) detected
   - Sections turn off when approaching interior boundaries
3. **Multiple boundary polygons**
   - Outer boundary and inner boundaries (headlands) all respected
4. **Boundary near field start**
   - Sections turn on only when fully inside boundary

**Speed Edge Cases:**
1. **Zero speed (stopped)**
   - All sections turn off
   - Turn-off delay bypassed for zero speed
2. **Very low speed (creeping)**
   - Sections may turn off if below threshold
   - Coverage still recorded if sections manually on
3. **Very high speed**
   - Look-ahead distance may need adjustment
   - Coverage triangles remain valid (no gaps)
4. **Rapid acceleration/deceleration**
   - Section speed calculations stable
   - No oscillation in section states

**Turning Edge Cases:**
1. **Zero radius turn (pivot)**
   - Inside sections have zero speed
   - Outside sections have maximum speed differential
2. **Reversing direction during turn**
   - All sections immediately turn off
   - Coverage stops recording
3. **S-curve (alternating turns)**
   - Section speeds update correctly
   - No lag or overshoot in speed calculations

**Coverage Edge Cases:**
1. **First pass in field**
   - All triangles have overlap count = 1
   - Displayed as green (ideal coverage)
2. **Second pass same path**
   - Triangles have overlap count = 2
   - Displayed as yellow (minor overlap)
3. **Three+ passes same path**
   - Triangles have overlap count >= 3
   - Displayed as red (significant overlap)
4. **Partial section overlap**
   - Some sections overlap, others don't
   - Overlap detection per-section accurate
5. **Gap in coverage (missed strip)**
   - Gap clearly visible in visualization
   - Coverage statistics reflect missed area

**Manual Override Edge Cases:**
1. **Manual on near boundary**
   - Section stays on despite boundary detection
   - Operator responsible for overlap
2. **Manual off in open field**
   - Section stays off despite clear conditions
3. **Switch from manual on to auto near boundary**
   - Section immediately turns off (boundary logic takes over)
4. **All sections manual override**
   - System respects all overrides
   - Auto mode effectively disabled

**File I/O Edge Cases:**
1. **Missing Coverage.txt**
   - Start fresh coverage map
   - No error to user (expected for new fields)
2. **Corrupted Coverage.txt**
   - Log error, backup corrupted file
   - Start fresh coverage map
3. **Very large Coverage.txt (100,000+ triangles)**
   - Efficient loading with progress indicator
   - Consider chunked loading or lazy loading
4. **Concurrent file access**
   - File locking to prevent corruption
   - Retry logic for locked files

**Switch State Edge Cases:**
1. **Work switch off during operation**
   - All sections immediately turn off
   - No coverage recorded
2. **Work switch on in covered area**
   - Sections respect overlap detection
   - Turn off delay applied
3. **Lock switch engaged**
   - All section states frozen
   - No automatic state changes
4. **Multiple switches changing simultaneously**
   - Priority order: Lock > Work > Steer
   - State updates atomic

### Confirmation of Information Completeness

**Confirmed Requirements:**
- ✅ Maximum section count: 31 sections
- ✅ Section width configuration: Individual widths, configurable per section
- ✅ Section state machine: Auto, ManualOn, ManualOff, Off
- ✅ Turn-on/turn-off delays: 1.0-15.0 seconds, configurable
- ✅ Section speed calculation: Individual speeds based on turning radius
- ✅ Boundary awareness: Automatic with look-ahead anticipation
- ✅ Manual override: Per-section manual control available
- ✅ Coverage tracking: Triangle strips following vehicle path
- ✅ Coverage persistence: Text-based file format (Coverage.txt)
- ✅ Overlap detection: 10% section width tolerance (configurable)
- ✅ Overlap visualization: Color-coded (green/yellow/red)
- ✅ Analog switches: Work switch, steer switch, lock switch
- ✅ Reversing behavior: Immediate section turn-off
- ✅ State management: Services own state, publish via events

**Services Architecture Confirmed:**
1. SectionControlService - State machine, timers, manual overrides
2. CoverageMapService - Triangle strips, overlap tracking, area calculation
3. SectionSpeedService - Individual section speed calculations
4. AnalogSwitchStateService - Work/steer/lock switch management
5. SectionConfigurationService - Section count/widths configuration
6. SectionControlFileService - Section configuration file I/O
7. CoverageMapFileService - Coverage data file I/O

**Models/Enums Confirmed:**
- SectionState enum (Auto, ManualOn, ManualOff, Off)
- SectionMode enum (Automatic, ManualOverride)
- AnalogSwitchType enum (WorkSwitch, SteerSwitch, LockSwitch)
- SwitchState enum (Active, Inactive)
- Section model (Id, Width, State, Speed, overrides, timers)
- SectionConfiguration model (count, widths, delays, tolerances)
- CoverageTriangle model (vertices, section ID, timestamp, overlap count)
- CoverageMap model (triangles list, area calculations)

**Key Algorithms Confirmed:**
1. Section boundary intersection checking (full section width, not just center)
2. Look-ahead boundary anticipation (configurable distance, default 3m)
3. Coverage triangle strip generation (two triangles per position update per section)
4. Section speed calculation for turning (radius-based differential speeds)
5. Overlap detection (spatial query with tolerance percentage)

**Integration Points Confirmed:**
- Depends on: VehicleKinematicsService, PositionUpdateService, Boundary models
- Provides to: Machine control (UDP PGN messages), UI visualization, data logging
- File I/O: Fields/{FieldName}/Coverage.txt, SectionConfig.txt

**Performance Targets Confirmed:**
- Section state update: <5ms
- Coverage triangle generation: <2ms
- Boundary check: <3ms per section
- Total loop: <10ms (100Hz capable)

**Scope Boundaries Confirmed:**
- IN: Basic section on/off, coverage tracking, boundary awareness, manual overrides
- OUT: VRA, prescription maps, elevation-based control, multi-implement coordination

**Information Completeness: CONFIRMED**

All necessary requirements have been gathered to create a comprehensive technical specification for Wave 4: Section Control. The requirements cover:
- Functional behavior (what the system does)
- Technical architecture (how it's structured)
- Data models (what information is tracked)
- Algorithms (how calculations are performed)
- Integration points (how it connects to existing systems)
- Performance targets (how fast it must be)
- Test scenarios (how to verify correctness)
- Edge cases (how to handle unusual situations)

The spec-writer has sufficient detail to create implementation specifications for parallel task delegation.
