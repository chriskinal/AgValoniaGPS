# Task Breakdown: Wave 5 - Field Operations

## Overview
Total Tasks: 6 major task groups with 30 sub-tasks
Assigned roles: api-engineer, testing-engineer
Estimated Total Effort: 20-28 hours

## Context
Wave 5 implements field boundary management, headland processing, automated turn generation, and tram line services. This wave extracts ~2,500 LOC of field operations logic from AgOpenGPS legacy codebase and provides the foundation for field-based operations in the precision agriculture workflow.

**Dependencies:**
- Wave 1 Complete: PositionUpdateService, VehicleKinematicsService
- Wave 2 Complete: ABLineService, CurveLineService, ContourService
- Wave 3 Complete: SteeringCoordinatorService, PurePursuitService, StanleySteeringService
- Wave 4 Complete: SectionControlService, SectionSpeedService, AnalogSwitchStateService
- Existing Infrastructure: UdpCommunicationService, VehicleConfiguration

**Critical Constraints:**
- Services organized in flat `AgValoniaGPS.Services/FieldOperations/` directory per NAMING_CONVENTIONS.md
- Point-in-polygon checks: <1ms per check at 10Hz
- All geometric operations use UTM coordinates internally
- File I/O supports 3 formats: AgOpenGPS .txt, GeoJSON, KML
- EventArgs pattern for all state changes with validation
- Thread-safe for concurrent operations

**Key Architectural Decisions:**
1. PointInPolygonService is foundational - built first for use by all other services
2. BoundaryManagementService provides core boundary operations for HeadlandService
3. UTurnService integrates with Wave 3 SteeringCoordinatorService
4. TramLineService integrates with Wave 2 ABLineService
5. All services subscribe to Wave 1 PositionUpdateService for real-time GPS updates

## Task List

### Task Group 1: Point-in-Polygon Service (Foundation)
**Assigned implementer:** api-engineer
**Dependencies:** None (foundational service)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

- [ ] 1.0 Complete PointInPolygonService implementation
  - [ ] 1.1 Write 6-8 focused tests for point-in-polygon algorithms
    - Test ray-casting algorithm basic cases (point inside, outside, on edge)
    - Test point on polygon vertex handling
    - Test horizontal edge handling (ray parallel to edge)
    - Test complex polygon shapes (concave, star-shaped)
    - Test polygon with holes (inner boundaries)
    - Test R-tree spatial index build and query
    - Test performance benchmark (<1ms per check)
    - Test multi-polygon support (non-contiguous fields)
  - [ ] 1.2 Create IPointInPolygonService interface
    - Method: `IsPointInPolygon(Position3D point, Boundary boundary)`
    - Method: `IsPointInPolygon(Position3D point, List<Position3D> polygon)`
    - Method: `IsPointInMultiPolygon(Position3D point, List<List<Position3D>> polygons)`
    - Method: `BuildSpatialIndex(Boundary boundary)`
    - Method: `ClearSpatialIndex()`
    - Property: `double GetLastCheckDurationMs()`
    - Property: `long GetTotalChecksPerformed()`
  - [ ] 1.3 Implement PointInPolygonService class
    - Implement ray-casting algorithm (cast horizontal ray, count intersections)
    - Handle edge cases: point on edge (return true), point on vertex, horizontal edges
    - Implement R-tree spatial indexing for polygons >500 vertices
    - Add bounding box pre-check for performance (O(1) rejection)
    - Thread-safe for concurrent queries
    - Track performance metrics (last duration, total checks)
  - [ ] 1.4 Implement R-tree spatial indexing
    - Build hierarchical tree of minimum bounding rectangles (MBRs)
    - Store polygon segments in leaf nodes
    - Query optimization: check only relevant segments
    - Use threshold: build index only for polygons >500 points
  - [ ] 1.5 Add performance monitoring
    - Track per-check duration using Stopwatch
    - Track total checks performed (thread-safe counter)
    - Expose metrics via interface properties
  - [ ] 1.6 Ensure point-in-polygon tests pass
    - Run ONLY the 6-8 tests written in 1.1
    - Verify ray-casting accuracy across all test cases
    - Verify R-tree performance improvement for large polygons
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 6-8 tests written in 1.1 pass
- Ray-casting algorithm accurate for all polygon types
- R-tree index improves query performance for large polygons
- Performance: <1ms per check verified by benchmark test
- Thread-safe for concurrent access
- Support for polygons with holes and multi-polygons

---

### Task Group 2: Boundary Management Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 (PointInPolygonService)
**Estimated Effort:** 5-6 hours
**Complexity:** High

- [ ] 2.0 Complete BoundaryManagementService implementation
  - [ ] 2.1 Write 6-8 focused tests for boundary operations
    - Test boundary recording in time-based mode (1 point/second)
    - Test boundary recording in distance-based mode (1 point/meter)
    - Test automatic simplification during recording (Douglas-Peucker)
    - Test manual simplification after recording
    - Test area calculation accuracy (Shoelace formula against known polygons)
    - Test boundary validation (min points, self-intersection, min area)
    - Test distance calculations (nearest edge, perpendicular distance)
    - Test file I/O (AgOpenGPS .txt, GeoJSON, KML formats)
  - [ ] 2.2 Create domain models in AgValoniaGPS.Models/FieldOperations/
    - Enum: `BoundaryRecordingMode` (TimeBased, DistanceBased)
    - Enum: `BoundaryRecordingState` (Idle, Recording, Paused, Completed)
    - Enum: `SimplificationMode` (Automatic, Manual)
    - Enum: `BoundaryFileFormat` (AgOpenGPSTxt, GeoJSON, KML)
    - Class: `BoundaryRecordingConfiguration` (mode, intervals, simplification settings)
    - Class: `BoundaryValidationResult` (isValid, warnings, errors, metrics)
  - [ ] 2.3 Create EventArgs in AgValoniaGPS.Models/Events/
    - `BoundaryRecordingStartedEventArgs` (config, timestamp)
    - `BoundaryPointAddedEventArgs` (point, totalPoints, area, perimeter, timestamp)
    - `BoundaryRecordingCompletedEventArgs` (boundary, validationResult, timestamp)
    - `BoundaryValidationEventArgs` (validationResult, timestamp)
    - `BoundaryAreaUpdatedEventArgs` (area, perimeter, pointCount, timestamp)
    - All with readonly fields, UTC timestamps, validation
  - [ ] 2.4 Create IBoundaryManagementService interface
    - Recording control: `StartRecording()`, `PauseRecording()`, `ResumeRecording()`, `StopRecording()`
    - State queries: `GetRecordingState()`, `GetCurrentBoundary()`, `GetCurrentArea()`, `GetPointCount()`
    - Simplification: `SimplifyBoundary(tolerance)`, `GetSimplifiedBoundary(boundary, tolerance)`
    - Validation: `ValidateBoundary(boundary)`
    - Distance calculations: `DistanceToNearestEdge()`, `DistanceToSegment()`, `PerpendicularDistance()`
    - File I/O: `LoadBoundary(directory, format)`, `SaveBoundary(boundary, directory, format)`, `ExportBoundary()`
    - Events: `RecordingStarted`, `PointAdded`, `RecordingCompleted`, `ValidationCompleted`, `AreaUpdated`
  - [ ] 2.5 Implement BoundaryManagementService class
    - Subscribe to `PositionUpdateService.PositionUpdated` for real-time recording
    - Implement time-based recording (capture point every X seconds)
    - Implement distance-based recording (capture point every X meters)
    - Real-time area calculation using Shoelace formula during recording
    - Automatic simplification using Douglas-Peucker if enabled
  - [ ] 2.6 Implement Douglas-Peucker simplification algorithm
    - Recursive algorithm: find max perpendicular distance point
    - Split at max point if distance > tolerance
    - Time complexity: O(n log n) average case
    - Configurable tolerance in meters (default 0.5m)
  - [ ] 2.7 Implement Shoelace formula for area calculation
    - Formula: Area = 0.5 * |Σ(xi * yi+1 - xi+1 * yi)|
    - Works for any non-self-intersecting polygon
    - Efficient: O(n) time, O(1) space
    - Use UTM coordinates (meters) for accurate area
  - [ ] 2.8 Implement boundary validation
    - Minimum 3 points required
    - Minimum area 100m² (warn if smaller)
    - Self-intersection detection (warn but allow with tolerance)
    - Maximum point spacing validation (gap detection)
  - [ ] 2.9 Implement distance calculations
    - Nearest edge: iterate all segments, find minimum distance
    - Segment distance: perpendicular distance to line segment
    - Use PointInPolygonService for containment checks
  - [ ] 2.10 Implement multi-format file I/O
    - AgOpenGPS Boundary.txt format (legacy compatibility)
    - GeoJSON export/import (standard geographic format)
    - KML export/import (Google Earth compatibility)
    - Coordinate conversion: WGS84 ↔ UTM
    - Support boundaries with holes (inner boundaries)
    - Support multi-part fields (non-contiguous polygons)
  - [ ] 2.11 Ensure boundary management tests pass
    - Run ONLY the 6-8 tests written in 2.1
    - Verify recording modes work correctly
    - Verify simplification reduces points while preserving shape
    - Verify area calculations match known values
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 6-8 tests written in 2.1 pass
- Recording modes (time/distance) work with real-time GPS updates
- Douglas-Peucker simplification reduces points effectively
- Shoelace area calculation accurate to <1% for test polygons
- Validation detects all specified error conditions
- File I/O preserves data in all 3 formats
- Events published with correct data and timestamps
- Performance: <10ms for simplification of typical field (50-200 points)

---

### Task Group 3: Headland Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1, 2 (PointInPolygonService, BoundaryManagementService)
**Estimated Effort:** 4-5 hours
**Complexity:** High

- [ ] 3.0 Complete HeadlandService implementation
  - [ ] 3.1 Write 5-7 focused tests for headland operations
    - Test single-pass headland generation (basic offset polygon)
    - Test multi-pass headland generation (multiple parallel offsets)
    - Test overlap prevention mode (no self-intersections)
    - Test overlap allowed mode (with configurable tolerance)
    - Test entry/exit point calculation (optimal field access)
    - Test entry/exit point nudging (manual adjustment)
    - Test section completion tracking (progress percentage)
  - [ ] 3.2 Create domain models in AgValoniaGPS.Models/FieldOperations/
    - Enum: `HeadlandOverlapMode` (PreventOverlap, AllowOverlap)
    - Class: `HeadlandConfiguration` (width, passes, overlap settings, corner radius)
    - Class: `HeadlandEntryExitPoint` (position, heading, isManuallyNudged)
    - Class: `HeadlandPass` (passNumber, points, offsetMeters)
  - [ ] 3.3 Create EventArgs in AgValoniaGPS.Models/Events/
    - `HeadlandGeneratedEventArgs` (headland, config, passCount, timestamp)
    - `HeadlandProgressChangedEventArgs` (sectionIndex, isCompleted, percentage, timestamp)
    - `EntryExitPointChangedEventArgs` (point, isEntry, wasNudged, timestamp)
    - All with readonly fields, UTC timestamps, validation
  - [ ] 3.4 Create IHeadlandService interface
    - Generation: `GenerateHeadland(boundary, config)`, `GenerateMultiPassHeadland(boundary, config)`
    - Entry/exit: `CalculateEntryPoint()`, `CalculateExitPoint()`, `NudgeEntryPoint()`, `NudgeExitPoint()`
    - Progress: `MarkSectionCompleted()`, `IsSectionCompleted()`, `GetCompletionPercentage()`, `GetCompletedSections()`
    - File I/O: `LoadHeadland(directory)`, `SaveHeadland(headland, directory)`
    - Events: `HeadlandGenerated`, `ProgressChanged`, `EntryExitPointChanged`
  - [ ] 3.5 Implement HeadlandService class
    - Generate headlands using offset polygon algorithm
    - Multi-pass generation: create offsets at widthMeters * passNumber
    - Store multi-pass as single multi-polygon structure
  - [ ] 3.6 Implement offset polygon algorithm
    - For each edge: calculate perpendicular unit vector, offset by distance
    - For each vertex: calculate intersection of adjacent offset edges
    - Corner handling: insert arc for smooth corners (configurable radius)
    - Self-intersection detection: segment intersection tests
    - Overlap handling: remove loops if prevention enabled
  - [ ] 3.7 Implement entry/exit point calculation
    - Find optimal entry: longest straight segment or user-specified point
    - Calculate approach angle based on field geometry
    - Manual nudging: adjust position and heading by specified offsets
    - Track whether points are automatic or manually adjusted
  - [ ] 3.8 Implement section completion tracking
    - Track completed sections by index
    - Calculate completion percentage: (completed sections / total sections) * 100
    - Integrate with coverage tracking (not purely geometric)
  - [ ] 3.9 Implement file I/O (AgOpenGPS Headland.Txt format)
    - Format: $Headland, isDriveThru, point count, easting,northing,heading per line
    - Maintain exact compatibility with AgOpenGPS legacy format
    - Save/load multi-pass headlands
  - [ ] 3.10 Ensure headland service tests pass
    - Run ONLY the 5-7 tests written in 3.1
    - Verify offset polygon geometry is correct
    - Verify multi-pass headlands at correct distances
    - Verify entry/exit point calculations
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 5-7 tests written in 3.1 pass
- Single and multi-pass headland generation works correctly
- Offset polygon algorithm produces valid geometry
- Corner handling (smooth vs sharp) works as configured
- Entry/exit points calculated optimally with manual override
- Section completion tracking accurate
- File I/O preserves headland data
- Performance: <50ms for typical field headland generation

---

### Task Group 4: U-Turn Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1, 2 + Wave 3 (SteeringCoordinatorService) + Wave 4 (SectionControlService)
**Estimated Effort:** 5-6 hours
**Complexity:** High

- [ ] 4.0 Complete UTurnService implementation
  - [ ] 4.1 Write 6-8 focused tests for U-turn operations
    - Test Question Mark turn pattern generation (LSL/RSR paths)
    - Test Semi-Circle turn pattern generation (LSR/RSL paths)
    - Test Keyhole turn pattern generation (RLR/LRL 3-arc paths)
    - Test T-turn pattern generation (with reverse segment)
    - Test Y-turn pattern generation (LSL/RSR with 45° offset)
    - Test optimal turn radius calculation from vehicle config
    - Test turn trigger detection (distance to boundary)
    - Test section pause/resume integration
  - [ ] 4.2 Create domain models in AgValoniaGPS.Models/FieldOperations/
    - Enum: `TurnPattern` (QuestionMark, SemiCircle, Keyhole, TTurn, YTurn)
    - Enum: `TurnState` (Idle, Approaching, Executing, Completing)
    - Class: `TurnConfiguration` (pattern, radius, trigger distance, overrides, timing)
    - Class: `TurnPath` (points, totalLength, minimumRadius)
  - [ ] 4.3 Create EventArgs in AgValoniaGPS.Models/Events/
    - `TurnPatternGeneratedEventArgs` (pattern, path, radius, timestamp)
    - `TurnStateChangedEventArgs` (oldState, newState, activePattern, timestamp)
    - `TurnTriggerDetectedEventArgs` (triggerPosition, distanceToBoundary, recommendedPattern, timestamp)
    - All with readonly fields, UTC timestamps, validation
  - [ ] 4.4 Create IUTurnService interface
    - Pattern generation: `GenerateTurnPattern(pattern, start, end, heading, config)`
    - Radius: `CalculateOptimalTurnRadius(vehicle, speed)`, `OverrideTurnRadius()`, `ClearRadiusOverride()`
    - Trigger: `CalculateTriggerDistance()`, `OverrideTriggerDistance()`, `IsTurnTriggered()`
    - State: `GetTurnState()`, `StartTurn()`, `CompleteTurn()`, `AbortTurn()`
    - Path following: `GetNextPathPoint(currentPosition)`, `GetCrossTrackError(currentPosition)`
    - Section control: `PauseSections()`, `ResumeSections()`
    - Events: `PatternGenerated`, `StateChanged`, `TriggerDetected`
  - [ ] 4.5 Implement UTurnService class
    - Inject: ISteeringCoordinatorService, ISectionControlService, IVehicleKinematicsService
    - Subscribe to PositionUpdateService for real-time trigger detection
    - Maintain turn state machine: Idle → Approaching → Executing → Completing
  - [ ] 4.6 Implement Dubins path algorithm
    - Calculate all 6 path types: LSL, RSR, LSR, RSL, RLR, LRL
    - Each path consists of arc-line-arc or arc-arc-arc segments
    - Select shortest valid path
    - Discretize path into waypoints for steering coordinator
  - [ ] 4.7 Implement turn pattern generation
    - Question Mark: LSL or RSR (same-direction turns, smooth curve)
    - Semi-Circle: LSR or RSL (opposite-direction turns, U-shape)
    - Keyhole: RLR or LRL (3-arc paths, tight spaces)
    - T-turn: Special case with reverse segment (backing up)
    - Y-turn: LSL/RSR with 45° offset (angled approach)
    - Generate patterns on-demand (not pre-calculated)
  - [ ] 4.8 Implement turn radius calculation
    - Base radius from vehicle wheelbase and max steer angle
    - Speed adaptation: increase radius at higher speeds for safety
    - User override: allow manual radius adjustment
    - Validation: ensure pattern fits within available space
  - [ ] 4.9 Implement turn trigger detection
    - Calculate distance to boundary using BoundaryManagementService
    - Trigger threshold: configurable distance (default = look-ahead distance + safety margin)
    - User override: manual trigger distance adjustment
    - Event: publish TriggerDetected when threshold reached
  - [ ] 4.10 Integrate with SteeringCoordinatorService (Wave 3)
    - Switch steering coordinator to turn path mode on StartTurn()
    - Pass turn path waypoints to coordinator for path following
    - Monitor cross-track error during turn execution
    - Switch back to guidance line mode on CompleteTurn()
  - [ ] 4.11 Integrate with SectionControlService (Wave 4)
    - Call PauseAllSections() when turn state → Executing
    - Call ResumeAllSections() when turn state → Idle
    - Configurable pause timing (default 0.5s before turn start)
    - Subscribe to SectionStateChanged events for coordinated management
  - [ ] 4.12 Ensure U-turn service tests pass
    - Run ONLY the 6-8 tests written in 4.1
    - Verify all 5 turn patterns generate valid paths
    - Verify Dubins path calculations correct
    - Verify section pause/resume integration
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 6-8 tests written in 4.1 pass
- All 5 turn patterns generate valid Dubins paths
- Turn radius calculation adapts to vehicle config and speed
- Turn trigger detection accurate based on boundary distance
- Section control integration: pause on start, resume on complete
- Steering coordinator integration: smooth transition to/from turn path
- Turn state machine transitions correctly
- Performance: <10ms for turn pattern generation

---

### Task Group 5: Tram Line Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 + Wave 2 (ABLineService)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

- [ ] 5.0 Complete TramLineService implementation
  - [ ] 5.1 Write 5-7 focused tests for tram line operations
    - Test tram line generation from AB line (parallel lines)
    - Test configurable spacing (track width, seed width, custom)
    - Test multi-pass support (multiple patterns)
    - Test start offset application (distance from AB origin)
    - Test pattern swap (A/B side reversal)
    - Test pattern navigation (next/previous)
    - Test file I/O (save/load to field directory)
  - [ ] 5.2 Create domain models in AgValoniaGPS.Models/FieldOperations/
    - Class: `TramLineConfiguration` (spacing, passCount, startOffset, swap, alpha)
    - Class: `TramLine` (startPoint, endPoint, heading, offsetFromBaseLine)
    - Class: `TramLinePattern` (name, lines, configuration)
  - [ ] 5.3 Create EventArgs in AgValoniaGPS.Models/Events/
    - `TramLineGeneratedEventArgs` (tramLines, configuration, timestamp)
    - `TramLineUpdatedEventArgs` (newConfig, oldConfig, timestamp)
    - `TramLinePatternChangedEventArgs` (oldIndex, newIndex, activePattern, timestamp)
    - All with readonly fields, UTC timestamps, validation
  - [ ] 5.4 Create ITramLineService interface
    - Generation: `GenerateTramLines(baseLine, config)`, `UpdateConfiguration(config)`
    - Pattern management: `AddPattern()`, `DeletePattern()`, `GetAllPatterns()`, `SetActivePattern()`, `GetActivePatternIndex()`
    - Navigation: `NavigateToNext()`, `NavigateToPrevious()`
    - File I/O: `SaveTramLines(directory)`, `LoadTramLines(directory)`
    - Events: `TramLinesGenerated`, `ConfigurationUpdated`, `ActivePatternChanged`
  - [ ] 5.5 Implement TramLineService class
    - Inject: IABLineService (access active AB line)
    - Generate parallel lines using offset perpendicular to AB line
    - Calculate offset positions: baseLine ± (spacing * n) for n passes
  - [ ] 5.6 Implement parallel line generation
    - Get active AB line from ABLineService
    - Calculate perpendicular vector to AB line heading
    - Generate lines at spacing intervals on both sides
    - Apply start offset (shift all lines by offset distance)
    - Apply pattern swap (reverse A/B sides if enabled)
  - [ ] 5.7 Implement pattern management
    - Store multiple patterns with names
    - Track active pattern index
    - Navigation: cycle through patterns with next/previous
    - Add/delete patterns dynamically
  - [ ] 5.8 Implement file I/O
    - Save tram line patterns to field directory (JSON format)
    - Include configuration and line geometry
    - Load patterns and restore active index
    - Coordinate with field file structure
  - [ ] 5.9 Ensure tram line service tests pass
    - Run ONLY the 5-7 tests written in 5.1
    - Verify parallel line geometry correct
    - Verify spacing calculations accurate
    - Verify pattern management (add/delete/navigate)
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 5-7 tests written in 5.1 pass
- Tram lines generated parallel to AB line at correct spacing
- Multi-pass support with pattern navigation works
- Start offset and pattern swap applied correctly
- Pattern management (add/delete/active) functional
- File I/O preserves all pattern data
- Integration with ABLineService: updates when AB line changes
- Performance: <20ms for typical tram line generation (10 lines)

---

### Task Group 6: Integration Testing & Performance Validation
**Assigned implementer:** testing-engineer
**Dependencies:** Task Groups 1-5
**Estimated Effort:** 4-5 hours
**Complexity:** High

- [ ] 6.0 Review existing tests and add integration scenarios
  - [ ] 6.1 Review tests from Task Groups 1-5
    - Review 7 tests from PointInPolygonService (1.1)
    - Review 7 tests from BoundaryManagementService (2.1)
    - Review 6 tests from HeadlandService (3.1)
    - Review 7 tests from UTurnService (4.1)
    - Review 6 tests from TramLineService (5.1)
    - Total existing tests: 33 tests
  - [ ] 6.2 Analyze integration test coverage gaps
    - Identify end-to-end workflow gaps (boundary recording → headland generation → turn execution)
    - Focus on cross-wave integration (Wave 1 position updates, Wave 2 guidance lines, Wave 3 steering, Wave 4 sections)
    - Identify edge case gaps (GPS loss during recording, very large/small fields, turn patterns in tight spaces)
    - Do NOT assess entire application coverage
  - [ ] 6.3 Write up to 10 additional integration tests maximum
    - Full boundary recording flow: GPS position → recording → simplification → validation → file save (1 test)
    - Headland generation from recorded boundary: boundary load → multi-pass headland → entry/exit calculation (1 test)
    - Turn execution integration: trigger detection → section pause → steering coordinator → section resume (1 test)
    - Tram line integration: AB line → tram generation → pattern management (1 test)
    - Edge case: GPS signal loss during boundary recording (gap detection) (1 test)
    - Edge case: boundary with holes (inner boundary containment checks) (1 test)
    - Edge case: turn pattern in constrained space (validation failure handling) (1 test)
    - Edge case: very large field (>1000 points) with R-tree indexing (1 test)
    - Edge case: multi-part field (non-contiguous polygons) (1 test)
    - Performance benchmark: 1000 point-in-polygon checks in <1s (proves 10Hz capability) (1 test)
  - [ ] 6.4 Create performance benchmark tests
    - Point-in-polygon: 1000 checks in <1000ms (1ms per check)
    - Boundary simplification: 100-point polygon in <10ms
    - Headland generation: typical field in <50ms
    - Turn pattern generation: <10ms per pattern
    - Tram line generation: 10 lines in <20ms
    - Integrated into test 6.3 where appropriate
  - [ ] 6.5 Create cross-wave integration tests
    - Wave 1 integration: PositionUpdateService → BoundaryManagementService recording
    - Wave 2 integration: ABLineService → TramLineService generation
    - Wave 3 integration: SteeringCoordinatorService → UTurnService path following
    - Wave 4 integration: SectionControlService → UTurnService pause/resume
    - Verify EventArgs flow across services
  - [ ] 6.6 Run feature-specific tests only
    - Run tests from 1.1, 2.1, 3.1, 4.1, 5.1, and 6.3
    - Expected total: approximately 43 tests maximum
    - Do NOT run the entire application test suite
    - Verify critical workflows pass
  - [ ] 6.7 Validate performance benchmarks
    - Verify point-in-polygon: <1ms per check at 10Hz
    - Verify boundary operations: <10ms simplification
    - Verify headland generation: <50ms typical field
    - Verify turn patterns: <10ms generation
    - Verify tram lines: <20ms for 10 lines
  - [ ] 6.8 Test file format compatibility
    - Load AgOpenGPS legacy Boundary.txt files
    - Save and reload all 3 formats (AgOpenGPS .txt, GeoJSON, KML)
    - Verify coordinate conversion accuracy (WGS84 ↔ UTM)
    - Verify no data loss on round-trip save/load

**Acceptance Criteria:**
- All feature-specific tests pass (approximately 43 tests total)
- No more than 10 additional tests added by testing-engineer
- All performance benchmarks met (point-in-polygon <1ms, headland <50ms, turns <10ms)
- Cross-wave integration verified (Waves 1-4 services work with Wave 5)
- End-to-end workflows complete successfully
- File format compatibility verified (all 3 formats)
- Edge cases handled gracefully (GPS loss, large fields, tight spaces)
- Testing focused exclusively on Wave 5 feature requirements

---

## Execution Order

**Recommended implementation sequence:**

1. **Task Group 1: PointInPolygonService (Foundation)** (3-4 hours)
   - Must be built first - used by all other services
   - Foundational geometric algorithm
   - R-tree indexing for performance
   - Independent of other Wave 5 services

2. **Task Group 2: BoundaryManagementService** (5-6 hours)
   - Depends on PointInPolygonService for containment checks
   - Provides boundary data for HeadlandService
   - GPS integration with Wave 1 PositionUpdateService
   - Core field operations foundation

3. **Parallel Execution (after Group 2 complete):**
   - **Task Group 3: HeadlandService** (4-5 hours)
     - Depends on Groups 1 & 2
     - Independent of Groups 4 & 5
   - **Task Group 5: TramLineService** (3-4 hours)
     - Depends on Group 1 and Wave 2 ABLineService
     - Independent of Groups 3 & 4
     - Can run parallel with Group 3

4. **Task Group 4: UTurnService** (5-6 hours)
   - Depends on Groups 1 & 2 for boundary/trigger detection
   - Depends on Wave 3 SteeringCoordinatorService
   - Depends on Wave 4 SectionControlService
   - Complex cross-wave integration
   - Build after Groups 1-2 complete, can parallel with Groups 3 & 5

5. **Task Group 6: Integration Testing & Performance Validation** (4-5 hours)
   - Depends on all Groups 1-5 being complete
   - Validates end-to-end workflows
   - Performance benchmarking
   - Cross-wave integration verification

**Critical Path:** 1 → 2 → (3 || 4 || 5) → 6

**Estimated Total Time:** 20-28 hours (with parallel execution of Groups 3, 4, 5 after Group 2)

**Parallelization Opportunities:**
- Groups 3, 4, 5 can run in parallel after Groups 1 & 2 complete
- Each group has minimal interdependencies during this phase
- Group 4 has most cross-wave dependencies but can still parallel

---

## Service Registration

All new services must be registered in `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Field Operations Services
services.AddSingleton<IPointInPolygonService, PointInPolygonService>();
services.AddSingleton<IBoundaryManagementService, BoundaryManagementService>();
services.AddSingleton<IHeadlandService, HeadlandService>();
services.AddSingleton<IUTurnService, UTurnService>();
services.AddSingleton<ITramLineService, TramLineService>();
```

---

## File Organization

All files follow `NAMING_CONVENTIONS.md` (flat structure in FieldOperations/):

```
AgValoniaGPS.Services/FieldOperations/
├── PointInPolygonService.cs                 (Task Group 1)
├── IPointInPolygonService.cs                (Task Group 1)
├── BoundaryManagementService.cs             (Task Group 2)
├── IBoundaryManagementService.cs            (Task Group 2)
├── HeadlandService.cs                       (Task Group 3)
├── IHeadlandService.cs                      (Task Group 3)
├── UTurnService.cs                          (Task Group 4)
├── IUTurnService.cs                         (Task Group 4)
├── TramLineService.cs                       (Task Group 5)
├── ITramLineService.cs                      (Task Group 5)

AgValoniaGPS.Models/FieldOperations/
├── BoundaryRecordingMode.cs                 (Task Group 2)
├── BoundaryRecordingState.cs                (Task Group 2)
├── SimplificationMode.cs                    (Task Group 2)
├── BoundaryFileFormat.cs                    (Task Group 2)
├── BoundaryRecordingConfiguration.cs        (Task Group 2)
├── BoundaryValidationResult.cs              (Task Group 2)
├── HeadlandOverlapMode.cs                   (Task Group 3)
├── HeadlandConfiguration.cs                 (Task Group 3)
├── HeadlandEntryExitPoint.cs                (Task Group 3)
├── HeadlandPass.cs                          (Task Group 3)
├── TurnPattern.cs                           (Task Group 4)
├── TurnState.cs                             (Task Group 4)
├── TurnConfiguration.cs                     (Task Group 4)
├── TurnPath.cs                              (Task Group 4)
├── TramLineConfiguration.cs                 (Task Group 5)
├── TramLine.cs                              (Task Group 5)
├── TramLinePattern.cs                       (Task Group 5)

AgValoniaGPS.Models/Events/
├── BoundaryRecordingStartedEventArgs.cs     (Task Group 2)
├── BoundaryPointAddedEventArgs.cs           (Task Group 2)
├── BoundaryRecordingCompletedEventArgs.cs   (Task Group 2)
├── BoundaryValidationEventArgs.cs           (Task Group 2)
├── BoundaryAreaUpdatedEventArgs.cs          (Task Group 2)
├── HeadlandGeneratedEventArgs.cs            (Task Group 3)
├── HeadlandProgressChangedEventArgs.cs      (Task Group 3)
├── EntryExitPointChangedEventArgs.cs        (Task Group 3)
├── TurnPatternGeneratedEventArgs.cs         (Task Group 4)
├── TurnStateChangedEventArgs.cs             (Task Group 4)
├── TurnTriggerDetectedEventArgs.cs          (Task Group 4)
├── TramLineGeneratedEventArgs.cs            (Task Group 5)
├── TramLineUpdatedEventArgs.cs              (Task Group 5)
├── TramLinePatternChangedEventArgs.cs       (Task Group 5)

AgValoniaGPS.Services.Tests/FieldOperations/
├── PointInPolygonServiceTests.cs            (Task Group 1)
├── BoundaryManagementServiceTests.cs        (Task Group 2)
├── HeadlandServiceTests.cs                  (Task Group 3)
├── UTurnServiceTests.cs                     (Task Group 4)
├── TramLineServiceTests.cs                  (Task Group 5)
├── FieldOperationsIntegrationTests.cs       (Task Group 6)
```

---

## Performance Requirements

Each service must meet these performance targets:

- **PointInPolygonService.IsPointInPolygon()**: <1ms per check (10Hz capable)
- **BoundaryManagementService.SimplifyBoundary()**: <10ms for typical field (50-200 points)
- **HeadlandService.GenerateHeadland()**: <50ms for typical field
- **UTurnService.GenerateTurnPattern()**: <10ms per pattern
- **TramLineService.GenerateTramLines()**: <20ms for 10 lines
- **Real-time area calculation**: No noticeable lag during recording

Performance verification included in Task Group 6.

---

## Testing Constraints

Per `agent-os/standards/testing/test-writing.md`:

- **Minimal tests during development**: Each task group writes 2-8 focused tests only
- **Test critical paths only**: Focus on core algorithms and workflows
- **Defer edge cases to Task Group 6**: testing-engineer adds maximum 10 integration tests
- **No comprehensive coverage**: Skip exhaustive testing of all scenarios
- **Fast execution**: All tests must complete in <10 seconds total

**Total expected tests for Wave 5:** Approximately 43 tests maximum

---

## Integration with Existing Code

**Wave 1 Integration (Position & Kinematics):**
- Subscribe to `PositionUpdateService.PositionUpdated` for boundary recording
- Use `GetCurrentPosition()`, `GetCurrentHeading()`, `GetCurrentSpeed()`
- Access `VehicleKinematicsService` for wheelbase, turn radius calculations

**Wave 2 Integration (Guidance Lines):**
- Access `ABLineService.GetActiveABLine()` for tram line generation
- Reference `ABLineFileService` for consistent file I/O patterns
- Use AB line heading and position for parallel line calculations

**Wave 3 Integration (Steering):**
- Coordinate with `SteeringCoordinatorService.Update()` during turn execution
- Pass turn path waypoints for path following
- Switch between turn path and guidance line steering modes
- Monitor `SteeringUpdated` event for cross-track error

**Wave 4 Integration (Section Control):**
- Call `SectionControlService.PauseAllSections()` when turn starts
- Call `SectionControlService.ResumeAllSections()` when turn completes
- Subscribe to `SectionStateChanged` events for state coordination
- Configurable pause timing based on section response

**Cross-Wave Event Flow Examples:**
```
Boundary Recording:
PositionUpdateService.PositionUpdated
  -> BoundaryManagementService.ProcessPosition()
  -> BoundaryManagementService.PointAdded (event)
  -> BoundaryManagementService.AreaUpdated (event)

Turn Execution:
PositionUpdateService.PositionUpdated
  -> UTurnService.IsTurnTriggered()
  -> UTurnService.TriggerDetected (event)
  -> UTurnService.StartTurn()
  -> SectionControlService.PauseAllSections()
  -> SteeringCoordinatorService.Update(turnPath)
  -> UTurnService.CompleteTurn()
  -> SectionControlService.ResumeAllSections()

Tram Line Generation:
ABLineService.GetActiveABLine()
  -> TramLineService.GenerateTramLines()
  -> TramLineService.TramLinesGenerated (event)
```

---

## Algorithm Implementation Notes

**Douglas-Peucker Simplification:**
- Recursive divide-and-conquer approach
- Find point with max perpendicular distance from line
- Split if distance > tolerance, recurse on segments
- O(n log n) average, O(n²) worst case

**Shoelace Formula:**
- Area = 0.5 * |Σ(xi * yi+1 - xi+1 * yi)|
- O(n) time, O(1) space
- Works for any non-self-intersecting polygon
- Use UTM coordinates for accuracy

**Ray-Casting (Point-in-Polygon):**
- Cast horizontal ray from point to infinity
- Count edge intersections
- Odd count = inside, even count = outside
- Handle edge cases: on edge, on vertex, horizontal edges

**R-tree Spatial Indexing:**
- Build hierarchical MBR tree for segments
- Query optimization: check only relevant segments
- Use for polygons >500 vertices
- O(log n) query time average

**Offset Polygon (Headlands):**
- Offset each edge perpendicular by distance
- Calculate vertex intersections
- Insert arcs for smooth corners
- Detect and remove self-intersections
- O(n²) for self-intersection detection

**Dubins Path (Turns):**
- Calculate 6 path types: LSL, RSR, LSR, RSL, RLR, LRL
- Select shortest valid path
- Discretize into waypoints
- Map to turn patterns (Question Mark, Semi-Circle, etc.)

---

## Success Criteria

**Functional Completeness:**
- All 5 services implemented with full interface coverage
- All recording modes functional (time/distance for boundary)
- All 5 turn patterns generate valid paths
- All 3 file formats (AgOpenGPS .txt, GeoJSON, KML) read/write correctly
- All integration points with Waves 1-4 functional

**Performance:**
- 100% of point-in-polygon checks complete in <1ms
- 95% of boundary simplifications complete in <10ms for typical fields
- 95% of headland generations complete in <50ms for typical fields
- 100% of turn pattern calculations complete in <10ms
- 100% of tram line generations complete in <20ms for typical patterns

**Quality:**
- 100% test pass rate for all unit tests (approximately 43 tests)
- All performance benchmarks met
- Zero memory leaks detected during load testing
- All edge cases handled gracefully
- All EventArgs follow established patterns with validation

**Integration:**
- Successful integration with PositionUpdateService (Wave 1)
- Successful integration with ABLineService (Wave 2)
- Successful integration with SteeringCoordinatorService (Wave 3)
- Successful integration with SectionControlService (Wave 4)
- All cross-wave event flows verified

**File Format Compatibility:**
- AgOpenGPS Boundary.txt files load correctly
- AgOpenGPS Headland.Txt files load correctly
- Coordinate conversions maintain precision
- GeoJSON and KML formats preserve data accuracy

---

Last Updated: 2025-10-19
Wave: 5 - Field Operations
Status: Ready for Implementation
