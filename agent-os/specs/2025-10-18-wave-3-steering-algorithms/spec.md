# Specification: Wave 3 Steering Algorithms

## Goal
Extract and modernize steering control algorithms from AgOpenGPS legacy codebase (~1,500 LOC) into clean, testable services that calculate steering angles using Stanley and Pure Pursuit algorithms, with real-time algorithm switching and 100Hz performance capability.

## User Stories
- As a tractor operator, I want to select between Stanley and Pure Pursuit steering algorithms so that I can choose the best approach for my field conditions
- As a tractor operator, I want the steering to adapt look-ahead distance based on speed and cross-track error so that I get smooth guidance in varying conditions
- As a developer, I want steering algorithms separated into testable services so that I can verify correctness and performance
- As a field technician, I want to tune steering parameters via UI so that I can optimize guidance for different vehicles and implements
- As a tractor operator, I want to switch steering algorithms in real-time during operation so that I can compare performance without stopping work

## Core Requirements

### Functional Requirements
- Calculate steering angles using Stanley algorithm (cross-track error + heading error based)
- Calculate steering angles using Pure Pursuit algorithm (look-ahead point based)
- Adapt look-ahead distance based on vehicle speed, cross-track error, guidance line curvature, and vehicle type
- Allow real-time switching between Stanley and Pure Pursuit algorithms during operation
- Send steering commands via PGN messages over UDP to AutoSteer module
- Expose all tuning parameters for UI configuration
- Support multiple vehicle types (tractor, harvester, articulated)
- Maintain integral control for both algorithms to eliminate steady-state error
- Handle edge cases: tight curves, U-turns, headlands, sudden course corrections

### Non-Functional Requirements
- **Performance**: Calculate steering angle in <10ms per iteration (100Hz capable)
- **Thread Safety**: Services must be thread-safe for concurrent UI and guidance loop access
- **Numerical Stability**: Handle edge cases without NaN/infinity errors
- **Accuracy**: Match or exceed original AgOpenGPS steering performance
- **Testability**: All algorithms must be unit testable with known input/output scenarios

## Visual Design
- Mockup reference: `planning/visuals/Screenshot 2025-10-18 at 6.04.46 AM.png` - Shows vehicle following guidance line with purple cross-track error visualization
- Mockup reference: `planning/visuals/Screenshot 2025-10-18 at 6.06.11 AM.png` - Shows multiple guidance lines and vehicle position
- Key UI elements visible in screenshots:
  - Real-time cross-track error line (purple) from vehicle to guidance path
  - Vehicle sprite with heading orientation
  - Guidance lines (yellow/green)
  - Bottom control panel for algorithm selection and parameter tuning

## Reusable Components

### Existing Code to Leverage

**From Wave 1 (Position & Kinematics):**
- `PositionUpdateService` - Provides current GPS position, heading, and speed
- `VehicleKinematicsService` - Transforms GPS antenna position to pivot and steer axle positions
- `Position2D`, `Position3D` models - Standard position data structures

**From Wave 2 (Guidance Lines):**
- `ABLineService` - Provides AB line geometry and closest point calculations
- `CurveLineService` - Provides curve line geometry
- `ContourService` - Provides contour line guidance
- `GuidanceLineResult` model - Standard cross-track error and closest point data

**Existing Infrastructure:**
- `VehicleConfiguration` model - Already has steering parameters (StanleyDistanceErrorGain, StanleyHeadingErrorGain, etc.)
- `UdpCommunicationService` - Handles UDP communication on port 9999
- `PgnMessage` model - Standard PGN message format with ToBytes() method
- `PgnNumbers` constants - Includes AUTOSTEER_DATA (253) and AUTOSTEER_DATA2 (254)

**Legacy Reference Implementation:**
- `SourceCode/GPS/Classes/CGuidance.cs` - Stanley algorithm (lines 40-411)
- `SourceCode/GPS/Classes/CVehicle.cs` - Look-ahead distance calculation (lines 105-139)
- `SourceCode/GPS/Forms/Position.designer.cs` - AutoSteer PGN output (lines 871-949)

**Existing Partial Implementation:**
- `AgValoniaGPS.Services/GuidanceService.cs` - Has basic Stanley/Pure Pursuit stubs (needs refactoring into separate services)

### New Components Required

**StanleySteeringService** - Dedicated service for Stanley algorithm
- Cannot reuse existing GuidanceService - that service mixes concerns and lacks proper integral control
- Needs separate service to allow algorithm switching without conditionals

**PurePursuitService** - Dedicated service for Pure Pursuit algorithm
- Pure Pursuit requires goal point calculation on guidance line (not in existing services)
- Different mathematical approach from Stanley requires separate implementation

**LookAheadDistanceService** - Adaptive look-ahead calculator
- Existing GuidanceService has basic speed-based calculation but missing:
  - Curvature adaptation (reduce look-ahead in tight curves)
  - Vehicle type consideration (different for tractors vs combines)
  - Multiple modes (tool width multiplier, time-based, hold/constant)

**SteeringCoordinatorService** - Manages algorithm selection and PGN output
- No existing coordinator for switching between algorithms
- Needed to route calculations to active algorithm and send PGN messages

## Technical Approach

### Database
No database changes required - all parameters stored in VehicleConfiguration model (already exists).

### API / Service Interfaces

**IStanleySteeringService**
```csharp
public interface IStanleySteeringService
{
    double CalculateSteeringAngle(
        double crossTrackError,
        double headingError,
        double speed,
        double pivotDistanceError,
        bool isReverse);

    void ResetIntegral();
    double IntegralValue { get; }
}
```

**IPurePursuitService**
```csharp
public interface IPurePursuitService
{
    double CalculateSteeringAngle(
        Position3D steerAxlePosition,
        Position3D goalPoint,
        double speed,
        double pivotDistanceError);

    void ResetIntegral();
    double IntegralValue { get; }
}
```

**ILookAheadDistanceService**
```csharp
public interface ILookAheadDistanceService
{
    double CalculateLookAheadDistance(
        double speed,
        double crossTrackError,
        double guidanceLineCurvature,
        VehicleType vehicleType,
        bool isAutoSteerActive);

    LookAheadMode Mode { get; set; }
}

public enum LookAheadMode
{
    ToolWidthMultiplier,  // Distance = speed * multiplier + toolWidth
    TimeBased,            // Distance = speed * lookAheadTime
    Hold                  // Constant distance
}
```

**ISteeringCoordinatorService**
```csharp
public interface ISteeringCoordinatorService
{
    SteeringAlgorithm ActiveAlgorithm { get; set; }

    void Update(
        Position3D pivotPosition,
        Position3D steerPosition,
        GuidanceLineResult guidanceResult,
        double speed,
        double heading,
        bool isAutoSteerActive);

    double CurrentSteeringAngle { get; }
    double CurrentCrossTrackError { get; }
    double CurrentLookAheadDistance { get; }

    event EventHandler<SteeringUpdateEventArgs> SteeringUpdated;
}
```

### Algorithm Implementations

**Stanley Steering Algorithm**
- Formula: `steerAngle = headingError * K_heading + atan(K_distance * xte / speed)`
- Heading error component: Difference between vehicle heading and guidance line heading
- Cross-track error component: Distance from steer axle to guidance line, normalized by speed
- Integral control: Applied to pivot distance error to eliminate steady-state offset
- Speed adaptation: Scale cross-track gain by speed (1 + 0.277*(speed-1) for speed > 1 m/s)
- Reverse mode: Negate heading error when vehicle in reverse
- Side-hill compensation: Add IMU roll * compensation factor to final angle
- Angle limiting: Clamp output to ±maxSteerAngle

**Pure Pursuit Algorithm**
- Goal point: Position on guidance line at look-ahead distance from steer axle
- Curvature calculation: `curvature = 2 * sin(alpha) / lookAheadDistance`
- Alpha: Angle from vehicle heading to goal point
- Steering angle: `steerAngle = atan(curvature * wheelbase)`
- Integral control: Applied to pivot distance error (same as Stanley)
- Look-ahead distance: From LookAheadDistanceService (adaptive)

**Look-Ahead Distance Calculation**
- Base distance: `speed * 0.05 * lookAheadMult`
- Cross-track error zones:
  - xte <= 0.1m: Use hold distance (on-line mode)
  - 0.1m < xte < 0.4m: Interpolate between hold and acquire distance
  - xte >= 0.4m: Use acquire distance (acquisition mode)
- Hold distance factor: `lookAheadHold` parameter (default 4.0)
- Acquire distance factor: `lookAheadHold * acquireFactor` (default 4.0 * 1.5 = 6.0)
- Curvature adaptation: Reduce look-ahead by 20% when curvature > threshold
- Vehicle type scaling: Combine 10% larger look-ahead, tractor 0% adjustment
- Minimum enforcement: Never less than 2.0 meters

**PGN Message Format (AutoSteer Data - PGN 254)**
```
Byte 0-1: Header (0x80, 0x81)
Byte 2: Source (0x7F)
Byte 3: PGN (254 / 0xFE)
Byte 4: Length (10 bytes)
Byte 5: speedHi - High byte of speed * 10
Byte 6: speedLo - Low byte of speed * 10
Byte 7: status - 0=off, 1=on
Byte 8: steerAngleHi - High byte of steer angle * 100
Byte 9: steerAngleLo - Low byte of steer angle * 100
Byte 10: distanceHi - High byte of cross-track error (mm)
Byte 11: distanceLo - Low byte of cross-track error (mm)
Byte 12-13: Reserved
Byte 14: CRC
```

### File Structure

**IMPORTANT:** All services follow `NAMING_CONVENTIONS.md` to prevent namespace collisions:
- Services placed **flat** in `AgValoniaGPS.Services/Guidance/` directory
- **No** `Steering/` subdirectory (would require namespace `AgValoniaGPS.Services.Guidance.Steering`)
- Avoid directory names that conflict with class names from `AgValoniaGPS.Models`

```
AgValoniaGPS.Services/Guidance/
├── StanleySteeringService.cs          (Stanley algorithm implementation)
├── IStanleySteeringService.cs         (Stanley interface)
├── PurePursuitService.cs              (Pure Pursuit implementation)
├── IPurePursuitService.cs             (Pure Pursuit interface)
├── LookAheadDistanceService.cs        (Adaptive look-ahead calculator)
├── ILookAheadDistanceService.cs       (Look-ahead interface)
├── SteeringCoordinatorService.cs      (Algorithm coordinator & PGN output)
├── ISteeringCoordinatorService.cs     (Coordinator interface)

AgValoniaGPS.Models/Guidance/
├── SteeringUpdateEventArgs.cs         (Event args for steering updates)
├── LookAheadMode.cs                   (Look-ahead mode enum - if not using existing)

AgValoniaGPS.Services.Tests/Guidance/
├── StanleySteeringServiceTests.cs     (Stanley tests)
├── PurePursuitServiceTests.cs         (Pure Pursuit tests)
├── LookAheadDistanceServiceTests.cs   (Look-ahead tests)
├── SteeringCoordinatorServiceTests.cs (Integration tests)
```

### Integration Points

**Inputs (from Wave 1 & Wave 2):**
- Current position from `PositionUpdateService`
- Pivot and steer axle positions from `VehicleKinematicsService`
- Cross-track error and guidance line heading from AB/Curve/Contour services
- Vehicle speed and heading from GPS data
- Vehicle configuration parameters from `VehicleConfiguration`

**Outputs:**
- Steering angle command to AutoSteer module via UDP (PGN 254)
- Cross-track error value (for UI display)
- Look-ahead distance (for UI display)
- Steering update events for UI visualization

**Configuration:**
- All tuning parameters read from `VehicleConfiguration` model
- UI bindings use ReactiveUI to update parameters in real-time
- Algorithm selection persisted in user settings

### Testing

**Unit Test Coverage:**
- Stanley: Test known xte/heading combinations, verify formula correctness
- Pure Pursuit: Test goal point calculations, verify curvature math
- Look-ahead: Test all three distance zones, vehicle type scaling
- Coordinator: Test algorithm switching, PGN message format

**Performance Benchmarks:**
- Each service Calculate method must complete in <10ms
- 1000 iterations in <10 seconds (proves 100Hz capability)
- No allocations in hot path (use value types, avoid LINQ)

**Edge Case Scenarios:**
- Tight curves (radius < 10m): Verify look-ahead reduction
- U-turns at headlands: Test heading error wrapping (±π)
- Sudden course corrections: Test integral reset logic
- GPS loss recovery: Test graceful degradation when position invalid
- Different vehicle configs: Test tractor vs combine vs articulated
- Zero speed: Test division-by-zero protection
- Reverse mode: Test heading error negation

## Out of Scope
Nothing explicitly excluded - all advanced features are in scope for this wave per user requirements.

## Success Criteria
- All four services implemented with full interfaces
- Unit tests achieve >90% code coverage
- Performance benchmark: 100 steering calculations in <1 second
- Integration test: Full guidance loop from GPS position to PGN output completes in <10ms
- Edge case tests: All scenarios pass without errors or NaN values
- Algorithm switching: User can toggle between Stanley and Pure Pursuit during active guidance without disruption
- Parameter tuning: All 10+ tuning parameters accessible and functional in UI
- Real-world validation: Steering performance matches or exceeds original AgOpenGPS on test field
