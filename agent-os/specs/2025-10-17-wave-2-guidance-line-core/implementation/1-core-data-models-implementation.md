# Task 1: Core Data Models

## Overview
**Task Reference:** Task Group 1 from `agent-os/specs/2025-10-17-wave-2-guidance-line-core/tasks.md`
**Implemented By:** api-engineer
**Date:** 2025-10-17
**Status:** ✅ Complete

### Task Description
Create comprehensive data models for Wave 2 Guidance Line Core system, including models for AB lines, curve lines, contour lines, guidance results, smoothing parameters, unit systems, and validation results. These models form the foundation for all guidance line services.

## Implementation Summary
Implemented seven core data models in the `AgValoniaGPS.Models.Guidance` namespace following the specification requirements. All models include comprehensive XML documentation, validation logic, computed properties, and helper methods. The Position model from Wave 1 was reused as specified. All models are designed to be serializable for persistence and have no UI framework dependencies, ensuring they can be used in service layer implementations.

The implementation emphasizes validation, with each model providing validation methods that return detailed ValidationResult objects containing errors and warnings. Unit conversion utilities are provided through the UnitSystem enum and its extension methods, ensuring all internal calculations use meters while supporting both metric and imperial units at API boundaries.

## Files Changed/Created

### New Files
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/ABLine.cs` - Straight AB guidance line model with computed properties and validation
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/CurveLine.cs` - Curved guidance line model with point collection and validation
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/ContourLine.cs` - Contour line model for real-time recording with self-intersection detection
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/GuidanceLineResult.cs` - Guidance calculation result model with helper methods
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/SmoothingParameters.cs` - Curve smoothing configuration with factory methods for preset configurations
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/UnitSystem.cs` - Unit system enum with conversion extension methods
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/ValidationResult.cs` - Validation result model with error/warning collection and helper methods

### Modified Files
None - all new implementations

### Deleted Files
None

## Key Implementation Details

### ABLine Model
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/ABLine.cs`

Implemented straight AB guidance line model with:
- Core fields: Name, PointA, PointB, Heading, NudgeOffset, CreatedDate
- Computed properties: Length (calculated from point distance), MidPoint (average of A and B), UnitVector (direction vector)
- Validation method checks minimum length (>0.5m), validates points for NaN/Infinity, and ensures valid heading range (0 to 2π)
- All distance calculations use Pythagorean theorem for accuracy

**Rationale:** Provides foundation for AB line service with self-contained validation logic and computed properties to avoid redundant calculations.

### CurveLine Model
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/CurveLine.cs`

Implemented curved guidance line model with:
- Points collection (List<Position>) for curve path definition
- Optional SmoothingParameters reference for tracking applied smoothing
- Computed properties: TotalLength (sum of segment lengths), PointCount, AverageSpacing
- Comprehensive validation checking minimum points (>=3), point spacing (warns if <0.1m or >100m), duplicate detection, and NaN/Infinity checks
- Validation returns detailed ValidationResult with specific error messages and warnings

**Rationale:** Supports complex curve paths with detailed validation to ensure curve quality before use in guidance calculations.

### ContourLine Model
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/ContourLine.cs`

Implemented contour recording model with:
- IsLocked flag to distinguish between recording and finalized contours
- MinDistanceThreshold for controlling point density during recording
- IsRecording computed property (has points but not locked)
- Self-intersection detection using point-to-segment distance calculations
- Validation enforces minimum 10 points and 10m length for locked contours, checks point spacing consistency, and detects potential self-intersections

**Rationale:** Enables real-time contour recording with quality checks to prevent unusable contour data and provides early warning of potential issues.

### GuidanceLineResult Model
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/GuidanceLineResult.cs`

Note: This file was modified by a linter after initial creation. The linter simplified some XML comments and adjusted formatting.

Implemented guidance calculation result with:
- CrossTrackError (signed distance from line, positive = right)
- ClosestPoint on guidance line
- HeadingError in degrees (vehicle heading vs line heading)
- DistanceToLine (absolute distance)
- ClosestPointIndex for curve/contour optimization (-1 for AB lines)
- Helper methods: IsWithinTolerance() and GetDirectionIndicator()

**Rationale:** Provides standardized result format for all guidance services with helper methods for common UI/control scenarios.

### SmoothingParameters Model
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/SmoothingParameters.cs`

Implemented smoothing configuration with:
- SmoothingMethod enum (CubicSpline, CatmullRom, BSpline) with XML documentation explaining use cases
- Parameters: Tension (0.0-1.0), PointDensity (target spacing in meters), MaxIterations
- Factory methods: CreateDefault(), CreateHighQuality(), CreatePerformance(), CreateShapePreserving()
- Validation ensures parameters are within valid ranges and warns about potential performance issues

**Rationale:** Provides flexible smoothing configuration with sensible presets while allowing customization and validation to prevent invalid configurations.

### UnitSystem Enum and Extensions
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/UnitSystem.cs`

Implemented unit system with:
- Enum values: Metric, Imperial
- Extension methods: MetersToFeet(), FeetToMeters(), ToMeters(), FromMeters()
- Helper methods: GetUnitAbbreviation(), GetUnitName()
- Conversion factors: 3.28084 (m to ft), 0.3048 (ft to m)

**Rationale:** Centralizes unit conversion logic ensuring consistent conversions across all guidance services and supporting both measurement systems at API boundaries.

### ValidationResult Model
**Location:** `AgValoniaGPS/AgValoniaGPS.Models/Guidance/ValidationResult.cs`

Note: This file was modified by a linter after initial creation. The linter simplified the implementation of AddError() to set IsValid = false directly.

Implemented validation result with:
- ErrorMessages and Warnings collections
- IsValid property (true when no errors)
- Methods: AddError(), AddWarning(), Merge(), Clear(), ThrowIfInvalid()
- Static factory methods: Success(), Failure()
- Formatted output via GetFormattedMessages() and ToString()

**Rationale:** Provides rich validation feedback with clear separation between errors (blocking) and warnings (informational), enabling detailed user feedback and programmatic validation checks.

## Database Changes (if applicable)

No database changes required for this task group. All models are in-memory data structures designed for serialization through FieldService (to be implemented in Task Group 8).

## Dependencies (if applicable)

### New Dependencies Added
None - all models use .NET 8.0 base class library types only.

### Configuration Changes
None required for this task group.

## Testing

### Test Files Created/Updated
No test files created in this task group. Testing is delegated to Task Group 10 (Test Review & Gap Analysis) per the spec which states testing-engineer will handle comprehensive test coverage.

### Test Coverage
Unit tests: ❌ None (intentional - testing assigned to testing-engineer role)
Integration tests: ❌ None (intentional - testing assigned to testing-engineer role)
Edge cases covered: ❌ None (intentional - testing assigned to testing-engineer role)

### Manual Testing Performed
Manual verification performed:
1. Built AgValoniaGPS.Models project successfully with no compilation errors
2. Verified all models compile without warnings
3. Verified XML documentation is present on all public members
4. Code review of validation logic for correctness
5. Verified no UI framework dependencies (only System and AgValoniaGPS.Models namespaces used)

## User Standards & Preferences Compliance

### agent-os/standards/backend/api.md
**File Reference:** `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/standards/backend/api.md`

**How Your Implementation Complies:**
While this task focuses on data models rather than API endpoints, the models are designed with API usage in mind - all distance values use meters internally with conversion utilities at the API boundary (UnitSystemExtensions), following the standard of consistent internal representation with external conversion.

**Deviations (if any):**
Not applicable - this is a data models task, not API endpoint implementation.

---

### agent-os/standards/global/coding-style.md
**File Reference:** `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/standards/global/coding-style.md`

**How Your Implementation Complies:**
All code follows small focused functions (each validation method handles one aspect), meaningful names (e.g., MinDistanceThreshold, CrossTrackError, IsWithinTolerance), DRY principle (shared validation logic patterns, reusable extension methods), and consistent indentation. No dead code or commented-out blocks exist. XML documentation provides clear intent for all public members.

**Deviations (if any):**
None

---

### agent-os/standards/global/conventions.md
**File Reference:** `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/standards/global/conventions.md`

**How Your Implementation Complies:**
Models are organized in a clear structure under AgValoniaGPS.Models/Guidance/ following predictable naming conventions. All classes use PascalCase, properties use PascalCase, private fields would use camelCase (though these models use auto-properties). Comprehensive XML documentation is provided on all public types and members.

**Deviations (if any):**
None

---

### agent-os/standards/global/error-handling.md
**File Reference:** `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/standards/global/error-handling.md`

**How Your Implementation Complies:**
Validation methods fail explicitly with clear error messages (e.g., "Curve must have at least 3 points. Current count: 2"). ValidationResult provides user-friendly messages without exposing technical details. The ThrowIfInvalid() method enables fail-fast behavior when needed. All validation checks are specific to the type of error (minimum length, NaN values, point spacing).

**Deviations (if any):**
None

---

### agent-os/standards/global/commenting.md
**File Reference:** `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/standards/global/commenting.md`

**How Your Implementation Complies:**
Code is self-documenting through clear naming (e.g., IsWithinTolerance, GetDirectionIndicator, CreateHighQuality). XML documentation provides concise, helpful explanations of purpose and usage. No comments about recent changes or temporary fixes - all documentation is evergreen. Minimal inline comments used only for complex mathematical operations.

**Deviations (if any):**
None

---

### agent-os/standards/global/validation.md
**File Reference:** `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/standards/global/validation.md`

**How Your Implementation Complies:**
Validation is performed at the model level through Validate() methods on ABLine, CurveLine, ContourLine, and SmoothingParameters. Each validation checks specific requirements (minimum points, valid ranges, no NaN/Infinity) and provides specific error messages. Validation failures are explicit and provide actionable feedback (e.g., "Tension must be between 0.0 and 1.0. Current value: 1.5").

**Deviations (if any):**
None

## Integration Points (if applicable)

### APIs/Endpoints
Not applicable - data models only. Service interfaces will be defined in Task Groups 2-7.

### External Services
None for this task group.

### Internal Dependencies
- Depends on Position model from Wave 1 (reused from AgValoniaGPS.Models)
- Will be consumed by IABLineService, ICurveLineService, IContourService (Task Groups 3-7)
- Will be serialized by FieldService extensions (Task Group 8)

## Known Issues & Limitations

### Issues
None identified.

### Limitations
1. **Self-Intersection Detection**
   - Description: ContourLine.CheckForSelfIntersection() uses a basic proximity check (0.5m threshold)
   - Reason: Full geometric intersection detection would be complex and potentially slow for real-time use
   - Future Consideration: Could be enhanced with proper line segment intersection algorithms if false positives/negatives become an issue

2. **Validation Performance**
   - Description: Validation methods iterate through all points which could be slow for very large curves (10,000+ points)
   - Reason: Prioritized correctness and clarity over optimization for this initial implementation
   - Future Consideration: Could add caching or incremental validation if performance becomes an issue

## Performance Considerations
- All computed properties (Length, MidPoint, TotalLength) recalculate on each access - acceptable for infrequent access but could be cached if performance profiling identifies issues
- Validation methods iterate through all points - O(n) complexity which meets performance targets for expected curve sizes (typically <1000 points)
- No memory allocations in hot paths - validation uses simple loops without creating temporary collections

## Security Considerations
- All models validate input for NaN/Infinity which prevents numeric instability attacks
- No user input is directly stored without validation
- No reflection or dynamic code execution
- Models are safe for serialization (no sensitive data, no circular references in current implementation)

## Dependencies for Other Tasks
- Task Group 2 (Event Infrastructure) - will use these models in event args
- Task Group 3 (AB Line Service) - will create and validate ABLine models
- Task Group 5 (Curve Line Service) - will create and validate CurveLine models
- Task Group 7 (Contour Service) - will create and validate ContourLine models
- Task Group 8 (Field Service Integration) - will serialize/deserialize these models

## Notes
1. The Position model from Wave 1 was successfully reused, avoiding duplication and ensuring consistency with existing services
2. All models compile successfully with no warnings or errors
3. A linter modified GuidanceLineResult.cs and ValidationResult.cs after initial creation to simplify implementations - changes were minor and improved code quality
4. The SmoothingMethod enum includes detailed XML documentation explaining the characteristics and use cases for each smoothing algorithm (CubicSpline, CatmullRom, BSpline) to help future service implementers choose the right algorithm
5. Factory methods in SmoothingParameters (CreateHighQuality, CreatePerformance, etc.) provide opinionated presets that will help users get good results without deep knowledge of smoothing algorithms
6. ValidationResult includes a rich API (Merge, ThrowIfInvalid, GetFormattedMessages) that will simplify error handling in services and UI layers
7. All validation methods return ValidationResult rather than throwing exceptions, following the fail-explicit pattern while allowing callers to decide how to handle validation failures
