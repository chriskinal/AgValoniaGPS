# Spec Requirements: Wave 7 - Display & Visualization

## Initial Description
Wave 7 focuses on extracting display and visualization business logic from the legacy AgOpenGPS WinForms application into clean, testable services for AgValoniaGPS.

### Key Features to Extract:

1. **Field Statistics Calculator** - Calculate and format field statistics (area, distance, time remaining)
2. **Display Formatters** - Format data for UI display (speed, heading, GPS quality, steering angle)

### Context

This is part of a systematic extraction roadmap for AgValoniaGPS. Previous waves (0-6) have completed:
- Wave 0: Foundation (already complete)
- Wave 1: Position & Kinematics
- Wave 2: Guidance Line Core
- Wave 3: Steering Algorithms
- Wave 4: Section Control
- Wave 5: Field Operations
- Wave 6: Hardware I/O & Communication

Wave 7 is LOW complexity focusing on data presentation rather than computation.

### Dependencies
- All previous waves (1-6) for data to format and display
- Existing FieldStatisticsService (to be expanded)

### Success Criteria
- ~1,000 lines of business logic extracted
- All formatters are testable and UI-agnostic
- Support for multiple unit systems (metric/imperial)
- Color coding for quality indicators
- Performance: <1ms for all formatting operations

## Requirements Discussion

### First Round Questions

**Q1: Unit System Support** - Should we support metric, imperial, and mixed units (like speed in mph with distance in meters)?
**Answer:** Metric and Imperial only (no mixed units)

**Q2: Speed Precision** - For the dynamic precision you mentioned (1 decimal >2 km/h, 2 decimal ≤2 km/h), should this apply to both metric and imperial?
**Answer:** Yes, continue with dynamic precision (1 decimal >2 km/h, 2 decimal ≤2 km/h)

**Q3: GPS Quality Color Coding** - I assume we should use the existing color mappings (RTK Fixed=PaleGreen, RTK Float=Orange, DGPS=Yellow, Others=Red). Is that correct?
**Answer:** Yes, keep existing mappings (RTK Fixed=PaleGreen, RTK Float=Orange, DGPS=Yellow, Others=Red)

**Q4: Heading Display Formats** - Should we support both degrees (0-360°) and cardinal directions (N, NE, E, etc.), or just degrees?
**Answer:** Degrees only (with ° symbol)

**Q5: Time Formatting** - For time remaining/elapsed, should we format as decimal hours (e.g., "2.5 hr") or hours:minutes (e.g., "2:30")?
**Answer:** Decimal hours (e.g., "2.25 hr")

**Q6: Localization & Culture** - Should formatters respect user's culture settings (thousands separator, decimal separator) or use invariant culture for consistency?
**Answer:** Keep InvariantCulture

**Q7: Formatter Service Organization** - Should we create separate formatter services (SpeedFormatter, HeadingFormatter, etc.) or one unified DisplayFormatterService?
**Answer:** Create one unified DisplayFormatterService

**Q8: Performance Requirement** - Given the <1ms requirement, should formatters cache formatted strings or just perform straightforward calculations?
**Answer:** Straightforward calculation (no caching needed)

**Q9: Explicit Exclusions** - What should NOT be included in Wave 7?
**Answer:** Wiring the backend to the frontend will be a separate body of work

### Existing Code to Reference

**Similar Features Identified:**
- Feature: Field Statistics Service - Path: `AgValoniaGPS/AgValoniaGPS.Services/FieldStatistics/FieldStatisticsService.cs`
  - This service will be expanded with additional formatting and calculation capabilities
  - Current implementation provides basic field area and distance calculations

### Follow-up Questions
None required - all requirements are clear from answers and visual analysis.

## Visual Assets

### Files Provided:
Based on bash check in visuals folder:
- `field-stats-1.png`: Shows rotating field statistics display - Screen 1 of 3
  - Top bar: "RTK fix: Age: 0.0"
  - Second bar: "34.42 App: 0.00 Actual: 0.00 100.0% 0.0 ac/hr"
  - Large display: "1.8 cm" (cross-track error)
  - Small icon showing direction indicator

- `field-stats-2.png`: Shows rotating field statistics display - Screen 2 of 3
  - Top bar: "RTK fix: Age: 0.0"
  - Section control indicator with colored bars (green, orange, yellow)
  - Second bar: "Field: Test Field"
  - Large display: "1.9 cm" (cross-track error)

- `field-stats-3.png`: Shows rotating field statistics display - Screen 3 of 3
  - Top bar: "RTK fix: Age: 0.0"
  - Section control indicator with colored bars
  - Second bar: "Line: AB 0°"
  - Large display: "0.2 cm" (cross-track error)

- `gps-quality-indicator.png`: Shows GPS quality visual indicators
  - GPS signal strength icon (green bars)
  - Numeric display: "0.01"
  - Small horizontal bar indicator (orange/yellow)
  - Additional status icons (folder, X)
  - Speedometer gauge showing "20.2"

- `speed-display.png`: Shows speed display example
  - Multiple status icons on top bar (field map, lightning bolt, signal strength)
  - Numeric display: "0.01"
  - Small horizontal bar indicator
  - Speedometer gauge with red needle showing "20.0"
  - Speed range appears to be 0-21 on gauge

- `legacy-display-shot.png`: Full legacy AgOpenGPS application screenshot (2.6MB)
  - Complete application window showing aerial/satellite map view
  - Field boundaries and guidance lines visible
  - Vehicle position indicator (green triangle)
  - Top toolbar with various control buttons
  - Bottom status bar with multiple metrics
  - Red progress bar at bottom showing field completion
  - Right side panel with additional controls

### Visual Insights:

**Design Patterns Identified:**
1. **Rotating Display Pattern**: Field statistics cycles through 3 different information screens
   - Screen 1: Application stats (34.42 App, Actual, percentage, ac/hr rate)
   - Screen 2: Field name ("Test Field")
   - Screen 3: Guidance line info ("Line: AB 0°")
   - All screens maintain GPS quality header and large cross-track error display

2. **GPS Quality Header**: Consistent format across all screens
   - Fixed/Float/DGPS status with age value
   - Format: "[Fix Type]: Age: [value]"
   - Always displayed at top of stats panel

3. **Large Cross-Track Error Display**: Prominent numeric display
   - Shows distance from guidance line in cm
   - Large font for at-a-glance reading
   - Appears on all 3 rotating screens

4. **Speed Gauge**: Analog-style speedometer
   - Numeric value displayed separately (e.g., "20.2" or "20.0")
   - Red needle indicator
   - Scale appears to be 0-21 (likely km/h or mph depending on unit setting)

5. **GPS Quality Numeric Indicator**: Small numeric display
   - Shows precision value (e.g., "0.01")
   - Accompanied by horizontal bar indicator
   - Bar color indicates quality (orange/yellow in examples)

6. **Status Icon Bar**: Multiple status indicators
   - GPS signal strength (bar graph icon)
   - Power/charging indicator (lightning bolt)
   - Field map icon
   - Other status icons (folder, close button)

**User Flow Implications:**
1. Field statistics rotate automatically (timing not specified in requirements - TBD)
2. GPS quality information always visible regardless of which stats screen is shown
3. Cross-track error is primary focus metric (largest display element)
4. Speed display is secondary but prominent (gauge + numeric)
5. GPS precision is tertiary but always visible

**UI Components Shown:**
1. Text labels with dynamic values
2. Multi-line status displays
3. Rotating/cycling information panels
4. Numeric displays with units
5. Color-coded section control bars
6. Icon-based status indicators
7. Analog gauge visualization
8. Horizontal bar quality indicators
9. Progress bars (bottom of legacy screenshot)

**Fidelity Level:** High-fidelity screenshots from actual legacy application

**Key Display Requirements Extracted:**

1. **GPS Quality Display:**
   - Format: "[FixType]: Age: [age_value]"
   - Fix types: "RTK fix", "RTK float", "DGPS", etc.
   - Age in decimal format (e.g., "0.0")

2. **Application Rate Statistics:**
   - Format: "[area] App: [app_rate] Actual: [actual_rate] [percentage]% [rate] ac/hr"
   - Example: "34.42 App: 0.00 Actual: 0.00 100.0% 0.0 ac/hr"
   - All values with specific decimal precision

3. **Field Name Display:**
   - Format: "Field: [field_name]"
   - Simple text label with value

4. **Guidance Line Display:**
   - Format: "Line: [line_type] [heading]°"
   - Example: "Line: AB 0°"
   - Line types: "AB", "Curve", "Contour", etc.
   - Heading with degree symbol

5. **Cross-Track Error:**
   - Large prominent display
   - Format: "[value] [unit]"
   - Example: "1.8 cm", "1.9 cm", "0.2 cm"
   - Unit changes based on unit system (cm, in)

6. **Speed Display:**
   - Numeric format with decimal precision
   - Example: "20.2", "20.0"
   - Unit implied by system setting (km/h or mph)

7. **GPS Precision Indicator:**
   - Small numeric display
   - Format: decimal value (e.g., "0.01")
   - Typically in meters

8. **Section Control Visualization:**
   - Color-coded horizontal bars
   - Green, orange, yellow colors visible in screenshots
   - Represents section on/off states

## Requirements Summary

### Functional Requirements

**1. Display Formatter Service (Unified)**

The DisplayFormatterService will provide all formatting functions for UI display:

**Speed Formatting:**
- Input: Speed value (double), unit system (enum)
- Output: Formatted string with dynamic precision
- Logic:
  - If speed > 2 km/h (or equivalent in mph): 1 decimal place
  - If speed ≤ 2 km/h (or equivalent in mph): 2 decimal places
  - No unit suffix in output (unit label shown separately in UI)
- Examples:
  - 20.0 km/h → "20.0"
  - 1.85 km/h → "1.85"
  - 12.5 mph → "12.5"

**Heading Formatting:**
- Input: Heading value (double, 0-360 degrees)
- Output: Formatted string with degree symbol
- Format: "[value]°"
- Examples:
  - 0.0 → "0°"
  - 45.5 → "46°" (rounded to nearest degree)
  - 359.8 → "360°"

**GPS Quality Formatting:**
- Input: Fix type (enum), age (double)
- Output: Formatted string and color
- Format: "[FixType]: Age: [age]"
- Color mapping:
  - RTK Fixed → PaleGreen
  - RTK Float → Orange
  - DGPS → Yellow
  - Others (Autonomous, Invalid, None) → Red
- Examples:
  - FixType.RtkFixed, 0.0 → "RTK fix: Age: 0.0", PaleGreen
  - FixType.RtkFloat, 1.2 → "RTK float: Age: 1.2", Orange

**GPS Precision Formatting:**
- Input: Precision value (double, in meters)
- Output: Formatted string
- Format: 2 decimal places
- Example: 0.012 → "0.01"

**Cross-Track Error Formatting:**
- Input: Error value (double, in meters), unit system (enum)
- Output: Formatted string with unit
- Logic:
  - Convert to cm or inches based on unit system
  - 1 decimal place precision
  - Append unit suffix
- Examples:
  - 0.018 m, Metric → "1.8 cm"
  - 0.019 m, Metric → "1.9 cm"
  - 0.002 m, Metric → "0.2 cm"
  - 0.018 m, Imperial → "0.7 in"

**Distance Formatting:**
- Input: Distance value (double, in meters), unit system (enum)
- Output: Formatted string with unit
- Logic:
  - Metric: Use meters (m) or kilometers (km) based on magnitude
    - < 1000m: Show as meters with 1 decimal
    - ≥ 1000m: Show as kilometers with 2 decimals
  - Imperial: Use feet (ft) or miles (mi) based on magnitude
    - < 5280ft: Show as feet with 0 decimals
    - ≥ 5280ft: Show as miles with 2 decimals
- Examples:
  - 450.5 m, Metric → "450.5 m"
  - 1250.0 m, Metric → "1.25 km"
  - 300.0 m, Imperial → "984 ft"
  - 2000.0 m, Imperial → "1.24 mi"

**Area Formatting:**
- Input: Area value (double, in square meters), unit system (enum)
- Output: Formatted string with unit
- Logic:
  - Metric: Use hectares (ha)
    - 2 decimal places
  - Imperial: Use acres (ac)
    - 2 decimal places
- Examples:
  - 10000 m², Metric → "1.00 ha"
  - 10000 m², Imperial → "2.47 ac"

**Time Formatting:**
- Input: Time value (double, in hours)
- Output: Formatted string with unit
- Format: Decimal hours with 2 decimal places, "hr" suffix
- Examples:
  - 2.25 hours → "2.25 hr"
  - 0.5 hours → "0.50 hr"
  - 10.75 hours → "10.75 hr"

**Application Rate Formatting:**
- Input: Rate value (double), unit system (enum)
- Output: Formatted string with unit
- Format: 2 decimal places, "[value] ac/hr" or "[value] ha/hr"
- Examples:
  - 5.0, Imperial → "5.00 ac/hr"
  - 2.5, Metric → "2.50 ha/hr"

**Percentage Formatting:**
- Input: Percentage value (double, 0-100)
- Output: Formatted string with % symbol
- Format: 1 decimal place, "%" suffix
- Examples:
  - 100.0 → "100.0%"
  - 75.5 → "75.5%"
  - 0.0 → "0.0%"

**Guidance Line Display Formatting:**
- Input: Line type (enum), heading (double)
- Output: Formatted string
- Format: "Line: [LineType] [heading]°"
- Line types: "AB", "Curve", "Contour"
- Examples:
  - LineType.AB, 0.0 → "Line: AB 0°"
  - LineType.Curve, 45.5 → "Line: Curve 46°"

**2. Field Statistics Calculator Service**

Expand existing FieldStatisticsService with rotating display support:

**Rotating Display Data:**
- Provide data for 3 rotating screens:
  - Screen 1: Application statistics (area, app rate, actual rate, percentage, rate per hour)
  - Screen 2: Field name
  - Screen 3: Guidance line info (type and heading)

**Application Statistics:**
- Input: Field data, section control data, time elapsed
- Output: ApplicationStatistics object containing:
  - Total area covered (double, m²)
  - Application rate target (double)
  - Actual application rate (double)
  - Coverage percentage (double, 0-100)
  - Work rate (double, area per hour)

**Field Information:**
- Input: Current field reference
- Output: Field name (string)

**Guidance Line Information:**
- Input: Active guidance line
- Output: Line type (enum) and heading (double)

**3. Unit System Support**

**Enum Definition:**
```csharp
public enum UnitSystem
{
    Metric,
    Imperial
}
```

**Conversions Required:**
- Meters ↔ Feet (1 m = 3.28084 ft)
- Meters ↔ Inches (1 m = 39.3701 in)
- Kilometers ↔ Miles (1 km = 0.621371 mi)
- Square meters ↔ Acres (1 m² = 0.000247105 ac)
- Square meters ↔ Hectares (1 m² = 0.0001 ha)
- Km/h ↔ Mph (1 km/h = 0.621371 mph)

**4. Data Models**

**ApplicationStatistics:**
```csharp
public class ApplicationStatistics
{
    public double TotalAreaCovered { get; set; }      // m²
    public double ApplicationRateTarget { get; set; } // target rate
    public double ActualApplicationRate { get; set; } // actual rate
    public double CoveragePercentage { get; set; }    // 0-100
    public double WorkRate { get; set; }              // area per hour
}
```

**GpsQualityDisplay:**
```csharp
public class GpsQualityDisplay
{
    public string FormattedText { get; set; }
    public string ColorName { get; set; } // "PaleGreen", "Orange", "Yellow", "Red"
}
```

**RotatingDisplayData:**
```csharp
public class RotatingDisplayData
{
    public int CurrentScreen { get; set; } // 1, 2, or 3
    public ApplicationStatistics AppStats { get; set; }
    public string FieldName { get; set; }
    public string GuidanceLineInfo { get; set; }
}
```

### Reusability Opportunities
- Existing FieldStatisticsService in `AgValoniaGPS/AgValoniaGPS.Services/FieldStatistics/FieldStatisticsService.cs`
  - Will be expanded with additional calculation and formatting methods
  - Current basic area/distance calculations will be enhanced
- Unit conversion utilities may exist in legacy codebase
  - Check for existing conversion constants or methods
  - Ensure consistency with legacy values for migration compatibility

### Scope Boundaries

**In Scope:**

1. **DisplayFormatterService:**
   - All formatting methods for speed, heading, GPS quality, distance, area, time, rates, percentages
   - Unit conversion logic
   - Culture-invariant string formatting
   - Color name output for GPS quality

2. **FieldStatisticsService Expansion:**
   - Application statistics calculations
   - Rotating display data provision
   - Field name retrieval
   - Guidance line info retrieval
   - Work rate calculations

3. **Data Models:**
   - ApplicationStatistics
   - GpsQualityDisplay
   - RotatingDisplayData
   - UnitSystem enum

4. **Unit Tests:**
   - Comprehensive tests for all formatter methods
   - Edge case testing (zero values, negative values, very large values)
   - Unit conversion accuracy tests
   - Performance tests (<1ms requirement)
   - Culture-invariant verification

**Out of Scope:**

1. **UI Implementation:**
   - Avalonia view models (separate wave)
   - Avalonia views/controls (separate wave)
   - Data binding setup (separate wave)
   - Rotating display timer/animation logic (separate wave)

2. **Data Acquisition:**
   - GPS data retrieval (already handled by Wave 1)
   - Field data retrieval (already handled by Wave 5)
   - Section control data (already handled by Wave 4)
   - Guidance line data (already handled by Wave 2)

3. **State Management:**
   - Current unit system storage/preferences
   - Current field selection
   - Active guidance line tracking
   - Display rotation state

4. **Advanced Features:**
   - Customizable unit preferences (metric/imperial only for now)
   - Localization/translation of labels
   - User-configurable precision settings
   - Custom color schemes (use specified colors only)
   - Mixed unit systems

### Technical Considerations

**Performance:**
- All formatting operations must complete in <1ms
- No caching required (straightforward calculations)
- Use efficient string formatting methods
- Minimize allocations where possible

**Culture Settings:**
- Use InvariantCulture for all formatting
- Consistent decimal separator (period)
- No thousands separators
- Ensures consistency across all user locales

**Service Architecture:**
- Single DisplayFormatterService with all formatting methods
- FieldStatisticsService expanded from existing implementation
- Both services registered as singletons in DI container
- Services expose interfaces for testability

**Dependency Injection:**
- Register in ServiceCollectionExtensions.cs
- DisplayFormatterService: Singleton, no dependencies initially
- FieldStatisticsService: Singleton, may depend on field/section/guidance services

**Testing Strategy:**
- Unit tests for each formatter method
- Test both metric and imperial outputs
- Test edge cases (0, negative, very large values, NaN, infinity)
- Performance tests to verify <1ms requirement
- Integration tests with real-world data samples from legacy app

**Existing Code Integration:**
- Reference existing FieldStatisticsService implementation
- Maintain compatibility with data models from Waves 1-6
- Use Position from Wave 1
- Use Field data from Wave 5
- Use SectionControl data from Wave 4
- Use GuidanceLine data from Wave 2

**Constants and Conversions:**
- Define all conversion factors as const double
- Use high precision values (e.g., 0.621371 not 0.62)
- Document conversion formulas in code comments
- Ensure consistency with legacy AgOpenGPS values

**Error Handling:**
- Handle null inputs gracefully
- Handle invalid values (NaN, Infinity) → return "---" or similar
- Validate enum values
- Don't throw exceptions from formatters (return safe defaults)

**Code Organization:**
```
AgValoniaGPS.Services/
  Display/
    DisplayFormatterService.cs
    IDisplayFormatterService.cs
  FieldStatistics/
    FieldStatisticsService.cs (existing - to be expanded)
    IFieldStatisticsService.cs (existing - to be expanded)

AgValoniaGPS.Models/
  Display/
    ApplicationStatistics.cs
    GpsQualityDisplay.cs
    RotatingDisplayData.cs
    UnitSystem.cs (enum)

AgValoniaGPS.Services.Tests/
  Display/
    DisplayFormatterServiceTests.cs
  FieldStatistics/
    FieldStatisticsServiceTests.cs (existing - to be expanded)
```

**Naming Conventions:**
- Follow existing AgValoniaGPS patterns
- Service suffix for all services
- Interface prefix "I" for all interfaces
- Descriptive method names (FormatSpeed, FormatHeading, etc.)
- Clear parameter names indicating units (speedKmh, distanceMeters)

**Documentation:**
- XML doc comments for all public methods
- Specify input units in parameter descriptions
- Specify output format in return descriptions
- Include example output in remarks
- Document performance characteristics where relevant
