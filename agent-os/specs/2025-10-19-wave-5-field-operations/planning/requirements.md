# Spec Requirements: Wave 5 - Field Operations

## Initial Description
Wave 5 focuses on field boundary management, headland processing, and automated turn generation for the AgValoniaGPS precision agriculture application.

**Context:**
This is Wave 5 of 8 in the AgValoniaGPS business logic extraction from AgOpenGPS. Previous waves completed:
- Wave 1: Position & Kinematics Services ✅
- Wave 2: Guidance Line Core (ABLine, Curve, Contour) ✅
- Wave 3: Steering Algorithms (Pure Pursuit, Stanley, Look-ahead) ✅
- Wave 4: Section Control ✅

**Wave 5 Scope:**
According to the feature extraction roadmap (agent-os/product/feature-extraction-roadmap.md), Wave 5 should include:

1. **Boundary Management Service** - Creating, editing, and validating field boundaries
   - Boundary recording from GPS positions
   - Point-in-polygon detection
   - Distance to boundary calculations
   - Area calculations (Shoelace formula)
   - Boundary simplification (Douglas-Peucker)

2. **Headland Service** - Headland creation and management
   - Generate headlands from field boundaries
   - Multi-pass headland support
   - Headland width configuration
   - Entry/exit point calculation

3. **U-Turn Service** - Automated turn pattern generation
   - Multiple turn styles (Question Mark, Semi-Circle, Keyhole)
   - Dubins path algorithm for minimum radius paths
   - Turn trigger detection
   - Path following during turns

4. **Tram Lines Service** - Tram line generation and management
   - Tram line pattern generation based on AB lines
   - Spacing configuration (track width, seed width)
   - Multi-pass tram line support
   - Visual display and management

## Requirements Discussion

### First Round Questions

**Q1: Boundary Recording - Should we support both time-based and distance-based recording modes for boundary capture?**
**Answer:** Support **both** time-based and distance-based recording modes

**Q2: Simplification - Should boundary simplification be automatic during recording or manual after completion?**
**Answer:** Allow user to configure **auto or manual** simplification mode

**Q3: Point-in-Polygon - Should we use a single reusable service for point-in-polygon detection that can be used by boundaries, headlands, and coverage tracking?**
**Answer:** Use a **single reusable service** for point-in-polygon (code maintainability)

**Q4: Validation Rules - Are the following validation rules sufficient?**
- Minimum 3 points for a valid boundary
- Maximum self-intersection tolerance (warn but allow)
- Minimum area threshold (e.g., 0.1 acres)
- Maximum point spacing (prevent GPS dropout gaps)

**Answer:** Validation rules listed are sufficient ✓

**Q5: File Formats - Should we support both AgOpenGPS .txt format AND modern formats (GeoJSON/KML) for boundary import/export?**
**Answer:** Support **both** AgOpenGPS .txt format and GeoJSON/KML

**Q6: Headland Storage - Should headlands be stored as a single multi-polygon structure or as separate polygons for each pass?**
**Answer:** Store as **single multi-polygon structure**

**Q7: Headland Overlap - When generating multiple headland passes, should the system prevent overlap or allow slight overlap for better coverage?**
**Answer:** Headland overlap handling: **user-selectable setting**

**Q8: Entry/Exit Points - Should entry/exit point calculation be purely automatic or allow manual nudging by the user?**
**Answer:** Allow **nudging of entry/exit points** (not purely automatic)

**Q9: Headland Progress - Should the system track which headland sections have been completed (painted) or is that handled purely by coverage tracking?**
**Answer:** **Track completed sections** (not purely geometric)

**Q10: Turn Patterns - Are the three turn styles (Question Mark, Semi-Circle, Keyhole) sufficient, or should we also include T-turn and Y-turn patterns?**
**Answer:** Include **T-turn and Y-turn** patterns (in addition to Question Mark, Semi-Circle, Keyhole)

**Q11: Turn Radius Override - Should the automatically calculated turn radius be user-overridable for specific situations?**
**Answer:** Allow **user override** of automatically calculated turn radius

**Q12: Turn Trigger - Should turn triggers be based purely on distance to boundary or also consider lookahead distance and vehicle configuration?**
**Answer:** Turn trigger: **calculated with user-configurable override**

**Q13: Section Control During Turns - Should sections automatically pause during turns or remain active throughout?**
**Answer:** Section control: **automatically pause** during turns

**Q14: Integration with Steering - Should U-Turn service integrate directly with SteeringCoordinatorService from Wave 3?**
**Answer:** **Integrate with SteeringCoordinatorService** from Wave 3

**Q15: Event Pattern - Should these services follow the same EventArgs pattern used in previous waves (e.g., BoundaryUpdatedEventArgs)?**
**Answer:** Use **EventArgs pattern** from previous waves ✓

**Q16: Area Calculation Timing - Should area calculations update in real-time during boundary recording or only after completion?**
**Answer:** Area calculations: **real-time update** during recording

**Q17: Spatial Indexing - For large fields with complex boundaries, should we implement spatial indexing (R-tree) for performance?**
**Answer:** Use **spatial indexing** (R-tree) for performance

**Q18: Configuration Storage - Should boundary/headland settings be stored in Field.txt or in a separate configuration file?**
**Answer:** Allow user to configure using **one or both** storage methods

**Q19: Turn Pattern Storage - Should turn patterns be pre-calculated and stored or generated on-demand when needed?**
**Answer:** Turn patterns: **generate as needed** (not pre-calculated)

**Q20: Edge Cases - Which edge cases are most important to test?**
- GPS signal loss during boundary recording
- Boundaries with holes (inner boundaries)
- Multi-part fields (non-contiguous)
- Very irregular/complex boundary shapes

**Answer:** **All edge cases important** (GPS loss, holes, multi-part fields, irregular shapes)

**Q21: Performance Benchmarks - Should we establish performance benchmarks (e.g., <5ms for point-in-polygon check)?**
**Answer:** **Yes, add performance benchmarks** (<5ms target)

**Q22: Out of Scope - Is anything explicitly OUT of scope for Wave 5?**
**Answer:** **Nothing out of scope** - implement all features

**Q23: Similar Features - Are there existing features in your codebase with similar patterns we should reference?**

**Answer:**
- **A. No geometric calculation examples available** (first implementation)
- **B. AgValoniaGPS POC has file I/O examples to reference**
  - Location: `AgValoniaGPS/AgValoniaGPS.Services/` folder
  - Examples: FieldPlaneFileService, BoundaryFileService, BackgroundImageFileService
- **C. Event patterns exist in AgValoniaGPS code folder**
  - Location: `AgValoniaGPS/AgValoniaGPS.Models/Events/`
  - Examples: CoverageMapUpdatedEventArgs, SectionStateChangedEventArgs, SteeringUpdateEventArgs
- **D. Yes, integrate with PositionUpdateService**
  - Location: `AgValoniaGPS/AgValoniaGPS.Services/GPS/PositionUpdateService.cs`

### Follow-up Questions

**Follow-up 1: Tram Lines Scope - I notice from the roadmap that Wave 5 includes Boundary, Headland, and U-Turn services. The visual assets show a Tram Lines feature. Should Tram Lines be included in Wave 5 or is it planned for a different wave?**

**Answer:** Checked planning documents per user instruction. Tram Lines feature is NOT mentioned in any other wave specifications (Waves 6, 7, or 8 per feature-extraction-roadmap.md). Therefore, **Tram Lines should be included in Wave 5 scope**.

**Follow-up 2: Directory Structure - Should we organize services in subdirectories by feature area (Boundary/, Headland/, Turn/, TramLines/) or keep them flat in a single FieldOperations/ folder?**

**Answer:** Use **flat structure**. No subdirectories with single files - all services directly in:
```
AgValoniaGPS.Services/
  └── FieldOperations/
      ├── BoundaryManagementService.cs
      ├── HeadlandService.cs
      ├── UTurnService.cs
      ├── TramLineService.cs
      └── PointInPolygonService.cs
```

**Follow-up 3: UI Scope - The visual assets show complex UI dialogs. Should Wave 5 include ViewModels and XAML views, or focus only on backend services?**

**Answer:** **Services only**. Wave 5 scope is:
- Backend business logic services
- Service interfaces
- Domain models
- Events/EventArgs
- NO ViewModels
- NO UI Views
- NO XAML

The visual assets serve as reference material showing what UI features the backend needs to support. UI and frontend-backend wiring is a separate future phase.

## Existing Code to Reference

**Similar Features Identified:**

Based on user guidance, reference the following existing code patterns:

**File I/O Patterns:**
- Location: `AgValoniaGPS/AgValoniaGPS.Services/` folder
- Services to model after:
  - FieldPlaneFileService (for Field.txt format)
  - BoundaryFileService (for Boundary.txt format)
  - BackgroundImageFileService (for file handling patterns)

**Event Patterns:**
- Location: `AgValoniaGPS/AgValoniaGPS.Models/Events/` folder
- EventArgs to follow:
  - CoverageMapUpdatedEventArgs
  - SectionStateChangedEventArgs
  - SteeringUpdateEventArgs

**Position Integration:**
- Service: PositionUpdateService
- Location: `AgValoniaGPS/AgValoniaGPS.Services/GPS/PositionUpdateService.cs`
- Use for: Real-time position updates during boundary recording

**Dependency Injection:**
- Location: `AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
- Pattern: Register all new services following existing patterns

**Note:** No existing geometric calculation services available - this is the first implementation of complex geometric algorithms in the AgValoniaGPS codebase.

## Visual Assets

### Files Provided:
The user provided 5 screenshots showing the boundary and headland management UI from AgOpenGPS:

- **Boundry Tools 1.png**: Main field view showing boundary tools menu with options including:
  - Boundary, Headland, Headland Builder
  - Tram Lines, Tram Lines Builder
  - Delete Applied Area
  - Flag By Lat Lon
  - Recorded Path
  - Shows field with yellow boundary outline and satellite imagery background

- **Boundry Tools 2.png**: Boundary management dialog showing:
  - "Start or Delete A Boundary" interface
  - Three sections: Boundary (Outer), Area (34.42248 Ac), Drive Thru (--)
  - Action buttons with icons for trash, boundary, area, drive-through, and confirm
  - Shows boundary creation workflow

- **Boundry Tools 3.png**: Headland creation/editing interface showing:
  - "Create and Edit Headland" dialog
  - Large preview area with white outer boundary and yellow inner headland
  - Right panel with controls:
    - B++ / B-- (Boundary adjustment)
    - A++ / A-- (Area adjustment)
    - Smooth path and straight path icons
    - Offset value: 0.0 (ft)
    - Tool width: 6.0 ft
    - "Build Around" and "Reset" buttons
    - "Clip Line" button
    - Zoom controls
    - Power and confirm buttons

- **Boundry Tools 4.png**: Alternative headland editing view showing:
  - Similar interface with slightly different button states
  - "Build" button (highlighted with star effect)
  - Navigation arrows (previous/next)
  - Delete button added
  - Same layout and control structure

- **Boundry Tools 5.png**: Tram lines interface showing:
  - Field view with boundary and red vertical guidance line
  - "Tram Lines" header with track/tram statistics
  - Right panel controls:
    - Alpha transparency: 80
    - Swap A/B icon
    - Delete button
    - Settings button
    - Undo button
    - Navigation: 1/1 with prev/next arrows
    - Start offset: 0
    - Pass count: 2
    - Add button
    - Power and save buttons

### Visual Insights:

**Design Patterns Identified:**
1. **Modal Dialog Pattern**: Full-screen modal dialogs for complex operations (boundary/headland creation)
2. **Icon-Based Actions**: Heavy use of icon buttons for common actions (delete, confirm, reset)
3. **Preview + Controls Layout**: Large preview area on left, vertical control panel on right
4. **Incremental Adjustment**: +/- buttons for numeric adjustments (B++/B--, A++/A--)
5. **Visual Feedback**: Color-coded elements (yellow for headland, white for boundary, red for guidance)
6. **Step-by-Step Workflow**: Sequential dialogs guide user through boundary → headland → tram lines
7. **Real-time Preview**: Shows result of operations before confirming

**User Flow Implications:**
1. **Boundary First**: Must create boundary before headland
2. **Manual Refinement**: Users can adjust boundaries and headlands after initial creation
3. **Tool-Based Approach**: Different tools for different operations (not a single unified editor)
4. **Immediate Visual Feedback**: Changes are shown immediately in preview area
5. **Multi-Pass Support**: Can create multiple headland passes with navigation controls
6. **Non-Destructive Editing**: Reset and undo capabilities preserve original data

**UI Components Shown:**
- Large canvas/viewport for field visualization
- Icon buttons (trash, confirm, settings, zoom, navigation)
- Numeric input fields with labels
- +/- increment/decrement buttons
- Preview thumbnails (smooth vs straight path options)
- Action buttons at bottom (power, delete, confirm)
- Status bar showing measurements (track, tram, seed, AB values)

**Fidelity Level:**
High-fidelity screenshots from the actual AgOpenGPS application. These show the complete implemented UI, not wireframes or mockups.

**Backend Support Required:**
The visuals indicate the backend services must support:
- Real-time boundary recording with live area updates
- Multi-pass headland generation with adjustable offsets
- Tram line pattern generation with configurable spacing
- Smooth vs straight path algorithms for headlands
- Navigation between multiple boundary/headland/tram line sets
- State management for recording, editing, and finalizing operations
- Undo/redo capability for non-destructive editing

## Requirements Summary

### Functional Requirements

#### 1. Boundary Management Service
- **Recording Modes:**
  - Time-based recording (capture point every X seconds)
  - Distance-based recording (capture point every X meters)
  - Both modes user-configurable
- **Recording Features:**
  - Real-time area calculation during recording (using Shoelace formula)
  - Real-time display of current position and perimeter length
  - Start/stop/pause recording controls
  - GPS signal loss handling with visual indicators
- **Simplification:**
  - Douglas-Peucker algorithm implementation
  - User-configurable: auto-simplify during recording OR manual after completion
  - Configurable tolerance settings
- **Validation:**
  - Minimum 3 points required
  - Maximum self-intersection tolerance (warn but allow)
  - Minimum area threshold (e.g., 0.1 acres)
  - Maximum point spacing validation (detect GPS dropout gaps)
- **Point-in-Polygon:**
  - Single reusable service for all point-in-polygon checks
  - Used by boundaries, headlands, coverage tracking
  - Ray-casting algorithm implementation
  - Performance target: <5ms per check
- **Distance Calculations:**
  - Distance from point to nearest boundary edge
  - Distance from point to specific boundary segment
  - Perpendicular distance calculations for headland generation
- **File Format Support:**
  - Read/write AgOpenGPS Boundary.txt format (backward compatibility)
  - Import/export GeoJSON format
  - Import/export KML format
  - Coordinate system conversion (WGS84 ↔ UTM)
- **Data Management:**
  - Support for multiple boundaries per field
  - Inner boundaries (holes) support
  - Multi-part fields (non-contiguous polygons)
  - Boundary metadata (name, creation date, area, perimeter)

#### 2. Headland Service
- **Generation:**
  - Generate headlands from field boundary using offset algorithm
  - Multi-pass headland support (generate multiple parallel passes)
  - User-configurable headland width per pass
  - Smooth corner handling (avoid sharp angles)
- **Storage:**
  - Store as single multi-polygon structure
  - Track individual pass geometry separately for visualization
  - Store headland configuration (width, number of passes)
- **Overlap Handling:**
  - User-selectable setting: prevent overlap OR allow slight overlap
  - Configurable overlap tolerance when allowed
- **Entry/Exit Points:**
  - Automatic calculation of optimal entry/exit points
  - Allow manual nudging/adjustment by user
  - Entry/exit point validation (ensure accessibility)
  - Store entry/exit preferences per field
- **Progress Tracking:**
  - Track which headland sections have been completed
  - Integration with coverage tracking system
  - Visual indication of completed vs remaining headland sections
  - Percentage completion calculation
- **Corner Handling:**
  - Rounded corners vs sharp corners options
  - Corner radius configuration
  - Smooth path generation for corners
- **File Format:**
  - Read/write AgOpenGPS Headland.Txt format
  - Include headland data in GeoJSON/KML exports
  - Store configuration in Field.txt or separate config file (user choice)

#### 3. U-Turn Service
- **Turn Patterns:**
  - Question Mark turn
  - Semi-Circle turn
  - Keyhole turn
  - T-turn pattern
  - Y-turn pattern
  - User-selectable turn pattern preference
- **Dubins Path Algorithm:**
  - Minimum radius path calculation
  - Path smoothness optimization
  - Integration with vehicle configuration (wheelbase, minimum turn radius)
- **Turn Radius:**
  - Automatic calculation based on vehicle configuration
  - User override capability for specific situations
  - Safety validation (warn if radius too tight)
- **Turn Trigger Detection:**
  - Calculate trigger point based on distance to boundary
  - Factor in lookahead distance from guidance system
  - Consider vehicle configuration (speed, wheelbase)
  - User-configurable override for trigger distance
  - Early trigger warning system
- **Path Following:**
  - Generate smooth path through turn
  - Integration with SteeringCoordinatorService from Wave 3
  - Real-time path tracking during turn execution
  - Cross-track error monitoring during turns
- **Section Control Integration:**
  - Automatically pause sections during turn initiation
  - Resume sections after turn completion
  - Configurable section pause timing (early vs late)
  - Integration with Wave 4 Section Control services
- **Turn Pattern Generation:**
  - Generate patterns on-demand (not pre-calculated)
  - Cache recently used patterns for performance
  - Validate turn fits within available space
  - Adjust pattern if space constraints detected
- **Turn State Management:**
  - Track turn state (idle, approaching, executing, completing)
  - Event-driven state changes
  - Integration with guidance loop
- **Configuration:**
  - Store turn pattern preferences per field
  - Global default turn settings
  - Per-operation turn overrides

#### 4. Tram Line Service
- **Pattern Generation:**
  - Generate tram lines based on active AB line
  - Configurable spacing (track width, seed width, custom)
  - Multi-pass tram line support (navigation between multiple patterns)
  - Offset from AB line origin (start offset)
- **Configuration:**
  - Track width (spacing between tram lines)
  - Seed width (for seed drill tram patterns)
  - Pass count (number of times to repeat pattern)
  - Start offset (distance from AB line origin to first tram)
  - Pattern swap (swap A/B sides)
- **Visual Management:**
  - Alpha transparency control for visual display
  - Color coding for tram lines
  - Navigation controls (previous/next pattern)
  - Pattern deletion and undo capability
- **Storage:**
  - Save tram line patterns to field file
  - Load existing tram line patterns
  - Store configuration preferences per field
- **Integration:**
  - Use AB line from Wave 2 guidance services
  - Generate parallel lines using ABLineService methods
  - Coordinate with coverage mapping for completed passes

### Reusability Opportunities

**Existing Services to Integrate With:**
1. **PositionUpdateService** (Wave 1)
   - Location: `AgValoniaGPS/AgValoniaGPS.Services/GPS/PositionUpdateService.cs`
   - Use for: Real-time position updates during boundary recording

2. **ABLineService** (Wave 2)
   - Location: `AgValoniaGPS/AgValoniaGPS.Services/Guidance/ABLineService.cs`
   - Use for: Tram line generation based on AB lines

3. **SteeringCoordinatorService** (Wave 3)
   - Location: `AgValoniaGPS/AgValoniaGPS.Services/Guidance/SteeringCoordinatorService.cs`
   - Use for: U-turn path following and steering integration

4. **Section Control Services** (Wave 4)
   - Location: `AgValoniaGPS/AgValoniaGPS.Services/Section/`
   - Use for: Automatic section pause/resume during turns

**Existing File I/O Patterns:**
- Reference: `AgValoniaGPS/AgValoniaGPS.Services/` folder
- Examples to model after:
  - FieldPlaneFileService (for Field.txt format)
  - BoundaryFileService (for Boundary.txt format)
  - BackgroundImageFileService (for file handling patterns)

**Event Patterns:**
- Reference: `AgValoniaGPS/AgValoniaGPS.Models/Events/` folder
- Examples to follow:
  - CoverageMapUpdatedEventArgs
  - SectionStateChangedEventArgs
  - SteeringUpdateEventArgs

**Service Registration:**
- Location: `AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
- Register all new services using existing DI patterns

### Scope Boundaries

**In Scope (Wave 5 - Backend Services Only):**
- Boundary recording from GPS positions (time-based and distance-based)
- Real-time boundary validation and area calculation
- Boundary simplification using Douglas-Peucker algorithm
- Point-in-polygon detection service (reusable across features)
- Distance to boundary calculations
- Headland generation from boundaries with multi-pass support
- Headland entry/exit point calculation and manual adjustment
- Headland progress tracking
- Five turn patterns: Question Mark, Semi-Circle, Keyhole, T-turn, Y-turn
- Dubins path algorithm for smooth turns
- Turn trigger detection with user override
- Automatic section control during turns
- Tram line pattern generation from AB lines
- Tram line spacing and configuration management
- Integration with Wave 3 steering and Wave 4 section control
- File format support: AgOpenGPS .txt, GeoJSON, KML
- Spatial indexing (R-tree) for performance
- Comprehensive testing of all edge cases
- Performance benchmarks (<5ms targets)
- Service interfaces
- Domain models
- Events and EventArgs

**Out of Scope (Wave 5):**
- ViewModels (future UI phase)
- XAML Views (future UI phase)
- Frontend-backend wiring (future UI phase)
- User interface dialogs (reference only)

**Future Enhancements (Not Wave 5):**
- AI-based boundary optimization
- Cloud-based field sharing (AgShare integration - planned for later phases)
- Automatic boundary detection from satellite imagery
- 3D terrain-aware boundary and headland generation
- Multi-vehicle coordination for boundary recording
- Real-time collaborative boundary editing

### Technical Considerations

**Service Organization (FINALIZED):**
Flat structure in FieldOperations/ directory:
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

**Performance Requirements:**
- Point-in-polygon check: <5ms per check
- Boundary simplification: <10ms for typical field
- Headland generation: <50ms for typical field
- Turn pattern calculation: <5ms per pattern
- Tram line generation: <10ms for typical pattern
- Area calculation: Real-time during recording (no noticeable lag)
- Spatial indexing: R-tree implementation for large/complex boundaries

**Integration Points:**
- PositionUpdateService (Wave 1) - GPS position updates
- VehicleKinematicsService (Wave 1) - Vehicle configuration data
- ABLineService (Wave 2) - Tram line generation
- SteeringCoordinatorService (Wave 3) - Turn execution and steering
- Section Control Services (Wave 4) - Automatic section pause/resume
- Guidance services (Wave 2/3) - Lookahead distance for turn triggers

**Existing System Constraints:**
- Must maintain AgOpenGPS file format compatibility
- Must use EventArgs pattern for state changes
- Must follow MVVM architecture with dependency injection
- Must support cross-platform (Windows, Linux, macOS)
- Must work offline (no cloud dependency)
- Backend services must be UI-agnostic (no Avalonia references)

**Technology Stack:**
- .NET 8, C# 12
- Microsoft.Extensions.DependencyInjection
- UTM coordinate system for calculations
- WGS84 for GPS data and file storage
- xUnit for testing

**Algorithms Required:**
- Shoelace formula (area calculation)
- Douglas-Peucker (boundary simplification)
- Ray-casting (point-in-polygon)
- Offset polygon algorithm (headland generation)
- Dubins path (minimum radius turns)
- R-tree spatial indexing
- Parallel line generation (tram lines)

**File Format Details:**
- **AgOpenGPS Formats:** Maintain exact compatibility
  - Boundary.txt: Lat,Lon pairs, one per line
  - Headland.Txt: Similar to Boundary.txt
  - Field.txt: Metadata including area, coordinate system
- **Modern Formats:**
  - GeoJSON: Standard geographic JSON format
  - KML: Google Earth compatible format
- **Configuration Storage:** User choice of Field.txt OR separate config file

**Event-Driven Architecture:**
- BoundaryRecordingStartedEventArgs
- BoundaryPointAddedEventArgs
- BoundaryRecordingCompletedEventArgs
- BoundaryValidationEventArgs
- HeadlandGeneratedEventArgs
- HeadlandProgressChangedEventArgs
- TurnPatternGeneratedEventArgs
- TurnStateChangedEventArgs
- TurnTriggerDetectedEventArgs
- TramLineGeneratedEventArgs
- TramLineUpdatedEventArgs

**Testing Requirements:**
- Unit tests for all services (xUnit)
- Integration tests for service interactions
- Performance benchmarks for critical operations
- Edge case testing:
  - GPS signal loss during boundary recording
  - Boundaries with holes (inner boundaries)
  - Multi-part fields (non-contiguous)
  - Very irregular/complex boundary shapes
  - Extremely large fields (>1000 acres)
  - Very small fields (<1 acre)
  - Turn patterns in constrained spaces
  - Tram line patterns with irregular AB lines

**Similar Code Patterns:**
No existing geometric calculation services available - this is the first implementation of complex geometric algorithms in the AgValoniaGPS codebase. However, follow these established patterns:
- Service interfaces for testability
- EventArgs for state changes
- File I/O patterns from existing services
- DI registration patterns
- Cross-platform file path handling
