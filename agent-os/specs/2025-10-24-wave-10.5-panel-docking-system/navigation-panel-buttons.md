# Navigation Panel Buttons

## Overview

The Navigation Panel (panelNavigation) is a floating 179×460px panel with a 5×2 grid of camera and display control buttons. This panel provides quick access to view controls for the OpenGL map display.

**Extracted from:** `SourceCode/GPS/Forms/FormGPS.Designer.cs`
**Panel Type:** System.Windows.Forms.TableLayoutPanel
**Grid Layout:** 5 rows × 2 columns
**Background:** WhiteSmoke
**Initial State:** Hidden (Visible=false)

---

## Grid Layout: 5 rows × 2 columns (179×460px)

### Row 0 (Camera Tilt Controls)
- **Column 0**: btnTiltDn - TiltDown.png - **Tilt camera view down**
  - Click Handler: `btnTiltDn_Click`
  - Purpose: Decrease vertical camera angle
- **Column 1**: btnTiltUp - TiltUp.png - **Tilt camera view up**
  - Click Handler: `btnTiltUp_Click`
  - Purpose: Increase vertical camera angle

### Row 1 (2D/3D View Controls)
- **Column 0**: btn2D - Camera2D64.png - **Switch to 2D overhead view**
  - Click Handler: `btn2D_Click`
  - Purpose: Set camera to 2D top-down perspective
- **Column 1**: btn3D - Camera3D64.png - **Switch to 3D perspective view**
  - Click Handler: `btn3D_Click`
  - Purpose: Set camera to 3D angled perspective

### Row 2 (North Lock and Grid Controls)
- **Column 0**: btnN2D - CameraNorth2D.png - **Switch to North-locked 2D view**
  - Click Handler: `btnN2D_Click`
  - Purpose: Set camera to 2D with North orientation locked
- **Column 1**: btnGrid - GridRotate.png - **Toggle grid display/rotation**
  - Click Handler: `btnGrid_Click`
  - Purpose: Toggle field grid overlay on map
  - Size: 80×83px (larger than others)

### Row 3 (Day/Night Mode)
- **Column 0**: btnDayNightMode - WindowNightMode.png - **Toggle day/night display mode**
  - Click Handler: `btnDayNightMode_Click`
  - Purpose: Switch between day (light) and night (dark) color schemes
- **Column 1**: lblHz - Label (not button) - **Displays Hz rate**
  - Purpose: Shows update frequency or frame rate
  - Type: Label control (not clickable button)

### Row 4 (Brightness Controls)
- **Column 0**: btnBrightnessDn - BrightnessDn.png - **Decrease brightness**
  - Click Handler: `btnBrightnessDn_Click`
  - Purpose: Lower display brightness
  - Text: "20%" (shows current brightness level)
  - Size: 83×75px
- **Column 1**: btnBrightnessUp - BrightnessUp.png - **Increase brightness**
  - Click Handler: `btnBrightnessUp_Click`
  - Purpose: Raise display brightness
  - Size: 84×75px

---

## Icon Mapping

All icons are available in `AgValoniaGPS/AgValoniaGPS.Desktop/Assets/Icons/`

| Button Name | Icon File | Size | Purpose |
|-------------|-----------|------|---------|
| btnTiltDn | TiltDown.png | 57×55px | Camera tilt down |
| btnTiltUp | TiltUp.png | 57×55px | Camera tilt up |
| btn2D | Camera2D64.png | 57×55px | 2D overhead view |
| btn3D | Camera3D64.png | 57×55px | 3D perspective view |
| btnN2D | CameraNorth2D.png | 57×55px | North-locked 2D |
| btnGrid | GridRotate.png | 80×83px | Grid overlay toggle |
| btnDayNightMode | WindowNightMode.png | 57×55px | Day/Night mode |
| btnBrightnessDn | BrightnessDn.png | 83×75px | Decrease brightness |
| btnBrightnessUp | BrightnessUp.png | 84×75px | Increase brightness |

---

## Button Styling (from legacy code)

**Common Properties:**
- BackColor: Transparent
- FlatStyle: Flat
- FlatAppearance.BorderSize: 0
- Font: Tahoma, 14.25pt Bold (most buttons)
- UseVisualStyleBackColor: false

**Hover/Active States:**
- FlatAppearance.MouseOverBackColor: Transparent
- FlatAppearance.MouseDownBackColor: Transparent
- FlatAppearance.CheckedBackColor: Transparent
- BorderColor: RoyalBlue (when active)

**Button Alignment:**
- Most buttons: Anchor = None (centered in cell)
- TextAlign: BottomCenter (for buttons with text)

---

## Implementation Notes

1. **lblHz Control**: Row 3, Column 1 contains a Label (not a Button), displaying update frequency
2. **Variable Button Sizes**: Rows 0-3 use ~57×55px buttons; Row 4 uses larger buttons (~83×75px)
3. **Grid Button Special**: btnGrid (Row 2, Col 1) is larger at 80×83px
4. **Panel Toggle**: Panel visibility toggled via NavigationButton in panelLeft
5. **Panel Position**: Floating panel appears at (505, 63) in legacy - center-left area
6. **Row Heights**: Each row is 20% of total height (92px per row at 460px total)
7. **Column Widths**: Two equal columns (~89.5px each at 179px total)

---

## Avalonia Implementation Strategy

**Grid Definition:**
```xml
<Grid RowDefinitions="*,*,*,*,*" ColumnDefinitions="*,*">
```

**Button Template:**
```xml
<Button Grid.Row="X" Grid.Column="Y"
        Classes="NavButton"
        Command="{Binding NavButtonXCommand}">
    <Image Source="/Assets/Icons/[IconName].png"
           Width="48" Height="48"/>
</Button>
```

**Label Template (for lblHz):**
```xml
<TextBlock Grid.Row="3" Grid.Column="1"
           Text="60 Hz"
           HorizontalAlignment="Center"
           VerticalAlignment="Center"
           FontSize="12"
           FontWeight="Bold"/>
```

---

## Wave 11 Integration

These buttons will be wired to OpenGL map controls in Wave 11:
- **Camera tilt**: Adjust 3D camera pitch angle
- **2D/3D/N2D**: Switch camera projection modes
- **Grid**: Toggle field grid overlay rendering
- **Day/Night**: Switch color palettes
- **Brightness**: Adjust display gamma/brightness

For now (Wave 10.5), buttons show status messages indicating "Not yet implemented".
