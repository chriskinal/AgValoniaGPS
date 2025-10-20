# Task Breakdown: Wave 8 - State Management

## Overview
Total Task Groups: 9
Estimated Total LOC: 3,500-4,500
Estimated Completion Time: 18-24 hours (experienced developer)
Complexity Level: VERY HIGH

**Assigned Implementers:** api-engineer, testing-engineer

**Core Deliverables:**
- 6 major service interfaces with implementations
- 11 settings model classes (50+ individual settings)
- Dual-format persistence (JSON + XML)
- Multi-profile support (vehicles and users)
- Session management with crash recovery
- Validation engine with 100+ rules
- Undo/redo command infrastructure
- Complete service integration with Waves 1-7

## Task List

### Foundation: Settings Data Models

#### Task Group 1: Settings Models and Event Args
**Assigned Implementer:** api-engineer
**Dependencies:** None
**Estimated Time:** 3-4 hours
**Estimated LOC:** 600-800
**Parallelizable:** Yes (can run in parallel with other task groups after completion)

- [ ] 1.0 Complete settings data models
  - [ ] 1.1 Write 2-8 focused tests for settings models
    - Limit to 2-8 highly focused tests maximum
    - Test only critical model behaviors (property assignments, default values, serialization)
    - Skip exhaustive testing of all properties
  - [ ] 1.2 Create ApplicationSettings root container model
    - Path: `AgValoniaGPS.Models/Configuration/ApplicationSettings.cs`
    - Properties: Vehicle, Steering, Tool, SectionControl, Gps, Imu, Guidance, WorkMode, Culture, SystemState, Display
    - Namespace: `AgValoniaGPS.Models.Configuration`
  - [ ] 1.3 Create VehicleSettings model
    - Path: `AgValoniaGPS.Models/Configuration/VehicleSettings.cs`
    - Properties: Wheelbase, Track, MaxSteerAngle, MaxAngularVelocity, AntennaPivot, AntennaHeight, AntennaOffset, PivotBehindAnt, SteerAxleAhead, VehicleType, VehicleHitchLength, MinUturnRadius
    - See spec.md lines 406-422 for full details
  - [ ] 1.4 Create SteeringSettings model
    - Path: `AgValoniaGPS.Models/Configuration/SteeringSettings.cs`
    - Properties: CountsPerDegree, Ackermann, WasOffset, HighPwm, LowPwm, MinPwm, MaxPwm, PanicStop
    - See spec.md lines 424-438 for full details
  - [ ] 1.5 Create ToolSettings model
    - Path: `AgValoniaGPS.Models/Configuration/ToolSettings.cs`
    - Properties: ToolWidth, ToolFront, ToolRearFixed, ToolTBT, ToolTrailing, ToolToPivotLength, ToolLookAheadOn, ToolLookAheadOff, ToolOffDelay, ToolOffset, ToolOverlap, TrailingHitchLength, TankHitchLength, HydLiftLookAhead
    - See spec.md lines 440-461 for full details
  - [ ] 1.6 Create SectionControlSettings model
    - Path: `AgValoniaGPS.Models/Configuration/SectionControlSettings.cs`
    - Properties: NumberSections, HeadlandSecControl, FastSections, SectionOffOutBounds, SectionsNotZones, LowSpeedCutoff, SectionPositions
    - See spec.md lines 463-476 for full details
  - [ ] 1.7 Create GpsSettings model
    - Path: `AgValoniaGPS.Models/Configuration/GpsSettings.cs`
    - Properties: Hdop, RawHz, Hz, GpsAgeAlarm, HeadingFrom, AutoStartAgIO, AutoOffAgIO, Rtk, RtkKillAutoSteer
    - See spec.md lines 478-493 for full details
  - [ ] 1.8 Create ImuSettings model
    - Path: `AgValoniaGPS.Models/Configuration/ImuSettings.cs`
    - Properties: DualAsImu, DualHeadingOffset, ImuFusionWeight, MinHeadingStep, MinStepLimit, RollZero, InvertRoll, RollFilter
    - See spec.md lines 495-509 for full details
  - [ ] 1.9 Create GuidanceSettings model
    - Path: `AgValoniaGPS.Models/Configuration/GuidanceSettings.cs`
    - Properties: AcquireFactor, LookAhead, SpeedFactor, PurePursuitIntegral, SnapDistance, RefSnapDistance, SideHillComp
    - See spec.md lines 511-524 for full details
  - [ ] 1.10 Create WorkModeSettings model
    - Path: `AgValoniaGPS.Models/Configuration/WorkModeSettings.cs`
    - Properties: RemoteWork, SteerWorkSwitch, SteerWorkManual, WorkActiveLow, WorkSwitch, WorkManualSection
    - See spec.md lines 526-537 for full details
  - [ ] 1.11 Create CultureSettings model
    - Path: `AgValoniaGPS.Models/Configuration/CultureSettings.cs`
    - Properties: CultureCode, LanguageName
    - See spec.md lines 539-548 for full details
  - [ ] 1.12 Create SystemStateSettings model
    - Path: `AgValoniaGPS.Models/Configuration/SystemStateSettings.cs`
    - Properties: StanleyUsed, SteerInReverse, ReverseOn, Heading, ImuHeading, HeadingError, DistanceError, SteerIntegral
    - See spec.md lines 550-563 for full details
  - [ ] 1.13 Create DisplaySettings model
    - Path: `AgValoniaGPS.Models/Configuration/DisplaySettings.cs`
    - Properties: DeadHeadDelay, North, East, Elevation, UnitSystem, SpeedSource
    - See spec.md lines 565-578 for full details
  - [ ] 1.14 Create SettingsChangedEventArgs
    - Path: `AgValoniaGPS.Models/StateManagement/SettingsChangedEventArgs.cs`
    - Properties: Category (SettingsCategory enum), OldValue, NewValue, Timestamp
  - [ ] 1.15 Ensure settings model tests pass
    - Run ONLY the 2-8 tests written in 1.1
    - Verify all models serialize/deserialize correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 1.1 pass
- All 11 settings models created with proper properties and types
- Models follow namespace convention: AgValoniaGPS.Models.Configuration
- SettingsChangedEventArgs properly captures state changes
- Models support JSON serialization/deserialization

---

### Foundation: Session and Profile Models

#### Task Group 2: Session, Profile, and Validation Models
**Assigned Implementer:** api-engineer
**Dependencies:** Task Group 1
**Estimated Time:** 2-3 hours
**Estimated LOC:** 400-500
**Parallelizable:** Can run after Group 1 completes

- [ ] 2.0 Complete session, profile, and validation models
  - [ ] 2.1 Write 2-8 focused tests for session/profile models
    - Limit to 2-8 highly focused tests maximum
    - Test only critical model behaviors (session state capture, profile data integrity)
    - Skip exhaustive testing of all scenarios
  - [ ] 2.2 Create SessionState model
    - Path: `AgValoniaGPS.Models/Session/SessionState.cs`
    - Properties: SessionStartTime, CurrentFieldName, CurrentGuidanceLineType, CurrentGuidanceLineData, WorkProgress, VehicleProfileName, UserProfileName, LastSnapshotTime
    - See spec.md lines 581-595 for full details
  - [ ] 2.3 Create WorkProgressData model
    - Path: `AgValoniaGPS.Models/Session/WorkProgressData.cs`
    - Properties: AreaCovered, DistanceTravelled, TimeWorked, CoverageTrail, SectionStates
    - See spec.md lines 597-606 for full details
  - [ ] 2.4 Create VehicleProfile model
    - Path: `AgValoniaGPS.Models/Profile/VehicleProfile.cs`
    - Properties: VehicleName, CreatedDate, LastModifiedDate, Settings
    - See spec.md lines 612-619 for full details
  - [ ] 2.5 Create UserProfile model
    - Path: `AgValoniaGPS.Models/Profile/UserProfile.cs`
    - Properties: UserName, CreatedDate, LastModifiedDate, Preferences
    - See spec.md lines 621-631 for full details
  - [ ] 2.6 Create UserPreferences model
    - Path: `AgValoniaGPS.Models/Profile/UserPreferences.cs`
    - Properties: PreferredUnitSystem, PreferredLanguage, DisplayPreferences, LastUsedVehicleProfile
    - See spec.md lines 633-642 for full details
  - [ ] 2.7 Create ValidationResult model
    - Path: `AgValoniaGPS.Models/Validation/ValidationResult.cs`
    - Properties: IsValid, Errors, Warnings
    - See spec.md lines 646-653 for full details
  - [ ] 2.8 Create ValidationError model
    - Path: `AgValoniaGPS.Models/Validation/ValidationError.cs`
    - Properties: SettingPath, Message, InvalidValue, Constraints
    - See spec.md lines 655-665 for full details
  - [ ] 2.9 Create ValidationWarning model
    - Path: `AgValoniaGPS.Models/Validation/ValidationWarning.cs`
    - Properties: SettingPath, Message, Value
    - See spec.md lines 667-675 for full details
  - [ ] 2.10 Create SettingConstraints model
    - Path: `AgValoniaGPS.Models/Validation/SettingConstraints.cs`
    - Properties: MinValue, MaxValue, AllowedValues, DataType, Dependencies, ValidationRule
    - See spec.md lines 677-688 for full details
  - [ ] 2.11 Create result models
    - SettingsLoadResult: Success, Settings, ErrorMessage, Source
    - SettingsSaveResult: Success, JsonSaved, XmlSaved, ErrorMessage
    - SessionRestoreResult: Success, RestoredSession, ErrorMessage, CrashTime
    - ProfileCreateResult, ProfileDeleteResult, ProfileSwitchResult
    - See spec.md lines 714-768 for full details
  - [ ] 2.12 Ensure session/profile model tests pass
    - Run ONLY the 2-8 tests written in 2.1
    - Verify all models work correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 2.1 pass
- All session, profile, and validation models created
- Models follow proper namespace conventions
- Result models provide clear success/failure information

---

### Core Service: Configuration Service

#### Task Group 3: Configuration Service with Dual-Format I/O
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Time:** 4-5 hours
**Estimated LOC:** 800-1000
**Parallelizable:** No (depends on models from Groups 1-2)

- [ ] 3.0 Complete Configuration Service with dual-format file I/O
  - [ ] 3.1 Write 2-8 focused tests for Configuration Service
    - Limit to 2-8 highly focused tests maximum
    - Test only critical operations (load JSON, save JSON/XML, settings retrieval)
    - Skip exhaustive testing of all settings categories
  - [ ] 3.2 Create IConfigurationService interface
    - Path: `AgValoniaGPS.Services/Configuration/IConfigurationService.cs`
    - Methods: LoadSettingsAsync, SaveSettingsAsync, GetXXXSettings (11 getters), UpdateXXXSettingsAsync (11 updaters), GetAllSettings, ResetToDefaultsAsync
    - Event: SettingsChanged
    - See spec.md lines 143-187 for full details
  - [ ] 3.3 Create ConfigurationService implementation
    - Path: `AgValoniaGPS.Services/Configuration/ConfigurationService.cs`
    - In-memory cache for settings (ApplicationSettings object)
    - Thread-safe access using locks or concurrent collections
    - Raise SettingsChanged event on any setting modification
  - [ ] 3.4 Create IConfigurationProvider interface
    - Path: `AgValoniaGPS.Services/Configuration/IConfigurationProvider.cs`
    - Methods: LoadAsync(string filePath), SaveAsync(string filePath, ApplicationSettings settings)
    - Abstraction for file format providers
  - [ ] 3.5 Create JsonConfigurationProvider
    - Path: `AgValoniaGPS.Services/Configuration/JsonConfigurationProvider.cs`
    - Use System.Text.Json for serialization
    - Hierarchical structure (matches ApplicationSettings model)
    - See spec.md lines 770-887 for JSON format example
  - [ ] 3.6 Create XmlConfigurationProvider
    - Path: `AgValoniaGPS.Services/Configuration/XmlConfigurationProvider.cs`
    - Use System.Xml for serialization
    - Flat structure (legacy compatibility)
    - See spec.md lines 889-979 for XML format example
  - [ ] 3.7 Create ConfigurationConverter
    - Path: `AgValoniaGPS.Services/Configuration/ConfigurationConverter.cs`
    - Methods: JsonToXml(ApplicationSettings), XmlToJson(flatXmlData)
    - Handle property name mappings (e.g., "MaxSteerDeg" XML → "MaxSteerAngle" JSON)
    - Handle special conversions (e.g., tool width integer code → decimal meters)
    - See spec.md lines 981-988 for conversion notes
  - [ ] 3.8 Implement dual-write in ConfigurationService
    - SaveSettingsAsync must write both JSON and XML atomically
    - If JSON write fails, rollback XML write (or vice versa)
    - Return SettingsSaveResult with JsonSaved and XmlSaved flags
  - [ ] 3.9 Implement LoadSettingsAsync with fallback strategy
    - Try JSON first (preferred format)
    - If JSON missing/corrupt, fall back to XML
    - If both missing, return defaults
    - Set SettingsLoadResult.Source appropriately (JSON, XML, or Defaults)
  - [ ] 3.10 Implement cross-platform file path handling
    - Use Path.Combine for all file paths
    - Base directory: Documents/AgValoniaGPS/Vehicles/
    - Vehicle file pattern: [VehicleName].json and [VehicleName].xml
  - [ ] 3.11 Ensure Configuration Service tests pass
    - Run ONLY the 2-8 tests written in 3.1
    - Verify load/save operations work correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 3.1 pass
- ConfigurationService provides single source of truth for settings
- Dual-write successfully saves both JSON and XML
- LoadSettingsAsync tries JSON first, falls back to XML, then defaults
- All 11 settings categories accessible via Get/Update methods
- SettingsChanged event fires on any modification
- Thread-safe concurrent access

---

### Core Service: Validation Service

#### Task Group 4: Validation Service with Rules Engine
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Time:** 3-4 hours
**Estimated LOC:** 600-800
**Parallelizable:** Can run in parallel with Group 3 after Groups 1-2 complete

- [ ] 4.0 Complete Validation Service with comprehensive rules
  - [ ] 4.1 Write 2-8 focused tests for Validation Service
    - Limit to 2-8 highly focused tests maximum
    - Test only critical validation rules (range checks, cross-setting dependencies)
    - Skip exhaustive testing of all validation scenarios
  - [ ] 4.2 Create IValidationService interface
    - Path: `AgValoniaGPS.Services/Validation/IValidationService.cs`
    - Methods: ValidateXXXSettings (11 validators), ValidateAllSettings, GetConstraints
    - See spec.md lines 190-212 for full details
  - [ ] 4.3 Create ValidationService implementation
    - Path: `AgValoniaGPS.Services/Validation/ValidationService.cs`
    - Orchestrates all individual validators
    - ValidateAllSettings calls all 11 validators + CrossSettingValidator
  - [ ] 4.4 Create VehicleSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/VehicleSettingsValidator.cs`
    - Validate Wheelbase: 50-500 cm
    - Validate Track: 10-400 cm
    - Validate MaxSteerAngle: 10-90 degrees
    - Validate MaxAngularVelocity: 10-200 degrees/sec
    - Validate MinUturnRadius: 1-20 meters
    - See spec.md lines 1040-1047 for full constraints
  - [ ] 4.5 Create SteeringSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/SteeringSettingsValidator.cs`
    - Validate CountsPerDegree: 1-1000
    - Validate Ackermann: 0-200 percentage
    - Validate WasOffset: -10.0 to 10.0
    - Validate PWM values: 0-255
    - See spec.md lines 1048-1053 for full constraints
  - [ ] 4.6 Create ToolSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/ToolSettingsValidator.cs`
    - Validate ToolWidth: 0.1-50.0 meters
    - See spec.md lines 1076-1078 for full constraints
  - [ ] 4.7 Create SectionControlSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/SectionControlSettingsValidator.cs`
    - Validate NumberSections: 1-32
    - Validate LowSpeedCutoff: 0.1-5.0 m/s
    - Validate SectionPositions array length matches NumberSections
    - See spec.md lines 1054-1058 for full constraints
  - [ ] 4.8 Create GpsSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/GpsSettingsValidator.cs`
    - Validate Hz: 1-20
    - Validate GpsAgeAlarm: 1-60 seconds
    - See spec.md lines 1059-1062 for full constraints
  - [ ] 4.9 Create ImuSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/ImuSettingsValidator.cs`
    - Validate ImuFusionWeight: 0.0-1.0
    - Validate MinHeadingStep: 0.01-5.0 degrees
    - Validate RollFilter: 0.0-1.0
    - Validate DualHeadingOffset: 0-359 degrees
    - See spec.md lines 1063-1068 for full constraints
  - [ ] 4.10 Create GuidanceSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/GuidanceSettingsValidator.cs`
    - Validate AcquireFactor: 0.5-2.0
    - Validate LookAhead: 1.0-10.0 meters
    - Validate SpeedFactor: 0.5-2.0
    - Validate SnapDistance: 1-100 meters
    - Validate RefSnapDistance: 1-50 meters
    - See spec.md lines 1069-1075 for full constraints
  - [ ] 4.11 Create WorkModeSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/WorkModeSettingsValidator.cs`
    - Boolean validation only (no range checks needed)
  - [ ] 4.12 Create CultureSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/CultureSettingsValidator.cs`
    - Validate CultureCode matches valid locale codes
    - See spec.md lines 1107-1109 for type validations
  - [ ] 4.13 Create SystemStateSettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/SystemStateSettingsValidator.cs`
    - Boolean and numeric validation
  - [ ] 4.14 Create DisplaySettingsValidator
    - Path: `AgValoniaGPS.Services/Validation/DisplaySettingsValidator.cs`
    - Type validation for arrays and enums
  - [ ] 4.15 Create CrossSettingValidator
    - Path: `AgValoniaGPS.Services/Validation/CrossSettingValidator.cs`
    - Tool Width affects Guidance: Warn if LookAhead < ToolWidth * 0.5
    - Section Count affects Section Positions: Length must match, positions must span tool width
    - GPS Heading Source affects IMU Settings: Validate DualAsImu, ImuFusionWeight relevance
    - See spec.md lines 1079-1102 for cross-setting dependencies
  - [ ] 4.16 Ensure Validation Service tests pass
    - Run ONLY the 2-8 tests written in 4.1
    - Verify critical validation rules work
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 4.1 pass
- All 11 category validators implemented with proper range checks
- CrossSettingValidator catches interdependent setting issues
- ValidationResult provides clear error messages with setting paths
- GetConstraints method returns constraints for any setting path
- Validation completes in <10ms for full settings validation

---

### Core Service: Profile Management

#### Task Group 5: Profile Management Service
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2, 3
**Estimated Time:** 3-4 hours
**Estimated LOC:** 500-700
**Parallelizable:** No (depends on Configuration Service from Group 3)

- [ ] 5.0 Complete Profile Management Service
  - [ ] 5.1 Write 2-8 focused tests for Profile Management
    - Limit to 2-8 highly focused tests maximum
    - Test only critical operations (profile CRUD, profile switching)
    - Skip exhaustive testing of all scenarios
  - [ ] 5.2 Create IProfileManagementService interface
    - Path: `AgValoniaGPS.Services/Profile/IProfileManagementService.cs`
    - Methods: GetVehicleProfilesAsync, GetVehicleProfileAsync, CreateVehicleProfileAsync, DeleteVehicleProfileAsync, SwitchVehicleProfileAsync
    - Methods: GetUserProfilesAsync, GetUserProfileAsync, CreateUserProfileAsync, DeleteUserProfileAsync, SwitchUserProfileAsync
    - Methods: GetCurrentVehicleProfile, GetCurrentUserProfile, GetDefaultUserProfileName
    - Event: ProfileChanged
    - See spec.md lines 247-274 for full details
  - [ ] 5.3 Create ProfileManagementService implementation
    - Path: `AgValoniaGPS.Services/Profile/ProfileManagementService.cs`
    - Track current vehicle profile
    - Track current user profile
    - Coordinate with IConfigurationService for settings loading
  - [ ] 5.4 Create IProfileProvider interface
    - Path: `AgValoniaGPS.Services/Profile/IProfileProvider.cs`
    - Methods: GetAllAsync, GetAsync, CreateAsync, DeleteAsync
    - Abstraction for vehicle vs user profile providers
  - [ ] 5.5 Create VehicleProfileProvider
    - Path: `AgValoniaGPS.Services/Profile/VehicleProfileProvider.cs`
    - Enumerate vehicle profiles in Documents/AgValoniaGPS/Vehicles/
    - Load VehicleProfile from JSON file
    - Create new vehicle profile with default settings
    - Delete vehicle profile (remove JSON and XML files)
  - [ ] 5.6 Create UserProfileProvider
    - Path: `AgValoniaGPS.Services/Profile/UserProfileProvider.cs`
    - Enumerate user profiles in Documents/AgValoniaGPS/Users/
    - Load UserProfile from JSON file
    - Create new user profile with default preferences
    - Delete user profile
  - [ ] 5.7 Implement SwitchVehicleProfileAsync with session carry-over logic
    - Parameter: carryOverSession (bool)
    - If true: Preserve current field name, guidance line, work progress
    - If false: Clear session state
    - Load new vehicle settings via IConfigurationService
    - Raise ProfileChanged event
  - [ ] 5.8 Implement SwitchUserProfileAsync
    - Load user preferences
    - Update display settings (unit system, language)
    - Do not affect vehicle settings or session
    - Raise ProfileChanged event
  - [ ] 5.9 Implement GetDefaultUserProfileName
    - Return first user added to system
    - Used at application startup if no user preference saved
  - [ ] 5.10 Ensure Profile Management tests pass
    - Run ONLY the 2-8 tests written in 5.1
    - Verify profile operations work correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 5.1 pass
- Vehicle profile switching works with/without session carry-over
- User profile switching updates preferences without affecting vehicle
- GetDefaultUserProfileName returns first user for startup
- ProfileChanged event fires on profile switches
- Profile files stored in correct directories (Vehicles/, Users/)

---

### Core Service: Session Management

#### Task Group 6: Session Management with Crash Recovery
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Time:** 3-4 hours
**Estimated LOC:** 400-600
**Parallelizable:** Can run in parallel with Groups 3-5 after Groups 1-2 complete

- [ ] 6.0 Complete Session Management Service with crash recovery
  - [ ] 6.1 Write 2-8 focused tests for Session Management
    - Limit to 2-8 highly focused tests maximum
    - Test only critical operations (session start/end, crash recovery)
    - Skip exhaustive testing of all scenarios
  - [ ] 6.2 Create ISessionManagementService interface
    - Path: `AgValoniaGPS.Services/Session/ISessionManagementService.cs`
    - Methods: StartSessionAsync, EndSessionAsync, SaveSessionSnapshotAsync, RestoreLastSessionAsync, GetCurrentSessionState
    - Methods: UpdateCurrentField, UpdateCurrentGuidanceLine, UpdateWorkProgress, ClearCrashRecoveryAsync
    - Event: SessionStateChanged
    - See spec.md lines 214-244 for full details
  - [ ] 6.3 Create SessionManagementService implementation
    - Path: `AgValoniaGPS.Services/Session/SessionManagementService.cs`
    - Maintain current SessionState in memory
    - Periodic snapshot timer (every 30 seconds)
    - Save to Documents/AgValoniaGPS/Sessions/CrashRecovery.json
  - [ ] 6.4 Create ICrashRecoveryService interface
    - Path: `AgValoniaGPS.Services/Session/ICrashRecoveryService.cs`
    - Methods: SaveSnapshotAsync, RestoreSnapshotAsync, ClearSnapshotAsync
    - File I/O abstraction for crash recovery
  - [ ] 6.5 Create CrashRecoveryService implementation
    - Path: `AgValoniaGPS.Services/Session/CrashRecoveryService.cs`
    - Serialize SessionState to CrashRecovery.json
    - Atomic write (write to temp file, rename)
    - Deserialize SessionState from CrashRecovery.json
    - Check file age (if >24 hours old, consider stale)
  - [ ] 6.6 Implement StartSessionAsync
    - Create new SessionState with current timestamp
    - Set VehicleProfileName and UserProfileName from current profiles
    - Start periodic snapshot timer (30 second interval)
    - Raise SessionStateChanged event
  - [ ] 6.7 Implement EndSessionAsync
    - Stop periodic snapshot timer
    - Save final session state to LastSession.json
    - Keep CrashRecovery.json (will be cleared if clean shutdown)
    - Raise SessionStateChanged event
  - [ ] 6.8 Implement SaveSessionSnapshotAsync
    - Called by timer every 30 seconds
    - Serialize current SessionState to CrashRecovery.json
    - Must complete in <500ms (non-blocking)
  - [ ] 6.9 Implement RestoreLastSessionAsync
    - Check for CrashRecovery.json existence
    - If exists, deserialize SessionRestoreResult
    - Return success/failure with crash time
    - UI can then prompt user to restore or discard
  - [ ] 6.10 Implement ClearCrashRecoveryAsync
    - Delete CrashRecovery.json
    - Called after clean shutdown or user declines restore
  - [ ] 6.11 Ensure Session Management tests pass
    - Run ONLY the 2-8 tests written in 6.1
    - Verify session operations work correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 6.1 pass
- Session starts and ends correctly with timestamp tracking
- Periodic snapshots save every 30 seconds to CrashRecovery.json
- RestoreLastSessionAsync successfully loads crash recovery data
- ClearCrashRecoveryAsync removes recovery file after clean shutdown
- Snapshot save completes in <500ms (performance requirement)

---

### Core Service: State Mediator

#### Task Group 7: State Mediator Service
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Time:** 2-3 hours
**Estimated LOC:** 300-400
**Parallelizable:** Can run in parallel with Groups 3-6 after Groups 1-2 complete

- [ ] 7.0 Complete State Mediator Service for cross-service coordination
  - [ ] 7.1 Write 2-8 focused tests for State Mediator
    - Limit to 2-8 highly focused tests maximum
    - Test only critical operations (service registration, notification routing)
    - Skip exhaustive testing of all notification scenarios
  - [ ] 7.2 Create IStateAwareService interface
    - Path: `AgValoniaGPS.Services/StateManagement/IStateAwareService.cs`
    - Methods: OnSettingsChanged, OnProfileSwitched, OnSessionStateChanged
    - Services implement this to receive state notifications
  - [ ] 7.3 Create IStateMediatorService interface
    - Path: `AgValoniaGPS.Services/StateManagement/IStateMediatorService.cs`
    - Methods: NotifySettingsChangedAsync, NotifyProfileSwitchAsync, NotifySessionStateChangedAsync
    - Methods: RegisterServiceForNotifications, UnregisterService
    - See spec.md lines 276-295 for full details
  - [ ] 7.4 Create StateMediatorService implementation
    - Path: `AgValoniaGPS.Services/StateManagement/StateMediatorService.cs`
    - Maintain list of registered IStateAwareService instances
    - Thread-safe registration/unregistration
    - Broadcast notifications to all registered services
  - [ ] 7.5 Create StateChangeNotification model
    - Path: `AgValoniaGPS.Services/StateManagement/StateChangeNotification.cs`
    - Properties: ChangeType (enum), Category, Data, Timestamp
    - Common notification payload structure
  - [ ] 7.6 Implement NotifySettingsChangedAsync
    - Parameters: SettingsCategory category, object newSettings
    - Call OnSettingsChanged on all registered services
    - Handle exceptions from individual services (log but don't fail)
  - [ ] 7.7 Implement NotifyProfileSwitchAsync
    - Parameters: ProfileType profileType, string profileName
    - Call OnProfileSwitched on all registered services
    - Services can reload configuration based on new profile
  - [ ] 7.8 Implement NotifySessionStateChangedAsync
    - Parameters: SessionStateChangeType changeType, object stateData
    - Call OnSessionStateChanged on all registered services
    - Services can update UI or state based on session changes
  - [ ] 7.9 Implement RegisterServiceForNotifications
    - Add service to internal list
    - Weak reference to avoid memory leaks
    - Log registration for debugging
  - [ ] 7.10 Implement UnregisterService
    - Remove service from internal list
    - Log unregistration for debugging
  - [ ] 7.11 Ensure State Mediator tests pass
    - Run ONLY the 2-8 tests written in 7.1
    - Verify notification routing works correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 7.1 pass
- Services can register for state change notifications
- NotifyXXXAsync methods broadcast to all registered services
- Exception handling prevents one service failure from affecting others
- Mediator completes notification cycle in <10ms (performance requirement)

---

### Supporting Service: Undo/Redo System

#### Task Group 8: Undo/Redo Service with Command Pattern
**Assigned Implementer:** api-engineer
**Dependencies:** Task Groups 1, 2
**Estimated Time:** 2-3 hours
**Estimated LOC:** 400-500
**Parallelizable:** Can run in parallel with Groups 3-7 after Groups 1-2 complete

- [ ] 8.0 Complete Undo/Redo Service with command infrastructure
  - [ ] 8.1 Write 2-8 focused tests for Undo/Redo Service
    - Limit to 2-8 highly focused tests maximum
    - Test only critical operations (command execution, undo, redo)
    - Skip exhaustive testing of all command types
  - [ ] 8.2 Create IUndoableCommand interface
    - Path: `AgValoniaGPS.Services/UndoRedo/IUndoableCommand.cs`
    - Properties: Description (string)
    - Methods: ExecuteAsync, UndoAsync
    - See spec.md lines 691-700 for full details
  - [ ] 8.3 Create IUndoRedoService interface
    - Path: `AgValoniaGPS.Services/UndoRedo/IUndoRedoService.cs`
    - Methods: ExecuteAsync, UndoAsync, RedoAsync, CanUndo, CanRedo, GetUndoStackDescriptions, GetRedoStackDescriptions, ClearUndoStack, ClearRedoStack, ClearAllStacks
    - Event: UndoRedoStateChanged
    - See spec.md lines 297-327 for full details
  - [ ] 8.4 Create UndoRedoService implementation
    - Path: `AgValoniaGPS.Services/UndoRedo/UndoRedoService.cs`
    - Maintain undo stack (List<IUndoableCommand>)
    - Maintain redo stack (List<IUndoableCommand>)
    - Thread-safe stack operations
  - [ ] 8.5 Implement ExecuteAsync
    - Call command.ExecuteAsync()
    - Push command onto undo stack
    - Clear redo stack (new action invalidates redo history)
    - Raise UndoRedoStateChanged event
    - Must complete in <50ms (performance requirement)
  - [ ] 8.6 Implement UndoAsync
    - Pop command from undo stack
    - Call command.UndoAsync()
    - Push command onto redo stack
    - Raise UndoRedoStateChanged event
    - Return UndoResult with success/failure
    - Must complete in <50ms (performance requirement)
  - [ ] 8.7 Implement RedoAsync
    - Pop command from redo stack
    - Call command.ExecuteAsync()
    - Push command onto undo stack
    - Raise UndoRedoStateChanged event
    - Return RedoResult with success/failure
    - Must complete in <50ms (performance requirement)
  - [ ] 8.8 Implement CanUndo and CanRedo
    - Check if respective stacks are non-empty
    - Used to enable/disable UI buttons
  - [ ] 8.9 Implement GetUndoStackDescriptions and GetRedoStackDescriptions
    - Return array of command descriptions
    - Used to display undo/redo history in UI
  - [ ] 8.10 Create example command implementations
    - CreateBoundaryCommand (Path: `AgValoniaGPS.Services/UndoRedo/Commands/CreateBoundaryCommand.cs`)
    - ModifyBoundaryCommand (Path: `AgValoniaGPS.Services/UndoRedo/Commands/ModifyBoundaryCommand.cs`)
    - DeleteBoundaryCommand (Path: `AgValoniaGPS.Services/UndoRedo/Commands/DeleteBoundaryCommand.cs`)
    - CreateABLineCommand (Path: `AgValoniaGPS.Services/UndoRedo/Commands/CreateABLineCommand.cs`)
    - Note: Full command implementations will be completed in field/guidance feature waves
  - [ ] 8.11 Ensure Undo/Redo Service tests pass
    - Run ONLY the 2-8 tests written in 8.1
    - Verify undo/redo operations work correctly
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 8.1 pass
- UndoRedoService maintains separate undo and redo stacks
- ExecuteAsync adds commands to undo stack and clears redo stack
- UndoAsync and RedoAsync work correctly with stack management
- CanUndo and CanRedo accurately reflect stack state
- Command execution/undo/redo completes in <50ms (performance requirement)

---

### Integration: Service Registration

#### Task Group 9: Service Registration and Integration
**Assigned Implementer:** api-engineer
**Dependencies:** All previous task groups (1-8)
**Estimated Time:** 2-3 hours
**Estimated LOC:** 200-300
**Parallelizable:** No (must wait for all services to be implemented)

- [ ] 9.0 Complete service registration and integration with Wave 1-7
  - [ ] 9.1 Write 2-8 focused integration tests
    - Limit to 2-8 highly focused tests maximum
    - Test only critical integration points (service initialization, cross-service communication)
    - Skip exhaustive testing of all integration scenarios
  - [ ] 9.2 Register services in ServiceCollectionExtensions
    - Path: `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`
    - Add IConfigurationService as Singleton
    - Add IValidationService as Singleton
    - Add ISessionManagementService as Singleton
    - Add IProfileManagementService as Singleton
    - Add IStateMediatorService as Singleton
    - Add IUndoRedoService as Singleton
    - Follow existing pattern from Wave 1-7 services
  - [ ] 9.3 Create ISetupWizardService interface
    - Path: `AgValoniaGPS.Services/Setup/ISetupWizardService.cs`
    - Methods: IsFirstTimeSetup, GetWizardSteps, CompleteStepAsync, SkipWizardAndUseDefaultsAsync, CompleteWizardAsync
    - See spec.md lines 329-348 for full details
  - [ ] 9.4 Create SetupWizardService implementation
    - Path: `AgValoniaGPS.Services/Setup/SetupWizardService.cs`
    - Check for existing profiles to determine first-time setup
    - Wizard steps: Vehicle selection, physical configuration, GPS setup, section control
  - [ ] 9.5 Create DefaultSettingsProvider
    - Path: `AgValoniaGPS.Services/Setup/DefaultSettingsProvider.cs`
    - Provide sensible defaults for all 11 settings categories
    - Use values from spec.md JSON example (lines 775-886)
  - [ ] 9.6 Integrate Configuration Service with Wave 1 (Position & Kinematics)
    - IPositionUpdateService receives GPS settings (Hz, HDOP, age alarm)
    - IVehicleKinematicsService receives vehicle dimensions (wheelbase, track, antenna)
    - Use IStateMediatorService for coordination
    - See spec.md lines 1113-1118 for integration details
  - [ ] 9.7 Integrate Configuration Service with Wave 2 (Guidance Line Core)
    - IABLineService, ICurveLineService, IContourService receive guidance settings (look-ahead, snap distances)
    - Use IStateMediatorService for coordination
    - See spec.md lines 1119-1123 for integration details
  - [ ] 9.8 Integrate Configuration Service with Wave 3 (Steering Algorithms)
    - IStanleySteeringService, IPurePursuitService receive algorithm parameters
    - ILookAheadDistanceService receives look-ahead settings
    - Use IStateMediatorService for coordination
    - See spec.md lines 1124-1129 for integration details
  - [ ] 9.9 Integrate Configuration Service with Wave 4 (Section Control)
    - ISectionConfigurationService receives section control settings (count, positions)
    - ISectionControlService receives work mode settings
    - Use IStateMediatorService for coordination
    - See spec.md lines 1130-1135 for integration details
  - [ ] 9.10 Integrate Configuration Service with Wave 5 (Field Operations)
    - IBoundaryManagementService, IHeadlandService, IUTurnService receive field settings
    - Use IUndoRedoService for boundary/headland edits
    - Use ISessionManagementService to track current field
    - See spec.md lines 1136-1142 for integration details
  - [ ] 9.11 Integrate Configuration Service with Wave 6 (Hardware I/O)
    - IAutoSteerCommunicationService receives steering hardware settings (CPD, Ackermann, PWM)
    - IMachineCommunicationService receives section control settings
    - IImuCommunicationService receives IMU settings
    - IModuleCoordinatorService receives communication settings (auto-start AgIO)
    - Use IStateMediatorService for coordination
    - See spec.md lines 1143-1149 for integration details
  - [ ] 9.12 Integrate Configuration Service with Wave 7 (Display & Visualization)
    - IDisplayFormatterService receives display settings (unit system, speed source)
    - IFieldStatisticsService receives unit system and display preferences
    - Use IStateMediatorService for coordination
    - See spec.md lines 1150-1153 for integration details
  - [ ] 9.13 Document application lifecycle
    - Startup sequence: Check first-time setup → Load profiles → Validate settings → Initialize services → Check crash recovery → Start session
    - Shutdown sequence: Save session → End session → Save settings → Clear crash recovery
    - See spec.md lines 1257-1339 for full lifecycle details
  - [ ] 9.14 Ensure integration tests pass
    - Run ONLY the 2-8 tests written in 9.1
    - Verify service initialization and communication
    - Do NOT run the entire test suite at this stage

**Acceptance Criteria:**
- The 2-8 tests written in 9.1 pass
- All 6 Wave 8 services registered in DI container
- SetupWizardService guides first-time users through configuration
- DefaultSettingsProvider provides sensible defaults for all settings
- Configuration Service successfully integrates with all Wave 1-7 services
- StateMediatorService coordinates state changes across all services
- Application startup and shutdown sequences work correctly

---

### Verification: Testing & Performance

#### Task Group 10: Testing & Performance Verification
**Assigned Implementer:** testing-engineer
**Dependencies:** All previous task groups (1-9)
**Estimated Time:** 4-5 hours
**Estimated LOC:** 600-800 (test code)
**Parallelizable:** No (must wait for all implementation to complete)

- [ ] 10.0 Complete comprehensive testing and performance verification
  - [ ] 10.1 Review tests from Task Groups 1-9
    - Review approximately 16-24 tests written by api-engineer across Groups 1-9
    - Identify any critical gaps in test coverage for Wave 8 features
    - Focus on integration points and end-to-end workflows
  - [ ] 10.2 Analyze test coverage gaps for Wave 8 features only
    - Identify critical user workflows that lack test coverage
    - Focus ONLY on gaps related to Wave 8's state management requirements
    - Do NOT assess entire application test coverage
    - Prioritize: Settings persistence, crash recovery, profile switching, validation rules
  - [ ] 10.3 Write up to 10 additional strategic tests maximum
    - Add maximum of 10 new tests to fill identified critical gaps
    - Focus on integration points and end-to-end workflows
    - DO NOT write comprehensive coverage for all scenarios
    - Skip edge cases unless business-critical
    - Suggested critical tests:
      - Settings dual-write atomicity (JSON and XML both succeed or both fail)
      - Profile switching with session carry-over end-to-end
      - Crash recovery simulation (save snapshot, simulate crash, restore)
      - Cross-setting validation (e.g., section count mismatch detection)
      - Mediator notification routing to multiple services
      - Undo/redo stack integrity after multiple operations
      - Settings load fallback strategy (JSON → XML → defaults)
      - Configuration Service thread-safe concurrent access
      - Validation Service performance (<10ms for full validation)
      - Session snapshot performance (<500ms for crash recovery save)
  - [ ] 10.4 Write performance tests
    - Settings load from JSON: Must complete in <100ms
    - Settings load from XML: Must complete in <100ms
    - Full settings validation: Must complete in <10ms
    - Crash recovery snapshot: Must complete in <500ms
    - Profile switching: Must complete in <200ms
    - Mediator notification: Must complete in <10ms
    - Undo/redo operations: Must complete in <50ms
    - See spec.md lines 1340-1370 for performance requirements
  - [ ] 10.5 Write file I/O round-trip tests
    - JSON serialization → deserialization produces identical settings
    - XML serialization → deserialization produces identical settings
    - JSON → XML conversion maintains fidelity
    - XML → JSON conversion maintains fidelity
    - Test all 11 settings categories
  - [ ] 10.6 Write validation rule tests
    - Test all range validations (wheelbase, PWM, section count, etc.)
    - Test all cross-setting dependency validations
    - Test type validations (boolean, integer, double, string, enum)
    - Ensure 100% coverage of validation rules from spec.md lines 1038-1109
  - [ ] 10.7 Write integration tests for Wave 1-7 coordination
    - Test Configuration Service → Wave 1 (Position & Kinematics) integration
    - Test Configuration Service → Wave 2 (Guidance Line Core) integration
    - Test Configuration Service → Wave 3 (Steering Algorithms) integration
    - Test Configuration Service → Wave 4 (Section Control) integration
    - Test Configuration Service → Wave 5 (Field Operations) integration
    - Test Configuration Service → Wave 6 (Hardware I/O) integration
    - Test Configuration Service → Wave 7 (Display & Visualization) integration
    - Verify StateMediatorService correctly routes notifications
  - [ ] 10.8 Run feature-specific tests only
    - Run ONLY tests related to Wave 8's state management features
    - Expected total: approximately 26-34 tests maximum (16-24 from Groups 1-9 + up to 10 from Group 10)
    - Do NOT run the entire application test suite
    - Verify critical workflows pass
  - [ ] 10.9 Generate test coverage report for Wave 8 only
    - Focus on Configuration, Validation, Session, Profile, StateManagement, UndoRedo services
    - Identify any remaining critical gaps
    - Do NOT require 100% coverage, focus on critical paths
  - [ ] 10.10 Document test results and performance metrics
    - Create summary report of all tests (pass/fail counts)
    - Document performance test results (actual vs required)
    - List any known issues or limitations
    - Provide recommendations for future testing improvements

**Acceptance Criteria:**
- All feature-specific tests pass (approximately 26-34 tests total)
- Critical user workflows for Wave 8 are covered
- No more than 10 additional tests added by testing-engineer
- Testing focused exclusively on Wave 8's state management features
- All performance requirements met (settings load <100ms, validation <10ms, snapshot <500ms)
- File I/O round-trip tests confirm 100% fidelity
- Validation rule tests confirm 100% coverage of constraints
- Integration tests confirm proper coordination with Waves 1-7

---

## Execution Order

**Recommended implementation sequence:**

### Phase 1: Foundation Models (Can run in parallel)
- Task Group 1: Settings Models (independent)
- Task Group 2: Session/Profile Models (independent)

### Phase 2: Core Services (Some parallelization possible)
- Task Group 3: Configuration Service (depends on Groups 1-2)
- Task Group 4: Validation Service (depends on Groups 1-2, can run parallel to Group 3)
- Task Group 6: Session Management (depends on Groups 1-2, can run parallel to Groups 3-4)
- Task Group 7: State Mediator (depends on Groups 1-2, can run parallel to Groups 3-6)
- Task Group 8: Undo/Redo Service (depends on Groups 1-2, can run parallel to Groups 3-7)

### Phase 3: Profile Management (Sequential)
- Task Group 5: Profile Management (depends on Groups 1-3)

### Phase 4: Integration (Sequential)
- Task Group 9: Service Registration & Integration (depends on all Groups 1-8)

### Phase 5: Verification (Sequential)
- Task Group 10: Testing & Performance (depends on all Groups 1-9)

**Estimated Timeline:**
- Phase 1: 5-7 hours (parallel execution)
- Phase 2: 10-14 hours (partial parallel execution)
- Phase 3: 3-4 hours (sequential)
- Phase 4: 2-3 hours (sequential)
- Phase 5: 4-5 hours (sequential)
- **Total: 24-33 hours** (with optimal parallelization, can be reduced to 18-24 hours)

---

## Key Implementation Notes

### Namespace Conventions
- Follow NAMING_CONVENTIONS.md strictly
- Configuration models: `AgValoniaGPS.Models.Configuration`
- Configuration services: `AgValoniaGPS.Services.Configuration`
- Session services: `AgValoniaGPS.Services.Session`
- State management services: `AgValoniaGPS.Services.StateManagement`
- Validation services: `AgValoniaGPS.Services.Validation`
- Profile services: `AgValoniaGPS.Services.Profile`
- Undo/Redo services: `AgValoniaGPS.Services.UndoRedo`
- Setup services: `AgValoniaGPS.Services.Setup`

### File Locations
- Settings files: `Documents/AgValoniaGPS/Vehicles/[VehicleName].json` and `[VehicleName].xml`
- User profiles: `Documents/AgValoniaGPS/Users/[UserName].json`
- Session state: `Documents/AgValoniaGPS/Sessions/LastSession.json`
- Crash recovery: `Documents/AgValoniaGPS/Sessions/CrashRecovery.json`

### Performance Targets
- Settings load: <100ms
- Validation: <10ms
- Crash recovery snapshot: <500ms
- Profile switching: <200ms
- Mediator notification: <10ms
- Undo/redo operations: <50ms

### Testing Strategy
- Minimal tests during development (2-8 tests per task group)
- Focus on critical behaviors only
- Comprehensive testing phase at end (Task Group 10)
- Maximum 10 additional tests from testing-engineer
- Total expected: 26-34 tests for entire Wave 8

### Cross-Service Integration
- Use IStateMediatorService for all cross-service coordination
- Avoid tight coupling between services
- Event-driven architecture for state changes
- Thread-safe concurrent access for all services

### Legacy Compatibility
- Maintain exact XML field names for backward compatibility
- Support both JSON (primary) and XML (legacy) formats
- Dual-write on every settings change
- Conversion between hierarchical JSON and flat XML
