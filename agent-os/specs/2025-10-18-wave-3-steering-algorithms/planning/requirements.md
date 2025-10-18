# Spec Requirements: Wave 3 Steering Algorithms

## Initial Description
Wave 3 focuses on implementing steering control algorithms for AgValoniaGPS. This wave depends on Wave 1 (Position & Kinematics) and Wave 2 (Guidance Lines) already being complete.

The three main features to implement are:
1. Stanley Steering Algorithm - Cross-track error and heading error based steering
2. Pure Pursuit Algorithm - Look-ahead point based steering
3. Look-Ahead Distance Calculator - Adaptive look-ahead distance based on speed and conditions

This extracts ~1,500 lines of business logic from the original AgOpenGPS FormGPS/Position.designer.cs and CVehicle.cs files.

## Requirements Discussion

### First Round Questions

**Q1:** I assume you want to implement both steering algorithms (Stanley and Pure Pursuit) as separate, selectable options in the UI, similar to how the original AgOpenGPS allows switching between them. Is that correct, or should one be prioritized over the other?
**Answer:** Implement both Stanley and Pure Pursuit as separate algorithms. User will select which algorithm to use via UI.

**Q2:** For the service directory structure, I'm thinking we should place these in `AgValoniaGPS.Services/Guidance/Steering/` to keep them organized alongside the guidance line services. Should we keep them in the flat Guidance folder instead, or use a Steering subdirectory?
**Answer:** Keep services flat in `AgValoniaGPS.Services/Guidance/` directory (no Steering subdirectory).

**Q3:** Regarding performance requirements - the original steering loop runs at a specific frequency. I assume we need to match the 100Hz update rate of the original system for real-time steering control. Is that the target, or do you have different performance requirements?
**Answer:** The steering loop in the MCU runs at 100Hz. The services should match that performance target.

**Q4:** For tuning parameters (like Stanley gain, Pure Pursuit look-ahead distance multipliers, etc.), should these be exposed as configurable settings that users can adjust, or should they use the proven defaults from the original AgOpenGPS codebase?
**Answer:** Yes, all tuning parameters need to be exposed so they can be set in the UI.

**Q5:** I'm assuming the look-ahead distance adaptation should consider vehicle speed, cross-track error magnitude, and guidance line curvature. Should it also account for other conditions like vehicle type (tractor vs combine) or implement type (cultivator vs planter)?
**Answer:** The assumptions are correct - conditions means cross-track error, curvature of the guidance line, and vehicle type.

**Q6:** For integration with existing services - these steering algorithms will need to consume position data from Wave 1 services and guidance line data from Wave 2 services. Should the steering commands then be sent to the AutoSteer module via PGN messages, or is there a different output mechanism you prefer?
**Answer:** Steering commands should be sent via PGN over UDP to the AutoSteer module.

**Q7:** Should we support runtime switching between Stanley and Pure Pursuit algorithms (user can toggle while operating), or should the algorithm selection only be changeable when guidance is not active?
**Answer:** Yes, allow real-time switching between algorithms during operation.

**Q8:** For testing scenarios, should we include edge cases like tight curves, U-turns at headlands, sudden course corrections after GPS loss, and different vehicle configurations (tractors vs combines)?
**Answer:** Yes, test edge cases including tight curves, U-turns at headlands, sudden course corrections, and different vehicle configurations (tractors vs combines).

**Q9:** Is there anything that should be explicitly OUT of scope for this wave? For example, should we defer advanced features like adaptive Stanley gain or machine-learning-based parameter tuning to a future wave?
**Answer:** Nothing is out of scope for this wave.

### Existing Code to Reference

**Similar Features Identified:**
According to the product roadmap (agent-os/product/roadmap.md), the following is already marked as completed:
- Feature: "Guidance service with Stanley and Pure Pursuit algorithms"
- Status: Already Completed in "GPS & Navigation Core"

This suggests there is existing implementation of these algorithms that can be referenced or potentially refactored to match the new Wave 3 architecture requirements.

**Investigation Needed:**
The spec-writer should investigate the existing guidance service implementation to:
- Understand the current Stanley and Pure Pursuit algorithm implementations
- Identify which tuning parameters are already exposed
- Determine what needs to be extracted or refactored from the existing code
- Ensure the new services align with the existing architecture patterns

## Visual Assets

### Files Provided:
- `Screenshot 2025-10-18 at 6.04.46 AM.png`: Shows the AgOpenGPS application UI with aerial field view, displaying a vehicle (tractor icon) following a guidance line. The purple cross-track error line is clearly visible, showing the vehicle's deviation from the guidance path. The UI includes a bottom panel with guidance controls and a red progress/status bar.

- `Screenshot 2025-10-18 at 6.06.11 AM.png`: Shows the same AgOpenGPS UI with multiple guidance lines visible across the field. The vehicle is positioned between guidance lines, demonstrating the navigation scenario where steering algorithms must handle line following and transitions.

### Visual Insights:
- **Real-time Feedback**: The UI shows real-time cross-track error visualization (purple line from vehicle to guidance line), which is critical input for both Stanley and Pure Pursuit algorithms
- **Guidance Line Visualization**: Yellow/green guidance lines are clearly rendered, showing the paths the steering algorithms must follow
- **Vehicle Representation**: The vehicle sprite shows orientation (heading), which is essential for Stanley algorithm's heading error calculation
- **Field Context**: The aerial imagery provides context for testing scenarios (field boundaries, headlands, obstacles)
- **UI Integration**: The bottom control panel suggests where algorithm selection and tuning parameter controls should be integrated
- **Fidelity Level**: High-fidelity screenshots of existing application UI

## Requirements Summary

### Functional Requirements

**Core Algorithms:**
1. **Stanley Steering Algorithm**
   - Calculate cross-track error from vehicle position to guidance line
   - Calculate heading error (difference between vehicle heading and guidance line heading)
   - Combine errors using Stanley gain parameter
   - Output steering angle command
   - Support configurable Stanley gain (K parameter)

2. **Pure Pursuit Algorithm**
   - Calculate look-ahead point on guidance line based on adaptive look-ahead distance
   - Calculate steering angle to reach look-ahead point
   - Support configurable look-ahead distance multipliers
   - Handle edge cases when look-ahead extends beyond guidance line

3. **Look-Ahead Distance Calculator**
   - Adapt look-ahead distance based on vehicle speed
   - Adjust for cross-track error magnitude (reduce look-ahead when far off track)
   - Adjust for guidance line curvature (reduce look-ahead in tight curves)
   - Consider vehicle type (different parameters for tractors vs combines)
   - Provide configurable parameters for adaptation logic

**User Actions Enabled:**
- Select steering algorithm (Stanley or Pure Pursuit) via UI
- Switch between algorithms in real-time during operation
- Configure tuning parameters for each algorithm
- Monitor steering performance through visual feedback

**Data to be Managed:**
- Current steering algorithm selection (Stanley/Pure Pursuit)
- Stanley algorithm parameters (gain K, wheelbase)
- Pure Pursuit parameters (look-ahead multipliers, minimum/maximum distances)
- Look-ahead distance adaptation parameters (speed coefficients, error thresholds, curvature limits)
- Vehicle configuration (type, dimensions affecting steering)
- Real-time steering commands (angle, direction)

**Performance Requirements:**
- Update rate: 100Hz to match MCU steering loop
- Low latency: Steering calculations must complete within 10ms per cycle
- Thread safety: Services must support concurrent access from UI and guidance loops
- Numerical stability: Algorithms must handle edge cases without NaN/infinity errors

### Reusability Opportunities

**Existing Guidance Service:**
- Current implementation already includes Stanley and Pure Pursuit algorithms
- Reference existing tuning parameters and proven default values
- Leverage existing cross-track error calculations
- Reuse existing guidance line geometry utilities

**Components to Investigate:**
- Position services from Wave 1 (GPS position, heading, speed)
- Guidance line services from Wave 2 (line geometry, closest point calculations)
- Vehicle configuration models (wheelbase, turning radius, vehicle type)
- PGN message formatting for AutoSteer commands
- UDP communication service for sending steering commands

**Backend Patterns to Follow:**
- Service-based architecture consistent with Wave 1 and Wave 2
- Dependency injection for service composition
- Interface-based design for testability
- ReactiveUI patterns for real-time data flow

### Scope Boundaries

**In Scope:**
- Stanley steering algorithm service with configurable parameters
- Pure Pursuit steering algorithm service with configurable parameters
- Look-ahead distance calculator service with adaptive logic
- Algorithm selection mechanism (UI integration points)
- Real-time algorithm switching capability
- Tuning parameter exposure for UI configuration
- PGN message generation for steering commands
- UDP transmission to AutoSteer module
- Comprehensive unit tests for all algorithms
- Edge case testing (tight curves, U-turns, headlands, GPS loss recovery)
- Multi-vehicle configuration support (tractors, combines)
- Performance optimization to meet 100Hz target
- Integration with Wave 1 (position/kinematics) and Wave 2 (guidance lines)

**Out of Scope:**
- Nothing explicitly excluded - all advanced features are in scope for this wave
- (Note: User specified no exclusions for this wave)

### Technical Considerations

**Integration Points:**
- Consumes position data from Wave 1 services (GPSPosition, Heading, Speed, KinematicsService)
- Consumes guidance line data from Wave 2 services (ActiveGuidanceLine, CrossTrackError)
- Outputs steering commands via UDP PGN messages to AutoSteer module
- Exposes configuration properties for UI binding (ReactiveUI)
- Supports real-time algorithm switching without disrupting operation

**Existing System Constraints:**
- Must maintain compatibility with existing AutoSteer module expecting PGN format
- Must align with .NET 8.0 / Avalonia UI / ReactiveUI architecture
- Must follow MVVM pattern with dependency injection
- Must integrate with existing UDP communication service (port 9999)
- Should match performance characteristics of original AgOpenGPS (100Hz update rate)

**Technology Preferences:**
- C# .NET 8.0
- Avalonia UI 11.3.6 for cross-platform UI
- ReactiveUI for reactive data binding
- Dependency injection for service composition
- NUnit for unit testing with AAA pattern
- Silk.NET OpenGL for rendering (if visualization needed)

**Architecture Patterns:**
- Services placed in flat structure: `AgValoniaGPS.Services/Guidance/`
- Interface-based design for algorithm abstraction
- Strategy pattern for algorithm selection
- Factory pattern or DI-based algorithm instantiation
- Reactive properties for real-time data flow
- Event-driven updates for steering command transmission

**Testing Requirements:**
- Unit tests for each algorithm with known input/output scenarios
- Edge case tests: tight curves, U-turns, headlands
- Recovery tests: GPS loss, sudden course corrections
- Configuration tests: different vehicle types and parameters
- Performance tests: verify 100Hz update rate capability
- Integration tests: end-to-end from position input to PGN output
- Regression tests: ensure behavior matches original AgOpenGPS
