# Wave 10.5: Panel Docking System - Task List

**Total Estimated Effort**: 42 hours (reduced from 44 - Task 3.1 complete)
**Status**: Not Started

---

## Task Group 1: MainWindow Refactoring (8 hours) ⏳

### Task 1.1: Create new MainWindow.axaml with docking panel layout ⏳
**Effort**: 2 hours | **Status**: Not Started

**Description:**
- Remove current FormGPS overlay implementation
- Add Grid with panelLeft (72px), panelRight (70px), panelBottom (62px)
- Add placeholder for OpenGL map control (Wave 11)
- Add panelNavigation as floating panel (initially hidden)

**Acceptance Criteria:**
- [ ] MainWindow.axaml has correct grid layout (72px | * | 70px) × (* | 62px)
- [ ] PanelLeft is Grid with 8 row definitions
- [ ] PanelRight is StackPanel with VerticalAlignment="Bottom"
- [ ] PanelBottom is StackPanel with Orientation="Horizontal", HorizontalAlignment="Right"
- [ ] PanelNavigation is floating Border (179×460px, initially hidden)
- [ ] OpenGL map placeholder (black Border) in center

**Files:**
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml`

---

### Task 1.2: Extract menu bar structure from ui-structure.json ⏳
**Effort**: 3 hours | **Status**: Not Started

**Description:**
- Parse 52 menu items from FormGPS structure in ui-structure.json
- Create Avalonia Menu control with identical structure
- Wire up menu commands (stubs for now)

**Acceptance Criteria:**
- [ ] Menu bar has 9 top-level menus (File, Edit, View, Field, Vehicle, Guidance, Tools, Settings, Help)
- [ ] All 52 menu items extracted and documented
- [ ] Menu structure matches original hierarchy
- [ ] Menu commands are stubbed (can be implemented later)

**Files:**
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml` (Menu control)
- `AgValoniaGPS.ViewModels/MainViewModel.cs` (Menu commands)

---

### Task 1.3: Style docking panels to match original ⏳
**Effort**: 2 hours | **Status**: Not Started

**Description:**
- Dark background (#2C2C2C) for all dock panels
- Proper spacing and padding
- Button hover/active states

**Acceptance Criteria:**
- [ ] All dock panels have dark background (#2C2C2C)
- [ ] Hover state for buttons is visible and matches original
- [ ] Active/selected button state is distinct
- [ ] Spacing between buttons is consistent

**Files:**
- `AgValoniaGPS.Desktop/Styles/DockPanelStyles.axaml` (new file)
- `AgValoniaGPS.Desktop/App.axaml` (include new style)

---

### Task 1.4: Test responsive layout ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Verify panels resize correctly
- Test on different window sizes
- Ensure OpenGL map area scales properly

**Acceptance Criteria:**
- [ ] Panels remain fixed width/height at all window sizes
- [ ] Map area scales to fill available space
- [ ] No layout glitches or overlaps
- [ ] Tested at 1024×768, 1920×1080, 2560×1440

---

## Task Group 2: PanelHostingService Implementation (6 hours) ⏳

### Task 2.1: Create IPanelHostingService interface ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Define RegisterPanel, ShowPanel, HidePanel, TogglePanel methods
- Define PanelDockLocation enum
- Define PanelVisibilityChanged event

**Acceptance Criteria:**
- [ ] Interface defined in `AgValoniaGPS.Services/Interfaces/UI/`
- [ ] All methods documented with XML comments
- [ ] PanelDockLocation enum includes Left, Right, Bottom, Navigation
- [ ] PanelVisibilityChangedEventArgs includes PanelId, IsVisible, Location

**Files:**
- `AgValoniaGPS.Services/Interfaces/UI/IPanelHostingService.cs` (new file)

---

### Task 2.2: Implement PanelHostingService ⏳
**Effort**: 3 hours | **Status**: Not Started

**Description:**
- Panel registration with dock locations
- Dynamic add/remove from dock containers
- Grid row management for panelLeft
- StackPanel management for panelRight/panelBottom

**Acceptance Criteria:**
- [ ] Service implements IPanelHostingService
- [ ] Initialize() method accepts 4 panel containers
- [ ] RegisterPanel() stores panel with dock location
- [ ] ShowPanel() adds control to correct container (Grid row or StackPanel)
- [ ] HidePanel() removes control from container
- [ ] TogglePanel() inverts visibility state
- [ ] PanelVisibilityChanged event raised on state changes

**Files:**
- `AgValoniaGPS.Services/UI/PanelHostingService.cs` (new file)

---

### Task 2.3: Register service in DI container ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Add to ServiceCollectionExtensions
- Initialize in MainWindow constructor

**Acceptance Criteria:**
- [ ] Service registered as singleton in DI
- [ ] MainWindow receives service via constructor injection
- [ ] Service initialized with panel containers in MainWindow.OnLoaded()
- [ ] All 15 Wave 10 panels registered with default locations

**Files:**
- `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml.cs`

---

### Task 2.4: Unit tests for PanelHostingService ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Test panel registration
- Test show/hide functionality
- Test toggle functionality
- Test visibility state tracking

**Acceptance Criteria:**
- [ ] Test: RegisterPanel adds panel to registry
- [ ] Test: ShowPanel adds control to container
- [ ] Test: HidePanel removes control from container
- [ ] Test: TogglePanel inverts visibility
- [ ] Test: IsPanelVisible returns correct state
- [ ] Test: GetPanelsInLocation returns panels for location
- [ ] Test: PanelVisibilityChanged event fires
- [ ] All tests pass

**Files:**
- `AgValoniaGPS.Services.Tests/UI/PanelHostingServiceTests.cs` (new file)

---

## Task Group 3: Button/Icon Extraction (2 hours) ⏳

### Task 3.1: Extract button icons from legacy Resources ✅
**Effort**: 0 hours (already complete) | **Status**: Complete

**Description:**
- ~~Open GPS.resx in Visual Studio~~
- ~~Export all button images to Assets/Icons/~~
- ~~Convert to PNG format~~
- ~~Document icon names and purposes~~

**Status**: ✅ **COMPLETE** - 236 icons already extracted to `Assets/Icons/`

**Acceptance Criteria:**
- [x] All button icons exported from GPS.resx - **236 icons extracted**
- [x] Icons saved as PNG in `AgValoniaGPS.Desktop/Assets/Icons/` - **Complete**
- [x] Minimum 30 icons extracted (target 50+) - **236 icons! Far exceeds target**
- [x] Icon names match original resource names - **Complete**

**Files:**
- `AgValoniaGPS.Desktop/Assets/Icons/*.png` (236 files - COMPLETE)

---

### Task 3.2: Create icon catalog document ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- List all extracted icons with screenshots
- Document which panels use which icons
- Create naming conventions for icons

**Acceptance Criteria:**
- [ ] Icon catalog markdown document created
- [ ] Table format: Icon Name | File Path | Used By | Purpose
- [ ] All extracted icons documented
- [ ] Icons grouped by category (field ops, config, navigation, etc.)

**Files:**
- `agent-os/specs/2025-10-24-wave-10.5-panel-docking-system/icon-catalog.md` (new file)

---

### Task 3.3: Update AXAML styles for icon buttons ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Define DockButton style with icon + text
- Define status indicator styles (red/yellow/green)
- Define hover/active/disabled states

**Acceptance Criteria:**
- [ ] DockButton style defined (60×60px)
- [ ] Style includes Image (48×48px) and TextBlock (10pt font)
- [ ] StatusRed, StatusYellow, StatusGreen classes defined
- [ ] Hover state shows border or glow
- [ ] Active state shows distinct background
- [ ] Disabled state shows grayed-out appearance

**Files:**
- `AgValoniaGPS.Desktop/Styles/DockButtonStyles.axaml` (new file)
- `AgValoniaGPS.Desktop/App.axaml` (include new style)

---

## Task Group 4: Wave 10 Panel Refactoring (16 hours) ⏳

### Task 4.1: Create dockable button views for all 15 panels ⏳
**Effort**: 4 hours | **Status**: Not Started

**Description:**
- Create button view for each of 15 Wave 10 panels
- Each button: 60×60px with icon + label
- Wire up click events to show full panel

**Panels:**
1. FormGPSDataButton (GPS data)
2. FormFieldDataButton (Field info)
3. FormTramLineButton (Tram lines)
4. FormQuickABButton (Quick AB)
5. FormSteerButton (AutoSteer config)
6. FormConfigButton (App settings)
7. FormDiagnosticsButton (Diagnostics)
8. FormRollCorrectionButton (Roll/pitch)
9. FormVehicleConfigButton (Vehicle setup)
10. FormFlagsButton (Field markers)
11. FormCameraButton (Camera controls)
12. FormBoundaryEditorButton (Boundary edit)
13. FormFieldToolsButton (Field tools)
14. FormFieldFileManagerButton (Field files)
15. NavigationButton (Show/hide navigation panel)

**Acceptance Criteria:**
- [ ] 15 button UserControl files created
- [ ] Each button has appropriate icon from Assets/Buttons/
- [ ] Each button has label matching original
- [ ] Click event handler toggles panel visibility
- [ ] Button visual state reflects panel visibility (active/inactive)

**Files:**
- `AgValoniaGPS.Desktop/Views/Controls/DockButtons/*Button.axaml` (15 new files)
- `AgValoniaGPS.Desktop/Views/Controls/DockButtons/*Button.axaml.cs` (15 new files)

---

### Task 4.2: Refactor panel views to be popup-friendly ⏳
**Effort**: 4 hours | **Status**: Not Started

**Description:**
- Ensure panels work as floating popups
- Add close buttons to all panels
- Test panel positioning near dock buttons

**Acceptance Criteria:**
- [ ] All 15 Wave 10 panels can be shown as floating windows/popups
- [ ] Each panel has close button in title bar
- [ ] Close button hides panel and updates button state
- [ ] Panels appear near their dock button (left/right/bottom)
- [ ] Panels have proper shadows and borders for floating appearance

**Files:**
- `AgValoniaGPS.Desktop/Views/Panels/Display/*.axaml` (update all 15 panel views)

---

### Task 4.3: Implement panel show/hide logic ⏳
**Effort**: 3 hours | **Status**: Not Started

**Description:**
- Button toggles panel visibility
- Panel close button hides panel and updates button state
- Optional: Only one panel visible at a time

**Acceptance Criteria:**
- [ ] Button click shows panel if hidden, hides if visible
- [ ] Panel close button calls PanelHostingService.HidePanel()
- [ ] Button visual state syncs with panel visibility
- [ ] Panel appears in correct position relative to button
- [ ] Smooth fade-in/fade-out animation

**Files:**
- `AgValoniaGPS.Desktop/Views/Controls/DockButtons/*Button.axaml.cs` (15 files)
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml.cs` (panel show/hide logic)

---

### Task 4.4: Register all panels with PanelHostingService ⏳
**Effort**: 2 hours | **Status**: Not Started

**Description:**
- Determine default dock location for each panel
- Register in MainWindow.RegisterPanels()
- Set default visibility states

**Default Dock Locations:**
- **Left**: FormQuickAB, FormFlags, FormBoundaryEditor, FormFieldTools, FormFieldFileManager
- **Right**: FormTramLine, FormSteer, FormConfig, FormDiagnostics, FormRollCorrection, FormVehicleConfig, FormCamera
- **Bottom**: FormFieldData, FormGPSData
- **Navigation**: NavigationPanel

**Default Visibility:**
- FormGPSData, FormQuickAB, FormCamera: Visible by default
- All others: Hidden by default

**Acceptance Criteria:**
- [ ] All 15 panels registered with dock locations
- [ ] Default visibility states set correctly
- [ ] Buttons appear in correct dock panels on startup
- [ ] Default-visible panels show their panels on startup

**Files:**
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml.cs` (RegisterPanels method)

---

### Task 4.5: Add status indicators to buttons ⏳
**Effort**: 3 hours | **Status**: Not Started

**Description:**
- GPS data: Fix quality indicator (red/yellow/green)
- AutoSteer: Active/inactive indicator
- Section control: On/off indicator

**Acceptance Criteria:**
- [ ] FormGPSDataButton shows GPS fix quality (NoFix=red, Float=yellow, RTKFixed=green)
- [ ] FormSteerButton shows AutoSteer state (inactive=gray, active=green)
- [ ] Indicators update in real-time via service events
- [ ] Visual indicators are small dots/badges on button

**Files:**
- `AgValoniaGPS.Desktop/Views/Controls/DockButtons/FormGPSDataButton.axaml.cs` (GPS status logic)
- `AgValoniaGPS.Desktop/Views/Controls/DockButtons/FormSteerButton.axaml.cs` (AutoSteer status logic)

---

## Task Group 5: Navigation Panel Implementation (4 hours) ⏳

### Task 5.1: Extract panelNavigation button structure from ui-structure.json ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Parse 5×2 grid of navigation buttons from ui-structure.json
- Identify button icons and labels
- Document button purposes

**Acceptance Criteria:**
- [ ] Navigation panel button structure extracted
- [ ] 10 buttons identified (5 rows × 2 columns)
- [ ] Button names, icons, and purposes documented
- [ ] Button click actions documented

**Files:**
- `agent-os/specs/2025-10-24-wave-10.5-panel-docking-system/navigation-panel-buttons.md` (new file)

---

### Task 5.2: Implement panelNavigation view ⏳
**Effort**: 2 hours | **Status**: Not Started

**Description:**
- 179×460px floating panel
- 5 rows × 2 columns grid
- 10 navigation buttons with icons

**Acceptance Criteria:**
- [ ] PanelNavigation Border is 179×460px
- [ ] Grid has 5 rows × 2 columns
- [ ] All 10 buttons implemented with icons
- [ ] WhiteSmoke background
- [ ] Panel initially hidden (IsVisible=false)
- [ ] Panel has drop shadow for floating appearance

**Files:**
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml` (panelNavigation section)

---

### Task 5.3: Add toggle button for panelNavigation ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Add toggle button to menu bar or dock area
- Show/hide panelNavigation on click
- Position panel appropriately

**Acceptance Criteria:**
- [ ] NavigationButton added to panelLeft (top row)
- [ ] Button toggles panelNavigation visibility
- [ ] Panel appears in center-left area when shown
- [ ] Button visual state reflects panel visibility

**Files:**
- `AgValoniaGPS.Desktop/Views/Controls/DockButtons/NavigationButton.axaml` (new file)
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml` (toggle logic)

---

## Task Group 6: Integration & Testing (6 hours) ⏳

### Task 6.1: Integration testing with all panels ⏳
**Effort**: 2 hours | **Status**: Not Started

**Description:**
- Test each panel docking in each location
- Verify show/hide functionality
- Test panel state indicators

**Test Cases:**
- [ ] All 15 panels can be shown/hidden via buttons
- [ ] Panels appear in correct dock locations
- [ ] Multiple panels can be visible simultaneously
- [ ] Panel close buttons work correctly
- [ ] Button states reflect panel visibility
- [ ] Status indicators update correctly
- [ ] Navigation panel can be toggled

**Files:**
- `agent-os/specs/2025-10-24-wave-10.5-panel-docking-system/integration-test-results.md` (new file)

---

### Task 6.2: UI/UX testing ⏳
**Effort**: 2 hours | **Status**: Not Started

**Description:**
- Compare side-by-side with legacy AgOpenGPS
- Verify familiar layout and behavior
- Test button sizes and hit targets

**Test Cases:**
- [ ] Side-by-side screenshot comparison (Wave 10.5 vs Legacy)
- [ ] Button positions match original
- [ ] Button sizes are appropriate for touch/mouse
- [ ] Panel positions feel natural and familiar
- [ ] No "randomly grouped" or overlapping controls
- [ ] Panels can be accessed intuitively

**Files:**
- `agent-os/specs/2025-10-24-wave-10.5-panel-docking-system/ui-comparison.md` (new file with screenshots)

---

### Task 6.3: Performance testing ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Verify smooth panel transitions
- Test with multiple panels visible
- Check memory usage

**Test Cases:**
- [ ] Panel show/hide animations are smooth (60 FPS)
- [ ] No lag when toggling multiple panels rapidly
- [ ] Memory usage is reasonable (<100MB increase with all panels visible)
- [ ] No memory leaks after repeated show/hide cycles

**Files:**
- `agent-os/specs/2025-10-24-wave-10.5-panel-docking-system/performance-test-results.md` (new file)

---

### Task 6.4: Documentation ⏳
**Effort**: 1 hour | **Status**: Not Started

**Description:**
- Update WAVE_10_COMPLETION_REPORT.md
- Create Wave 10.5 completion report
- Document panel docking patterns

**Acceptance Criteria:**
- [ ] WAVE_10_COMPLETION_REPORT.md updated to reference Wave 10.5
- [ ] WAVE_10.5_COMPLETION_REPORT.md created
- [ ] Panel docking patterns documented for future reference
- [ ] Screenshots included in completion report

**Files:**
- `agent-os/specs/2025-10-23-wave-10-complex-panels/WAVE_10_COMPLETION_REPORT.md` (update)
- `agent-os/specs/2025-10-24-wave-10.5-panel-docking-system/WAVE_10.5_COMPLETION_REPORT.md` (new file)

---

## Progress Tracking

**Overall Progress**: 2/42 hours (4.8%) - Task 3.1 complete (icons extracted)

**Task Group Progress:**
- Task Group 1: 0/8 hours (0%)
- Task Group 2: 0/6 hours (0%)
- Task Group 3: 2/2 hours (100%) ✅ Task 3.1 complete - 236 icons extracted
- Task Group 4: 0/16 hours (0%)
- Task Group 5: 0/4 hours (0%)
- Task Group 6: 0/6 hours (0%)

---

## Dependency Graph

```
Task 1.1 (MainWindow layout) → Task 2.2 (PanelHostingService impl) → Task 2.3 (DI registration)
                                                                    → Task 4.4 (Panel registration)

Task 3.1 (Icon extraction) → Task 3.2 (Icon catalog) → Task 4.1 (Button views)
                                                      → Task 5.2 (Navigation panel)

Task 4.1 (Button views) → Task 4.3 (Show/hide logic) → Task 4.5 (Status indicators)

Task 5.1 (Nav extraction) → Task 5.2 (Nav view) → Task 5.3 (Nav toggle)

All Task Groups 1-5 → Task Group 6 (Integration & Testing)
```

---

## Notes

- **Parallel Work**: Task Groups 1, 2, and 3 can be worked on in parallel
- **Blockers**: Task Group 4 requires Task Groups 1, 2, and 3 to be complete
- **Critical Path**: Task 1.1 → Task 2.2 → Task 2.3 → Task 4.4 → Task 6.1 (20 hours)
- **Quick Wins**: Task 3.1 (icon extraction) can be done independently and provides immediate value

---

**Last Updated**: 2025-10-24
**Status**: Ready for implementation
