# Specification: Business Logic Extraction from AgOpenGPS to AgValoniaGPS

## 1. Executive Summary

### Project Overview
This specification defines a comprehensive, systematic approach to extract approximately 20,000 lines of embedded business logic from the AgOpenGPS WinForms application into clean, testable, UI-agnostic services for AgValoniaGPS. The project addresses the fundamental architectural limitation that prevents cross-platform deployment, comprehensive testing, and long-term maintainability.

### Goals and Objectives
- Extract all business logic from 65+ WinForms files into dedicated service classes
- Create interface-based, dependency-injected service architecture
- Achieve >80% unit test coverage on extracted services
- Maintain 100% behavioral compatibility with original AgOpenGPS
- Enable true cross-platform support (Windows, Linux, macOS)
- Establish foundation for Qt, web, and mobile UI implementations

### Timeline
**Total Duration**: 10-12 weeks (8 waves of extraction)
- Wave 0: Already completed (GPS, Field I/O, UDP, NTRIP)
- Wave 1: Weeks 1-2 (Position & Kinematics)
- Wave 2: Weeks 3-4 (Guidance Lines)
- Wave 3: Weeks 4-5 (Steering Algorithms)
- Wave 4: Weeks 5-6 (Section Control & Coverage)
- Wave 5: Weeks 6-7 (Field Operations)
- Wave 6: Weeks 7-8 (Hardware Communication)
- Wave 7: Weeks 8-9 (Display & Visualization)
- Wave 8: Weeks 9-10 (State Management)

### Success Criteria
1. Zero UI framework dependencies in service layer
2. All services behind interfaces with dependency injection
3. Test coverage >80% on extracted services
4. All original functionality preserved and validated
5. Performance equal to or better than original
6. Services compile and run on .NET 8.0 on all platforms
7. Complete API documentation for all services
8. Migration guide documenting extraction process

---

## 2. Technical Architecture

### Current State: WinForms with Embedded Logic

**Problems:**
- Business logic embedded in 65+ form files (~20,000 LOC)
- Timer-based calculations mixed with UI updates
- OpenGL rendering loops performing business calculations
- Direct cross-form communication via control references
- Global static variables holding application state
- No separation between calculation and presentation
- Impossible to unit test business logic
- Windows-only due to WinForms dependency

**Key Anti-Patterns:**
- Event handlers containing complex algorithms
- Form fields used as state containers
- Controls (ListBox, ComboBox) storing domain data
- Render loops calculating business values
- UI thread performing all calculations
- Scattered settings via Properties.Settings.Default

### Target State: Clean Service Layer

**Architecture:**
```
┌─────────────────────────────────────────────────────────────┐
│                    UI Layer (Avalonia)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  MainView    │  │ FieldView    │  │ SteerView    │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          │                  │                  │
┌─────────┼──────────────────┼──────────────────┼─────────────┐
│         │       ViewModel Layer (MVVM)        │             │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────▼───────┐     │
│  │ MainViewModel│  │FieldViewModel│  │SteerViewModel│     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          │                  │                  │
┌─────────┼──────────────────┼──────────────────┼─────────────┐
│         │          Service Layer              │             │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────▼───────┐     │
│  │ IGpsService  │  │IFieldService │  │ISteeringServ.│     │
│  ├──────────────┤  ├──────────────┤  ├──────────────┤     │
│  │GpsService    │  │FieldService  │  │SteeringServ. │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          │                  │                  │
┌─────────┼──────────────────┼──────────────────┼─────────────┐
│         │           Model Layer               │             │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────▼───────┐     │
│  │  GpsData     │  │   Field      │  │ SteerParams  │     │
│  │  Position    │  │   Boundary   │  │   ABLine     │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

**Service Layer Principles:**
- All services behind interfaces (IServiceName)
- Dependency injection, no static state
- UI-agnostic (no Avalonia, WinForms, or any UI references)
- Event-driven communication between services
- Pure functions for calculations where possible
- Thread-safe where concurrency expected
- Comprehensive XML documentation

### Service Layer Organization

```
AgValoniaGPS.Services/
├── GPS/
│   ├── IGpsService.cs              # GPS orchestration
│   ├── INmeaParserService.cs       # NMEA parsing
│   ├── IPositionUpdateService.cs   # Position pipeline
│   └── IHeadingCalculatorService.cs # Heading fusion
├── Guidance/
│   ├── IGuidanceService.cs         # Guidance orchestration
│   ├── IABLineService.cs           # AB line management
│   ├── ICurveLineService.cs        # Curve following
│   ├── IContourService.cs          # Contour guidance
│   └── ILookAheadService.cs        # Look-ahead calculation
├── Steering/
│   ├── IStanleySteeringService.cs  # Stanley algorithm
│   ├── IPurePursuitService.cs      # Pure Pursuit algorithm
│   └── ISteerAngleCalculator.cs    # Angle calculations
├── Machine/
│   ├── ISectionControlService.cs   # Section on/off logic
│   ├── ICoverageMapService.cs      # Coverage recording
│   ├── ISectionSpeedService.cs     # Section speeds
│   └── IRelayService.cs            # Relay control
├── Field/
│   ├── IFieldService.cs            # Field orchestration
│   ├── IBoundaryService.cs         # Boundary operations
│   ├── IHeadlandService.cs         # Headland management
│   └── IAreaCalculator.cs          # Area calculations
├── Vehicle/
│   ├── IVehicleKinematicsService.cs # Vehicle kinematics
│   ├── IPositionCalculator.cs       # Position calculations
│   └── ITurnCompensation.cs         # Turn compensation
├── Hardware/
│   ├── IUdpCommunicationService.cs  # UDP messaging
│   ├── IPgnMessageBuilder.cs        # PGN formatting
│   └── IModuleMonitor.cs            # Module status
├── State/
│   ├── IApplicationStateService.cs  # App state machine
│   └── IFieldOperationStateMachine.cs # Field state
├── Display/
│   ├── IRenderStateService.cs       # Render state prep
│   ├── IDisplayFormatterService.cs  # Value formatting
│   └── IFieldStatisticsService.cs   # Statistics
└── Infrastructure/
    ├── IMessageBus.cs               # Event bus
    ├── IConfigurationService.cs     # Settings
    └── ITimerCoordinatorService.cs  # Timer management
```

### Dependency Injection Strategy

**Registration Pattern:**
```csharp
// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgValoniaServices(
        this IServiceCollection services)
    {
        // GPS Services (Wave 0 - Complete)
        services.AddSingleton<INmeaParserService, NmeaParserService>();
        services.AddSingleton<IGpsService, GpsService>();

        // Position Services (Wave 1)
        services.AddSingleton<IPositionUpdateService, PositionUpdateService>();
        services.AddSingleton<IHeadingCalculatorService, HeadingCalculatorService>();
        services.AddSingleton<IVehicleKinematicsService, VehicleKinematicsService>();

        // Guidance Services (Wave 2)
        services.AddSingleton<IABLineService, ABLineService>();
        services.AddSingleton<ICurveLineService, CurveLineService>();
        services.AddSingleton<IContourService, ContourService>();

        // Steering Services (Wave 3)
        services.AddSingleton<IStanleySteeringService, StanleySteeringService>();
        services.AddSingleton<IPurePursuitService, PurePursuitService>();
        services.AddSingleton<ILookAheadService, LookAheadService>();

        // Section Control Services (Wave 4)
        services.AddSingleton<ISectionControlService, SectionControlService>();
        services.AddSingleton<ICoverageMapService, CoverageMapService>();

        // Field Services (Wave 5)
        services.AddSingleton<IBoundaryService, BoundaryService>();
        services.AddSingleton<IHeadlandService, HeadlandService>();
        services.AddSingleton<IUTurnService, UTurnService>();

        // Hardware Services (Wave 6)
        services.AddSingleton<IPgnMessageBuilder, PgnMessageBuilder>();
        services.AddSingleton<IHardwareSettingsService, HardwareSettingsService>();

        // Display Services (Wave 7)
        services.AddSingleton<IRenderStateService, RenderStateService>();
        services.AddSingleton<IDisplayFormatterService, DisplayFormatterService>();

        // State Services (Wave 8)
        services.AddSingleton<IApplicationStateService, ApplicationStateService>();
        services.AddSingleton<ITimerCoordinatorService, TimerCoordinatorService>();

        // Infrastructure
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        return services;
    }
}
```

**Lifetime Scopes:**
- Singleton: Services holding state (GPS, Field, State machines)
- Scoped: Per-field operations (not used currently)
- Transient: Pure calculation services (if needed)

### Event-Driven Communication

**Message Bus Pattern:**
```csharp
public interface IMessageBus
{
    void Publish<T>(T message) where T : class;
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
}

// Usage Example
public class GpsService
{
    private readonly IMessageBus _messageBus;

    public void ProcessPosition(GpsData data)
    {
        var position = CalculatePosition(data);

        // Publish for subscribers
        _messageBus.Publish(new PositionUpdated
        {
            Position = position,
            Timestamp = DateTime.Now
        });
    }
}

public class GuidanceService
{
    public GuidanceService(IMessageBus messageBus)
    {
        // Subscribe to position updates
        messageBus.Subscribe<PositionUpdated>(OnPositionUpdate);
    }

    private void OnPositionUpdate(PositionUpdated msg)
    {
        UpdateGuidance(msg.Position);
    }
}
```

---

## 3. Extraction Methodology

### Wave-Based Approach

**Dependency Ordering Principle:**
Extract features in order of dependencies to maintain working system at each stage.

```
Wave 0 (Complete) ──┐
                    ├──> Wave 1 (Foundation) ──┐
Wave 0 (Complete) ──┘                          ├──> Wave 2 (Guidance) ──┐
                                               │                         ├──> Wave 3 (Steering) ──┐
                    Wave 1 ────────────────────┘                         │                        ├──> Wave 4 (Sections)
                                                                         │                        │
                    Wave 1 + Wave 2 ────────────────────────────────────┘                        │
                                                                                                  │
                    Waves 1-3 ────────────────────────────────────────────────────────────────────┤
                                                                                                  ├──> Wave 5 (Field Ops)
                    Waves 1-4 ────────────────────────────────────────────────────────────────────┤
                                                                                                  ├──> Wave 6 (Hardware)
                    Waves 1-5 ────────────────────────────────────────────────────────────────────┤
                                                                                                  ├──> Wave 7 (Display)
                    All Previous ─────────────────────────────────────────────────────────────────┤
                                                                                                  └──> Wave 8 (State)
```

### Test-First Extraction Process

**For Each Feature:**

1. **Analysis**
   - Locate original logic in WinForms code
   - Identify all calculations and algorithms
   - Map dependencies on other features
   - Document edge cases and validation rules

2. **Test Design**
   - Write unit tests based on expected behavior
   - Create test fixtures and mock dependencies
   - Define success criteria
   - Plan regression validation

3. **Service Creation**
   - Define interface contract
   - Create models for data transfer
   - Implement service with clean code
   - Register in DI container

4. **Testing**
   - Run unit tests (must pass)
   - Integration test with UI
   - Validate against original behavior
   - Performance benchmarking

5. **Documentation**
   - XML documentation on public APIs
   - Usage examples in comments
   - Migration notes from original
   - Update architecture diagrams

### Validation Against Original

**Regression Testing Strategy:**
- Capture test data from original AgOpenGPS
- Play back same data through extracted services
- Compare outputs (must match within tolerance)
- Verify timing and performance
- Test edge cases and error conditions

**Example Test Data:**
- GPS NMEA sentences (real field recordings)
- Guidance line coordinates
- Section control scenarios
- Boundary polygons
- Steering parameters

---

## 4. Detailed Wave Specifications

### Wave 0: Already Completed ✅

**Status**: Fully implemented in current AgValoniaGPS

**Features Extracted:**
1. NMEA Parsing - `NmeaParserService`
2. UDP Communication - `UdpCommunicationService`
3. GPS Data Model - `GpsData`, `Position`
4. Field File I/O - `FieldService`, `BoundaryFileService`
5. NTRIP Client - `NtripClientService`
6. Module Monitoring - Hello packet tracking

**Source Locations:**
- `FormGPS/GPS.Designer.cs` - NMEA parsing
- `FormGPS/GPS.Designer.cs` - UDP messaging
- `FormGPS/Classes/CModuleComm.cs` - Module monitoring
- `FormNtrip.cs` - NTRIP client

**Validation:**
- ✅ Reads AgOpenGPS field files correctly
- ✅ UDP communication with Teensy modules working
- ✅ NTRIP corrections forwarding
- ✅ Module timeout detection functional

---

### Wave 1: Position & Kinematics (Weeks 1-2)

**Objective**: Extract core position processing and vehicle kinematics - foundation for all other features

**Complexity**: CRITICAL - Everything depends on this
**Estimated LOC**: ~2,500
**Dependencies**: Wave 0 (GPS data input)
**Timeline**: 2 weeks

#### Feature 1.1: Position Update Pipeline

**Source**: `FormGPS/Position.designer.cs` - `UpdateFixPosition()` (Lines 128-1200)

**Service Interface**:
```csharp
public interface IPositionUpdateService
{
    event EventHandler<PositionUpdate> PositionUpdated;
    event EventHandler<VelocityUpdate> VelocityChanged;

    void ProcessGpsPosition(GpsData gpsData, ImuData imuData);
    Position GetCurrentPosition();
    double GetCurrentSpeed();
    double GetCurrentHeading();
    bool IsReversing();
    Position[] GetPositionHistory(int count);
}
```

**Business Logic to Extract**:
- GPS position smoothing and filtering
- Speed calculation from position deltas
- Direction reversal detection (forward/reverse threshold)
- Position history management (step array buffer)
- Velocity vector calculation

**Test Cases**:
- Position updates at 10Hz (100ms intervals)
- Speed calculation accuracy (compare with GPS speed)
- Reverse detection threshold (0.5 m/s)
- History buffer circular management (20 positions)
- Smoothing filter response time

**Success Criteria**:
- ✅ Position updates synchronized with GPS data
- ✅ Speed accuracy within 0.1 m/s
- ✅ Reverse detection < 500ms latency
- ✅ Zero memory leaks in history buffer
- ✅ Thread-safe position access

#### Feature 1.2: Heading Calculation System

**Source**: `FormGPS/Position.designer.cs` (Lines 190-810)

**Service Interface**:
```csharp
public interface IHeadingCalculatorService
{
    event EventHandler<HeadingUpdate> HeadingChanged;

    double CalculateHeading(HeadingSource source);
    double FuseImuHeading(double gpsHeading, double imuHeading, double fusionWeight);
    double CompensateForRoll(double heading, double roll, double antennaHeight);
    HeadingSource DetermineOptimalSource(double speed, bool hasDual, bool hasImu);
}

public enum HeadingSource
{
    FixToFix,      // Dead reckoning from position changes
    VTG,           // GPS velocity track
    DualAntenna,   // Dual GPS heading
    IMU,           // IMU compass
    Fused          // IMU + GPS fusion
}
```

**Complex Algorithms**:
- Fix-to-fix heading from position deltas
- VTG heading processing
- Dual antenna heading calculation
- IMU sensor fusion (weighted averaging)
- Roll compensation mathematics
- Circular heading math (0-360° wrapping)

**Test Cases**:
- Heading accuracy within 0.1 degrees
- Smooth transitions between sources
- Correct wrapping at 0/360 boundary
- Fusion weights properly applied
- Roll compensation accuracy

**Success Criteria**:
- ✅ Source selection logic correct
- ✅ IMU fusion smooth and stable
- ✅ No heading jumps during transitions
- ✅ Roll compensation validated
- ✅ Performance: <1ms calculation time

#### Feature 1.3: Vehicle Kinematics Engine

**Source**: `FormGPS/Position.designer.cs` - `CalculatePositionHeading()` (Lines 1271-1409)

**Service Interface**:
```csharp
public interface IVehicleKinematicsService
{
    PivotPosition CalculatePivotPosition(Position gps, double heading, VehicleConfig config);
    SteerPosition CalculateSteerAxle(PivotPosition pivot, double heading, VehicleConfig config);
    ToolPosition CalculateToolPosition(PivotPosition pivot, double heading, ToolConfig config);
    HitchPosition CalculateHitchPosition(SteerPosition steer, double heading, VehicleConfig config);

    // Multi-articulated vehicles
    TankPosition CalculateTankPosition(HitchPosition hitch, double heading, double hitchAngle);
    bool IsJackknifed(double hitchAngle, double threshold);

    // Validation
    bool ValidateConfiguration(VehicleConfig config);
}
```

**Mathematical Formulas**:
```csharp
// Pivot position from GPS antenna
pivotEasting = gpsEasting - (sin(heading) * antennaPivot);
pivotNorthing = gpsNorthing - (cos(heading) * antennaPivot);

// Steer axle from pivot
steerEasting = pivotEasting + (sin(heading) * wheelbase);
steerNorthing = pivotNorthing + (cos(heading) * wheelbase);

// Tool position with hitch articulation
toolEasting = hitchEasting + (sin(toolHeading) * hitchLength);
toolNorthing = hitchNorthing + (cos(toolHeading) * hitchLength);
```

**Edge Cases**:
- Jackknife detection (articulation angle > 90°)
- Multi-articulated implements (tank, chisel plow)
- Negative antenna offsets (antenna behind pivot)
- Zero wheelbase (tracked vehicles)
- Front-mounted implements

**Test Cases**:
- Validate kinematics for standard tractor
- Test articulated implement scenarios
- Verify jackknife detection
- Test negative offsets
- Performance: All calculations <5ms

**Success Criteria**:
- ✅ Position calculations match original
- ✅ Articulation handling correct
- ✅ Edge cases handled gracefully
- ✅ No trigonometric singularities
- ✅ Configuration validation comprehensive

**Wave 1 Deliverables**:
1. Three service interfaces and implementations
2. Position, PivotPosition, SteerPosition, ToolPosition models
3. VehicleConfig, ToolConfig configuration objects
4. 30+ unit tests covering all scenarios
5. Integration tests with GPS data
6. Performance benchmarks
7. API documentation

---

### Wave 2: Guidance Lines (Weeks 3-4)

**Objective**: Extract AB line, curve, and contour guidance logic

**Complexity**: HIGH
**Estimated LOC**: ~2,000
**Dependencies**: Wave 1 (Position, Heading, Kinematics)
**Timeline**: 2 weeks

#### Feature 2.1: AB Line Management

**Source**: `FormGPS/Classes/CABLine.cs`, `FormABLine.cs`

**Service Interface**:
```csharp
public interface IABLineService
{
    event EventHandler<ABLineChanged> ABLineChanged;

    // Creation
    ABLine CreateFromPoints(Position pointA, Position pointB);
    ABLine CreateFromHeading(Position origin, double heading);
    ABLine CreateFromExisting(ABLine reference, double offset);

    // Calculations
    double CalculateCrossTrackError(Position current, ABLine line);
    Position GetOnlinePoint(Position current, ABLine line);
    double GetHeadingToLine(Position current, ABLine line);

    // Manipulation
    ABLine NudgeLine(ABLine line, double offsetMeters);
    List<ABLine> GetParallelLines(ABLine reference, double spacing, int count);
    ABLine SnapToHeading(ABLine line, double targetHeading);

    // State
    ABLine GetActiveLine();
    void SetActiveLine(ABLine line);
}
```

**Algorithms to Extract**:
- Line equation from two points (y = mx + b)
- Point-to-line perpendicular distance
- Closest point on line projection
- Parallel line generation
- Line rotation and translation
- Snap-to-heading (90°, 45°, etc.)

**Test Cases**:
- Create line from points (various angles)
- XTE calculation accuracy (±0.01m)
- Parallel line spacing validation
- Nudge operation (positive/negative)
- Edge cases: vertical lines, zero length

**Success Criteria**:
- ✅ XTE accuracy within 1cm
- ✅ All geometric operations validated
- ✅ Performance: <1ms per calculation
- ✅ No numeric instability

#### Feature 2.2: Curve Line System

**Source**: `FormGPS/Classes/CABCurve.cs`, `FormABCurve.cs`

**Service Interface**:
```csharp
public interface ICurveLineService
{
    event EventHandler<CurveLineChanged> CurveChanged;

    // Recording
    CurveLine StartRecording(Position start);
    void AddPoint(Position point, double minDistance);
    CurveLine FinishRecording();

    // Calculations
    double CalculateCrossTrackError(Position current, CurveLine curve);
    Position GetClosestPoint(Position current, CurveLine curve);
    double GetHeadingAtPoint(Position point, CurveLine curve);

    // Processing
    CurveLine SmoothCurve(CurveLine curve, double smoothingFactor);
    CurveLine SimplifyCurve(CurveLine curve, double tolerance);
    List<CurveLine> GenerateParallelCurves(CurveLine reference, double spacing, int count);
}
```

**Complex Algorithms**:
- Cubic spline interpolation
- Curve smoothing (moving average, Gaussian)
- Closest point on curve (iterative search)
- Curve offset generation (perpendicular offsets)
- Point reduction (Douglas-Peucker)

**Test Cases**:
- Record smooth curve from points
- XTE accuracy on curved path
- Smoothing preserves shape
- Parallel curve spacing correct
- Performance with 1000+ point curves

**Success Criteria**:
- ✅ Smooth curve following
- ✅ XTE accuracy <5cm on curves
- ✅ Parallel curves maintain offset
- ✅ No jagged edges after smoothing
- ✅ Handles sharp turns

#### Feature 2.3: Contour Following

**Source**: `FormGPS/Classes/CContour.cs`

**Service Interface**:
```csharp
public interface IContourService
{
    event EventHandler<ContourUpdate> ContourUpdated;

    ContourLine StartRecording(Position start);
    void AddPoint(Position point, double minDistance);
    ContourLine LockContour();

    double CalculateOffset(Position current, ContourLine contour);
    void UpdateGuidance(Position current, ContourLine locked, double targetOffset);
}
```

**Business Logic**:
- Real-time contour recording
- Fixed offset maintenance
- Contour line locking
- Perpendicular offset calculation

**Test Cases**:
- Record contour while moving
- Lock contour and follow with offset
- Offset accuracy (±2cm)
- Smooth transitions

**Success Criteria**:
- ✅ Real-time recording <10ms latency
- ✅ Consistent offset maintenance
- ✅ Works on irregular contours
- ✅ No drift over time

**Wave 2 Deliverables**:
1. Three guidance service interfaces
2. ABLine, CurveLine, ContourLine models
3. Geometric algorithm library
4. 40+ unit tests
5. Real-world curve data tests
6. API documentation

---

### Wave 3: Steering Algorithms (Weeks 4-5)

**Objective**: Extract steering control algorithms

**Complexity**: HIGH
**Estimated LOC**: ~1,500
**Dependencies**: Waves 1-2 (Position, Guidance Lines)
**Timeline**: 1.5 weeks

#### Feature 3.1: Stanley Steering Algorithm

**Source**: `FormGPS/Position.designer.cs` (Lines 871-912)

**Service Interface**:
```csharp
public interface IStanleySteeringService
{
    double CalculateSteerAngle(
        double crossTrackError,
        double headingError,
        double speed,
        StanleyParameters parameters);

    double CalculateIntegralTerm(
        double crossTrackError,
        TimeSpan deltaTime,
        StanleyParameters parameters);

    StanleyParameters AutoTune(
        List<SteeringSample> samples,
        VehicleConfig vehicle);
}

public class StanleyParameters
{
    public double DistanceGain { get; set; }    // Typically 0.8
    public double HeadingGain { get; set; }     // Typically 1.0
    public double IntegralGain { get; set; }    // Typically 0.0-0.1
    public double MaxSteerAngle { get; set; }   // Vehicle limit
}
```

**Stanley Algorithm**:
```
steerAngle = (headingError * headingGain) +
             atan(distanceGain * crossTrackError / speed) +
             (integralTerm * integralGain)
```

**Test Cases**:
- Zero error produces zero output
- Heading error response
- Cross-track error correction
- Speed dependency validation
- Integral windup prevention
- Angle limiting

**Success Criteria**:
- ✅ Algorithm matches original
- ✅ Stable at all speeds
- ✅ Proper gain tuning
- ✅ No oscillation

#### Feature 3.2: Pure Pursuit Algorithm

**Source**: `FormGPS/Classes/CVehicle.cs`

**Service Interface**:
```csharp
public interface IPurePursuitService
{
    double CalculateSteerAngle(
        Position current,
        Position goalPoint,
        double wheelbase,
        PurePursuitParameters parameters);

    Position CalculateGoalPoint(
        Position current,
        GuidanceLine line,
        double lookAheadDistance);
}
```

**Pure Pursuit Formula**:
```
curvature = 2 * sin(alpha) / lookAheadDistance
steerAngle = atan(wheelbase * curvature)
```

**Test Cases**:
- Goal point calculation
- Curvature calculation
- Wheelbase dependency
- Look-ahead distance tuning

**Success Criteria**:
- ✅ Smooth path following
- ✅ Correct wheelbase scaling
- ✅ No overshoot

#### Feature 3.3: Look-Ahead Distance Calculator

**Source**: `FormGPS/Position.designer.cs` (Lines 1350-1380)

**Service Interface**:
```csharp
public interface ILookAheadService
{
    double CalculateLookAheadDistance(
        double speed,
        double crossTrackError,
        double toolWidth,
        LookAheadMode mode);

    double CalculateLookAheadTime(double distance, double speed);
}

public enum LookAheadMode
{
    ToolWidthMultiplier,  // Distance = toolWidth * multiplier
    TimeBased,            // Distance = speed * time
    Hold                  // Constant distance
}
```

**Test Cases**:
- All modes produce valid distances
- Speed dependency correct
- Tool width multiplier
- Minimum/maximum limits

**Success Criteria**:
- ✅ Smooth distance changes
- ✅ All modes functional
- ✅ Safe limit enforcement

**Wave 3 Deliverables**:
1. Three steering service interfaces
2. StanleyParameters, PurePursuitParameters models
3. 25+ algorithm tests
4. Comparison with original outputs
5. Tuning documentation

---

### Wave 4: Section Control & Coverage (Weeks 5-6)

**Objective**: Extract section control logic and coverage mapping

**Complexity**: HIGH
**Estimated LOC**: ~2,000
**Dependencies**: Waves 1-3
**Timeline**: 2 weeks

#### Feature 4.1: Section State Machine

**Source**: `FormGPS/Classes/CSection.cs`

**Service Interface**:
```csharp
public interface ISectionControlService
{
    event EventHandler<SectionStateChanged> StateChanged;

    void UpdateSectionStates(
        Position current,
        Boundary boundary,
        double lookAheadDistance,
        bool[] manualOverrides);

    bool[] GetSectionStates();
    void ManualToggleSection(int sectionIndex);
    void SetMasterSwitch(bool on);
    void SetAutoMode(bool auto);

    bool IsSectionInBoundary(int sectionIndex, Position position);
    bool IsOverlapping(int sectionIndex, Position position);
}
```

**State Logic**:
- Boundary checking per section
- Look-ahead turn on/off
- Overlap detection
- Manual override handling
- Master switch control

**Test Cases**:
- Sections turn on inside boundary
- Sections turn off at boundary
- Look-ahead timing correct
- Manual overrides work
- Master switch disables all

**Success Criteria**:
- ✅ No missed turns
- ✅ Smooth on/off transitions
- ✅ Overlap detection accurate
- ✅ <50ms update latency

#### Feature 4.2: Coverage Mapping

**Source**: `FormGPS/Position.designer.cs` - `AddSectionOrPathPoints()` (Lines 1629-1668)

**Service Interface**:
```csharp
public interface ICoverageMapService
{
    void RecordCoverage(
        Position position,
        bool[] sectionStates,
        double[] sectionWidths,
        DateTime timestamp);

    double CalculateWorkedArea();
    double CalculateOverlapArea();
    CoverageMap GetCoverageMap();
    void StartNewPatch();
    void Clear();
}
```

**Algorithms**:
- Triangle strip generation
- Area accumulation
- Overlap calculation
- Patch color coding

**Test Cases**:
- Area calculation accuracy
- Overlap detection
- Memory management
- Performance with large maps

**Success Criteria**:
- ✅ Area accurate to 0.1%
- ✅ Handles 8+ hour fields
- ✅ Memory < 200MB
- ✅ Fast visualization

#### Feature 4.3: Section Speed Calculation

**Source**: `FormGPS/Position.designer.cs` - `CalculateSectionLookAhead()` (Lines 1412-1508)

**Service Interface**:
```csharp
public interface ISectionSpeedService
{
    SectionSpeeds CalculateSectionSpeeds(
        Position current,
        double toolWidth,
        int sectionCount,
        double vehicleSpeed,
        double turnRadius);

    double GetLookAheadTime(int section, double speed);
}
```

**Logic**:
- Individual section speeds (inner vs outer during turns)
- Look-ahead distance per section
- Turn radius compensation

**Test Cases**:
- Straight line: all speeds equal
- Turning: outer faster than inner
- Speed differential calculation

**Success Criteria**:
- ✅ Accurate turn compensation
- ✅ Smooth speed transitions

**Wave 4 Deliverables**:
1. Three section control services
2. CoverageMap, SectionStates models
3. 35+ tests
4. Performance benchmarks
5. Memory profiling

---

### Wave 5: Field Operations (Weeks 6-7)

**Objective**: Extract boundary, headland, and U-turn logic

**Complexity**: MEDIUM
**Estimated LOC**: ~1,800
**Dependencies**: Waves 1-4
**Timeline**: 1.5 weeks

#### Feature 5.1: Boundary Management

**Source**: `FormGPS/Classes/CBoundary.cs`, `FormBoundary.cs`

**Service Interface**:
```csharp
public interface IBoundaryManagementService
{
    // Recording
    void StartRecording(BoundaryType type);
    void AddPoint(Position point, double minDistance);
    Boundary FinishRecording();

    // Queries
    bool IsInside(Position point, Boundary boundary);
    double DistanceToBoundary(Position point, Boundary boundary);
    Position GetClosestBoundaryPoint(Position point, Boundary boundary);

    // Processing
    double CalculateArea(Boundary boundary);
    Boundary SimplifyBoundary(Boundary boundary, double tolerance);
    Boundary OffsetBoundary(Boundary boundary, double distance);
}
```

**Algorithms**:
- Ray-casting point-in-polygon
- Douglas-Peucker simplification
- Shoelace area formula
- Polygon offset generation

**Test Cases**:
- Point-in-polygon accuracy
- Area calculation validation
- Simplification preserves shape
- Offset boundary correct

**Success Criteria**:
- ✅ No false positives/negatives
- ✅ Area accuracy <0.5%
- ✅ Handles complex polygons

#### Feature 5.2: Headland Processing

**Source**: `FormGPS/Classes/CHeadland.cs`, `FormHeadland.cs`

**Service Interface**:
```csharp
public interface IHeadlandService
{
    Headland CreateFromBoundary(Boundary boundary, double width, int passes);
    bool IsInHeadland(Position point, Headland headland);
    void SetHeadlandMode(HeadlandMode mode);
    Position GetHeadlandEntryPoint(Position current, double heading);
}
```

**Logic**:
- Headland offset calculation
- Multiple pass generation
- Entry/exit point detection

**Test Cases**:
- Headland width accuracy
- Multiple passes correct
- Entry point detection

**Success Criteria**:
- ✅ Headland width within 5cm
- ✅ Multiple passes work
- ✅ Entry detection reliable

#### Feature 5.3: U-Turn Generation

**Source**: `FormGPS/Position.designer.cs` (Lines 1073-1185)

**Service Interface**:
```csharp
public interface IUTurnService
{
    UTurnPath GenerateUTurn(
        Position current,
        double heading,
        UTurnStyle style,
        double turnRadius);

    bool ShouldTriggerUTurn(
        Position current,
        Boundary boundary,
        double triggerDistance);

    Position GetNextPathPoint(UTurnPath path, Position current);
    void StartUTurn();
    void CancelUTurn();
}

public enum UTurnStyle
{
    QuestionMark,
    SemiCircle,
    KeyHole,
    Custom
}
```

**Algorithms**:
- Dubins path planning
- Turn radius constraints
- Boundary proximity triggers

**Test Cases**:
- All turn styles generate valid paths
- Turn radius respected
- Trigger distance correct

**Success Criteria**:
- ✅ Smooth turn paths
- ✅ No boundary violations
- ✅ All styles work

**Wave 5 Deliverables**:
1. Three field operation services
2. Boundary, Headland, UTurnPath models
3. 30+ tests
4. Geometric algorithm validation

---

### Wave 6: Hardware Communication (Weeks 7-8)

**Objective**: Extract PGN messaging and hardware settings

**Complexity**: MEDIUM
**Estimated LOC**: ~1,200
**Dependencies**: Waves 1-5 (for data to send)
**Timeline**: 1.5 weeks

#### Feature 6.1: PGN Message Builder

**Source**: `FormGPS/GPS.Designer.cs` - `SendPgnToLoop()` and related

**Service Interface**:
```csharp
public interface IPgnMessageBuilderService
{
    byte[] BuildAutoSteerData(
        double steerAngle,
        double speed,
        bool enabled,
        byte status);

    byte[] BuildMachineData(
        bool[] sectionStates,
        double rate,
        bool masterSwitch);

    byte[] BuildSettingsMessage(
        VehicleConfig vehicle,
        SteerSettings steer);

    byte[] BuildFromData(int pgn, byte[] data);
    ushort CalculateCrc(byte[] data);
}
```

**Protocol**:
- Header: `[0x80, 0x81, source, PGN, length]`
- CRC-16 calculation
- Byte packing for different data types

**Test Cases**:
- Message format correct
- CRC calculation matches original
- All PGNs build correctly

**Success Criteria**:
- ✅ Hardware accepts messages
- ✅ CRC validation passes
- ✅ All data types supported

#### Feature 6.2: Hardware Settings Manager

**Source**: `FormGPS/GPS.Designer.cs` - `SendSettings()` (Lines 1158-1197)

**Service Interface**:
```csharp
public interface IHardwareSettingsService
{
    void SendVehicleConfiguration(VehicleConfig config);
    void SendSteerSettings(SteerSettings settings);
    void SendSectionConfiguration(SectionConfig config);
    void SendPinConfiguration(PinConfig pins);

    byte ConvertPwmFrequency(int frequency);
    byte[] PackSectionWidths(double[] widths);
}
```

**Logic**:
- Settings format conversion
- PWM frequency mapping
- Section width packing

**Test Cases**:
- Settings send successfully
- Values received by hardware
- Conversion accuracy

**Success Criteria**:
- ✅ All settings apply correctly
- ✅ Hardware responds properly

**Wave 6 Deliverables**:
1. Two hardware services
2. PGN message models
3. Protocol documentation
4. Hardware validation tests

---

### Wave 7: Display & Visualization (Weeks 8-9)

**Objective**: Extract display calculations and formatting

**Complexity**: LOW
**Estimated LOC**: ~1,000
**Dependencies**: All previous waves
**Timeline**: 1 week

#### Feature 7.1: Field Statistics Calculator

**Source**: Various calculations throughout FormGPS

**Service Interface**:
```csharp
public interface IFieldStatisticsCalculator
{
    FieldStats Calculate(Field field, CoverageMap coverage);
    string FormatArea(double hectares, UnitSystem units);
    string FormatDistance(double meters, UnitSystem units);
    double EstimateTimeRemaining(double remainingArea, double workRate);
}
```

**Calculations**:
- Total field area
- Worked area
- Remaining area
- Time estimates
- Work rate

**Test Cases**:
- Area calculations accurate
- Unit conversions correct
- Time estimates reasonable

**Success Criteria**:
- ✅ All statistics correct
- ✅ Format strings proper
- ✅ Performance <1ms

#### Feature 7.2: Display Formatters

**Source**: Various ToString() and display methods

**Service Interface**:
```csharp
public interface IDisplayFormatterService
{
    string FormatSpeed(double metersPerSecond, UnitSystem units);
    string FormatHeading(double radians, HeadingFormat format);
    string FormatGpsQuality(int fixQuality, int satellites);
    string FormatSteerAngle(double angle, int precision);
    Color GetGpsQualityColor(int fixQuality);
}
```

**Formatting**:
- Speed (m/s, km/h, mph)
- Heading (degrees, cardinal)
- GPS quality indicators
- Angle precision

**Test Cases**:
- All unit conversions
- Format strings correct
- Color mappings

**Success Criteria**:
- ✅ Clean string output
- ✅ Proper precision
- ✅ Culture-aware formatting

**Wave 7 Deliverables**:
1. Two display services
2. 20+ formatting tests
3. Localization support

---

### Wave 8: State Management (Weeks 9-10)

**Objective**: Extract application state and timer coordination

**Complexity**: HIGH
**Estimated LOC**: ~1,500
**Dependencies**: All previous waves
**Timeline**: 1.5 weeks

#### Feature 8.1: Application State Machine

**Source**: `FormGPS.cs` - `JobNew()`, `JobClose()`, state variables

**Service Interface**:
```csharp
public interface IApplicationStateService
{
    event EventHandler<StateTransition> StateChanged;

    ApplicationState Current { get; }

    void StartNewField(string name);
    void OpenExistingField(string path);
    void CloseField();
    void EnterSimulationMode();
    void ExitSimulationMode();

    bool CanTransitionTo(ApplicationState newState);
}

public enum ApplicationState
{
    Idle,
    FieldOpen,
    Recording,
    Playback,
    Simulation
}
```

**State Logic**:
- Valid state transitions
- State-dependent features
- Cleanup on transitions

**Test Cases**:
- All transitions valid
- Invalid transitions blocked
- Cleanup occurs

**Success Criteria**:
- ✅ No invalid states
- ✅ Clean transitions
- ✅ State persistence

#### Feature 8.2: Timer Coordination Service

**Source**: `FormGPS` - `tmrWatchdog_tick` and timers

**Service Interface**:
```csharp
public interface ITimerCoordinatorService
{
    void RegisterTimer(string name, TimeSpan interval, Action callback);
    void Start();
    void Stop();
    void UpdateGpsFrequency(int hertz);
    void CheckTimeouts();
}
```

**Managed Timers**:
- GPS watchdog (2s timeout)
- Module data flow (100-300ms)
- Display updates (125ms)
- Auto-save (5 minutes)

**Test Cases**:
- Timers fire at intervals
- Frequency adjustments work
- Timeout detection

**Success Criteria**:
- ✅ Accurate timing
- ✅ No missed callbacks
- ✅ Clean shutdown

**Wave 8 Deliverables**:
1. Two state services
2. State machine tests
3. Timer accuracy tests
4. Integration validation

---

## 5. Service Interface Definitions

### Core Service Examples

#### IPositionUpdateService

```csharp
/// <summary>
/// Processes GPS position updates and maintains position history.
/// </summary>
public interface IPositionUpdateService
{
    /// <summary>
    /// Raised when position is updated.
    /// </summary>
    event EventHandler<PositionUpdate> PositionUpdated;

    /// <summary>
    /// Raised when velocity changes significantly.
    /// </summary>
    event EventHandler<VelocityUpdate> VelocityChanged;

    /// <summary>
    /// Processes incoming GPS position data.
    /// </summary>
    /// <param name="gpsData">GPS data from NMEA parser</param>
    /// <param name="imuData">Optional IMU data for sensor fusion</param>
    void ProcessGpsPosition(GpsData gpsData, ImuData imuData);

    /// <summary>
    /// Gets the current vehicle position.
    /// </summary>
    Position GetCurrentPosition();

    /// <summary>
    /// Gets current speed in meters per second.
    /// </summary>
    double GetCurrentSpeed();

    /// <summary>
    /// Gets current heading in radians.
    /// </summary>
    double GetCurrentHeading();

    /// <summary>
    /// Determines if vehicle is reversing.
    /// </summary>
    bool IsReversing();

    /// <summary>
    /// Gets position history for trail display.
    /// </summary>
    /// <param name="count">Number of positions to retrieve</param>
    Position[] GetPositionHistory(int count);
}
```

#### IGuidanceService

```csharp
/// <summary>
/// Orchestrates guidance line following and cross-track error calculation.
/// </summary>
public interface IGuidanceService
{
    /// <summary>
    /// Raised when guidance state changes.
    /// </summary>
    event EventHandler<GuidanceStateChanged> StateChanged;

    /// <summary>
    /// Sets the active guidance mode.
    /// </summary>
    void SetGuidanceMode(GuidanceMode mode);

    /// <summary>
    /// Updates guidance calculations with current position.
    /// </summary>
    void Update(Position current, double heading);

    /// <summary>
    /// Gets current cross-track error in meters.
    /// </summary>
    double GetCrossTrackError();

    /// <summary>
    /// Gets heading error in radians.
    /// </summary>
    double GetHeadingError();

    /// <summary>
    /// Gets the active guidance line.
    /// </summary>
    IGuidanceLine GetActiveLine();
}
```

#### ISectionControlService

```csharp
/// <summary>
/// Manages automatic section control based on position and boundaries.
/// </summary>
public interface ISectionControlService
{
    /// <summary>
    /// Raised when any section state changes.
    /// </summary>
    event EventHandler<SectionStateChange> SectionStateChanged;

    /// <summary>
    /// Updates section states based on current conditions.
    /// </summary>
    /// <param name="current">Current vehicle position</param>
    /// <param name="boundary">Field boundary</param>
    /// <param name="lookAheadDistance">Distance to look ahead</param>
    void UpdateSectionStates(
        Position current,
        Boundary boundary,
        double lookAheadDistance);

    /// <summary>
    /// Gets current section on/off states.
    /// </summary>
    bool[] GetSectionStates();

    /// <summary>
    /// Manually toggles a section on/off.
    /// </summary>
    void ManualToggleSection(int sectionIndex);

    /// <summary>
    /// Sets master switch (all sections on/off).
    /// </summary>
    void SetMasterSwitch(bool on);

    /// <summary>
    /// Enables/disables automatic section control.
    /// </summary>
    void SetAutoMode(bool auto);
}
```

---

## 6. Extraction Patterns

### Pattern Summary

The extraction follows 9 proven patterns documented in `extraction-patterns-guide.md`:

1. **Timer Tick Extraction**: Move timer logic to background services with events
2. **Complex Calculation Extraction**: Create pure functions for mathematical operations
3. **State Machine Extraction**: Formalize state transitions in dedicated services
4. **Cross-Form Communication Extraction**: Replace direct form access with message bus
5. **OpenGL Rendering Calculation Extraction**: Pre-calculate all values before rendering
6. **Global Variable Elimination**: Replace statics with dependency injection
7. **Async Operation Extraction**: Use async/await for long-running operations
8. **Configuration Extraction**: Centralize settings management
9. **Testable Service Extraction**: Design as pure, mockable components

### Key Principles

**DO:**
- Write interface before implementation
- Create unit tests immediately
- Use dependency injection
- Keep services stateless when possible
- Document extraction source location

**DO NOT:**
- Copy code wholesale from WinForms
- Mix UI concerns in services
- Use WinForms types in services
- Access services from models
- Create circular dependencies

---

## 7. Testing Strategy

### Unit Testing Approach

**Framework**: xUnit.net with NSubstitute for mocking

**Test Structure**:
```csharp
public class PositionUpdateServiceTests
{
    private PositionUpdateService _service;
    private Mock<IHeadingCalculatorService> _mockHeading;

    [SetUp]
    public void Setup()
    {
        _mockHeading = new Mock<IHeadingCalculatorService>();
        _service = new PositionUpdateService(_mockHeading.Object);
    }

    [Test]
    public void ProcessGpsPosition_ValidData_UpdatesPosition()
    {
        // Arrange
        var gpsData = new GpsData
        {
            Latitude = 42.123456,
            Longitude = -83.234567,
            Altitude = 250.5
        };

        // Act
        _service.ProcessGpsPosition(gpsData, null);

        // Assert
        var position = _service.GetCurrentPosition();
        Assert.That(position.Latitude, Is.EqualTo(42.123456).Within(0.000001));
    }

    [Test]
    public void GetCurrentSpeed_NoMovement_ReturnsZero()
    {
        // Arrange - send same position twice
        var gpsData = CreateTestGpsData();
        _service.ProcessGpsPosition(gpsData, null);
        Thread.Sleep(100);
        _service.ProcessGpsPosition(gpsData, null);

        // Act
        var speed = _service.GetCurrentSpeed();

        // Assert
        Assert.That(speed, Is.EqualTo(0).Within(0.01));
    }
}
```

**Coverage Targets**:
- Critical services (Position, Steering): >90%
- Standard services: >80%
- Simple formatters: >70%
- Overall target: >80%

### Integration Testing Approach

**Scenario-Based Tests**:
```csharp
[TestFixture]
public class GuidanceIntegrationTests
{
    private IServiceProvider _services;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddAgValoniaServices();
        _services = services.BuildServiceProvider();
    }

    [Test]
    public async Task FullGuidanceLoop_SimulatedGPS_ProducesCorrectSteerCommands()
    {
        // Arrange
        var gpsService = _services.GetRequiredService<IGpsService>();
        var guidanceService = _services.GetRequiredService<IGuidanceService>();
        var steeringService = _services.GetRequiredService<IStanleySteeringService>();

        // Create AB line
        var abLine = CreateTestABLine();
        guidanceService.SetActiveLine(abLine);

        // Act - Simulate GPS positions along line
        var steerAngles = new List<double>();
        foreach (var position in GenerateTestPath())
        {
            gpsService.UpdatePosition(position);
            await Task.Delay(100); // 10Hz

            var xte = guidanceService.GetCrossTrackError();
            var headingError = guidanceService.GetHeadingError();
            var angle = steeringService.CalculateSteerAngle(xte, headingError, 2.0, defaults);
            steerAngles.Add(angle);
        }

        // Assert
        Assert.That(steerAngles, Has.All.LessThan(30)); // Max angle
        Assert.That(steerAngles.Last(), Is.EqualTo(0).Within(1)); // Converges to zero
    }
}
```

### Regression Testing

**Validation Against Original**:
1. Record GPS data from real AgOpenGPS session
2. Play back through extracted services
3. Compare outputs (XTE, steer angle, section states)
4. Validate within acceptable tolerance

**Test Data Sources**:
- Real field recordings (NMEA logs)
- Synthetic test scenarios
- Edge case data sets
- Performance stress tests

### Performance Benchmarking

**Metrics to Track**:
- GPS processing: <10ms per update (10Hz capable)
- Steering calculation: <10ms
- Section control update: <50ms
- Coverage map update: <20ms
- Memory usage: <500MB working set

**Benchmark Framework**:
```csharp
[Benchmark]
public void BenchmarkPositionUpdate()
{
    var gpsData = CreateTestGpsData();
    _positionService.ProcessGpsPosition(gpsData, null);
}
```

---

## 8. Quality Assurance

### Code Review Process

**Review Checklist**:
- [ ] No UI framework references
- [ ] Interface-based design
- [ ] Dependency injection used
- [ ] Unit tests included
- [ ] XML documentation complete
- [ ] No circular dependencies
- [ ] Thread-safe where needed
- [ ] Error handling appropriate
- [ ] Performance acceptable
- [ ] Matches original behavior

### Architecture Compliance

**Automated Checks**:
- NDepend or ArchUnitNet rules
- No references to Avalonia in Services project
- No references to System.Windows.Forms
- All public interfaces documented

### Cross-Platform Testing

**Test Matrix**:
| Platform | Architecture | Status |
|----------|-------------|--------|
| Windows 10/11 | x64 | Required |
| Ubuntu 20.04+ | x64 | Required |
| macOS 11+ | x64, ARM64 | Required |
| Raspberry Pi | ARM32 | Optional |

### Performance Validation

**Continuous Monitoring**:
- BenchmarkDotNet for critical paths
- Memory profiler for leak detection
- CPU profiler for hotspots
- CI performance regression tests

---

## 9. Risk Management

### Technical Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Algorithm accuracy mismatch | HIGH | MEDIUM | Comprehensive regression tests with recorded data |
| Performance degradation | HIGH | LOW | Continuous benchmarking, profiling |
| Memory leaks | MEDIUM | MEDIUM | Memory profiling, lifetime management |
| Thread safety issues | HIGH | MEDIUM | Thread safety tests, code review |
| Numeric instability | MEDIUM | LOW | Unit tests with edge cases |

### Dependency Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Circular dependencies | HIGH | MEDIUM | Architecture reviews, dependency graphs |
| Breaking wave dependencies | HIGH | LOW | Follow strict wave ordering |
| Interface changes | MEDIUM | MEDIUM | Version interfaces carefully |

### Timeline Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Wave overruns | MEDIUM | MEDIUM | Buffer time in estimates, prioritize |
| Complexity underestimated | HIGH | MEDIUM | Start with Wave 1 to calibrate |
| Testing bottleneck | MEDIUM | MEDIUM | Test-first approach, parallel testing |
| Integration issues | HIGH | LOW | Incremental integration per wave |

### Quality Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Insufficient test coverage | HIGH | MEDIUM | Enforce >80% coverage requirement |
| Behavioral differences | CRITICAL | LOW | Regression testing against original |
| Documentation lag | MEDIUM | HIGH | Document as you extract |

---

## 10. Migration Strategy

### Handling Existing AgValoniaGPS Code

**Current Status**: Wave 0 features already extracted
- GPS services operational
- Field I/O working
- UDP communication functional
- NTRIP client complete

**Integration Approach**:
1. New services extend existing functionality
2. Maintain interface compatibility where possible
3. Deprecate old code gradually
4. Update ViewModels to use new services

### Backward Compatibility

**File Format Compatibility**:
- Maintain AgOpenGPS field file formats
- Support reading existing fields
- Preserve PGN protocol compatibility
- Hardware compatibility maintained

**Configuration Migration**:
- Support legacy settings format
- Provide migration utility
- Document breaking changes
- Version configuration files

### Incremental Delivery

**Per-Wave Delivery**:
1. Complete wave extraction
2. Integrate with existing UI
3. Validate functionality
4. Deploy to test users
5. Gather feedback
6. Fix issues before next wave

**Rollback Strategy**:
- Feature flags for new services
- Keep original reference code
- Ability to disable extracted features
- Rollback to previous wave if issues

---

## 11. Documentation Requirements

### Service API Documentation

**XML Documentation**:
- All public interfaces
- All public methods
- All events
- Parameters and return values
- Usage examples
- Thread safety notes

**Example**:
```csharp
/// <summary>
/// Calculates cross-track error for AB line guidance.
/// </summary>
/// <param name="current">Current vehicle position in UTM coordinates</param>
/// <param name="line">Active AB line</param>
/// <returns>
/// Cross-track error in meters. Positive values indicate
/// vehicle is to the right of the line.
/// </returns>
/// <remarks>
/// This method is thread-safe and can be called at up to 10Hz.
/// Calculation complexity is O(1).
/// </remarks>
/// <example>
/// <code>
/// var xte = _abLineService.CalculateCrossTrackError(position, abLine);
/// if (Math.Abs(xte) > 0.5)
/// {
///     // Apply correction
/// }
/// </code>
/// </example>
double CalculateCrossTrackError(Position current, ABLine line);
```

### Migration Guides

**Per-Wave Guide**:
- Original code location
- New service location
- API changes
- Integration steps
- Breaking changes
- Testing checklist

### Pattern Examples

**Extraction Pattern Library**:
- Before/after code samples
- Common pitfalls
- Best practices
- Performance tips

### Integration Guides

**UI Integration**:
- ViewModel setup
- Service injection
- Event subscription
- Error handling
- Performance optimization

---

## 12. Deliverables

### Wave 1 Deliverables
- [ ] IPositionUpdateService interface and implementation
- [ ] IHeadingCalculatorService interface and implementation
- [ ] IVehicleKinematicsService interface and implementation
- [ ] Position, PivotPosition, SteerPosition, ToolPosition models
- [ ] VehicleConfig, ToolConfig configuration classes
- [ ] 30+ unit tests (>80% coverage)
- [ ] Integration tests with GPS data
- [ ] Performance benchmarks
- [ ] API documentation (XML comments)
- [ ] Wave 1 migration guide

### Wave 2 Deliverables
- [ ] IABLineService interface and implementation
- [ ] ICurveLineService interface and implementation
- [ ] IContourService interface and implementation
- [ ] ABLine, CurveLine, ContourLine models
- [ ] Geometric algorithm library
- [ ] 40+ unit tests
- [ ] Real-world curve data validation
- [ ] API documentation
- [ ] Wave 2 migration guide

### Wave 3 Deliverables
- [ ] IStanleySteeringService interface and implementation
- [ ] IPurePursuitService interface and implementation
- [ ] ILookAheadService interface and implementation
- [ ] StanleyParameters, PurePursuitParameters models
- [ ] 25+ algorithm tests
- [ ] Comparison with original outputs
- [ ] Tuning documentation
- [ ] API documentation
- [ ] Wave 3 migration guide

### Wave 4 Deliverables
- [ ] ISectionControlService interface and implementation
- [ ] ICoverageMapService interface and implementation
- [ ] ISectionSpeedService interface and implementation
- [ ] CoverageMap, SectionStates models
- [ ] 35+ unit tests
- [ ] Performance benchmarks
- [ ] Memory profiling results
- [ ] API documentation
- [ ] Wave 4 migration guide

### Wave 5 Deliverables
- [ ] IBoundaryManagementService interface and implementation
- [ ] IHeadlandService interface and implementation
- [ ] IUTurnService interface and implementation
- [ ] Boundary, Headland, UTurnPath models
- [ ] 30+ geometric tests
- [ ] Algorithm validation
- [ ] API documentation
- [ ] Wave 5 migration guide

### Wave 6 Deliverables
- [ ] IPgnMessageBuilderService interface and implementation
- [ ] IHardwareSettingsService interface and implementation
- [ ] PGN message models
- [ ] Protocol documentation
- [ ] Hardware validation tests
- [ ] API documentation
- [ ] Wave 6 migration guide

### Wave 7 Deliverables
- [ ] IFieldStatisticsCalculator interface and implementation
- [ ] IDisplayFormatterService interface and implementation
- [ ] 20+ formatting tests
- [ ] Localization support
- [ ] API documentation
- [ ] Wave 7 migration guide

### Wave 8 Deliverables
- [ ] IApplicationStateService interface and implementation
- [ ] ITimerCoordinatorService interface and implementation
- [ ] State machine documentation
- [ ] Timer accuracy tests
- [ ] Integration validation
- [ ] API documentation
- [ ] Wave 8 migration guide

### Final Project Deliverables
- [ ] Complete service layer (50+ services)
- [ ] Full test suite (300+ tests, >80% coverage)
- [ ] Performance validation report
- [ ] Architecture documentation
- [ ] Complete API reference (DocFX generated)
- [ ] User migration guide
- [ ] Developer onboarding guide
- [ ] Cross-platform validation report

---

## Out of Scope

### Explicitly NOT Included

**UI Rewriting**:
- Complete Avalonia UI redesign
- New UI features not in original
- UI performance optimization
- Screen layout changes

**Algorithm Improvements**:
- Better steering algorithms
- Improved path planning
- Enhanced section control logic
- (Extract as-is first, improve later)

**New Features**:
- AgShare cloud integration (separate project)
- Mobile apps (future work)
- Web interface (future work)
- Features not in AgOpenGPS

**Performance Optimization**:
- Algorithm optimization (unless blocking)
- Memory optimization (unless critical)
- GPU acceleration
- (Optimize after extraction complete)

### Future Phases (After Extraction)

**Phase 2: Algorithm Enhancement**
- Improve steering algorithms
- Better path planning
- Enhanced look-ahead
- Optimized section control

**Phase 3: New Features**
- AgShare integration
- Cloud field sync
- Weather integration
- Yield mapping

**Phase 4: Platform Expansion**
- Mobile companion apps
- Web field management
- Qt desktop UI option
- Embedded Linux support

---

## Success Criteria

### Project Complete When:

1. **Architecture**
   - ✅ All services behind interfaces
   - ✅ Zero UI dependencies in services
   - ✅ Dependency injection throughout
   - ✅ Clean layer separation

2. **Functionality**
   - ✅ All 50+ features extracted
   - ✅ 100% feature parity with original
   - ✅ All edge cases handled
   - ✅ Error handling comprehensive

3. **Quality**
   - ✅ Test coverage >80%
   - ✅ All regression tests pass
   - ✅ Performance meets targets
   - ✅ Memory usage acceptable

4. **Cross-Platform**
   - ✅ Compiles on .NET 8.0
   - ✅ Runs on Windows, Linux, macOS
   - ✅ No platform-specific code in services
   - ✅ File I/O platform-agnostic

5. **Documentation**
   - ✅ All APIs documented
   - ✅ Migration guides complete
   - ✅ Integration examples provided
   - ✅ Architecture diagrams updated

6. **Validation**
   - ✅ Tested with real GPS hardware
   - ✅ Field tested in real conditions
   - ✅ Performance validated
   - ✅ User acceptance achieved

---

## Conclusion

This specification provides a complete blueprint for systematically extracting 20,000+ lines of business logic from AgOpenGPS WinForms into clean, testable services for AgValoniaGPS. The 8-wave approach ensures orderly extraction, maintaining functionality at each stage while building toward a truly cross-platform, maintainable architecture.

The success of this project will enable:
- True cross-platform deployment (Windows, Linux, macOS)
- Comprehensive unit testing of business logic
- Future UI implementations (Qt, web, mobile)
- Independent evolution of UI and business layers
- Long-term maintainability and extensibility

By following the test-first extraction methodology and strict architectural principles, we will achieve 100% behavioral compatibility while establishing a foundation for the next decade of AgValoniaGPS development.

---

**Document Version**: 1.0
**Date**: 2025-10-17
**Status**: Approved for Implementation
**Next Review**: After Wave 1 completion
