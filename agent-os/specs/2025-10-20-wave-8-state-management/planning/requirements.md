# Spec Requirements: Wave 8 - State Management

## Initial Description
Wave 8: State Management

## Requirements Discussion

### First Round Questions

**Q1: Settings Persistence Strategy**
Should we maintain XML compatibility and also introduce JSON for modern settings management?
**Answer:** Maintain XML for compatibility. Create JSON equivalents as path to phasing out XML. Two-way sync for this first version.

**Q2: Configuration Service Pattern**
Should we have a centralized Configuration Service that serves as the single source of truth for all application settings, or should individual services maintain their own settings?
**Answer:** Centralized service is ideal. One source of truth for all settings. No settings stored in individual services.

**Q3: Session Management Scope**
Should session management include crash recovery (saving current field, work in progress, guidance state)?
**Answer:** Crash recovery is excellent. Solves a current system problem.

**Q4: State Coordination Between Services**
How should state changes coordinate across services? Should we use a Mediator pattern, pub/sub events, or have services directly reference the state management service?
**Answer:** Mediator pattern preferred. Services shouldn't get bogged down by central service.

**Q5: Settings Migration & Defaults**
When a user upgrades from legacy AgOpenGPS, should we auto-migrate their XML settings to the new format?
**Answer:** Two-way for this first version.

**Q6: Undo/Redo Scope**
Should undo/redo apply to all state changes (including GPS position updates) or just user-initiated actions (like creating guidance lines, changing settings)?
**Answer:** User entered data has undo/redo. Right scope confirmed.

**Q7: Application Lifecycle Management**
On first launch, should we guide users through a setup wizard or load with sensible defaults they can modify later?
**Answer:** Both wizard and defaults. Walk users through setup with defaults.

**Q8: Settings Validation & Constraints**
Should validation be part of the Configuration Service or a separate Validation Service?
**Answer:** A validation service.

**Q9: Multi-User or Multi-Vehicle Support**
Should the state management system support multiple vehicles (tractors with different configurations) and/or multiple user profiles?
**Answer:** Multi-vehicle support is a must. Users take tablet from tractor to tractor. User profile support would be good.

### Existing Code to Reference

**Similar Features Identified:**
No specific similar features were identified for reference, as this is foundational infrastructure.

### Follow-up Questions

**Follow-up 1: Settings Organization & Hierarchy**
The visual shows a flat XML structure with 50+ settings covering Vehicle, Guidance, GPS, Section Control, and Hardware areas. For the new system with both XML and JSON, should we:
- Keep a flat structure like the legacy system for XML compatibility?
- Create a hierarchical/grouped structure in JSON (e.g., VehicleSettings.Wheelbase, GuidanceSettings.LookAhead)?
- Or maintain the exact flat structure in both formats for consistency?
**Answer:** No to flat structure for compatibility. Yes to hierarchical/grouped structure in JSON. Convert from Hierarchical JSON to flat XML on save.

**Follow-up 2: Service Directory Naming**
According to NAMING_CONVENTIONS.md, we should organize by functional area. For State Management, should we create:
- AgValoniaGPS.Services/Configuration/ (for settings services)?
- AgValoniaGPS.Services/Session/ (for session management)?
- AgValoniaGPS.Services/StateManagement/ (combining all state concerns)?
- Or use an existing directory like AgValoniaGPS.Services/Application/?
**Answer:** Yes to Configuration/, Yes to Session/, Yes to StateManagement/, No to Application/.

**Follow-up 3: Validation Constraints**
Looking at the settings (e.g., "Max Steer Deg" affects steering limits, "Number Sections" affects section control), what are the critical validation rules we need to enforce? For example:
- Wheelbase must be positive and realistic (50-500 cm)?
- PWM values must be in valid ranges (0-255)?
- Section count must match physical hardware (1-32)?
- Any cross-setting dependencies (e.g., tool width affects guidance calculations)?
**Answer:** Yes to wheelbase validation (50-500 cm). Yes to PWM validation (0-255). Yes to section count validation (1-32). Yes to cross-setting dependencies.

**Follow-up 4: Vehicle/User Profile Switching UX**
Since users move tablets between tractors and multiple operators exist, how should profile switching work:
- At application startup (select vehicle/user from list)?
- Runtime switching via menu (no restart required)?
- Should user profiles be separate from vehicle profiles, or combined?
- When switching vehicles, should session data (current field, work progress) carry over or reset?
**Answer:** Yes to startup selection with the first user added to the system as default. Yes to runtime switching via menu. Separate user profiles from vehicle profiles. Option to allow user choice on session data carry-over vs reset.

**Follow-up 5: XML-to-JSON Migration Timeline**
You mentioned "path to phasing out XML." Should we:
- Immediately create JSON files alongside XML on every settings change (dual-write)?
- Migrate on first app launch (read XML, write JSON, keep XML for compatibility)?
- Only migrate when user explicitly requests it?
- Is there a future Wave where we'll deprecate XML reading entirely?
**Answer:** Yes to dual-write (create JSON alongside XML on every change). No to automatic migration on first launch. Make explicit migration an option. Not planning to deprecate XML reading in this current iteration - expect XML support for months to years.

## Visual Assets

### Files Provided:
- `All_Settings.png`: Comprehensive screenshot of legacy AgOpenGPS settings structure showing the Deere 5055e.xml file contents

### Visual Insights:

**File Organization:**
- Settings stored in vehicle-specific XML files (e.g., "Deere 5055e.xml")
- Directory path shows settings stored in Documents/AgOpenGPS/Vehicles/
- This confirms the multi-vehicle support requirement

**Settings Structure (Flat XML):**
The screenshot reveals 50+ settings organized across multiple functional areas:

1. **Culture & Localization:**
   - Culture code ("en")
   - Language settings

2. **Vehicle Physical Configuration:**
   - Wheelbase: 180 cm
   - Track: 30 cm
   - Max Steer Deg: 45 degrees
   - Pivot Behind Ant: 30 cm
   - Steer Axle Ahead: 110 cm
   - Max Angular Vel: 100 degrees/sec
   - Antenna Pivot, Height, Offset: 25, 50, 0 cm
   - Vehicle Type: 0
   - Veh Hitch Length: 0 cm

3. **Steering Hardware:**
   - CPD (Counts Per Degree): 100
   - Ackermann: 100
   - WAS Offset: 0.04
   - High PWM: 235
   - Low PWM: 78
   - Min PWM: 5
   - Max PWM: 10
   - Panic Stop: 0

4. **Tool Configuration:**
   - Tool Width: 4 (1.828 actual value shown)
   - Tool Front: False
   - Tool Rear Fixed: True
   - Tool TBT: False
   - Tool Trailing: False
   - Tool To Pivot Len: 0
   - Tool Look And On: 1
   - Tool Look And Off: 0.5
   - Tool Off Delay: 0
   - Tool Offset: 0
   - Tool Overlap: 0
   - Trailing Hitch Len: -2.5
   - Tank Hitch Length: 3
   - Hyd Lift Look And: 2

5. **Work Modes & Switches:**
   - Remote Work: False
   - Steer Work Sw: False
   - Steer Work Manual: True
   - Work Active Low: False
   - Work Switch: False
   - Work Manual Sec: True

6. **Section Control:**
   - Number Sections: 3
   - Headland Sec Control: False
   - Fast Sections: True
   - Section Off Out Bnds: True
   - Sections Not Zones: True
   - Low Speed Cutoff: 0.5 (actual: 1)

7. **GPS & RTK Settings:**
   - Min Uturn Radius: 3
   - HDOP: 0.69
   - Frame: 4.7
   - Raw Hz: 9.890
   - Hz: 10.0
   - GPS Age Alarm: 20
   - Heading From: Dual
   - Auto Start AgIO: True
   - Auto Off AgIO: False
   - RTK: False
   - RTK Kill AutoSteer: False
   - Dual As IMU: False
   - Dual Heading Offset: 90

8. **IMU & Heading:**
   - IMU Fusion Weight: 0.06
   - Min Heading Step: 0.5
   - Min Step Limit: 0.05
   - SideHill Comp: 0
   - Heading: 20.6 degrees
   - IMU: 0 degrees
   - Heading Error: 1
   - Distance Error: 1
   - Steer Integral: 0

9. **Guidance Settings:**
   - WAS Offset: 0.04
   - Ackermann: 100
   - Acquire Factor: 0.9 (actual: 0.90)
   - Look Ahead: 3
   - Speed Factor: 1
   - PP Integral: 0
   - Snap Distance: 20
   - Ref Snap Distance: 5

10. **System State:**
    - Roll Zero: 0
    - Invert Roll: False
    - Roll Filter: 0.15
    - Steer In Reverse: False
    - Reverse On: True
    - Stanley Used: False

11. **UI/Display:**
    - Dead Head Delay: 10, 10
    - North: 1.15
    - East: -1.76
    - Elev: 200.8
    - RTK fixx: (status indicator)
    - # Sats: 19
    - Missed: 527
    - Fix2Fix: 20.6 degrees
    - AV: 0.00

**Data Types Observed:**
- Boolean flags (True/False): Work switches, feature toggles, hardware options
- Integer values: Dimensions in cm, counts, section numbers, PWM ranges
- Decimal/floating point: Angular measurements, precise physical dimensions, filter weights, factors
- String values: Culture codes, heading source indicators
- Enum-like integers: Vehicle Type (0)

**Fidelity Level:** High-fidelity screenshot - this is actual production data from a real vehicle configuration file.

**Key Design Implications:**
1. Need to maintain exact XML field names for backward compatibility
2. JSON structure should group these flat settings into logical hierarchies
3. Many interdependent settings (e.g., tool width affects guidance, steering limits affect control)
4. Settings span hardware, software, and user preference concerns
5. Some settings are hardware-constrained (PWM ranges), others are physical measurements (wheelbase)
6. Multiple coordinate systems and units (cm, degrees, Hz, counts)

## Requirements Summary

### Functional Requirements

**Core State Management:**
- Centralized Configuration Service as single source of truth for all application settings
- Session management with crash recovery (save/restore current field, work progress, guidance state)
- Undo/redo stack for user-initiated actions (exclude automatic updates like GPS positions)
- Application lifecycle management with setup wizard and sensible defaults

**Settings Persistence:**
- Dual-format support: hierarchical JSON (primary) with flat XML (compatibility)
- Dual-write: save to both JSON and XML on every settings change
- JSON-to-flat-XML conversion on save to maintain legacy format compatibility
- Optional explicit XML-to-JSON migration tool for users
- Vehicle-specific settings files (e.g., "Deere 5055e.json" and "Deere 5055e.xml")

**Settings Hierarchy (JSON Structure):**
- VehicleSettings: Physical configuration (wheelbase, track, dimensions, vehicle type)
- SteeringSettings: Hardware and control parameters (PWM ranges, Ackermann, CPD, limits)
- ToolSettings: Implement configuration (width, offsets, hitches, look-ahead/off, delays)
- SectionControlSettings: Section management (count, fast sections, out-of-bounds behavior)
- GpsSettings: GPS/RTK configuration (Hz, HDOP, age alarm, heading source)
- ImuSettings: IMU and heading fusion (fusion weight, heading steps, roll filter)
- GuidanceSettings: Guidance algorithms (acquire factor, look-ahead, speed factor, snap distances)
- WorkModeSettings: Work switches and manual controls
- CultureSettings: Localization and language
- SystemStateSettings: Runtime state (heading, position, errors, satellite counts)

**Multi-Profile Support:**
- Multiple vehicle profiles (different tractors with different configurations)
- Multiple user profiles (separate from vehicles)
- Startup: select vehicle/user from list (default to first user added)
- Runtime: switch vehicle/user via menu without restart
- Session carry-over: user choice to preserve or reset session data when switching vehicles

**Validation:**
- Separate Validation Service (not embedded in Configuration Service)
- Range validation: Wheelbase (50-500 cm), PWM (0-255), Section count (1-32)
- Cross-setting dependency validation (e.g., tool width affects guidance calculations)
- Type safety: Boolean flags, integer counts, decimal measurements, string codes
- Unit consistency: cm for distances, degrees for angles, Hz for frequencies

**State Coordination:**
- Mediator pattern for cross-service state coordination
- Services don't directly depend on state management service to avoid coupling
- Event-driven state change notifications
- Ensure services aren't bogged down by central coordination

### Reusability Opportunities
No specific existing features identified, but should follow established patterns:
- Service registration patterns from existing AgValoniaGPS.Services
- Dependency injection via ServiceCollectionExtensions.cs
- Event-driven architecture for state change notifications
- Settings validation patterns similar to existing validation in the codebase

### Scope Boundaries

**In Scope:**
- Configuration Service (centralized settings management)
- Session Management Service (crash recovery)
- Validation Service (settings constraints)
- Undo/Redo Service (user action history)
- Settings file I/O (dual JSON/XML format)
- JSON-to-XML conversion on save
- Multi-vehicle profile management
- Multi-user profile management
- Profile switching at startup and runtime
- Setup wizard with defaults
- Optional explicit XML-to-JSON migration tool

**Out of Scope:**
- Deprecating XML reading entirely (planned for months/years in the future)
- Automatic XML-to-JSON migration on first launch
- Cloud sync of settings across devices
- Settings version control/history beyond undo/redo
- Real-time collaborative editing of settings
- Settings export/import for sharing between users
- Advanced analytics on settings usage patterns

### Technical Considerations

**Service Organization:**
- `AgValoniaGPS.Services/Configuration/`: Settings management services
- `AgValoniaGPS.Services/Session/`: Session and crash recovery services
- `AgValoniaGPS.Services/StateManagement/`: Combined state coordination concerns
- Follow NAMING_CONVENTIONS.md for directory structure to avoid namespace collisions

**Legacy Compatibility:**
- Must read existing flat XML files (50+ settings)
- Must maintain exact XML field names for backward compatibility
- Settings stored in Documents/AgOpenGPS/Vehicles/ directory pattern
- Vehicle-specific file naming: [VehicleName].xml and [VehicleName].json

**Data Integrity:**
- Atomic dual-write to JSON and XML (both succeed or both fail)
- Validation before save to prevent invalid settings persistence
- Crash recovery to restore last known good state
- Cross-setting dependency validation to prevent inconsistent states

**Performance:**
- Settings load on application startup
- In-memory settings cache (Configuration Service holds current state)
- Efficient validation (don't validate unchanged settings)
- Mediator pattern should not create performance bottlenecks

**Migration Strategy:**
- Phase 1 (this Wave): Dual-format with JSON as primary, XML for compatibility
- Phase 2 (future): Optional explicit migration tool for users
- Phase 3 (months/years): Potentially deprecate XML reading (not in this iteration)

**Technology Stack:**
- .NET 8
- System.Text.Json for JSON serialization
- System.Xml for XML compatibility
- Microsoft.Extensions.DependencyInjection for service registration
- Event-driven architecture for state change notifications
- Mediator pattern for cross-service coordination

**Testing Requirements:**
- Comprehensive unit tests for all services (xUnit or NUnit per project standards)
- Settings validation test coverage
- JSON/XML conversion round-trip tests
- Profile switching tests
- Crash recovery simulation tests
- Performance tests for settings load/save operations
