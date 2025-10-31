# Button-to-Panel Mapping Proposal

**Date:** 2025-10-29
**Purpose:** Wire 27 floating buttons to 15 Wave 10 panels + additional actions

---

## Research Summary

Based on comprehensive analysis of:
1. `ui-structure.json` - Legacy AgOpenGPS UI structure
2. `FormGPS.Designer.cs` - Legacy button-to-panel assignments
3. `navigation-panel-buttons.md` - Navigation panel documentation
4. Current floating button tooltips in `MainWindow.axaml`

---

## LEFT PANEL BUTTONS (FloatingBtn1-7)

### FloatingBtn1 - Navigation Settings
**Icon:** NavigationSettings.png
**Tooltip:** "Navigation Settings"
**Action:** Toggle `PanelNavigation` (navigation panel with camera controls)
**Maps to:** `navigation` panel
**Implementation:** `_panelHostingService.TogglePanel("navigation")`

### FloatingBtn2 - Wizards/Tools
**Icon:** SpecialFunctions.png
**Tooltip:** "Wizards Menu"
**Action:** Toggle `fieldTools` panel (field management wizards)
**Maps to:** `fieldTools` panel
**Implementation:** `_panelHostingService.TogglePanel("fieldTools")`

### FloatingBtn3 - Configuration
**Icon:** Settings48.png
**Tooltip:** "Configuration"
**Action:** Toggle `config` panel (application settings)
**Maps to:** `config` panel
**Implementation:** `_panelHostingService.TogglePanel("config")`

### FloatingBtn4 - Field Manager
**Icon:** FieldOpen.png (assumed)
**Tooltip:** "Field Manager"
**Action:** Toggle `fieldFileManager` panel (open/save fields)
**Maps to:** `fieldFileManager` panel
**Implementation:** `_panelHostingService.TogglePanel("fieldFileManager")`

### FloatingBtn5 - Field Tools
**Icon:** FieldTools.png (assumed)
**Tooltip:** "Field Tools"
**Action:** Toggle `boundaryEditor` panel (boundary editing)
**Maps to:** `boundaryEditor` panel
**Implementation:** `_panelHostingService.TogglePanel("boundaryEditor")`

### FloatingBtn6 - Auto Steer Config
**Icon:** AutoSteerConf.png (assumed)
**Tooltip:** "Auto Steer Configuration"
**Action:** Toggle `steer` panel (AutoSteer settings)
**Maps to:** `steer` panel
**Implementation:** `_panelHostingService.TogglePanel("steer")`

### FloatingBtn7 - Data I/O Status
**Icon:** Data status icon (assumed)
**Tooltip:** "Data I/O Status"
**Action:** Toggle `diagnostics` panel (system diagnostics)
**Maps to:** `diagnostics` panel
**Implementation:** `_panelHostingService.TogglePanel("diagnostics")`

---

## RIGHT PANEL BUTTONS (FloatingBtnR1-R9)

### FloatingBtnR9 (Top) - Auto Steer Toggle
**Action:** Toggle AutoSteer on/off (not a panel)
**Maps to:** Direct AutoSteer service call
**Implementation:** Call steering service `ToggleAutoSteer()`

### FloatingBtnR8 - Auto You-Turn
**Action:** Toggle auto U-turn feature (not a panel)
**Maps to:** Direct U-turn service call
**Implementation:** Call `ToggleAutoUTurn()`

### FloatingBtnR7 - Section Master Auto
**Action:** Toggle automatic section control (not a panel)
**Maps to:** Section control service
**Implementation:** Call `SetSectionMode(SectionMode.Auto)`

### FloatingBtnR6 - Section Master Manual
**Action:** Toggle manual section control (not a panel)
**Maps to:** Section control service
**Implementation:** Call `SetSectionMode(SectionMode.Manual)`

### FloatingBtnR5 - Auto Track
**Action:** Toggle automatic AB line tracking (not a panel)
**Maps to:** Guidance service
**Implementation:** Call `ToggleAutoTrack()`

### FloatingBtnR4 - Cycle Lines Forward
**Action:** Cycle to next guidance line (not a panel)
**Maps to:** Guidance service
**Implementation:** Call `CycleGuidanceLineForward()`

### FloatingBtnR3 - Cycle Lines Backward
**Action:** Cycle to previous guidance line (not a panel)
**Maps to:** Guidance service
**Implementation:** Call `CycleGuidanceLineBackward()`

### FloatingBtnR2 - Contour Mode
**Action:** Toggle contour tracking mode
**Maps to:** Could toggle `quickAB` panel OR direct service call
**Implementation:** `_panelHostingService.TogglePanel("quickAB")` OR call contour service

### FloatingBtnR1 (Bottom) - Contour Lock
**Action:** Lock/unlock contour tracking
**Maps to:** Direct service call
**Implementation:** Call `ToggleContourLock()`

---

## BOTTOM PANEL BUTTONS (FloatingBtnB1-B11)

### FloatingBtnB11 (Left) - Track Recording
**Action:** Start/stop track recording
**Maps to:** Direct recording service
**Implementation:** Call `ToggleTrackRecording()`

### FloatingBtnB10 - Snap to Pivot
**Action:** Snap guidance to pivot point
**Maps to:** Direct guidance service
**Implementation:** Call `SnapToPivot()`

### FloatingBtnB9 - Nudge Right
**Action:** Adjust AB line right
**Maps to:** Direct guidance service
**Implementation:** Call `NudgeABLineRight()`

### FloatingBtnB8 - Nudge Left
**Action:** Adjust AB line left
**Maps to:** Direct guidance service
**Implementation:** Call `NudgeABLineLeft()`

### FloatingBtnB7 - Flags
**Action:** Toggle `flags` panel (field markers/waypoints)
**Maps to:** `flags` panel
**Implementation:** `_panelHostingService.TogglePanel("flags")`

### FloatingBtnB6 - Headland On/Off
**Action:** Toggle headland mode
**Maps to:** Direct service call
**Implementation:** Call `ToggleHeadlandMode()`

### FloatingBtnB5 - Section Control Toggle
**Action:** Master section on/off
**Maps to:** Direct section service
**Implementation:** Call `ToggleSectionControl()`

### FloatingBtnB4 - Hydraulic Lift
**Action:** Raise/lower implement
**Maps to:** Direct hydraulic service
**Implementation:** Call `ToggleHydraulicLift()`

### FloatingBtnB3 - Tram Lines
**Action:** Toggle `tramLine` panel (tramline configuration)
**Maps to:** `tramLine` panel
**Implementation:** `_panelHostingService.TogglePanel("tramLine")`

### FloatingBtnB2 - Reset Tool Heading
**Action:** Reset implement heading
**Maps to:** Direct service call
**Implementation:** Call `ResetToolHeading()`

### FloatingBtnB1 (Right) - Camera/View
**Action:** Toggle `camera` panel (camera/view settings)
**Maps to:** `camera` panel
**Implementation:** `_panelHostingService.TogglePanel("camera")`

---

## PANELS NOT MAPPED TO BUTTONS YET

The following Wave 10 panels are registered but don't have dedicated buttons:

1. **fieldData** - Field statistics panel
2. **gpsData** - GPS status panel
3. **rollCorrection** - Roll/IMU correction panel
4. **vehicleConfig** - Vehicle dimensions configuration

**Recommendation:** These could be:
- Accessed via menu items
- Added as additional floating buttons
- Opened automatically based on context
- Accessed via FloatingBtn2 (Wizards Menu) as sub-menu items

---

## IMPLEMENTATION PRIORITY

### Phase 1: Panel Toggle Buttons (High Priority)
Wire these buttons that directly open Wave 10 panels:
- FloatingBtn1 → navigation
- FloatingBtn2 → fieldTools
- FloatingBtn3 → config
- FloatingBtn4 → fieldFileManager
- FloatingBtn5 → boundaryEditor
- FloatingBtn6 → steer
- FloatingBtn7 → diagnostics
- FloatingBtnB7 → flags
- FloatingBtnB3 → tramLine
- FloatingBtnB1 → camera

### Phase 2: Service Action Buttons (Medium Priority)
Wire these buttons to call services directly (no panels):
- Right panel buttons (R1-R9): Steering, section, guidance controls
- Bottom panel buttons (B2, B4-B6, B8-B11): Field operations

### Phase 3: Unmapped Panels (Low Priority)
Determine how to access:
- fieldData, gpsData, rollCorrection, vehicleConfig

---

## NOTES

1. **Right Panel Design:** Most buttons are **action buttons** (not panel toggles)
2. **Bottom Panel Design:** Mix of **action buttons** and **panel toggles**
3. **Navigation Panel:** Special floating panel with 10 camera control buttons
4. **Current Count:** 10 buttons open panels, 17 buttons trigger actions
5. **Missing Services:** Many action buttons require services that may not exist yet (steering coordinator, section control, etc.)

---

## NEXT STEPS

1. Implement Phase 1: Add click handlers to panel toggle buttons
2. Create stub methods for Phase 2 action buttons
3. Test panel show/hide functionality
4. Determine access method for unmapped panels
5. Implement actual service calls as services become available

---

*This mapping preserves the familiar AgOpenGPS workflow while adapting to the modern AgValoniaGPS architecture.*
