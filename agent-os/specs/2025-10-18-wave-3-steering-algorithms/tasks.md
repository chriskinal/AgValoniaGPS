# Task Breakdown: Wave 3 Steering Algorithms

## Overview
Total Tasks: 4 major task groups with 21 sub-tasks
Assigned roles: api-engineer, testing-engineer
Estimated Total Effort: 12-16 hours

## Context
This wave extracts and modernizes ~1,500 LOC of steering control logic from AgOpenGPS legacy codebase into clean, testable services. The implementation must achieve 100Hz performance (10ms max per calculation) and support real-time algorithm switching between Stanley and Pure Pursuit steering algorithms.

**Dependencies:**
- Wave 1 Complete: PositionUpdateService, VehicleKinematicsService
- Wave 2 Complete: ABLineService, CurveLineService, ContourService
- Existing Infrastructure: UdpCommunicationService, VehicleConfiguration, PgnMessage

**Critical Constraints:**
- Services must be flat in `AgValoniaGPS.Services/Guidance/` directory (no Steering/ subdirectory per NAMING_CONVENTIONS.md)
- 100Hz performance target (10ms max per calculation)
- Thread-safe for concurrent UI and guidance loop access
- Real-time algorithm switching without disruption
- All parameters exposed via VehicleConfiguration for UI tuning

## Task List

### Task Group 1: Look-Ahead Distance Service
**Assigned implementer:** api-engineer
**Dependencies:** None (uses existing VehicleConfiguration)
**Estimated Effort:** 2-3 hours
**Complexity:** Low-Medium

- [ ] 1.0 Complete LookAheadDistanceService implementation
  - [ ] 1.1 Write 4-6 focused tests for look-ahead calculations
    - Test base distance calculation (speed * multiplier)
    - Test cross-track error zones (on-line ≤0.1m, transition 0.1-0.4m, acquire ≥0.4m)
    - Test minimum distance enforcement (2.0m floor)
    - Test vehicle type scaling (combine 10% larger, tractor baseline)
    - Test curvature adaptation (20% reduction when curvature high)
    - Test AutoSteer off mode (fixed 5.0 hold distance)
  - [ ] 1.2 Create ILookAheadDistanceService interface
    - Method: `CalculateLookAheadDistance(speed, xte, curvature, vehicleType, isAutoSteerActive)`
    - Property: `LookAheadMode Mode { get; set; }`
    - Return type: double (meters)
  - [ ] 1.3 Implement LookAheadDistanceService class
    - Extract algorithm from existing GuidanceService.CalculateGoalPointDistance()
    - Add curvature adaptation (reduce 20% when curvature > threshold)
    - Add vehicle type scaling (VehicleType.Harvester: 1.1x, others: 1.0x)
    - Support three modes: ToolWidthMultiplier, TimeBased, Hold
    - Use VehicleConfiguration for all parameters (GoalPointLookAheadHold, GoalPointAcquireFactor, MinLookAheadDistance)
  - [ ] 1.4 Add LookAheadMode enum to AgValoniaGPS.Models/Guidance/
    - ToolWidthMultiplier (default)
    - TimeBased
    - Hold (constant distance)
  - [ ] 1.5 Ensure look-ahead service tests pass
    - Run ONLY the 4-6 tests written in 1.1
    - Verify all three distance zones work correctly
    - Verify minimum distance enforcement
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 4-6 tests written in 1.1 pass
- Look-ahead distance adapts correctly to speed, XTE, and curvature
- Minimum 2.0m distance enforced
- Vehicle type scaling applied correctly
- Performance: <1ms calculation time

---

### Task Group 2: Stanley Steering Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 (for integration testing)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

- [ ] 2.0 Complete StanleySteeringService implementation
  - [ ] 2.1 Write 5-7 focused tests for Stanley algorithm
    - Test basic formula: steerAngle = headingError * K_h + atan(K_d * xte / speed)
    - Test heading error component (vary heading error -π to π)
    - Test cross-track error component (vary XTE -2m to 2m)
    - Test speed adaptation (verify 1 + 0.277*(speed-1) scaling for speed > 1 m/s)
    - Test integral control (accumulates pivot distance error, resets on threshold)
    - Test reverse mode (heading error negated)
    - Test angle limiting (clamp to ±maxSteerAngle)
  - [ ] 2.2 Create IStanleySteeringService interface
    - Method: `CalculateSteeringAngle(xte, headingError, speed, pivotDistanceError, isReverse)`
    - Method: `ResetIntegral()`
    - Property: `double IntegralValue { get; }`
  - [ ] 2.3 Implement StanleySteeringService class
    - Extract and refactor from GuidanceService.CalculateStanleySteering()
    - Heading component: `headingError * StanleyHeadingErrorGain`
    - Distance component: `atan(StanleyDistanceErrorGain * xte / speedMs)`
    - Integral control: accumulate pivot error when |pivotError| > IntegralDistanceAwayTriggerAB
    - Speed adaptation: scale cross-track gain by `1 + 0.277 * (speed - 1)` when speed > 1 m/s
    - Reverse mode: negate heading error when isReverse = true
    - Angle limiting: clamp output to ±MaxSteerAngle from VehicleConfiguration
  - [ ] 2.4 Add integral control with reset logic
    - Accumulate pivotDistanceError * StanleyIntegralGainAB
    - Reset integral when heading error reverses sign (crossing line)
    - Reset integral when AutoSteer disabled
    - Expose ResetIntegral() method for external control
  - [ ] 2.5 Add thread safety for integral state
    - Use lock for integral accumulator access
    - Ensure CalculateSteeringAngle() is thread-safe
  - [ ] 2.6 Ensure Stanley service tests pass
    - Run ONLY the 5-7 tests written in 2.1
    - Verify formula correctness with known inputs
    - Verify integral accumulation and reset
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 5-7 tests written in 2.1 pass
- Stanley formula matches specification exactly
- Integral control accumulates and resets correctly
- Thread-safe for concurrent access
- Performance: <2ms calculation time
- Reverse mode negates heading error correctly

---

### Task Group 3: Pure Pursuit Steering Service
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1 (look-ahead distance)
**Estimated Effort:** 3-4 hours
**Complexity:** Medium

- [ ] 3.0 Complete PurePursuitService implementation
  - [ ] 3.1 Write 5-7 focused tests for Pure Pursuit algorithm
    - Test goal point calculation on guidance line (at look-ahead distance)
    - Test alpha angle calculation (vehicle heading to goal point)
    - Test curvature formula: curvature = 2 * sin(alpha) / lookAheadDistance
    - Test steering angle formula: steerAngle = atan(curvature * wheelbase)
    - Test integral control (accumulates pivot distance error)
    - Test zero look-ahead protection (return 0 when lookAhead < 0.1m)
    - Test angle limiting (clamp to ±maxSteerAngle)
  - [ ] 3.2 Create IPurePursuitService interface
    - Method: `CalculateSteeringAngle(steerAxlePosition, goalPoint, speed, pivotDistanceError)`
    - Method: `CalculateGoalPoint(guidanceLine, steerAxlePosition, lookAheadDistance)`
    - Method: `ResetIntegral()`
    - Property: `double IntegralValue { get; }`
  - [ ] 3.3 Implement PurePursuitService class
    - Extract from GuidanceService.CalculatePurePursuitSteering()
    - CalculateGoalPoint: find point on guidance line at lookAheadDistance from steer axle
    - Calculate alpha: `atan2(goalPoint.Y - steerY, goalPoint.X - steerX)`
    - Calculate curvature: `2 * sin(alpha) / lookAheadDistance`
    - Calculate steer angle: `atan(curvature * wheelbase)`
    - Zero-distance protection: return 0 if lookAheadDistance < 0.1
  - [ ] 3.4 Add integral control (same as Stanley)
    - Accumulate pivotDistanceError * PurePursuitIntegralGain
    - Reset when heading error changes sign
    - Reset when AutoSteer disabled
    - Expose ResetIntegral() method
  - [ ] 3.5 Add thread safety for integral state
    - Use lock for integral accumulator access
    - Ensure CalculateSteeringAngle() is thread-safe
  - [ ] 3.6 Ensure Pure Pursuit service tests pass
    - Run ONLY the 5-7 tests written in 3.1
    - Verify goal point calculations with known geometry
    - Verify curvature and steering angle formulas
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 5-7 tests written in 3.1 pass
- Goal point calculation correct for AB lines and curves
- Curvature and steering formulas match specification
- Integral control works identically to Stanley
- Thread-safe for concurrent access
- Performance: <3ms calculation time (includes goal point finding)

---

### Task Group 4: Steering Coordinator & PGN Output
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1, 2, 3
**Estimated Effort:** 3-4 hours
**Complexity:** Medium-High

- [ ] 4.0 Complete SteeringCoordinatorService implementation
  - [ ] 4.1 Write 4-6 focused tests for coordinator
    - Test algorithm routing (Stanley vs Pure Pursuit selection)
    - Test PGN 254 message format (AutoSteer Data)
    - Test real-time algorithm switching (no disruption)
    - Test integral reset when switching algorithms
    - Test UDP transmission via UdpCommunicationService
    - Test event publishing (SteeringUpdated with current values)
  - [ ] 4.2 Create ISteeringCoordinatorService interface
    - Property: `SteeringAlgorithm ActiveAlgorithm { get; set; }`
    - Method: `Update(pivotPos, steerPos, guidanceResult, speed, heading, isAutoSteerActive)`
    - Property: `double CurrentSteeringAngle { get; }`
    - Property: `double CurrentCrossTrackError { get; }`
    - Property: `double CurrentLookAheadDistance { get; }`
    - Event: `EventHandler<SteeringUpdateEventArgs> SteeringUpdated`
  - [ ] 4.3 Implement SteeringCoordinatorService class
    - Inject: IStanleySteeringService, IPurePursuitService, ILookAheadDistanceService, UdpCommunicationService
    - Route calculations to active algorithm based on ActiveAlgorithm property
    - Calculate look-ahead distance using LookAheadDistanceService
    - For Pure Pursuit: calculate goal point from guidance line + look-ahead
    - For Stanley: use cross-track error and heading error directly
    - Apply dead-zone logic (from existing GuidanceService.UpdateDeadZone)
  - [ ] 4.4 Implement PGN 254 message construction
    - Format: Header (0x80, 0x81), Source (0x7F), PGN (254)
    - Byte 5-6: speed * 10 (uint16, big-endian)
    - Byte 7: status (0=off, 1=on)
    - Byte 8-9: steer angle * 100 (int16, big-endian)
    - Byte 10-11: cross-track error in mm (int16, big-endian)
    - Byte 14: CRC (use existing PgnMessage.CalculateCrc)
    - Send via UdpCommunicationService.SendAsync() to port 9999
  - [ ] 4.5 Implement algorithm switching logic
    - When ActiveAlgorithm changes: reset integrals in both services
    - Maintain state continuity (no steering jump on switch)
    - Thread-safe property setter
  - [ ] 4.6 Create SteeringUpdateEventArgs in AgValoniaGPS.Models/Guidance/
    - Properties: SteeringAngle, CrossTrackError, LookAheadDistance, Algorithm, Timestamp
  - [ ] 4.7 Ensure coordinator tests pass
    - Run ONLY the 4-6 tests written in 4.1
    - Verify PGN message format matches specification
    - Verify algorithm switching resets integrals
    - Do NOT run entire test suite

**Acceptance Criteria:**
- The 4-6 tests written in 4.1 pass
- Algorithm routing works correctly (Stanley vs Pure Pursuit)
- PGN 254 messages formatted correctly (verified with byte-level tests)
- Real-time algorithm switching works without steering jump
- UDP transmission verified (mock UdpCommunicationService in tests)
- SteeringUpdated event published with correct data
- Performance: <5ms total Update() execution time

---

### Task Group 5: Integration Testing & Performance Validation
**Assigned implementer:** testing-engineer
**Dependencies:** Task Groups 1-4
**Estimated Effort:** 3-4 hours
**Complexity:** Medium-High

- [ ] 5.0 Review existing tests and add integration scenarios
  - [ ] 5.1 Review tests from Task Groups 1-4
    - Review 4-6 tests from LookAheadDistanceService (1.1)
    - Review 5-7 tests from StanleySteeringService (2.1)
    - Review 5-7 tests from PurePursuitService (3.1)
    - Review 4-6 tests from SteeringCoordinatorService (4.1)
    - Total existing tests: approximately 18-26 tests
  - [ ] 5.2 Analyze integration test coverage gaps
    - Identify end-to-end workflow gaps (GPS position → steering command → PGN output)
    - Focus on Wave 1 & Wave 2 integration (PositionUpdateService, VehicleKinematicsService, ABLineService)
    - Identify edge case gaps (tight curves, U-turns, GPS loss)
    - Do NOT assess entire application coverage
  - [ ] 5.3 Write up to 10 additional integration tests maximum
    - Full guidance loop: GPS position → kinematics → guidance line → steering → PGN (2-3 tests)
    - Edge case: tight curve (radius < 10m) with look-ahead reduction (1 test)
    - Edge case: U-turn at headland with heading error wrapping ±π (1 test)
    - Edge case: sudden course correction with integral reset (1 test)
    - Edge case: zero speed protection (division-by-zero safety) (1 test)
    - Edge case: reverse mode with heading error negation (1 test)
    - Vehicle type variations: tractor vs combine vs articulated (1-2 tests)
    - Algorithm switching during active guidance (1 test)
  - [ ] 5.4 Create performance benchmark tests
    - Benchmark: 1000 Stanley calculations in <1000ms (proves 100Hz capable)
    - Benchmark: 1000 Pure Pursuit calculations in <3000ms
    - Benchmark: Full guidance loop iteration in <10ms
    - Memory: no allocations in hot path (use BenchmarkDotNet or manual Stopwatch)
  - [ ] 5.5 Run feature-specific tests only
    - Run tests from 1.1, 2.1, 3.1, 4.1, and 5.3
    - Expected total: approximately 28-36 tests maximum
    - Do NOT run entire application test suite
    - Verify all critical workflows pass
  - [ ] 5.6 Validate performance benchmarks
    - Verify 100Hz capability (10ms max per iteration)
    - Verify no allocations in Calculate methods
    - Document performance results in test output

**Acceptance Criteria:**
- All feature-specific tests pass (approximately 28-36 tests total)
- No more than 10 additional tests added by testing-engineer
- Full guidance loop completes in <10ms (verified by benchmark)
- 1000 steering calculations in <1 second (proves 100Hz capable)
- Edge cases handled without errors or NaN values
- Algorithm switching works seamlessly during active guidance
- Testing focused exclusively on Wave 3 feature requirements

---

## Execution Order

**Recommended implementation sequence:**

1. **Task Group 1: Look-Ahead Distance Service** (2-3 hours)
   - Foundation for Pure Pursuit algorithm
   - Independent component, can be built first
   - Tests verify adaptive behavior

2. **Parallel Execution:**
   - **Task Group 2: Stanley Steering Service** (3-4 hours)
   - **Task Group 3: Pure Pursuit Service** (3-4 hours)
   - Both can be developed in parallel since they're independent
   - Both depend on VehicleConfiguration but not on each other

3. **Task Group 4: Steering Coordinator** (3-4 hours)
   - Depends on Groups 1, 2, 3 being complete
   - Integrates all services
   - Adds PGN output and algorithm switching

4. **Task Group 5: Integration Testing** (3-4 hours)
   - Depends on Groups 1-4 being complete
   - Validates end-to-end workflows
   - Performance benchmarking

**Critical Path:** 1 → (2 || 3) → 4 → 5
**Estimated Total Time:** 12-16 hours (with parallel execution of Groups 2 & 3)

---

## Service Registration

All new services must be registered in `AgValoniaGPS.Core/ServiceCollectionExtensions.cs`:

```csharp
// Steering services
services.AddSingleton<ILookAheadDistanceService, LookAheadDistanceService>();
services.AddSingleton<IStanleySteeringService, StanleySteeringService>();
services.AddSingleton<IPurePursuitService, PurePursuitService>();
services.AddSingleton<ISteeringCoordinatorService, SteeringCoordinatorService>();
```

---

## File Organization

All files follow `NAMING_CONVENTIONS.md`:

```
AgValoniaGPS.Services/Guidance/
├── LookAheadDistanceService.cs        (Task Group 1)
├── ILookAheadDistanceService.cs       (Task Group 1)
├── StanleySteeringService.cs          (Task Group 2)
├── IStanleySteeringService.cs         (Task Group 2)
├── PurePursuitService.cs              (Task Group 3)
├── IPurePursuitService.cs             (Task Group 3)
├── SteeringCoordinatorService.cs      (Task Group 4)
├── ISteeringCoordinatorService.cs     (Task Group 4)

AgValoniaGPS.Models/Guidance/
├── LookAheadMode.cs                   (Task Group 1)
├── SteeringUpdateEventArgs.cs         (Task Group 4)

AgValoniaGPS.Services.Tests/Guidance/
├── LookAheadDistanceServiceTests.cs   (Task Group 1)
├── StanleySteeringServiceTests.cs     (Task Group 2)
├── PurePursuitServiceTests.cs         (Task Group 3)
├── SteeringCoordinatorServiceTests.cs (Task Group 4)
├── SteeringIntegrationTests.cs        (Task Group 5)
├── SteeringPerformanceTests.cs        (Task Group 5)
```

---

## Performance Requirements

Each service must meet these performance targets:

- **LookAheadDistanceService.CalculateLookAheadDistance()**: <1ms
- **StanleySteeringService.CalculateSteeringAngle()**: <2ms
- **PurePursuitService.CalculateSteeringAngle()**: <3ms (includes goal point calculation)
- **SteeringCoordinatorService.Update()**: <5ms total
- **Full guidance loop (GPS → PGN)**: <10ms (100Hz capable)

Performance verification included in Task Group 5.

---

## Testing Constraints

Per `agent-os/standards/testing/test-writing.md`:

- **Minimal tests during development**: Each task group writes 2-8 focused tests only
- **Test critical paths only**: Focus on core algorithms and formulas
- **Defer edge cases to Task Group 5**: testing-engineer adds maximum 10 integration tests
- **No comprehensive coverage**: Skip exhaustive testing of all scenarios
- **Fast execution**: All tests must complete in <5 seconds total

**Total expected tests for Wave 3:** Approximately 28-36 tests maximum

---

## Integration with Existing Code

**Deprecate after Wave 3 complete:**
- `GuidanceService.CalculateGoalPointDistance()` → Use LookAheadDistanceService
- `GuidanceService.CalculateStanleySteering()` → Use StanleySteeringService
- `GuidanceService.CalculatePurePursuitSteering()` → Use PurePursuitService
- `GuidanceService.UpdateDeadZone()` → Move to SteeringCoordinatorService

**Preserve:**
- VehicleConfiguration (all parameters already present)
- UdpCommunicationService (used for PGN transmission)
- PgnMessage (used for message construction)
- PositionUpdateService, VehicleKinematicsService (Wave 1)
- ABLineService, CurveLineService, ContourService (Wave 2)

---

## Success Criteria

- ✅ All four services implemented with complete interfaces
- ✅ Approximately 28-36 tests pass (including integration tests)
- ✅ Performance benchmark: 1000 steering calculations in <1 second
- ✅ Integration test: Full guidance loop completes in <10ms
- ✅ Edge case tests: Tight curves, U-turns, reverse mode all pass
- ✅ Algorithm switching: Real-time toggle works without steering jump
- ✅ PGN output: Messages formatted correctly and transmitted via UDP
- ✅ Thread safety: Concurrent UI and guidance loop access verified

---

Last Updated: 2025-10-18
Wave: 3 - Steering Algorithms
