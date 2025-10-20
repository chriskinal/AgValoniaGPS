# Specification: Wave 7 - Display & Visualization

## Goal
Extract display and visualization business logic from legacy AgOpenGPS into clean, testable, UI-agnostic services that format data for presentation across metric and imperial unit systems.

## User Stories
- As a farmer, I want to see my speed displayed with appropriate precision so that I can monitor my vehicle speed at a glance
- As a farmer, I want to see field statistics (area covered, efficiency, work rate) formatted in my preferred unit system so that I can track my progress
- As a farmer, I want GPS quality indicators with color coding so that I can quickly assess positioning accuracy
- As a farmer, I want cross-track error displayed prominently so that I can maintain accurate guidance
- As a developer, I want all display formatters to be testable and UI-agnostic so that I can verify formatting logic independently of the UI framework

## Core Requirements

### Functional Requirements
- Format speed with dynamic precision (1 decimal >2 km/h, 2 decimals ≤2 km/h)
- Format heading in degrees with ° symbol
- Format GPS quality with color-coded text (RTK Fixed=PaleGreen, Float=Orange, DGPS=Yellow, Others=Red)
- Format cross-track error in cm (metric) or inches (imperial)
- Format distances, areas, times, rates, and percentages with appropriate units
- Calculate application statistics (applied area, actual area, efficiency, work rate)
- Provide rotating display data for 3 information screens
- Support metric and imperial unit systems (no mixed units)
- Use InvariantCulture for all number formatting

### Non-Functional Requirements
- Performance: All formatting operations must complete in <1ms
- All services must be thread-safe and UI-agnostic
- No caching required - straightforward calculations only
- 100% test coverage for all formatting logic
- Return safe defaults for invalid inputs (NaN, Infinity) - never throw exceptions

## Visual Design

### Mockup References
- `planning/visuals/field-stats-1.png` - Application statistics display (Screen 1 of rotating display)
- `planning/visuals/field-stats-2.png` - Field name display (Screen 2 of rotating display)
- `planning/visuals/field-stats-3.png` - Guidance line info display (Screen 3 of rotating display)
- `planning/visuals/gps-quality-indicator.png` - GPS quality and precision indicators
- `planning/visuals/speed-display.png` - Speed gauge with numeric display
- `planning/visuals/legacy-display-shot.png` - Full legacy application reference

### Key UI Elements to Implement (Service Layer Only)
The services will provide formatted data for these UI elements (actual UI implementation is out of scope):

**GPS Quality Header** (consistent across all screens):
- Format: "[FixType]: Age: [age]"
- Example: "RTK fix: Age: 0.0"
- Color mapping by fix type

**Rotating Display Screen 1 - Application Statistics**:
- Format: "[area] App: [app_rate] Actual: [actual_rate] [percentage]% [rate] ac/hr"
- Example: "34.42 App: 0.00 Actual: 0.00 100.0% 0.0 ac/hr"

**Rotating Display Screen 2 - Field Name**:
- Format: "Field: [field_name]"
- Example: "Field: Test Field"

**Rotating Display Screen 3 - Guidance Line**:
- Format: "Line: [line_type] [heading]°"
- Example: "Line: AB 0°"

**Cross-Track Error** (large display on all screens):
- Format: "[value] [unit]"
- Examples: "1.8 cm", "0.7 in"

**Speed Display**:
- Numeric value with dynamic precision
- Examples: "20.2", "1.85"

**GPS Precision Indicator**:
- Format: 2 decimal places
- Example: "0.01"

## Reusable Components

### Existing Code to Leverage

**UnitSystem Enum** - `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Guidance/UnitSystem.cs`:
- Already defines Metric and Imperial enum values
- Provides UnitSystemExtensions with conversion methods:
  - `MetersToFeet()` and `FeetToMeters()` with conversion factors
  - `ToMeters()` and `FromMeters()` for unit conversion
  - `GetUnitAbbreviation()` and `GetUnitName()` for display
- Can be reused directly for distance conversions

**Position Model** - `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/Position.cs`:
- Contains `Speed` (m/s) and `Heading` (degrees) properties
- Source of data for speed and heading formatters

**Service Registration Pattern** - `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`:
- Established pattern for registering services as Singleton
- Wave 7 services will follow the same registration approach

**Existing Wave Services** - Data sources for display formatters:
- Wave 1 (Position & Kinematics): Position data
- Wave 2 (Guidance): ABLine, CurveLine, Contour data
- Wave 4 (Section Control): Section state and coverage data
- Wave 5 (Field Operations): Field boundary and area data
- Wave 6 (Communication): GPS fix type and quality data

### New Components Required

**DisplayFormatterService** - Does not exist in current codebase:
- Unified service for all display formatting operations
- Cannot reuse existing code because legacy formatting is embedded in WinForms UI
- Required to decouple formatting logic from UI layer
- Will provide culture-invariant, testable formatting methods

**Display Models** - Do not exist in current codebase:
- `ApplicationStatistics` - Structure for application rate statistics
- `GpsQualityDisplay` - Structure for GPS quality text and color
- `RotatingDisplayData` - Structure for cycling display screens
- Required to provide strongly-typed data structures for display

**FieldStatisticsService Enhancement** - Partial existence:
- Directory structure does not currently exist (need to create `FieldStatistics/` directory)
- Basic field statistics may exist in legacy code but need to be extracted
- Required to calculate application statistics and provide rotating display data

## Technical Approach

### Database
No database changes required. All services consume existing data from Wave 1-6 services.

### API

**IDisplayFormatterService Interface**:
```csharp
public interface IDisplayFormatterService
{
    // Speed formatting
    string FormatSpeed(double speedMetersPerSecond, UnitSystem unitSystem);

    // Heading formatting
    string FormatHeading(double headingDegrees);

    // GPS quality formatting
    GpsQualityDisplay FormatGpsQuality(GpsFixType fixType, double age);

    // GPS precision formatting
    string FormatGpsPrecision(double precisionMeters);

    // Cross-track error formatting
    string FormatCrossTrackError(double errorMeters, UnitSystem unitSystem);

    // Distance formatting
    string FormatDistance(double distanceMeters, UnitSystem unitSystem);

    // Area formatting
    string FormatArea(double areaSquareMeters, UnitSystem unitSystem);

    // Time formatting
    string FormatTime(double hours);

    // Rate formatting
    string FormatApplicationRate(double rate, UnitSystem unitSystem);

    // Percentage formatting
    string FormatPercentage(double percentage);

    // Guidance line formatting
    string FormatGuidanceLine(GuidanceLineType lineType, double headingDegrees);
}
```

**IFieldStatisticsService Interface** (expansion of existing):
```csharp
public interface IFieldStatisticsService
{
    // Existing methods (to be preserved)
    // ...

    // New methods for Wave 7
    ApplicationStatistics CalculateApplicationStatistics();
    RotatingDisplayData GetRotatingDisplayData(int screenNumber);
    string GetCurrentFieldName();
    (GuidanceLineType Type, double Heading) GetActiveGuidanceLineInfo();
}
```

**Data Models**:

```csharp
// AgValoniaGPS.Models/Display/ApplicationStatistics.cs
public class ApplicationStatistics
{
    public double TotalAreaCovered { get; set; }      // m²
    public double ApplicationRateTarget { get; set; } // target rate
    public double ActualApplicationRate { get; set; } // actual rate
    public double CoveragePercentage { get; set; }    // 0-100
    public double WorkRate { get; set; }              // area per hour
}

// AgValoniaGPS.Models/Display/GpsQualityDisplay.cs
public class GpsQualityDisplay
{
    public string FormattedText { get; set; }
    public string ColorName { get; set; } // "PaleGreen", "Orange", "Yellow", "Red"
}

// AgValoniaGPS.Models/Display/RotatingDisplayData.cs
public class RotatingDisplayData
{
    public int CurrentScreen { get; set; } // 1, 2, or 3
    public ApplicationStatistics AppStats { get; set; }
    public string FieldName { get; set; }
    public string GuidanceLineInfo { get; set; }
}

// AgValoniaGPS.Models/Display/GuidanceLineType.cs
public enum GuidanceLineType
{
    AB,
    Curve,
    Contour
}

// AgValoniaGPS.Models/Display/GpsFixType.cs (if not already exists)
public enum GpsFixType
{
    None = 0,
    Autonomous = 1,
    DGPS = 2,
    RtkFloat = 5,
    RtkFixed = 4
}
```

### Frontend
All services are UI-agnostic. Frontend integration (Avalonia UI binding) is explicitly out of scope for Wave 7.

### Testing

**DisplayFormatterService Tests**:
- Test each formatter method with metric and imperial units
- Test dynamic speed precision logic (>2 km/h vs ≤2 km/h)
- Test GPS quality color mapping for all fix types
- Test edge cases: zero, negative, NaN, Infinity values
- Test InvariantCulture formatting (decimal separators)
- Performance tests: verify <1ms execution time

**FieldStatisticsService Tests**:
- Test application statistics calculations
- Test rotating display data generation
- Test field name retrieval
- Test guidance line info retrieval

**Unit Conversion Tests**:
- Verify conversion accuracy (meters to feet, km to miles, etc.)
- Test conversion constants match legacy values
- Test area conversions (m² to acres/hectares)
- Test speed conversions (m/s to km/h and mph)

## Formatting Rules (Detailed Specifications)

### Speed Formatting
- Input: Speed in meters per second
- Convert to km/h (metric) or mph (imperial)
- Dynamic precision:
  - If converted speed > 2 km/h: 1 decimal place
  - If converted speed ≤ 2 km/h: 2 decimal places
- No unit suffix (unit label shown separately in UI)
- Use InvariantCulture
- Examples:
  - 5.56 m/s, Metric → "20.0" km/h (5.56 * 3.6 = 20.016)
  - 0.51 m/s, Metric → "1.85" km/h (0.51 * 3.6 = 1.836)
  - 8.94 m/s, Imperial → "20.0" mph (8.94 * 2.23694 = 20.0)

### Heading Formatting
- Input: Heading in degrees (0-360)
- Round to nearest whole degree
- Append ° symbol
- Examples:
  - 0.0 → "0°"
  - 45.5 → "46°"
  - 359.8 → "360°"

### GPS Quality Formatting
- Input: Fix type enum, age in seconds
- Output: Formatted text with color name
- Format: "[FixType]: Age: [age]"
- Age: 1 decimal place
- Color mapping:
  - RtkFixed (4) → "RTK fix: Age: 0.0", "PaleGreen"
  - RtkFloat (5) → "RTK float: Age: 1.2", "Orange"
  - DGPS (2) → "DGPS: Age: 0.5", "Yellow"
  - Autonomous (1) → "Autonomous: Age: 2.0", "Red"
  - None (0) → "No Fix: Age: 0.0", "Red"

### Cross-Track Error Formatting
- Input: Error in meters
- Metric: Convert to centimeters (× 100), 1 decimal, append " cm"
- Imperial: Convert to inches (× 39.3701), 1 decimal, append " in"
- Examples:
  - 0.018 m, Metric → "1.8 cm"
  - 0.002 m, Metric → "0.2 cm"
  - 0.018 m, Imperial → "0.7 in"

### Distance Formatting
- Input: Distance in meters
- Metric:
  - < 1000 m: "[value] m" with 1 decimal
  - ≥ 1000 m: "[value] km" with 2 decimals
- Imperial:
  - < 5280 ft: "[value] ft" with 0 decimals
  - ≥ 5280 ft: "[value] mi" with 2 decimals
- Examples:
  - 450.5 m, Metric → "450.5 m"
  - 1250.0 m, Metric → "1.25 km"
  - 914.4 m, Imperial → "3000 ft"
  - 1609.3 m, Imperial → "1.00 mi"

### Area Formatting
- Input: Area in square meters
- Metric: Convert to hectares (÷ 10000), 2 decimals, append " ha"
- Imperial: Convert to acres (× 0.000247105), 2 decimals, append " ac"
- Examples:
  - 10000 m², Metric → "1.00 ha"
  - 10000 m², Imperial → "2.47 ac"

### Time Formatting
- Input: Time in hours
- Format: 2 decimal places, append " hr"
- Examples:
  - 2.25 → "2.25 hr"
  - 0.5 → "0.50 hr"

### Application Rate Formatting
- Input: Rate value
- Metric: "[value] ha/hr" with 2 decimals
- Imperial: "[value] ac/hr" with 2 decimals
- Examples:
  - 5.0, Imperial → "5.00 ac/hr"
  - 2.5, Metric → "2.50 ha/hr"

### Percentage Formatting
- Input: Percentage (0-100)
- Format: 1 decimal place, append "%"
- Examples:
  - 100.0 → "100.0%"
  - 75.5 → "75.5%"

### Guidance Line Formatting
- Input: Line type enum, heading in degrees
- Format: "Line: [LineType] [heading]°"
- Heading rounded to nearest degree
- Examples:
  - AB, 0.0 → "Line: AB 0°"
  - Curve, 45.5 → "Line: Curve 46°"

## Conversion Constants
All conversion factors must use high precision and match legacy AgOpenGPS values:

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

## Service Organization

### Directory Structure
```
AgValoniaGPS.Services/
  Display/
    DisplayFormatterService.cs
    IDisplayFormatterService.cs
  FieldStatistics/
    FieldStatisticsService.cs
    IFieldStatisticsService.cs

AgValoniaGPS.Models/
  Display/
    ApplicationStatistics.cs
    GpsQualityDisplay.cs
    RotatingDisplayData.cs
    GuidanceLineType.cs
    GpsFixType.cs (if needed)

AgValoniaGPS.Services.Tests/
  Display/
    DisplayFormatterServiceTests.cs
  FieldStatistics/
    FieldStatisticsServiceTests.cs
```

### Dependency Injection Registration
Add to `ServiceCollectionExtensions.cs`:

```csharp
private static void AddWave7DisplayServices(IServiceCollection services)
{
    // Display Formatter Service - Provides culture-invariant formatting for all display elements
    services.AddSingleton<IDisplayFormatterService, DisplayFormatterService>();

    // Field Statistics Service - Expanded with rotating display and application statistics
    services.AddSingleton<IFieldStatisticsService, FieldStatisticsService>();
}
```

Call from `AddAgValoniaServices()`:
```csharp
// Wave 7: Display & Visualization Services
AddWave7DisplayServices(services);
```

## Integration Points

### With Wave 1 (Position & Kinematics)
- Consume `Position.Speed` and `Position.Heading` for speed/heading formatters
- Use `IPositionUpdateService` to access current position data

### With Wave 2 (Guidance Line Core)
- Consume active guidance line data for guidance line formatter
- Use `IABLineService`, `ICurveLineService`, `IContourService` for line info

### With Wave 4 (Section Control)
- Consume section state and coverage data for application statistics
- Use `ISectionControlService` and `ICoverageMapService`

### With Wave 5 (Field Operations)
- Consume field boundary data for field name and area calculations
- Use `IBoundaryManagementService` for field information

### With Wave 6 (Hardware I/O)
- Consume GPS fix type and quality data for GPS quality formatter
- Use appropriate communication services for fix type information

## Out of Scope

### UI Implementation (Separate Wave)
- Avalonia view models and data binding
- Avalonia UI controls and views
- Rotating display timer/animation logic
- Color object instantiation (services return color names only)
- Gauge visualizations and analog displays
- Icon rendering and status bars

### Data Acquisition (Already Handled by Waves 1-6)
- GPS data retrieval
- Field data retrieval
- Section control data collection
- Guidance line data tracking

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

## Success Criteria

### Code Extraction
- Approximately 1,000 lines of business logic extracted from legacy AgOpenGPS
- All display formatters are testable and UI-agnostic
- Zero dependencies on UI frameworks (WinForms, Avalonia, etc.)

### Performance Targets
- All formatting operations complete in <1ms
- No memory allocations beyond necessary string creation
- Thread-safe for concurrent access from UI and background threads

### Test Coverage
- 100% test coverage for all formatting methods
- All edge cases tested (zero, negative, NaN, Infinity)
- All unit conversions verified for accuracy
- Dynamic precision logic validated with boundary cases
- Culture-invariant formatting verified

### Quality Metrics
- All formatters return safe defaults for invalid inputs (never throw)
- InvariantCulture used consistently throughout
- Conversion constants match legacy values exactly
- Services registered and accessible via dependency injection

### Integration Success
- Services integrate cleanly with existing Wave 1-6 services
- Data models are strongly-typed and reusable
- Service interfaces are well-defined and mockable for testing
- Dependency injection registration follows established patterns
