# Specification: Wave 2 - Guidance Line Core

## Goal

Extract and implement three independent guidance line services (ABLineService, CurveLineService, ContourService) from AgOpenGPS into clean, testable, event-driven services optimized for 20-25 Hz operation with modern curve smoothing algorithms.

## User Stories

- As a farmer, I want to create straight AB guidance lines from two points or a heading so that I can perform precise parallel passes across my field
- As a farmer, I want to follow curved guidance paths so that I can efficiently work irregular field shapes and follow terrain contours
- As a farmer, I want to record and follow contour lines in real-time so that I can maintain consistent offset distances during field operations
- As a developer, I want loosely coupled guidance services so that I can test, maintain, and enhance guidance algorithms independently
- As a system integrator, I want guidance services that emit state change events so that UI components can react without tight coupling

## Core Requirements

### Functional Requirements

**FR1: AB Line Service**
- Create straight AB line from two GPS positions (Point A and Point B)
- Create AB line from single point and heading angle
- Calculate perpendicular cross-track error from vehicle position to AB line
- Find closest point on AB line to current vehicle position
- Generate parallel AB lines at specified spacing (metric or imperial units)
- Nudge/offset AB line by specified distance (positive or negative)
- Calculate on-line target point for steering algorithms
- Support snap distance logic to activate line when vehicle approaches
- Validate AB line (minimum length, valid points, non-zero angle)
- Emit events when AB line created, modified, or activated

**FR2: Curve Line Service**
- Record curved path from sequence of GPS positions
- Apply improved curve smoothing using cubic splines (MathNet.Numerics)
- Calculate cross-track error from vehicle position to curved path
- Find closest point on curve to current vehicle position (global and local search)
- Generate parallel curves at specified spacing (metric or imperial units)
- Calculate tangent heading at closest point for steering guidance
- Handle curves with varying point density
- Support configurable smoothing parameters
- Validate curve quality (minimum points, point spacing, smoothness)
- Emit events when curve recorded, smoothed, or activated

**FR3: Contour Service**
- Start real-time contour recording from initial position
- Add points to recording with configurable minimum distance threshold
- Lock/finalize contour for guidance use
- Calculate offset from vehicle position to contour path
- Update guidance target as vehicle moves along contour
- Validate contour (minimum length, point quality, consistent spacing)
- Handle contour recording at field boundaries/edges
- Support contour following with real-time cross-track error
- Calculate look-ahead point on contour for smooth following
- Emit events during recording, locking, and guidance updates

**FR4: Unit System Support**
- Accept spacing values in meters or feet based on user configuration
- Return cross-track error in configured units
- Handle minimum distance thresholds in configured units
- Perform internal calculations in consistent base unit (meters)
- Support runtime unit switching without data loss

**FR5: Performance Requirements**
- All calculations complete within time budget for 20-25 Hz operation
- Cross-track error calculations optimized for high-frequency updates (<5ms per call)
- Parallel line generation performed efficiently (cached when appropriate)
- Curve smoothing performed once at creation, not on every update
- Memory efficient for long-duration field operations (8+ hours)

**FR6: Data Persistence Integration**
- AB lines serializable for storage via FieldService extension
- Curve lines serializable for storage via FieldService extension
- Contours serializable for storage via FieldService extension
- Maintain compatibility with AgOpenGPS field file formats for migration
- Support deserialization of existing AgOpenGPS guidance lines

### Non-Functional Requirements

**Performance**
- Cross-track error calculation: <5ms per call
- Parallel line generation: <50ms for 10 lines
- Curve smoothing: <200ms for 1000-point curve
- Memory footprint: <50MB per guidance line set
- Thread-safe for concurrent access from UI and calculation threads

**Reliability**
- Handle degenerate cases gracefully (zero-length lines, duplicate points, NaN values)
- Prevent numeric overflow/underflow in trigonometric calculations
- Validate all input parameters before processing
- No crashes on invalid input data

**Maintainability**
- Interface-based design for all services (IABLineService, ICurveLineService, IContourService)
- Dependency injection pattern following Wave 1 conventions
- Comprehensive XML documentation on all public APIs
- Clear separation of concerns (calculation vs. state management)

**Testability**
- Unit test coverage >80% on all services
- Edge case testing prioritized (boundaries, zero-length, point density)
- Pure functions for mathematical calculations where possible
- Mockable dependencies for integration testing

## Visual Design

**Mockup References:**
- `planning/visuals/Straight A_B Line.png` - Demonstrates straight AB line guidance with parallel lines, cross-track visualization, and vehicle tracking
- `planning/visuals/Curved A_B Line.png` - Shows curved guidance line with smooth interpolation, magenta line rendering, and contour following

**Key UI Elements (Data to Provide):**
- Line coordinates for rendering (start/end points or curve point list)
- Cross-track error value (signed distance from line)
- Closest point on line for target visualization
- Parallel line coordinates for coverage visualization
- Active line indication (which line vehicle is following)
- Line heading at closest point for heading error calculation

**Responsive Considerations:**
- Services are UI-agnostic; UI layer handles all rendering
- Event-driven updates allow UI to refresh on state changes
- Data models provide all information needed for visualization

## Reusable Components

### Existing Code to Leverage

**Wave 1 Services (Completed):**
- Wave 1 position and kinematics services exist in `SourceCode/AgOpenGPS.Core/Services/GPS/` (based on documentation reference)
- Event-driven service pattern established
- Dependency injection registration pattern via ServiceCollectionExtensions
- Position, heading, and vehicle kinematics data passed as method parameters (loosely coupled)

**Existing AgOpenGPS.Core Components:**
- `Models/Field/Contour.cs` - Contour data model (can extend or reference)
- `Streamers/Field/ContourStreamer.cs` - Contour file I/O pattern
- Field service infrastructure for persistence

**Mathematical Libraries:**
- MathNet.Numerics - Use for cubic spline interpolation, Catmull-Rom splines, and advanced curve smoothing
- System.Numerics.Vectors - Use for optimized vector calculations where appropriate

**Testing Infrastructure:**
- NUnit 4.3.2 framework (established in Wave 1)
- AAA (Arrange, Act, Assert) pattern
- Assert.That syntax convention
- Test project structure in `SourceCode/AgLibrary.Tests/` and `SourceCode/AgOpenGPS.Core.Tests/`

### New Components Required

**IABLineService / ABLineService**
- No existing service for AB line guidance logic exists
- Original logic in `SourceCode/GPS/Classes/CABLine.cs` is tightly coupled to FormGPS
- Need clean extraction with improved separation of concerns

**ICurveLineService / CurveLineService**
- No existing service for curve line guidance exists
- Original logic in `SourceCode/GPS/Classes/CABCurve.cs` uses basic curve smoothing
- Need improved curve smoothing using MathNet.Numerics for better quality

**IContourService / ContourService**
- No existing service for contour guidance exists
- Original logic in `SourceCode/GPS/Classes/CContour.cs` is embedded in form logic
- Need extraction with configurable parameters and event-driven architecture

**Data Models:**
- ABLine (origin, heading, length, nudge offset)
- CurveLine (point list, smoothing parameters, heading interpolation)
- ContourLine (recorded points, lock state, offset distance)
- GuidanceLineResult (cross-track error, closest point, heading error)

**Events:**
- ABLineChanged (line created, modified, activated, nudged)
- CurveLineChanged (curve recorded, smoothed, activated)
- ContourStateChanged (recording started, point added, contour locked)
- GuidanceStateChanged (active line changed, XTE updated)

## Technical Approach

### Database

**No database changes required** - All data persisted via FieldService extension:
- Extend existing FieldService to save/load guidance line data
- Maintain AgOpenGPS field file format compatibility for migration
- Use existing file I/O patterns from `Streamers/Field/` namespace
- Store guidance lines in field-specific files (e.g., FieldName.ABL, FieldName.CRV, FieldName.CNT)

### API

**Service Interfaces:**

```csharp
public interface IABLineService
{
    event EventHandler<ABLineChangedEventArgs> ABLineChanged;

    // Creation
    ABLine CreateFromPoints(Position pointA, Position pointB, string name);
    ABLine CreateFromHeading(Position origin, double headingRadians, string name);

    // Calculations
    GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, ABLine line);
    Position GetClosestPoint(Position currentPosition, ABLine line);

    // Manipulation
    ABLine NudgeLine(ABLine line, double offsetMeters);
    List<ABLine> GenerateParallelLines(ABLine referenceLine, double spacingMeters, int count, UnitSystem units);

    // Validation
    ValidationResult ValidateLine(ABLine line);
}

public interface ICurveLineService
{
    event EventHandler<CurveLineChangedEventArgs> CurveChanged;

    // Recording
    void StartRecording(Position startPosition);
    void AddPoint(Position point, double minDistanceMeters);
    CurveLine FinishRecording(string name);

    // Calculations
    GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, CurveLine curve, bool findGlobal = false);
    Position GetClosestPoint(Position currentPosition, CurveLine curve, int searchStartIndex = -1);
    double GetHeadingAtPoint(Position point, CurveLine curve);

    // Processing
    CurveLine SmoothCurve(CurveLine curve, SmoothingParameters parameters);
    List<CurveLine> GenerateParallelCurves(CurveLine referenceCurve, double spacingMeters, int count, UnitSystem units);

    // Validation
    ValidationResult ValidateCurve(CurveLine curve);
}

public interface IContourService
{
    event EventHandler<ContourStateChangedEventArgs> StateChanged;

    // Recording
    void StartRecording(Position startPosition, double minDistanceMeters);
    void AddPoint(Position currentPosition, double offset);
    ContourLine LockContour(string name);
    void StopRecording();

    // Calculations
    GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, ContourLine contour);
    double CalculateOffset(Position currentPosition, ContourLine contour);

    // State Management
    bool IsRecording { get; }
    bool IsLocked { get; }
    void SetLocked(bool locked);

    // Validation
    ValidationResult ValidateContour(ContourLine contour);
}
```

**Data Flow:**
1. UI or automation system receives position/heading from Wave 1 services (PositionUpdateService, HeadingCalculatorService)
2. UI passes position/heading as parameters to guidance service methods (loose coupling)
3. Guidance service calculates cross-track error, closest point, heading error
4. Service emits GuidanceStateChanged event with results
5. UI/ViewModels subscribe to events and update display/steering commands

### Frontend

**Not applicable** - This is service layer only. UI integration is Wave 3 scope.

**Data provided for UI rendering:**
- Line/curve coordinates (for visual rendering)
- Cross-track error (signed meters)
- Closest point on line/curve
- Heading error (radians)
- Active line indication
- Parallel line coordinates

### Testing

**Unit Test Requirements (>80% coverage):**

```csharp
// ABLineService Tests
[TestFixture]
public class ABLineServiceTests
{
    [Test]
    public void CreateFromPoints_ValidPoints_CreatesLine()
    {
        // Arrange
        var service = new ABLineService();
        var pointA = new Position(1000.0, 2000.0, 0.0);
        var pointB = new Position(1100.0, 2100.0, 0.0);

        // Act
        var line = service.CreateFromPoints(pointA, pointB, "Test Line");

        // Assert
        Assert.That(line, Is.Not.Null);
        Assert.That(line.Name, Is.EqualTo("Test Line"));
        Assert.That(line.PointA.Easting, Is.EqualTo(1000.0).Within(0.001));
        Assert.That(line.PointB.Easting, Is.EqualTo(1100.0).Within(0.001));
    }

    [Test]
    public void CreateFromPoints_IdenticalPoints_ThrowsValidationException()
    {
        // Arrange
        var service = new ABLineService();
        var pointA = new Position(1000.0, 2000.0, 0.0);

        // Act & Assert
        Assert.Throws<ValidationException>(() =>
            service.CreateFromPoints(pointA, pointA, "Invalid"));
    }

    [Test]
    public void CalculateGuidance_OnLine_ReturnsZeroXTE()
    {
        // Arrange
        var service = new ABLineService();
        var line = service.CreateFromPoints(
            new Position(0.0, 0.0, 0.0),
            new Position(100.0, 0.0, 0.0),
            "Test"
        );
        var currentPos = new Position(50.0, 0.0, 0.0);

        // Act
        var result = service.CalculateGuidance(currentPos, 0.0, line);

        // Assert
        Assert.That(result.CrossTrackError, Is.EqualTo(0.0).Within(0.01));
    }

    [Test]
    public void CalculateGuidance_RightOfLine_ReturnsPositiveXTE()
    {
        // Arrange
        var service = new ABLineService();
        var line = service.CreateFromPoints(
            new Position(0.0, 0.0, 0.0),
            new Position(100.0, 0.0, 0.0),
            "Test"
        );
        var currentPos = new Position(50.0, -5.0, 0.0); // 5m to the right

        // Act
        var result = service.CalculateGuidance(currentPos, 0.0, line);

        // Assert
        Assert.That(result.CrossTrackError, Is.EqualTo(5.0).Within(0.01));
    }

    [Test]
    public void GenerateParallelLines_ValidSpacing_CreatesCorrectOffsets()
    {
        // Arrange
        var service = new ABLineService();
        var referenceLine = service.CreateFromPoints(
            new Position(0.0, 0.0, 0.0),
            new Position(100.0, 0.0, 0.0),
            "Reference"
        );

        // Act
        var parallels = service.GenerateParallelLines(referenceLine, 10.0, 3, UnitSystem.Metric);

        // Assert
        Assert.That(parallels.Count, Is.EqualTo(6)); // 3 left + 3 right
        // Verify spacing is exactly 10m apart
    }

    [Test]
    public void NudgeLine_PositiveOffset_MovesLineCorrectly()
    {
        // Arrange
        var service = new ABLineService();
        var originalLine = service.CreateFromPoints(
            new Position(0.0, 0.0, 0.0),
            new Position(100.0, 0.0, 0.0),
            "Original"
        );

        // Act
        var nudgedLine = service.NudgeLine(originalLine, 2.5);

        // Assert
        Assert.That(nudgedLine.NudgeOffset, Is.EqualTo(2.5));
        // Verify line moved perpendicular to heading
    }
}

// CurveLineService Tests
[TestFixture]
public class CurveLineServiceTests
{
    [Test]
    public void SmoothCurve_WithCubicSpline_ProducesSmootherPath()
    {
        // Arrange
        var service = new CurveLineService();
        var rawPoints = CreateJaggedCurvePoints();
        var curve = new CurveLine { Points = rawPoints };
        var smoothingParams = new SmoothingParameters
        {
            Method = SmoothingMethod.CubicSpline,
            Tension = 0.5
        };

        // Act
        var smoothedCurve = service.SmoothCurve(curve, smoothingParams);

        // Assert
        Assert.That(smoothedCurve.Points.Count, Is.GreaterThan(rawPoints.Count));
        // Verify curvature is smoother (measure heading change variance)
    }

    [Test]
    public void CalculateGuidance_FindGlobal_FindsNearestPointAcrossEntireCurve()
    {
        // Arrange
        var service = new CurveLineService();
        var curve = CreateCircularCurve(100.0); // 100m radius circle
        var farAwayPosition = new Position(200.0, 200.0, 0.0);

        // Act
        var result = service.CalculateGuidance(farAwayPosition, 0.0, curve, findGlobal: true);

        // Assert
        Assert.That(result.ClosestPointIndex, Is.GreaterThanOrEqualTo(0));
        // Verify found the globally closest point
    }

    [Test]
    public void GenerateParallelCurves_ComplexCurve_MaintainsOffsetDistance()
    {
        // Arrange
        var service = new CurveLineService();
        var referenceCurve = CreateSCurve();

        // Act
        var parallels = service.GenerateParallelCurves(referenceCurve, 12.0, 2, UnitSystem.Metric);

        // Assert
        Assert.That(parallels.Count, Is.EqualTo(4)); // 2 left + 2 right
        // Verify offset distance is maintained along entire curve
    }
}

// ContourService Tests
[TestFixture]
public class ContourServiceTests
{
    [Test]
    public void StartRecording_ValidPosition_InitializesRecording()
    {
        // Arrange
        var service = new ContourService();
        var startPos = new Position(1000.0, 2000.0, 0.0);

        // Act
        service.StartRecording(startPos, minDistanceMeters: 0.5);

        // Assert
        Assert.That(service.IsRecording, Is.True);
        Assert.That(service.IsLocked, Is.False);
    }

    [Test]
    public void AddPoint_WithinMinDistance_SkipsPoint()
    {
        // Arrange
        var service = new ContourService();
        service.StartRecording(new Position(0.0, 0.0, 0.0), minDistanceMeters: 1.0);
        int pointCount = 0;
        service.StateChanged += (s, e) => { if (e.Type == ContourEventType.PointAdded) pointCount++; };

        // Act
        service.AddPoint(new Position(0.2, 0.2, 0.0), offset: 0.0); // <1m away

        // Assert
        Assert.That(pointCount, Is.EqualTo(0)); // Point not added
    }

    [Test]
    public void AddPoint_ExceedsMinDistance_AddsPoint()
    {
        // Arrange
        var service = new ContourService();
        service.StartRecording(new Position(0.0, 0.0, 0.0), minDistanceMeters: 1.0);
        int pointCount = 0;
        service.StateChanged += (s, e) => { if (e.Type == ContourEventType.PointAdded) pointCount++; };

        // Act
        service.AddPoint(new Position(2.0, 0.0, 0.0), offset: 0.0); // 2m away

        // Assert
        Assert.That(pointCount, Is.EqualTo(1)); // Point added
    }

    [Test]
    public void LockContour_SufficientPoints_LocksSuccessfully()
    {
        // Arrange
        var service = new ContourService();
        service.StartRecording(new Position(0.0, 0.0, 0.0), minDistanceMeters: 1.0);
        for (int i = 1; i <= 10; i++)
        {
            service.AddPoint(new Position(i * 2.0, 0.0, 0.0), offset: 0.0);
        }

        // Act
        var contour = service.LockContour("Test Contour");

        // Assert
        Assert.That(contour, Is.Not.Null);
        Assert.That(service.IsLocked, Is.True);
        Assert.That(contour.Name, Is.EqualTo("Test Contour"));
    }
}
```

**Edge Case Tests (Prioritized):**
- Zero-length AB line creation (identical points)
- AB line at exactly 0/90/180/270 degrees
- Parallel lines at vehicle boundary extremes
- Curve with too few points (<3)
- Curve with points too close together (<0.1m)
- Curve recording at field boundary intersection
- Contour with duplicate consecutive points
- Contour lock with insufficient points
- Contour following past end of recorded line
- NaN/Infinity values in position inputs
- Numeric overflow in distance calculations

**Performance Tests:**
- XTE calculation at 25 Hz for 60 seconds (sustained load)
- Parallel line generation with 20 lines in <100ms
- Curve smoothing with 2000 points in <500ms
- Memory leak test over 8-hour simulation

## Out of Scope

**Explicitly NOT Included:**
- UI integration (ViewModels, Views, user interactions) - Wave 3
- Steering algorithm integration - Wave 3
- Section control based on guidance lines - Wave 4
- Field boundary collision detection - Wave 5
- Auto-steering activation/deactivation logic - Wave 3
- Hardware communication (PGN messages) - Wave 6
- Field file format changes (maintain compatibility)
- Migration tools for existing fields (use FieldService as-is)
- Rendering logic (OpenGL, Avalonia drawing) - UI layer responsibility
- Configuration UI for smoothing parameters - UI layer

**Future Enhancements (Post Wave 2):**
- Advanced curve optimization algorithms
- Machine learning for path prediction
- Automatic curve simplification
- Multi-curve blending for field transitions
- Prescription map integration with guidance
- Cloud-based guidance line sharing

## Success Criteria

**Code Quality:**
- All services implement interfaces (IABLineService, ICurveLineService, IContourService)
- Dependency injection registration in ServiceCollectionExtensions
- Comprehensive XML documentation on all public APIs
- No UI framework dependencies (Avalonia, WinForms, etc.)
- All calculations in meters internally, unit conversion at API boundary

**Testing:**
- >80% unit test coverage on all services
- All edge cases covered with tests
- Performance benchmarks pass (<5ms XTE, <50ms parallels, <200ms smoothing)
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

**Documentation:**
- All public APIs have XML documentation
- Usage examples in API comments
- Migration notes from AgOpenGPS CABLine/CABCurve/CContour
- Architecture diagrams updated to show Wave 2 services
- Test coverage report generated

---

**Relevant File Paths:**

**Source Code to Reference:**
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\GPS\Classes\CABLine.cs` - Original AB line logic
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\GPS\Classes\CABCurve.cs` - Original curve line logic
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\GPS\Classes\CContour.cs` - Original contour logic
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\GPS\Forms\Guidance\FormABLine.cs` - AB line creation UI (extract logic only)
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\GPS\Forms\Guidance\FormABCurve.cs` - Curve creation UI (extract logic only)

**Wave 1 Reference:**
- `C:\Users\chrisk\Documents\AgValoniaGPS\agent-os\specs\20251017-business-logic-extraction\spec.md` - Service architecture patterns

**Visual References:**
- `C:\Users\chrisk\Documents\AgValoniaGPS\agent-os\specs\2025-10-17-wave-2-guidance-line-core\planning\visuals\Straight A_B Line.png`
- `C:\Users\chrisk\Documents\AgValoniaGPS\agent-os\specs\2025-10-17-wave-2-guidance-line-core\planning\visuals\Curved A_B Line.png`

**Target Service Location:**
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\AgOpenGPS.Core\Services\Guidance\` (new directory)

**Target Test Location:**
- `C:\Users\chrisk\Documents\AgValoniaGPS\SourceCode\AgOpenGPS.Core.Tests\Services\Guidance\` (new directory)
