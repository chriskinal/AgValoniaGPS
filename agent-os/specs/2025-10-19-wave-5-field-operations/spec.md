# Specification: Wave 5 - Field Operations

## Executive Summary

Wave 5 implements field boundary management, headland processing, automated turn generation, and tram line services for the AgValoniaGPS precision agriculture application. This wave provides the backend business logic services that enable operators to define field boundaries, generate headlands for efficient field entry/exit, execute automated turns at field edges, and manage tram line patterns for controlled traffic farming.

**Wave Context:** This is Wave 5 of 8 in the AgValoniaGPS business logic extraction from AgOpenGPS. Previous waves completed Position & Kinematics (Wave 1), Guidance Line Core (Wave 2), Steering Algorithms (Wave 3), and Section Control (Wave 4).

**Key Deliverables:**
- 5 backend services with comprehensive interfaces
- Integration with Waves 1-4 services
- EventArgs-based state change notifications
- Multi-format file I/O (AgOpenGPS .txt, GeoJSON, KML)
- Performance-optimized geometric algorithms
- Comprehensive test coverage for all edge cases

**Scope:** Backend services only - no ViewModels, UI, or XAML in this wave.

## Goal

Provide robust, performant backend services for field boundary recording, headland generation, automated turn execution, and tram line management that integrate seamlessly with existing position tracking, guidance, steering, and section control services.

## User Stories

**Boundary Management:**
- As an operator, I want to record field boundaries using GPS while driving the perimeter so that I can define my working area
- As an operator, I want boundaries automatically simplified to reduce file size while maintaining accuracy so that my files are manageable
- As an operator, I want to validate boundaries for errors (self-intersection, minimum area) so that I catch problems early
- As an operator, I want to save boundaries in multiple formats (AgOpenGPS, GeoJSON, KML) so that I can share data with other systems

**Headland Operations:**
- As an operator, I want to generate headlands from my field boundary so that I have dedicated turn areas
- As an operator, I want multi-pass headlands with configurable width so that I can plan my field entry/exit strategy
- As an operator, I want to track which headland sections I've completed so that I know my progress
- As an operator, I want to adjust entry/exit points so that I can optimize my field access

**Automated Turns:**
- As an operator, I want automated turn patterns (Question Mark, Semi-Circle, Keyhole, T-turn, Y-turn) so that I can efficiently navigate field edges
- As an operator, I want sections to automatically pause during turns so that I don't apply product in turn areas
- As an operator, I want turn radius calculated from my vehicle configuration with override capability so that turns are safe and efficient
- As an operator, I want configurable turn trigger distance so that I can adjust when turns start based on my preferences

**Tram Lines:**
- As an operator, I want to generate tram line patterns from my AB line so that I can implement controlled traffic farming
- As an operator, I want configurable spacing between tram lines so that I can match my equipment configuration
- As an operator, I want multi-pass tram line support so that I can plan multiple operations

## Core Requirements

### Functional Requirements

**1. Boundary Management Service**
- Record boundaries in time-based mode (capture point every X seconds) or distance-based mode (capture point every X meters)
- Real-time area calculation during recording using Shoelace formula
- User-configurable simplification: auto-simplify during recording OR manual after completion using Douglas-Peucker algorithm
- Validation: minimum 3 points, maximum self-intersection tolerance (warn but allow), minimum area 100m², maximum point spacing
- Point-in-polygon detection delegated to PointInPolygonService
- Distance to boundary calculations (nearest edge, specific segment, perpendicular distance)
- File I/O: AgOpenGPS Boundary.txt format, GeoJSON import/export, KML import/export
- Support multiple boundaries per field, inner boundaries (holes), multi-part fields (non-contiguous polygons)

**2. Headland Service**
- Generate headlands from field boundaries using offset polygon algorithm
- Multi-pass headland generation stored as single multi-polygon structure
- User-configurable headland width per pass
- User-selectable overlap handling: prevent overlap OR allow slight overlap with configurable tolerance
- Automatic calculation of optimal entry/exit points with manual nudging capability
- Track completed headland sections (not purely geometric - integrates with coverage tracking)
- Smooth corner handling with configurable corner radius

**3. U-Turn Service**
- Five turn patterns: Question Mark, Semi-Circle, Keyhole, T-turn, Y-turn
- Dubins path algorithm for minimum radius path calculation
- Automatic turn radius calculation from vehicle configuration with user override capability
- Turn trigger detection based on distance to boundary, lookahead distance, and vehicle configuration
- User-configurable override for trigger distance
- Automatic section control pause during turn initiation, resume after completion
- Integration with SteeringCoordinatorService (Wave 3) for path following
- Generate turn patterns on-demand (not pre-calculated)
- Turn state tracking: idle, approaching, executing, completing

**4. Tram Line Service**
- Generate tram line patterns from active AB line (Wave 2)
- Configurable spacing: track width, seed width, custom spacing
- Multi-pass support with navigation between patterns
- Start offset configuration (distance from AB line origin to first tram)
- Pattern swap (swap A/B sides)
- Alpha transparency control for visual display
- Save/load tram line patterns to field file

**5. Point-In-Polygon Service (Supporting)**
- Reusable service for geometric containment checks
- Ray-casting algorithm implementation
- R-tree spatial indexing for performance optimization
- Performance target: <1ms per check at 10Hz GPS update rate
- Used by boundary service, headland service, and coverage tracking

### Non-Functional Requirements

**Performance:**
- Point-in-polygon check: <1ms per check
- Boundary simplification: <10ms for typical field (50-200 points)
- Headland generation: <50ms for typical field
- Turn pattern calculation: <5ms per pattern
- Tram line generation: <10ms for typical pattern
- Area calculation: Real-time during recording with no noticeable lag
- R-tree spatial indexing required for fields >500 points

**Reliability:**
- Handle GPS signal loss during boundary recording gracefully
- Support boundaries with holes (inner boundaries)
- Support multi-part fields (non-contiguous polygons)
- Handle very irregular/complex boundary shapes
- Handle extremely large fields (>1000 acres) and very small fields (<1 acre)
- Validate turn patterns fit within available space

**Compatibility:**
- Maintain exact AgOpenGPS file format compatibility for Boundary.txt and Headland.Txt
- Support coordinate system conversion (WGS84 ↔ UTM)
- Cross-platform file path handling (Windows, Linux, macOS)

**Integration:**
- Seamless integration with PositionUpdateService (Wave 1)
- Integration with ABLineService (Wave 2) for tram lines
- Integration with SteeringCoordinatorService (Wave 3) for turn execution
- Integration with Section Control services (Wave 4) for automatic pause/resume
- EventArgs pattern for all state change notifications

## Visual Design

Visual assets provided in `planning/visuals/` directory serve as reference material showing what UI features the backend needs to support. Wave 5 focuses on backend services only - UI implementation is a separate future phase.

**Key UI Insights from Visual Assets:**
- Modal dialog workflow: Boundary → Headland → Tram Lines
- Real-time preview of operations before confirmation
- Incremental adjustment controls (B++/B--, A++/A--)
- Multi-pass navigation with previous/next controls
- Non-destructive editing with reset and undo capabilities
- Status indicators showing measurements (area, perimeter, track, tram values)

**Backend Support Required:**
- State management for recording, editing, finalizing operations
- Live area updates during boundary recording
- Multi-pass headland generation with adjustable offsets
- Smooth vs straight path algorithms for headlands
- Navigation between multiple boundary/headland/tram line sets
- Undo/redo state tracking for non-destructive editing

## Reusable Components

### Existing Code to Leverage

**File I/O Patterns:**
- Location: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services/`
- Reference services:
  - `BoundaryFileService.cs` - Legacy boundary file format handling
  - `ABLineFileService.cs` - JSON + legacy text format pattern
  - `CurveLineFileService.cs` - File I/O with validation
- Pattern: Support both modern formats (JSON) and AgOpenGPS legacy formats (.txt) for backward compatibility

**Event Patterns:**
- Location: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Events/`
- Reference EventArgs:
  - `CoverageMapUpdatedEventArgs.cs` - Readonly fields, UTC timestamp, validation
  - `SectionStateChangedEventArgs.cs` - Change type enum, old/new state tracking
- Pattern: Readonly fields, UTC timestamps, validation in constructor, specific change type enums

**Position Integration:**
- Service: `PositionUpdateService`
- Location: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services/GPS/PositionUpdateService.cs`
- Pattern: Subscribe to `PositionUpdated` event for real-time GPS updates during boundary recording
- Use: `GetCurrentPosition()`, `GetCurrentHeading()`, `GetCurrentSpeed()`

**Steering Integration:**
- Service: `SteeringCoordinatorService`
- Location: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services/Guidance/SteeringCoordinatorService.cs`
- Pattern: Coordinate with existing steering algorithms for turn execution
- Use: `Update()` method for path following during turns, `SteeringUpdated` event

**Section Control Integration:**
- Services: `SectionControlService`, `SectionSpeedService`, `AnalogSwitchStateService`
- Location: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services/Section/`
- Pattern: Pause all sections during turn initiation, resume after completion
- Use: Section state management methods, `SectionStateChanged` event

**Dependency Injection:**
- Location: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
- Pattern: `services.AddSingleton<IServiceName, ServiceName>()`
- All new services must be registered following existing pattern

### New Components Required

**PointInPolygonService** - No existing geometric containment check service
- Why new: First implementation of spatial indexing and point-in-polygon algorithms
- Algorithm: Ray-casting with R-tree spatial indexing for performance
- Reusability: Used by boundary service, headland service, coverage tracking, turn validation

**BoundaryManagementService** - Extends existing BoundaryFileService
- Why new: Existing service only handles file I/O, not recording, validation, or geometric operations
- Algorithms: Douglas-Peucker simplification, Shoelace area calculation
- Extends: BoundaryFileService for file I/O compatibility

**HeadlandService** - No existing headland processing service
- Why new: Polygon offset algorithm for headland generation not yet implemented
- Algorithms: Offset polygon with smooth corner handling, entry/exit point calculation
- Integration: Uses BoundaryManagementService for boundary data

**UTurnService** - No existing turn pattern generation service
- Why new: Dubins path algorithm and turn state management not yet implemented
- Algorithms: Dubins path for five turn patterns (Question Mark, Semi-Circle, Keyhole, T, Y)
- Integration: Uses SteeringCoordinatorService for path following, Section Control for auto-pause

**TramLineService** - No existing tram line generation service
- Why new: Parallel line generation from AB lines not yet implemented
- Algorithms: Parallel line offset from AB line with configurable spacing
- Integration: Uses ABLineService for base guidance line

## Technical Approach

### Service Organization

**Directory Structure (Flat):**
```
AgValoniaGPS.Services/
  └── FieldOperations/
      ├── IBoundaryManagementService.cs
      ├── BoundaryManagementService.cs
      ├── IHeadlandService.cs
      ├── HeadlandService.cs
      ├── IUTurnService.cs
      ├── UTurnService.cs
      ├── ITramLineService.cs
      ├── TramLineService.cs
      ├── IPointInPolygonService.cs
      └── PointInPolygonService.cs
```

**Namespace:**
```csharp
namespace AgValoniaGPS.Services.FieldOperations;
```

**Note:** Per NAMING_CONVENTIONS.md, using flat structure in FieldOperations/ directory to avoid namespace collisions. No subdirectories for single services.

### Database / Models

**Domain Models (AgValoniaGPS.Models):**

```csharp
// Boundary recording state
public enum BoundaryRecordingMode
{
    TimeBased,
    DistanceBased
}

public enum BoundaryRecordingState
{
    Idle,
    Recording,
    Paused,
    Completed
}

// Boundary simplification
public enum SimplificationMode
{
    Automatic,     // Simplify during recording
    Manual         // Simplify after completion
}

public class BoundaryRecordingConfiguration
{
    public BoundaryRecordingMode Mode { get; set; }
    public double TimeIntervalSeconds { get; set; } = 1.0;
    public double DistanceIntervalMeters { get; set; } = 1.0;
    public SimplificationMode SimplificationMode { get; set; }
    public double SimplificationTolerance { get; set; } = 0.5; // meters
}

// Headland configuration
public enum HeadlandOverlapMode
{
    PreventOverlap,
    AllowOverlap
}

public class HeadlandConfiguration
{
    public double WidthMeters { get; set; } = 6.0;
    public int NumberOfPasses { get; set; } = 1;
    public HeadlandOverlapMode OverlapMode { get; set; }
    public double OverlapToleranceMeters { get; set; } = 0.1;
    public double CornerRadiusMeters { get; set; } = 2.0;
}

public class HeadlandEntryExitPoint
{
    public Position3D Position { get; set; }
    public double Heading { get; set; }
    public bool IsManuallyNudged { get; set; }
}

// Turn patterns
public enum TurnPattern
{
    QuestionMark,
    SemiCircle,
    Keyhole,
    TTurn,
    YTurn
}

public enum TurnState
{
    Idle,
    Approaching,
    Executing,
    Completing
}

public class TurnConfiguration
{
    public TurnPattern Pattern { get; set; }
    public double TurnRadiusMeters { get; set; }
    public bool IsRadiusOverridden { get; set; }
    public double TriggerDistanceMeters { get; set; }
    public bool IsTriggerDistanceOverridden { get; set; }
    public double SectionPauseTimingSeconds { get; set; } = 0.5;
}

// Tram lines
public class TramLineConfiguration
{
    public double SpacingMeters { get; set; } = 3.0;
    public int PassCount { get; set; } = 2;
    public double StartOffsetMeters { get; set; } = 0.0;
    public bool SwapABSides { get; set; }
    public int AlphaTransparency { get; set; } = 80;
}

// Validation results
public class BoundaryValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool HasSelfIntersection { get; set; }
    public double AreaSquareMeters { get; set; }
    public int PointCount { get; set; }
}
```

### API / Service Interfaces

**1. IBoundaryManagementService**

```csharp
public interface IBoundaryManagementService
{
    // Recording control
    void StartRecording(BoundaryRecordingConfiguration config);
    void PauseRecording();
    void ResumeRecording();
    void StopRecording();
    BoundaryRecordingState GetRecordingState();

    // Recording data
    Boundary GetCurrentBoundary();
    double GetCurrentArea(); // Square meters
    double GetCurrentPerimeter(); // Meters
    int GetPointCount();

    // Simplification
    void SimplifyBoundary(double toleranceMeters);
    Boundary GetSimplifiedBoundary(Boundary boundary, double toleranceMeters);

    // Validation
    BoundaryValidationResult ValidateBoundary(Boundary boundary);

    // Distance calculations
    double DistanceToNearestEdge(Position3D point, Boundary boundary);
    double DistanceToSegment(Position3D point, Position3D segmentStart, Position3D segmentEnd);
    double PerpendicularDistance(Position3D point, Position3D lineStart, Position3D lineEnd);

    // File I/O
    Boundary LoadBoundary(string fieldDirectory, BoundaryFileFormat format);
    void SaveBoundary(Boundary boundary, string fieldDirectory, BoundaryFileFormat format);
    void ExportBoundary(Boundary boundary, string filePath, BoundaryFileFormat format);

    // Events
    event EventHandler<BoundaryRecordingStartedEventArgs>? RecordingStarted;
    event EventHandler<BoundaryPointAddedEventArgs>? PointAdded;
    event EventHandler<BoundaryRecordingCompletedEventArgs>? RecordingCompleted;
    event EventHandler<BoundaryValidationEventArgs>? ValidationCompleted;
    event EventHandler<BoundaryAreaUpdatedEventArgs>? AreaUpdated;
}

public enum BoundaryFileFormat
{
    AgOpenGPSTxt,
    GeoJSON,
    KML
}
```

**2. IHeadlandService**

```csharp
public interface IHeadlandService
{
    // Headland generation
    Headland GenerateHeadland(Boundary boundary, HeadlandConfiguration config);
    List<HeadlandPass> GenerateMultiPassHeadland(Boundary boundary, HeadlandConfiguration config);

    // Entry/exit points
    HeadlandEntryExitPoint CalculateEntryPoint(Headland headland);
    HeadlandEntryExitPoint CalculateExitPoint(Headland headland);
    void NudgeEntryPoint(Headland headland, double offsetMeters, double headingDegrees);
    void NudgeExitPoint(Headland headland, double offsetMeters, double headingDegrees);

    // Progress tracking
    void MarkSectionCompleted(Headland headland, int sectionIndex);
    bool IsSectionCompleted(Headland headland, int sectionIndex);
    double GetCompletionPercentage(Headland headland);
    List<int> GetCompletedSections(Headland headland);

    // File I/O
    Headland LoadHeadland(string fieldDirectory);
    void SaveHeadland(Headland headland, string fieldDirectory);

    // Events
    event EventHandler<HeadlandGeneratedEventArgs>? HeadlandGenerated;
    event EventHandler<HeadlandProgressChangedEventArgs>? ProgressChanged;
    event EventHandler<EntryExitPointChangedEventArgs>? EntryExitPointChanged;
}

public class HeadlandPass
{
    public int PassNumber { get; set; }
    public List<Position3D> Points { get; set; }
    public double OffsetMeters { get; set; }
}
```

**3. IUTurnService**

```csharp
public interface IUTurnService
{
    // Turn pattern generation
    TurnPath GenerateTurnPattern(
        TurnPattern pattern,
        Position3D startPoint,
        Position3D endPoint,
        double heading,
        TurnConfiguration config);

    // Turn radius calculation
    double CalculateOptimalTurnRadius(VehicleConfiguration vehicle, double speed);
    void OverrideTurnRadius(double radiusMeters);
    void ClearRadiusOverride();

    // Turn trigger detection
    double CalculateTriggerDistance(VehicleConfiguration vehicle, double speed, double lookAheadDistance);
    void OverrideTriggerDistance(double distanceMeters);
    void ClearTriggerOverride();
    bool IsTurnTriggered(Position3D currentPosition, Boundary boundary, double heading);

    // Turn state management
    TurnState GetTurnState();
    void StartTurn(TurnPattern pattern);
    void CompleteTurn();
    void AbortTurn();

    // Turn path following
    Position3D GetNextPathPoint(Position3D currentPosition);
    double GetCrossTrackError(Position3D currentPosition);

    // Integration with section control
    void PauseSections();
    void ResumeSections();

    // Events
    event EventHandler<TurnPatternGeneratedEventArgs>? PatternGenerated;
    event EventHandler<TurnStateChangedEventArgs>? StateChanged;
    event EventHandler<TurnTriggerDetectedEventArgs>? TriggerDetected;
}

public class TurnPath
{
    public List<Position3D> Points { get; set; }
    public double TotalLength { get; set; }
    public double MinimumRadius { get; set; }
}
```

**4. ITramLineService**

```csharp
public interface ITramLineService
{
    // Tram line generation
    List<TramLine> GenerateTramLines(ABLine baseLine, TramLineConfiguration config);
    void UpdateConfiguration(TramLineConfiguration config);

    // Pattern management
    void AddPattern(string name, TramLineConfiguration config);
    void DeletePattern(int patternIndex);
    List<TramLinePattern> GetAllPatterns();
    void SetActivePattern(int patternIndex);
    int GetActivePatternIndex();

    // Navigation
    void NavigateToNext();
    void NavigateToPrevious();

    // File I/O
    void SaveTramLines(string fieldDirectory);
    void LoadTramLines(string fieldDirectory);

    // Events
    event EventHandler<TramLineGeneratedEventArgs>? TramLinesGenerated;
    event EventHandler<TramLineUpdatedEventArgs>? ConfigurationUpdated;
    event EventHandler<TramLinePatternChangedEventArgs>? ActivePatternChanged;
}

public class TramLine
{
    public Position3D StartPoint { get; set; }
    public Position3D EndPoint { get; set; }
    public double Heading { get; set; }
    public double OffsetFromBaseLine { get; set; }
}

public class TramLinePattern
{
    public string Name { get; set; }
    public List<TramLine> Lines { get; set; }
    public TramLineConfiguration Configuration { get; set; }
}
```

**5. IPointInPolygonService**

```csharp
public interface IPointInPolygonService
{
    // Point-in-polygon checks
    bool IsPointInPolygon(Position3D point, Boundary boundary);
    bool IsPointInPolygon(Position3D point, List<Position3D> polygon);

    // Multi-polygon support
    bool IsPointInMultiPolygon(Position3D point, List<List<Position3D>> polygons);

    // Spatial indexing
    void BuildSpatialIndex(Boundary boundary);
    void ClearSpatialIndex();

    // Performance monitoring
    double GetLastCheckDurationMs();
    long GetTotalChecksPerformed();
}
```

### Event Definitions

**Boundary Events:**

```csharp
public class BoundaryRecordingStartedEventArgs : EventArgs
{
    public readonly BoundaryRecordingConfiguration Configuration;
    public readonly DateTime Timestamp;

    public BoundaryRecordingStartedEventArgs(BoundaryRecordingConfiguration config)
    {
        Configuration = config ?? throw new ArgumentNullException(nameof(config));
        Timestamp = DateTime.UtcNow;
    }
}

public class BoundaryPointAddedEventArgs : EventArgs
{
    public readonly Position3D Point;
    public readonly int TotalPointCount;
    public readonly double CurrentArea;
    public readonly double CurrentPerimeter;
    public readonly DateTime Timestamp;

    public BoundaryPointAddedEventArgs(Position3D point, int totalPoints, double area, double perimeter)
    {
        Point = point ?? throw new ArgumentNullException(nameof(point));
        if (totalPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(totalPoints));
        if (area < 0)
            throw new ArgumentOutOfRangeException(nameof(area));
        if (perimeter < 0)
            throw new ArgumentOutOfRangeException(nameof(perimeter));

        TotalPointCount = totalPoints;
        CurrentArea = area;
        CurrentPerimeter = perimeter;
        Timestamp = DateTime.UtcNow;
    }
}

public class BoundaryRecordingCompletedEventArgs : EventArgs
{
    public readonly Boundary Boundary;
    public readonly BoundaryValidationResult ValidationResult;
    public readonly DateTime Timestamp;

    public BoundaryRecordingCompletedEventArgs(Boundary boundary, BoundaryValidationResult validationResult)
    {
        Boundary = boundary ?? throw new ArgumentNullException(nameof(boundary));
        ValidationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
        Timestamp = DateTime.UtcNow;
    }
}

public class BoundaryValidationEventArgs : EventArgs
{
    public readonly BoundaryValidationResult Result;
    public readonly DateTime Timestamp;

    public BoundaryValidationEventArgs(BoundaryValidationResult result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Timestamp = DateTime.UtcNow;
    }
}

public class BoundaryAreaUpdatedEventArgs : EventArgs
{
    public readonly double AreaSquareMeters;
    public readonly double PerimeterMeters;
    public readonly int PointCount;
    public readonly DateTime Timestamp;

    public BoundaryAreaUpdatedEventArgs(double area, double perimeter, int pointCount)
    {
        if (area < 0)
            throw new ArgumentOutOfRangeException(nameof(area));
        if (perimeter < 0)
            throw new ArgumentOutOfRangeException(nameof(perimeter));
        if (pointCount < 0)
            throw new ArgumentOutOfRangeException(nameof(pointCount));

        AreaSquareMeters = area;
        PerimeterMeters = perimeter;
        PointCount = pointCount;
        Timestamp = DateTime.UtcNow;
    }
}
```

**Headland Events:**

```csharp
public class HeadlandGeneratedEventArgs : EventArgs
{
    public readonly Headland Headland;
    public readonly HeadlandConfiguration Configuration;
    public readonly int PassCount;
    public readonly DateTime Timestamp;

    public HeadlandGeneratedEventArgs(Headland headland, HeadlandConfiguration config, int passCount)
    {
        Headland = headland ?? throw new ArgumentNullException(nameof(headland));
        Configuration = config ?? throw new ArgumentNullException(nameof(config));
        if (passCount < 0)
            throw new ArgumentOutOfRangeException(nameof(passCount));

        PassCount = passCount;
        Timestamp = DateTime.UtcNow;
    }
}

public class HeadlandProgressChangedEventArgs : EventArgs
{
    public readonly int SectionIndex;
    public readonly bool IsCompleted;
    public readonly double CompletionPercentage;
    public readonly DateTime Timestamp;

    public HeadlandProgressChangedEventArgs(int sectionIndex, bool isCompleted, double percentage)
    {
        if (sectionIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(sectionIndex));
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage));

        SectionIndex = sectionIndex;
        IsCompleted = isCompleted;
        CompletionPercentage = percentage;
        Timestamp = DateTime.UtcNow;
    }
}

public class EntryExitPointChangedEventArgs : EventArgs
{
    public readonly HeadlandEntryExitPoint Point;
    public readonly bool IsEntryPoint;
    public readonly bool WasManuallyNudged;
    public readonly DateTime Timestamp;

    public EntryExitPointChangedEventArgs(HeadlandEntryExitPoint point, bool isEntry, bool wasNudged)
    {
        Point = point ?? throw new ArgumentNullException(nameof(point));
        IsEntryPoint = isEntry;
        WasManuallyNudged = wasNudged;
        Timestamp = DateTime.UtcNow;
    }
}
```

**Turn Events:**

```csharp
public class TurnPatternGeneratedEventArgs : EventArgs
{
    public readonly TurnPattern Pattern;
    public readonly TurnPath Path;
    public readonly double TurnRadius;
    public readonly DateTime Timestamp;

    public TurnPatternGeneratedEventArgs(TurnPattern pattern, TurnPath path, double radius)
    {
        if (radius <= 0)
            throw new ArgumentOutOfRangeException(nameof(radius));

        Pattern = pattern;
        Path = path ?? throw new ArgumentNullException(nameof(path));
        TurnRadius = radius;
        Timestamp = DateTime.UtcNow;
    }
}

public class TurnStateChangedEventArgs : EventArgs
{
    public readonly TurnState OldState;
    public readonly TurnState NewState;
    public readonly TurnPattern? ActivePattern;
    public readonly DateTime Timestamp;

    public TurnStateChangedEventArgs(TurnState oldState, TurnState newState, TurnPattern? pattern)
    {
        OldState = oldState;
        NewState = newState;
        ActivePattern = pattern;
        Timestamp = DateTime.UtcNow;
    }
}

public class TurnTriggerDetectedEventArgs : EventArgs
{
    public readonly Position3D TriggerPosition;
    public readonly double DistanceToBoundary;
    public readonly TurnPattern RecommendedPattern;
    public readonly DateTime Timestamp;

    public TurnTriggerDetectedEventArgs(Position3D position, double distance, TurnPattern pattern)
    {
        TriggerPosition = position ?? throw new ArgumentNullException(nameof(position));
        if (distance < 0)
            throw new ArgumentOutOfRangeException(nameof(distance));

        DistanceToBoundary = distance;
        RecommendedPattern = pattern;
        Timestamp = DateTime.UtcNow;
    }
}
```

**Tram Line Events:**

```csharp
public class TramLineGeneratedEventArgs : EventArgs
{
    public readonly List<TramLine> TramLines;
    public readonly TramLineConfiguration Configuration;
    public readonly DateTime Timestamp;

    public TramLineGeneratedEventArgs(List<TramLine> tramLines, TramLineConfiguration config)
    {
        TramLines = tramLines ?? throw new ArgumentNullException(nameof(tramLines));
        Configuration = config ?? throw new ArgumentNullException(nameof(config));
        Timestamp = DateTime.UtcNow;
    }
}

public class TramLineUpdatedEventArgs : EventArgs
{
    public readonly TramLineConfiguration NewConfiguration;
    public readonly TramLineConfiguration? OldConfiguration;
    public readonly DateTime Timestamp;

    public TramLineUpdatedEventArgs(TramLineConfiguration newConfig, TramLineConfiguration? oldConfig)
    {
        NewConfiguration = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        OldConfiguration = oldConfig;
        Timestamp = DateTime.UtcNow;
    }
}

public class TramLinePatternChangedEventArgs : EventArgs
{
    public readonly int OldPatternIndex;
    public readonly int NewPatternIndex;
    public readonly TramLinePattern? ActivePattern;
    public readonly DateTime Timestamp;

    public TramLinePatternChangedEventArgs(int oldIndex, int newIndex, TramLinePattern? pattern)
    {
        if (oldIndex < -1)
            throw new ArgumentOutOfRangeException(nameof(oldIndex));
        if (newIndex < -1)
            throw new ArgumentOutOfRangeException(nameof(newIndex));

        OldPatternIndex = oldIndex;
        NewPatternIndex = newIndex;
        ActivePattern = pattern;
        Timestamp = DateTime.UtcNow;
    }
}
```

### Algorithm Specifications

**1. Douglas-Peucker Simplification Algorithm**

Purpose: Reduce boundary point count while preserving shape accuracy.

```
Input: List of points, tolerance (meters)
Output: Simplified list of points

Algorithm:
1. Find the point with maximum perpendicular distance from line segment (first -> last)
2. If max distance > tolerance:
   a. Recursively simplify left segment (first -> max point)
   b. Recursively simplify right segment (max point -> last)
   c. Combine results
3. Else:
   a. Return only first and last points

Time Complexity: O(n log n) average, O(n²) worst case
Space Complexity: O(n) for recursion stack
```

**2. Shoelace Formula (Area Calculation)**

Purpose: Calculate polygon area from vertices in real-time.

```
Input: List of polygon vertices (x, y coordinates)
Output: Area in square meters

Formula:
Area = 0.5 * |Σ(xi * yi+1 - xi+1 * yi)|

Where:
- i ranges from 0 to n-1
- n = number of vertices
- xn = x0, yn = y0 (polygon closure)

Properties:
- Works for any non-self-intersecting polygon
- Returns absolute value (area always positive)
- Efficient: O(n) time, O(1) space

Implementation:
double area = 0.0;
for (int i = 0; i < n; i++) {
    int j = (i + 1) % n;
    area += points[i].Easting * points[j].Northing;
    area -= points[j].Easting * points[i].Northing;
}
return Math.Abs(area) / 2.0;
```

**3. Ray-Casting Algorithm (Point-in-Polygon)**

Purpose: Determine if a point is inside a polygon.

```
Input: Point (x, y), Polygon vertices
Output: Boolean (inside/outside)

Algorithm:
1. Cast horizontal ray from point to infinity (right direction)
2. Count intersections with polygon edges
3. If count is odd -> point inside
4. If count is even -> point outside

Edge Cases:
- Point on edge -> considered inside
- Ray through vertex -> count only if vertex is local extrema
- Horizontal edges -> ignore

Time Complexity: O(n) where n = number of vertices
Space Complexity: O(1)

Optimization: Pre-compute bounding box, reject points outside box in O(1)
```

**4. R-tree Spatial Indexing**

Purpose: Accelerate point-in-polygon checks for large/complex boundaries.

```
Structure:
- Hierarchical tree of minimum bounding rectangles (MBRs)
- Leaf nodes contain polygon segments
- Internal nodes contain child MBRs

Operations:
Build Index:
1. Divide polygon into segments
2. Create MBR for each segment
3. Group MBRs hierarchically (R-tree insertion)

Query (Point-in-Polygon):
1. Check point against root MBR
2. Recursively check child MBRs that contain point
3. Perform ray-casting only on segments in matching leaf nodes

Performance:
- Build time: O(n log n)
- Query time: O(log n) average, O(n) worst case
- Memory: O(n)

When to use: Polygons with >500 vertices
```

**5. Offset Polygon Algorithm (Headland Generation)**

Purpose: Generate parallel offset polygons for headlands.

```
Input: Boundary polygon, offset distance (meters), corner handling mode
Output: Offset polygon(s)

Algorithm:
1. For each edge:
   a. Calculate perpendicular unit vector
   b. Offset edge by distance * unit vector
2. For each vertex:
   a. Calculate intersection of adjacent offset edges
   b. If intersection beyond threshold -> insert arc for smooth corner
3. Handle self-intersections:
   a. Detect using segment intersection tests
   b. Remove loops if overlap prevention enabled
4. Validate result polygon

Corner Handling:
- Miter join: Direct intersection (sharp corners)
- Round join: Arc with specified radius (smooth corners)
- Bevel join: Straight cut across corner

Time Complexity: O(n²) for self-intersection detection
Space Complexity: O(n)

Note: Multi-pass headlands generate multiple offsets at different distances
```

**6. Dubins Path Algorithm (Turn Patterns)**

Purpose: Calculate minimum-radius path between two poses (position + heading).

```
Input:
- Start pose (x1, y1, θ1)
- End pose (x2, y2, θ2)
- Minimum turn radius R

Output: Optimal path consisting of arc-line-arc segments

Path Types:
- LSL: Left arc - Straight - Left arc
- RSR: Right arc - Straight - Right arc
- LSR: Left arc - Straight - Right arc
- RSL: Right arc - Straight - Left arc
- RLR: Right arc - Left arc - Right arc
- LRL: Left arc - Right arc - Left arc

Algorithm:
1. Calculate all 6 path types
2. Compute length of each path
3. Select shortest valid path
4. Discretize path into waypoints

Turn Pattern Mapping:
- Question Mark: LSL or RSR (same-direction turns)
- Semi-Circle: LSR or RSL (opposite-direction turns)
- Keyhole: RLR or LRL (3-arc paths)
- T-turn: Special case with reverse segment
- Y-turn: LSL/RSR with 45° offset

Time Complexity: O(1) for path calculation, O(n) for discretization
Space Complexity: O(n) for waypoint storage
```

### Integration Points

**Wave 1 Integration (Position & Kinematics):**
- Subscribe to `PositionUpdateService.PositionUpdated` event for boundary recording
- Use `GetCurrentPosition()` for real-time position during recording
- Use `GetCurrentHeading()` and `GetCurrentSpeed()` for turn trigger calculations
- Access `VehicleKinematicsService` for vehicle configuration in turn radius calculation

**Wave 2 Integration (Guidance Lines):**
- Access `ABLineService` to get active AB line for tram line generation
- Use AB line heading and position for parallel line calculations
- Reference `ABLineFileService` for consistent file I/O patterns

**Wave 3 Integration (Steering):**
- Coordinate with `SteeringCoordinatorService.Update()` during turn execution
- Pass turn path waypoints to steering coordinator for path following
- Monitor `SteeringUpdated` event for cross-track error during turns
- Switch between turn path and guidance line steering

**Wave 4 Integration (Section Control):**
- Call section control pause methods when turn state changes to `Executing`
- Call section control resume methods when turn state changes to `Idle`
- Subscribe to `SectionStateChanged` events for coordinated state management
- Access section configuration for turn timing adjustments

**Cross-Wave Event Flow:**
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

### File Format Specifications

**AgOpenGPS Boundary.txt Format:**
```
$Boundary
<isDriveThru: bool>
<point count: int>
<easting>,<northing>,<heading>
<easting>,<northing>,<heading>
...
[optional additional polygons for holes]
```

**AgOpenGPS Headland.Txt Format:**
```
$Headland
<isDriveThru: bool>
<point count: int>
<easting>,<northing>,<heading>
<easting>,<northing>,<heading>
...
```

**GeoJSON Format (Boundary Export):**
```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": {
        "type": "Polygon",
        "coordinates": [
          [[easting1, northing1], [easting2, northing2], ...]
        ]
      },
      "properties": {
        "name": "Field Boundary",
        "area": 34.42,
        "areaUnit": "acres",
        "isDriveThru": false
      }
    }
  ]
}
```

**KML Format (Boundary Export):**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2">
  <Document>
    <name>Field Boundary</name>
    <Placemark>
      <name>Boundary</name>
      <description>Area: 34.42 acres</description>
      <Polygon>
        <outerBoundaryIs>
          <LinearRing>
            <coordinates>
              lon1,lat1,0 lon2,lat2,0 ...
            </coordinates>
          </LinearRing>
        </outerBoundaryIs>
      </Polygon>
    </Placemark>
  </Document>
</kml>
```

**Coordinate System Handling:**
- Internal calculations: UTM (meters)
- GPS input: WGS84 (lat/lon) -> convert to UTM
- File storage: WGS84 for AgOpenGPS .txt format, UTM for JSON
- GeoJSON/KML export: WGS84 (standard for geographic formats)

## Testing Requirements

### Unit Test Coverage

**BoundaryManagementService Tests:**
- Boundary recording in time-based mode
- Boundary recording in distance-based mode
- Automatic simplification during recording
- Manual simplification after recording
- Area calculation accuracy (compare to known polygons)
- Validation: minimum points, self-intersection, minimum area
- Distance calculations: nearest edge, segment, perpendicular
- File I/O: AgOpenGPS .txt, GeoJSON, KML formats
- GPS signal loss handling
- Boundary with holes (inner boundaries)
- Multi-part fields (non-contiguous polygons)

**HeadlandService Tests:**
- Single-pass headland generation
- Multi-pass headland generation
- Overlap prevention mode
- Overlap allowed mode
- Entry/exit point calculation
- Entry/exit point nudging
- Section completion tracking
- Completion percentage calculation
- Smooth corner vs sharp corner handling
- File I/O for headland data

**UTurnService Tests:**
- Each turn pattern generation (Question Mark, Semi-Circle, Keyhole, T, Y)
- Optimal turn radius calculation
- Turn radius override
- Trigger distance calculation
- Trigger distance override
- Turn state transitions (idle -> approaching -> executing -> completing)
- Section pause on turn start
- Section resume on turn complete
- Turn path waypoint generation
- Cross-track error during turn execution
- Turn pattern validation (fits within available space)

**TramLineService Tests:**
- Tram line generation from AB line
- Configurable spacing (track width, seed width, custom)
- Multi-pass support
- Start offset application
- Pattern swap (A/B sides)
- Pattern management (add, delete, navigate)
- File I/O for tram line data

**PointInPolygonService Tests:**
- Ray-casting accuracy for various polygon shapes
- Point on edge handling
- Point on vertex handling
- Horizontal edge handling
- R-tree index build performance
- R-tree query performance
- Performance benchmarks (<1ms per check)

### Integration Tests

**Boundary Recording Flow:**
1. Start PositionUpdateService
2. Start BoundaryManagementService recording
3. Simulate GPS position updates
4. Verify points added with correct intervals
5. Verify real-time area updates
6. Stop recording
7. Verify validation runs automatically
8. Verify boundary saved to file

**Headland Generation Flow:**
1. Load boundary from file
2. Generate multi-pass headland
3. Verify headland geometry
4. Calculate entry/exit points
5. Track section completion
6. Save headland to file

**Turn Execution Flow:**
1. Start PositionUpdateService
2. Load boundary
3. Configure UTurnService
4. Approach boundary
5. Verify turn trigger detection
6. Verify section control pause
7. Execute turn pattern
8. Verify steering integration
9. Complete turn
10. Verify section control resume

**Tram Line Generation Flow:**
1. Load AB line from Wave 2
2. Configure tram line spacing
3. Generate tram lines
4. Verify parallel line geometry
5. Save tram lines to file

### Performance Benchmarks

**Target Performance (Typical Hardware):**
- Point-in-polygon check: <1ms per check
- Boundary simplification (100 points): <10ms
- Headland generation (single pass, 100 points): <50ms
- Turn pattern calculation: <5ms
- Tram line generation (10 lines): <10ms
- Area calculation (100 points): <1ms
- R-tree index build (500 points): <20ms

**Load Testing:**
- Large field boundary (1000+ points)
- Complex boundary with 10+ holes
- Multi-pass headland (5+ passes)
- High-frequency GPS updates (10Hz) during recording
- Concurrent operations (boundary recording + headland generation)

### Edge Case Testing

**GPS Signal Loss:**
- Simulate GPS dropout during boundary recording
- Verify gap detection and handling
- Verify user notification of large gaps

**Boundary Topology:**
- Self-intersecting boundaries (warn but allow)
- Boundaries with holes (inner boundaries)
- Multi-part fields (non-contiguous polygons)
- Very irregular shapes (concave, star-shaped)

**Field Size Extremes:**
- Very large fields (>1000 acres, >1000 points)
- Very small fields (<1 acre, <10 points)
- Long thin fields (high aspect ratio)

**Turn Pattern Edge Cases:**
- Turn patterns in constrained spaces
- Minimum turn radius violations
- Turn trigger at field corners
- Turn patterns near inner boundaries

**Tram Line Edge Cases:**
- Tram lines with irregular AB lines
- Very wide spacing (>20 meters)
- Very narrow spacing (<1 meter)
- Start offset exceeding field length

## Out of Scope

**Explicitly Not Included in Wave 5:**
- ViewModels for UI binding
- XAML views and user controls
- Frontend-backend wiring and data binding
- User interface dialogs and modal windows
- Visual rendering of boundaries, headlands, turns, tram lines
- User interaction handlers (button clicks, drag operations)
- Undo/redo UI implementation (state tracking logic IS in scope)
- Real-time map display integration
- 3D terrain visualization

**Future Enhancements (Post-Wave 5):**
- AI-based boundary optimization
- Cloud-based field sharing (AgShare integration)
- Automatic boundary detection from satellite imagery
- 3D terrain-aware boundary and headland generation
- Multi-vehicle coordination for boundary recording
- Real-time collaborative boundary editing
- Predictive turn pattern selection based on machine learning
- Headland optimization for minimum fuel consumption
- Tram line pattern optimization for minimum soil compaction

## Success Criteria

**Functional Completeness:**
- All 5 services implemented with full interface coverage
- All 4 recording modes functional (time/distance for boundary, on-demand for others)
- All 5 turn patterns generate valid paths
- All 3 file formats (AgOpenGPS .txt, GeoJSON, KML) read/write correctly
- All integration points with Waves 1-4 functional

**Performance:**
- 100% of point-in-polygon checks complete in <1ms
- 95% of boundary simplifications complete in <10ms for typical fields
- 95% of headland generations complete in <50ms for typical fields
- 100% of turn pattern calculations complete in <5ms
- 100% of tram line generations complete in <10ms for typical patterns

**Quality:**
- 100% test pass rate for all unit tests
- 100% test pass rate for all integration tests
- Zero memory leaks detected during load testing
- All edge cases handled gracefully with appropriate warnings/errors
- All EventArgs follow established patterns with validation

**Integration:**
- Successful integration tests with PositionUpdateService (Wave 1)
- Successful integration tests with ABLineService (Wave 2)
- Successful integration tests with SteeringCoordinatorService (Wave 3)
- Successful integration tests with Section Control services (Wave 4)
- All cross-wave event flows verified

**Documentation:**
- All service interfaces fully documented with XML comments
- All algorithms documented with complexity analysis
- All file formats documented with examples
- All integration points documented with sequence diagrams
- All performance benchmarks documented with test results

**Backward Compatibility:**
- AgOpenGPS Boundary.txt files load correctly
- AgOpenGPS Headland.Txt files load correctly
- Coordinate system conversions maintain precision
- Legacy file format quirks handled (duplicate lines, extra whitespace)
