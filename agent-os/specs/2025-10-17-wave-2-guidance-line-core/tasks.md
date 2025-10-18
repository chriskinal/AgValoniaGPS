# Task Breakdown: Wave 2 - Guidance Line Core

## Overview

**Total Estimated Tasks:** 80+ sub-tasks across 10 major task groups
**Estimated LOC:** ~2,000 lines of code
**Complexity:** HIGH
**Target Performance:** 20-25 Hz operation
**Test Coverage Target:** >80%
**Dependencies:** Wave 1 (PositionUpdateService, HeadingCalculatorService, VehicleKinematicsService) - COMPLETE

**Assigned Roles:**
- api-engineer (business logic services)
- testing-engineer (comprehensive test coverage)

## Task List

---

### Phase 1: Data Models & Infrastructure

#### Task Group 1: Core Data Models
**Assigned Implementer:** api-engineer
**Dependencies:** None
**Estimated Effort:** 8 hours

- [x] 1.0 Create core data models for guidance lines
  - [x] 1.1 Create Position model (if not exists from Wave 1)
    - Fields: Easting, Northing, Altitude
    - Validation: Non-NaN, non-infinite values
    - Reuse from Wave 1 if available
  - [x] 1.2 Create ABLine model
    - Fields: Name, PointA, PointB, Heading, NudgeOffset, CreatedDate
    - Computed properties: Length, MidPoint, UnitVector
    - Validation: Minimum length threshold (>0.5m), valid points
    - XML documentation
  - [x] 1.3 Create CurveLine model
    - Fields: Name, Points (List<Position>), SmoothingParameters, CreatedDate
    - Properties: TotalLength, PointCount, AverageSpacing
    - Validation: Minimum points (>=3), point spacing checks
    - XML documentation
  - [x] 1.4 Create ContourLine model
    - Fields: Name, Points (List<Position>), IsLocked, MinDistanceThreshold, CreatedDate
    - Properties: TotalLength, PointCount, IsRecording
    - Validation: Sufficient points for locking (>=10)
    - XML documentation
  - [x] 1.5 Create GuidanceLineResult model
    - Fields: CrossTrackError, ClosestPoint, HeadingError, DistanceToLine, ClosestPointIndex
    - Units: All distances in meters (internal), converted at API boundary
    - XML documentation
  - [x] 1.6 Create SmoothingParameters model
    - Fields: Method (enum), Tension, PointDensity, MaxIterations
    - Enum: SmoothingMethod (CubicSpline, CatmullRom, BSpline)
    - Default values for each method
    - XML documentation
  - [x] 1.7 Create UnitSystem enum
    - Values: Metric, Imperial
    - Conversion utilities: MetersToFeet, FeetToMeters
    - XML documentation
  - [x] 1.8 Create ValidationResult model
    - Fields: IsValid, ErrorMessages (List<string>), Warnings (List<string>)
    - Helper methods: AddError, AddWarning, Merge
    - XML documentation

**Acceptance Criteria:**
- All models have proper validation logic
- XML documentation complete for all public properties/methods
- No UI framework dependencies
- Models are serializable for persistence
- Unit conversion utilities tested

---

#### Task Group 2: Event Infrastructure
**Assigned Implementer:** api-engineer
**Dependencies:** Task Group 1
**Estimated Effort:** 4 hours

- [x] 2.0 Create event argument classes for guidance services
  - [x] 2.1 Create ABLineChangedEventArgs
    - Fields: Line (ABLine), ChangeType (enum: Created, Modified, Activated, Nudged)
    - Timestamp property
    - XML documentation
  - [x] 2.2 Create CurveLineChangedEventArgs
    - Fields: Curve (CurveLine), ChangeType (enum: Recorded, Smoothed, Activated)
    - Timestamp property
    - XML documentation
  - [x] 2.3 Create ContourStateChangedEventArgs
    - Fields: Contour (ContourLine), EventType (enum: RecordingStarted, PointAdded, Locked, Unlocked)
    - PointCount property
    - Timestamp property
    - XML documentation
  - [x] 2.4 Create GuidanceStateChangedEventArgs
    - Fields: ActiveLineType (enum: ABLine, Curve, Contour), Result (GuidanceLineResult)
    - IsActive boolean
    - Timestamp property
    - XML documentation

**Acceptance Criteria:**
- All event args follow EventArgs pattern
- Immutable event data (readonly fields)
- XML documentation complete
- Enum types properly defined

---

### Phase 2: AB Line Service Implementation

#### Task Group 3: AB Line Service Core
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Effort:** 16 hours

- [x] 3.0 Implement AB Line Service
  - [x] 3.1 Write 2-8 focused tests for ABLine core functionality
    - Test: CreateFromPoints with valid points creates line
    - Test: CreateFromPoints with identical points throws exception
    - Test: CreateFromHeading creates line with correct orientation
    - Test: CalculateGuidance on line returns zero XTE
    - Test: CalculateGuidance right of line returns positive XTE
    - Test: CalculateGuidance left of line returns negative XTE
    - Limit to 6 critical tests maximum
  - [x] 3.2 Create IABLineService interface
    - Method: CreateFromPoints(Position pointA, Position pointB, string name)
    - Method: CreateFromHeading(Position origin, double headingRadians, string name)
    - Method: CalculateGuidance(Position currentPosition, double currentHeading, ABLine line)
    - Method: GetClosestPoint(Position currentPosition, ABLine line)
    - Event: ABLineChanged
    - XML documentation with usage examples
  - [x] 3.3 Implement ABLineService class
    - Implement CreateFromPoints logic
      - Validate points are not identical
      - Calculate heading between points
      - Create ABLine with origin at midpoint
      - Emit ABLineChanged event (Created)
    - Implement CreateFromHeading logic
      - Validate heading is valid (0-2π)
      - Create ABLine with specified origin and heading
      - Emit ABLineChanged event (Created)
    - Implement CalculateGuidance logic
      - Calculate perpendicular distance (point-to-line formula)
      - Find closest point on infinite line projection
      - Calculate heading error relative to line heading
      - Return GuidanceLineResult
      - Optimize for <5ms execution time
    - Implement GetClosestPoint logic
      - Project current position onto line
      - Return closest position on line segment
      - Handle edge cases (before start, after end)
  - [x] 3.4 Implement line validation logic
    - ValidateLine(ABLine line) method
    - Check: Minimum length (>0.5m)
    - Check: Valid points (no NaN, no infinity)
    - Check: Non-zero heading
    - Return ValidationResult
  - [x] 3.5 Add snap distance activation logic
    - CalculateSnapDistance(Position currentPosition, ABLine line, double snapThreshold)
    - Return boolean: within snap threshold
    - Emit GuidanceStateChanged when activated
  - [x] 3.6 Ensure AB Line core tests pass
    - Run ONLY the 2-8 tests written in 3.1
    - Verify XTE calculation accuracy (±1cm)
    - Do NOT run entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 3.1 pass
- Cross-track error calculation accurate to ±1cm
- Closest point calculation correct in all cases
- Validation logic handles all edge cases
- Events fire on state changes
- Execution time <5ms for CalculateGuidance

---

#### Task Group 4: AB Line Advanced Operations
**Assigned Implementer:** api-engineer
**Dependencies:** Task Group 3
**Estimated Effort:** 12 hours

- [x] 4.0 Implement AB Line advanced operations
  - [x] 4.1 Write 2-8 focused tests for advanced AB Line operations
    - Test: NudgeLine with positive offset moves line correctly
    - Test: NudgeLine with negative offset moves line correctly
    - Test: GenerateParallelLines creates correct number of lines
    - Test: GenerateParallelLines maintains exact spacing (±2cm)
    - Test: Parallel lines at vehicle boundary extremes
    - Test: Unit conversion (metric to imperial) correctness
    - Limit to 6 critical tests maximum
  - [x] 4.2 Implement NudgeLine operation
    - NudgeLine(ABLine line, double offsetMeters) method
    - Calculate perpendicular offset direction
    - Create new ABLine with nudged origin
    - Preserve line heading and length
    - Update NudgeOffset property
    - Emit ABLineChanged event (Nudged)
    - Handle positive and negative offsets
  - [x] 4.3 Implement parallel line generation
    - GenerateParallelLines(ABLine referenceLine, double spacingMeters, int count, UnitSystem units) method
    - Generate 'count' lines on left side
    - Generate 'count' lines on right side
    - Maintain exact spacing (±2cm accuracy)
    - Convert spacing based on UnitSystem
    - Return List<ABLine> with all parallel lines
    - Optimize for <50ms execution time for 10 lines
  - [x] 4.4 Implement unit conversion support
    - Internal calculations always in meters
    - Accept spacing in meters or feet based on UnitSystem
    - Convert at API boundary (input/output)
    - Helper methods: ConvertToMeters, ConvertFromMeters
  - [x] 4.5 Implement line naming conventions
    - Auto-generate names for parallel lines: "Line +1", "Line -1", etc.
    - Support custom naming prefix
    - Maintain reference to parent line
  - [ ] 4.6 Ensure advanced AB Line tests pass
    - Run ONLY the 2-8 tests written in 4.1
    - Verify parallel line spacing accuracy (±2cm)
    - Verify nudge operation correctness
    - Do NOT run entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 4.1 pass
- Parallel line spacing accurate to ±2cm
- Nudge operation moves line perpendicular to heading
- Unit conversion works correctly for metric/imperial
- Parallel generation completes in <50ms for 10 lines

---

### Phase 3: Curve Line Service Implementation

#### Task Group 5: Curve Line Service Core
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Effort:** 20 hours

- [ ] 5.0 Implement Curve Line Service
  - [ ] 5.1 Write 2-8 focused tests for Curve Line core functionality
    - Test: StartRecording initializes recording state
    - Test: AddPoint with minimum distance adds point
    - Test: AddPoint within minimum distance skips point
    - Test: FinishRecording creates CurveLine
    - Test: CalculateGuidance on curve returns correct XTE
    - Test: GetClosestPoint finds nearest curve point
    - Limit to 6 critical tests maximum
  - [ ] 5.2 Create ICurveLineService interface
    - Method: StartRecording(Position startPosition)
    - Method: AddPoint(Position point, double minDistanceMeters)
    - Method: FinishRecording(string name)
    - Method: CalculateGuidance(Position currentPosition, double currentHeading, CurveLine curve, bool findGlobal = false)
    - Method: GetClosestPoint(Position currentPosition, CurveLine curve, int searchStartIndex = -1)
    - Method: GetHeadingAtPoint(Position point, CurveLine curve)
    - Event: CurveChanged
    - XML documentation with usage examples
  - [ ] 5.3 Implement CurveLineService class - Recording
    - Implement StartRecording logic
      - Initialize empty point list
      - Store starting position
      - Set recording state to true
      - Emit CurveChanged event (Recorded)
    - Implement AddPoint logic
      - Calculate distance from last point
      - If distance >= minDistanceMeters, add point
      - If distance < minDistanceMeters, skip point
      - Emit CurveChanged event on point added
      - Handle first point special case
    - Implement FinishRecording logic
      - Validate sufficient points (>=3)
      - Create CurveLine from recorded points
      - Set recording state to false
      - Return CurveLine
      - Emit CurveChanged event (Recorded)
  - [ ] 5.4 Implement CurveLineService class - Guidance Calculations
    - Implement GetClosestPoint logic (local search)
      - Start from searchStartIndex (or 0 if -1)
      - Search forward and backward from start index
      - Use local optimization for performance
      - Return closest position and index
      - Optimize for sequential calls (vehicle moving along curve)
    - Implement GetClosestPoint logic (global search)
      - Search entire curve when findGlobal=true
      - Use spatial indexing if point count > 1000
      - Return globally closest position and index
      - More expensive but accurate for initial approach
    - Implement CalculateGuidance logic
      - Find closest point (local or global based on flag)
      - Calculate perpendicular distance to curve segment
      - Calculate tangent heading at closest point
      - Calculate heading error
      - Return GuidanceLineResult
      - Optimize for <5ms execution time
    - Implement GetHeadingAtPoint logic
      - Find two nearest curve points
      - Calculate tangent vector between them
      - Return heading in radians
      - Handle curve endpoints (use direction to/from endpoint)
  - [ ] 5.5 Implement curve validation logic
    - ValidateCurve(CurveLine curve) method
    - Check: Minimum points (>=3)
    - Check: Point spacing not too close (<0.1m)
    - Check: Point spacing not too far (>100m)
    - Check: No duplicate consecutive points
    - Check: Curve smoothness (max heading change per segment)
    - Return ValidationResult
  - [ ] 5.6 Ensure Curve Line core tests pass
    - Run ONLY the 2-8 tests written in 5.1
    - Verify closest point finding works
    - Verify XTE calculation accuracy
    - Do NOT run entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 5.1 pass
- Recording respects minimum distance threshold
- Closest point finding works for local and global search
- XTE calculation accurate for curved paths
- Heading calculation at curve points correct
- Execution time <5ms for CalculateGuidance

---

#### Task Group 6: Curve Smoothing & Advanced Operations
**Assigned Implementer:** api-engineer
**Dependencies:** Task Group 5
**Estimated Effort:** 16 hours

- [ ] 6.0 Implement curve smoothing and advanced operations
  - [ ] 6.1 Write 2-8 focused tests for curve smoothing
    - Test: SmoothCurve with CubicSpline produces smoother path
    - Test: SmoothCurve increases point count appropriately
    - Test: GenerateParallelCurves creates correct number of curves
    - Test: Parallel curves maintain offset distance along curve
    - Test: Smoothing with different parameters produces different results
    - Test: Complex S-curve maintains smoothness
    - Limit to 6 critical tests maximum
  - [ ] 6.2 Add MathNet.Numerics package dependency
    - Add NuGet package reference to AgOpenGPS.Core project
    - Version: Latest stable (7.x or higher)
    - Verify package compatibility with .NET 8.0
  - [ ] 6.3 Implement SmoothCurve operation
    - SmoothCurve(CurveLine curve, SmoothingParameters parameters) method
    - Extract X and Y coordinates from curve points
    - Implement CubicSpline smoothing using MathNet.Numerics
      - Use CubicSpline.InterpolateNatural for natural cubic spline
      - Resample curve at uniform intervals based on PointDensity
      - Apply tension parameter for spline stiffness
    - Implement CatmullRom smoothing option
      - Use Catmull-Rom spline algorithm
      - Better for maintaining curve shape through control points
    - Implement BSpline smoothing option
      - Use B-spline algorithm for maximum smoothness
    - Validate smoothed curve (no self-intersections, reasonable point spacing)
    - Return new CurveLine with smoothed points
    - Emit CurveChanged event (Smoothed)
    - Optimize for <200ms execution time for 1000-point curve
  - [ ] 6.4 Implement parallel curve generation
    - GenerateParallelCurves(CurveLine referenceCurve, double spacingMeters, int count, UnitSystem units) method
    - Calculate perpendicular offset at each curve point
    - Generate 'count' curves on left side (negative offset)
    - Generate 'count' curves on right side (positive offset)
    - Use tangent heading at each point for perpendicular direction
    - Maintain exact offset distance along entire curve (±2cm)
    - Apply smoothing to parallel curves to prevent jagged paths
    - Convert spacing based on UnitSystem
    - Return List<CurveLine> with all parallel curves
    - Handle varying curvature (tighter curves need more points)
  - [ ] 6.5 Implement curve quality metrics
    - CalculateCurveQuality(CurveLine curve) method
    - Metrics: Average curvature, max curvature, smoothness score
    - Detect sharp corners or discontinuities
    - Use for validation and user feedback
  - [ ] 6.6 Ensure curve smoothing tests pass
    - Run ONLY the 2-8 tests written in 6.1
    - Verify smoothing produces visually smooth paths
    - Verify parallel curve generation accuracy
    - Do NOT run entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 6.1 pass
- Cubic spline smoothing works correctly using MathNet.Numerics
- Smoothed curves are visually smoother than input
- Parallel curves maintain offset distance (±2cm)
- Smoothing completes in <200ms for 1000-point curve
- MathNet.Numerics integrated successfully

---

### Phase 4: Contour Service Implementation

#### Task Group 7: Contour Service
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Effort:** 12 hours

- [ ] 7.0 Implement Contour Service
  - [ ] 7.1 Write 2-8 focused tests for Contour functionality
    - Test: StartRecording initializes recording state
    - Test: AddPoint within minimum distance skips point
    - Test: AddPoint exceeds minimum distance adds point
    - Test: LockContour with sufficient points locks successfully
    - Test: LockContour with insufficient points fails
    - Test: CalculateGuidance on contour returns correct offset
    - Limit to 6 critical tests maximum
  - [ ] 7.2 Create IContourService interface
    - Method: StartRecording(Position startPosition, double minDistanceMeters)
    - Method: AddPoint(Position currentPosition, double offset)
    - Method: LockContour(string name)
    - Method: StopRecording()
    - Method: CalculateGuidance(Position currentPosition, double currentHeading, ContourLine contour)
    - Method: CalculateOffset(Position currentPosition, ContourLine contour)
    - Method: SetLocked(bool locked)
    - Method: ValidateContour(ContourLine contour)
    - Property: bool IsRecording { get; }
    - Property: bool IsLocked { get; }
    - Event: StateChanged
    - XML documentation with usage examples
  - [ ] 7.3 Implement ContourService class - Recording
    - Implement StartRecording logic
      - Initialize empty contour point list
      - Store starting position and minimum distance threshold
      - Set IsRecording = true, IsLocked = false
      - Emit StateChanged event (RecordingStarted)
    - Implement AddPoint logic
      - Calculate distance from last recorded point
      - If distance >= minDistanceMeters, add point to contour
      - If distance < minDistanceMeters, skip point
      - Apply offset to recorded position (perpendicular to vehicle heading)
      - Emit StateChanged event (PointAdded) when point added
      - Handle field boundary proximity (warn if near edge)
    - Implement StopRecording logic
      - Set IsRecording = false
      - Do NOT lock contour (requires explicit LockContour call)
      - Clear temporary recording state
    - Implement LockContour logic
      - Validate contour has sufficient points (>=10)
      - Create ContourLine from recorded points
      - Set IsLocked = true, IsRecording = false
      - Emit StateChanged event (Locked)
      - Return ContourLine
    - Implement SetLocked logic
      - Toggle lock state
      - Emit StateChanged event (Locked/Unlocked)
  - [ ] 7.4 Implement ContourService class - Guidance Calculations
    - Implement CalculateOffset logic
      - Find closest point on contour to current position
      - Calculate perpendicular distance to contour segment
      - Return signed offset (positive = right, negative = left)
    - Implement CalculateGuidance logic
      - Find closest point on contour
      - Calculate cross-track error (offset from contour)
      - Calculate look-ahead point on contour for smooth following
      - Calculate heading to look-ahead point
      - Calculate heading error
      - Return GuidanceLineResult
      - Optimize for <5ms execution time
  - [ ] 7.5 Implement contour validation logic
    - ValidateContour(ContourLine contour) method
    - Check: Minimum points (>=10 for reliable following)
    - Check: Minimum total length (>10m)
    - Check: Point spacing consistency
    - Check: No duplicate consecutive points
    - Check: Contour doesn't self-intersect
    - Return ValidationResult
  - [ ] 7.6 Ensure Contour Service tests pass
    - Run ONLY the 2-8 tests written in 7.1
    - Verify recording respects minimum distance
    - Verify lock/unlock behavior
    - Do NOT run entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 7.1 pass
- Recording respects minimum distance threshold
- Lock requires sufficient points
- Offset calculation accurate
- Guidance calculation provides smooth following
- Execution time <5ms for CalculateGuidance

---

### Phase 5: Integration & Persistence

#### Task Group 8: Field Service Integration
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 3, 5, 7
**Estimated Effort:** 8 hours

- [x] 8.0 Extend FieldService for guidance line persistence
  - [x] 8.1 Write 2-8 focused tests for persistence operations
    - Test: SaveABLine persists to field file
    - Test: LoadABLine deserializes correctly
    - Test: SaveCurveLine persists smoothed curve
    - Test: LoadCurveLine with backward compatibility
    - Test: SaveContour persists recorded points
    - Test: LoadContour recreates contour accurately
    - Limit to 6 critical tests maximum
  - [x] 8.2 Design serialization format for guidance lines
    - Review existing AgOpenGPS field file format
    - Design JSON serialization for ABLine (or maintain binary compatibility)
    - Design JSON serialization for CurveLine
    - Design JSON serialization for ContourLine
    - Ensure backward compatibility with AgOpenGPS formats
    - Support versioning for future format changes
  - [x] 8.3 Extend FieldService with guidance line methods
    - Add SaveABLine(ABLine line, string fieldName) method
    - Add LoadABLine(string fieldName) method
    - Add SaveCurveLine(CurveLine curve, string fieldName) method
    - Add LoadCurveLine(string fieldName) method
    - Add SaveContour(ContourLine contour, string fieldName) method
    - Add LoadContour(string fieldName) method
    - Add DeleteGuidanceLine(string fieldName, GuidanceLineType type) method
  - [x] 8.4 Implement file I/O for guidance lines
    - Use existing FieldStreamer patterns from AgValoniaGPS
    - Store guidance lines in field-specific files:
      - ABLines: `FieldName.ABL`
      - Curves: `FieldName.CRV`
      - Contours: `FieldName.CNT`
    - Handle file locking for concurrent access
    - Implement error handling for corrupted files
  - [x] 8.5 Implement AgOpenGPS format migration support
    - Read existing AgOpenGPS ABLine files (binary format)
    - Convert to new format on load
    - Maintain original file as backup
    - Log migration success/failures
  - [x] 8.6 Ensure persistence tests pass
    - Run ONLY the 2-8 tests written in 8.1
    - Verify save/load round-trip accuracy
    - Verify backward compatibility
    - Do NOT run entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 8.1 pass
- Guidance lines serialize/deserialize correctly
- Backward compatibility with AgOpenGPS formats maintained
- File I/O handles errors gracefully
- No data loss on save/load round-trip

---

#### Task Group 9: Dependency Injection Registration
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 3, 5, 7
**Estimated Effort:** 2 hours

- [x] 9.0 Register Wave 2 services in DI container
  - [x] 9.1 Update ServiceCollectionExtensions.cs
    - Add IABLineService -> ABLineService registration (Scoped lifetime)
    - Add ICurveLineService -> CurveLineService registration (Scoped lifetime)
    - Add IContourService -> ContourService registration (Scoped lifetime)
    - Follow Wave 1 registration pattern
    - Add XML documentation for registration methods
  - [x] 9.2 Create service registration tests
    - Test: Services resolve correctly from container
    - Test: Service lifetimes correct (Scoped)
    - Test: No circular dependencies
    - Test: Services can be instantiated
  - [x] 9.3 Update ApplicationCore or startup configuration
    - Ensure Wave 2 services registered during app initialization
    - Verify no conflicts with existing service registrations
    - Test end-to-end service resolution

**Acceptance Criteria:**
- All Wave 2 services resolve from DI container
- Service lifetimes configured correctly
- No circular dependencies detected
- Registration tests pass

---

### Phase 6: Comprehensive Testing & Validation

#### Task Group 10: Test Review & Gap Analysis
**Assigned Implementer:** testing-engineer
**Dependencies:** Task Groups 3-9
**Estimated Effort:** 16 hours

- [ ] 10.0 Review existing tests and fill critical gaps only
  - [ ] 10.1 Review tests from Task Groups 3-9
    - Review ABLineService tests (2-8 tests from Tasks 3.1 and 4.1)
    - Review CurveLineService tests (2-8 tests from Tasks 5.1 and 6.1)
    - Review ContourService tests (2-8 tests from Task 7.1)
    - Review FieldService extension tests (2-8 tests from Task 8.1)
    - Total existing tests: approximately 24-48 tests
  - [ ] 10.2 Analyze test coverage gaps for Wave 2 features only
    - Identify critical workflows lacking test coverage
    - Focus ONLY on gaps related to Wave 2 guidance line requirements
    - Do NOT assess entire application test coverage
    - Prioritize edge cases and integration scenarios
    - Document gaps in coverage report
  - [ ] 10.3 Write up to 10 additional strategic tests maximum
    - **Edge Case Tests (Prioritized):**
      - Zero-length AB line creation (identical points) - should throw
      - AB line at exactly 0°, 90°, 180°, 270° headings
      - Curve with too few points (<3) - should fail validation
      - Curve with points too close together (<0.1m) - should warn
      - Contour lock with insufficient points (<10) - should fail
      - Contour following past end of recorded line - should handle gracefully
      - NaN/Infinity values in position inputs - should throw validation exception
      - Numeric overflow in distance calculations - should handle safely
    - **Performance Tests:**
      - XTE calculation sustained at 25 Hz for 60 seconds
      - Parallel line generation with 20 lines in <100ms
      - Curve smoothing with 2000 points in <500ms
    - **Integration Tests:**
      - End-to-end: Create AB line, calculate guidance, nudge, generate parallels
      - End-to-end: Record curve, smooth, generate parallels, calculate guidance
      - End-to-end: Record contour, lock, follow contour guidance
    - Maximum of 10 new tests total
    - Focus on critical gaps only
  - [ ] 10.4 Run Wave 2 feature-specific tests only
    - Run ONLY tests related to Wave 2 features
    - Expected total: approximately 34-58 tests maximum
    - Do NOT run entire application test suite
    - Verify all critical workflows pass
    - Generate coverage report for Wave 2 code only
    - Target: >80% coverage on Wave 2 services

**Acceptance Criteria:**
- All Wave 2 feature-specific tests pass (approximately 34-58 tests total)
- Critical edge cases covered with tests
- Performance benchmarks pass (<5ms XTE, <50ms parallels, <200ms smoothing)
- No more than 10 additional tests added by testing-engineer
- Test coverage >80% on Wave 2 services
- Testing focused exclusively on Wave 2 feature requirements

---

## Execution Order

**Recommended implementation sequence:**

1. **Phase 1: Data Models & Infrastructure** (Task Groups 1-2)
   - Foundation for all services
   - No dependencies
   - ~12 hours

2. **Phase 2: AB Line Service** (Task Groups 3-4)
   - Simplest service to implement
   - Establishes service patterns
   - ~28 hours

3. **Phase 3: Curve Line Service** (Task Groups 5-6)
   - More complex than AB Line
   - Requires MathNet.Numerics integration
   - ~36 hours

4. **Phase 4: Contour Service** (Task Groups 7)
   - Similar complexity to Curve Line
   - Can be implemented in parallel with Curve Line
   - ~12 hours

5. **Phase 5: Integration & Persistence** (Task Groups 8-9)
   - Requires all three services complete
   - ~10 hours

6. **Phase 6: Comprehensive Testing** (Task Group 10)
   - Final validation phase
   - ~16 hours

**Total Estimated Effort:** ~114 hours (approximately 2-3 weeks for one engineer)

**Critical Path Items:**
- Task Group 1 (Data Models) - blocks everything
- Task Group 3 (AB Line Core) - establishes service patterns
- Task Group 6 (Curve Smoothing) - MathNet.Numerics integration critical
- Task Group 8 (Persistence) - required for production use
- Task Group 10 (Testing) - required for release

---

## Performance Optimization Notes

**Key Performance Targets:**
- Cross-track error calculation: <5ms per call
- Parallel line generation: <50ms for 10 lines
- Curve smoothing: <200ms for 1000-point curve
- Memory footprint: <50MB per guidance line set
- No memory leaks over 8-hour operation

**Optimization Strategies:**
- Cache parallel lines (regenerate only when reference line changes)
- Use local search for closest point finding (vehicle moves sequentially)
- Perform curve smoothing once at creation, not on every update
- Use spatial indexing for curves with >1000 points
- Minimize allocations in hot paths (reuse arrays/lists)
- Use `Span<T>` and `Memory<T>` for zero-copy operations where beneficial
- Profile with BenchmarkDotNet for critical methods

---

## Standards Compliance

**All tasks must adhere to:**
- `agent-os/standards/global/coding-style.md` - Small focused functions, meaningful names, DRY principle
- `agent-os/standards/global/conventions.md` - C# naming conventions, SOLID principles
- `agent-os/standards/global/error-handling.md` - Validation, edge case handling
- `agent-os/standards/global/tech-stack.md` - .NET 8.0, C# 12, dependency injection
- `agent-os/standards/testing/test-writing.md` - Minimal tests during development, core flows only

**Key Standards:**
- Use interface-based design (IABLineService, ICurveLineService, IContourService)
- Follow dependency injection patterns from Wave 1
- XML documentation on all public APIs
- No UI framework dependencies
- Internal calculations in meters, convert at API boundary
- Event-driven architecture for loose coupling
- Comprehensive error handling and validation

---

## Migration from AgOpenGPS

**Source Code References:**
- `SourceCode/GPS/Classes/CABLine.cs` - Original AB line logic (behavioral reference)
- `SourceCode/GPS/Classes/CABCurve.cs` - Original curve logic (behavioral reference)
- `SourceCode/GPS/Classes/CContour.cs` - Original contour logic (behavioral reference)
- `SourceCode/GPS/Forms/Guidance/FormABLine.cs` - AB line UI (extract logic only)
- `SourceCode/GPS/Forms/Guidance/FormABCurve.cs` - Curve UI (extract logic only)

**Migration Approach:**
- **DO NOT** copy code directly from AgOpenGPS
- **DO** use AgOpenGPS as behavioral reference for algorithms
- **DO** improve upon AgOpenGPS methods (better curve smoothing with MathNet.Numerics)
- **DO** extract business logic from UI layer
- **DO** maintain file format compatibility for user migration
- **DO** implement modern patterns (DI, events, interfaces)

---

## Success Criteria Summary

**Code Quality:**
- All services implement interfaces
- Dependency injection registration complete
- Comprehensive XML documentation
- No UI framework dependencies
- All calculations in meters internally

**Testing:**
- >80% unit test coverage on all services
- All edge cases covered with tests
- Performance benchmarks pass
- No memory leaks in 8-hour test
- Thread-safety verified for concurrent access

**Functionality:**
- AB line creation from points and heading works correctly
- Cross-track error calculation accurate to ±1cm
- Parallel line generation maintains exact spacing (±2cm)
- Curve smoothing produces visually smooth paths using MathNet.Numerics
- Contour recording respects minimum distance threshold
- All events fire correctly on state changes

**Performance:**
- Cross-track error calculation: <5ms at 25 Hz
- Parallel line generation: <50ms for 10 lines
- Curve smoothing: <200ms for 1000-point curve
- Memory footprint: <50MB per guidance line set
- No performance degradation over 8-hour operation

**Integration:**
- Services integrate with FieldService for persistence
- Position/heading passed as method parameters (loose coupling)
- Events consumed by test subscribers successfully
- Metric/imperial unit handling works correctly
- AgOpenGPS field file format compatibility maintained

---

## File Structure

**New directories to create:**
```
SourceCode/AgOpenGPS.Core/Services/Guidance/
  - IABLineService.cs
  - ABLineService.cs
  - ICurveLineService.cs
  - CurveLineService.cs
  - IContourService.cs
  - ContourService.cs

SourceCode/AgOpenGPS.Core/Models/Guidance/
  - ABLine.cs
  - CurveLine.cs
  - ContourLine.cs
  - GuidanceLineResult.cs
  - SmoothingParameters.cs
  - UnitSystem.cs
  - ValidationResult.cs

SourceCode/AgOpenGPS.Core/Events/Guidance/
  - ABLineChangedEventArgs.cs
  - CurveLineChangedEventArgs.cs
  - ContourStateChangedEventArgs.cs
  - GuidanceStateChangedEventArgs.cs

SourceCode/AgOpenGPS.Core.Tests/Services/Guidance/
  - ABLineServiceTests.cs
  - CurveLineServiceTests.cs
  - ContourServiceTests.cs
  - GuidanceIntegrationTests.cs
  - GuidancePerformanceTests.cs
```

**Files to modify:**
```
SourceCode/AgOpenGPS.Core/DependencyInjection/ServiceCollectionExtensions.cs
  - Add Wave 2 service registrations

SourceCode/AgOpenGPS.Core/Services/Field/FieldService.cs (or equivalent)
  - Add guidance line persistence methods
```

---

## Notes

- Wave 1 services (PositionUpdateService, HeadingCalculatorService, VehicleKinematicsService) are complete with 68/68 tests passing
- This wave focuses exclusively on guidance line services - UI integration is Wave 3
- Performance optimization is critical - target 20-25 Hz operation
- MathNet.Numerics integration required for quality curve smoothing
- Maintain AgOpenGPS file format compatibility for user migration
- Thread safety required for concurrent access from UI and calculation threads
