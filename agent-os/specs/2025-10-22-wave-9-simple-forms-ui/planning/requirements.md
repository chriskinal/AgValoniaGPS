# Wave 9: Simple Forms UI - Requirements Gathering

## Requirements Source
All requirements are derived from the **AgOpenGPS UI Extraction** tool analysis of the legacy Windows Forms application.

**Primary Documentation Location**: `AgValoniaGPS/UI_Extraction/`

**Extraction Date**: 2025-10-21
**Source Application**: AgOpenGPS v6.x
**Extraction Method**: Automated Roslyn-based static analysis (6 phases)

## Extraction Methodology

### Phase 1-5: Static Analysis (Already Complete)
1. **Phase 1**: Static UI structure from `*.Designer.cs` files
2. **Phase 2**: Dynamic behavior from `*.cs` code-behind files
3. **Phase 3**: Resources from `*.resx` files
4. **Phase 4**: Navigation analysis and form relationships
5. **Phase 5**: Documentation generation (Mermaid diagrams, migration guides)

### Phase 6: Dynamic Behavior Analysis (Already Complete)
6. **Phase 6**: Dynamic behavior patterns
   - 312 visibility rules extracted
   - 2,033 property changes tracked
   - 11 UI modes inferred
   - 25 state transitions identified

## Extracted Requirements

### 1. Form Inventory

#### Simple Forms (<100 controls) - 53 Total
Detailed in `AgValoniaGPS/UI_Extraction/UI-EXTRACTION-SUMMARY.md`

**Picker Dialogs** (6):
- FormColorPicker: 89 controls
- FormDrivePicker: 12 controls
- FormFilePicker: 57 controls
- FormRecordPicker: 20 controls
- FormLoadProfile: 37 controls
- FormNewProfile: 25 controls

**Input Dialogs** (2):
- FormKeyboard: 64 controls
- FormNumeric: 33 controls

**Utility Dialogs** (14):
- FormDialog: 8 controls
- Form_About: 90 controls
- Form_Help: 20 controls
- Form_Keys: 217 controls (exceeds simple threshold but structurally simple)
- FormEventViewer: 22 controls
- FormGPSData: 94 controls
- FormPan: 12 controls
- FormSaveOrNot: 16 controls
- FormSaving: 5 controls
- FormShiftPos: 29 controls
- FormTimedMessage: 9 controls
- FormWebCam: 5 controls
- Form_First: 74 controls
- FormAgShareSettings: 81 controls

**Field Management** (12):
- FormFieldDir: 35 controls
- FormFieldExisting: 48 controls
- FormFieldData: 40 controls
- FormFieldKML: 28 controls
- FormFieldISOXML: 35 controls
- FormBoundary: 27 controls
- FormBndTool: 20 controls
- FormBoundaryPlayer: 30 controls
- FormBuildBoundaryFromTracks: 25 controls
- FormFlags: 40 controls
- FormEnterFlag: 25 controls
- FormAgShareDownloader: 67 controls

**Guidance Dialogs** (9):
- FormABDraw: 117 controls (slightly over threshold but included)
- FormQuickAB: 167 controls (over threshold but simple pattern)
- FormSmoothAB: 35 controls
- FormGrid: 91 controls
- FormTram: 87 controls
- FormTramLine: 30 controls
- FormHeadLine: 60 controls
- FormHeadAche: 40 controls
- FormRecordName: 20 controls

**Settings Dialogs** (8):
- ConfigSummaryControl: 103 controls
- ConfigVehicleControl: 158 controls
- ConfigData: 0 controls (placeholder)
- ConfigHelp: 0 controls (placeholder)
- ConfigMenu: 0 controls (placeholder)
- ConfigModule: 0 controls (placeholder)
- ConfigTool: 0 controls (placeholder)
- ConfigVehicle: 0 controls (placeholder)

### 2. Control Mappings

From `AgValoniaGPS/UI_Extraction/AVALONIA-MIGRATION-GUIDE.md`:

| Windows Forms | Avalonia | Notes |
|---------------|----------|-------|
| Form | Window | Top-level windows |
| UserControl | UserControl | Reusable controls |
| Button | Button | Standard buttons |
| Label | TextBlock | Read-only text |
| TextBox | TextBox | Text input |
| CheckBox | CheckBox | Boolean input |
| RadioButton | RadioButton | Exclusive selection |
| ComboBox | ComboBox | Dropdown selection |
| ListBox | ListBox | List selection |
| NumericUpDown | NumericUpDown | Numeric input |
| Panel | Panel/StackPanel/Grid | Layout containers |
| GroupBox | Border | Visual grouping |
| PictureBox | Image | Image display |
| ProgressBar | ProgressBar | Progress indication |
| TabControl | TabControl | Tabbed interface |
| MenuStrip | Menu | Application menu |
| ToolStrip | ToolBar | Toolbar |
| ContextMenuStrip | ContextMenu | Right-click menu |

### 3. Dynamic Behavior Requirements

From `AgValoniaGPS/UI_Extraction/visibility-rules.md`:

**Top Forms by Visibility Rules**:
1. FormGPS.cs: 45 visibility rules
2. FormConfig.cs: 38 visibility rules
3. FormSteer.cs: 28 visibility rules
4. FormTram.cs: 22 visibility rules
5. FormBuildTracks.cs: 18 visibility rules

**Sample Visibility Patterns**:
```
# Legacy Pattern
if (isJobStarted) {
    btnStart.Visible = false;
    btnStop.Visible = true;
}

# Avalonia MVVM Pattern
<Button IsVisible="{Binding !IsJobStarted}" />
<Button IsVisible="{Binding IsJobStarted}" />
```

### 4. Property Change Requirements

From `AgValoniaGPS/UI_Extraction/property-changes.md`:

**Property Changes by Type**:
- Text: 847 changes
- BackColor: 412 changes
- Enabled: 298 changes
- Image: 187 changes
- ForeColor: 156 changes
- Checked: 89 changes
- Font: 44 changes

**Conversion Strategy**:
- Text changes → Computed properties
- Color changes → Value converters
- Enabled changes → Boolean bindings
- Image changes → Resource bindings
- Checked changes → Two-way bindings

### 5. UI Mode Requirements

From `AgValoniaGPS/UI_Extraction/ui-state-machine.md`:

**Inferred UI Modes** (11 total):
1. JobActive - Job/work in progress
2. AutoSteerActive - Auto-steering engaged
3. BoundaryMode - Boundary editing
4. GuidanceMode - Guidance line active
5. FieldOpen - Field loaded
6. panelTractorBrandsMode - Vehicle type selection
7. btnMoveDnMode - Pan control mode
8. btnABDrawMode - AB line drawing mode
9. lblTotalAreaMode - Area display mode
10. (Additional modes in documentation)

**State Transitions** (25 identified):
- Mode changes triggered by user actions
- Button clicks, checkbox changes
- Field load/unload events
- Hardware connection status

### 6. Navigation Requirements

From `AgValoniaGPS/UI_Extraction/navigation-graph.md`:

**Navigation Pattern**:
- 16 total form navigations
- 64 entry points (can be opened directly)
- 8 forms with child dialogs
- Modal dialog pattern (most dialogs)
- Tool window pattern (some utility dialogs)

**Dialog Launch Patterns**:
- From main menu items
- From toolbar buttons
- From button clicks in forms
- From context menus

### 7. Event Handler Requirements

**Total Event Handlers**: 670 across all forms

**Common Event Types**:
- Button Click
- TextBox TextChanged
- CheckBox CheckedChanged
- ComboBox SelectedIndexChanged
- Form Load
- Form Closing
- Timer Tick
- Key Press/Down/Up

**Conversion to Commands**:
- Click events → ICommand
- Value changed events → Property setters with RaisePropertyChanged
- Form lifecycle → ViewModel lifecycle methods

### 8. Validation Requirements

**Input Validation Patterns**:
- Numeric range validation
- Required field validation
- File path validation
- Coordinate validation
- Unique name validation
- Cross-field validation

**Error Display**:
- Visual indication (red border, error icon)
- Tooltip with error message
- Prevent OK until valid

### 9. Localization Requirements

**Resource Strings**: 1 string resource found in extraction

**Requirements**:
- All UI text in resource files
- Support for multiple languages
- Culture-specific formatting (dates, numbers)

### 10. Accessibility Requirements

**Touch Requirements**:
- Minimum button size: 44x44 pixels
- Touch-friendly spacing
- Large input controls
- Virtual keyboard support

**Keyboard Navigation**:
- Tab order logical
- All controls accessible via keyboard
- Keyboard shortcuts for common actions
- Enter/Escape for OK/Cancel

**Screen Reader**:
- All controls have accessible names
- Error messages announced
- State changes announced

## Non-Functional Requirements

### Performance
- **Dialog Open**: <100ms from command to display
- **Input Response**: <50ms for user input
- **Memory**: <50MB for all dialogs combined
- **No Memory Leaks**: Proper disposal of resources

### Reliability
- **Error Handling**: Graceful degradation on errors
- **Data Validation**: Prevent invalid input
- **State Consistency**: UI reflects actual state
- **Thread Safety**: No UI thread blocking

### Usability
- **Consistent UX**: Same patterns across all dialogs
- **Clear Feedback**: Visual response to actions
- **Undo/Redo**: Where applicable
- **Help/Tooltips**: Guidance for complex controls

### Maintainability
- **MVVM Pattern**: Consistent architecture
- **Testable**: 100% ViewModel unit test coverage
- **Documented**: XML docs for all public APIs
- **Clean Code**: Follow C# coding standards

## Requirements Traceability

### Source Documents
1. **ui-structure.json** (3.5 MB) - Machine-readable form data
2. **UI-EXTRACTION-SUMMARY.md** - Human-readable summary
3. **visibility-rules.md** - 312 visibility patterns
4. **property-changes.md** - 2,033 property changes
5. **ui-state-machine.md** - 11 modes, 25 transitions
6. **avalonia-mvvm-guide.md** - Conversion patterns
7. **AVALONIA-MIGRATION-GUIDE.md** - Control mappings
8. **navigation-graph.md** - Form relationships

### Requirements Categories
| Category | Source Document | Count |
|----------|----------------|-------|
| Forms | UI-EXTRACTION-SUMMARY.md | 53 |
| Controls | ui-structure.json | 10,774 |
| Event Handlers | ui-structure.json | 670 |
| Visibility Rules | visibility-rules.md | 312 |
| Property Changes | property-changes.md | 2,033 |
| UI Modes | ui-state-machine.md | 11 |
| State Transitions | ui-state-machine.md | 25 |
| Control Mappings | AVALONIA-MIGRATION-GUIDE.md | 18 |

## Requirements Validation

### Completeness Check
- ✅ All simple forms identified
- ✅ Control inventory complete
- ✅ Event handlers documented
- ✅ Dynamic behavior analyzed
- ✅ Navigation patterns extracted
- ✅ Conversion patterns defined

### Consistency Check
- ✅ No conflicting requirements
- ✅ Control mappings consistent
- ✅ MVVM patterns aligned
- ✅ Naming conventions established

### Feasibility Check
- ✅ All controls available in Avalonia
- ✅ MVVM patterns proven
- ✅ Performance targets achievable
- ✅ Timeline realistic

## Assumptions

1. **Avalonia Capabilities**: All required controls exist or can be created
2. **Service Availability**: Wave 1-8 services are stable and complete
3. **Development Environment**: Rider/VS with Avalonia designer available
4. **Testing Tools**: xUnit and Moq available for testing
5. **Platform Support**: Windows, Linux, Android (iOS deferred)

## Constraints

1. **Legacy Compatibility**: Must maintain functional parity with legacy UI
2. **Cross-Platform**: Must work identically on Windows, Linux, Android
3. **Performance**: Must meet 100ms dialog open target
4. **Architecture**: Must follow MVVM with ReactiveUI
5. **Testing**: Must achieve >90% code coverage

## Dependencies

1. **Wave 1-8 Services**: All backend services must be complete and stable
2. **UI Extraction**: Documentation must be complete ✅
3. **Avalonia UI**: Version 11.3.6 or later ✅
4. **Development Tools**: Rider/VS with Avalonia support ✅

## Open Questions

1. **Touch Screen Testing**: What hardware available for testing?
   - **Resolution**: Use Windows tablet or Android emulator

2. **Localization Strategy**: When to add multi-language support?
   - **Resolution**: English only for Wave 9, localization in later wave

3. **Theme Support**: Light/dark theme requirements?
   - **Resolution**: Support both, test both continuously

4. **Help System**: How to integrate help documentation?
   - **Resolution**: Simple help dialogs for Wave 9, full help system later

## Sign-Off

**Requirements Gathered By**: UI Extraction Tool (automated)
**Requirements Reviewed By**: TBD
**Requirements Approved By**: TBD
**Date**: 2025-10-22
**Status**: ✅ COMPLETE - Ready for implementation

---

*All requirements are traceable to source documents in `AgValoniaGPS/UI_Extraction/`. Any new requirements discovered during implementation should be documented here.*
