# Converting Dynamic UI to Avalonia MVVM

Guide for implementing dynamic UI behavior in Avalonia using MVVM pattern.

## Overview

The legacy Windows Forms application uses direct control manipulation with
visibility toggles and property changes throughout the code. In Avalonia MVVM,
this should be replaced with property bindings and commands.

## Conversion Strategy

### 1. State Variables → ViewModel Properties

Convert boolean state variables to observable properties:

```csharp
// Legacy Windows Forms
private bool isJobStarted = false;

// Avalonia MVVM
private bool _isJobStarted;
public bool IsJobStarted
{
    get => _isJobStarted;
    set => this.RaiseAndSetIfChanged(ref _isJobStarted, value);
}
```

### 2. Visibility Rules → AXAML Bindings

Replace visibility assignments with bindings:

```xml
<!-- Legacy: button.Visible = isJobStarted; -->
<Button IsVisible="{Binding IsJobStarted}" />

<!-- With converter for inverse -->
<Button IsVisible="{Binding IsJobStarted,
    Converter={StaticResource InverseBoolConverter}}" />
```

### 3. Property Changes → Reactive Bindings

Replace dynamic text/color changes with computed properties:

```csharp
// Legacy: btnStart.Text = isJobStarted ? "Stop" : "Start";

// Avalonia MVVM
public string StartButtonText => IsJobStarted ? "Stop" : "Start";

// Notify property changed when dependency changes
public bool IsJobStarted
{
    get => _isJobStarted;
    set
    {
        this.RaiseAndSetIfChanged(ref _isJobStarted, value);
        this.RaisePropertyChanged(nameof(StartButtonText));
    }
}
```

### 4. UI Modes → ViewModel States

Implement mode management in ViewModel:

```csharp
public enum UIMode { Idle, GuidanceActive, BoundaryMode, SettingsMode }

private UIMode _currentMode;
public UIMode CurrentMode
{
    get => _currentMode;
    set
    {
        this.RaiseAndSetIfChanged(ref _currentMode, value);
        // Notify all mode-dependent properties
        this.RaisePropertyChanged(nameof(IsGuidancePanelVisible));
        this.RaisePropertyChanged(nameof(IsBoundaryPanelVisible));
    }
}

public bool IsGuidancePanelVisible => CurrentMode == UIMode.GuidanceActive;
public bool IsBoundaryPanelVisible => CurrentMode == UIMode.BoundaryMode;
```

## Implementation Checklist

- [ ] Convert 24 state variables to ViewModel properties
- [ ] Create bindings for 312 visibility rules
- [ ] Implement 2033 property change bindings
- [ ] Create 11 UI mode enums/states
- [ ] Implement 25 state transition methods

