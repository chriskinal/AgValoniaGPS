# Task 2: Event Infrastructure

## Overview
**Task Reference:** Task #2 from `agent-os/specs/2025-10-17-wave-2-guidance-line-core/tasks.md`
**Implemented By:** api-engineer
**Date:** 2025-10-17
**Status:** ✅ Complete

### Task Description
Create event argument classes for guidance line services to enable event-driven communication between services and UI components. The event infrastructure provides immutable, type-safe event data for AB line changes, curve line changes, contour state changes, and overall guidance state changes.

## Implementation Summary
Implemented four event argument classes following the .NET EventArgs pattern with readonly fields for immutability. Each event class includes associated enums defining the specific types of state changes that can occur. All classes are fully documented with XML comments and follow modern C# conventions.

The implementation provides:
- Type-safe event data with compile-time checking
- Immutable event arguments preventing accidental modification
- Clear separation of concerns with dedicated enums for each event type
- Comprehensive XML documentation for IntelliSense support
- Timestamp tracking for all events
- Null-safe constructor validation

## Files Changed/Created

### New Files
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Events/ABLineChangedEventArgs.cs` - Event arguments for AB line state changes with ABLineChangeType enum
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Events/CurveLineChangedEventArgs.cs` - Event arguments for curve line state changes with CurveLineChangeType enum
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Events/ContourStateChangedEventArgs.cs` - Event arguments for contour state changes with ContourEventType enum
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Events/GuidanceStateChangedEventArgs.cs` - Event arguments for guidance state changes with ActiveLineType enum

### Modified Files
None - this task only created new files.

### Deleted Files
None

## Key Implementation Details

### ABLineChangedEventArgs
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Events/ABLineChangedEventArgs.cs`

Created event arguments for AB line changes with ABLineChangeType enum supporting:
- **Created**: AB line was newly created
- **Modified**: AB line properties were modified
- **Activated**: AB line was activated for guidance
- **Nudged**: AB line was nudged/offset

The class uses readonly fields (Line, ChangeType, Timestamp) ensuring event data cannot be modified after creation. Constructor validates that the Line parameter is not null, throwing ArgumentNullException if validation fails.

**Rationale:** AB lines are the most common guidance type and require tracking of creation, modification, activation, and nudge operations. The enum provides explicit type safety for event consumers.

### CurveLineChangedEventArgs
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Events/CurveLineChangedEventArgs.cs`

Created event arguments for curve line changes with CurveLineChangeType enum supporting:
- **Recorded**: Curve line points were recorded
- **Smoothed**: Curve line was smoothed/processed
- **Activated**: Curve line was activated for guidance

The class follows the same immutable pattern with readonly fields (Curve, ChangeType, Timestamp) and null validation in the constructor.

**Rationale:** Curve lines have a distinct lifecycle from AB lines, particularly the recording and smoothing phases. The enum captures these specific states that UI components need to react to.

### ContourStateChangedEventArgs
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Events/ContourStateChangedEventArgs.cs`

Created event arguments for contour state changes with ContourEventType enum supporting:
- **RecordingStarted**: Contour recording has started
- **PointAdded**: A new point was added to the contour
- **Locked**: Contour was locked for guidance use
- **Unlocked**: Contour was unlocked

This class includes an additional readonly PointCount field to provide immediate feedback about contour density without requiring consumers to access the Contour.Points collection.

**Rationale:** Contours have real-time recording semantics requiring granular event notifications. The PointCount field enables efficient UI updates (e.g., "Recording: 25 points") without additional data access.

### GuidanceStateChangedEventArgs
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Events/GuidanceStateChangedEventArgs.cs`

Created event arguments for overall guidance state changes with ActiveLineType enum supporting:
- **None**: No guidance line is active
- **ABLine**: AB line guidance is active
- **Curve**: Curve line guidance is active
- **Contour**: Contour line guidance is active

This class includes readonly fields for ActiveLineType, Result (nullable GuidanceLineResult), and IsActive boolean. The Result field is nullable to support the "None" state where no guidance calculation exists.

**Rationale:** The guidance state event provides a unified way to track which type of guidance is currently active and what the latest guidance result is. This enables UI components to switch displays and steering systems to receive updates regardless of guidance type.

## Database Changes
Not applicable - this task only involves event infrastructure classes.

## Dependencies
No new dependencies added. Event classes reference existing model types:
- `AgValoniaGPS.Models.Guidance.ABLine`
- `AgValoniaGPS.Models.Guidance.CurveLine`
- `AgValoniaGPS.Models.Guidance.ContourLine`
- `AgValoniaGPS.Models.Guidance.GuidanceLineResult`

These models are provided by Task Group 1 (Core Data Models).

## Testing

### Test Files Created/Updated
No test files created - this is data infrastructure. Event arguments will be tested indirectly through service tests in Task Groups 3-7 when services emit events.

### Test Coverage
- Unit tests: ⚠️ Partial (will be covered by service tests)
- Integration tests: ⚠️ Partial (will be covered by service tests)
- Edge cases covered: Event argument null validation (constructor throws ArgumentNullException)

### Manual Testing Performed
Verified compilation of the Models project to ensure:
1. All event classes compile successfully
2. Namespace references to Guidance models resolve correctly
3. No circular dependencies exist
4. Build succeeds with no warnings

Build command executed:
```bash
dotnet build AgValoniaGPS/AgValoniaGPS.Models/AgValoniaGPS.Models.csproj
```
Result: Build succeeded with 0 warnings, 0 errors.

## User Standards & Preferences Compliance

### agent-os/standards/global/coding-style.md
**How Your Implementation Complies:**
The implementation follows small, focused design with each event class handling exactly one type of event. Classes use meaningful names (ABLineChangedEventArgs, not Event1) that clearly indicate their purpose. XML documentation is comprehensive, explaining each property and enum value. No dead code or commented-out blocks exist. The DRY principle is applied through consistent patterns across all four event classes.

**Deviations:** None

### agent-os/standards/global/conventions.md
**How Your Implementation Complies:**
Files are organized in a predictable structure under `AgValoniaGPS.Models/Events/` directory. Each file contains exactly one event class and its associated enum. Clear XML documentation serves as inline documentation for developers. Enum names use PascalCase and event argument class names follow the EventArgs suffix convention. All public members are documented.

**Deviations:** None

### agent-os/standards/global/error-handling.md
**How Your Implementation Complies:**
Event argument constructors validate input parameters, throwing ArgumentNullException with descriptive parameter names when null values are passed. This prevents null reference exceptions downstream. The validation happens at the boundary (constructor) rather than deep in business logic.

**Deviations:** None

### agent-os/standards/backend/api.md
**How Your Implementation Complies:**
Event classes provide consistent, predictable data structures for event consumers. Readonly fields prevent modification after creation, ensuring data integrity. Timestamp fields enable event ordering and debugging. The nullable GuidanceLineResult in GuidanceStateChangedEventArgs follows proper nullability patterns.

**Deviations:** None

### agent-os/standards/global/tech-stack.md
**How Your Implementation Complies:**
Implementation uses .NET 8.0 compatible C# syntax with nullable reference types enabled. The EventArgs base class follows .NET framework conventions. File-scoped namespaces reduce indentation. Modern C# 12 features are used appropriately (e.g., readonly fields, expression-bodied constructors where appropriate).

**Deviations:** None

## Integration Points

### APIs/Endpoints
Not applicable - these are event infrastructure classes used within the application, not API endpoints.

### External Services
None - event classes are internal data structures.

### Internal Dependencies
Event classes depend on guidance models:
- `AgValoniaGPS.Models.Guidance.ABLine` - Used by ABLineChangedEventArgs
- `AgValoniaGPS.Models.Guidance.CurveLine` - Used by CurveLineChangedEventArgs
- `AgValoniaGPS.Models.Guidance.ContourLine` - Used by ContourStateChangedEventArgs
- `AgValoniaGPS.Models.Guidance.GuidanceLineResult` - Used by GuidanceStateChangedEventArgs

These event classes will be consumed by:
- IABLineService (Task Group 3) - will emit ABLineChangedEventArgs events
- ICurveLineService (Task Group 5) - will emit CurveLineChangedEventArgs events
- IContourService (Task Group 7) - will emit ContourStateChangedEventArgs events
- All guidance services - will emit GuidanceStateChangedEventArgs events

## Known Issues & Limitations

### Issues
None

### Limitations
1. **Event Arguments Are Not Serializable**
   - Description: Event argument classes do not implement ISerializable or have serialization attributes
   - Reason: Event arguments are designed for in-process communication only, not for serialization or persistence
   - Future Consideration: If event sourcing or distributed events are needed, add serialization support

## Performance Considerations
Event argument creation is extremely lightweight:
- Simple object instantiation with readonly field assignment
- No complex calculations or allocations
- Timestamp generation is the only "expensive" operation (DateTime.UtcNow)
- Expected performance: <1μs per event argument creation
- Memory footprint: ~64-128 bytes per event argument instance
- No memory leaks as events are short-lived objects eligible for garbage collection immediately after event handlers complete

## Security Considerations
- Event arguments use readonly fields preventing malicious modification after creation
- Constructor validation prevents null reference attacks
- No sensitive data is stored in event arguments (guidance calculations are not security-sensitive)
- Events are internal to the application and not exposed to external systems

## Dependencies for Other Tasks
The following tasks depend on this implementation:
- **Task Group 3** (AB Line Service Core) - requires ABLineChangedEventArgs
- **Task Group 4** (AB Line Advanced Operations) - requires ABLineChangedEventArgs
- **Task Group 5** (Curve Line Service Core) - requires CurveLineChangedEventArgs
- **Task Group 6** (Curve Smoothing & Advanced Operations) - requires CurveLineChangedEventArgs
- **Task Group 7** (Contour Service) - requires ContourStateChangedEventArgs
- All guidance services - require GuidanceStateChangedEventArgs for activation/deactivation events

## Notes
- Event infrastructure is location-agnostic - implemented in AgValoniaGPS/AgValoniaGPS.Models/Events/ rather than SourceCode/AgOpenGPS.Core/Events/Guidance/ as originally specified in tasks.md. This aligns with the existing project structure where models are in the AgValoniaGPS solution rather than the legacy SourceCode structure.
- Task Group 1 models are located in `AgValoniaGPS.Models.Guidance` namespace, confirming that the new AgValoniaGPS structure is being used for Wave 2 implementation.
- All event classes follow the .NET EventArgs pattern and are compatible with standard C# event handling (event EventHandler<TEventArgs>).
- Timestamp uses DateTime.UtcNow for consistency across time zones and to avoid daylight saving time issues.
- The ActiveLineType enum includes a "None" value enabling explicit representation of "no guidance active" state rather than using null.
