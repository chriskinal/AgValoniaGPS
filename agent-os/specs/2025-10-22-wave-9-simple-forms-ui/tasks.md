# Wave 9: Simple Forms UI - Task List

## Overview
Implementation of 53 simple Avalonia UI forms (<100 controls each) organized into 8 task groups. Tasks are designed for parallel execution where dependencies allow.

## Task Status Legend
- ⏸️ **PENDING**: Not started
- 🔄 **IN_PROGRESS**: Currently being worked on
- ✅ **COMPLETED**: Finished and verified
- ❌ **BLOCKED**: Waiting on dependency

---

## Task Group 1: Foundation & Architecture
**Priority**: HIGH | **Duration**: 3 days | **Dependencies**: None

### Task 1.1: Create ViewModel Base Classes
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 4h

**Description**: Create foundation ViewModel base classes using ReactiveUI

**Deliverables**:
- [ ] `ViewModelBase` class inheriting `ReactiveObject`
  - Common property change notification
  - Busy/loading state properties
  - Error handling properties
- [ ] `DialogViewModelBase` class inheriting `ViewModelBase`
  - `OKCommand` and `CancelCommand`
  - `DialogResult` property
  - `CloseRequested` event
- [ ] `PickerViewModelBase<T>` class for selection dialogs
  - `Items` observable collection
  - `SelectedItem` property
  - `SearchText` property with filter logic

**Files**:
- `AgValoniaGPS.ViewModels/Base/ViewModelBase.cs`
- `AgValoniaGPS.ViewModels/Base/DialogViewModelBase.cs`
- `AgValoniaGPS.ViewModels/Base/PickerViewModelBase.cs`

**Tests**:
- `AgValoniaGPS.ViewModels.Tests/Base/ViewModelBaseTests.cs`
- `AgValoniaGPS.ViewModels.Tests/Base/DialogViewModelBaseTests.cs`

**Acceptance Criteria**:
- All base classes compile without warnings
- Unit tests pass with >95% coverage
- ReactiveUI property change notifications work correctly

---

### Task 1.2: Create Dialog Service
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 6h

**Description**: Implement IDialogService for showing dialogs across the application

**Deliverables**:
- [ ] `IDialogService` interface
  - `Task<TResult> ShowDialogAsync<TViewModel>()`
  - `Task<bool> ShowConfirmationAsync(string message)`
  - `Task ShowMessageAsync(string message, MessageType type)`
  - `Task<string> ShowFilePickerAsync(FilePickerOptions options)`
- [ ] `DialogService` implementation
  - Window creation and management
  - Result handling
  - Platform-specific dialog wrappers

**Files**:
- `AgValoniaGPS.Services/UI/IDialogService.cs`
- `AgValoniaGPS.Services/UI/DialogService.cs`
- `AgValoniaGPS.Models/DialogResult.cs`
- `AgValoniaGPS.Models/MessageType.cs`

**Tests**:
- `AgValoniaGPS.Services.Tests/UI/DialogServiceTests.cs`

**Acceptance Criteria**:
- Can show modal dialogs
- Can return typed results
- Properly disposes resources
- Thread-safe

---

### Task 1.3: Create Value Converters
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3h

**Description**: Create reusable value converters for data binding

**Deliverables**:
- [ ] `InverseBoolConverter` - Negates boolean values
- [ ] `BoolToVisibilityConverter` - Boolean to IsVisible
- [ ] `NullToVisibilityConverter` - Hide if null
- [ ] `EmptyStringToVisibilityConverter` - Hide if empty
- [ ] `UnitConverter` - Metric/Imperial conversion
- [ ] `EnumToStringConverter` - Enum to localized string

**Files**:
- `AgValoniaGPS.Desktop/Converters/InverseBoolConverter.cs`
- `AgValoniaGPS.Desktop/Converters/BoolToVisibilityConverter.cs`
- `AgValoniaGPS.Desktop/Converters/NullToVisibilityConverter.cs`
- `AgValoniaGPS.Desktop/Converters/EmptyStringToVisibilityConverter.cs`
- `AgValoniaGPS.Desktop/Converters/UnitConverter.cs`
- `AgValoniaGPS.Desktop/Converters/EnumToStringConverter.cs`

**Tests**:
- `AgValoniaGPS.Desktop.Tests/Converters/ConverterTests.cs`

**Acceptance Criteria**:
- All converters implement IValueConverter
- Registered in App.axaml resources
- Unit tests verify conversion logic
- Handle null/invalid input gracefully

---

### Task 1.4: Setup ViewModel DI Registration
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 2h

**Description**: Establish DI pattern for ViewModels

**Deliverables**:
- [ ] Extend `ServiceCollectionExtensions` with ViewModel registration
- [ ] Register all base classes
- [ ] Register DialogService
- [ ] Document registration pattern

**Files**:
- `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs` (update)

**Acceptance Criteria**:
- ViewModels can be resolved from DI container
- Services inject correctly
- Singleton vs Transient lifetimes correct

---

### Task 1.5: Create Unit Test Infrastructure
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3h

**Description**: Setup test project and helper utilities

**Deliverables**:
- [ ] Create `AgValoniaGPS.ViewModels.Tests` project
- [ ] Setup xUnit test framework
- [ ] Create mock service helpers
- [ ] Create test data builders
- [ ] Setup test coverage reporting

**Files**:
- `AgValoniaGPS.ViewModels.Tests/AgValoniaGPS.ViewModels.Tests.csproj`
- `AgValoniaGPS.ViewModels.Tests/Helpers/MockServiceFactory.cs`
- `AgValoniaGPS.ViewModels.Tests/Builders/TestDataBuilder.cs`

**Acceptance Criteria**:
- Test project compiles
- Can run tests via `dotnet test`
- Mock services available
- Test coverage reporting configured

---

## Task Group 2: Picker Dialogs
**Priority**: HIGH | **Duration**: 4 days | **Dependencies**: Task Group 1

### Task 2.1: Implement FormColorPicker
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 4h

**Legacy Form**: 89 controls - Color selection with palette and RGB preview

**Deliverables**:
- [ ] `ColorPickerViewModel` with color properties and commands
- [ ] `FormColorPicker.axaml` view with color palette
- [ ] Custom `ColorPalette` control
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Pickers/ColorPickerViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/FormColorPicker.axaml`
- `AgValoniaGPS.Desktop/Controls/ColorPalette.axaml`

**Acceptance Criteria**:
- Can select color from palette
- RGB sliders update color
- Hex input works
- Preview shows selected color

---

### Task 2.2: Implement FormDrivePicker
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 2h

**Legacy Form**: 12 controls - Drive/volume selection

**Deliverables**:
- [ ] `DrivePickerViewModel` with drive enumeration
- [ ] `FormDrivePicker.axaml` view with drive list
- [ ] Drive info display (space, type)
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Pickers/DrivePickerViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/FormDrivePicker.axaml`

**Acceptance Criteria**:
- Shows all available drives
- Displays drive info (capacity, free space)
- Cross-platform compatible

---

### Task 2.3: Implement FormFilePicker
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 6h

**Legacy Form**: 57 controls - File browser dialog

**Deliverables**:
- [ ] `FilePickerViewModel` with file system navigation
- [ ] `FormFilePicker.axaml` view with file list
- [ ] File filtering by extension
- [ ] Directory navigation
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Pickers/FilePickerViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/FormFilePicker.axaml`

**Acceptance Criteria**:
- Can navigate directories
- Filter by file extension
- Show file details (size, date)
- Return selected file path

---

### Task 2.4: Implement FormRecordPicker
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3h

**Legacy Form**: 20 controls - Record selection from lists

**Deliverables**:
- [ ] `RecordPickerViewModel<T>` generic picker
- [ ] `FormRecordPicker.axaml` view with searchable list
- [ ] Search/filter functionality
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Pickers/RecordPickerViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/FormRecordPicker.axaml`

**Acceptance Criteria**:
- Generic picker works with any type
- Search filters list in real-time
- Shows custom display template

---

### Task 2.5: Implement FormLoadProfile
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 4h

**Legacy Form**: 37 controls - Profile loading dialog

**Deliverables**:
- [ ] `LoadProfileViewModel` with profile list
- [ ] `FormLoadProfile.axaml` view
- [ ] Integration with Wave 8 IConfigurationService
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Pickers/LoadProfileViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/FormLoadProfile.axaml`

**Acceptance Criteria**:
- Lists available profiles
- Shows profile details
- Loads selected profile

---

### Task 2.6: Implement FormNewProfile
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3h

**Legacy Form**: 25 controls - Profile creation dialog

**Deliverables**:
- [ ] `NewProfileViewModel` with validation
- [ ] `FormNewProfile.axaml` view
- [ ] Profile name validation
- [ ] Integration with Wave 8 IConfigurationService
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Pickers/NewProfileViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/FormNewProfile.axaml`

**Acceptance Criteria**:
- Validates profile name (unique, valid chars)
- Creates new profile
- Shows validation errors

---

## Task Group 3: Input Dialogs
**Priority**: HIGH | **Duration**: 3 days | **Dependencies**: Task Group 1

### Task 3.1: Create NumericKeypad Custom Control
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 4h

**Description**: Touch-friendly numeric input control

**Deliverables**:
- [ ] `NumericKeypad` custom control
- [ ] Large buttons (60x60 minimum)
- [ ] Decimal support
- [ ] Backspace, clear, enter
- [ ] Value binding property

**Files**:
- `AgValoniaGPS.Desktop/Controls/NumericKeypad.axaml`
- `AgValoniaGPS.Desktop/Controls/NumericKeypad.axaml.cs`

**Acceptance Criteria**:
- Touch-friendly button sizes
- Decimal point handling
- Two-way value binding
- Visual feedback on press

---

### Task 3.2: Create VirtualKeyboard Custom Control
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 6h

**Description**: On-screen QWERTY keyboard

**Deliverables**:
- [ ] `VirtualKeyboard` custom control
- [ ] QWERTY layout
- [ ] Shift/caps lock support
- [ ] Special characters
- [ ] Backspace, enter, space

**Files**:
- `AgValoniaGPS.Desktop/Controls/VirtualKeyboard.axaml`
- `AgValoniaGPS.Desktop/Controls/VirtualKeyboard.axaml.cs`

**Acceptance Criteria**:
- Full QWERTY layout
- Shift toggles case
- Special char mode
- Two-way text binding

---

### Task 3.3: Implement FormNumeric
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3h

**Legacy Form**: 33 controls - Numeric input dialog

**Deliverables**:
- [ ] `NumericInputViewModel`
- [ ] `FormNumeric.axaml` using NumericKeypad control
- [ ] Min/max validation
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Input/NumericInputViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Input/FormNumeric.axaml`

**Acceptance Criteria**:
- Shows NumericKeypad control
- Validates range
- Returns numeric value

---

### Task 3.4: Implement FormKeyboard
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3h

**Legacy Form**: 64 controls - Virtual keyboard dialog

**Deliverables**:
- [ ] `KeyboardInputViewModel`
- [ ] `FormKeyboard.axaml` using VirtualKeyboard control
- [ ] Unit tests

**Files**:
- `AgValoniaGPS.ViewModels/Dialogs/Input/KeyboardInputViewModel.cs`
- `AgValoniaGPS.Desktop/Views/Dialogs/Input/FormKeyboard.axaml`

**Acceptance Criteria**:
- Shows VirtualKeyboard control
- Returns text value
- Handles special characters

---

## Task Group 4: Utility Dialogs
**Priority**: MEDIUM | **Duration**: 7 days | **Dependencies**: Task Groups 1, 2

### Task 4.1-4.14: Implement Utility Dialogs
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3-5h each

**Forms** (14 total):
1. FormDialog (8 controls)
2. Form_About (90 controls)
3. Form_Help (20 controls)
4. Form_Keys (217 controls)
5. FormEventViewer (22 controls)
6. FormGPSData (94 controls)
7. FormPan (12 controls)
8. FormSaveOrNot (16 controls)
9. FormSaving (5 controls)
10. FormShiftPos (29 controls)
11. FormTimedMessage (9 controls)
12. FormWebCam (5 controls)
13. Form_First (74 controls)
14. FormAgShareSettings (81 controls)

**Common Deliverables** (per form):
- [ ] ViewModel with properties/commands
- [ ] AXAML view with bindings
- [ ] Service integration where needed
- [ ] Unit tests

**Special Requirements**:
- FormTimedMessage: Timer implementation
- FormWebCam: Camera integration
- FormGPSData: Real-time GPS data binding
- FormEventViewer: Event log display

**Acceptance Criteria** (per form):
- Dialog opens and closes correctly
- All controls functional
- Data binding works
- Tests pass

---

## Task Group 5: Field Management Dialogs
**Priority**: MEDIUM | **Duration**: 6 days | **Dependencies**: Task Groups 1, 2, Wave 5

### Task 5.1-5.12: Implement Field Management Dialogs
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3-5h each

**Forms** (12 total):
1. FormFieldDir (35 controls)
2. FormFieldExisting (48 controls)
3. FormFieldData (40 controls)
4. FormFieldKML (28 controls)
5. FormFieldISOXML (35 controls)
6. FormBoundary (27 controls)
7. FormBndTool (20 controls)
8. FormBoundaryPlayer (30 controls)
9. FormBuildBoundaryFromTracks (25 controls)
10. FormFlags (40 controls)
11. FormEnterFlag (25 controls)
12. FormAgShareDownloader (67 controls)

**Common Deliverables** (per form):
- [ ] ViewModel with field service integration
- [ ] AXAML view with bindings
- [ ] Integration with Wave 5 services
- [ ] Unit tests

**Service Integration**:
- IFieldService: Field loading/saving
- IBoundaryManagementService: Boundary operations
- ISessionManagementService: Current field state

**Acceptance Criteria** (per form):
- Integrates with field services
- Loads/saves field data correctly
- Handles file I/O errors
- Tests pass

---

## Task Group 6: Guidance Dialogs
**Priority**: MEDIUM | **Duration**: 5 days | **Dependencies**: Task Groups 1, Wave 2, Wave 5

### Task 6.1-6.9: Implement Guidance Dialogs
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 3-6h each

**Forms** (9 total):
1. FormABDraw (117 controls)
2. FormQuickAB (167 controls)
3. FormSmoothAB (35 controls)
4. FormGrid (91 controls)
5. FormTram (87 controls)
6. FormTramLine (30 controls)
7. FormHeadLine (60 controls)
8. FormHeadAche (40 controls)
9. FormRecordName (20 controls)

**Common Deliverables** (per form):
- [ ] ViewModel with guidance service integration
- [ ] AXAML view with bindings
- [ ] Integration with Wave 2/5 services
- [ ] Unit tests

**Service Integration**:
- IABLineService: AB line operations
- ICurveLineService: Curve operations
- IHeadlandService: Headland operations
- ITramLineService: Tram line operations

**Acceptance Criteria** (per form):
- Integrates with guidance services
- Creates/edits guidance lines
- Visualizes lines correctly
- Tests pass

---

## Task Group 7: Simple Settings Dialogs
**Priority**: LOW | **Duration**: 4 days | **Dependencies**: Task Groups 1, Wave 8

### Task 7.1-7.8: Implement Settings Dialogs
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 2-5h each

**Forms** (8 total):
1. ConfigSummaryControl (103 controls)
2. ConfigVehicleControl (158 controls)
3. ConfigData (0 controls - placeholder)
4. ConfigHelp (0 controls - placeholder)
5. ConfigMenu (0 controls - placeholder)
6. ConfigModule (0 controls - placeholder)
7. ConfigTool (0 controls - placeholder)
8. ConfigVehicle (0 controls - placeholder)

**Common Deliverables** (per form):
- [ ] ViewModel bound to IConfigurationService
- [ ] AXAML view with two-way bindings
- [ ] Validation for settings
- [ ] Unit tests

**Service Integration**:
- IConfigurationService: All settings
- VehicleConfiguration: Vehicle settings

**Acceptance Criteria** (per form):
- Binds to configuration service
- Updates settings correctly
- Validates input
- Tests pass

---

## Task Group 8: Integration & Testing
**Priority**: CONTINUOUS | **Duration**: Throughout wave | **Dependencies**: All task groups

### Task 8.1: MainViewModel Integration
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 4h

**Deliverables**:
- [ ] Add dialog launching methods to MainViewModel
- [ ] Update commands to use DialogService
- [ ] Handle dialog results

**Files**:
- `AgValoniaGPS.ViewModels/MainViewModel.cs` (update)

---

### Task 8.2: Integration Testing
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 8h

**Deliverables**:
- [ ] Create integration test suite
- [ ] Test dialog workflows end-to-end
- [ ] Test service integration
- [ ] Test data persistence

**Files**:
- `AgValoniaGPS.Integration.Tests/` (new project)

---

### Task 8.3: Manual Testing
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 16h

**Deliverables**:
- [ ] Manual test plan document
- [ ] Windows testing
- [ ] Linux testing
- [ ] Touch device testing
- [ ] Performance profiling
- [ ] Memory leak testing

---

### Task 8.4: Accessibility Testing
**Status**: ⏸️ PENDING | **Assignee**: TBD | **Estimated**: 4h

**Deliverables**:
- [ ] Keyboard navigation testing
- [ ] Screen reader compatibility
- [ ] High contrast theme testing
- [ ] Touch target size verification

---

## Summary Statistics

### By Priority
- **HIGH**: 15 tasks (Groups 1, 2, 3)
- **MEDIUM**: 35 tasks (Groups 4, 5, 6)
- **LOW**: 8 tasks (Group 7)
- **CONTINUOUS**: 4 tasks (Group 8)

### By Duration
- **Total Tasks**: 62
- **Estimated Hours**: 210h
- **Estimated Days**: 26 days (1 developer)
- **Estimated Weeks**: 5.2 weeks

### Critical Path
1. Task Group 1 (Foundation) - 3 days
2. Task Group 2 (Pickers) - 4 days (parallel with Group 3)
3. Task Group 3 (Input) - 3 days (parallel with Group 2)
4. Task Groups 4-7 (All Dialogs) - 16 days (partially parallel)
5. Task Group 8 (Integration) - 3 days

**Optimistic**: 20 days (with 2-3 developers, parallelization)
**Realistic**: 26 days (1 developer, sequential)
**Conservative**: 35 days (1 developer, buffer for issues)

---

*Task list will be updated as work progresses. Status changes should be committed regularly.*
