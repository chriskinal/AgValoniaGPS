# Task Breakdown: Wave 7 - Display & Visualization

## Overview
**Total Task Groups:** 6
**Complexity Level:** LOW (simpler than Waves 3-6)
**Estimated LOC:** ~1,000 lines of business logic extraction
**Performance Target:** <1ms for all formatting operations

**Assigned Implementers:**
- `api-engineer` - Business logic services and formatters
- `testing-engineer` - Test review and gap analysis

**Note:** This wave does NOT require database or UI implementation. All services are UI-agnostic business logic layers.

## Task List

### Domain Models & Data Structures

#### Task Group 1: Display Domain Models
**Assigned implementer:** api-engineer
**Dependencies:** None
**Estimated Effort:** 2-3 hours

- [x] 1.0 Complete display domain models
  - [x] 1.1 Write 2-6 focused tests for domain models
    - Limit to 2-6 highly focused tests maximum
    - Test only critical model behaviors (e.g., property assignment, basic validation)
    - Skip exhaustive coverage of all properties
  - [x] 1.2 Create `Display/` directory in AgValoniaGPS.Models
    - Path: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Display/`
    - Verify no namespace collision with existing classes per `NAMING_CONVENTIONS.md`
  - [x] 1.3 Create ApplicationStatistics.cs model
    - Path: `AgValoniaGPS.Models/Display/ApplicationStatistics.cs`
    - Properties: TotalAreaCovered (m²), ApplicationRateTarget, ActualApplicationRate, CoveragePercentage (0-100), WorkRate (area per hour)
    - Add XML documentation for all properties
  - [x] 1.4 Create GpsQualityDisplay.cs model
    - Path: `AgValoniaGPS.Models/Display/GpsQualityDisplay.cs`
    - Properties: FormattedText (string), ColorName (string)
    - Color names: "PaleGreen", "Orange", "Yellow", "Red"
    - Add XML documentation
  - [x] 1.5 Create RotatingDisplayData.cs model
    - Path: `AgValoniaGPS.Models/Display/RotatingDisplayData.cs`
    - Properties: CurrentScreen (int 1-3), AppStats (ApplicationStatistics), FieldName (string), GuidanceLineInfo (string)
    - Add XML documentation
  - [x] 1.6 Create GuidanceLineType.cs enum
    - Path: `AgValoniaGPS.Models/Display/GuidanceLineType.cs`
    - Values: AB, Curve, Contour
    - Add XML documentation for each value
  - [x] 1.7 Create GpsFixType.cs enum (if not already exists in Communication)
    - Check: `AgValoniaGPS.Models/Communication/` first
    - If exists, skip this step
    - If not exists, create with values: None=0, Autonomous=1, DGPS=2, RtkFixed=4, RtkFloat=5
    - Add XML documentation for each value
  - [x] 1.8 Ensure domain model tests pass
    - Run ONLY the 2-6 tests written in 1.1
    - Verify model instantiation and property assignment work
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-6 tests written in 1.1 pass
- All models have proper XML documentation
- Models follow existing AgValoniaGPS naming conventions
- No namespace collisions with existing classes
- All properties are strongly typed

---

### Display Formatter Service - Core Implementation

#### Task Group 2: DisplayFormatterService Foundation & Basic Formatters
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 1
**Estimated Effort:** 4-5 hours

- [x] 2.0 Complete DisplayFormatterService foundation and basic formatters
  - [x] 2.1 Write 4-8 focused tests for basic formatters
    - Limit to 4-8 highly focused tests maximum
    - Test speed formatter (dynamic precision >2 km/h vs ≤2 km/h)
    - Test heading formatter (degree symbol, rounding)
    - Test distance formatter (metric: m vs km, imperial: ft vs mi)
    - Test both unit systems for each formatter
    - Skip exhaustive edge case testing (NaN, Infinity, negative)
  - [x] 2.2 Create `Display/` directory in AgValoniaGPS.Services
    - Path: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services/Display/`
    - Verify no namespace collision per `NAMING_CONVENTIONS.md`
  - [x] 2.3 Create IDisplayFormatterService.cs interface
    - Path: `AgValoniaGPS.Services/Display/IDisplayFormatterService.cs`
    - Define all 11 formatter method signatures per spec.md
    - Use InvariantCulture for all formatting
    - Add XML documentation for each method with examples
  - [x] 2.4 Create DisplayFormatterService.cs class
    - Path: `AgValoniaGPS.Services/Display/DisplayFormatterService.cs`
    - Implement IDisplayFormatterService
    - Add all conversion constants as private const (MetersToFeet=3.28084, MetersToInches=39.3701, etc.)
    - Reuse existing UnitSystem enum from `AgValoniaGPS.Models.Guidance.UnitSystem`
    - Add comprehensive XML documentation
  - [x] 2.5 Implement FormatSpeed method
    - Input: double speedMetersPerSecond, UnitSystem unitSystem
    - Convert to km/h (metric) or mph (imperial)
    - Dynamic precision: 1 decimal if >2 km/h, 2 decimals if ≤2 km/h
    - Use InvariantCulture: `value.ToString("F1", CultureInfo.InvariantCulture)`
    - Return safe default "0.0" for NaN/Infinity
    - Examples: 5.56 m/s, Metric → "20.0" km/h
  - [x] 2.6 Implement FormatHeading method
    - Input: double headingDegrees
    - Round to nearest whole degree: `Math.Round(headingDegrees, 0)`
    - Append ° symbol (Unicode \u00B0 or "°")
    - Handle wrapping (360° wrap to 0°)
    - Return "0°" for NaN/Infinity
    - Examples: 45.5 → "46°", 359.8 → "360°"
  - [x] 2.7 Implement FormatDistance method
    - Input: double distanceMeters, UnitSystem unitSystem
    - Metric: <1000m → "X.X m", ≥1000m → "X.XX km"
    - Imperial: <5280ft → "XXXX ft" (0 decimals), ≥5280ft → "X.XX mi"
    - Use appropriate conversion factors
    - Return "0.0 m" or "0 ft" for NaN/Infinity
    - Examples: 1250.0 m, Metric → "1.25 km"
  - [x] 2.8 Ensure basic formatter tests pass
    - Run ONLY the 4-8 tests written in 2.1
    - Verify speed, heading, distance formatters work correctly
    - Verify both metric and imperial unit systems
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 4-8 tests written in 2.1 pass
- Speed formatter uses dynamic precision correctly
- Heading formatter includes ° symbol
- Distance formatter switches units based on magnitude
- Both metric and imperial unit systems supported
- InvariantCulture used consistently
- No exceptions thrown for invalid inputs

---

### Display Formatter Service - Advanced Formatters

#### Task Group 3: DisplayFormatterService Advanced & GPS Formatters
**Assigned implementer:** api-engineer
**Dependencies:** Task Group 2
**Estimated Effort:** 4-5 hours

- [x] 3.0 Complete advanced and GPS formatters
  - [x] 3.1 Write 4-8 focused tests for advanced formatters
    - Limit to 4-8 highly focused tests maximum
    - Test area formatter (hectares vs acres)
    - Test cross-track error formatter (cm vs inches)
    - Test GPS quality formatter (color mapping by fix type)
    - Test percentage formatter (1 decimal precision)
    - Skip exhaustive edge case testing
  - [x] 3.2 Implement FormatArea method
    - Input: double areaSquareMeters, UnitSystem unitSystem
    - Metric: Convert to hectares (÷10000), 2 decimals, append " ha"
    - Imperial: Convert to acres (×0.000247105), 2 decimals, append " ac"
    - Use InvariantCulture
    - Return "0.00 ha" or "0.00 ac" for NaN/Infinity
    - Examples: 10000 m², Metric → "1.00 ha"
  - [x] 3.3 Implement FormatCrossTrackError method
    - Input: double errorMeters, UnitSystem unitSystem
    - Metric: Convert to cm (×100), 1 decimal, append " cm"
    - Imperial: Convert to inches (×39.3701), 1 decimal, append " in"
    - Use InvariantCulture
    - Return "0.0 cm" or "0.0 in" for NaN/Infinity
    - Examples: 0.018 m, Metric → "1.8 cm"
  - [x] 3.4 Implement FormatGpsQuality method
    - Input: GpsFixType fixType, double age
    - Output: GpsQualityDisplay with FormattedText and ColorName
    - Format: "[FixType]: Age: [age with 1 decimal]"
    - Color mapping:
      - RtkFixed (4) → "RTK fix: Age: X.X", "PaleGreen"
      - RtkFloat (5) → "RTK float: Age: X.X", "Orange"
      - DGPS (2) → "DGPS: Age: X.X", "Yellow"
      - Autonomous (1) → "Autonomous: Age: X.X", "Red"
      - None (0) or other → "No Fix: Age: X.X", "Red"
    - Use InvariantCulture for age formatting
    - Examples: RtkFixed, 0.0 → "RTK fix: Age: 0.0", "PaleGreen"
  - [x] 3.5 Implement FormatGpsPrecision method
    - Input: double precisionMeters
    - Format: 2 decimal places, no unit suffix
    - Use InvariantCulture
    - Return "0.00" for NaN/Infinity
    - Examples: 0.012 → "0.01"
  - [x] 3.6 Implement FormatPercentage method
    - Input: double percentage (0-100)
    - Format: 1 decimal place, append "%"
    - Use InvariantCulture
    - Return "0.0%" for NaN/Infinity
    - Examples: 100.0 → "100.0%", 75.5 → "75.5%"
  - [x] 3.7 Implement FormatTime method
    - Input: double hours
    - Format: 2 decimal places, append " hr"
    - Use InvariantCulture
    - Return "0.00 hr" for NaN/Infinity
    - Examples: 2.25 → "2.25 hr", 0.5 → "0.50 hr"
  - [x] 3.8 Implement FormatApplicationRate method
    - Input: double rate, UnitSystem unitSystem
    - Metric: Format with 2 decimals, append " ha/hr"
    - Imperial: Format with 2 decimals, append " ac/hr"
    - Use InvariantCulture
    - Return "0.00 ha/hr" or "0.00 ac/hr" for NaN/Infinity
    - Examples: 5.0, Imperial → "5.00 ac/hr"
  - [x] 3.9 Implement FormatGuidanceLine method
    - Input: GuidanceLineType lineType, double headingDegrees
    - Format: "Line: [LineType] [heading]°"
    - Round heading to nearest degree
    - Use InvariantCulture
    - Return "Line: AB 0°" for NaN/Infinity heading
    - Examples: AB, 0.0 → "Line: AB 0°", Curve, 45.5 → "Line: Curve 46°"
  - [x] 3.10 Ensure advanced formatter tests pass
    - Run ONLY the 4-8 tests written in 3.1
    - Verify area, cross-track error, GPS quality, percentage formatters work
    - Verify GPS quality color mapping is correct
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 4-8 tests written in 3.1 pass
- All formatters use InvariantCulture
- Area formatter converts to hectares/acres correctly
- Cross-track error formatter converts to cm/inches correctly
- GPS quality formatter returns correct color names
- All formatters handle invalid inputs gracefully (no exceptions)
- Performance target: each formatter <1ms execution time

---

### Field Statistics Service Enhancement

#### Task Group 4: FieldStatisticsService Expansion
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1-3
**Estimated Effort:** 3-4 hours

- [x] 4.0 Expand FieldStatisticsService with rotating display support
  - [x] 4.1 Write 4-6 focused tests for new methods
    - Limit to 4-6 highly focused tests maximum
    - Test CalculateApplicationStatistics method
    - Test GetRotatingDisplayData method (screens 1-3)
    - Test GetCurrentFieldName method
    - Test GetActiveGuidanceLineInfo method
    - Skip edge cases and null handling tests
  - [x] 4.2 Update existing FieldStatisticsService.cs
    - Path: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services/FieldStatisticsService.cs`
    - Keep all existing methods (WorkedAreaSquareMeters, GetRemainingAreaHectares, etc.)
    - Add new methods for Wave 7 functionality
    - Remove existing FormatArea and FormatDistance methods (now in DisplayFormatterService)
  - [x] 4.3 Create IFieldStatisticsService.cs interface (if not exists)
    - Path: `AgValoniaGPS.Services/IFieldStatisticsService.cs`
    - Include all existing methods from current FieldStatisticsService
    - Add new method signatures for Wave 7
    - Add comprehensive XML documentation
  - [x] 4.4 Implement CalculateApplicationStatistics method
    - Output: ApplicationStatistics object
    - Calculate: TotalAreaCovered (from WorkedAreaSquareMeters)
    - Calculate: ApplicationRateTarget (from configuration/defaults)
    - Calculate: ActualApplicationRate (based on section control data)
    - Calculate: CoveragePercentage (ActualAreaCovered / BoundaryAreaSquareMeters * 100)
    - Calculate: WorkRate (use existing GetWorkRatePerHour method)
    - Return defaults (zeros) for invalid data
  - [x] 4.5 Implement GetRotatingDisplayData method
    - Input: int screenNumber (1, 2, or 3)
    - Output: RotatingDisplayData object
    - Screen 1: Populate AppStats using CalculateApplicationStatistics
    - Screen 2: Populate FieldName using GetCurrentFieldName
    - Screen 3: Populate GuidanceLineInfo using GetActiveGuidanceLineInfo
    - Set CurrentScreen property to screenNumber
    - Return appropriate data based on screen number
  - [x] 4.6 Implement GetCurrentFieldName method
    - Output: string (field name)
    - Retrieve from current field selection (may need dependency on FieldService)
    - Return "No Field" or empty string if no field selected
    - Add XML documentation
  - [x] 4.7 Implement GetActiveGuidanceLineInfo method
    - Output: (GuidanceLineType Type, double Heading) tuple
    - Retrieve from active guidance line service
    - May need dependencies on IABLineService, ICurveLineService, IContourService
    - Return (GuidanceLineType.AB, 0.0) as default if no active line
    - Add XML documentation
  - [x] 4.8 Update FieldStatisticsService to implement IFieldStatisticsService
    - Add interface implementation
    - Ensure all methods match interface signature
    - Add constructor for dependency injection of required services
  - [x] 4.9 Ensure FieldStatisticsService tests pass
    - Run ONLY the 4-6 tests written in 4.1
    - Verify new methods return expected data structures
    - Verify rotating display data switches correctly by screen number
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 4-6 tests written in 4.1 pass
- CalculateApplicationStatistics returns populated ApplicationStatistics object
- GetRotatingDisplayData returns appropriate data for screens 1-3
- GetCurrentFieldName retrieves field name correctly
- GetActiveGuidanceLineInfo retrieves guidance line info correctly
- Existing FieldStatisticsService methods remain functional
- Interface properly defines all methods

---

### Integration & Dependency Injection

#### Task Group 5: Service Registration & Integration
**Assigned implementer:** api-engineer
**Dependencies:** Task Groups 1-4
**Estimated Effort:** 2-3 hours

- [x] 5.0 Complete service registration and integration
  - [x] 5.1 Write 2-4 focused integration tests
    - Limit to 2-4 highly focused tests maximum
    - Test DisplayFormatterService resolves from DI container
    - Test FieldStatisticsService resolves from DI container
    - Test end-to-end workflow: get stats → format for display
    - Skip exhaustive integration testing
  - [x] 5.2 Update ServiceCollectionExtensions.cs
    - Path: `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
    - Add private method AddWave7DisplayServices
    - Register DisplayFormatterService as Singleton
    - Register FieldStatisticsService as Singleton (update existing registration if needed)
    - Follow existing Wave 1-6 registration patterns
  - [x] 5.3 Call AddWave7DisplayServices from AddAgValoniaServices
    - Add comment: "// Wave 7: Display & Visualization Services"
    - Call AddWave7DisplayServices(services) after Wave 6 services
    - Maintain sequential wave ordering
  - [x] 5.4 Add XML documentation to service registration
    - Document DisplayFormatterService purpose: "Provides culture-invariant formatting for all display elements"
    - Document FieldStatisticsService purpose: "Expanded with rotating display and application statistics"
  - [x] 5.5 Verify no circular dependencies
    - Check DisplayFormatterService has no service dependencies (should be stateless)
    - Check FieldStatisticsService dependencies are resolvable
    - Test DI container builds successfully
  - [x] 5.6 Create performance benchmark tests
    - Test each DisplayFormatterService method execution time
    - Verify all methods complete in <1ms
    - Use System.Diagnostics.Stopwatch for measurement
    - Run each formatter 1000 times, measure average time
  - [x] 5.7 Ensure integration tests pass
    - Run ONLY the 2-4 tests written in 5.1
    - Verify services resolve from DI container
    - Verify basic end-to-end workflow
    - Do NOT run the entire test suite at this stage
  - [x] 5.8 Ensure performance benchmarks meet targets
    - Run performance benchmark tests
    - Verify all formatters <1ms average execution time
    - Log results for documentation

**Acceptance Criteria:**
- The 2-4 tests written in 5.1 pass
- DisplayFormatterService registered as Singleton in DI container
- FieldStatisticsService registered as Singleton in DI container
- Services resolve correctly from DI container
- No circular dependencies exist
- All formatters meet <1ms performance target
- Registration follows existing Wave 1-6 patterns

---

### Testing & Validation

#### Task Group 6: Test Review & Gap Analysis
**Assigned implementer:** testing-engineer
**Dependencies:** Task Groups 1-5
**Estimated Effort:** 3-4 hours

- [x] 6.0 Review existing tests and fill critical gaps only
  - [x] 6.1 Review tests from Task Groups 1-5
    - Review the 6 tests written by api-engineer (Task 1.1)
    - Review the 8 tests written by api-engineer (Task 2.1)
    - Review the 12 tests written by api-engineer (Task 3.1)
    - Review the 6 tests written by api-engineer (Task 4.1)
    - Review the 4 tests written by api-engineer (Task 5.1)
    - Total existing tests: 36 tests
  - [x] 6.2 Analyze test coverage gaps for Wave 7 only
    - Identified critical edge cases missing from existing tests
    - Focused on: NaN handling, Infinity handling, negative values, boundary conditions
    - Identified unit conversion accuracy gaps (verify constants match spec)
    - Identified InvariantCulture verification gaps (decimal separator consistency)
    - Did NOT assess entire application test coverage
    - Prioritized formatter edge cases over integration tests
  - [x] 6.3 Write up to 12 additional strategic tests maximum
    - Added exactly 12 new tests to fill identified critical gaps
    - Focus on edge case handling:
      - Test formatters with NaN input → return safe defaults
      - Test formatters with Infinity input → return safe defaults
      - Test formatters with negative values → appropriate handling
      - Test speed formatter boundary case: exactly 2.0 km/h (precision switch)
      - Test distance formatter boundary: exactly 1000m and 5280ft (unit switch)
      - Test heading wrapping: 360° → 0° or 360°
      - Test unit conversion accuracy (compare to spec constants)
      - Test InvariantCulture formatting (verify "." not "," for decimals)
      - Test percentage edge cases: 0.0%, 100.0%
    - Did NOT write comprehensive coverage for all scenarios
    - Skipped performance stress tests (covered in Task 5.6)
    - Skipped accessibility tests (out of scope for Wave 7)
  - [x] 6.4 Run feature-specific tests only
    - Ran ONLY tests related to Wave 7 feature (tests from 1.1, 2.1, 3.1, 4.1, 5.1, and 6.3)
    - Total: 47 tests (36 existing + 12 new - 1 duplicate fixed)
    - Did NOT run the entire application test suite
    - Verified all critical edge cases pass
    - Verified unit conversion accuracy
    - Verified InvariantCulture formatting consistency
  - [x] 6.5 Verify test coverage for formatters
    - All critical paths covered for DisplayFormatterService
    - All new FieldStatisticsService methods covered
    - Edge cases covered: NaN, Infinity, negative values, boundary conditions
    - InvariantCulture verified across all formatters
    - Unit conversion accuracy verified
  - [x] 6.6 Create test documentation
    - Test organization documented in implementation report
    - Edge case testing strategy documented
    - Performance benchmark results documented (all formatters < 0.01ms)
    - Unit conversion verification approach documented

**Acceptance Criteria:**
- All feature-specific tests pass (47 tests total)
- Critical edge cases for formatters are covered (NaN, Infinity, negatives)
- Unit conversion accuracy verified against spec constants
- InvariantCulture formatting verified (decimal separator consistency)
- Exactly 12 additional tests added by testing-engineer
- All formatters have comprehensive edge case coverage
- All new FieldStatisticsService methods tested
- Testing focused exclusively on Wave 7 feature requirements

---

## Execution Order

Recommended implementation sequence:

1. **Task Group 1: Domain Models** (Parallel Ready)
   - No dependencies
   - Creates foundation for all other groups
   - Must complete before Groups 2-4

2. **Task Group 2: Basic Formatters** (After Group 1)
   - Depends on: Domain Models (Group 1)
   - Creates DisplayFormatterService foundation
   - Must complete before Group 3

3. **Task Group 3: Advanced Formatters** (After Group 2)
   - Depends on: Basic Formatters (Group 2)
   - Completes DisplayFormatterService implementation
   - Can run in parallel with Group 4 if desired

4. **Task Group 4: FieldStatisticsService** (After Group 1)
   - Depends on: Domain Models (Group 1)
   - Can run in parallel with Groups 2-3 if desired
   - Must complete before Group 5

5. **Task Group 5: Integration & DI** (After Groups 2-4)
   - Depends on: All service implementations (Groups 2-4)
   - Integrates all services
   - Must complete before Group 6

6. **Task Group 6: Testing & Validation** (After Groups 1-5)
   - Depends on: All previous groups
   - Final validation and gap filling
   - Must complete last

**Parallel Execution Opportunities:**
- Groups 2-3 can be parallelized (both work on DisplayFormatterService)
- Group 4 can run in parallel with Groups 2-3 (different services)
- After Group 1 completes, Groups 2 and 4 can start simultaneously

---

## Success Metrics

### Code Extraction
- Approximately 1,000 lines of business logic extracted from legacy AgOpenGPS
- All display formatters are testable and UI-agnostic
- Zero dependencies on UI frameworks (WinForms, Avalonia)
- Formatters decoupled from presentation layer

### Performance Targets
- All formatting operations complete in <1ms
- No memory allocations beyond necessary string creation
- Thread-safe for concurrent access from UI and background threads
- Performance benchmarks documented and verified

### Test Coverage
- 100% test coverage for DisplayFormatterService methods
- 95%+ test coverage for new FieldStatisticsService methods
- All edge cases tested (zero, negative, NaN, Infinity)
- All unit conversions verified for accuracy
- Dynamic precision logic validated with boundary cases
- Culture-invariant formatting verified (decimal separators)

### Quality Metrics
- All formatters return safe defaults for invalid inputs (never throw exceptions)
- InvariantCulture used consistently throughout
- Conversion constants match legacy values exactly per spec
- Services registered and accessible via dependency injection
- No circular dependencies
- Code follows existing AgValoniaGPS patterns and conventions

### Integration Success
- Services integrate cleanly with existing Wave 1-6 services
- Data models are strongly-typed and reusable
- Service interfaces are well-defined and mockable for testing
- Dependency injection registration follows established patterns
- No namespace collisions (verified against NAMING_CONVENTIONS.md)

### Documentation
- All public methods have XML documentation
- All parameters and return values documented
- Examples provided in remarks sections
- Performance characteristics documented where relevant
- Test organization and strategy documented

---

## Key Technical Considerations

### Unit System Support
- Reuse existing `UnitSystem` enum from `AgValoniaGPS.Models.Guidance.UnitSystem`
- Support Metric and Imperial only (no mixed units)
- All internal calculations in base units (meters, square meters, meters per second)
- Convert to display units at formatting time only

### Conversion Constants
All conversion factors must use high precision and match spec values:
```csharp
// Distance
private const double MetersToFeet = 3.28084;
private const double MetersToInches = 39.3701;
private const double MetersToKilometers = 0.001;
private const double MetersToMiles = 0.000621371;

// Speed
private const double MetersPerSecondToKmh = 3.6;
private const double MetersPerSecondToMph = 2.23694;

// Area
private const double SquareMetersToHectares = 0.0001;
private const double SquareMetersToAcres = 0.000247105;

// Thresholds
private const double SpeedPrecisionThresholdKmh = 2.0;
private const double DistanceThresholdMeters = 1000.0;
private const double DistanceThresholdFeet = 5280.0;
```

### Error Handling Strategy
- Never throw exceptions from formatter methods
- Return safe defaults for invalid inputs:
  - NaN → "0.0" or "0.00" with appropriate units
  - Infinity → "0.0" or "0.00" with appropriate units
  - Negative values → Format as-is or return zero (depending on context)
- Validate enum values, return default formatting for invalid enums
- Handle null references gracefully

### Performance Optimization
- No caching required (straightforward calculations per spec)
- Use efficient string formatting methods
- Minimize memory allocations where possible
- Use StringBuilder only if building complex strings
- Avoid LINQ for simple operations in hot paths
- Profile each formatter to ensure <1ms target

### InvariantCulture Usage
Always use `CultureInfo.InvariantCulture` for number formatting:
```csharp
value.ToString("F2", CultureInfo.InvariantCulture)  // 2 decimals
value.ToString("F1", CultureInfo.InvariantCulture)  // 1 decimal
value.ToString("F0", CultureInfo.InvariantCulture)  // 0 decimals (whole number)
```

### Integration with Existing Services
- **Wave 1 (Position & Kinematics):** Consume Position.Speed and Position.Heading
- **Wave 2 (Guidance):** Consume active guidance line data from IABLineService, ICurveLineService, IContourService
- **Wave 4 (Section Control):** Consume section state and coverage data for application statistics
- **Wave 5 (Field Operations):** Consume field boundary data for field name and area calculations
- **Wave 6 (Hardware I/O):** Consume GPS fix type and quality data for GPS quality formatter

### Directory Structure
```
AgValoniaGPS.Services/
  Display/
    DisplayFormatterService.cs
    IDisplayFormatterService.cs
  FieldStatisticsService.cs (existing - updated)
  IFieldStatisticsService.cs (new)

AgValoniaGPS.Models/
  Display/
    ApplicationStatistics.cs
    GpsQualityDisplay.cs
    RotatingDisplayData.cs
    GuidanceLineType.cs
    GpsFixType.cs (check Communication/ first)

AgValoniaGPS.Services.Tests/
  Display/
    DisplayFormatterServiceTests.cs
    DisplayFormatterServiceAdvancedTests.cs
    DisplayFormatterServiceEdgeCaseTests.cs
    DisplayModelsTests.cs
    DisplayIntegrationTests.cs
  FieldStatisticsServiceTests.cs (existing - updated)
```

---

## Out of Scope (Explicitly Excluded)

### UI Implementation (Separate Wave)
- Avalonia view models and data binding
- Avalonia UI controls and views
- Rotating display timer/animation logic
- Color object instantiation (services return color names as strings only)
- Gauge visualizations and analog displays
- Icon rendering and status bars
- UI framework-specific code

### Data Acquisition (Already Handled by Waves 1-6)
- GPS data retrieval
- Field data retrieval
- Section control data collection
- Guidance line data tracking
- Hardware communication

### State Management (Separate Concern)
- Unit system preferences storage
- Current field selection persistence
- Active guidance line tracking state
- Display rotation state management
- User preference settings

### Advanced Features (Future Enhancements)
- Custom unit preferences or mixed units
- Localization/translation of display labels
- User-configurable precision settings
- Custom color schemes
- Theme support
- Accessibility features (screen readers, high contrast)
- Dynamic unit system switching at runtime

---

## Notes

- **Complexity Level:** This is a LOW complexity wave focusing on straightforward data formatting
- **No Database:** Wave 7 requires zero database changes
- **No UI:** All services are UI-agnostic business logic layers
- **Test-First Approach:** Each task group starts with writing 2-8 focused tests
- **Actual Test Count:** Total of 47 tests (36 existing + 12 new - 1 duplicate fixed)
- **Performance Critical:** All formatters must meet <1ms execution time target
- **Culture-Invariant:** InvariantCulture must be used consistently for all number formatting
- **Safe Defaults:** Formatters must never throw exceptions, always return safe defaults

---

Last Updated: 2025-10-20
Specification: Wave 7 - Display & Visualization
Status: Complete
