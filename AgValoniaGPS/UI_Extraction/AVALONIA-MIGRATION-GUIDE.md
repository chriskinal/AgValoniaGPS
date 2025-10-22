# AgOpenGPS to Avalonia Migration Guide

**Generated**: 2025-10-21 20:34:21

## Overview

This guide covers the migration of 74 forms from Windows Forms to Avalonia UI.

## Executive Summary

- **Total Forms**: 74
- **Total Controls**: 10,774
- **Complex Forms (>500 controls)**: 6
- **Moderate Forms (100-500 controls)**: 15
- **Simple Forms (<100 controls)**: 53
- **Total Event Handlers**: 670

### Migration Effort Estimate

Based on control complexity:
- **High Effort**: 6 forms (estimated 40-80 hours each)
- **Medium Effort**: 15 forms (estimated 8-20 hours each)
- **Low Effort**: 53 forms (estimated 2-4 hours each)

**Total Estimated Effort**: 466-992 hours

## Control Mapping Reference

### Windows Forms → Avalonia

| Windows Forms Control | Avalonia Equivalent | Notes |
|----------------------|---------------------|-------|
| Button ✓ | Button | Direct equivalent available |
| CheckBox ✓ | CheckBox | Direct equivalent available |
| ColorDialog  | Custom color picker (no built-in) | No built-in, use community package |
| ComboBox ✓ | ComboBox | Direct equivalent available |
| ContextMenuStrip  | ContextMenu | Direct equivalent available |
| DataGridView  | DataGrid | Feature-rich DataGrid available |
| FlowLayoutPanel ✓ | WrapPanel | Direct equivalent available |
| FolderBrowserDialog  | StorageProvider.OpenFolderPickerAsync | Direct equivalent available |
| GLControl ✓ | OpenGlControlBase (Avalonia.OpenGL) | Requires Avalonia.OpenGL package |
| GroupBox ✓ | Border with HeaderedContentControl | Combine Border + TextBlock for header |
| Label ✓ | TextBlock | Direct equivalent available |
| ListBox  | ListBox | Direct equivalent available |
| ListView ✓ | ListBox or DataGrid | Direct equivalent available |
| MenuStrip  | Menu | Direct equivalent available |
| MessageBox  | Window with custom content or ContentDialog | Direct equivalent available |
| NumericUpDown  | NumericUpDown | Available via NuGet |
| OpenFileDialog  | StorageProvider.OpenFilePickerAsync | Direct equivalent available |
| Panel ✓ | Panel or StackPanel or Grid | Choose based on layout needs |
| PictureBox ✓ | Image | Direct equivalent available |
| ProgressBar ✓ | ProgressBar | Direct equivalent available |
| RadioButton ✓ | RadioButton | Direct equivalent available |
| SaveFileDialog  | StorageProvider.SaveFilePickerAsync | Direct equivalent available |
| SplitContainer  | Grid with GridSplitter | Use Grid with GridSplitter for resizable panels |
| StatusStrip ✓ | StatusBar | Direct equivalent available |
| TabControl ✓ | TabControl | Direct equivalent available |
| TableLayoutPanel ✓ | Grid | Direct equivalent available |
| TabPage ✓ | TabItem | Direct equivalent available |
| TextBox ✓ | TextBox | Direct equivalent available |
| ToolStrip  | ToolBar | Direct equivalent available |
| ToolStripButton  | Button in ToolBar | Direct equivalent available |
| ToolStripMenuItem  | MenuItem | Direct equivalent available |
| TrackBar ✓ | Slider | Direct equivalent available |
| TreeView ✓ | TreeView | Direct equivalent available |

✓ = Used in extracted forms

## Form-by-Form Migration Plan

Forms ordered by migration priority (simplest to most complex):

### 1. Controls

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 2. GUI

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 3. OpenGL

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 4. PGN

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 5. SaveOpen

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 6. Sections

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 7. UDPComm

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 8. ConfigData

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 9. ConfigHelp

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 10. ConfigMenu

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 11. ConfigModule

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 12. ConfigTool

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 13. ConfigVehicle

- **Complexity**: ✅ Low
- **Total Controls**: 0
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 0
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours

### 14. FormEventViewer

- **Complexity**: ✅ Low
- **Total Controls**: 17
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 17
- **Event Handlers**: 3
- **Estimated Effort**: 2-4 hours
- **Title**: Event Log Viewer

### 15. FormSaving

- **Complexity**: ✅ Low
- **Total Controls**: 17
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 17
- **Event Handlers**: 0
- **Estimated Effort**: 2-4 hours
- **Title**: FormSaving

### 16. FormTimedMessage

- **Complexity**: ✅ Low
- **Total Controls**: 19
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 19
- **Event Handlers**: 1
- **Estimated Effort**: 2-4 hours
- **Title**: AgOpenGPS Message

### 17. FormDrivePicker

- **Complexity**: ✅ Low
- **Total Controls**: 22
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 22
- **Event Handlers**: 3
- **Estimated Effort**: 2-4 hours
- **Title**: FormFilePicker

### 18. FormDialog

- **Complexity**: ✅ Low
- **Total Controls**: 23
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 23
- **Event Handlers**: 2
- **Estimated Effort**: 2-4 hours
- **Title**: FormDialog

### 19. FormKeyboard

- **Complexity**: ✅ Low
- **Total Controls**: 23
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 23
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: Keyboard

### 20. FormWebCam

- **Complexity**: ✅ Low
- **Total Controls**: 24
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 24
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: WebCam

### 21. FormSmoothAB

- **Complexity**: ✅ Low
- **Total Controls**: 33
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 33
- **Event Handlers**: 6
- **Estimated Effort**: 2-4 hours
- **Title**: Smooth AB Curve

### 22. FormNumeric

- **Complexity**: ✅ Low
- **Total Controls**: 33
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 33
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: Enter a Value

### 23. FormLoadProfile

- **Complexity**: ✅ Low
- **Total Controls**: 33
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 33
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: Load Profile

### 24. FormAgShareDownloader

- **Complexity**: ✅ Low
- **Total Controls**: 38
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 38
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: Download Fields From AgShare

### 25. FormGrid

- **Complexity**: ✅ Low
- **Total Controls**: 39
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 39
- **Event Handlers**: 12
- **Estimated Effort**: 2-4 hours
- **Title**: 2 Points - Align Grid

### 26. FormNewProfile

- **Complexity**: ✅ Low
- **Total Controls**: 39
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 39
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: Create New Profile

### 27. FormFilePicker

- **Complexity**: ✅ Low
- **Total Controls**: 41
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 41
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: FormFilePicker

### 28. FormRecordPicker

- **Complexity**: ✅ Low
- **Total Controls**: 41
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 41
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: FormRecordPicker

### 29. Form_Help

- **Complexity**: ✅ Low
- **Total Controls**: 44
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 44
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: AgOpenGPS Help

### 30. FormFieldDir

- **Complexity**: ✅ Low
- **Total Controls**: 45
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 45
- **Event Handlers**: 7
- **Estimated Effort**: 2-4 hours
- **Title**: Create New Field 

### 31. FormPan

- **Complexity**: ✅ Low
- **Total Controls**: 49
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 49
- **Event Handlers**: 10
- **Estimated Effort**: 2-4 hours

### 32. FormFieldKML

- **Complexity**: ✅ Low
- **Total Controls**: 49
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 49
- **Event Handlers**: 8
- **Estimated Effort**: 2-4 hours
- **Title**: Create New Field 

### 33. FormGraphHeading

- **Complexity**: ✅ Low
- **Total Controls**: 49
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 49
- **Event Handlers**: 3
- **Estimated Effort**: 2-4 hours
- **Title**: Roll Correction Graph

### 34. FormRecordName

- **Complexity**: ✅ Low
- **Total Controls**: 50
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 50
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: Create New Record

### 35. FormSimCoords

- **Complexity**: ✅ Low
- **Total Controls**: 53
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 53
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: Enter Coordinates For Simulator

### 36. FormFieldISOXML

- **Complexity**: ✅ Low
- **Total Controls**: 54
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 54
- **Event Handlers**: 8
- **Estimated Effort**: 2-4 hours
- **Title**: Create New Field From ISO-XML

### 37. FormGraphSteer

- **Complexity**: ✅ Low
- **Total Controls**: 55
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 55
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: AutoSteer Graph

### 38. FormGraphXTE

- **Complexity**: ✅ Low
- **Total Controls**: 56
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 56
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: XTE Graph

### 39. FormAgShareSettings

- **Complexity**: ✅ Low
- **Total Controls**: 57
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 57
- **Event Handlers**: 12
- **Estimated Effort**: 2-4 hours
- **Title**: AgShare Settings

### 40. FormCorrection

- **Complexity**: ✅ Low
- **Total Controls**: 57
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 57
- **Event Handlers**: 4
- **Estimated Effort**: 2-4 hours
- **Title**: Roll Correction Graph

### 41. FormSaveOrNot

- **Complexity**: ✅ Low
- **Total Controls**: 62
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 62
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours

### 42. FormEnterFlag

- **Complexity**: ✅ Low
- **Total Controls**: 71
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 71
- **Event Handlers**: 7
- **Estimated Effort**: 2-4 hours
- **Title**: Enter AB or A+

### 43. FormBuildBoundaryFromTracks

- **Complexity**: ✅ Low
- **Total Controls**: 72
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 72
- **Event Handlers**: 16
- **Estimated Effort**: 2-4 hours
- **Title**: Build Boundary From Tracks

### 44. Form_First

- **Complexity**: ✅ Low
- **Total Controls**: 74
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 74
- **Event Handlers**: 6
- **Estimated Effort**: 2-4 hours
- **Title**: Accept Terms and Conditions

### 45. FormMap

- **Complexity**: ✅ Low
- **Total Controls**: 76
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 76
- **Event Handlers**: 15
- **Estimated Effort**: 2-4 hours
- **Title**: Bing Maps for Background

### 46. FormColor

- **Complexity**: ✅ Low
- **Total Controls**: 81
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 81
- **Event Handlers**: 11
- **Estimated Effort**: 2-4 hours
- **Title**: Color Set

### 47. FormShiftPos

- **Complexity**: ✅ Low
- **Total Controls**: 86
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 86
- **Event Handlers**: 10
- **Estimated Effort**: 2-4 hours
- **Title**: Shift GPS Position (cm)

### 48. FormBoundaryPlayer

- **Complexity**: ✅ Low
- **Total Controls**: 87
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 87
- **Event Handlers**: 12
- **Estimated Effort**: 2-4 hours
- **Title**: Stop Record Pause Boundary

### 49. FormTram

- **Complexity**: ✅ Low
- **Total Controls**: 88
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 88
- **Event Handlers**: 11
- **Estimated Effort**: 2-4 hours
- **Title**: AB Line Tramline

### 50. Form_About

- **Complexity**: ✅ Low
- **Total Controls**: 90
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 90
- **Event Handlers**: 5
- **Estimated Effort**: 2-4 hours
- **Title**: About AgOpenGPS

### 51. FormFlags

- **Complexity**: ✅ Low
- **Total Controls**: 91
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 91
- **Event Handlers**: 9
- **Estimated Effort**: 2-4 hours
- **Title**: Flags

### 52. FormJob

- **Complexity**: ✅ Low
- **Total Controls**: 95
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 95
- **Event Handlers**: 12
- **Estimated Effort**: 2-4 hours

### 53. FormBoundary

- **Complexity**: ✅ Low
- **Total Controls**: 99
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 99
- **Event Handlers**: 14
- **Estimated Effort**: 2-4 hours
- **Title**: Boundary

### 54. ConfigSummaryControl

- **Complexity**: 🟢 Moderate
- **Total Controls**: 103
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 103
- **Event Handlers**: 0
- **Estimated Effort**: 8-20 hours

### 55. FormABDraw

- **Complexity**: 🟢 Moderate
- **Total Controls**: 107
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 107
- **Event Handlers**: 25
- **Estimated Effort**: 8-20 hours
- **Title**: Draw AB - Click 2 points on the Boundary to Begin

### 56. FormHeadLine

- **Complexity**: 🟢 Moderate
- **Total Controls**: 109
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 109
- **Event Handlers**: 22
- **Estimated Effort**: 8-20 hours
- **Title**: Click 2 points on the Boundary to Begin

### 57. FormColorPicker

- **Complexity**: 🟢 Moderate
- **Total Controls**: 113
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 113
- **Event Handlers**: 6
- **Estimated Effort**: 8-20 hours
- **Title**: Color Picker

### 58. FormFieldExisting

- **Complexity**: 🟢 Moderate
- **Total Controls**: 115
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 115
- **Event Handlers**: 12
- **Estimated Effort**: 8-20 hours
- **Title**: Field Save As

### 59. FormHeadAche

- **Complexity**: 🟢 Moderate
- **Total Controls**: 119
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 119
- **Event Handlers**: 24
- **Estimated Effort**: 8-20 hours
- **Title**: Click 2 points on the Boundary to Begin

### 60. FormTramLine

- **Complexity**: 🟢 Moderate
- **Total Controls**: 120
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 120
- **Event Handlers**: 24
- **Estimated Effort**: 8-20 hours
- **Title**: Draw AB - Click 2 points on the Boundary to Begin

### 61. FormButtonsRightPanel

- **Complexity**: 🟢 Moderate
- **Total Controls**: 125
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 125
- **Event Handlers**: 14
- **Estimated Effort**: 8-20 hours
- **Title**: Button Arrange

### 62. FormGPSData

- **Complexity**: 🟢 Moderate
- **Total Controls**: 138
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 138
- **Event Handlers**: 3
- **Estimated Effort**: 8-20 hours
- **Title**: System Data

### 63. FormFieldData

- **Complexity**: 🟢 Moderate
- **Total Controls**: 141
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 141
- **Event Handlers**: 3
- **Estimated Effort**: 8-20 hours
- **Title**: System Data

### 64. FormBndTool

- **Complexity**: 🟢 Moderate
- **Total Controls**: 146
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 146
- **Event Handlers**: 24
- **Estimated Effort**: 8-20 hours
- **Title**: Create Boundary From Mapping

### 65. ConfigVehicleControl

- **Complexity**: 🟢 Moderate
- **Total Controls**: 158
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 158
- **Event Handlers**: 9
- **Estimated Effort**: 8-20 hours

### 66. FormQuickAB

- **Complexity**: 🟢 Moderate
- **Total Controls**: 167
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 167
- **Event Handlers**: 22
- **Estimated Effort**: 8-20 hours
- **Title**: Tracks

### 67. FormColorSection

- **Complexity**: 🟢 Moderate
- **Total Controls**: 191
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 191
- **Event Handlers**: 7
- **Estimated Effort**: 8-20 hours
- **Title**: Section Color Set

### 68. Form_Keys

- **Complexity**: 🟢 Moderate
- **Total Controls**: 217
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 217
- **Event Handlers**: 6
- **Estimated Effort**: 8-20 hours

### 69. FormBuildTracks

- **Complexity**: 🟡 High
- **Total Controls**: 501
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 501
- **Event Handlers**: 57
- **Estimated Effort**: 20-40 hours
- **Title**: Tracks

**Recommendations**:
- Break into smaller ViewModels and UserControls
- Use TabControl or split into separate windows
- Consider refactoring before migration

### 70. FormGPS

- **Complexity**: 🟡 High
- **Total Controls**: 720
  - Menu Items: 52
  - Toolbar Buttons: 8
  - Other Controls: 660
- **Event Handlers**: 5
- **Estimated Effort**: 20-40 hours
- **Title**: AgOpenGPS

**Recommendations**:
- Break into smaller ViewModels and UserControls
- Use TabControl or split into separate windows
- Consider refactoring before migration

### 71. FormSteer

- **Complexity**: 🟡 High
- **Total Controls**: 900
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 900
- **Event Handlers**: 53
- **Estimated Effort**: 20-40 hours
- **Title**: Auto Steer Configuration

**Recommendations**:
- Break into smaller ViewModels and UserControls
- Use TabControl or split into separate windows
- Consider refactoring before migration

### 72. FormAllSettings

- **Complexity**: 🔴 Very High
- **Total Controls**: 1080
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 1080
- **Event Handlers**: 3
- **Estimated Effort**: 40-60 hours
- **Title**: Create Image of Settings or Copy to clipboard to paste in Telegram

**Recommendations**:
- Break into smaller ViewModels and UserControls
- Use TabControl or split into separate windows
- Consider refactoring before migration

### 73. FormSteerWiz

- **Complexity**: 🔴 Very High
- **Total Controls**: 1299
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 1299
- **Event Handlers**: 70
- **Estimated Effort**: 40-60 hours
- **Title**: Auto Steer Wizard

**Recommendations**:
- Break into smaller ViewModels and UserControls
- Use TabControl or split into separate windows
- Consider refactoring before migration

### 74. FormConfig

- **Complexity**: ⚠️ Extreme
- **Total Controls**: 2133
  - Menu Items: 0
  - Toolbar Buttons: 0
  - Other Controls: 2133
- **Event Handlers**: 10
- **Estimated Effort**: 60-80 hours
- **Title**: Configuration

**Recommendations**:
- Break into smaller ViewModels and UserControls
- Use TabControl or split into separate windows
- Consider refactoring before migration

## Architecture Recommendations

### MVVM Pattern

Avalonia strongly encourages MVVM. For each form:

1. **Create a ViewModel**
   - Inherit from `ReactiveObject` or `ViewModelBase`
   - Implement `INotifyPropertyChanged`
   - Move all business logic from code-behind

2. **Create AXAML View**
   - Replace Designer.cs with AXAML markup
   - Use data binding instead of direct control access
   - Use commands instead of event handlers

3. **Dependency Injection**
   - Register ViewModels in DI container
   - Inject services into ViewModels
   - Use `ServiceCollectionExtensions` pattern

### Project Structure

```
AgValoniaGPS/
├── AgValoniaGPS.Models/          # Domain models
├── AgValoniaGPS.ViewModels/      # ViewModels
├── AgValoniaGPS.Services/        # Business logic services
└── AgValoniaGPS.Desktop/         # Avalonia UI
    ├── Views/                    # AXAML views
    ├── Controls/                 # Custom controls
    └── Converters/               # Value converters
```

## Event Handler Migration

### Common Event Patterns

| Windows Forms Event | Avalonia Equivalent |
|---------------------|---------------------|
| Click | Command binding or Tapped event |
| TextChanged | PropertyChanged notification |
| SelectedIndexChanged | PropertyChanged on SelectedItem |
| CheckedChanged | PropertyChanged on IsChecked |
| Load | OnInitialized or OnLoaded |
| FormClosing | Window.Closing event |
| KeyDown | KeyDown event or KeyBindings |
| MouseClick | PointerPressed/PointerReleased |
| Paint | Custom control with OnRender |

**Total Event Handlers to Migrate**: 670

Most event handlers should be converted to:
1. **Commands** (ICommand) for user actions
2. **Property bindings** for state changes
3. **ReactiveCommand** for async operations

## Known Migration Challenges

### 1. OpenGL Rendering

**Challenge**: GLControl is Windows Forms specific

**Solution**: Use Avalonia.OpenGL
- Inherit from `OpenGlControlBase`
- Implement `OnOpenGlInit`, `OnOpenGlDeinit`, `OnOpenGlRender`
- Same OpenTK API available

### 2. Complex Forms (>500 controls)

Affected forms: FormGPS, FormBuildTracks, FormAllSettings, FormConfig, FormSteer, FormSteerWiz

**Challenge**: Large forms cause slow startup and poor UX

**Solution**:
- Break into multiple UserControls
- Use lazy loading for tab content
- Consider data virtualization

### 3. Custom Dialogs

**Challenge**: No MessageBox.Show() equivalent

**Solution**:
- Create custom dialog windows
- Use ShowDialog() for modal behavior
- Consider MessageBox.Avalonia NuGet package

### 4. Designer Migration

**Challenge**: No visual designer for Avalonia (yet)

**Solution**:
- Use Avalonia XAML Intellisense in VS/Rider
- Use Avalonia Previewer for live preview
- Hand-code AXAML based on extracted structure

