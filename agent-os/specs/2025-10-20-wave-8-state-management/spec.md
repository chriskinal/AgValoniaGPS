# Specification: Wave 8 - State Management

## Goal
Implement centralized state management infrastructure with configuration services, session management with crash recovery, multi-profile support (vehicles and users), validation services, and undo/redo capabilities for a robust, production-ready application lifecycle.

## User Stories
- As a farmer, I want my application settings to persist between sessions so that I don't have to reconfigure my vehicle every time
- As a farmer with multiple tractors, I want to switch between vehicle profiles so that each tractor uses its correct configuration
- As a farm operator, I want separate user profiles so that multiple operators can maintain their own preferences
- As a farmer, I want the application to recover my work after a crash so that I don't lose field data
- As a farmer, I want to undo accidental changes to field boundaries and guidance lines so that I can correct mistakes
- As a developer, I want validation of all settings so that invalid configurations are caught before they cause problems
- As a new user, I want a setup wizard with sensible defaults so that I can get started quickly

## Core Requirements

### Functional Requirements
- Centralized Configuration Service as single source of truth for all application settings
- Dual-format persistence: hierarchical JSON (primary) with flat XML (legacy compatibility)
- Dual-write strategy: save both JSON and XML on every settings change
- Multi-vehicle profile support with runtime switching without restart
- Multi-user profile support separate from vehicle profiles
- Session management with crash recovery (save/restore current field, work progress, guidance state)
- Validation Service with range checking and cross-setting dependency validation
- Undo/Redo Service for user-initiated actions (boundaries, guidance lines, field edits)
- Application lifecycle management with setup wizard and sensible defaults
- Mediator pattern for state coordination between services
- Optional explicit XML-to-JSON migration tool

### Non-Functional Requirements
- Performance: Settings load <100ms, validation <10ms, crash recovery snapshot <500ms
- All services must be thread-safe for concurrent access
- Atomic dual-write to JSON and XML (both succeed or both fail)
- No settings stored in individual services (Configuration Service is single source of truth)
- Cross-platform file path handling (Windows, Linux, Android)
- 100% test coverage for validation rules and file I/O round-trips

## Visual Design

### Mockup References
- `planning/visuals/All_Settings.png` - Comprehensive legacy settings structure (50+ settings across 11 categories)

### Settings Categories from Visual
The visual shows a flat XML structure with the following categories to be transformed into hierarchical JSON:

1. **Vehicle Settings**: wheelbase (180cm), track (30cm), max steer angle (45°), antenna pivot/height/offset, vehicle type
2. **Steering Settings**: CPD (100), Ackermann (100), WAS offset, PWM ranges (high=235, low=78, min=5, max=10)
3. **Tool Settings**: tool width (4=1.828m actual), tool position flags, look-ahead on/off, delays, offsets, hitches
4. **Section Control Settings**: number sections (3), headland control, fast sections, out-of-bounds behavior
5. **GPS Settings**: min U-turn radius, HDOP, Hz (10.0), GPS age alarm (20), heading source (Dual), auto-start AgIO
6. **IMU Settings**: fusion weight (0.06), min heading step (0.5), roll filter (0.15), invert roll, dual antenna offset (90°)
7. **Guidance Settings**: acquire factor (0.9), look-ahead (3), speed factor (1), snap distances (20, 5)
8. **Work Mode Settings**: remote work, steer work switch, work manual, active low, section switches
9. **Culture Settings**: language code ("en")
10. **System State Settings**: roll zero, Stanley used flag, steer in reverse, reverse on
11. **Display Settings**: dead head delay, north/east/elev coordinates, satellite count, missed packets

## Reusable Components

### Existing Code to Leverage

**Service Registration Pattern** - `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`:
- Established pattern for registering services with DI container
- Wave 8 services will follow same registration approach
- Use Singleton lifetime for Configuration, Session, and StateManagement services

**VehicleConfiguration Model** - `/mnt/c/Users/chris/Documents/code/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Models/VehicleConfiguration.cs`:
- Already defines vehicle physical dimensions and steering parameters
- Contains wheelbase, track width, antenna configuration, steering limits
- Can be extended to include additional vehicle settings from XML
- Represents subset of vehicle settings shown in visual

**Event-Driven Architecture Pattern** - Existing Wave services:
- Wave 1-7 services use EventHandler<TEventArgs> pattern for state changes
- Wave 8 will follow same event pattern for settings changes and session updates
- Example: `PositionUpdateEventArgs` from IPositionUpdateService

**Existing Wave Services** - Integration points:
- Wave 1 (Position & Kinematics): Needs vehicle configuration at startup
- Wave 2 (Guidance): Needs guidance settings (look-ahead, snap distances)
- Wave 3 (Steering): Needs steering algorithm parameters (Stanley, Pure Pursuit gains)
- Wave 4 (Section Control): Needs section configuration (count, positions)
- Wave 5 (Field Operations): Needs field boundary and headland settings
- Wave 6 (Hardware I/O): Needs communication settings (auto-start AgIO, transport config)
- Wave 7 (Display): Needs unit system and display preferences

### New Components Required

**Configuration Service** - Does not exist in current codebase:
- Centralized settings management with in-memory cache
- Cannot reuse existing code because legacy settings are scattered across WinForms UI
- Required to provide single source of truth for all application settings
- Will load from JSON/XML files and serve settings to all other services

**Session Management Service** - Does not exist in current codebase:
- Crash recovery with periodic state snapshots
- Session state persistence (current field, guidance line, work progress)
- Cannot reuse existing code because no crash recovery exists in legacy system
- Required to solve current system problem of losing work on crashes

**Validation Service** - Does not exist in current codebase:
- Range validation (wheelbase 50-500cm, PWM 0-255, sections 1-32)
- Cross-setting dependency validation (tool width affects guidance)
- Cannot reuse existing code because validation is embedded in legacy UI
- Required to prevent invalid configurations before persistence

**StateMediator Service** - Does not exist in current codebase:
- Coordinates state changes across multiple services using Mediator pattern
- Prevents tight coupling between services and central state management
- Cannot reuse existing code because no coordination layer exists
- Required to manage complex state dependencies without service coupling

**Undo/Redo Service** - Does not exist in current codebase:
- Command pattern for user-initiated actions
- Stack-based undo/redo for field boundaries, guidance lines, field edits
- Cannot reuse existing code because no undo capability exists in legacy system
- Required to provide user-friendly editing experience

**Profile Management Service** - Does not exist in current codebase:
- Multi-vehicle and multi-user profile management
- Runtime profile switching without restart
- Cannot reuse existing code because legacy only supports single vehicle per installation
- Required to support farmers with multiple tractors

**Settings Models** - Partial existence:
- VehicleConfiguration exists but needs extension
- New models needed: SteeringSettings, ToolSettings, SectionControlSettings, GpsSettings, ImuSettings, GuidanceSettings, WorkModeSettings, CultureSettings, SystemStateSettings, DisplaySettings
- Required to provide strongly-typed hierarchical structure for JSON

## Technical Approach

### Database
No database required. All settings and session data stored in JSON and XML files on local filesystem.

**File Locations**:
- Settings: `Documents/AgValoniaGPS/Vehicles/[VehicleName].json` and `[VehicleName].xml`
- User Profiles: `Documents/AgValoniaGPS/Users/[UserName].json`
- Session State: `Documents/AgValoniaGPS/Sessions/LastSession.json`
- Crash Recovery: `Documents/AgValoniaGPS/Sessions/CrashRecovery.json` (updated every 30 seconds)

### API

**IConfigurationService Interface**:
```csharp
public interface IConfigurationService
{
    // Raised when any setting changes
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    // Load settings from file (both JSON and XML)
    Task<SettingsLoadResult> LoadSettingsAsync(string vehicleName);

    // Save settings to both JSON and XML formats
    Task<SettingsSaveResult> SaveSettingsAsync();

    // Get settings by category
    VehicleSettings GetVehicleSettings();
    SteeringSettings GetSteeringSettings();
    ToolSettings GetToolSettings();
    SectionControlSettings GetSectionControlSettings();
    GpsSettings GetGpsSettings();
    ImuSettings GetImuSettings();
    GuidanceSettings GetGuidanceSettings();
    WorkModeSettings GetWorkModeSettings();
    CultureSettings GetCultureSettings();
    SystemStateSettings GetSystemStateSettings();
    DisplaySettings GetDisplaySettings();

    // Update settings by category (validates before accepting)
    Task<ValidationResult> UpdateVehicleSettingsAsync(VehicleSettings settings);
    Task<ValidationResult> UpdateSteeringSettingsAsync(SteeringSettings settings);
    Task<ValidationResult> UpdateToolSettingsAsync(ToolSettings settings);
    Task<ValidationResult> UpdateSectionControlSettingsAsync(SectionControlSettings settings);
    Task<ValidationResult> UpdateGpsSettingsAsync(GpsSettings settings);
    Task<ValidationResult> UpdateImuSettingsAsync(ImuSettings settings);
    Task<ValidationResult> UpdateGuidanceSettingsAsync(GuidanceSettings settings);
    Task<ValidationResult> UpdateWorkModeSettingsAsync(WorkModeSettings settings);
    Task<ValidationResult> UpdateCultureSettingsAsync(CultureSettings settings);
    Task<ValidationResult> UpdateSystemStateSettingsAsync(SystemStateSettings settings);
    Task<ValidationResult> UpdateDisplaySettingsAsync(DisplaySettings settings);

    // Get all settings as unified object
    ApplicationSettings GetAllSettings();

    // Reset to defaults
    Task ResetToDefaultsAsync();
}
```

**IValidationService Interface**:
```csharp
public interface IValidationService
{
    // Validate individual setting categories
    ValidationResult ValidateVehicleSettings(VehicleSettings settings);
    ValidationResult ValidateSteeringSettings(SteeringSettings settings);
    ValidationResult ValidateToolSettings(ToolSettings settings);
    ValidationResult ValidateSectionControlSettings(SectionControlSettings settings);
    ValidationResult ValidateGpsSettings(GpsSettings settings);
    ValidationResult ValidateImuSettings(ImuSettings settings);
    ValidationResult ValidateGuidanceSettings(GuidanceSettings settings);
    ValidationResult ValidateWorkModeSettings(WorkModeSettings settings);
    ValidationResult ValidateCultureSettings(CultureSettings settings);
    ValidationResult ValidateSystemStateSettings(SystemStateSettings settings);
    ValidationResult ValidateDisplaySettings(DisplaySettings settings);

    // Validate entire settings object including cross-setting dependencies
    ValidationResult ValidateAllSettings(ApplicationSettings settings);

    // Get validation constraints for a specific setting
    SettingConstraints GetConstraints(string settingPath);
}
```

**ISessionManagementService Interface**:
```csharp
public interface ISessionManagementService
{
    // Raised when session state changes
    event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

    // Start new session
    Task StartSessionAsync();

    // End current session and save state
    Task EndSessionAsync();

    // Save current session state (periodic snapshots for crash recovery)
    Task SaveSessionSnapshotAsync();

    // Restore session from crash recovery file
    Task<SessionRestoreResult> RestoreLastSessionAsync();

    // Get current session state
    SessionState GetCurrentSessionState();

    // Update session state components
    void UpdateCurrentField(string fieldName);
    void UpdateCurrentGuidanceLine(GuidanceLineType lineType, object lineData);
    void UpdateWorkProgress(WorkProgressData progressData);

    // Clear crash recovery file
    Task ClearCrashRecoveryAsync();
}
```

**IProfileManagementService Interface**:
```csharp
public interface IProfileManagementService
{
    // Raised when active profile changes
    event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    // Vehicle profile management
    Task<string[]> GetVehicleProfilesAsync();
    Task<VehicleProfile> GetVehicleProfileAsync(string vehicleName);
    Task<ProfileCreateResult> CreateVehicleProfileAsync(string vehicleName, VehicleSettings settings);
    Task<ProfileDeleteResult> DeleteVehicleProfileAsync(string vehicleName);
    Task<ProfileSwitchResult> SwitchVehicleProfileAsync(string vehicleName, bool carryOverSession);

    // User profile management
    Task<string[]> GetUserProfilesAsync();
    Task<UserProfile> GetUserProfileAsync(string userName);
    Task<ProfileCreateResult> CreateUserProfileAsync(string userName, UserPreferences preferences);
    Task<ProfileDeleteResult> DeleteUserProfileAsync(string userName);
    Task<ProfileSwitchResult> SwitchUserProfileAsync(string userName);

    // Get current profiles
    VehicleProfile GetCurrentVehicleProfile();
    UserProfile GetCurrentUserProfile();

    // Get default profile (first user added to system)
    string GetDefaultUserProfileName();
}
```

**IStateMediatorService Interface**:
```csharp
public interface IStateMediatorService
{
    // Coordinate settings changes across services
    Task NotifySettingsChangedAsync(SettingsCategory category, object newSettings);

    // Coordinate profile switches across services
    Task NotifyProfileSwitchAsync(ProfileType profileType, string profileName);

    // Coordinate session state changes across services
    Task NotifySessionStateChangedAsync(SessionStateChangeType changeType, object stateData);

    // Register service for state change notifications
    void RegisterServiceForNotifications(IStateAwareService service);

    // Unregister service
    void UnregisterService(IStateAwareService service);
}
```

**IUndoRedoService Interface**:
```csharp
public interface IUndoRedoService
{
    // Raised when undo/redo state changes
    event EventHandler<UndoRedoStateChangedEventArgs>? UndoRedoStateChanged;

    // Execute command (adds to undo stack)
    Task ExecuteAsync(IUndoableCommand command);

    // Undo last command
    Task<UndoResult> UndoAsync();

    // Redo last undone command
    Task<RedoResult> RedoAsync();

    // Check if undo/redo available
    bool CanUndo();
    bool CanRedo();

    // Get undo/redo stack descriptions
    string[] GetUndoStackDescriptions();
    string[] GetRedoStackDescriptions();

    // Clear stacks
    void ClearUndoStack();
    void ClearRedoStack();
    void ClearAllStacks();
}
```

**ISetupWizardService Interface**:
```csharp
public interface ISetupWizardService
{
    // Check if first-time setup needed
    bool IsFirstTimeSetup();

    // Get wizard steps
    SetupWizardStep[] GetWizardSteps();

    // Complete wizard step
    Task<StepCompletionResult> CompleteStepAsync(int stepIndex, object stepData);

    // Skip wizard and use defaults
    Task<SetupResult> SkipWizardAndUseDefaultsAsync();

    // Complete wizard
    Task<SetupResult> CompleteWizardAsync();
}
```

### Frontend
No UI implementation in this Wave - services are UI-agnostic. Future waves will consume these services for UI.

**Integration Points for Future UI**:
- Settings screens will call IConfigurationService methods
- Profile switcher will call IProfileManagementService methods
- Setup wizard will call ISetupWizardService methods
- Undo/Redo buttons will call IUndoRedoService methods

### Testing

**Unit Tests Required**:
- Configuration Service: Load/save JSON, load/save XML, round-trip conversion, settings retrieval/update
- Validation Service: All validation rules, cross-setting dependencies, constraint checking
- Session Management Service: Session start/end, snapshot creation, crash recovery restore
- Profile Management Service: Profile CRUD operations, profile switching with/without session carry-over
- State Mediator Service: Notification routing, service registration/unregistration
- Undo/Redo Service: Command execution, undo/redo operations, stack management
- Setup Wizard Service: First-time detection, step completion, default creation

**Performance Tests Required**:
- Settings load from JSON: <100ms
- Settings load from XML: <100ms
- Validation of all settings: <10ms
- Crash recovery snapshot: <500ms
- Profile switching: <200ms

**Integration Tests Required**:
- Dual-write atomicity (both JSON and XML succeed or both fail)
- Profile switching with session carry-over
- Crash recovery end-to-end (save snapshot, simulate crash, restore)
- Cross-service coordination via Mediator pattern

## Data Models

### Settings Models (Hierarchical JSON Structure)

**ApplicationSettings** (root container):
```csharp
public class ApplicationSettings
{
    public VehicleSettings Vehicle { get; set; }
    public SteeringSettings Steering { get; set; }
    public ToolSettings Tool { get; set; }
    public SectionControlSettings SectionControl { get; set; }
    public GpsSettings Gps { get; set; }
    public ImuSettings Imu { get; set; }
    public GuidanceSettings Guidance { get; set; }
    public WorkModeSettings WorkMode { get; set; }
    public CultureSettings Culture { get; set; }
    public SystemStateSettings SystemState { get; set; }
    public DisplaySettings Display { get; set; }
}
```

**VehicleSettings**:
```csharp
public class VehicleSettings
{
    // From All_Settings.png visual
    public double Wheelbase { get; set; } // cm, range: 50-500
    public double Track { get; set; } // cm, range: 10-400
    public double MaxSteerAngle { get; set; } // degrees, range: 10-90
    public double MaxAngularVelocity { get; set; } // degrees/sec, range: 10-200
    public double AntennaPivot { get; set; } // cm
    public double AntennaHeight { get; set; } // cm
    public double AntennaOffset { get; set; } // cm
    public double PivotBehindAnt { get; set; } // cm
    public double SteerAxleAhead { get; set; } // cm
    public int VehicleType { get; set; } // 0=Tractor, 1=Harvester, 2=4WD
    public double VehicleHitchLength { get; set; } // cm
    public double MinUturnRadius { get; set; } // meters, range: 1-20
}
```

**SteeringSettings**:
```csharp
public class SteeringSettings
{
    // From All_Settings.png visual
    public int CountsPerDegree { get; set; } // CPD, range: 1-1000
    public int Ackermann { get; set; } // percentage, range: 0-200
    public double WasOffset { get; set; } // range: -10.0 to 10.0
    public int HighPwm { get; set; } // range: 0-255
    public int LowPwm { get; set; } // range: 0-255
    public int MinPwm { get; set; } // range: 0-255
    public int MaxPwm { get; set; } // range: 0-255
    public int PanicStop { get; set; } // 0 or 1
}
```

**ToolSettings**:
```csharp
public class ToolSettings
{
    // From All_Settings.png visual
    public double ToolWidth { get; set; } // meters, range: 0.1-50.0
    public bool ToolFront { get; set; }
    public bool ToolRearFixed { get; set; }
    public bool ToolTBT { get; set; }
    public bool ToolTrailing { get; set; }
    public double ToolToPivotLength { get; set; } // meters
    public double ToolLookAheadOn { get; set; } // meters
    public double ToolLookAheadOff { get; set; } // meters
    public double ToolOffDelay { get; set; } // seconds
    public double ToolOffset { get; set; } // meters
    public double ToolOverlap { get; set; } // meters
    public double TrailingHitchLength { get; set; } // meters
    public double TankHitchLength { get; set; } // meters
    public double HydLiftLookAhead { get; set; } // meters
}
```

**SectionControlSettings**:
```csharp
public class SectionControlSettings
{
    // From All_Settings.png visual
    public int NumberSections { get; set; } // range: 1-32
    public bool HeadlandSecControl { get; set; }
    public bool FastSections { get; set; }
    public bool SectionOffOutBounds { get; set; }
    public bool SectionsNotZones { get; set; }
    public double LowSpeedCutoff { get; set; } // m/s, range: 0.1-5.0
    public double[] SectionPositions { get; set; } // Array of section positions across tool width
}
```

**GpsSettings**:
```csharp
public class GpsSettings
{
    // From All_Settings.png visual
    public double Hdop { get; set; } // Horizontal Dilution of Precision
    public double RawHz { get; set; } // Raw GPS frequency
    public double Hz { get; set; } // Filtered GPS frequency, range: 1-20
    public int GpsAgeAlarm { get; set; } // seconds, range: 1-60
    public string HeadingFrom { get; set; } // "GPS", "Dual", "IMU"
    public bool AutoStartAgIO { get; set; }
    public bool AutoOffAgIO { get; set; }
    public bool Rtk { get; set; }
    public bool RtkKillAutoSteer { get; set; }
}
```

**ImuSettings**:
```csharp
public class ImuSettings
{
    // From All_Settings.png visual
    public bool DualAsImu { get; set; }
    public int DualHeadingOffset { get; set; } // degrees, range: 0-359
    public double ImuFusionWeight { get; set; } // range: 0.0-1.0
    public double MinHeadingStep { get; set; } // degrees, range: 0.01-5.0
    public double MinStepLimit { get; set; } // range: 0.01-1.0
    public double RollZero { get; set; } // degrees
    public bool InvertRoll { get; set; }
    public double RollFilter { get; set; } // range: 0.0-1.0
}
```

**GuidanceSettings**:
```csharp
public class GuidanceSettings
{
    // From All_Settings.png visual
    public double AcquireFactor { get; set; } // range: 0.5-2.0
    public double LookAhead { get; set; } // meters, range: 1.0-10.0
    public double SpeedFactor { get; set; } // range: 0.5-2.0
    public double PurePursuitIntegral { get; set; } // range: 0.0-10.0
    public double SnapDistance { get; set; } // meters, range: 1-100
    public double RefSnapDistance { get; set; } // meters, range: 1-50
    public double SideHillComp { get; set; } // compensation factor
}
```

**WorkModeSettings**:
```csharp
public class WorkModeSettings
{
    // From All_Settings.png visual
    public bool RemoteWork { get; set; }
    public bool SteerWorkSwitch { get; set; }
    public bool SteerWorkManual { get; set; }
    public bool WorkActiveLow { get; set; }
    public bool WorkSwitch { get; set; }
    public bool WorkManualSection { get; set; }
}
```

**CultureSettings**:
```csharp
public class CultureSettings
{
    // From All_Settings.png visual
    public string CultureCode { get; set; } // e.g., "en", "de", "fr"
    public string LanguageName { get; set; }
}
```

**SystemStateSettings**:
```csharp
public class SystemStateSettings
{
    // From All_Settings.png visual - runtime state
    public bool StanleyUsed { get; set; }
    public bool SteerInReverse { get; set; }
    public bool ReverseOn { get; set; }
    public double Heading { get; set; } // degrees
    public double ImuHeading { get; set; } // degrees
    public int HeadingError { get; set; }
    public int DistanceError { get; set; }
    public int SteerIntegral { get; set; }
}
```

**DisplaySettings**:
```csharp
public class DisplaySettings
{
    // From All_Settings.png visual
    public int[] DeadHeadDelay { get; set; } // Array of 2 values
    public double North { get; set; }
    public double East { get; set; }
    public double Elevation { get; set; }
    public UnitSystem UnitSystem { get; set; } // Metric or Imperial
    public string SpeedSource { get; set; } // "GPS", "Wheel", "Simulated"
}
```

### Session Models

**SessionState**:
```csharp
public class SessionState
{
    public DateTime SessionStartTime { get; set; }
    public string CurrentFieldName { get; set; }
    public GuidanceLineType CurrentGuidanceLineType { get; set; }
    public object CurrentGuidanceLineData { get; set; } // ABLine, CurveLine, or Contour
    public WorkProgressData WorkProgress { get; set; }
    public string VehicleProfileName { get; set; }
    public string UserProfileName { get; set; }
    public DateTime LastSnapshotTime { get; set; }
}
```

**WorkProgressData**:
```csharp
public class WorkProgressData
{
    public double AreaCovered { get; set; } // square meters
    public double DistanceTravelled { get; set; } // meters
    public TimeSpan TimeWorked { get; set; }
    public List<Position> CoverageTrail { get; set; }
    public bool[] SectionStates { get; set; }
}
```

### Profile Models

**VehicleProfile**:
```csharp
public class VehicleProfile
{
    public string VehicleName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public ApplicationSettings Settings { get; set; }
}
```

**UserProfile**:
```csharp
public class UserProfile
{
    public string UserName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public UserPreferences Preferences { get; set; }
}
```

**UserPreferences**:
```csharp
public class UserPreferences
{
    public UnitSystem PreferredUnitSystem { get; set; }
    public string PreferredLanguage { get; set; }
    public DisplayPreferences DisplayPreferences { get; set; }
    public string LastUsedVehicleProfile { get; set; }
}
```

### Validation Models

**ValidationResult**:
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<ValidationWarning> Warnings { get; set; }
}
```

**ValidationError**:
```csharp
public class ValidationError
{
    public string SettingPath { get; set; } // e.g., "Vehicle.Wheelbase"
    public string Message { get; set; }
    public object InvalidValue { get; set; }
    public SettingConstraints Constraints { get; set; }
}
```

**ValidationWarning**:
```csharp
public class ValidationWarning
{
    public string SettingPath { get; set; }
    public string Message { get; set; }
    public object Value { get; set; }
}
```

**SettingConstraints**:
```csharp
public class SettingConstraints
{
    public object MinValue { get; set; }
    public object MaxValue { get; set; }
    public string[] AllowedValues { get; set; }
    public string DataType { get; set; }
    public string[] Dependencies { get; set; } // Other settings this depends on
    public string ValidationRule { get; set; } // Description of validation logic
}
```

### Undo/Redo Models

**IUndoableCommand**:
```csharp
public interface IUndoableCommand
{
    string Description { get; }
    Task ExecuteAsync();
    Task UndoAsync();
}
```

**Command Examples**:
- CreateBoundaryCommand
- ModifyBoundaryCommand
- DeleteBoundaryCommand
- CreateABLineCommand
- ModifyABLineCommand
- DeleteABLineCommand
- CreateCurveLineCommand
- ModifyCurveLineCommand
- DeleteCurveLineCommand

### Result Models

**SettingsLoadResult**:
```csharp
public class SettingsLoadResult
{
    public bool Success { get; set; }
    public ApplicationSettings Settings { get; set; }
    public string ErrorMessage { get; set; }
    public SettingsLoadSource Source { get; set; } // JSON, XML, or Defaults
}
```

**SettingsSaveResult**:
```csharp
public class SettingsSaveResult
{
    public bool Success { get; set; }
    public bool JsonSaved { get; set; }
    public bool XmlSaved { get; set; }
    public string ErrorMessage { get; set; }
}
```

**SessionRestoreResult**:
```csharp
public class SessionRestoreResult
{
    public bool Success { get; set; }
    public SessionState RestoredSession { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CrashTime { get; set; }
}
```

**ProfileCreateResult**, **ProfileDeleteResult**, **ProfileSwitchResult**:
```csharp
public class ProfileCreateResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class ProfileDeleteResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class ProfileSwitchResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public bool SessionCarriedOver { get; set; }
}
```

## File Formats

### JSON Structure (Primary Format)

**Example: Deere5055e.json**:
```json
{
  "Vehicle": {
    "Wheelbase": 180.0,
    "Track": 30.0,
    "MaxSteerAngle": 45.0,
    "MaxAngularVelocity": 100.0,
    "AntennaPivot": 25.0,
    "AntennaHeight": 50.0,
    "AntennaOffset": 0.0,
    "PivotBehindAnt": 30.0,
    "SteerAxleAhead": 110.0,
    "VehicleType": 0,
    "VehicleHitchLength": 0.0,
    "MinUturnRadius": 3.0
  },
  "Steering": {
    "CountsPerDegree": 100,
    "Ackermann": 100,
    "WasOffset": 0.04,
    "HighPwm": 235,
    "LowPwm": 78,
    "MinPwm": 5,
    "MaxPwm": 10,
    "PanicStop": 0
  },
  "Tool": {
    "ToolWidth": 1.828,
    "ToolFront": false,
    "ToolRearFixed": true,
    "ToolTBT": false,
    "ToolTrailing": false,
    "ToolToPivotLength": 0.0,
    "ToolLookAheadOn": 1.0,
    "ToolLookAheadOff": 0.5,
    "ToolOffDelay": 0.0,
    "ToolOffset": 0.0,
    "ToolOverlap": 0.0,
    "TrailingHitchLength": -2.5,
    "TankHitchLength": 3.0,
    "HydLiftLookAhead": 2.0
  },
  "SectionControl": {
    "NumberSections": 3,
    "HeadlandSecControl": false,
    "FastSections": true,
    "SectionOffOutBounds": true,
    "SectionsNotZones": true,
    "LowSpeedCutoff": 1.0,
    "SectionPositions": [-0.914, 0.0, 0.914]
  },
  "Gps": {
    "Hdop": 0.69,
    "RawHz": 9.890,
    "Hz": 10.0,
    "GpsAgeAlarm": 20,
    "HeadingFrom": "Dual",
    "AutoStartAgIO": true,
    "AutoOffAgIO": false,
    "Rtk": false,
    "RtkKillAutoSteer": false
  },
  "Imu": {
    "DualAsImu": false,
    "DualHeadingOffset": 90,
    "ImuFusionWeight": 0.06,
    "MinHeadingStep": 0.5,
    "MinStepLimit": 0.05,
    "RollZero": 0.0,
    "InvertRoll": false,
    "RollFilter": 0.15
  },
  "Guidance": {
    "AcquireFactor": 0.90,
    "LookAhead": 3.0,
    "SpeedFactor": 1.0,
    "PurePursuitIntegral": 0.0,
    "SnapDistance": 20.0,
    "RefSnapDistance": 5.0,
    "SideHillComp": 0.0
  },
  "WorkMode": {
    "RemoteWork": false,
    "SteerWorkSwitch": false,
    "SteerWorkManual": true,
    "WorkActiveLow": false,
    "WorkSwitch": false,
    "WorkManualSection": true
  },
  "Culture": {
    "CultureCode": "en",
    "LanguageName": "English"
  },
  "SystemState": {
    "StanleyUsed": false,
    "SteerInReverse": false,
    "ReverseOn": true,
    "Heading": 20.6,
    "ImuHeading": 0.0,
    "HeadingError": 1,
    "DistanceError": 1,
    "SteerIntegral": 0
  },
  "Display": {
    "DeadHeadDelay": [10, 10],
    "North": 1.15,
    "East": -1.76,
    "Elevation": 200.8,
    "UnitSystem": "Metric",
    "SpeedSource": "GPS"
  }
}
```

### XML Structure (Legacy Compatibility)

**Example: Deere5055e.xml (Flat Structure)**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<VehicleSettings>
  <Culture>en</Culture>
  <Wheelbase>180</Wheelbase>
  <Track>30</Track>
  <MaxSteerDeg>45</MaxSteerDeg>
  <MaxAngularVel>100</MaxAngularVel>
  <AntennaPivot>25</AntennaPivot>
  <AntennaHeight>50</AntennaHeight>
  <AntennaOffset>0</AntennaOffset>
  <PivotBehindAnt>30</PivotBehindAnt>
  <SteerAxleAhead>110</SteerAxleAhead>
  <VehicleType>0</VehicleType>
  <VehHitchLength>0</VehHitchLength>
  <MinUturnRadius>3</MinUturnRadius>
  <CPD>100</CPD>
  <Ackermann>100</Ackermann>
  <WASOffset>0.04</WASOffset>
  <HighPWM>235</HighPWM>
  <LowPWM>78</LowPWM>
  <MinPWM>5</MinPWM>
  <MaxPWM>10</MaxPWM>
  <PanicStop>0</PanicStop>
  <ToolWidth>4</ToolWidth>
  <ToolFront>False</ToolFront>
  <ToolRearFixed>True</ToolRearFixed>
  <ToolTBT>False</ToolTBT>
  <ToolTrailing>False</ToolTrailing>
  <ToolToPivotLen>0</ToolToPivotLen>
  <ToolLookAndOn>1</ToolLookAndOn>
  <ToolLookAndOff>0.5</ToolLookAndOff>
  <ToolOffDelay>0</ToolOffDelay>
  <ToolOffset>0</ToolOffset>
  <ToolOverlap>0</ToolOverlap>
  <TrailingHitchLen>-2.5</TrailingHitchLen>
  <TankHitchLength>3</TankHitchLength>
  <HydLiftLookAnd>2</HydLiftLookAnd>
  <NumberSections>3</NumberSections>
  <HeadlandSecControl>False</HeadlandSecControl>
  <FastSections>True</FastSections>
  <SectionOffOutBnds>True</SectionOffOutBnds>
  <SectionsNotZones>True</SectionsNotZones>
  <LowSpeedCutoff>0.5</LowSpeedCutoff>
  <HDOP>0.69</HDOP>
  <RawHz>9.890</RawHz>
  <Hz>10.0</Hz>
  <GPSAgeAlarm>20</GPSAgeAlarm>
  <HeadingFrom>Dual</HeadingFrom>
  <AutoStartAgIO>True</AutoStartAgIO>
  <AutoOffAgIO>False</AutoOffAgIO>
  <RTK>False</RTK>
  <RTKKillAutoSteer>False</RTKKillAutoSteer>
  <DualAsIMU>False</DualAsIMU>
  <DualHeadingOffset>90</DualHeadingOffset>
  <IMUFusionWeight>0.06</IMUFusionWeight>
  <MinHeadingStep>0.5</MinHeadingStep>
  <MinStepLimit>0.05</MinStepLimit>
  <RollZero>0</RollZero>
  <InvertRoll>False</InvertRoll>
  <RollFilter>0.15</RollFilter>
  <AcquireFactor>0.9</AcquireFactor>
  <LookAhead>3</LookAhead>
  <SpeedFactor>1</SpeedFactor>
  <PPIntegral>0</PPIntegral>
  <SnapDistance>20</SnapDistance>
  <RefSnapDistance>5</RefSnapDistance>
  <SideHillComp>0</SideHillComp>
  <RemoteWork>False</RemoteWork>
  <SteerWorkSw>False</SteerWorkSw>
  <SteerWorkManual>True</SteerWorkManual>
  <WorkActiveLow>False</WorkActiveLow>
  <WorkSwitch>False</WorkSwitch>
  <WorkManualSec>True</WorkManualSec>
  <StanleyUsed>False</StanleyUsed>
  <SteerInReverse>False</SteerInReverse>
  <ReverseOn>True</ReverseOn>
  <Heading>20.6</Heading>
  <IMU>0</IMU>
  <HeadingError>1</HeadingError>
  <DistanceError>1</DistanceError>
  <SteerIntegral>0</SteerIntegral>
  <DeadHeadDelay>10,10</DeadHeadDelay>
  <North>1.15</North>
  <East>-1.76</East>
  <Elev>200.8</Elev>
</VehicleSettings>
```

**XML Conversion Notes**:
- Flat structure with camelCase element names (legacy convention)
- Boolean values as "True"/"False" strings
- Arrays as comma-separated values (e.g., DeadHeadDelay)
- Some property names differ from JSON (e.g., "MaxSteerDeg" vs "MaxSteerAngle")
- Tool width stored as integer code (4) in XML, decimal meters (1.828) in JSON
- LowSpeedCutoff stored as 0.5 in XML, 1.0 actual value in JSON

### Session File Format

**LastSession.json**:
```json
{
  "SessionStartTime": "2025-10-20T14:30:00Z",
  "CurrentFieldName": "North40",
  "CurrentGuidanceLineType": "ABLine",
  "CurrentGuidanceLineData": {
    "PointA": { "Easting": 500000.0, "Northing": 4500000.0 },
    "PointB": { "Easting": 500100.0, "Northing": 4500100.0 },
    "Heading": 45.0
  },
  "WorkProgress": {
    "AreaCovered": 12500.0,
    "DistanceTravelled": 2500.0,
    "TimeWorked": "02:30:00",
    "CoverageTrailCount": 1500,
    "SectionStates": [true, true, false]
  },
  "VehicleProfileName": "Deere 5055e",
  "UserProfileName": "John",
  "LastSnapshotTime": "2025-10-20T16:59:30Z"
}
```

### User Profile File Format

**John.json**:
```json
{
  "UserName": "John",
  "CreatedDate": "2025-10-15T10:00:00Z",
  "LastModifiedDate": "2025-10-20T14:30:00Z",
  "Preferences": {
    "PreferredUnitSystem": "Imperial",
    "PreferredLanguage": "en",
    "DisplayPreferences": {
      "ShowSatelliteCount": true,
      "ShowSpeedGauge": true,
      "RotatingDisplayEnabled": true,
      "RotatingDisplayInterval": 5
    },
    "LastUsedVehicleProfile": "Deere 5055e"
  }
}
```

## Validation Rules

### Range Validations

**Vehicle Settings**:
- Wheelbase: 50-500 cm (positive, realistic for agricultural vehicles)
- Track: 10-400 cm (positive, realistic width)
- MaxSteerAngle: 10-90 degrees (physically achievable steering limits)
- MaxAngularVelocity: 10-200 degrees/sec (realistic steering speed)
- MinUturnRadius: 1-20 meters (realistic turning radius)

**Steering Settings**:
- CountsPerDegree: 1-1000 (positive, hardware-dependent)
- Ackermann: 0-200 percentage (steering geometry compensation)
- WasOffset: -10.0 to 10.0 (wheel angle sensor calibration)
- PWM values: 0-255 (standard PWM range)

**Section Control Settings**:
- NumberSections: 1-32 (must match physical hardware)
- LowSpeedCutoff: 0.1-5.0 m/s (realistic minimum working speed)
- SectionPositions array length must equal NumberSections

**GPS Settings**:
- Hz: 1-20 (realistic GPS update rates)
- GpsAgeAlarm: 1-60 seconds (data freshness threshold)

**IMU Settings**:
- ImuFusionWeight: 0.0-1.0 (fusion weight factor)
- MinHeadingStep: 0.01-5.0 degrees (heading change threshold)
- RollFilter: 0.0-1.0 (filter coefficient)
- DualHeadingOffset: 0-359 degrees (antenna orientation)

**Guidance Settings**:
- AcquireFactor: 0.5-2.0 (line acquisition multiplier)
- LookAhead: 1.0-10.0 meters (look-ahead distance)
- SpeedFactor: 0.5-2.0 (speed-based adjustment)
- SnapDistance: 1-100 meters (maximum distance to snap to line)
- RefSnapDistance: 1-50 meters (reference line snap distance)

**Tool Settings**:
- ToolWidth: 0.1-50.0 meters (realistic implement widths)

### Cross-Setting Dependencies

**Tool Width affects Guidance**:
- When ToolWidth changes, validate that LookAhead is reasonable relative to tool size
- Warning if LookAhead < ToolWidth * 0.5 (too short for effective guidance)

**Section Count affects Section Positions**:
- SectionPositions array length must equal NumberSections
- SectionPositions must span ToolWidth (first position ≈ -ToolWidth/2, last ≈ +ToolWidth/2)

**Steering Limits affect Control**:
- MaxSteerAngle must be achievable by hardware (validated against CPD and PWM range)
- MaxAngularVelocity must be realistic given MaxSteerAngle

**GPS Heading Source affects IMU Settings**:
- If HeadingFrom = "Dual", DualAsImu and DualHeadingOffset are relevant
- If HeadingFrom = "IMU", ImuFusionWeight is relevant
- Warn if HeadingFrom = "GPS" but single antenna (insufficient for heading)

**Vehicle Type affects Configuration**:
- VehicleType = Tractor: typical wheelbase, track, steering limits
- VehicleType = Harvester: different physical constraints
- VehicleType = 4WD: typically larger dimensions, tighter turning

### Type Validations

**Boolean Fields**: Must be true/false (strict type checking)
**Integer Fields**: Must be whole numbers within range
**Double Fields**: Must be valid decimal numbers, not NaN or Infinity
**String Fields**: CultureCode must match valid locale codes, HeadingFrom must be one of ["GPS", "Dual", "IMU"]
**Array Fields**: SectionPositions must contain valid numeric values

## Integration with Existing Waves

### Wave 1 (Position & Kinematics) Integration
- **IPositionUpdateService** will receive GPS settings (Hz, HDOP, age alarm) from IConfigurationService on startup
- **IVehicleKinematicsService** will receive vehicle physical dimensions (wheelbase, track, antenna position) from IConfigurationService
- Settings changes will trigger re-initialization of kinematics calculations via IStateMediatorService

### Wave 2 (Guidance Line Core) Integration
- **IABLineService**, **ICurveLineService**, **IContourService** will receive guidance settings (look-ahead, snap distances) from IConfigurationService
- Guidance line creation/modification will use IUndoRedoService for undo/redo capability
- Settings changes will update guidance parameters in real-time

### Wave 3 (Steering Algorithms) Integration
- **IStanleySteeringService** will receive Stanley algorithm parameters (distance error gain, heading error gain, integral gain) from IConfigurationService
- **IPurePursuitService** will receive Pure Pursuit parameters (integral gain) from IConfigurationService
- **ILookAheadDistanceService** will receive look-ahead settings (hold distance, multiplier, acquire factor) from IConfigurationService
- Settings changes will update algorithm parameters without restart

### Wave 4 (Section Control) Integration
- **ISectionConfigurationService** will receive section control settings (number sections, section positions) from IConfigurationService on startup
- **ISectionControlService** will receive work mode settings (switches, manual control) from IConfigurationService
- **ICoverageMapService** will receive section configuration for coverage tracking
- Settings changes will trigger section reconfiguration via IStateMediatorService

### Wave 5 (Field Operations) Integration
- **IBoundaryManagementService** will receive boundary settings from IConfigurationService
- **IHeadlandService** will receive headland control settings (headland section control) from IConfigurationService
- **IUTurnService** will receive U-turn settings (min radius, compensation) from IConfigurationService
- Boundary and headland creation/modification will use IUndoRedoService
- Current field name will be tracked in ISessionManagementService

### Wave 6 (Hardware I/O Communication) Integration
- **IAutoSteerCommunicationService** will receive steering hardware settings (CPD, Ackermann, PWM ranges) from IConfigurationService
- **IMachineCommunicationService** will receive section control settings (number sections) from IConfigurationService
- **IImuCommunicationService** will receive IMU settings (fusion weight, roll filter, invert roll) from IConfigurationService
- **IModuleCoordinatorService** will receive communication settings (auto-start AgIO) from IConfigurationService
- Settings changes will trigger hardware reconfiguration messages

### Wave 7 (Display & Visualization) Integration
- **IDisplayFormatterService** will receive display settings (unit system, speed source) from IConfigurationService
- **IFieldStatisticsService** will receive unit system and display preferences from IConfigurationService
- Settings changes will update display formatting in real-time

## Service Organization

### Directory Structure

Following NAMING_CONVENTIONS.md to avoid namespace collisions:

```
AgValoniaGPS.Services/
├── Configuration/
│   ├── ConfigurationService.cs
│   ├── IConfigurationService.cs
│   ├── JsonConfigurationProvider.cs
│   ├── XmlConfigurationProvider.cs
│   ├── IConfigurationProvider.cs
│   └── ConfigurationConverter.cs (JSON ↔ XML conversion)
├── Session/
│   ├── SessionManagementService.cs
│   ├── ISessionManagementService.cs
│   ├── CrashRecoveryService.cs
│   └── ICrashRecoveryService.cs
├── StateManagement/
│   ├── StateMediatorService.cs
│   ├── IStateMediatorService.cs
│   ├── IStateAwareService.cs (interface for services that need state notifications)
│   └── StateChangeNotification.cs
├── Validation/
│   ├── ValidationService.cs
│   ├── IValidationService.cs
│   ├── VehicleSettingsValidator.cs
│   ├── SteeringSettingsValidator.cs
│   ├── ToolSettingsValidator.cs
│   ├── SectionControlSettingsValidator.cs
│   ├── GpsSettingsValidator.cs
│   ├── ImuSettingsValidator.cs
│   ├── GuidanceSettingsValidator.cs
│   ├── WorkModeSettingsValidator.cs
│   ├── CultureSettingsValidator.cs
│   ├── SystemStateSettingsValidator.cs
│   ├── DisplaySettingsValidator.cs
│   └── CrossSettingValidator.cs
├── Profile/
│   ├── ProfileManagementService.cs
│   ├── IProfileManagementService.cs
│   ├── VehicleProfileProvider.cs
│   ├── UserProfileProvider.cs
│   └── IProfileProvider.cs
├── UndoRedo/
│   ├── UndoRedoService.cs
│   ├── IUndoRedoService.cs
│   ├── IUndoableCommand.cs
│   └── Commands/
│       ├── CreateBoundaryCommand.cs
│       ├── ModifyBoundaryCommand.cs
│       ├── DeleteBoundaryCommand.cs
│       ├── CreateABLineCommand.cs
│       ├── ModifyABLineCommand.cs
│       ├── DeleteABLineCommand.cs
│       ├── CreateCurveLineCommand.cs
│       ├── ModifyCurveLineCommand.cs
│       └── DeleteCurveLineCommand.cs
└── Setup/
    ├── SetupWizardService.cs
    ├── ISetupWizardService.cs
    └── DefaultSettingsProvider.cs
```

### Models Organization

```
AgValoniaGPS.Models/
├── Configuration/
│   ├── ApplicationSettings.cs (root container)
│   ├── VehicleSettings.cs
│   ├── SteeringSettings.cs
│   ├── ToolSettings.cs
│   ├── SectionControlSettings.cs
│   ├── GpsSettings.cs
│   ├── ImuSettings.cs
│   ├── GuidanceSettings.cs
│   ├── WorkModeSettings.cs
│   ├── CultureSettings.cs
│   ├── SystemStateSettings.cs
│   └── DisplaySettings.cs
├── Session/
│   ├── SessionState.cs
│   ├── WorkProgressData.cs
│   └── SessionRestoreResult.cs
├── Profile/
│   ├── VehicleProfile.cs
│   ├── UserProfile.cs
│   └── UserPreferences.cs
├── Validation/
│   ├── ValidationResult.cs
│   ├── ValidationError.cs
│   ├── ValidationWarning.cs
│   └── SettingConstraints.cs
└── StateManagement/
    ├── SettingsChangedEventArgs.cs
    ├── SessionStateChangedEventArgs.cs
    ├── ProfileChangedEventArgs.cs
    └── UndoRedoStateChangedEventArgs.cs
```

## Application Lifecycle

### Startup Sequence

1. **Check First-Time Setup**: ISetupWizardService.IsFirstTimeSetup()
   - If true: Show setup wizard
   - If false: Continue to step 2

2. **Load User Profiles**: IProfileManagementService.GetUserProfilesAsync()
   - If no profiles exist: Force setup wizard
   - If profiles exist: Load default user profile (first user added to system)

3. **Load Vehicle Profile**: IProfileManagementService.GetVehicleProfileAsync(lastUsedVehicle)
   - Load vehicle settings from JSON (preferred) or XML (fallback)
   - If load fails: Use defaults and log error

4. **Validate Settings**: IValidationService.ValidateAllSettings(loadedSettings)
   - If validation fails: Log errors, use safe fallback values
   - If validation passes: Continue

5. **Initialize Configuration Service**: IConfigurationService.LoadSettingsAsync(vehicleName)
   - Load settings into in-memory cache
   - Raise SettingsChanged event for all categories

6. **Initialize Services via Mediator**: IStateMediatorService.NotifySettingsChangedAsync()
   - Notify all Wave 1-7 services of settings
   - Services initialize with configuration values

7. **Check Session Recovery**: ISessionManagementService.RestoreLastSessionAsync()
   - Check for crash recovery file
   - If exists: Prompt user to restore session
   - If user accepts: Restore field, guidance line, work progress
   - If user declines or no crash recovery: Start new session

8. **Start Session**: ISessionManagementService.StartSessionAsync()
   - Create new session state
   - Start periodic crash recovery snapshots (every 30 seconds)

9. **Launch UI**: Show main application window

### Shutdown Sequence

1. **Save Session State**: ISessionManagementService.SaveSessionSnapshotAsync()
   - Save current field, guidance line, work progress

2. **End Session**: ISessionManagementService.EndSessionAsync()
   - Stop periodic snapshots
   - Save final session state to LastSession.json

3. **Save Modified Settings**: IConfigurationService.SaveSettingsAsync()
   - Dual-write to both JSON and XML
   - Only if settings were modified during session

4. **Stop Hardware Communication**: Notify Wave 6 services via IStateMediatorService
   - Gracefully close UDP/Bluetooth/CAN connections
   - Send shutdown messages to modules

5. **Clear Crash Recovery**: ISessionManagementService.ClearCrashRecoveryAsync()
   - Delete crash recovery file (clean shutdown)

6. **Exit Application**

### Runtime Profile Switching

**Vehicle Profile Switch**:
1. User selects new vehicle profile from menu
2. Prompt: "Carry over current session data?" (Yes/No)
3. If Yes: Call IProfileManagementService.SwitchVehicleProfileAsync(newVehicle, carryOverSession: true)
4. If No: Call IProfileManagementService.SwitchVehicleProfileAsync(newVehicle, carryOverSession: false)
5. IStateMediatorService notifies all services of profile switch
6. IConfigurationService loads new vehicle settings
7. Services re-initialize with new settings (without restart)
8. If carryOverSession=true: Preserve current field and work progress
9. If carryOverSession=false: Clear field and work progress

**User Profile Switch**:
1. User selects new user profile from menu
2. IProfileManagementService.SwitchUserProfileAsync(newUser)
3. Load user preferences (unit system, language, display settings)
4. Update IConfigurationService with user preferences
5. IStateMediatorService notifies display services of preference changes
6. UI updates to reflect new preferences (without restart)

## Performance Requirements

### Settings Load Performance
- **JSON Load**: <100ms for typical settings file (~50KB)
- **XML Load**: <100ms for legacy settings file (~30KB)
- **In-Memory Access**: <1ms for any GetSettings() call (cached)

### Validation Performance
- **Single Category Validation**: <5ms (e.g., ValidateVehicleSettings)
- **Full Settings Validation**: <10ms (all categories + cross-setting dependencies)
- **Constraint Lookup**: <1ms (GetConstraints call)

### Session Management Performance
- **Crash Recovery Snapshot**: <500ms (serialize and write session state)
- **Session Restore**: <200ms (read and deserialize session state)
- **Periodic Snapshot Overhead**: <50ms every 30 seconds (non-blocking)

### Profile Management Performance
- **Profile List**: <50ms (enumerate available profiles)
- **Profile Load**: <100ms (read and parse profile file)
- **Profile Switch**: <200ms (load new profile and notify services)

### State Coordination Performance
- **Mediator Notification**: <10ms (notify all registered services)
- **Service Registration**: <1ms (add service to notification list)

### Undo/Redo Performance
- **Command Execution**: <50ms (execute and add to undo stack)
- **Undo Operation**: <50ms (revert to previous state)
- **Redo Operation**: <50ms (re-apply undone command)

## Out of Scope

- Deprecating XML reading entirely (planned for months/years in the future)
- Automatic XML-to-JSON migration on first launch (user must explicitly migrate)
- Cloud sync of settings across devices
- Settings version control/history beyond undo/redo (no git-like commits)
- Real-time collaborative editing of settings (multi-user simultaneous editing)
- Settings export/import for sharing between users
- Advanced analytics on settings usage patterns
- Settings comparison tool (diff between profiles)
- Settings templates or presets for common vehicle types
- Encrypted settings files (security enhancement)
- Settings audit log (change tracking)
- Rollback to previous settings versions (beyond undo/redo)

## Success Criteria

- All settings categories load from JSON and XML successfully with 100% test coverage
- Dual-write ensures JSON and XML files are always in sync (atomic writes)
- Profile switching works at runtime without application restart
- Crash recovery successfully restores session after simulated crash (100% success rate in tests)
- Validation catches all invalid settings before persistence (zero invalid configurations saved)
- Undo/redo works for all user-entered data (boundaries, guidance lines) with no data loss
- Setup wizard creates valid default configuration for first-time users
- All performance requirements met under test (settings load <100ms, validation <10ms, snapshot <500ms)
- Cross-setting dependency validation prevents inconsistent configurations (e.g., section count mismatch)
- Wave 1-7 services successfully receive and apply settings from Configuration Service
- Mediator pattern prevents tight coupling (services don't directly depend on Configuration Service)
- XML-to-JSON conversion maintains exact fidelity (round-trip conversion produces identical settings)
- Multi-vehicle support allows farmers to switch tractors seamlessly
- Multi-user support allows multiple operators to maintain separate preferences
- No crashes or data loss during normal operation, settings changes, or profile switches
