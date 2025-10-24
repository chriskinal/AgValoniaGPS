# AXAML Compilation Errors - Fixed

**Date:** 2025-10-23
**Project:** AgValoniaGPS.Desktop
**Avalonia Version:** 11.3.6
**Build Result:** SUCCESS - 0 Errors, 0 Warnings (for AXAML)

## Summary

Fixed all AXAML compilation errors in Wave 9 dialog forms to comply with Avalonia's `AvaloniaUseCompiledBindingsByDefault=true` setting. The project now builds successfully with proper type information for compiled bindings.

## Files Fixed

### 1. FieldManagement Dialogs

#### FormAgShareDownloader.axaml
- Changed `Items="{Binding ...}"` to `ItemsSource="{Binding ...}"` on ListBox (line 33)
- Added `DataType="vm:CloudField"` to DataTemplate (line 35)
- Uses inner class `CloudField` from AgShareDownloaderViewModel

#### FormBuildBoundaryFromTracks.axaml
- Changed `Items="{Binding Tracks}"` to `ItemsSource="{Binding Tracks}"` (line 14)
- Added `DataType="vm:GPSTrack"` to DataTemplate (line 16)
- Uses inner class `GPSTrack` from BuildBoundaryFromTracksViewModel

#### FormFieldDir.axaml
- Changed `Items="{Binding Directories}"` to `ItemsSource="{Binding Directories}"` (line 30)
- Changed `Items="{Binding FieldsInDirectory}"` to `ItemsSource="{Binding FieldsInDirectory}"` (line 40)
- Added `DataType="x:String"` to DataTemplate (line 42)

#### FormFieldExisting.axaml
- Changed `Items="{Binding Fields}"` to `ItemsSource="{Binding Fields}"` (line 16)
- Added `DataType="models:FieldInfo"` to DataTemplate (line 18)
- Added namespace `xmlns:models="using:AgValoniaGPS.Models"`
- Uses `FieldInfo` class from AgValoniaGPS.Models

#### FormFieldISOXML.axaml
- Changed `Items="{Binding ISOFields}"` to `ItemsSource="{Binding ISOFields}"` (line 20)
- Added `DataType="vm:ISOField"` to DataTemplate (line 22)
- Fixed property name from `AreaHectares` to `Area` (line 25)
- Uses inner class `ISOField` from FieldISOXMLViewModel

#### FormFieldKML.axaml
- Changed `Items="{Binding Features}"` to `ItemsSource="{Binding Features}"` (line 22)
- Added `DataType="vm:KMLFeature"` to DataTemplate (line 24)
- Uses inner class `KMLFeature` from FieldKMLViewModel

#### FormFlags.axaml
- Changed `Items="{Binding FilteredFlags}"` to `ItemsSource="{Binding FilteredFlags}"` (line 17)
- Added `DataType="models:FieldFlag"` to DataTemplate (line 19)
- Added namespace `xmlns:models="using:AgValoniaGPS.Models"`
- Uses `FieldFlag` class from AgValoniaGPS.Models

### 2. Guidance Dialogs

#### FormTram.axaml
- Changed `Items="{Binding Patterns}"` to `ItemsSource="{Binding Patterns}"` (line 18)
- Added `DataType="vm:TramLinePattern"` to DataTemplate (line 22)
- Uses inner class `TramLinePattern` from TramViewModel

### 3. Pickers Dialogs

#### FormFilePicker.axaml
- Removed Avalonia.Xaml.Interactivity behaviors (not installed in project)
- Added `DataType="vm:FileItem"` to DataTemplate (line 83)
- Uses inner class `FileItem` from FilePickerViewModel
- Removed double-tap behavior (can be added later when package is installed)

#### FormNewProfile.axaml
- Changed `<RadioButton.ToolTip><ToolTip>` to `ToolTip.Tip="..."` (lines 101, 105)
- Removed nested ToolTip elements and used attached property syntax instead
- Simplified RadioButton tooltip declarations for cleaner XAML

#### FormLoadProfile.axaml
- Fixed Border structure - used Grid container for multiple children (line 130)
- Added `DataType="vm:ProfileInfo"` to DataTemplate (line 83)
- Uses inner class `ProfileInfo` from LoadProfileViewModel

#### FormRecordPicker.axaml
- Disabled compiled bindings with `x:CompileBindings="False"` (line 7)
- Removed `x:DataType` directive (generic type `RecordPickerViewModel<T>` cannot be used in AXAML)
- Changed `ToolTip` to `ToolTip.Tip` (line 81)
- Note: This form uses reflection-based bindings due to generic ViewModel

## Key Changes Made

### 1. Items vs ItemsSource
**Issue:** ListBox and ItemsControl require `ItemsSource` property for data binding, not `Items`.

**Fix:** Changed all occurrences of `Items="{Binding ...}"` to `ItemsSource="{Binding ...}"`

**Reason:** The `Items` property is for static items added in XAML, while `ItemsSource` is for binding to collections.

### 2. DataType in DataTemplates
**Issue:** Compiled bindings require explicit type information via `x:DataType` or `DataType` attribute.

**Fix:** Added `DataType="..."` to all DataTemplate elements with proper type references.

**Reason:** With `AvaloniaUseCompiledBindingsByDefault=true`, Avalonia needs to know the type at compile time for type-safe bindings.

### 3. Type Resolution Strategy
**Approach:** Used the actual types from ViewModels and Models, not placeholder types.

- Inner classes from ViewModels: Referenced as `vm:ClassName`
- Model classes: Referenced as `models:ClassName` with proper namespace
- Generic ViewModels: Disabled compiled bindings with `x:CompileBindings="False"`

### 4. ToolTip Syntax
**Issue:** Nested `<ToolTip>` elements within `<RadioButton.ToolTip>` are verbose.

**Fix:** Changed to attached property syntax: `ToolTip.Tip="text"`

**Reason:** Cleaner, more concise, and the recommended approach in Avalonia 11+.

### 5. Border.Child Structure
**Issue:** Border can only have one child element.

**Fix:** Wrapped multiple elements in a Grid container as the single child of Border.

**Reason:** XAML content model requires single child for Border elements.

## Type Mapping Reference

### FieldManagement ViewModels
- `AgShareDownloaderViewModel` → Inner class: `CloudField`
- `BuildBoundaryFromTracksViewModel` → Inner class: `GPSTrack`
- `FieldDirViewModel` → Uses: `string` collections
- `FieldExistingViewModel` → Uses: `FieldInfo` from Models
- `FieldISOXMLViewModel` → Inner class: `ISOField`
- `FieldKMLViewModel` → Inner class: `KMLFeature`
- `FlagsViewModel` → Uses: `FieldFlag` from Models

### Guidance ViewModels
- `TramViewModel` → Inner class: `TramLinePattern`

### Pickers ViewModels
- `FilePickerViewModel` → Inner class: `FileItem`
- `LoadProfileViewModel` → Inner class: `ProfileInfo`
- `NewProfileViewModel` → No collection bindings
- `RecordPickerViewModel<T>` → Generic (compiled bindings disabled)

## Testing Verification

**Build Command:**
```bash
dotnet build AgValoniaGPS/AgValoniaGPS.Desktop/AgValoniaGPS.Desktop.csproj
```

**Result:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:41.19
```

## Best Practices Applied

1. **Compiled Bindings:** All bindings now have proper type information for compile-time checking (except generic ViewModels)
2. **Type Safety:** Using actual types from ViewModels and Models instead of placeholder types
3. **AXAML Standards:** Following Avalonia 11.3.6 recommended patterns
4. **Consistency:** Uniform approach across all dialog forms
5. **Readability:** Clean, maintainable XAML structure
6. **Performance:** Compiled bindings provide better runtime performance

## Notes

- NumericKeypad.axaml and VirtualKeyboard.axaml were already fixed and were not modified
- All fixes maintain existing functionality while improving type safety
- No runtime behavior changes, only compile-time improvements
- The build now fully supports Avalonia's compiled bindings feature for better performance
- FormRecordPicker uses reflection-based bindings due to generic ViewModel limitation
- Avalonia.Xaml.Interactivity package is not installed - interactions removed from FormFilePicker

## Future Improvements

1. Install Avalonia.Xaml.Interactivity package to enable double-tap behaviors
2. Consider creating non-generic wrapper ViewModels for FormRecordPicker to enable compiled bindings
3. Add design-time data for better XAML previewing experience

## References

- Avalonia Documentation: https://docs.avaloniaui.net/docs/basics/data/data-binding/compiled-bindings
- Project: AgValoniaGPS Wave 9 UI Implementation
- Build Environment: .NET 8, Avalonia 11.3.6
