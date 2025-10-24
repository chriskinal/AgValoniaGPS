# Wave 10.5: Panel Docking System - Specification

**Wave**: 10.5 (Bridge between Wave 10 and Wave 11)
**Created**: 2025-10-24
**Status**: Draft
**Estimated Effort**: 18-38 hours (reduced from 20-40 hours - icons already extracted)

---

## Executive Summary

Wave 10.5 implements the authentic docking panel system from the legacy AgOpenGPS application. This wave corrects the architectural misunderstanding from the initial Wave 10 implementation, which incorrectly treated FormGPS as a simple overlay with fixed controls.

**Key Objectives:**
1. Refactor MainWindow/FormGPS to implement proper docking panel architecture
2. Create PanelHostingService for dynamic panel loading/unloading
3. ✅ ~~Extract button icons from legacy Resources~~ **COMPLETE** - 236 icons already in Assets/Icons/
4. Update all 15 Wave 10 panels to support docking behavior
5. Provide familiar UI layout matching original AgOpenGPS

**Why This Wave Exists:**

The initial Wave 10 implementation was based on an incorrect assumption that FormGPS was a simple overlay panel. In reality, FormGPS is the **main application window** with 720 controls and 8 container panels that host other forms dynamically. The current implementation has:
- ❌ ~30 controls vs. 720 in original
- ❌ Fixed overlay panels vs. dynamic docking areas
- ❌ No panel hosting system
- ❌ Unfamiliar layout and behavior
- ❌ Controls randomly placed and overlapping

Wave 10.5 fixes this by implementing the authentic docking panel architecture before Wave 11 adds OpenGL rendering.

---

## Current State Analysis

### What's Wrong with Current Implementation

**File**: `AgValoniaGPS.Desktop/Views/Panels/Display/FormGPS.axaml`

**Current Implementation** (INCORRECT):
```xml
<Grid>
    <!-- Left Panel: Section Control -->
    <Border Classes="FloatingPanel" HorizontalAlignment="Left" VerticalAlignment="Center">
        <!-- Fixed section toggle buttons -->
    </Border>

    <!-- Bottom Panel: Vehicle Status -->
    <Border Classes="FloatingPanel" HorizontalAlignment="Center" VerticalAlignment="Bottom">
        <!-- Fixed speed/heading displays -->
    </Border>

    <!-- Right Panel: Camera Controls -->
    <Border Classes="FloatingPanel" HorizontalAlignment="Right" VerticalAlignment="Center">
        <!-- Fixed camera buttons -->
    </Border>
</Grid>
```

**Issues:**
1. **Fixed Controls**: Controls are hardcoded in FormGPS instead of being separate Wave 10 panels
2. **No Docking System**: Panels can't be moved, docked, or undocked
3. **Wrong Architecture**: FormGPS should be a container, not a control host
4. **Missing Panel Areas**: No panelLeft, panelRight, panelBottom, panelNavigation containers
5. **Incomplete**: Only ~30 controls vs. 720 in original
6. **Unfamiliar**: Layout doesn't match original AgOpenGPS

**User Feedback:**
> "Ok so we have a bunch of controls but they appear kind of randomly grouped some on top of the other n can't be moved."

> "I think having a familiar UI layout with familiar icons would help ease the transition to the new version."

> "Dumping them into an unfamiliar layout with unfamiliar behavior and icons would not be good."

---

## Architecture Design

### Correct Architecture (from ui-structure.json)

FormGPS is the **main window** with a central OpenGL map control and four docking panel areas:

```
┌─────────────────────────────────────────────────────────────────┐
│ Menu Bar (52 items)                                             │
├────┬────────────────────────────────────────────┬───────────────┤
│    │                                            │               │
│ P  │                                            │       P       │
│ a  │                                            │       a       │
│ n  │                                            │       n       │
│ e  │         OpenGL Map Control                 │       e       │
│ l  │         (GLControl - ZIndex=0)             │       l       │
│    │                                            │               │
│ L  │                                            │       R       │
│ e  │                                            │       i       │
│ f  │                                            │       g       │
│ t  │                                            │       h       │
│    │                                            │       t       │
│ 72 │                                            │      70       │
│ px │                                            │      px       │
│    │                                            │               │
├────┴────────────────────────────────────────────┴───────────────┤
│ Panel Bottom (974px x 62px)                                     │
│ FlowLayoutPanel - RightToLeft                                   │
└─────────────────────────────────────────────────────────────────┘

Panel Navigation (179px x 460px) - Initially Hidden
Appears as floating panel in center-left area
```

### Panel Specifications (from ui-structure.json)

#### 1. panelLeft
```json
{
  "name": "panelLeft",
  "type": "System.Windows.Forms.TableLayoutPanel",
  "properties": {
    "Anchor": "Top | Bottom | Left",
    "ColumnCount": "1",
    "RowCount": "8",
    "Size": "new System.Drawing.Size(72, 600)",
    "Location": "new System.Drawing.Point(3, 50)",
    "TabIndex": "529"
  },
  "children": []
}
```

**Purpose**: Hosts vertical stack of tool buttons (8 rows)
**Avalonia Equivalent**: Grid with 8 RowDefinitions
**Docking**: Left side, stretches vertically
**Width**: Fixed 72px
**Content**: Empty container - Wave 10 panels dock here dynamically

#### 2. panelRight
```json
{
  "name": "panelRight",
  "type": "System.Windows.Forms.FlowLayoutPanel",
  "properties": {
    "Anchor": "Bottom | Right",
    "FlowDirection": "BottomUp",
    "Size": "new System.Drawing.Size(70, 650)",
    "Location": "new System.Drawing.Point(926, 53)",
    "TabIndex": "541"
  },
  "children": []
}
```

**Purpose**: Hosts vertical stack of tool buttons (flows bottom-up)
**Avalonia Equivalent**: StackPanel with Orientation="Vertical" and VerticalAlignment="Bottom"
**Docking**: Right side, anchored to bottom
**Width**: Fixed 70px
**Content**: Empty container - Wave 10 panels dock here dynamically

#### 3. panelBottom
```json
{
  "name": "panelBottom",
  "type": "System.Windows.Forms.FlowLayoutPanel",
  "properties": {
    "Anchor": "Bottom | Right",
    "FlowDirection": "RightToLeft",
    "Size": "new System.Drawing.Size(974, 62)",
    "Location": "new System.Drawing.Point(-54, 652)",
    "TabIndex": "540"
  },
  "children": []
}
```

**Purpose**: Hosts horizontal row of status/control panels
**Avalonia Equivalent**: StackPanel with Orientation="Horizontal" and HorizontalAlignment="Right"
**Docking**: Bottom, flows right-to-left
**Height**: Fixed 62px
**Content**: Empty container - Wave 10 panels dock here dynamically

#### 4. panelNavigation
```json
{
  "name": "panelNavigation",
  "type": "System.Windows.Forms.TableLayoutPanel",
  "properties": {
    "BackColor": "System.Drawing.Color.WhiteSmoke",
    "ColumnCount": "2",
    "RowCount": "5",
    "Size": "new System.Drawing.Size(179, 460)",
    "Location": "new System.Drawing.Point(505, 63)",
    "TabIndex": "468",
    "Visible": "false"
  },
  "children": []
}
```

**Purpose**: Floating navigation panel (toggle visibility)
**Avalonia Equivalent**: Grid with 5 rows × 2 columns in a Popup or floating Border
**Docking**: Floating (center-left area)
**Size**: Fixed 179px × 460px
**Initial State**: Hidden (IsVisible=false)
**Content**: Navigation buttons (to be implemented from ui-structure.json)

---

## MainWindow Layout Structure

### AXAML Layout (Correct Architecture)

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="AgValoniaGPS.Desktop.Views.MainWindow"
        Title="AgValoniaGPS"
        Width="1024" Height="768">

    <Grid RowDefinitions="Auto,*">
        <!-- Menu Bar -->
        <Menu Grid.Row="0">
            <!-- 52 menu items (to be extracted from ui-structure.json) -->
        </Menu>

        <!-- Main Content Area -->
        <Grid Grid.Row="1" ColumnDefinitions="72,*,70" RowDefinitions="*,62">

            <!-- Panel Left (Column 0, Row 0) -->
            <Grid Grid.Column="0" Grid.Row="0" x:Name="PanelLeft" Background="#2C2C2C">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <!-- Panels dock here dynamically via PanelHostingService -->
            </Grid>

            <!-- OpenGL Map Control (Column 1, Row 0) -->
            <Border Grid.Column="1" Grid.Row="0" Background="Black" x:Name="MapContainer">
                <!-- AvaloniaOpenGL control (Wave 11) -->
            </Border>

            <!-- Panel Right (Column 2, Row 0) -->
            <StackPanel Grid.Column="2" Grid.Row="0"
                        x:Name="PanelRight"
                        Background="#2C2C2C"
                        VerticalAlignment="Bottom"
                        Spacing="0">
                <!-- Panels dock here dynamically via PanelHostingService -->
            </StackPanel>

            <!-- Panel Bottom (Column 0-2, Row 1) -->
            <StackPanel Grid.Column="0" Grid.Row="1" Grid.ColumnSpan="3"
                        x:Name="PanelBottom"
                        Background="#2C2C2C"
                        Orientation="Horizontal"
                        HorizontalAlignment="Right"
                        Spacing="0">
                <!-- Panels dock here dynamically via PanelHostingService -->
            </StackPanel>
        </Grid>

        <!-- Panel Navigation (Floating) -->
        <Border Grid.Row="1"
                x:Name="PanelNavigation"
                IsVisible="False"
                Background="WhiteSmoke"
                Width="179" Height="460"
                HorizontalAlignment="Left"
                VerticalAlignment="Top"
                Margin="100,80,0,0"
                CornerRadius="4"
                BoxShadow="0 4 8 0 #40000000">
            <Grid RowDefinitions="*,*,*,*,*" ColumnDefinitions="*,*">
                <!-- Navigation buttons (to be extracted from ui-structure.json) -->
            </Grid>
        </Border>
    </Grid>
</Window>
```

---

## PanelHostingService Design

### Service Interface

```csharp
namespace AgValoniaGPS.Services.UI;

public interface IPanelHostingService
{
    /// <summary>
    /// Registers a panel as dockable in a specific panel area.
    /// </summary>
    void RegisterPanel(string panelId, PanelDockLocation location, Control panelControl);

    /// <summary>
    /// Shows a panel in its registered dock location.
    /// </summary>
    void ShowPanel(string panelId);

    /// <summary>
    /// Hides a panel from its dock location.
    /// </summary>
    void HidePanel(string panelId);

    /// <summary>
    /// Toggles panel visibility.
    /// </summary>
    void TogglePanel(string panelId);

    /// <summary>
    /// Checks if a panel is currently visible.
    /// </summary>
    bool IsPanelVisible(string panelId);

    /// <summary>
    /// Gets all panels in a specific dock location.
    /// </summary>
    IEnumerable<string> GetPanelsInLocation(PanelDockLocation location);

    /// <summary>
    /// Event raised when panel visibility changes.
    /// </summary>
    event EventHandler<PanelVisibilityChangedEventArgs>? PanelVisibilityChanged;
}

public enum PanelDockLocation
{
    Left,
    Right,
    Bottom,
    Navigation
}

public class PanelVisibilityChangedEventArgs : EventArgs
{
    public string PanelId { get; set; } = "";
    public bool IsVisible { get; set; }
    public PanelDockLocation Location { get; set; }
}
```

### Service Implementation

```csharp
namespace AgValoniaGPS.Services.UI;

public class PanelHostingService : IPanelHostingService
{
    private readonly Dictionary<string, PanelRegistration> _panels = new();
    private readonly Dictionary<PanelDockLocation, Panel> _dockContainers = new();

    public event EventHandler<PanelVisibilityChangedEventArgs>? PanelVisibilityChanged;

    public void Initialize(Panel panelLeft, Panel panelRight, Panel panelBottom, Panel panelNavigation)
    {
        _dockContainers[PanelDockLocation.Left] = panelLeft;
        _dockContainers[PanelDockLocation.Right] = panelRight;
        _dockContainers[PanelDockLocation.Bottom] = panelBottom;
        _dockContainers[PanelDockLocation.Navigation] = panelNavigation;
    }

    public void RegisterPanel(string panelId, PanelDockLocation location, Control panelControl)
    {
        var registration = new PanelRegistration
        {
            PanelId = panelId,
            Location = location,
            Control = panelControl,
            IsVisible = false
        };

        _panels[panelId] = registration;
    }

    public void ShowPanel(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var registration))
            return;

        if (registration.IsVisible)
            return;

        var container = _dockContainers[registration.Location];

        // Add control to container
        if (container is Grid grid)
        {
            // Find first available row in grid (for panelLeft)
            int row = FindAvailableRow(grid);
            Grid.SetRow(registration.Control, row);
            grid.Children.Add(registration.Control);
        }
        else if (container is StackPanel stackPanel)
        {
            // Add to stack panel (for panelRight, panelBottom)
            stackPanel.Children.Add(registration.Control);
        }

        registration.IsVisible = true;
        PanelVisibilityChanged?.Invoke(this, new PanelVisibilityChangedEventArgs
        {
            PanelId = panelId,
            IsVisible = true,
            Location = registration.Location
        });
    }

    public void HidePanel(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var registration))
            return;

        if (!registration.IsVisible)
            return;

        var container = _dockContainers[registration.Location];
        container.Children.Remove(registration.Control);

        registration.IsVisible = false;
        PanelVisibilityChanged?.Invoke(this, new PanelVisibilityChangedEventArgs
        {
            PanelId = panelId,
            IsVisible = false,
            Location = registration.Location
        });
    }

    public void TogglePanel(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var registration))
            return;

        if (registration.IsVisible)
            HidePanel(panelId);
        else
            ShowPanel(panelId);
    }

    public bool IsPanelVisible(string panelId)
    {
        return _panels.TryGetValue(panelId, out var registration) && registration.IsVisible;
    }

    public IEnumerable<string> GetPanelsInLocation(PanelDockLocation location)
    {
        return _panels.Values
            .Where(p => p.Location == location)
            .Select(p => p.PanelId);
    }

    private int FindAvailableRow(Grid grid)
    {
        // Find first row without children
        for (int i = 0; i < grid.RowDefinitions.Count; i++)
        {
            bool hasChild = grid.Children.Any(c => Grid.GetRow(c) == i);
            if (!hasChild)
                return i;
        }
        return 0; // Default to first row if all occupied
    }

    private class PanelRegistration
    {
        public string PanelId { get; set; } = "";
        public PanelDockLocation Location { get; set; }
        public Control Control { get; set; } = null!;
        public bool IsVisible { get; set; }
    }
}
```

### Service Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<IPanelHostingService, PanelHostingService>();
```

### MainWindow Integration

```csharp
public partial class MainWindow : Window
{
    private readonly IPanelHostingService _panelHostingService;

    public MainWindow(IPanelHostingService panelHostingService)
    {
        _panelHostingService = panelHostingService;
        InitializeComponent();

        // Initialize panel hosting service with dock containers
        _panelHostingService.Initialize(
            PanelLeft,
            PanelRight,
            PanelBottom,
            PanelNavigation
        );

        // Register Wave 10 panels (example)
        RegisterPanels();
    }

    private void RegisterPanels()
    {
        // Example: Register FormGPSData to dock in panelBottom
        var gpsDataView = new FormGPSData { DataContext = MainViewModel.GPSDataVM };
        _panelHostingService.RegisterPanel("gpsData", PanelDockLocation.Bottom, gpsDataView);

        // Example: Register FormSteer to dock in panelRight
        var steerView = new FormSteer { DataContext = MainViewModel.SteerVM };
        _panelHostingService.RegisterPanel("steer", PanelDockLocation.Right, steerView);

        // Register all other Wave 10 panels...
    }
}
```

---

## Button/Icon Extraction

### Current Situation ✅ COMPLETE

**Status**: ✅ Icons already extracted
**Location**: `AgValoniaGPS/AgValoniaGPS.Desktop/Assets/Icons/`
**Count**: 236 PNG icons

The original AgOpenGPS button icons have already been extracted and are available in the Assets/Icons directory:
- Field operation buttons (guidance, section control, etc.)
- Configuration tool buttons
- Camera control buttons
- File operation buttons
- Navigation buttons

These icons provide visual familiarity for authentic UI recreation.

**Sample Icons Available:**
- AutoSteerOn.png, AutoSteerOff.png
- ABDraw.png, ABLineCycle.png, ABTrackAB.png
- Boundary.png, BoundaryFromTracks.png
- Camera.png, CameraOn.png
- FileOpen.png, FileSave.png
- And 216 more...

### ~~Extraction Strategy~~ (No longer needed - already complete)

~~Option 1: Manual Extraction from Resources~~
~~Option 2: Programmatic Extraction~~

Icons have already been extracted to `Assets/Icons/` directory.

### Icon Integration

```xml
<!-- Example button with extracted icon -->
<Button Classes="IconButton" ToolTip.Tip="Start Guidance">
    <Image Source="/Assets/Icons/AutoSteerOn.png" Width="48" Height="48"/>
</Button>
```

### Required Icons (from ui-structure.json analysis)

**Panel Left Buttons:**
- btnAutoSteerOn/Off
- btnSectionControl
- btnFieldBoundary
- btnContour
- btnTrack
- btnFlags
- btnTram
- btnHeadland

**Panel Right Buttons:**
- btn3D
- btnZoomIn
- btnZoomOut
- btnReset
- btnDayNight
- btnGrid

**Panel Bottom Buttons:**
- btnGuidance
- btnABLine
- btnCurve
- btnContour
- btnJobStart
- btnSettings

---

## Wave 10 Panel Refactoring

### Current Panel Architecture

Wave 10 panels inherit from `PanelViewModelBase`:

```csharp
public partial class FormGPSDataViewModel : PanelViewModelBase
{
    // ...
}
```

### Required Changes for Docking

#### 1. Panel Views Must Be Minimal

Each panel view should be a compact button/widget suitable for docking:

**BEFORE (Full Panel Window):**
```xml
<!-- FormGPSData.axaml - Currently a full window -->
<UserControl>
    <Border Classes="Panel" Width="400" Height="600">
        <Grid>
            <!-- Full GPS data display -->
        </Grid>
    </Border>
</UserControl>
```

**AFTER (Dockable Button):**
```xml
<!-- FormGPSDataButton.axaml - Button for docking -->
<Button Classes="DockButton" Width="60" Height="60">
    <StackPanel>
        <Image Source="/Assets/Buttons/btnGPS.png" Width="48" Height="48"/>
        <TextBlock Text="GPS" FontSize="10" HorizontalAlignment="Center"/>
    </StackPanel>
</Button>
```

**Full Panel (Popup):**
```xml
<!-- FormGPSDataPanel.axaml - Full panel that appears on button click -->
<Border Classes="FloatingPanel" Width="400" Height="600">
    <Grid>
        <!-- Full GPS data display -->
    </Grid>
</Border>
```

#### 2. Panel Registration Pattern

```csharp
// In MainWindow.axaml.cs
private void RegisterPanels()
{
    // Create dockable button
    var gpsDataButton = new FormGPSDataButton();
    gpsDataButton.Click += (s, e) => ShowGPSDataPanel();

    // Register button in dock location
    _panelHostingService.RegisterPanel(
        "gpsData",
        PanelDockLocation.Bottom,
        gpsDataButton
    );

    // Show button by default
    _panelHostingService.ShowPanel("gpsData");
}

private void ShowGPSDataPanel()
{
    // Show full panel as popup/flyout
    var panel = new FormGPSDataPanel { DataContext = MainViewModel.GPSDataVM };
    // Position near button, or in center, etc.
    panel.Show();
}
```

#### 3. Button States

Buttons should indicate panel state:
- **Default**: Panel hidden
- **Active**: Panel visible
- **Indicator**: Data status (e.g., GPS fix quality)

```csharp
public partial class FormGPSDataButton : Button
{
    public void UpdateStatus(GpsFixQuality quality)
    {
        // Change button color based on GPS status
        switch (quality)
        {
            case GpsFixQuality.NoFix:
                Classes.Add("StatusRed");
                break;
            case GpsFixQuality.RTKFloat:
                Classes.Add("StatusYellow");
                break;
            case GpsFixQuality.RTKFixed:
                Classes.Add("StatusGreen");
                break;
        }
    }
}
```

---

## Implementation Tasks

### Task Group 1: MainWindow Refactoring (8 hours)

**Task 1.1**: Create new MainWindow.axaml with docking panel layout
- Remove current FormGPS overlay implementation
- Add Grid with panelLeft (72px), panelRight (70px), panelBottom (62px)
- Add placeholder for OpenGL map control (Wave 11)
- Add panelNavigation as floating panel (initially hidden)
- **Effort**: 2 hours

**Task 1.2**: Extract menu bar structure from ui-structure.json
- Parse 52 menu items from FormGPS structure
- Create Avalonia Menu control with identical structure
- Wire up menu commands (stubs for now)
- **Effort**: 3 hours

**Task 1.3**: Style docking panels to match original
- Dark background (#2C2C2C)
- Proper spacing and padding
- Button hover/active states
- **Effort**: 2 hours

**Task 1.4**: Test responsive layout
- Verify panels resize correctly
- Test on different window sizes
- Ensure OpenGL map area scales properly
- **Effort**: 1 hour

---

### Task Group 2: PanelHostingService Implementation (6 hours)

**Task 2.1**: Create IPanelHostingService interface
- Define RegisterPanel, ShowPanel, HidePanel, TogglePanel methods
- Define PanelDockLocation enum
- Define PanelVisibilityChanged event
- **Effort**: 1 hour

**Task 2.2**: Implement PanelHostingService
- Panel registration with dock locations
- Dynamic add/remove from dock containers
- Grid row management for panelLeft
- StackPanel management for panelRight/panelBottom
- **Effort**: 3 hours

**Task 2.3**: Register service in DI container
- Add to ServiceCollectionExtensions
- Initialize in MainWindow constructor
- **Effort**: 1 hour

**Task 2.4**: Unit tests for PanelHostingService
- Test panel registration
- Test show/hide functionality
- Test toggle functionality
- Test visibility state tracking
- **Effort**: 1 hour

---

### Task Group 3: Button/Icon Extraction (4 hours)

**Task 3.1**: Extract button icons from legacy Resources
- Open GPS.resx in Visual Studio
- Export all button images to Assets/Buttons/
- Convert to PNG format
- Document icon names and purposes
- **Effort**: 2 hours

**Task 3.2**: Create icon catalog document
- List all extracted icons with screenshots
- Document which panels use which icons
- Create naming conventions for icons
- **Effort**: 1 hour

**Task 3.3**: Update AXAML styles for icon buttons
- Define DockButton style with icon + text
- Define status indicator styles (red/yellow/green)
- Define hover/active/disabled states
- **Effort**: 1 hour

---

### Task Group 4: Wave 10 Panel Refactoring (16 hours)

**Task 4.1**: Create dockable button views for all 15 panels
- FormGPSDataButton, FormSteerButton, etc.
- Each button: 60×60px with icon + label
- Wire up click events to show full panel
- **Effort**: 4 hours (15 buttons × 15 min each)

**Task 4.2**: Refactor panel views to be popup-friendly
- Ensure panels work as floating popups
- Add close buttons to all panels
- Test panel positioning near dock buttons
- **Effort**: 4 hours

**Task 4.3**: Implement panel show/hide logic
- Button toggles panel visibility
- Panel close button hides panel and updates button state
- Only one panel visible at a time (optional)
- **Effort**: 3 hours

**Task 4.4**: Register all panels with PanelHostingService
- Determine default dock location for each panel
- Register in MainWindow.RegisterPanels()
- Set default visibility states
- **Effort**: 2 hours

**Task 4.5**: Add status indicators to buttons
- GPS data: Fix quality indicator
- AutoSteer: Active/inactive indicator
- Section control: On/off indicator
- **Effort**: 3 hours

---

### Task Group 5: Navigation Panel Implementation (4 hours)

**Task 5.1**: Extract panelNavigation button structure from ui-structure.json
- Parse 5×2 grid of navigation buttons
- Identify button icons and labels
- Document button purposes
- **Effort**: 1 hour

**Task 5.2**: Implement panelNavigation view
- 179×460px floating panel
- 5 rows × 2 columns grid
- 10 navigation buttons with icons
- **Effort**: 2 hours

**Task 5.3**: Add toggle button for panelNavigation
- Add to menu bar or dock area
- Show/hide panelNavigation on click
- Position panel appropriately
- **Effort**: 1 hour

---

### Task Group 6: Integration & Testing (6 hours)

**Task 6.1**: Integration testing with all panels
- Test each panel docking in each location
- Verify show/hide functionality
- Test panel state indicators
- **Effort**: 2 hours

**Task 6.2**: UI/UX testing
- Compare side-by-side with legacy AgOpenGPS
- Verify familiar layout and behavior
- Test button sizes and hit targets
- **Effort**: 2 hours

**Task 6.3**: Performance testing
- Verify smooth panel transitions
- Test with multiple panels visible
- Check memory usage
- **Effort**: 1 hour

**Task 6.4**: Documentation
- Update WAVE_10_COMPLETION_REPORT.md
- Create Wave 10.5 completion report
- Document panel docking patterns
- **Effort**: 1 hour

---

## Testing Plan

### Unit Tests

**PanelHostingService Tests:**
```csharp
[TestClass]
public class PanelHostingServiceTests
{
    [TestMethod]
    public void RegisterPanel_ShouldAddPanelToRegistry()
    {
        // Arrange
        var service = new PanelHostingService();
        var panel = new Button();

        // Act
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Assert
        Assert.AreEqual(1, service.GetPanelsInLocation(PanelDockLocation.Left).Count());
    }

    [TestMethod]
    public void ShowPanel_ShouldAddControlToContainer()
    {
        // Arrange
        var service = new PanelHostingService();
        var container = new Grid();
        service.Initialize(container, new StackPanel(), new StackPanel(), new Grid());
        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Act
        service.ShowPanel("test");

        // Assert
        Assert.IsTrue(container.Children.Contains(panel));
        Assert.IsTrue(service.IsPanelVisible("test"));
    }

    [TestMethod]
    public void HidePanel_ShouldRemoveControlFromContainer()
    {
        // Arrange
        var service = new PanelHostingService();
        var container = new Grid();
        service.Initialize(container, new StackPanel(), new StackPanel(), new Grid());
        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);
        service.ShowPanel("test");

        // Act
        service.HidePanel("test");

        // Assert
        Assert.IsFalse(container.Children.Contains(panel));
        Assert.IsFalse(service.IsPanelVisible("test"));
    }

    [TestMethod]
    public void TogglePanel_ShouldInvertVisibility()
    {
        // Arrange
        var service = new PanelHostingService();
        var container = new Grid();
        service.Initialize(container, new StackPanel(), new StackPanel(), new Grid());
        var panel = new Button();
        service.RegisterPanel("test", PanelDockLocation.Left, panel);

        // Act & Assert
        service.TogglePanel("test");
        Assert.IsTrue(service.IsPanelVisible("test"));

        service.TogglePanel("test");
        Assert.IsFalse(service.IsPanelVisible("test"));
    }
}
```

### Integration Tests

**MainWindow Panel Docking Tests:**
- Load MainWindow with all 15 panels registered
- Verify each panel can be shown/hidden
- Verify panels appear in correct dock locations
- Verify no layout glitches or overlaps

**Visual Comparison Tests:**
- Screenshot side-by-side with legacy AgOpenGPS
- Verify panel positions match original
- Verify button sizes and spacing match original
- Verify colors and styling match original

---

## Success Criteria

Wave 10.5 is considered complete when:

✅ **Architecture**
- MainWindow has correct 4-panel docking layout (Left/Right/Bottom/Navigation)
- OpenGL map area is properly positioned (center)
- Menu bar is implemented with 52 items matching original

✅ **PanelHostingService**
- Service can register panels in any dock location
- Service can show/hide panels dynamically
- Service correctly manages Grid rows (panelLeft) and StackPanel children (panelRight/panelBottom)
- Service raises PanelVisibilityChanged events

✅ **Buttons/Icons**
- All button icons extracted from legacy Resources
- Icon catalog document created
- Buttons styled to match original (size, spacing, colors)
- Status indicators working (GPS fix, AutoSteer, etc.)

✅ **Wave 10 Panel Integration**
- All 15 Wave 10 panels have dockable button views
- All panels can be shown as floating popups
- Buttons indicate panel visibility state
- Default panel visibility matches original

✅ **Navigation Panel**
- panelNavigation implemented with 5×2 button grid
- Can be toggled visible/hidden
- Positioned as floating panel

✅ **Testing**
- All unit tests pass
- Integration tests pass
- Visual comparison with legacy UI is satisfactory
- Performance is smooth (no lag when showing/hiding panels)

✅ **Documentation**
- Wave 10.5 completion report created
- Panel docking patterns documented
- Icon extraction process documented

✅ **User Acceptance**
- UI layout is familiar to legacy users
- Button icons are recognizable
- Panel behavior matches original
- No "randomly grouped" or "can't be moved" issues

---

## Risks and Mitigation

### Risk 1: Icon Quality
**Risk**: Extracted icons may be low resolution or poor quality.
**Mitigation**:
- Export at highest quality from Resources
- Consider recreating icons in vector format if needed
- Use AI upscaling for low-res icons

### Risk 2: Panel Positioning
**Risk**: Avalonia layout may not exactly match Windows Forms layout.
**Mitigation**:
- Use exact pixel sizes from ui-structure.json
- Test on multiple screen sizes
- Allow minor variations if functionally equivalent

### Risk 3: Performance
**Risk**: Dynamic panel loading may cause UI lag.
**Mitigation**:
- Pre-create all panel instances at startup
- Use virtualization if needed
- Profile with DevTools

### Risk 4: Scope Creep
**Risk**: 40-80 hour estimate may be exceeded.
**Mitigation**:
- Focus on core docking functionality first
- Defer advanced features (drag-and-drop reordering) to later wave
- Use existing Wave 10 panels without major refactoring if possible

---

## Dependencies

**Depends On:**
- ✅ Wave 9: Dialog Forms (53 forms)
- ✅ Wave 10: Operational Panels (15 panels with ViewModels)

**Blocks:**
- ⏸️ Wave 11: OpenGL Map Rendering (needs docking layout to render into)

**Related:**
- Phase 6: Dynamic UI Behavior Analysis (provides visibility rules and state transitions)

---

## Future Enhancements (Post-Wave 10.5)

These features are deferred to maintain focus on core docking functionality:

1. **Drag-and-Drop Panel Reordering**
   - Allow users to reorder buttons within dock areas
   - Persist user preferences

2. **Custom Panel Layouts**
   - Allow users to move panels between dock locations
   - Save/load custom layouts

3. **Panel Pinning**
   - Pin panels to stay open permanently
   - Auto-hide unpinned panels

4. **Panel Tabs**
   - Group multiple panels in one dock button
   - Tab interface to switch between panels

5. **Panel Resize**
   - Allow users to resize floating panels
   - Persist panel sizes

6. **Keyboard Shortcuts**
   - Hotkeys to toggle specific panels
   - Navigation between panels

---

## Appendix A: Menu Bar Structure

(To be extracted from ui-structure.json - 52 menu items)

**Top-Level Menus:**
1. File
2. Edit
3. View
4. Field
5. Vehicle
6. Guidance
7. Tools
8. Settings
9. Help

(Full structure to be documented after extraction)

---

## Appendix B: Icon Catalog

(To be populated after icon extraction)

**Format:**
| Icon Name | File Path | Used By | Purpose |
|-----------|-----------|---------|---------|
| btnAutoSteerOn.png | Assets/Buttons/ | FormSteer | Enable AutoSteer |
| btnGPS.png | Assets/Buttons/ | FormGPSData | GPS data panel |
| ... | ... | ... | ... |

---

## Appendix C: Panel Dock Location Mapping

Default dock locations for Wave 10 panels:

| Panel | Dock Location | Visibility | Purpose |
|-------|---------------|------------|---------|
| FormGPS | N/A (Main Window) | Always | Main window container |
| FormFieldData | Bottom | On Demand | Field boundary info |
| FormGPSData | Bottom | Default | GPS position/fix data |
| FormTramLine | Right | On Demand | Tram line patterns |
| FormQuickAB | Left | Default | Quick AB line creation |
| FormSteer | Right | On Demand | AutoSteer settings |
| FormConfig | Right | On Demand | App configuration |
| FormDiagnostics | Bottom | On Demand | System diagnostics |
| FormRollCorrection | Right | On Demand | IMU roll/pitch |
| FormVehicleConfig | Right | On Demand | Vehicle dimensions |
| FormFlags | Left | On Demand | Field markers |
| FormCamera | Right | Default | Camera controls |
| FormBoundaryEditor | Left | On Demand | Boundary editing |
| FormFieldTools | Left | On Demand | Field utilities |
| FormFieldFileManager | Left | On Demand | Field file browser |

---

**End of Specification**

**Next Steps:**
1. Review and approve specification
2. Create detailed tasks list (tasks.md)
3. Begin implementation with Task Group 1
4. Parallel work: Icon extraction can happen alongside coding

**Estimated Total Effort**: 44 hours (across 6 task groups)
**Recommended Team Size**: 1-2 developers
**Timeline**: 1-2 weeks at full-time capacity
