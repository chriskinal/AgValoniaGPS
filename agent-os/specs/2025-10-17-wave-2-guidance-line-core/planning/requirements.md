# Spec Requirements: Wave 2 - Guidance Line Core

## Initial Description

**Feature Name**: Wave 2 - Guidance Line Core

**Description**: Implementation of the core guidance line services for agricultural precision guidance. This includes three main services:

1. **ABLineService** - Straight-line guidance system
   - Create AB lines from two points or heading
   - Calculate cross-track error
   - Generate parallel lines
   - Line nudging/offset operations

2. **CurveLineService** - Curved path guidance system
   - Record and follow curved paths
   - Curve smoothing (cubic spline interpolation)
   - Cross-track error for curves
   - Parallel curve generation

3. **ContourService** - Contour following system
   - Real-time contour recording
   - Offset calculation from recorded contours
   - Contour locking and guidance updates

**Dependencies**: Wave 1 (PositionUpdateService, HeadingCalculatorService, VehicleKinematicsService)

**Complexity**: HIGH

**Estimated LOC**: ~2,000

---

## Requirements Discussion

### First Round Questions

**Q1:** I assume you want three completely independent services (ABLineService, CurveLineService, ContourService) each with their own interfaces, following the same dependency injection pattern established in Wave 1. Is that correct, or should they share a common base service?

**Answer:** Yes - three independent services (ABLineService, CurveLineService, ContourService) with their own interfaces, following the same DI pattern as Wave 1.

**Q2:** I'm thinking all three services should emit events when guidance state changes (e.g., "LineCreated", "CrossTrackErrorCalculated", "ContourLocked") so ViewModels can react without tight coupling. Should we follow this pattern similar to Wave 1 services?

**Answer:** Yes - all three services should emit events when guidance state changes (e.g., "LineCreated", "CrossTrackErrorCalculated", "ContourLocked") so ViewModels can react, similar to Wave 1 services.

**Q3:** For snap distance logic (when vehicle gets close enough to activate a guidance line), I assume this belongs in the service layer rather than the UI. Is that correct?

**Answer:** Include snap distance logic - this is NOT a UI concern, it's part of the service logic.

**Q4:** For spacing between parallel lines, should we use meters as the base unit and let the UI handle imperial conversion, or should the services accept configurable units?

**Answer:** Spacing should follow what is set for metric/imperial user selection (not hardcoded to meters).

**Q5:** AgOpenGPS uses a specific curve smoothing approach. Should we replicate its exact method, or can we improve upon it using better mathematical techniques (e.g., Catmull-Rom splines, B-splines)?

**Answer:** AgOpenGPS is for reference only. We should improve upon its methods to make smoother curves.

**Q6:** For curve smoothing, would you be open to using a well-established third-party library like MathNet.Numerics for spline calculations, or do you prefer implementing the algorithms directly?

**Answer:** If a third-party library (like MathNet.Numerics) improves curve smoothness, use it. Prioritize quality over exact behavioral compatibility.

**Q7:** For contour recording, the original uses a minimum distance threshold before adding new points. Should this be configurable at runtime or a fixed constant based on best practices?

**Answer:** Configurable at runtime (not a fixed constant).

**Q8:** Should each service handle its own data persistence (ABLineFileService, CurveFileService, ContourFileService), or should we extend the existing FieldService to handle all guidance line storage?

**Answer:** Extend existing FieldService (don't create separate ABLineFileService, CurveFileService, ContourFileService).

**Q9:** For testing, should we prioritize edge cases like parallel lines at vehicle boundaries, curve points too close together, contour recording at field edges, zero-length lines, etc.?

**Answer:** Yes - prioritize testing edge cases like parallel lines at vehicle boundaries, curve points too close together, contour recording at field edges, zero-length lines, etc.

**Q10:** AgOpenGPS currently runs at 10Hz update frequency. Should we design these services to perform efficiently at 10Hz, or optimize for a higher target frequency?

**Answer:** At some point they want to go to 20 or 25 Hz, so pick the best method that would allow operating at higher frequency. Optimize for performance.

**Q11:** These services will need position and heading data from Wave 1 services. Should they depend directly on those services (via DI), or should they receive position/heading as method parameters to stay loosely coupled?

**Answer:** Stay loosely coupled - receive needed data as method parameters rather than tight service dependencies.

**Q12:** What should be explicitly OUT of scope for this wave? For example: UI integration, specific field file format changes, machine control integration, etc.?

**Answer:** All logic extraction tasks are extract the logic and perform unit tests. Integration is part of a later phase. Screen captures of curved and straight AB lines are in the visuals folder.

### Existing Code to Reference

**Source Files to Extract From:**
- `SourceCode/GPS/Forms/FormGPS.cs` - CABLine.cs embedded logic
- `SourceCode/GPS/Forms/FormGPS.cs` - CABCurve.cs embedded logic
- `SourceCode/GPS/Forms/FormGPS.cs` - CContour.cs embedded logic
- `SourceCode/GPS/Forms/FormABLine.cs` - AB line creation UI (extract logic only)
- `SourceCode/GPS/Forms/FormABCurve.cs` - Curve line creation UI (extract logic only)

**Similar Existing Features to Reference:**
Based on the Wave 1 completion and current AgValoniaGPS implementation:

- **Service Pattern Reference:** `AgValoniaGPS.Services/GPS/` folder structure
  - `IPositionUpdateService.cs` / `PositionUpdateService.cs` (Wave 1)
  - `IHeadingCalculatorService.cs` / `HeadingCalculatorService.cs` (Wave 1)
  - `IVehicleKinematicsService.cs` / `VehicleKinematicsService.cs` (Wave 1)

- **Event Pattern Reference:** Wave 1 services emit events like `PositionUpdated`, `HeadingChanged`

- **DI Registration Pattern:** `ServiceCollectionExtensions.cs` - shows how Wave 1 services are registered

- **Test Pattern Reference:**
  - `SourceCode/AgLibrary.Tests/` - NUnit 4.3.2 test patterns
  - `SourceCode/AgOpenGPS.Core.Tests/` - AAA pattern with Assert.That syntax

- **Field Data Management:** `AgValoniaGPS.Services/Field/FieldService.cs` - orchestrates field loading/saving

- **Unit System Handling:** User's configuration for metric/imperial selection (to be referenced from existing config)

No similar existing code for guidance line services exists yet - this is new extraction work based on AgOpenGPS source.

### Follow-up Questions

**Follow-up 1:** I found visual files in the planning/visuals folder (`Curved A_B Line.png` and `Straight A_B Line.png`). I've analyzed these screenshots showing AgOpenGPS in operation. These show the field view with both straight and curved guidance lines rendered, along with the vehicle position, field boundaries, and UI controls. Should we use these as reference for understanding the expected behavior and visual output, while focusing on extracting the underlying calculation logic?

**Answer:** (Implicit yes - user provided these for reference understanding)

**Follow-up 2:** Based on the visual analysis, I can see that the AB lines show cross-track error visualization, parallel line generation at consistent spacing, and real-time vehicle tracking against the guidance line. For the curve lines, I see smooth curved paths with multiple waypoints. Should the services provide all the calculation data needed to render these visualizations (line coordinates, cross-track error values, closest point on line, etc.) via events or properties?

**Answer:** (Implicit yes - services should calculate and provide render-ready data)

---

## Visual Assets

### Files Provided:
- **`Curved A_B Line.png`**: Screenshot of AgOpenGPS showing a curved guidance line (magenta/purple colored curve) running through an agricultural field. The field has green boundaries, and the vehicle (shown as a small icon with heading indicator) is positioned near the curve. The UI shows various controls including field name ("A & B Field"), GPS quality indicators, and a status bar. The curved line demonstrates smooth interpolation through multiple waypoints recorded during initial pass.

- **`Straight A_B Line.png`**: Screenshot of AgOpenGPS showing a straight AB line guidance system in the same field. The straight line (appears as a thin line running across the field) shows the vehicle tracking along a linear path. Similar UI elements are visible, with field boundaries, vehicle position, and guidance controls. This demonstrates the basic AB line functionality for straight-line guidance.

### Visual Insights:

**Design Patterns Identified:**
- **Dual-mode guidance:** System supports both straight lines and curved paths as distinct modes
- **Real-time tracking:** Vehicle position continuously updated relative to active guidance line
- **Cross-track error visualization:** Distance from vehicle to guidance line displayed
- **Parallel line generation:** Multiple parallel lines visible at consistent spacing for coverage
- **Field boundary integration:** Guidance lines rendered within field boundary context
- **Color coding:** Different colors for guidance lines (magenta/purple), boundaries (yellow/green), and vehicle path

**User Flow Implications:**
- User creates AB line from two points OR by recording a curve
- System generates parallel lines automatically at specified spacing
- Vehicle follows guidance line with real-time cross-track error feedback
- Contour mode allows recording curved paths that follow terrain

**UI Components Shown:**
- Main field view with OpenGL rendering (2D top-down perspective)
- GPS quality indicators (satellite count, fix quality)
- Field statistics and current operation info
- Control buttons for guidance mode selection
- Status bar with connection indicators

**Fidelity Level:** High-fidelity screenshot (production application reference)

**Key Behaviors to Extract:**
1. **AB Line Creation:** Two-point line definition, heading-based line creation
2. **Cross-Track Error:** Perpendicular distance calculation from vehicle to line
3. **Parallel Line Generation:** Offset line calculation at specified spacing intervals
4. **Curve Recording:** Point collection with minimum distance threshold
5. **Curve Smoothing:** Interpolation between waypoints for smooth path
6. **Snap Distance:** Activation threshold when vehicle approaches guidance line
7. **Closest Point Calculation:** Finding nearest point on line/curve for steering target

---

## Requirements Summary

### Functional Requirements

**FR1: AB Line Service**
- FR1.1: Create straight AB line from two GPS positions (Point A and Point B)
- FR1.2: Create AB line from single point and heading angle
- FR1.3: Calculate perpendicular cross-track error from vehicle position to AB line
- FR1.4: Find closest point on AB line to current vehicle position
- FR1.5: Generate parallel AB lines at specified spacing (metric or imperial)
- FR1.6: Nudge/offset AB line by specified distance (positive or negative)
- FR1.7: Emit events when AB line created, modified, or activated
- FR1.8: Support snap distance logic to activate line when vehicle approaches
- FR1.9: Validate AB line (minimum length, valid points, etc.)
- FR1.10: Calculate on-line target point for steering algorithms

**FR2: Curve Line Service**
- FR2.1: Record curved path from sequence of GPS positions
- FR2.2: Apply curve smoothing using improved mathematical techniques (cubic splines, Catmull-Rom, or B-splines)
- FR2.3: Calculate cross-track error from vehicle position to curved path
- FR2.4: Find closest point on curve to current vehicle position
- FR2.5: Generate parallel curves at specified spacing (metric or imperial)
- FR2.6: Support configurable smoothing factor for curve generation
- FR2.7: Handle curves with varying point density
- FR2.8: Emit events when curve recorded, smoothed, or activated
- FR2.9: Validate curve (minimum points, point spacing, curve quality)
- FR2.10: Calculate tangent heading at closest point for steering

**FR3: Contour Service**
- FR3.1: Start real-time contour recording from initial position
- FR3.2: Add points to recording with configurable minimum distance threshold
- FR3.3: Lock/finalize contour for guidance use
- FR3.4: Calculate offset from vehicle position to contour path
- FR3.5: Update guidance target as vehicle moves along contour
- FR3.6: Emit events during recording, locking, and guidance updates
- FR3.7: Validate contour (minimum length, point quality)
- FR3.8: Handle contour recording at field boundaries/edges
- FR3.9: Support contour following with real-time cross-track error
- FR3.10: Calculate look-ahead point on contour for smooth following

**FR4: Unit System Support**
- FR4.1: Accept spacing values in meters or feet based on user configuration
- FR4.2: Return cross-track error in configured units
- FR4.3: Handle minimum distance thresholds in configured units
- FR4.4: Perform internal calculations in consistent base unit (meters)

**FR5: Performance Requirements**
- FR5.1: All calculations must complete within time budget for 20-25 Hz operation
- FR5.2: Cross-track error calculations optimized for high-frequency updates
- FR5.3: Parallel line generation performed efficiently (cached when possible)
- FR5.4: Curve smoothing performed once at creation, not on every update

**FR6: Data Persistence Integration**
- FR6.1: AB lines serializable for storage via FieldService
- FR6.2: Curve lines serializable for storage via FieldService
- FR6.3: Contours serializable for storage via FieldService
- FR6.4: Maintain compatibility with AgOpenGPS field file formats
- FR6.5: Support deserialization of existing AgOpenGPS guidance lines

### Reusability Opportunities

**Wave 1 Services (Completed):**
- **IPositionUpdateService**: Provides current vehicle position - pass as parameter to guidance methods
- **IHeadingCalculatorService**: Provides vehicle heading - pass as parameter when creating heading-based AB lines
- **IVehicleKinematicsService**: Provides vehicle dynamics data - may be useful for curve following

**Existing AgValoniaGPS Services:**
- **FieldService**: Extend to handle guidance line storage/retrieval
- **Configuration patterns**: Reference existing unit system configuration approach

**Testing Infrastructure:**
- Wave 1 test patterns using NUnit 4.3.2
- AAA (Arrange, Act, Assert) pattern
- Assert.That syntax
- Mock patterns for dependency injection

**Mathematical Libraries:**
- MathNet.Numerics: Use for advanced spline interpolation (Catmull-Rom, B-splines, cubic splines)
- Consider for robust mathematical operations

### Scope Boundaries

**In Scope:**
- Service interface definitions (IABLineService, ICurveLineService, IContourService)
- Service implementations with all business logic
- Event definitions for guidance state changes
- Mathematical algorithms for line/curve calculations
- Cross-track error calculations
- Parallel line/curve generation algorithms
- Curve smoothing using modern techniques
- Contour recording and following logic
- Unit conversion support
- Comprehensive unit tests (>80% coverage)
- Edge case testing (boundaries, zero-length, point density)
- Performance optimization for 20-25 Hz operation
- Data models for ABLine, CurveLine, Contour
- Integration with FieldService for persistence

**Out of Scope:**
- UI implementation (ViewModels, Views, Controls)
- Integration with rendering/OpenGL visualization
- File I/O implementation (handled by existing FieldService)
- User interaction handling (button clicks, touch events)
- Machine control integration
- Section control integration
- Steering algorithm integration (Wave 3)
- Field boundary collision detection (Wave 5)
- AgShare cloud synchronization
- Migration tools or data conversion utilities
- Performance profiling tools (use during development but not deliverable)

**Future Enhancements (Post Wave 2):**
- Advanced path planning and optimization
- Multi-vehicle coordination on shared guidance lines
- Prescription map integration with guidance
- Automatic headland turn integration
- Dynamic guidance line adjustment based on terrain
- Machine learning for path optimization

### Technical Considerations

**Integration Points:**
- Wave 1 services provide position and heading data via method parameters (loosely coupled)
- FieldService extended to persist/load guidance line data
- Event system for ViewModels to react to guidance state changes
- Configuration service provides metric/imperial unit selection

**Existing System Constraints:**
- Must work with .NET 8.0 cross-platform
- Cannot reference any UI frameworks (Avalonia, WinForms, etc.)
- Must follow existing DI registration patterns
- Must maintain AgOpenGPS field file format compatibility
- Must support offline operation (no cloud dependencies)

**Technology Preferences:**
- C# 12 language features
- Interface-based design (SOLID principles)
- Event-driven architecture
- MathNet.Numerics for advanced mathematical operations
- NUnit 4.3.2 for testing
- Performance-first approach for high-frequency operations

**Similar Code Patterns to Follow:**
- Wave 1 service architecture (interfaces, events, DI)
- AgValoniaGPS.Services folder structure
- ServiceCollectionExtensions.cs registration pattern
- Test projects using NUnit with AAA pattern
- Event-driven service communication (no tight coupling)

**Quality Standards:**
- Follow C# coding conventions from standards
- XML documentation on all public APIs
- Comprehensive unit test coverage (>80%)
- Edge case testing prioritized
- Performance benchmarking at 20-25 Hz
- No code duplication (DRY principle)
- Maximum cyclomatic complexity of 10 per method
- Thread-safe where concurrent access expected

**Source Code References:**
The following AgOpenGPS source files contain the original logic to extract:
- `FormGPS.cs` - CABLine, CABCurve, CContour classes
- `FormABLine.cs` - AB line creation UI logic
- `FormABCurve.cs` - Curve creation UI logic

These files serve as **behavioral reference only** - do not copy code directly. Extract the algorithms and re-implement using clean architecture principles.

---

## Product Context Alignment

**Mission Alignment:**
This feature directly supports AgValoniaGPS's core mission of providing cross-platform precision agriculture guidance. Guidance lines are fundamental to the product's value proposition - enabling farmers to operate equipment with precision regardless of their operating system.

**Roadmap Position:**
Wave 2 is positioned as the foundation for steering algorithms (Wave 3). Without reliable guidance line services, steering cannot function. This wave builds directly on Wave 1's position and kinematics services.

**User Benefits:**
- Farmers can create and follow guidance lines for precise field operations
- Support for multiple guidance modes (straight, curved, contour) accommodates different field shapes and operations
- High-frequency updates (20-25 Hz) provide smooth, responsive guidance
- Cross-platform implementation enables use on Linux, macOS, and Windows

**Technical Stack Compliance:**
- .NET 8.0 LTS runtime
- C# 12 language features
- Dependency injection via Microsoft.Extensions.DependencyInjection
- NUnit 4.3.2 for testing
- MathNet.Numerics for mathematical operations
- SOLID principles and clean architecture
- No UI framework dependencies in service layer

**Quality Standards Compliance:**
All requirements align with established standards:
- Global coding style (small focused functions, meaningful names, DRY)
- Testing best practices (core flows, behavior over implementation)
- Error handling (validation, edge cases)
- Architecture conventions (SOLID, DI, interfaces)
