# Wave 10: Moderate Forms UI - Specification

**Date**: 2025-10-23
**Wave**: 10
**Category**: UI Implementation (Moderate Complexity)
**Dependencies**: Wave 1-8 (backend services), Wave 9 (simple forms ViewModels)
**Priority**: High (Main operational interface)

---

## Executive Summary

Wave 10 implements 15 moderate-complexity forms (100-300 controls each) that integrate directly into the existing POC UI layout. These forms provide the core operational interface for field operations, guidance setup, configuration, and real-time monitoring. Unlike Wave 9's standalone dialogs, Wave 10 forms are integrated panels that fit within the established TOP/LEFT/RIGHT/BOTTOM/CENTER layout pattern.

**Key Objectives:**
- Build 15 operational interface forms (100-300 controls each)
- Integrate seamlessly with POC UI design system
- Provide real-time data visualization and configuration
- Ensure touch-friendly controls (≥48x48px)
- Achieve 100% test coverage with proper AXAML ↔ ViewModel binding

**Strategic Decision Context:**
- Wave 9 completed ViewModels but has 187 AXAML errors (deferred)
- Wave 10 delivers immediate operational value
- Focuses on main interface vs. supporting dialogs
- Establishes patterns for Wave 11 (complex forms)

---

## 1. Forms Inventory

### 1.1 Priority Tier 1: Core Operations (5 forms)

#### **FormGPS** - Main Field View (217 controls, actual count)
**Purpose**: Primary operational interface with OpenGL map
**Complexity**: 217 controls across 8 panels
**Layout**: Integrates into CENTER of POC UI (already present)
**Key Features**:
- OpenGL map control (vehicle, boundaries, guidance lines)
- Real-time position and heading display
- Guidance status indicators
- Section control visualization
- Camera controls (pan, zoom, pitch, follow)

**Control Distribution**:
- Buttons: 78
- Labels: 7
- Panels: 8 (panelDrag, panelSim, panelNavigation, panelLeft, flp1, panelBottom, panelRight, panelControlBox)
- Event Handlers: 135

**Integration Point**: Already in MainWindow CENTER, needs enhancement

---

#### **FormFieldData** - Field Statistics & Info (141 controls)
**Purpose**: Real-time field statistics and operation data
**Complexity**: 141 controls
**Layout**: TOP-RIGHT floating panel or RIGHT sidebar panel
**Key Features**:
- Current field name and area
- Distance traveled
- Area covered/remaining
- Average speed
- Time elapsed/remaining
- Work switch state
- Implement information

**Integration Point**: RIGHT panel expansion

---

#### **FormGPSData** - GPS Status & Details (138 controls)
**Purpose**: Detailed GPS data monitoring
**Complexity**: 138 controls
**Layout**: TOP-RIGHT floating panel (collapsible)
**Key Features**:
- Fix quality (No Fix, GPS, DGPS, RTK Float, RTK Fixed)
- Latitude, longitude, altitude
- Speed and heading
- HDOP, satellite count
- Age of correction
- NTRIP connection status
- Dual antenna data (if present)

**Integration Point**: TOP-RIGHT panel expansion

---

#### **FormTramLine** - Tram Line Configuration (120 controls)
**Purpose**: Configure tram lines (guide passes) for field operations
**Complexity**: 120 controls
**Layout**: LEFT sidebar panel
**Key Features**:
- Tram line mode (A+, A-, All)
- Pass interval (e.g., every 6th pass)
- Display width
- Left/right tram display
- Generate/clear tram lines

**Integration Point**: LEFT sidebar in guidance section

---

#### **FormQuickAB** - Quick A-B Line Setup (167 controls)
**Purpose**: Quick A-B line creation from current position
**Complexity**: 167 controls
**Layout**: LEFT sidebar panel
**Key Features**:
- Set A point (current position)
- Set B point (current heading + distance)
- Heading adjustment
- Line offset
- Apply/cancel buttons

**Integration Point**: LEFT sidebar in guidance section

---

### 1.2 Priority Tier 2: Configuration (5 forms)

#### **ConfigVehicleControl** - Vehicle Settings (158 controls)
**Purpose**: Vehicle configuration panel
**Complexity**: 158 controls
**Layout**: Settings dialog or CENTER panel
**Key Features**:
- Wheelbase, track width
- Antenna positions (front, rear)
- Implement dimensions
- Hitch settings
- Hydraulic settings

**Integration Point**: Replaces CENTER when opened from settings

---

#### **FormColorSection** - Section Color Configuration (191 controls)
**Purpose**: Configure colors for sections and coverage display
**Complexity**: 191 controls
**Layout**: Settings dialog
**Key Features**:
- Section color palette
- Coverage map colors
- Boundary colors
- Guidance line colors
- Day/night mode colors

**Integration Point**: Modal dialog over main interface

---

#### **FormSteerSettings** - Steering Parameters (Subset of FormSteer)
**Purpose**: Steering algorithm parameters
**Complexity**: ~150 controls (subset of 900-control FormSteer)
**Layout**: Settings dialog with tabs
**Key Features**:
- Pure Pursuit settings (look-ahead distance, gain)
- Stanley settings (gain, heading error weight)
- Min speed, max speed
- Aggressiveness factor
- U-turn settings

**Integration Point**: Modal dialog with 5 tabs

---

#### **FormSectionConfig** - Section Configuration (Subset of FormConfig)
**Purpose**: Section control setup
**Complexity**: ~150 controls (subset of 2133-control FormConfig)
**Layout**: Settings dialog with tabs
**Key Features**:
- Number of sections
- Section width
- Look-ahead distance
- Manual section control
- Section mapping

**Integration Point**: Modal dialog

---

#### **FormVehicleSetup** - Vehicle Geometry (Subset of FormConfig)
**Purpose**: Vehicle and implement geometry setup
**Complexity**: ~150 controls (subset of 2133-control FormConfig)
**Layout**: Settings dialog with tabs
**Key Features**:
- Wheelbase, track width
- Antenna positions
- Tool dimensions
- Hitch configuration
- Pivot settings

**Integration Point**: Modal dialog

---

### 1.3 Priority Tier 3: Field Management (5 forms)

#### **FormBndTool** - Boundary Tool (146 controls)
**Purpose**: Create and edit field boundaries
**Complexity**: 146 controls
**Layout**: RIGHT panel tool or modal dialog
**Key Features**:
- Draw boundary points
- Edit boundary points
- Delete boundary points
- Calculate field area
- Simplify boundary (Douglas-Peucker)
- Save/load boundary

**Integration Point**: RIGHT panel tool mode

---

#### **FormFieldDirectory** - Field Manager (Estimated 120 controls)
**Purpose**: Browse and manage fields
**Complexity**: ~120 controls
**Layout**: Modal dialog or CENTER panel
**Key Features**:
- Field list with thumbnails
- Search and filter
- Create new field
- Delete field
- Export field data
- Import field data

**Integration Point**: Modal dialog or CENTER replacement

---

#### **FormFieldNew** - New Field Creation (Estimated 130 controls)
**Purpose**: Create a new field
**Complexity**: ~130 controls
**Layout**: Modal dialog
**Key Features**:
- Field name input
- Field location (lat/lon or pick on map)
- Import boundary from file
- Draw boundary manually
- Set field flags
- Background image import

**Integration Point**: Modal dialog

---

#### **FormHeadland** - Headland Configuration (Estimated 110 controls)
**Purpose**: Configure field headlands
**Complexity**: ~110 controls
**Layout**: RIGHT panel tool or modal dialog
**Key Features**:
- Headland mode (Auto/Manual)
- Number of passes
- Pass width
- Turn type (Omega/T/Y)
- Generate/clear headland

**Integration Point**: RIGHT panel tool mode

---

#### **FormAgShareField** - AgShare Field Browser (Estimated 140 controls)
**Purpose**: Browse and download fields from AgShare
**Complexity**: ~140 controls
**Layout**: Modal dialog
**Key Features**:
- AgShare login
- Field browser
- Field preview
- Download field
- Upload field
- Sync status

**Integration Point**: Modal dialog

---

## 2. POC UI Integration Strategy

### 2.1 Layout Zones

**POC UI Structure** (from MainWindow.axaml):
```
┌─────────────────────────────────────────────────────┐
│ TOP: RTK Status (LEFT) + Quick Actions (RIGHT)     │ Z=10
├──────┬──────────────────────────────────┬───────────┤
│      │                                  │           │
│ LEFT │     CENTER: OpenGL Map           │   RIGHT   │
│ Side │     (Vehicle, Boundaries, etc.)  │   Panel   │ Z=0-9
│ bar  │                                  │   Tools   │
│      │                                  │           │
├──────┴──────────────────────────────────┴───────────┤
│ BOTTOM: Function Buttons (A-B, Contour, etc.)      │ Z=10
└─────────────────────────────────────────────────────┘
```

**Z-Index Layers**:
- Z=0: OpenGL map (background)
- Z=1: Transparent mouse overlay
- Z=10: Floating panels (non-modal)
- Z=20: Modal dialogs (when shown)

### 2.2 Integration Patterns

#### **Pattern 1: Embedded Panels** (FormGPS, FormFieldData, FormGPSData)
- Integrate directly into TOP/LEFT/RIGHT zones
- Use `FloatingPanel` style with semi-transparent background
- Collapsible/expandable
- Touch-friendly buttons (48x48px minimum)

#### **Pattern 2: Modal Dialogs** (ConfigVehicleControl, FormColorSection, FormSteerSettings)
- Overlay entire window at Z=20
- Semi-transparent dark overlay (#AA000000)
- Centered window with `FloatingPanel` style
- Close button and ESC key support

#### **Pattern 3: Contextual Tools** (FormBndTool, FormHeadland)
- RIGHT panel tool mode
- Activates when tool button clicked
- Shows tool-specific controls
- Integrates with map interaction

#### **Pattern 4: Full-Screen Replacement** (FormFieldDirectory)
- Replaces CENTER OpenGL map
- Full-screen panel with back button
- Used for data-heavy interfaces

### 2.3 Design System Reuse

**From POC UI (MainWindow.axaml styles)**:

```xml
<!-- Floating Panel Style -->
<Style Selector="Border.FloatingPanel">
    <Setter Property="Background" Value="#DD2C3E50"/>
    <Setter Property="CornerRadius" Value="12"/>
    <Setter Property="Padding" Value="15"/>
    <Setter Property="BoxShadow" Value="0 4 16 2 #40000000"/>
</Style>

<!-- Modern Button Style -->
<Style Selector="Button.ModernButton">
    <Setter Property="Background" Value="#3498DB"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="CornerRadius" Value="6"/>
    <Setter Property="Padding" Value="12,6"/>
</Style>

<!-- Icon Button Style -->
<Style Selector="Button.IconButton">
    <Setter Property="Background" Value="#DD34495E"/>
    <Setter Property="Width" Value="48"/>
    <Setter Property="Height" Value="48"/>
    <Setter Property="CornerRadius" Value="8"/>
</Style>

<!-- Status Indicator Style -->
<Style Selector="Border.StatusBox">
    <Setter Property="Background" Value="#DD1E2930"/>
    <Setter Property="CornerRadius" Value="8"/>
    <Setter Property="Padding" Value="10"/>
</Style>
```

**Color Palette**:
- Background: `#1a1a1a` (dark gray)
- Panel Background: `#DD2C3E50` (semi-transparent blue-gray)
- Primary Blue: `#3498DB` (buttons, highlights)
- Success Green: `#27AE60`
- Warning Orange: `#E67E22`
- Error Red: `#E74C3C`
- Text: `White` on dark backgrounds

---

## 3. ViewModel Architecture

### 3.1 Base Classes (Reuse from Wave 9)

#### **ViewModelBase** (from AgValoniaGPS.ViewModels/Base)
- Implements `INotifyPropertyChanged` via ReactiveUI
- `RaiseAndSetIfChanged` for property changes
- Base for all ViewModels

#### **PanelViewModelBase** (New for Wave 10)
- Extends `ViewModelBase`
- Adds panel visibility and expand/collapse logic
- Event: `CloseRequested` for panel dismissal
- Properties: `IsExpanded`, `IsPinned`, `CanCollapse`

#### **DialogViewModelBase** (Reuse from Wave 9)
- For modal dialogs
- Event: `CloseRequested<bool>` (true = OK, false = Cancel)
- Commands: `OKCommand`, `CancelCommand`
- Properties: `DialogResult`, `Title`

### 3.2 ViewModel Patterns

#### **Pattern 1: Service-Backed ViewModels**
ViewModels bind directly to Wave 1-8 services:

```csharp
public class FormGPSDataViewModel : PanelViewModelBase
{
    private readonly IGPSDataService _gpsService;
    private readonly INTRIPClientService _ntripService;

    // Bind to service properties
    public FixQuality FixQuality => _gpsService.FixQuality;
    public double Latitude => _gpsService.Latitude;
    public double Longitude => _gpsService.Longitude;

    public FormGPSDataViewModel(IGPSDataService gpsService, INTRIPClientService ntripService)
    {
        _gpsService = gpsService;
        _ntripService = ntripService;

        // Subscribe to service events
        _gpsService.FixQualityChanged += (s, e) =>
        {
            this.RaisePropertyChanged(nameof(FixQuality));
        };
    }
}
```

#### **Pattern 2: Reactive Commands**
Use ReactiveUI commands with observables:

```csharp
public class FormQuickABViewModel : PanelViewModelBase
{
    public ICommand SetPointACommand { get; }
    public ICommand SetPointBCommand { get; }
    public ICommand ApplyCommand { get; }

    private bool _canApply;
    public bool CanApply
    {
        get => _canApply;
        set => this.RaiseAndSetIfChanged(ref _canApply, value);
    }

    public FormQuickABViewModel(IABLineService abLineService)
    {
        SetPointACommand = ReactiveCommand.Create(OnSetPointA);
        SetPointBCommand = ReactiveCommand.Create(OnSetPointB);
        ApplyCommand = ReactiveCommand.Create(OnApply,
            this.WhenAnyValue(x => x.CanApply));
    }
}
```

#### **Pattern 3: Validation**
Use Wave 8 validation service:

```csharp
public class ConfigVehicleControlViewModel : DialogViewModelBase
{
    private readonly IValidationService _validationService;

    private double _wheelbase;
    public double Wheelbase
    {
        get => _wheelbase;
        set
        {
            if (_validationService.ValidateRange(value, 1.0, 10.0, "Wheelbase"))
            {
                this.RaiseAndSetIfChanged(ref _wheelbase, value);
            }
        }
    }
}
```

### 3.3 ViewModels List

1. **FormGPSViewModel** - Main view (OpenGL control wrapper)
2. **FormFieldDataViewModel** - Field statistics panel
3. **FormGPSDataViewModel** - GPS data panel
4. **FormTramLineViewModel** - Tram line configuration
5. **FormQuickABViewModel** - Quick A-B line setup
6. **ConfigVehicleControlViewModel** - Vehicle settings
7. **FormColorSectionViewModel** - Color configuration
8. **FormSteerSettingsViewModel** - Steering parameters
9. **FormSectionConfigViewModel** - Section control setup
10. **FormVehicleSetupViewModel** - Vehicle geometry
11. **FormBndToolViewModel** - Boundary tool
12. **FormFieldDirectoryViewModel** - Field manager
13. **FormFieldNewViewModel** - New field creation
14. **FormHeadlandViewModel** - Headland configuration
15. **FormAgShareFieldViewModel** - AgShare browser

---

## 4. AXAML View Design

### 4.1 View Standards

**Critical Rules (Lessons from Wave 9)**:
1. ✅ **ALWAYS include `x:DataType`** for compiled bindings
2. ✅ **Match property names exactly** between AXAML and ViewModel
3. ✅ **Use proper binding syntax** for complex bindings
4. ✅ **Test binding in-app** before committing

**Template Structure**:
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:AgValoniaGPS.ViewModels.Forms"
             x:Class="AgValoniaGPS.Desktop.Views.Forms.FormGPSData"
             x:DataType="vm:FormGPSDataViewModel">

    <!-- Content here -->

</UserControl>
```

### 4.2 Panel Views (Embedded in POC UI)

#### **FormGPSData.axaml** (RIGHT panel, collapsible)
```xml
<Border Classes="FloatingPanel" Width="300" MinHeight="400">
    <StackPanel Spacing="12">
        <DockPanel>
            <TextBlock Text="GPS Data" FontWeight="Bold" FontSize="16" DockPanel.Dock="Left"/>
            <Button Classes="IconButton" Content="×" Width="32" Height="32"
                    Command="{Binding CloseCommand}" DockPanel.Dock="Right"/>
        </DockPanel>

        <Border Classes="StatusBox">
            <StackPanel Spacing="8">
                <TextBlock Text="Fix Quality" Foreground="#AAA" FontSize="12"/>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <Ellipse Width="12" Height="12"
                             Fill="{Binding FixQuality, Converter={StaticResource FixQualityToColorConverter}}"/>
                    <TextBlock Text="{Binding FixQuality}" FontWeight="Bold" FontSize="14"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <Border Classes="StatusBox">
            <Grid RowDefinitions="Auto,Auto" ColumnDefinitions="*,Auto">
                <TextBlock Text="Latitude" Foreground="#AAA" FontSize="12" Grid.Row="0" Grid.Column="0"/>
                <TextBlock Text="{Binding Latitude, StringFormat={}{0:F7}°}" Grid.Row="0" Grid.Column="1"
                           FontWeight="Bold" FontSize="14"/>
                <TextBlock Text="Longitude" Foreground="#AAA" FontSize="12" Grid.Row="1" Grid.Column="0"/>
                <TextBlock Text="{Binding Longitude, StringFormat={}{0:F7}°}" Grid.Row="1" Grid.Column="1"
                           FontWeight="Bold" FontSize="14"/>
            </Grid>
        </Border>

        <!-- More status boxes... -->
    </StackPanel>
</Border>
```

#### **FormFieldData.axaml** (RIGHT panel, collapsible)
Similar structure showing field statistics

#### **FormTramLine.axaml** (LEFT panel)
Configuration panel with numeric inputs and buttons

#### **FormQuickAB.axaml** (LEFT panel)
Two buttons (Set A, Set B) + heading adjustment + Apply

### 4.3 Dialog Views (Modal over POC UI)

#### **ConfigVehicleControl.axaml** (Modal dialog)
```xml
<Window xmlns="https://github.com/avaloniaui"
        x:Class="AgValoniaGPS.Desktop.Views.Forms.ConfigVehicleControl"
        x:DataType="vm:ConfigVehicleControlViewModel"
        Title="Vehicle Configuration"
        Width="800" Height="600"
        WindowStartupLocation="CenterOwner"
        Background="#DD1a1a1a">

    <Border Classes="FloatingPanel" Margin="20">
        <DockPanel>
            <StackPanel DockPanel.Dock="Bottom" Orientation="Horizontal"
                        HorizontalAlignment="Right" Spacing="10">
                <Button Classes="ModernButton" Content="OK" Command="{Binding OKCommand}"/>
                <Button Classes="ModernButton" Content="Cancel" Command="{Binding CancelCommand}"/>
            </StackPanel>

            <TabControl DockPanel.Dock="Top">
                <TabItem Header="Dimensions">
                    <ScrollViewer>
                        <StackPanel Spacing="12" Margin="20">
                            <!-- Wheelbase, track, etc. -->
                        </StackPanel>
                    </ScrollViewer>
                </TabItem>
                <TabItem Header="Antenna">
                    <!-- Antenna settings -->
                </TabItem>
                <TabItem Header="Implement">
                    <!-- Implement settings -->
                </TabItem>
            </TabControl>
        </DockPanel>
    </Border>
</Window>
```

### 4.4 Tool Views (RIGHT panel, contextual)

#### **FormBndTool.axaml** (RIGHT panel tool mode)
```xml
<Border Classes="FloatingPanel" Width="300">
    <StackPanel Spacing="12">
        <TextBlock Text="Boundary Tool" FontWeight="Bold" FontSize="16"/>

        <Button Classes="ModernButton" Content="Start Drawing" Command="{Binding StartDrawingCommand}"/>
        <Button Classes="ModernButton" Content="Close Boundary" Command="{Binding CloseBoundaryCommand}"
                IsEnabled="{Binding CanCloseBoundary}"/>
        <Button Classes="ModernButton" Content="Simplify" Command="{Binding SimplifyCommand}"/>
        <Button Classes="ModernButton" Content="Clear" Command="{Binding ClearCommand}"/>

        <Border Classes="StatusBox">
            <StackPanel Spacing="4">
                <TextBlock Text="Points" Foreground="#AAA" FontSize="12"/>
                <TextBlock Text="{Binding PointCount}" FontWeight="Bold" FontSize="14"/>
            </StackPanel>
        </Border>

        <Border Classes="StatusBox">
            <StackPanel Spacing="4">
                <TextBlock Text="Area" Foreground="#AAA" FontSize="12"/>
                <TextBlock Text="{Binding Area, StringFormat={}{0:F2} ha}" FontWeight="Bold" FontSize="14"/>
            </StackPanel>
        </Border>
    </StackPanel>
</Border>
```

---

## 5. Testing Strategy

### 5.1 Unit Tests (xUnit)

**Test Structure**:
```csharp
public class FormGPSDataViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var mockGpsService = new Mock<IGPSDataService>();
        mockGpsService.Setup(x => x.FixQuality).Returns(FixQuality.RTKFixed);

        // Act
        var vm = new FormGPSDataViewModel(mockGpsService.Object);

        // Assert
        Assert.Equal(FixQuality.RTKFixed, vm.FixQuality);
    }

    [Fact]
    public void FixQualityChanged_UpdatesProperty()
    {
        // Arrange
        var mockGpsService = new Mock<IGPSDataService>();
        var vm = new FormGPSDataViewModel(mockGpsService.Object);

        bool propertyChanged = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FormGPSDataViewModel.FixQuality))
                propertyChanged = true;
        };

        // Act
        mockGpsService.Raise(x => x.FixQualityChanged += null,
                             new FixQualityChangedEventArgs(FixQuality.RTKFloat));

        // Assert
        Assert.True(propertyChanged);
    }
}
```

**Coverage Requirements**:
- 100% of ViewModels must have tests
- 100% of commands must have tests
- 100% of property change notifications must have tests
- Edge cases and validation tests

### 5.2 AXAML Binding Validation

**Pre-Commit Checklist**:
1. ✅ All AXAML files compile without errors
2. ✅ All bindings have matching ViewModel properties
3. ✅ `x:DataType` is present and correct
4. ✅ Converters are registered in App.axaml
5. ✅ Application runs and forms display correctly

**Testing Approach**:
1. Build project (must succeed with 0 errors)
2. Run application
3. Navigate to each form
4. Verify data displays correctly
5. Test all commands/buttons
6. Verify property change notifications work

### 5.3 Integration Tests

**Test Scenarios**:
1. **Form Navigation**: Click buttons in POC UI to open forms
2. **Service Integration**: Verify forms update when services emit events
3. **Command Execution**: Verify commands execute and update services
4. **Validation**: Verify invalid input shows errors
5. **Responsiveness**: Verify UI updates within 100ms of service changes

---

## 6. Implementation Plan

### 6.1 Task Groups (Parallelizable)

#### **Task Group 1: Core Operations (Priority 1)**
**Forms**: FormGPS, FormFieldData, FormGPSData, FormTramLine, FormQuickAB
**Effort**: 2-3 days
**Deliverables**:
- 5 ViewModels with unit tests
- 5 AXAML views integrated into POC UI
- Integration with Wave 1-8 services
- Full end-to-end testing

**Dependencies**: Wave 1 (GPS), Wave 2 (Guidance), Wave 5 (TramLine)

---

#### **Task Group 2: Configuration (Priority 2)**
**Forms**: ConfigVehicleControl, FormColorSection, FormSteerSettings, FormSectionConfig, FormVehicleSetup
**Effort**: 2-3 days
**Deliverables**:
- 5 ViewModels with unit tests
- 5 modal dialogs with proper styling
- Integration with Wave 8 (Configuration service)
- Settings persistence

**Dependencies**: Wave 8 (Configuration, Validation)

---

#### **Task Group 3: Field Management (Priority 2)**
**Forms**: FormBndTool, FormFieldDirectory, FormFieldNew, FormHeadland, FormAgShareField
**Effort**: 2-3 days
**Deliverables**:
- 5 ViewModels with unit tests
- 3 modal dialogs + 2 tool panels
- Integration with Wave 5 (Boundary, Headland)
- File I/O integration

**Dependencies**: Wave 5 (Boundary, Headland, Field I/O)

---

### 6.2 Development Workflow

1. **Create ViewModels** (TDD approach)
   - Write tests first
   - Implement ViewModel logic
   - Ensure 100% test pass rate

2. **Create AXAML Views**
   - Use POC UI design system
   - Include `x:DataType` directives
   - Match ViewModel property names exactly

3. **Build & Test**
   - Compile project (must succeed with 0 errors)
   - Run application
   - Verify forms display and function correctly

4. **Integrate into POC UI**
   - Add navigation buttons
   - Wire up commands
   - Test end-to-end workflows

5. **Commit**
   - Commit working code only
   - Include tests in commit
   - Update documentation

---

## 7. Success Criteria

### 7.1 Functional Requirements

✅ **All 15 forms implemented and integrated**
✅ **100% unit test coverage for ViewModels**
✅ **0 AXAML compilation errors**
✅ **All bindings work correctly (no binding errors in output)**
✅ **All commands execute properly**
✅ **Forms integrate seamlessly with POC UI layout**
✅ **Touch-friendly controls (≥48x48px buttons)**
✅ **Responsive UI (<100ms update latency)**

### 7.2 Non-Functional Requirements

✅ **Consistent design language with POC UI**
✅ **Proper z-indexing (panels don't obscure critical controls)**
✅ **Collapsible panels work correctly**
✅ **Modal dialogs overlay properly**
✅ **Keyboard navigation works (Tab, Enter, ESC)**
✅ **Clean Release build (0 errors, 0 warnings)**

### 7.3 Integration Requirements

✅ **Forms update in real-time when services change**
✅ **Commands trigger service actions**
✅ **Validation service prevents invalid input**
✅ **Configuration service persists settings**
✅ **Navigation flows match legacy AgOpenGPS patterns**

---

## 8. Risk Mitigation

### 8.1 Known Risks

**Risk 1: AXAML Binding Errors (Wave 9 Lesson)**
- **Mitigation**: Build and test in-app before committing
- **Verification**: 0 compilation errors, 0 binding errors in output
- **Fallback**: Fix errors immediately, don't defer

**Risk 2: Property Name Mismatches**
- **Mitigation**: Code review, automated testing
- **Verification**: Unit tests for all properties
- **Fallback**: Refactor ViewModels if AXAML doesn't match

**Risk 3: Performance (100-300 controls per form)**
- **Mitigation**: Virtualization for lists, lazy loading
- **Verification**: UI responds within 100ms
- **Fallback**: Optimize rendering, reduce control count

**Risk 4: Touch-Unfriendly Controls**
- **Mitigation**: ≥48x48px buttons, proper spacing
- **Verification**: Manual testing on touch screen
- **Fallback**: Increase control sizes

---

## 9. Acceptance Criteria

### 9.1 Per-Form Checklist

For each of the 15 forms:

- [ ] ViewModel created and tested (100% pass rate)
- [ ] AXAML view created with correct `x:DataType`
- [ ] All bindings work (0 binding errors)
- [ ] Integrated into POC UI layout
- [ ] Navigation buttons added
- [ ] Commands execute properly
- [ ] Validation works (if applicable)
- [ ] Settings persist (if applicable)
- [ ] Manual testing completed
- [ ] Code reviewed and approved

### 9.2 Wave 10 Completion Checklist

- [ ] All 15 forms completed per-form checklist
- [ ] All 15 ViewModels have ≥90% test coverage
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Application runs without crashes
- [ ] All navigation flows work end-to-end
- [ ] Performance requirements met (<100ms updates)
- [ ] Design consistency verified
- [ ] Documentation updated
- [ ] Git commits clean and descriptive

---

## 10. Documentation Deliverables

1. **Implementation Report** (per task group)
   - ViewModels created
   - Tests written and passed
   - AXAML views created
   - Integration points

2. **User Guide Updates**
   - Screenshots of new forms
   - Usage instructions
   - Keyboard shortcuts

3. **Developer Guide Updates**
   - ViewModel patterns
   - AXAML patterns
   - Integration patterns

4. **Verification Report**
   - Test results
   - Build output
   - Performance metrics
   - Known issues (if any)

---

## 11. Dependencies

### 11.1 Backend Services (Waves 1-8)

**Wave 1**: IGPSDataService, IPositionUpdateService
**Wave 2**: IABLineService, ICurveLineService, IContourLineService
**Wave 3**: ISteeringCoordinatorService, IPurePursuitService, IStanleySteeringService
**Wave 4**: ISectionControlService, ISectionConfigurationService
**Wave 5**: IBoundaryManagementService, IHeadlandService, ITramLineService, IFieldService
**Wave 6**: IModuleCommunicationService, IPgnMessageBuilderService
**Wave 7**: IDisplayFormatterService, IFieldStatisticsService
**Wave 8**: IConfigurationService, ISessionManagementService, IValidationService

### 11.2 UI Dependencies

**Wave 9**: DialogViewModelBase, ViewModelBase (base classes)
**POC UI**: MainWindow.axaml (layout and styles), OpenGLMapControl

---

## 12. Future Enhancements (Post-Wave 10)

1. **Keyboard Shortcuts** (Wave 11)
   - Hotkeys for common actions
   - Configurable shortcuts

2. **Multi-Language Support** (Wave 11)
   - Translation resources
   - Language switcher

3. **Theming** (Wave 11)
   - Light/dark mode toggle
   - Custom color schemes

4. **Accessibility** (Wave 11)
   - Screen reader support
   - High contrast mode
   - Keyboard-only navigation

5. **Advanced Visualizations** (Wave 12)
   - 3D terrain rendering
   - Real-time telemetry charts
   - Coverage heatmaps

---

## 13. References

- Wave 9 Status: `agent-os/specs/2025-10-22-wave-9-simple-forms-ui/WAVE_9_STATUS.md`
- UI Extraction Data: `AgValoniaGPS/UI_Extraction/`
- POC UI Layout: `AgValoniaGPS.Desktop/Views/MainWindow.axaml`
- Legacy Forms: `SourceCode/GPS/Forms/`
- Backend Services: `BACKEND_SERVICES.md`

---

**End of Specification**
