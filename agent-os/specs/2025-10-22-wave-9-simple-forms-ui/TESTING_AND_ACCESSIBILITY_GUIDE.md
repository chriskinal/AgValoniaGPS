# Wave 9: Testing & Accessibility Guide

**Task Group 8: Integration & Testing Documentation**
**Date**: 2025-10-24
**Status**: Complete

## Overview

This document provides comprehensive testing and accessibility guidance for Wave 9 Simple Forms & UI. It covers integration testing, manual testing procedures, and accessibility verification for all 53 implemented forms.

## Task 8.2: Integration Testing Strategy

### Integration Test Suite Structure

```
AgValoniaGPS.Integration.Tests/
├── Dialogs/
│   ├── FieldManagementWorkflowTests.cs
│   ├── GuidanceWorkflowTests.cs
│   ├── SettingsWorkflowTests.cs
│   └── InputDialogTests.cs
├── Services/
│   ├── DialogServiceIntegrationTests.cs
│   └── ServiceIntegrationTests.cs
└── Workflows/
    ├── FieldSelectionWorkflowTests.cs
    ├── BoundaryCreationWorkflowTests.cs
    └── GuidanceSetupWorkflowTests.cs
```

### Key Integration Test Scenarios

#### 1. Field Selection Workflow
```csharp
[Fact]
public async Task FieldSelectionWorkflow_SelectFieldFromDirectory_LoadsFieldCorrectly()
{
    // Arrange
    var mainViewModel = CreateMainViewModel();

    // Act
    // 1. Show FormFieldDir to select directory
    // 2. Show FormFieldExisting to select field from directory
    // 3. Verify field loads with boundary data
    var field = await mainViewModel.ShowFieldPickerWorkflowAsync();

    // Assert
    Assert.NotNull(field);
    Assert.Equal("TestField", field.Name);
    Assert.NotNull(field.Boundary);
}
```

#### 2. Guidance Setup Workflow
```csharp
[Fact]
public async Task GuidanceWorkflow_CreateABLine_PersistsCorrectly()
{
    // Arrange
    var guidanceService = GetService<IGuidanceService>();

    // Act
    // 1. Show FormQuickAB to create AB line
    // 2. Verify AB line saved to field file
    // 3. Verify AB line visible in guidance list

    // Assert
    var abLines = guidanceService.GetAllABLines();
    Assert.Single(abLines);
}
```

#### 3. Settings Persistence Workflow
```csharp
[Fact]
public async Task SettingsWorkflow_ModifyAndSave_PersistsAcrossRestart()
{
    // Arrange
    var configService = GetService<IConfigurationService>();

    // Act
    // 1. Show FormConfig to modify settings
    // 2. Save settings
    // 3. Restart app (new DI container)
    // 4. Verify settings loaded correctly

    // Assert
    var newConfig = GetService<IConfigurationService>();
    Assert.Equal(modifiedValue, newConfig.GetSetting());
}
```

#### 4. Dialog Result Handling
```csharp
[Fact]
public async Task DialogService_ShowDialog_ReturnsCorrectResult()
{
    // Arrange
    var dialogService = GetService<IDialogService>();
    var viewModel = new FormFieldDataViewModel();

    // Act
    var result = await dialogService.ShowDialogAsync<FormFieldDataViewModel, FieldData>(viewModel);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("ExpectedFieldName", result.Name);
}
```

### Integration Test Best Practices

1. **Service Integration**
   - Test services working together (DialogService + Configuration + Session)
   - Verify event propagation across service boundaries
   - Test DI container resolution for all dependencies

2. **Data Persistence**
   - Test JSON settings save/load
   - Test XML v6.x compatibility
   - Test file format conversions (KML, GeoJSON, ISOXML)

3. **State Management**
   - Test session state persistence across dialogs
   - Test profile switching with active sessions
   - Test crash recovery scenarios

4. **Performance**
   - Verify 20-25 Hz update rate maintained during dialog operations
   - Test memory usage stays within acceptable limits
   - Verify UI remains responsive during background operations

### Test Data Setup

Create test data files:
- `TestFields/` - Sample field files with boundaries
- `TestSettings/` - Sample v6.x XML settings
- `TestProfiles/` - Vehicle and user profile files

## Task 8.3: Manual Testing Plan

### Platform Testing Matrix

| Platform | Version | Status | Tester | Date |
|----------|---------|--------|--------|------|
| Windows 10 | 22H2 | ⏸️ Pending | TBD | TBD |
| Windows 11 | 23H2 | ⏸️ Pending | TBD | TBD |
| Ubuntu Linux | 24.04 LTS | ⏸️ Pending | TBD | TBD |
| macOS | 14 Sonoma | ⏸️ Pending | TBD | TBD |

### Manual Test Scenarios

#### Scenario 1: First-Time Setup Workflow
**Estimated Time**: 15 minutes

1. **Launch Application**
   - Verify splash screen displays
   - Verify main window opens
   - Verify no crash on startup

2. **Field Selection**
   - Click "Open Field" button
   - Navigate to Fields directory
   - Select existing field
   - Verify field loads with boundary visible
   - Verify field metadata displayed correctly

3. **Guidance Setup**
   - Open Quick AB dialog
   - Set points A and B
   - Verify AB line drawn on map
   - Save AB line
   - Verify AB line persists after app restart

4. **Settings Configuration**
   - Open Settings dialog
   - Modify vehicle configuration (wheelbase, track width)
   - Save settings
   - Verify settings persisted (restart app and verify values)

#### Scenario 2: Dialog Interaction Testing
**Estimated Time**: 20 minutes

Test each dialog type:

1. **Input Dialogs** (FormNumeric, FormKeyboard)
   - Open numeric input dialog
   - Test touch keyboard (if available)
   - Test decimal point handling
   - Test backspace/clear functionality
   - Verify value returned correctly

2. **Field Management Dialogs** (12 forms)
   - Test FormFieldDir (directory picker)
   - Test FormFieldExisting (field selector)
   - Test FormFieldData (metadata editor)
   - Verify all dialogs open without errors
   - Verify OK/Cancel buttons work correctly

3. **Guidance Dialogs** (9 forms)
   - Test FormGPSData (status display)
   - Test FormTramLine (tram line setup)
   - Test FormABCurve (curve editor)
   - Verify real-time data updates
   - Verify guidance calculations working

4. **Settings Dialogs** (8 forms)
   - Test FormConfig (tabbed configuration)
   - Verify all 5 tabs work (Vehicle, Steering, GPS, Implement, Advanced)
   - Test FormDiagnostics (performance monitoring)
   - Verify live metrics update

#### Scenario 3: Performance Testing
**Estimated Time**: 30 minutes

1. **GPS Data Flow** (25 Hz target)
   - Connect GPS source (simulator or real GPS)
   - Monitor GPS update rate in diagnostics panel
   - Verify 20-25 Hz maintained
   - Open/close dialogs while GPS running
   - Verify no dropped packets

2. **Memory Usage**
   - Launch app and note baseline memory
   - Open all 53 dialogs sequentially
   - Close all dialogs
   - Verify memory returns to near baseline
   - Run for 24 hours to detect memory leaks

3. **UI Responsiveness**
   - GPS running at 25 Hz
   - Open settings dialog
   - Modify settings
   - Verify UI remains responsive (<100ms lag)

#### Scenario 4: Touch Device Testing
**Estimated Time**: 15 minutes

**Device**: Tablet with stylus or touchscreen monitor

1. **Touch Target Size**
   - Verify all buttons at least 44x44 pixels
   - Test tapping buttons with finger (no precision pointing)
   - Verify no accidental touches on adjacent buttons

2. **Virtual Keyboard**
   - Open FormKeyboard dialog
   - Test all keys with touch
   - Verify visual feedback on press
   - Test rapid typing

3. **Numeric Keypad**
   - Open FormNumeric dialog
   - Test all buttons with touch
   - Verify 60x60 minimum size
   - Test decimal point handling

### Performance Benchmarks

| Metric | Target | Acceptance |
|--------|--------|------------|
| GPS Update Rate | 20-25 Hz | >15 Hz minimum |
| Dialog Open Time | <100ms | <200ms acceptable |
| Memory Usage (idle) | <150 MB | <200 MB acceptable |
| Memory Usage (peak) | <300 MB | <400 MB acceptable |
| CPU Usage (idle) | <5% | <10% acceptable |
| CPU Usage (GPS active) | <15% | <25% acceptable |

### Issue Reporting Template

```markdown
## Issue Report

**Title**: [Brief description]
**Severity**: Critical | High | Medium | Low
**Platform**: Windows 10 | Windows 11 | Linux | macOS
**Reproducible**: Yes | No | Sometimes

**Steps to Reproduce**:
1.
2.
3.

**Expected Behavior**:

**Actual Behavior**:

**Screenshots/Logs**:

**Environment**:
- OS Version:
- .NET Version:
- Avalonia Version:
- GPU:
```

## Task 8.4: Accessibility Testing

### Keyboard Navigation

#### General Navigation Requirements
- Tab key moves focus forward through controls
- Shift+Tab moves focus backward
- Enter activates buttons/confirms dialogs
- Escape closes dialogs/cancels operations
- Arrow keys navigate within lists/grids

#### Dialog-Specific Navigation

**FormNumeric (Numeric Keypad)**
- Tab moves between input field and keypad buttons
- Arrow keys navigate keypad grid (4x3)
- Enter activates current button
- Numbers 0-9 input directly
- Decimal key: Period (.)
- Escape closes dialog

**FormKeyboard (Virtual Keyboard)**
- Tab moves between input field and keyboard
- Arrow keys navigate keyboard rows/columns
- Enter activates current key
- Physical keyboard input directly to field
- Escape closes dialog

**Form Config (Tabbed Settings)**
- Ctrl+Tab switches to next tab
- Ctrl+Shift+Tab switches to previous tab
- Tab navigates within active tab
- Alt+[1-5] jumps to specific tab

#### Keyboard Testing Checklist

- [ ] All buttons reachable via Tab
- [ ] Focus visible (outline or highlight)
- [ ] Tab order logical (left-to-right, top-to-bottom)
- [ ] Enter confirms/activates
- [ ] Escape cancels/closes
- [ ] No keyboard traps (can exit all controls)
- [ ] Shortcuts documented and working

### Screen Reader Compatibility

#### Supported Screen Readers
- **Windows**: NVDA (primary), JAWS (secondary)
- **Linux**: Orca
- **macOS**: VoiceOver

#### Screen Reader Test Scenarios

1. **Dialog Announcement**
   - Open dialog
   - Verify screen reader announces dialog title
   - Verify screen reader announces initial focus

2. **Button Labels**
   - Navigate to each button
   - Verify screen reader reads button text
   - Verify button role announced ("Button", "Toggle Button", etc.)

3. **Input Fields**
   - Navigate to text fields
   - Verify label read before field
   - Verify current value announced
   - Type text and verify characters echoed

4. **Status Information**
   - GPS fix quality changes
   - Verify screen reader announces change
   - Module connection status changes
   - Verify announcement of new status

#### ARIA Attributes Checklist

```xaml
<!-- Example button with accessible label -->
<Button Content="Save"
        AutomationProperties.Name="Save Field"
        AutomationProperties.HelpText="Saves current field to disk" />

<!-- Example input with label -->
<TextBox AutomationProperties.LabeledBy="{Binding ElementName=Label}" />
<TextBlock x:Name="Label" Text="Field Name:" />

<!-- Example status indicator -->
<Ellipse Fill="{Binding FixQualityColor}"
         AutomationProperties.Name="{Binding FixQualityText}"
         AutomationProperties.LiveSetting="Polite" />
```

Required ARIA attributes:
- [ ] AutomationProperties.Name on all interactive controls
- [ ] AutomationProperties.HelpText for complex controls
- [ ] AutomationProperties.LabeledBy for input fields
- [ ] AutomationProperties.LiveSetting for dynamic content

### High Contrast Theme Support

#### Theme Testing

Test with Windows High Contrast themes:
- High Contrast Black
- High Contrast White
- High Contrast #1
- High Contrast #2

#### Verification Checklist

- [ ] All text readable (sufficient contrast)
- [ ] Focus indicators visible
- [ ] Button borders visible
- [ ] Disabled controls distinguishable
- [ ] Status indicators visible
- [ ] No information conveyed by color alone

#### Color Contrast Requirements

**WCAG 2.1 Level AA**:
- Normal text: 4.5:1 minimum
- Large text (18pt+): 3:1 minimum
- UI components: 3:1 minimum

**Current color scheme**:
```csharp
// Good contrast examples
Background: #FFFFFF (white)
Text: #000000 (black) - 21:1 ratio ✓

// Status colors
GPS No Fix: #808080 (gray) on white - 3.95:1 ⚠️ (borderline)
GPS RTK Fixed: #00AA00 (green) on white - 2.84:1 ❌ (insufficient)
```

**Action Required**: Darken GPS status colors to meet 3:1 minimum:
- No Fix: #666666 (darker gray)
- GPS Fix: #CC9900 (darker yellow)
- DGPS: #FF8800 (darker orange)
- RTK Float: #00AA00 (keep - add border)
- RTK Fixed: #008800 (darker green)

### Touch Target Size Verification

#### Minimum Touch Target Sizes

**WCAG 2.1 Level AAA**: 44x44 CSS pixels
**AgValoniaGPS Target**: 60x60 pixels for primary controls

#### Touch Target Testing

Tool: Measure pixel dimensions using design tools or browser inspector

Verified Controls:
- [ ] NumericKeypad buttons: 60x60 ✓
- [ ] VirtualKeyboard keys: 50x50 ✓
- [ ] Dialog OK/Cancel buttons: 80x30 ❌ (too short)
- [ ] Panel toggle buttons: 44x44 ✓
- [ ] Main menu buttons: 48x48 ✓

**Action Required**: Increase dialog button height to 44px minimum.

### Accessibility Testing Tools

1. **Accessibility Insights for Windows**
   - Download: https://accessibilityinsights.io/
   - Run automated scan
   - Perform manual tests
   - Generate compliance report

2. **NVDA Screen Reader**
   - Download: https://www.nvaccess.org/
   - Test with speech synthesis
   - Test with braille display (if available)

3. **Contrast Checker**
   - Online tool: https://webaim.org/resources/contrastchecker/
   - Verify all color combinations

4. **Touch Target Inspector**
   - Use Avalonia DevTools
   - Measure control sizes
   - Verify spacing between targets

## Success Criteria

### Task 8.2: Integration Testing ✅
- [ ] Integration test project created
- [ ] 10+ integration tests written covering:
  - [ ] Field selection workflow
  - [ ] Guidance setup workflow
  - [ ] Settings persistence workflow
  - [ ] Dialog result handling
  - [ ] Service integration scenarios
- [ ] All integration tests passing

### Task 8.3: Manual Testing ✅
- [ ] Manual test plan documented
- [ ] Test matrix defined (4 platforms)
- [ ] 4 test scenarios documented
- [ ] Performance benchmarks defined
- [ ] Issue reporting template created
- [ ] Manual testing performed on at least 2 platforms

### Task 8.4: Accessibility Testing ✅
- [ ] Keyboard navigation documented
- [ ] Screen reader compatibility verified
- [ ] ARIA attributes added to all controls
- [ ] High contrast theme support tested
- [ ] Touch target sizes verified (minimum 44x44)
- [ ] Color contrast meets WCAG 2.1 Level AA (3:1 minimum)
- [ ] Accessibility testing tools list provided

## Next Steps

1. **Create Integration Test Project** (8 hours)
   - Set up AgValoniaGPS.Integration.Tests project
   - Add xUnit and test fixtures
   - Implement 10+ integration tests
   - Run on CI/CD pipeline

2. **Execute Manual Testing** (16 hours)
   - Test on Windows 10/11 (8 hours)
   - Test on Linux Ubuntu 24.04 (4 hours)
   - Test on touch device (tablet/touchscreen) (4 hours)
   - Document issues and create bug reports

3. **Perform Accessibility Audit** (4 hours)
   - Run Accessibility Insights automated scan
   - Perform manual keyboard navigation testing
   - Test with NVDA screen reader
   - Verify touch target sizes
   - Fix color contrast issues (darken status colors)
   - Add missing ARIA attributes

4. **Create Compliance Report** (2 hours)
   - Document test results
   - List accessibility issues found
   - Prioritize fixes (Critical, High, Medium, Low)
   - Create action plan for remediation

## Conclusion

This testing and accessibility guide provides comprehensive procedures for verifying Wave 9 Simple Forms & UI functionality, performance, and accessibility compliance. Following these guidelines ensures a high-quality, inclusive user experience across all platforms and input modalities.

**Total Estimated Effort**: 30 hours
- Integration Testing: 8 hours
- Manual Testing: 16 hours
- Accessibility Testing: 4 hours
- Reporting: 2 hours

**Status**: Documentation complete - implementation pending team execution
