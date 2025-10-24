# Wave 10: Moderate Forms UI - Tasks Breakdown

**Date**: 2025-10-23
**Wave**: 10
**Total Forms**: 15
**Estimated Effort**: 6-9 days (3 task groups × 2-3 days each)

---

## Task Organization Strategy

Wave 10 is organized into **3 parallel task groups** based on functionality and dependencies:

1. **Task Group 1: Core Operations** (Priority 1) - Main operational interface
2. **Task Group 2: Configuration** (Priority 2) - Settings and configuration dialogs
3. **Task Group 3: Field Management** (Priority 2) - Field tools and data management

Each task group can be developed **in parallel** by different agents or developers.

---

## Task Group 1: Core Operations (5 Forms)

**Priority**: High
**Estimated Effort**: 2-3 days
**Dependencies**: Wave 1 (GPS), Wave 2 (Guidance), Wave 5 (TramLine)
**Agent Type**: `ui-designer` + `testing-engineer`

### Forms in This Group
1. FormGPS - Main field view (217 controls)
2. FormFieldData - Field statistics panel (141 controls)
3. FormGPSData - GPS data panel (138 controls)
4. FormTramLine - Tram line configuration (120 controls)
5. FormQuickAB - Quick A-B line setup (167 controls)

### Tasks

#### Task 1.1: Create Base ViewModel Class
**File**: `AgValoniaGPS.ViewModels/Base/PanelViewModelBase.cs`
**Description**: Create base class for panel ViewModels (Wave 10-specific)
**Deliverables**:
- `PanelViewModelBase` class with:
  - `IsExpanded` property
  - `IsPinned` property
  - `CanCollapse` property
  - `CloseCommand` command
  - `CloseRequested` event
- Unit tests for PanelViewModelBase

**Acceptance Criteria**:
- [ ] PanelViewModelBase created and compiles
- [ ] All properties have getters/setters with RaiseAndSetIfChanged
- [ ] CloseCommand triggers CloseRequested event
- [ ] 100% test coverage

---

#### Task 1.2: FormGPSViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormGPSViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormGPSViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormGPS.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormGPS.axaml.cs`

**Description**: Main operational view with OpenGL map control
**Dependencies**: `IPositionUpdateService`, `IVehicleKinematicsService`, `IGuidanceService`

**ViewModel Properties**:
- `Position` (from IPositionUpdateService)
- `Heading` (from IVehicleKinematicsService)
- `GuidanceActive` (from IGuidanceService)
- `CrossTrackError` (from IGuidanceService)
- `DistanceToTurn` (from IGuidanceService)
- Commands: `ToggleGuidanceCommand`, `ZoomInCommand`, `ZoomOutCommand`

**AXAML Structure**:
- Central OpenGL map control (reuse existing OpenGLMapControl)
- Overlay panels for status displays
- Integration into MainWindow CENTER zone

**Acceptance Criteria**:
- [ ] ViewModel created with all properties
- [ ] All properties bound to services
- [ ] All commands execute properly
- [ ] AXAML view created with x:DataType
- [ ] View compiles with 0 errors
- [ ] View integrated into MainWindow
- [ ] Manual testing completed
- [ ] Unit tests pass (100%)

---

#### Task 1.3: FormFieldDataViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormFieldDataViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormFieldDataViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormFieldData.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormFieldData.axaml.cs`

**Description**: Field statistics and operation data panel
**Dependencies**: `IFieldStatisticsService`, `ISessionManagementService`

**ViewModel Properties**:
- `FieldName` (from ISessionManagementService)
- `FieldArea` (from IFieldStatisticsService)
- `DistanceTraveled` (from IFieldStatisticsService)
- `AreaCovered` (from IFieldStatisticsService)
- `AreaRemaining` (from IFieldStatisticsService)
- `AverageSpeed` (from IFieldStatisticsService)
- `TimeElapsed` (from IFieldStatisticsService)
- `TimeRemaining` (from IFieldStatisticsService)
- `WorkSwitchState` (from ISectionControlService)

**AXAML Structure**:
- RIGHT panel floating panel (collapsible)
- StatusBox borders for each statistic
- Close button (X)

**Acceptance Criteria**:
- [ ] ViewModel created with all properties
- [ ] Properties update in real-time when services change
- [ ] AXAML view matches POC UI design
- [ ] View integrates into RIGHT panel
- [ ] Collapse/expand works
- [ ] Unit tests pass (100%)

---

#### Task 1.4: FormGPSDataViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormGPSDataViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormGPSDataViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormGPSData.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormGPSData.axaml.cs`

**Description**: GPS data monitoring panel
**Dependencies**: `IGPSDataService`, `INTRIPClientService`

**ViewModel Properties**:
- `FixQuality` (from IGPSDataService)
- `Latitude` (from IGPSDataService)
- `Longitude` (from IGPSDataService)
- `Altitude` (from IGPSDataService)
- `Speed` (from IGPSDataService)
- `Heading` (from IGPSDataService)
- `HDOP` (from IGPSDataService)
- `SatelliteCount` (from IGPSDataService)
- `AgeOfCorrection` (from IGPSDataService)
- `NTRIPConnected` (from INTRIPClientService)

**AXAML Structure**:
- RIGHT panel floating panel (collapsible)
- Color-coded fix quality indicator
- Lat/lon display
- Status boxes for each metric
- Close button (X)

**Acceptance Criteria**:
- [ ] ViewModel created with all properties
- [ ] Properties update in real-time
- [ ] Fix quality color converter works
- [ ] AXAML view matches POC UI design
- [ ] View integrates into RIGHT panel
- [ ] Unit tests pass (100%)

---

#### Task 1.5: FormTramLineViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormTramLineViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormTramLineViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormTramLine.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormTramLine.axaml.cs`

**Description**: Tram line configuration panel
**Dependencies**: `ITramLineService`

**ViewModel Properties**:
- `TramMode` (A+, A-, All)
- `PassInterval` (e.g., 6)
- `DisplayWidth`
- `ShowLeftTram`
- `ShowRightTram`
- Commands: `GenerateCommand`, `ClearCommand`

**AXAML Structure**:
- LEFT panel floating panel
- Radio buttons for mode
- NumericUpDown for pass interval
- CheckBoxes for display options
- Generate/Clear buttons

**Acceptance Criteria**:
- [ ] ViewModel created with all properties
- [ ] Commands execute ITramLineService methods
- [ ] AXAML view matches POC UI design
- [ ] View integrates into LEFT panel
- [ ] Input validation works
- [ ] Unit tests pass (100%)

---

#### Task 1.6: FormQuickABViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormQuickABViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormQuickABViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormQuickAB.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormQuickAB.axaml.cs`

**Description**: Quick A-B line setup panel
**Dependencies**: `IABLineService`, `IPositionUpdateService`

**ViewModel Properties**:
- `PointA` (Position)
- `PointB` (Position)
- `HeadingAdjustment` (degrees)
- `LineOffset` (meters)
- `CanApply` (bool)
- Commands: `SetPointACommand`, `SetPointBCommand`, `ApplyCommand`, `CancelCommand`

**AXAML Structure**:
- LEFT panel floating panel
- Two big buttons: "Set A" and "Set B"
- Slider for heading adjustment
- NumericUpDown for line offset
- Apply/Cancel buttons

**Acceptance Criteria**:
- [ ] ViewModel created with all properties
- [ ] SetPointA uses current position
- [ ] SetPointB projects from A
- [ ] ApplyCommand creates A-B line in service
- [ ] AXAML view matches POC UI design
- [ ] Unit tests pass (100%)

---

#### Task 1.7: Integration & Testing
**Description**: Integrate all 5 forms into POC UI and test end-to-end

**Tasks**:
1. Add navigation buttons to MainWindow
2. Wire up commands to show/hide panels
3. Verify z-indexing works correctly
4. Test all forms display properly
5. Test service integration (real-time updates)
6. Performance testing (<100ms updates)

**Acceptance Criteria**:
- [ ] All 5 forms accessible from MainWindow
- [ ] Panels expand/collapse correctly
- [ ] Real-time updates work
- [ ] No z-index conflicts
- [ ] Performance targets met
- [ ] Manual testing completed

---

## Task Group 2: Configuration (5 Forms)

**Priority**: Medium
**Estimated Effort**: 2-3 days
**Dependencies**: Wave 8 (Configuration, Validation)
**Agent Type**: `ui-designer` + `testing-engineer`

### Forms in This Group
1. ConfigVehicleControl - Vehicle settings (158 controls)
2. FormColorSection - Color configuration (191 controls)
3. FormSteerSettings - Steering parameters (~150 controls)
4. FormSectionConfig - Section control setup (~150 controls)
5. FormVehicleSetup - Vehicle geometry (~150 controls)

### Tasks

#### Task 2.1: ConfigVehicleControlViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/ConfigVehicleControlViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/ConfigVehicleControlViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/ConfigVehicleControl.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/ConfigVehicleControl.axaml.cs`

**Description**: Vehicle configuration dialog
**Dependencies**: `IConfigurationService`, `IValidationService`, `IVehicleProfileService`

**ViewModel Properties**:
- `Wheelbase` (1.0 - 10.0 m)
- `TrackWidth` (1.0 - 20.0 m)
- `AntennaHeight` (0.0 - 5.0 m)
- `AntennaPivotDistance` (0.0 - 10.0 m)
- `MaxSteerAngle` (10.0 - 60.0 degrees)
- `MinTurningRadius` (calculated)
- Commands: `OKCommand`, `CancelCommand`, `ResetCommand`

**AXAML Structure**:
- Modal dialog (Window)
- TabControl with 3 tabs: Dimensions, Antenna, Implement
- NumericUpDown controls with validation
- OK/Cancel buttons

**Acceptance Criteria**:
- [ ] ViewModel created with validation
- [ ] All properties validated on set
- [ ] OKCommand saves to IConfigurationService
- [ ] AXAML dialog created with proper styling
- [ ] Modal dialog works correctly
- [ ] Unit tests pass (100%)

---

#### Task 2.2: FormColorSectionViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormColorSectionViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormColorSectionViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormColorSection.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormColorSection.axaml.cs`

**Description**: Section and coverage color configuration
**Dependencies**: `IConfigurationService`, `ISectionConfigurationService`

**ViewModel Properties**:
- `SectionColors` (ObservableCollection<Color>)
- `CoverageColor` (Color)
- `BoundaryColor` (Color)
- `GuidanceLineColor` (Color)
- `SelectedSectionIndex` (int)
- Commands: `OKCommand`, `CancelCommand`, `ResetCommand`

**AXAML Structure**:
- Modal dialog
- ColorPalette custom control (reuse from Wave 9)
- Section list with color preview
- Color pickers for each element

**Acceptance Criteria**:
- [ ] ViewModel created
- [ ] Color changes update preview
- [ ] OKCommand saves to IConfigurationService
- [ ] AXAML dialog uses ColorPalette control
- [ ] Unit tests pass (100%)

---

#### Task 2.3: FormSteerSettingsViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormSteerSettingsViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormSteerSettingsViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormSteerSettings.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormSteerSettings.axaml.cs`

**Description**: Steering algorithm parameters
**Dependencies**: `ISteeringCoordinatorService`, `IConfigurationService`

**ViewModel Properties**:
- Pure Pursuit:
  - `PPLookAheadDistance` (1.0 - 20.0 m)
  - `PPGain` (0.1 - 5.0)
- Stanley:
  - `StanleyGain` (0.1 - 5.0)
  - `StanleyHeadingWeight` (0.0 - 1.0)
- General:
  - `MinSpeed` (0.5 - 5.0 km/h)
  - `MaxSpeed` (5.0 - 40.0 km/h)
  - `Aggressiveness` (0 - 10)
- Commands: `OKCommand`, `CancelCommand`, `TestCommand`

**AXAML Structure**:
- Modal dialog
- TabControl: Pure Pursuit, Stanley, General, Advanced
- Sliders with numeric display
- Test button to preview settings

**Acceptance Criteria**:
- [ ] ViewModel created with validation
- [ ] All parameters validated
- [ ] TestCommand applies settings temporarily
- [ ] OKCommand saves to IConfigurationService
- [ ] AXAML dialog with 4 tabs
- [ ] Unit tests pass (100%)

---

#### Task 2.4: FormSectionConfigViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormSectionConfigViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormSectionConfigViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormSectionConfig.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormSectionConfig.axaml.cs`

**Description**: Section control configuration
**Dependencies**: `ISectionConfigurationService`, `IConfigurationService`

**ViewModel Properties**:
- `NumberOfSections` (1 - 31)
- `SectionWidth` (0.1 - 10.0 m)
- `LookAheadDistance` (1.0 - 20.0 m)
- `ManualSections` (bool)
- `Sections` (ObservableCollection<SectionViewModel>)
- Commands: `OKCommand`, `CancelCommand`, `AutoConfigureCommand`

**AXAML Structure**:
- Modal dialog
- Section list with enable/disable checkboxes
- Section width and look-ahead inputs
- Visual section layout preview

**Acceptance Criteria**:
- [ ] ViewModel created
- [ ] Section list updates when count changes
- [ ] AutoConfigureCommand distributes sections evenly
- [ ] OKCommand saves to ISectionConfigurationService
- [ ] AXAML dialog with visual preview
- [ ] Unit tests pass (100%)

---

#### Task 2.5: FormVehicleSetupViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormVehicleSetupViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormVehicleSetupViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormVehicleSetup.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormVehicleSetup.axaml.cs`

**Description**: Complete vehicle and implement geometry
**Dependencies**: `IVehicleProfileService`, `IConfigurationService`

**ViewModel Properties**:
- Vehicle:
  - `Wheelbase`, `TrackWidth`, `MaxSteerAngle`
- Antenna:
  - `AntennaHeight`, `AntennaPivotDistance`, `AntennaOffset`
- Implement:
  - `ImplementLength`, `ImplementWidth`, `ImplementOffset`
  - `HitchLength`, `PivotType`
- Commands: `OKCommand`, `CancelCommand`, `LoadProfileCommand`, `SaveProfileCommand`

**AXAML Structure**:
- Modal dialog
- TabControl: Vehicle, Antenna, Implement, Hitch
- Visual diagram showing measurements
- Profile dropdown for quick setup

**Acceptance Criteria**:
- [ ] ViewModel created with validation
- [ ] All parameters validated
- [ ] LoadProfileCommand populates from saved profile
- [ ] SaveProfileCommand creates new profile
- [ ] AXAML dialog with visual diagrams
- [ ] Unit tests pass (100%)

---

#### Task 2.6: Integration & Testing
**Description**: Integrate all 5 config forms and test

**Tasks**:
1. Add settings menu to MainWindow
2. Wire up menu items to show dialogs
3. Test modal overlay works
4. Test settings persistence
5. Test validation prevents invalid input

**Acceptance Criteria**:
- [ ] All 5 config forms accessible from settings menu
- [ ] Modal dialogs overlay correctly
- [ ] Settings persist between sessions
- [ ] Validation works properly
- [ ] Manual testing completed

---

## Task Group 3: Field Management (5 Forms)

**Priority**: Medium
**Estimated Effort**: 2-3 days
**Dependencies**: Wave 5 (Boundary, Headland, Field I/O)
**Agent Type**: `ui-designer` + `testing-engineer`

### Forms in This Group
1. FormBndTool - Boundary tool (146 controls)
2. FormFieldDirectory - Field manager (~120 controls)
3. FormFieldNew - New field creation (~130 controls)
4. FormHeadland - Headland configuration (~110 controls)
5. FormAgShareField - AgShare browser (~140 controls)

### Tasks

#### Task 3.1: FormBndToolViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormBndToolViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormBndToolViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormBndTool.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormBndTool.axaml.cs`

**Description**: Boundary drawing and editing tool
**Dependencies**: `IBoundaryManagementService`, `IFieldService`

**ViewModel Properties**:
- `ToolMode` (Draw, Edit, Delete)
- `Points` (ObservableCollection<Position>)
- `PointCount` (int)
- `Area` (double, hectares)
- `SimplificationTolerance` (0.5 - 5.0 m)
- `CanCloseBoundary` (bool)
- Commands: `StartDrawingCommand`, `CloseBoundaryCommand`, `SimplifyCommand`, `ClearCommand`

**AXAML Structure**:
- RIGHT panel tool mode
- Button stack: Start Drawing, Close Boundary, Simplify, Clear
- Status boxes: Point count, Area
- Slider for simplification tolerance

**Acceptance Criteria**:
- [ ] ViewModel created
- [ ] Commands trigger IBoundaryManagementService methods
- [ ] Points list updates in real-time
- [ ] Area calculates automatically
- [ ] AXAML view integrates into RIGHT panel
- [ ] Unit tests pass (100%)

---

#### Task 3.2: FormFieldDirectoryViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormFieldDirectoryViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormFieldDirectoryViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormFieldDirectory.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormFieldDirectory.axaml.cs`

**Description**: Field browser and manager
**Dependencies**: `IFieldService`, `ISessionManagementService`

**ViewModel Properties**:
- `Fields` (ObservableCollection<FieldInfo>)
- `SelectedField` (FieldInfo)
- `SearchText` (string)
- `FilteredFields` (computed from Fields + SearchText)
- Commands: `OpenFieldCommand`, `NewFieldCommand`, `DeleteFieldCommand`, `ExportFieldCommand`

**AXAML Structure**:
- Modal dialog or CENTER panel replacement
- Search box at top
- ListBox with field thumbnails
- Buttons: Open, New, Delete, Export

**Acceptance Criteria**:
- [ ] ViewModel created
- [ ] Fields list loads from IFieldService
- [ ] Search filters correctly
- [ ] OpenFieldCommand loads field in ISessionManagementService
- [ ] AXAML view created
- [ ] Unit tests pass (100%)

---

#### Task 3.3: FormFieldNewViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormFieldNewViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormFieldNewViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormFieldNew.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormFieldNew.axaml.cs`

**Description**: New field creation dialog
**Dependencies**: `IFieldService`, `IBoundaryManagementService`

**ViewModel Properties**:
- `FieldName` (string, required)
- `Latitude` (double)
- `Longitude` (double)
- `BoundarySource` (None, File, Draw)
- `BoundaryFilePath` (string)
- `BackgroundImagePath` (string)
- Commands: `CreateCommand`, `CancelCommand`, `BrowseBoundaryCommand`, `BrowseImageCommand`

**AXAML Structure**:
- Modal dialog
- Text input for field name
- Lat/lon inputs (or "Pick on Map" button)
- Radio buttons for boundary source
- File pickers for boundary and image
- Create/Cancel buttons

**Acceptance Criteria**:
- [ ] ViewModel created with validation
- [ ] FieldName is required
- [ ] BoundarySource options work
- [ ] CreateCommand creates field in IFieldService
- [ ] AXAML dialog created
- [ ] Unit tests pass (100%)

---

#### Task 3.4: FormHeadlandViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormHeadlandViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormHeadlandViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormHeadland.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormHeadland.axaml.cs`

**Description**: Headland configuration tool
**Dependencies**: `IHeadlandService`

**ViewModel Properties**:
- `HeadlandMode` (Auto, Manual)
- `NumberOfPasses` (1 - 10)
- `PassWidth` (1.0 - 50.0 m)
- `TurnType` (Omega, T, Y)
- Commands: `GenerateCommand`, `ClearCommand`

**AXAML Structure**:
- RIGHT panel tool or modal dialog
- Radio buttons for mode
- NumericUpDown for passes and width
- ComboBox for turn type
- Generate/Clear buttons

**Acceptance Criteria**:
- [ ] ViewModel created
- [ ] GenerateCommand triggers IHeadlandService
- [ ] Input validation works
- [ ] AXAML view created
- [ ] Unit tests pass (100%)

---

#### Task 3.5: FormAgShareFieldViewModel + View
**Files**:
- `AgValoniaGPS.ViewModels/Forms/FormAgShareFieldViewModel.cs`
- `AgValoniaGPS.ViewModels.Tests/Forms/FormAgShareFieldViewModelTests.cs`
- `AgValoniaGPS.Desktop/Views/Forms/FormAgShareField.axaml`
- `AgValoniaGPS.Desktop/Views/Forms/FormAgShareField.axaml.cs`

**Description**: AgShare field browser and downloader
**Dependencies**: `IAgShareService` (future), `IFieldService`

**ViewModel Properties**:
- `IsLoggedIn` (bool)
- `Username` (string)
- `Password` (string)
- `AvailableFields` (ObservableCollection<AgShareFieldInfo>)
- `SelectedField` (AgShareFieldInfo)
- Commands: `LoginCommand`, `DownloadCommand`, `UploadCommand`

**AXAML Structure**:
- Modal dialog
- Login panel (username/password)
- Field list with previews
- Download/Upload buttons

**Acceptance Criteria**:
- [ ] ViewModel created
- [ ] LoginCommand authenticates (mock for now)
- [ ] DownloadCommand saves field to local
- [ ] UploadCommand uploads current field
- [ ] AXAML dialog created
- [ ] Unit tests pass (100%)

---

#### Task 3.6: Integration & Testing
**Description**: Integrate all 5 field management forms and test

**Tasks**:
1. Add field tools menu to MainWindow
2. Wire up menu items
3. Test boundary tool with map interaction
4. Test field directory browsing
5. Test end-to-end field creation workflow

**Acceptance Criteria**:
- [ ] All 5 forms accessible
- [ ] Boundary tool integrates with OpenGL map
- [ ] Field directory loads and displays correctly
- [ ] New field workflow works end-to-end
- [ ] Manual testing completed

---

## Cross-Cutting Tasks

### Task 4.1: Update MainWindow Navigation
**File**: `AgValoniaGPS.Desktop/Views/MainWindow.axaml`
**Description**: Add navigation buttons for all Wave 10 forms

**Changes**:
1. Add "View" menu with submenu items:
   - Field Data Panel
   - GPS Data Panel
   - Tram Line Config
   - Quick A-B Setup
2. Add "Settings" menu with submenu items:
   - Vehicle Configuration
   - Color Settings
   - Steering Settings
   - Section Configuration
   - Vehicle Setup
3. Add "Field" menu with submenu items:
   - Boundary Tool
   - Field Directory
   - New Field
   - Headland Configuration
   - AgShare Browser

**Acceptance Criteria**:
- [ ] All menus added to MainWindow
- [ ] Menu items trigger correct forms
- [ ] Keyboard shortcuts work (Alt+V, Alt+S, Alt+F)

---

### Task 4.2: Update ServiceCollectionExtensions
**File**: `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
**Description**: Register all Wave 10 ViewModels in DI container

**Changes**:
```csharp
// Wave 10: Moderate Forms UI
services.AddTransient<FormGPSViewModel>();
services.AddTransient<FormFieldDataViewModel>();
services.AddTransient<FormGPSDataViewModel>();
services.AddTransient<FormTramLineViewModel>();
services.AddTransient<FormQuickABViewModel>();
services.AddTransient<ConfigVehicleControlViewModel>();
services.AddTransient<FormColorSectionViewModel>();
services.AddTransient<FormSteerSettingsViewModel>();
services.AddTransient<FormSectionConfigViewModel>();
services.AddTransient<FormVehicleSetupViewModel>();
services.AddTransient<FormBndToolViewModel>();
services.AddTransient<FormFieldDirectoryViewModel>();
services.AddTransient<FormFieldNewViewModel>();
services.AddTransient<FormHeadlandViewModel>();
services.AddTransient<FormAgShareFieldViewModel>();
```

**Acceptance Criteria**:
- [ ] All 15 ViewModels registered
- [ ] Application builds successfully
- [ ] ViewModels can be resolved from container

---

### Task 4.3: Create Value Converters (if needed)
**Files**: `AgValoniaGPS.Desktop/Converters/`
**Description**: Create any additional converters needed for Wave 10

**Potential Converters**:
1. `EnumToStringConverter` (for TurnType, HeadlandMode, etc.)
2. `DoubleToStringConverter` with formatting
3. `BoolToVisibilityConverter` (if not already present)

**Acceptance Criteria**:
- [ ] Converters created
- [ ] Converters registered in App.axaml
- [ ] Converters tested

---

### Task 4.4: Documentation
**Files**:
- `agent-os/specs/2025-10-23-wave-10-moderate-forms-ui/implementation-report-task-group-1.md`
- `agent-os/specs/2025-10-23-wave-10-moderate-forms-ui/implementation-report-task-group-2.md`
- `agent-os/specs/2025-10-23-wave-10-moderate-forms-ui/implementation-report-task-group-3.md`
- `agent-os/specs/2025-10-23-wave-10-moderate-forms-ui/verification-report.md`

**Description**: Document implementation progress and results

**Deliverables**:
1. Implementation report per task group (what was built, how it works)
2. Verification report (test results, build output, screenshots)
3. Known issues document (if any)

**Acceptance Criteria**:
- [ ] All reports created
- [ ] Test results documented
- [ ] Screenshots included
- [ ] Known issues listed

---

## Summary

**Total Tasks**: 24 (18 form tasks + 6 cross-cutting tasks)
**Total ViewModels**: 15 + 1 base class
**Total AXAML Views**: 15
**Total Test Files**: 15
**Estimated LOC**: ~8000 lines (ViewModels + tests + AXAML)

**Parallel Execution**:
- Task Groups 1, 2, 3 can run in parallel
- Cross-cutting tasks run after task groups complete
- Total time: 2-3 days per group + 1 day integration = 3-4 days total (with parallelization)

**Success Metrics**:
- 100% test coverage for ViewModels
- 0 AXAML compilation errors
- 0 binding errors in output
- All forms display correctly
- All commands execute properly
- Performance <100ms updates

---

**End of Tasks Document**
