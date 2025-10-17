# Feature Extraction Roadmap for AgValoniaGPS

## Executive Summary

This roadmap provides a detailed, actionable plan for systematically extracting 50+ embedded business features from AgOpenGPS WinForms into clean, testable services for AgValoniaGPS. Based on analysis of ~20,000 lines of embedded business logic, features are organized into dependency-ordered waves for safe, incremental extraction.

## Extraction Principles

1. **No Code Copying**: Reference original for behavior, rewrite clean
2. **Test First**: Write tests based on expected behavior before extraction
3. **Dependency Order**: Extract foundational features before dependent ones
4. **Maintain Functionality**: Each extraction must preserve original behavior
5. **Clean Architecture**: Services must be UI-agnostic and testable

## Feature Extraction Waves

### 🌊 Wave 0: Already Completed ✅
These features have already been extracted in the current AgValoniaGPS implementation:

| Feature | Original Location | New Service | Status |
|---------|------------------|------------|--------|
| NMEA Parsing | FormGPS ParseNMEA() | NmeaParserService | ✅ Complete |
| UDP Communication | FormGPS SendPgnToLoop() | UdpCommunicationService | ✅ Complete |
| GPS Data Model | FormGPS pn.* variables | GpsData model | ✅ Complete |
| Field File I/O | FormGPS FileSaveField() | FieldService + FileServices | ✅ Complete |
| Boundary Polygon | CBoundary | Boundary model | ✅ Complete |
| NTRIP Client | FormNtrip | NtripClientService | ✅ Complete |
| Module Monitoring | FormGPS hello tracking | UdpCommunicationService | ✅ Complete |

---

### 🌊 Wave 1: Core Position & Kinematics (Weeks 1-2)

**Dependencies**: None (Foundation Layer)
**Complexity**: CRITICAL
**Total LOC to Extract**: ~2,500

#### Feature 1.1: Position Update Pipeline
**Source**: `FormGPS/Position.designer.cs UpdateFixPosition()` (Lines 128-1200)

**Extract to Service**: `PositionUpdateService`

**Business Logic to Extract**:
```csharp
public interface IPositionUpdateService
{
    event EventHandler<PositionUpdate> PositionUpdated;

    void ProcessGpsPosition(GpsData gpsData, ImuData imuData);
    Position GetCurrentPosition();
    double GetCurrentHeading();
    double GetCurrentSpeed();
    bool IsReversing();
}
```

**Specific Calculations**:
- GPS position smoothing and filtering
- Speed calculation from position history
- Direction reversal detection
- Position history management (step array)

**Test Cases**:
- [ ] Position updates at 10Hz
- [ ] Speed calculation accuracy
- [ ] Reverse detection threshold
- [ ] History buffer management

#### Feature 1.2: Heading Calculation System
**Source**: `FormGPS/Position.designer.cs` (Lines 190-810)

**Extract to Service**: `HeadingCalculatorService`

**Business Logic to Extract**:
```csharp
public interface IHeadingCalculatorService
{
    double CalculateHeading(HeadingSource source);
    double FuseImuHeading(double gpsHeading, double imuHeading, double fusionWeight);
    double CompensateForRoll(double heading, double roll, double antennaHeight);
    HeadingSource DetermineOptimalSource(double speed, bool hasDual, bool hasImu);
}
```

**Complex Logic**:
- Fix-to-fix heading calculation
- VTG heading processing
- Dual antenna heading
- IMU fusion with weighted averaging
- Roll compensation mathematics

**Validation Criteria**:
- [ ] Heading accuracy within 0.1 degrees
- [ ] Smooth transitions between sources
- [ ] Correct circular math (0-360 wrapping)

#### Feature 1.3: Vehicle Kinematics Engine
**Source**: `FormGPS/Position.designer.cs CalculatePositionHeading()` (Lines 1271-1409)

**Extract to Service**: `VehicleKinematicsService`

**Business Logic to Extract**:
```csharp
public interface IVehicleKinematicsService
{
    PivotPosition CalculatePivotPosition(Position gps, double heading, VehicleConfig config);
    SteerPosition CalculateSteerAxle(PivotPosition pivot, double heading, VehicleConfig config);
    ToolPosition CalculateToolPosition(PivotPosition pivot, double heading, ToolConfig config);
    HitchPosition CalculateHitchPosition(SteerPosition steer, double heading, VehicleConfig config);

    // Complex multi-joint kinematics
    TankPosition CalculateTankPosition(HitchPosition hitch, double heading, double hitchAngle);
    bool IsJackknifed(double hitchAngle, double threshold);
}
```

**Mathematical Formulas**:
```
pivotEasting = gpsEasting - (sin(heading) * antennaPivot)
pivotNorthing = gpsNorthing - (cos(heading) * antennaPivot)
steerEasting = pivotEasting + (sin(heading) * wheelbase)
toolEasting = hitchEasting + (sin(toolHeading) * hitchLength)
```

**Edge Cases**:
- [ ] Jackknife prevention
- [ ] Multi-articulated tools
- [ ] Negative antenna offsets
- [ ] Zero wheelbase (tracked vehicles)

---

### 🌊 Wave 2: Guidance Line Core (Weeks 3-4)

**Dependencies**: Wave 1 (Position, Heading)
**Complexity**: HIGH
**Total LOC to Extract**: ~2,000

#### Feature 2.1: AB Line Management
**Source**: `FormGPS CABLine.cs`, `FormABLine.cs`

**Extract to Service**: `ABLineService`

**Business Logic to Extract**:
```csharp
public interface IABLineService
{
    ABLine CreateFromPoints(Position pointA, Position pointB);
    ABLine CreateFromHeading(Position origin, double heading);
    double CalculateCrossTrackError(Position current, ABLine line);
    Position GetOnlinePoint(Position current, ABLine line);
    ABLine NudgeLine(ABLine line, double offsetMeters);
    List<ABLine> GetParallelLines(ABLine reference, double spacing, int count);
}
```

**Algorithms**:
- Point-to-line perpendicular distance
- Line equation from two points
- Parallel line generation
- Snap-to-line calculations

#### Feature 2.2: Curve Line System
**Source**: `FormGPS CABCurve.cs`, `FormABCurve.cs`

**Extract to Service**: `CurveLineService`

**Business Logic to Extract**:
```csharp
public interface ICurveLineService
{
    CurveLine RecordCurve(List<Position> points);
    double CalculateCrossTrackError(Position current, CurveLine curve);
    Position GetClosestPoint(Position current, CurveLine curve);
    CurveLine SmoothCurve(CurveLine curve, double smoothingFactor);
    List<CurveLine> GenerateParallelCurves(CurveLine reference, double spacing);
}
```

**Complex Algorithms**:
- Cubic spline interpolation
- Curve smoothing algorithms
- Closest point on curve
- Curve offset calculations

#### Feature 2.3: Contour Following
**Source**: `FormGPS CContour.cs`

**Extract to Service**: `ContourService`

**Business Logic to Extract**:
```csharp
public interface IContourService
{
    ContourLine StartRecording(Position start);
    void AddPoint(Position point, double minDistance);
    double CalculateOffset(Position current, ContourLine contour);
    ContourLine LockContour(ContourLine recording);
    void UpdateGuidance(Position current, ContourLine locked);
}
```

---

### 🌊 Wave 3: Steering Algorithms (Weeks 4-5)

**Dependencies**: Waves 1-2 (Position, Guidance Lines)
**Complexity**: HIGH
**Total LOC to Extract**: ~1,500

#### Feature 3.1: Stanley Steering Algorithm
**Source**: `FormGPS/Position.designer.cs` (Lines 871-912)

**Extract to Service**: `StanleySteeringService`

**Business Logic to Extract**:
```csharp
public interface IStanleySteeringService
{
    double CalculateSteerAngle(
        double crossTrackError,
        double headingError,
        double speed,
        StanleyParameters parameters);

    StanleyParameters AutoTune(
        List<SteeringSample> samples,
        VehicleConfig vehicle);
}

public class StanleyParameters
{
    public double DistanceGain { get; set; }  // Typically 0.8
    public double HeadingGain { get; set; }   // Typically 1.0
    public double IntegralGain { get; set; }  // Typically 0.0
}
```

**Stanley Formula**:
```
steerAngle = headingError * headingGain +
             atan(distanceGain * crossTrackError / speed)
```

#### Feature 3.2: Pure Pursuit Algorithm
**Source**: `FormGPS CVehicle.cs`

**Extract to Service**: `PurePursuitService`

**Business Logic to Extract**:
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

#### Feature 3.3: Look-Ahead Distance Calculator
**Source**: `FormGPS/Position.designer.cs` (Lines 1350-1380)

**Extract to Service**: `LookAheadService`

**Business Logic to Extract**:
```csharp
public interface ILookAheadService
{
    double CalculateLookAheadDistance(
        double speed,
        double crossTrackError,
        double toolWidth,
        LookAheadMode mode);

    double CalculateLookAheadTime(
        double distance,
        double speed);
}
```

**Modes**:
- Tool width multiplier mode
- Time-based mode
- Hold mode (constant distance)

---

### 🌊 Wave 4: Section Control (Weeks 5-6)

**Dependencies**: Waves 1-3
**Complexity**: HIGH
**Total LOC to Extract**: ~2,000

#### Feature 4.1: Section State Machine
**Source**: `FormGPS CSection.cs`, various event handlers

**Extract to Service**: `SectionControlService`

**Business Logic to Extract**:
```csharp
public interface ISectionControlService
{
    event EventHandler<SectionStateChange> SectionStateChanged;

    void UpdateSectionStates(
        Position current,
        Boundary boundary,
        double lookAheadDistance);

    bool[] GetSectionStates();
    void ManualToggleSection(int sectionIndex);
    void SetMasterSwitch(bool on);
    void SetAutoMode(bool auto);
}
```

**State Logic**:
- Boundary checking per section
- Look-ahead for turn on/off
- Overlap detection
- Manual override handling

#### Feature 4.2: Coverage Mapping
**Source**: `FormGPS/Position.designer.cs AddSectionOrPathPoints()` (Lines 1629-1668)

**Extract to Service**: `CoverageMapService`

**Business Logic to Extract**:
```csharp
public interface ICoverageMapService
{
    void RecordCoverage(
        Position position,
        bool[] sectionStates,
        double[] sectionWidths);

    double CalculateWorkedArea();
    double CalculateOverlapArea();
    CoverageMap GetCoverageMap();
    void StartNewPatch();
}
```

**Patch Management**:
- Triangle strip generation
- Color coding by time/section
- Overlap calculation
- Area accumulation

#### Feature 4.3: Section Speed Calculation
**Source**: `FormGPS/Position.designer.cs CalculateSectionLookAhead()` (Lines 1412-1508)

**Extract to Service**: `SectionSpeedService`

**Business Logic to Extract**:
```csharp
public interface ISectionSpeedService
{
    SectionSpeeds CalculateSectionSpeeds(
        Position current,
        double toolWidth,
        int sectionCount,
        double vehicleSpeed);

    double GetLookAheadTime(int section, double speed);
}
```

---

### 🌊 Wave 5: Field Operations (Weeks 6-7)

**Dependencies**: Waves 1-4
**Complexity**: MEDIUM
**Total LOC to Extract**: ~1,800

#### Feature 5.1: Boundary Management
**Source**: `FormGPS CBoundary.cs`, `FormBoundary.cs`

**Extract to Service**: `BoundaryManagementService`

**Business Logic to Extract**:
```csharp
public interface IBoundaryManagementService
{
    void StartRecording(BoundaryType type);
    void AddPoint(Position point, double minDistance);
    Boundary FinishRecording();

    bool IsInside(Position point, Boundary boundary);
    double DistanceToBoundary(Position point, Boundary boundary);
    Position GetClosestBoundaryPoint(Position point, Boundary boundary);

    double CalculateArea(Boundary boundary);
    Boundary SimplifyBoundary(Boundary boundary, double tolerance);
}
```

**Algorithms**:
- Ray-casting point-in-polygon
- Douglas-Peucker simplification
- Shoelace area formula
- Boundary offset generation

#### Feature 5.2: Headland Processing
**Source**: `FormGPS CHeadland.cs`, `FormHeadland.cs`

**Extract to Service**: `HeadlandService`

**Business Logic to Extract**:
```csharp
public interface IHeadlandService
{
    Headland CreateFromBoundary(Boundary boundary, double width, int passes);
    bool IsInHeadland(Position point, Headland headland);
    void SetHeadlandMode(HeadlandMode mode);
    Position GetHeadlandEntryPoint(Position current, double heading);
}
```

#### Feature 5.3: U-Turn Generation
**Source**: `FormGPS/Position.designer.cs` U-Turn logic (Lines 1073-1185)

**Extract to Service**: `UTurnService`

**Business Logic to Extract**:
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

**Dubins Path Algorithm**:
- Minimum radius path planning
- Multiple turn patterns
- Boundary proximity triggers

---

### 🌊 Wave 6: Hardware Communication (Weeks 7-8)

**Dependencies**: Waves 1-5 (for data to send)
**Complexity**: MEDIUM
**Total LOC to Extract**: ~1,200

#### Feature 6.1: PGN Message Builder
**Source**: `FormGPS SendPgnToLoop()`, various send methods

**Extract to Service**: `PgnMessageBuilderService`

**Business Logic to Extract**:
```csharp
public interface IPgnMessageBuilderService
{
    byte[] BuildAutoSteerData(
        double steerAngle,
        double speed,
        bool enabled);

    byte[] BuildMachineData(
        bool[] sectionStates,
        double rate,
        bool masterSwitch);

    byte[] BuildSettingsMessage(
        VehicleConfig vehicle,
        SteerSettings steer);

    byte[] BuildFromData(int pgn, byte[] data);
}
```

**Protocol Details**:
- Header: [0x80, 0x81, source, PGN, length]
- CRC calculation
- Byte packing for different data types

#### Feature 6.2: Hardware Settings Manager
**Source**: `FormGPS SendSettings()` (Lines 1158-1197)

**Extract to Service**: `HardwareSettingsService`

**Business Logic to Extract**:
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

---

### 🌊 Wave 7: Display & Visualization (Weeks 8-9)

**Dependencies**: All previous waves
**Complexity**: LOW
**Total LOC to Extract**: ~1,000

#### Feature 7.1: Field Statistics Calculator
**Source**: Various calculations throughout FormGPS

**Extract to Service**: Already partially done as `FieldStatisticsService`

**Expand with**:
```csharp
public interface IFieldStatisticsCalculator
{
    FieldStats Calculate(Field field, CoverageMap coverage);
    string FormatArea(double hectares, UnitSystem units);
    string FormatDistance(double meters, UnitSystem units);
    double EstimateTimeRemaining(double remainingArea, double workRate);
}
```

#### Feature 7.2: Display Formatters
**Source**: Various ToString() and display methods

**Extract to Service**: `DisplayFormatterService`

**Business Logic to Extract**:
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

---

### 🌊 Wave 8: State Management (Weeks 9-10)

**Dependencies**: All previous waves
**Complexity**: HIGH
**Total LOC to Extract**: ~1,500

#### Feature 8.1: Application State Machine
**Source**: `FormGPS JobNew(), JobClose()`, various state variables

**Extract to Service**: `ApplicationStateService`

**Business Logic to Extract**:
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

#### Feature 8.2: Timer Coordination Service
**Source**: `FormGPS tmrWatchdog_tick` and related timers

**Extract to Service**: `TimerCoordinatorService`

**Business Logic to Extract**:
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
- GPS watchdog (2 second timeout)
- Module data flow (100-300ms)
- Display updates (125ms)
- Auto-save (5 minutes)

---

## Extraction Validation Criteria

### For Each Extracted Feature:

#### Code Quality Checklist
- [ ] No WinForms references in service
- [ ] Interface defined before implementation
- [ ] Dependency injection ready
- [ ] No static state (except constants)
- [ ] Thread-safe where necessary

#### Testing Checklist
- [ ] Unit tests written first (TDD)
- [ ] Edge cases covered
- [ ] Performance benchmarked
- [ ] Integration test with UI
- [ ] Regression test against original

#### Documentation Checklist
- [ ] XML documentation on public API
- [ ] Algorithm explanation in comments
- [ ] Migration notes from original
- [ ] Usage examples provided

## Complexity & Effort Estimates

### Summary by Wave

| Wave | Features | Total LOC | Complexity | Duration | Dependencies |
|------|----------|-----------|-----------|----------|--------------|
| 0 | Already Done | ~3,000 | - | Complete | - |
| 1 | Position & Kinematics | ~2,500 | CRITICAL | 2 weeks | None |
| 2 | Guidance Lines | ~2,000 | HIGH | 2 weeks | Wave 1 |
| 3 | Steering Algorithms | ~1,500 | HIGH | 1.5 weeks | Waves 1-2 |
| 4 | Section Control | ~2,000 | HIGH | 2 weeks | Waves 1-3 |
| 5 | Field Operations | ~1,800 | MEDIUM | 1.5 weeks | Waves 1-4 |
| 6 | Hardware Comm | ~1,200 | MEDIUM | 1.5 weeks | Waves 1-5 |
| 7 | Display/Visual | ~1,000 | LOW | 1 week | All |
| 8 | State Management | ~1,500 | HIGH | 1.5 weeks | All |

**Total Timeline**: 10-12 weeks for complete extraction

### Risk Factors

#### High Risk Areas
1. **Position/Kinematics** - Core to everything, must be perfect
2. **Timer Coordination** - Complex interdependencies
3. **State Management** - Touches everything

#### Mitigation Strategies
- Start with Wave 1 (foundation)
- Extensive testing at each wave
- Keep original code as reference
- Regular integration testing
- Performance benchmarking

## Implementation Order

### Sprint 1 (Weeks 1-2): Foundation
- [ ] Extract Position Update Pipeline
- [ ] Extract Heading Calculator
- [ ] Extract Vehicle Kinematics
- [ ] Comprehensive unit tests
- [ ] Integration with existing UI

### Sprint 2 (Weeks 3-4): Guidance
- [ ] Extract AB Line logic
- [ ] Extract Curve Line logic
- [ ] Extract Contour logic
- [ ] Create guidance orchestrator
- [ ] Test with real GPS data

### Sprint 3 (Weeks 5-6): Control Systems
- [ ] Extract Stanley steering
- [ ] Extract Pure Pursuit
- [ ] Extract Section Control
- [ ] Extract Coverage Mapping
- [ ] Hardware output testing

### Sprint 4 (Weeks 7-8): Field Management
- [ ] Extract Boundary operations
- [ ] Extract Headland logic
- [ ] Extract U-Turn generation
- [ ] Extract Hardware messaging
- [ ] Field testing with equipment

### Sprint 5 (Weeks 9-10): Polish & State
- [ ] Extract Display formatters
- [ ] Extract State management
- [ ] Extract Timer coordination
- [ ] System integration testing
- [ ] Performance optimization

## Success Metrics

### Technical Metrics
- **Test Coverage**: >80% for extracted services
- **Performance**: Equal or better than original
- **Memory Usage**: <500MB working set
- **Latency**: <10ms service call overhead

### Architecture Metrics
- **Coupling**: No circular dependencies
- **Cohesion**: Single responsibility per service
- **Abstraction**: All services behind interfaces
- **Testability**: 100% unit testable

### Functional Metrics
- **Feature Parity**: 100% with original
- **Bug Rate**: <1 per 1000 LOC
- **Integration**: Seamless with Avalonia UI
- **Cross-Platform**: Runs on Windows/Linux/macOS

## Parallel Work Streams

While extraction proceeds, parallel efforts can include:

### UI Development Track
- Design new Avalonia UI screens
- Create ViewModels for each feature
- Implement data binding
- Build configuration dialogs

### Testing Track
- Set up test infrastructure
- Create test fixtures
- Build simulator/mocks
- Performance benchmarks

### Documentation Track
- API documentation
- User guides
- Migration guides
- Video tutorials

## Conclusion

This roadmap provides a systematic approach to extracting ~15,000 lines of embedded business logic from AgOpenGPS WinForms into 40+ clean, testable services. By following the dependency-ordered waves and validation criteria, AgValoniaGPS will achieve a maintainable, cross-platform architecture while preserving all original functionality.

The key to success is disciplined extraction in dependency order, comprehensive testing at each stage, and maintaining clear separation between UI and business logic throughout the process.

---

*Last Updated: Current Date*
*Total Features to Extract: 50+*
*Estimated Timeline: 10-12 weeks*
*Risk Level: Manageable with proper planning*