# Specification: Wave 9 - Simple Forms UI

## Goal
Implement 53 simple Avalonia UI forms (<100 controls each) using MVVM pattern with reactive bindings, following extracted requirements from legacy AgOpenGPS Windows Forms application. Establish UI patterns, reusable controls, and MVVM architecture that will be leveraged in Waves 10-11 for moderate and complex forms.

## User Stories
- As a farmer, I want to pick files and folders using modern dialogs so that field and data management is intuitive
- As a farmer, I want to enter numeric values using a touch-friendly on-screen keyboard so that I can operate the system without a physical keyboard
- As a farmer, I want to pick colors for display elements so that I can customize the UI to my preferences
- As a farmer, I want to manage field names and metadata through simple dialogs so that my fields are organized
- As a developer, I want consistent dialog patterns across the application so that maintenance is simplified
- As a developer, I want reusable controls and converters so that UI development is efficient
- As a user, I want responsive, touch-friendly controls so that the application works well on tablets and touch displays

## Core Requirements

### Functional Requirements

#### Dialog Categories (53 Simple Forms)
1. **Picker Dialogs** (6 forms)
   - FormColorPicker - Color selection with preview
   - FormDrivePicker - Drive/volume selection
   - FormFilePicker - File browser dialog
   - FormRecordPicker - Record selection from lists
   - FormLoadProfile - Profile loading dialog
   - FormNewProfile - Profile creation dialog

2. **Input Dialogs** (2 forms)
   - FormKeyboard - Virtual keyboard for text input
   - FormNumeric - Numeric keypad for number entry

3. **Simple Settings** (8 forms)
   - ConfigData, ConfigHelp, ConfigMenu, ConfigModule, ConfigTool, ConfigVehicle - Empty/placeholder forms
   - ConfigSummaryControl - 103 controls, settings summary
   - ConfigVehicleControl - 158 controls, vehicle setup

4. **Field Management** (12 forms)
   - FormAgShareDownloader - AgShare field downloads
   - FormBndTool - Boundary editing tools
   - FormBoundary - Boundary management
   - FormBoundaryPlayer - Boundary playback
   - FormBuildBoundaryFromTracks - Track-to-boundary conversion
   - FormEnterFlag - Flag/marker entry
   - FormFieldData - Field metadata editor
   - FormFieldDir - Field directory browser
   - FormFieldExisting - Existing field loader
   - FormFieldISOXML - ISOXML import
   - FormFieldKML - KML import
   - FormFlags - Flag management

5. **Utility Dialogs** (14 forms)
   - FormAgShareSettings - AgShare configuration
   - FormDialog - Generic dialog wrapper
   - FormEventViewer - Event log viewer
   - FormGPSData - GPS data display
   - FormJob - Job/task management
   - FormMap - Map selection
   - FormPan - Manual panning control
   - FormRecordName - Record naming dialog
   - FormSaveOrNot - Save confirmation
   - FormSaving - Save progress indicator
   - FormShiftPos - Position shift tool
   - FormTimedMessage - Timed notification
   - FormWebCam - Camera feed display
   - Form_About - About dialog
   - Form_First - First-run wizard/terms
   - Form_Help - Help documentation
   - Form_Keys - Keyboard shortcuts reference

6. **Guidance Dialogs** (11 forms)
   - FormABDraw - AB line drawing interface
   - FormGrid - Grid guidance setup
   - FormHeadAche - Headland management
   - FormHeadLine - Headline creation
   - FormQuickAB - Quick AB line creation
   - FormRecordName - Recording naming
   - FormSmoothAB - AB line smoothing
   - FormTram - Tram line configuration
   - FormTramLine - Tram line editor

### UI Architecture Requirements
- **MVVM Pattern**: All forms use ViewModel with ReactiveUI
- **Dependency Injection**: ViewModels registered in DI container
- **Data Binding**: Two-way bindings for all user inputs
- **Value Converters**: Reusable converters for visibility, colors, units
- **Command Pattern**: All user actions use ICommand
- **Validation**: Input validation with visual feedback
- **Localization**: i18n-ready with resource strings

### Dynamic Behavior Requirements
Based on `AgValoniaGPS/UI_Extraction/` analysis:
- **312 Visibility Rules**: Implement conditional control visibility using bindings
- **2,033 Property Changes**: Convert imperative property changes to reactive computed properties
- **11 UI Modes**: Implement mode-based visibility through ViewModel states
- **State Variables → ViewModel Properties**: Convert 47 state variables to observable properties

### Non-Functional Requirements
- **Performance**: Dialog open <100ms, input response <50ms
- **Touch-Friendly**: All controls minimum 44x44 pixels, appropriate spacing
- **Responsive**: Support screen sizes from 800x600 to 4K
- **Cross-Platform**: Windows, Linux, Android UI consistency
- **Accessibility**: Keyboard navigation, screen reader support
- **Theme Support**: Light/dark themes, custom color schemes
- **Unit Testing**: 100% ViewModel test coverage

## Visual Design

### Mockup References
All visual mockups extracted to:
- `AgValoniaGPS/UI_Extraction/ui-documentation.html` - Interactive documentation
- `AgValoniaGPS/UI_Extraction/ui-structure.json` - Machine-readable form data
- `SourceCode/GPS/Forms/` - Original WinForms Designer files

### UI Patterns from Legacy System

#### Dialog Patterns
1. **Modal Dialogs**: Standard OK/Cancel pattern
2. **Tool Windows**: Non-modal, stay-on-top dialogs
3. **Picker Dialogs**: List/grid selection with search
4. **Numeric Input**: Calculator-style numpad with decimal support
5. **Color Picker**: Palette selection with RGB preview

#### Control Patterns
From `AgValoniaGPS/UI_Extraction/AVALONIA-MIGRATION-GUIDE.md`:
- Windows Forms Button → Avalonia Button
- Windows Forms TextBox → Avalonia TextBox
- Windows Forms Label → Avalonia TextBlock
- Windows Forms CheckBox → Avalonia CheckBox
- Windows Forms ComboBox → Avalonia ComboBox
- Windows Forms NumericUpDown → Avalonia NumericUpDown
- Windows Forms ListBox → Avalonia ListBox
- Windows Forms Panel → Avalonia Panel/StackPanel/Grid

### Dynamic Behavior Conversion
From `AgValoniaGPS/UI_Extraction/avalonia-mvvm-guide.md`:

**Legacy Pattern**:
```csharp
// Windows Forms - Imperative
private bool isJobStarted = false;
void UpdateUI() {
    btnStart.Visible = isJobStarted;
    btnStop.Visible = !isJobStarted;
    btnStart.Text = isJobStarted ? "Stop" : "Start";
}
```

**Avalonia MVVM Pattern**:
```csharp
// ViewModel - Reactive
private bool _isJobStarted;
public bool IsJobStarted
{
    get => _isJobStarted;
    set
    {
        this.RaiseAndSetIfChanged(ref _isJobStarted, value);
        this.RaisePropertyChanged(nameof(StartButtonText));
    }
}
public string StartButtonText => IsJobStarted ? "Stop" : "Start";
public bool IsStopButtonVisible => IsJobStarted;
```

```xml
<!-- AXAML - Declarative -->
<Button Content="{Binding StartButtonText}" Command="{Binding ToggleJobCommand}" />
<Button Content="Stop" IsVisible="{Binding IsStopButtonVisible}" />
```

## Reusable Components

### Existing Code to Leverage

**ViewModel Base Classes** - Create for Wave 9:
- `ViewModelBase` with ReactiveUI's `ReactiveObject`
- `DialogViewModelBase` with OK/Cancel commands
- `PickerViewModelBase` with search and selection

**Value Converters** - Already exist in `AgValoniaGPS.Desktop/Converters/`:
- `BoolToColorConverter` - Boolean to color conversion
- `BoolToStatusConverter` - Boolean to status text
- `FixQualityToColorConverter` - GPS fix quality colors
- Need: `InverseBoolConverter`, `VisibilityConverter`, `UnitConverter`

**Service Integration** - Existing services to use:
- Wave 8 (State Management): IConfigurationService for settings
- Wave 8 (State Management): ISessionManagementService for current state
- Wave 5 (Field Operations): IFieldService for field data
- Wave 2 (Guidance): IABLineService, ICurveLineService for guidance data
- Wave 6 (Hardware I/O): IModuleCommunicationService for hardware status

**MainViewModel** - Existing at `AgValoniaGPS.ViewModels/MainViewModel.cs`:
- Already integrates Wave 1-8 services
- Dialog launching pattern to follow
- Service locator pattern for ViewModels

### New Components Required

**Dialog Service** - `IDialogService`:
- Show modal dialogs with result
- Show tool windows (non-modal)
- Message boxes (info, warning, error, confirm)
- File/folder picker dialogs
- Abstract platform-specific dialog implementations

**Reusable Custom Controls**:
- `NumericKeypad` - Touch-friendly number entry
- `VirtualKeyboard` - On-screen QWERTY keyboard
- `ColorPalette` - Color grid picker
- `UnitDisplay` - Display with unit conversion (metric/imperial)
- `TouchButton` - Large touch-optimized button
- `SearchableListBox` - List with search filter

**Value Converters to Create**:
- `InverseBoolConverter` - Negate boolean for visibility
- `VisibilityConverter` - Boolean to IsVisible
- `NullToVisibilityConverter` - Null checking
- `UnitConverter` - Metric/Imperial conversion
- `EmptyStringToVisibilityConverter` - Show/hide on empty

**Validation Rules**:
- Numeric range validation
- Required field validation
- File path validation
- Coordinate validation
- Custom validation attributes

## Task Groups

### Task Group 1: Foundation & Architecture (HIGH PRIORITY)
**Goal**: Establish MVVM infrastructure and base classes

#### Tasks:
1. Create `ViewModelBase` with ReactiveUI integration
2. Create `DialogViewModelBase` with OK/Cancel pattern
3. Create `IDialogService` interface and implementation
4. Create required value converters (InverseBool, Visibility, Unit)
5. Set up ViewModel DI registration pattern
6. Create unit test infrastructure for ViewModels

**Deliverables**:
- Base classes in `AgValoniaGPS.ViewModels/Base/`
- Dialog service in `AgValoniaGPS.Services/UI/`
- Converters in `AgValoniaGPS.Desktop/Converters/`
- Test project setup `AgValoniaGPS.ViewModels.Tests/`

**Dependencies**: None (pure infrastructure)

---

### Task Group 2: Picker Dialogs (HIGH PRIORITY)
**Goal**: Implement 6 picker dialogs for file/data selection

#### Forms:
1. **FormColorPicker** (89 controls) - Color selection
2. **FormDrivePicker** (12 controls) - Drive selection
3. **FormFilePicker** (57 controls) - File browser
4. **FormRecordPicker** (20 controls) - Record selection
5. **FormLoadProfile** (37 controls) - Profile loader
6. **FormNewProfile** (25 controls) - Profile creator

#### Tasks per Form:
1. Create ViewModel with properties and commands
2. Create AXAML view with data bindings
3. Implement search/filter logic (where applicable)
4. Wire up to DialogService
5. Add unit tests for ViewModel
6. Manual UI testing

**Deliverables**:
- 6 ViewModels in `AgValoniaGPS.ViewModels/Dialogs/Pickers/`
- 6 Views in `AgValoniaGPS.Desktop/Views/Dialogs/Pickers/`
- 6 test suites in `AgValoniaGPS.ViewModels.Tests/Dialogs/Pickers/`

**Dependencies**: Task Group 1 (Foundation)

---

### Task Group 3: Input Dialogs (HIGH PRIORITY)
**Goal**: Implement touch-friendly input dialogs

#### Forms:
1. **FormKeyboard** (64 controls) - Virtual QWERTY keyboard
2. **FormNumeric** (33 controls) - Numeric keypad

#### Special Requirements:
- Large touch-optimized buttons (60x60 minimum)
- Visual feedback on press
- Support for backspace, clear, enter
- Decimal point handling for numeric
- Shift/caps lock for keyboard

#### Tasks:
1. Create `NumericKeypad` custom control
2. Create `VirtualKeyboard` custom control
3. Create FormKeyboard ViewModel
4. Create FormNumeric ViewModel
5. Create AXAML views with touch-friendly layout
6. Add unit tests

**Deliverables**:
- 2 custom controls in `AgValoniaGPS.Desktop/Controls/`
- 2 ViewModels in `AgValoniaGPS.ViewModels/Dialogs/Input/`
- 2 Views in `AgValoniaGPS.Desktop/Views/Dialogs/Input/`

**Dependencies**: Task Group 1 (Foundation)

---

### Task Group 4: Utility Dialogs (MEDIUM PRIORITY)
**Goal**: Implement 17 utility and information dialogs

#### Forms:
1. **FormDialog** (8 controls) - Generic dialog wrapper
2. **Form_About** (90 controls) - About/version info
3. **Form_Help** (20 controls) - Help documentation
4. **Form_Keys** (217 controls) - Keyboard shortcuts
5. **FormEventViewer** (22 controls) - Event log
6. **FormGPSData** (94 controls) - GPS data display
7. **FormPan** (12 controls) - Manual pan control
8. **FormSaveOrNot** (16 controls) - Save confirmation
9. **FormSaving** (5 controls) - Save progress
10. **FormShiftPos** (29 controls) - Position shifter
11. **FormTimedMessage** (9 controls) - Timed notification
12. **FormWebCam** (5 controls) - Camera feed
13. **Form_First** (74 controls) - First-run/terms
14. **FormAgShareSettings** (81 controls) - AgShare config

#### Tasks:
1. Group forms by similar patterns
2. Create ViewModels with shared base classes
3. Create AXAML views
4. Implement timer logic for FormTimedMessage
5. Implement webcam integration for FormWebCam
6. Add unit tests

**Deliverables**:
- 14 ViewModels in `AgValoniaGPS.ViewModels/Dialogs/Utility/`
- 14 Views in `AgValoniaGPS.Desktop/Views/Dialogs/Utility/`

**Dependencies**: Task Group 1, 2 (uses DialogService, picker dialogs)

---

### Task Group 5: Field Management Dialogs (MEDIUM PRIORITY)
**Goal**: Implement 12 field-related dialogs

#### Forms:
1. **FormFieldDir** (35 controls) - Field directory browser
2. **FormFieldExisting** (48 controls) - Load existing field
3. **FormFieldData** (40 controls) - Field metadata editor
4. **FormFieldKML** (28 controls) - KML import
5. **FormFieldISOXML** (35 controls) - ISOXML import
6. **FormBoundary** (27 controls) - Boundary management
7. **FormBndTool** (20 controls) - Boundary tools
8. **FormBoundaryPlayer** (30 controls) - Boundary playback
9. **FormBuildBoundaryFromTracks** (25 controls) - Track converter
10. **FormFlags** (40 controls) - Flag management
11. **FormEnterFlag** (25 controls) - Flag entry
12. **FormAgShareDownloader** (67 controls) - AgShare downloads

#### Integration Points:
- Wave 5 (Field Operations): IFieldService, IBoundaryManagementService
- Wave 8 (State Management): ISessionManagementService
- AgShare API (future)

#### Tasks:
1. Create ViewModels with field service integration
2. Create AXAML views with field visualization
3. Implement import/export logic
4. Wire up to existing field services
5. Add unit tests

**Deliverables**:
- 12 ViewModels in `AgValoniaGPS.ViewModels/Dialogs/Fields/`
- 12 Views in `AgValoniaGPS.Desktop/Views/Dialogs/Fields/`

**Dependencies**: Task Group 1, Wave 5 (Field Operations)

---

### Task Group 6: Guidance Dialogs (MEDIUM PRIORITY)
**Goal**: Implement 9 guidance-related dialogs

#### Forms:
1. **FormABDraw** (117 controls) - AB line drawing
2. **FormQuickAB** (167 controls) - Quick AB creation
3. **FormSmoothAB** (35 controls) - AB smoothing
4. **FormGrid** (91 controls) - Grid setup
5. **FormTram** (87 controls) - Tram line config
6. **FormTramLine** (30 controls) - Tram editor
7. **FormHeadLine** (60 controls) - Headline creation
8. **FormHeadAche** (40 controls) - Headland management
9. **FormRecordName** (20 controls) - Recording naming

#### Integration Points:
- Wave 2 (Guidance): IABLineService, ICurveLineService, IContourLineService
- Wave 5 (Field Operations): IHeadlandService, ITramLineService
- Wave 8 (State Management): IConfigurationService for guidance settings

#### Tasks:
1. Create ViewModels with guidance service integration
2. Create AXAML views with line visualization
3. Implement line creation/editing logic
4. Wire up to existing guidance services
5. Add unit tests

**Deliverables**:
- 9 ViewModels in `AgValoniaGPS.ViewModels/Dialogs/Guidance/`
- 9 Views in `AgValoniaGPS.Desktop/Views/Dialogs/Guidance/`

**Dependencies**: Task Group 1, Wave 2 (Guidance), Wave 5 (Field Operations)

---

### Task Group 7: Simple Settings Dialogs (LOW PRIORITY)
**Goal**: Implement 8 settings-related dialogs

#### Forms:
1. **ConfigSummaryControl** (103 controls) - Settings summary
2. **ConfigVehicleControl** (158 controls) - Vehicle setup
3. **ConfigData** (0 controls) - Placeholder
4. **ConfigHelp** (0 controls) - Placeholder
5. **ConfigMenu** (0 controls) - Placeholder
6. **ConfigModule** (0 controls) - Placeholder
7. **ConfigTool** (0 controls) - Placeholder
8. **ConfigVehicle** (0 controls) - Placeholder

#### Integration Points:
- Wave 8 (State Management): IConfigurationService for all settings
- Wave 1 (Vehicle): VehicleConfiguration model

#### Tasks:
1. Create ViewModels bound to ConfigurationService
2. Create AXAML views with settings display
3. Implement two-way bindings for settings
4. Add validation for setting changes
5. Add unit tests

**Deliverables**:
- 8 ViewModels in `AgValoniaGPS.ViewModels/Dialogs/Settings/`
- 8 Views in `AgValoniaGPS.Desktop/Views/Dialogs/Settings/`

**Dependencies**: Task Group 1, Wave 8 (State Management)

---

### Task Group 8: Integration & Testing (CONTINUOUS)
**Goal**: Integrate all dialogs with MainViewModel and test

#### Tasks:
1. Update MainViewModel with dialog launching methods
2. Create integration tests for dialog workflows
3. Manual testing on Windows, Linux
4. Touch testing on tablet/touch display
5. Performance profiling (dialog open times)
6. Memory leak testing
7. Accessibility testing (keyboard navigation, screen reader)

**Deliverables**:
- Updated MainViewModel
- Integration test suite
- Performance benchmarks
- Test results document

**Dependencies**: All task groups

## Integration Points

### Service Dependencies
- **IConfigurationService** (Wave 8): All settings-related dialogs
- **ISessionManagementService** (Wave 8): Current state for dialogs
- **IFieldService** (Wave 5): Field management dialogs
- **IBoundaryManagementService** (Wave 5): Boundary dialogs
- **IABLineService, ICurveLineService** (Wave 2): Guidance dialogs
- **IHeadlandService, ITramLineService** (Wave 5): Headland/tram dialogs
- **IDialogService** (Wave 9 new): All dialogs

### Event Flow
1. User clicks button in MainWindow
2. MainViewModel command executes
3. DialogService.ShowDialog<TViewModel>()
4. Dialog ViewModel initialized with services
5. User interacts with dialog
6. OK/Cancel command executes
7. DialogService returns result
8. MainViewModel updates based on result

## Testing Strategy

### Unit Tests (Required)
- Every ViewModel must have unit tests
- Test all properties and commands
- Test validation logic
- Test computed property updates
- Mock service dependencies
- Aim for 100% code coverage

### Integration Tests
- Test dialog service integration
- Test service dependencies
- Test data binding workflows
- Test validation with real services

### Manual Testing Checklist
- Dialog opens correctly
- All controls visible and functional
- Keyboard navigation works
- Touch targets appropriate size
- Data binding works both ways
- Validation shows errors correctly
- OK/Cancel behavior correct
- Dialog closes properly
- Memory cleanup verified

### Performance Tests
- Dialog open time <100ms
- Input response <50ms
- Memory usage acceptable
- No memory leaks on close

## Acceptance Criteria

### Per-Dialog Criteria
- [ ] ViewModel created with ReactiveUI
- [ ] AXAML view created with bindings
- [ ] All controls bound to ViewModel properties
- [ ] Commands implemented for user actions
- [ ] Validation implemented where needed
- [ ] Unit tests passing (>90% coverage)
- [ ] Manual testing completed
- [ ] No memory leaks
- [ ] Dialog service integration working

### Wave 9 Completion Criteria
- [ ] All 53 simple forms implemented
- [ ] Dialog service fully functional
- [ ] All reusable controls created
- [ ] All value converters created
- [ ] 100% unit test coverage for ViewModels
- [ ] All integration tests passing
- [ ] Performance benchmarks met
- [ ] Documentation completed
- [ ] Code reviewed
- [ ] No critical bugs

### Quality Gates
- Zero build warnings
- All unit tests passing
- All integration tests passing
- Code coverage >90%
- Performance metrics met
- Cross-platform testing completed
- Accessibility tested

## Success Metrics

### Code Quality
- 0 build warnings
- 100% test pass rate
- >90% code coverage
- 0 critical bugs
- 0 high-severity bugs

### Performance
- Dialog open <100ms average
- Input response <50ms
- Memory usage <50MB for all dialogs
- No memory leaks detected

### Development Velocity
- 2-3 simple dialogs per day
- ~20 working days for all 53 forms
- 80% time on implementation, 20% on testing

## Risk Mitigation

### Technical Risks
- **Complex data bindings**: Use established ReactiveUI patterns
- **Performance issues**: Profile early, optimize hot paths
- **Cross-platform differences**: Test on all platforms continuously
- **Touch compatibility**: Design with touch-first mindset

### Project Risks
- **Scope creep**: Stick to simple forms, defer complex features
- **Timeline slippage**: Parallel development where possible
- **Integration issues**: Early integration with existing services

## Documentation Requirements

### Code Documentation
- XML documentation for all public APIs
- Comments for complex binding logic
- README for custom controls

### Developer Documentation
- MVVM patterns guide
- Dialog service usage guide
- Custom control documentation
- Testing guide

### User Documentation
- Not required for Wave 9 (internal dialogs)

## Migration from Legacy

### Conversion Checklist (per form)
1. Review legacy Designer.cs and code-behind
2. Extract properties and event handlers
3. Design ViewModel with properties
4. Convert event handlers to commands
5. Convert visibility rules to bindings
6. Convert property changes to computed properties
7. Create AXAML view
8. Test with mock data
9. Integrate with real services
10. Final testing

### Tools and References
- `AgValoniaGPS/UI_Extraction/ui-structure.json` - Form metadata
- `AgValoniaGPS/UI_Extraction/visibility-rules.md` - Visibility patterns
- `AgValoniaGPS/UI_Extraction/property-changes.md` - Property patterns
- `AgValoniaGPS/UI_Extraction/avalonia-mvvm-guide.md` - Conversion guide

## Timeline Estimate

### By Task Group
- **Group 1 (Foundation)**: 3 days
- **Group 2 (Pickers)**: 4 days
- **Group 3 (Input)**: 3 days
- **Group 4 (Utility)**: 7 days
- **Group 5 (Field Mgmt)**: 6 days
- **Group 6 (Guidance)**: 5 days
- **Group 7 (Settings)**: 4 days
- **Group 8 (Integration)**: 3 days (continuous)

**Total**: ~35 days (7 weeks) with 1 developer
**Parallel**: ~20 days with 2 developers (some tasks can be parallelized)

---

*This specification is a living document and will be updated as implementation progresses and requirements are refined.*
