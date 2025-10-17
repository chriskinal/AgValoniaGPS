# Requirements: Business Logic Extraction

## Project Goal

Systematically extract all embedded business logic from AgOpenGPS WinForms application into clean, testable, UI-agnostic services for AgValoniaGPS, enabling true cross-platform support while preserving all original functionality.

## Background

The AgOpenGPS WinForms application contains approximately 20,000 lines of business logic embedded throughout 65+ form files. This logic includes:
- GPS position processing and sensor fusion
- Complex vehicle kinematics calculations
- Guidance algorithms (AB lines, curves, contours)
- Steering control (Stanley, Pure Pursuit)
- Section control and coverage mapping
- Field boundary operations
- Hardware communication protocols
- State management across the application

This embedded logic prevents:
- Cross-platform deployment
- Unit testing of business logic
- Reuse of services in different UIs (Qt, web, mobile)
- Clean separation of concerns
- Independent evolution of UI and business logic

## Functional Requirements

### FR1: Extract All Business Logic from UI
- **FR1.1**: Remove all calculation logic from event handlers
- **FR1.2**: Remove all state management from form fields
- **FR1.3**: Remove all timer-based business logic
- **FR1.4**: Remove all algorithmic code from rendering loops
- **FR1.5**: Extract all hardware communication logic

### FR2: Create Service Layer Architecture
- **FR2.1**: Design and implement interface-based services
- **FR2.2**: Organize services by domain (GPS, Guidance, Field, Machine, etc.)
- **FR2.3**: Implement dependency injection for all services
- **FR2.4**: Use standard .NET events for service-to-service communication
- **FR2.5**: Ensure services are stateless where possible

### FR3: Preserve Original Functionality
- **FR3.1**: All calculations must produce identical results to original
- **FR3.2**: Timing and performance must be equal or better
- **FR3.3**: Hardware communication protocols must remain compatible
- **FR3.4**: File formats must remain compatible with AgOpenGPS

### FR4: Extract Features in Dependency Order
- **FR4.1**: Wave 1 - Position & Kinematics (foundation)
- **FR4.2**: Wave 2 - Guidance Lines (depends on Wave 1)
- **FR4.3**: Wave 3 - Steering Algorithms (depends on Waves 1-2)
- **FR4.4**: Wave 4 - Section Control (depends on Waves 1-3)
- **FR4.5**: Wave 5 - Field Operations (depends on Waves 1-4)
- **FR4.6**: Wave 6 - Hardware Communication (depends on Waves 1-5)
- **FR4.7**: Wave 7 - Display & Visualization (depends on all)
- **FR4.8**: Wave 8 - State Management (depends on all)

## Non-Functional Requirements

### NFR1: Architecture Quality
- **NFR1.1**: Services MUST NOT reference any UI framework (Avalonia, WinForms, etc.)
- **NFR1.2**: Services MUST only depend on Models and standard .NET libraries
- **NFR1.3**: All services MUST be behind interfaces
- **NFR1.4**: Services MUST use dependency injection, no static state
- **NFR1.5**: Services MUST be thread-safe where concurrency is expected

### NFR2: Testing Requirements
- **NFR2.1**: Unit test coverage MUST be >80% for extracted services
- **NFR2.2**: Integration tests MUST verify service orchestration
- **NFR2.3**: Regression tests MUST confirm behavior matches original
- **NFR2.4**: Performance benchmarks MUST show equal or better performance

### NFR3: Code Quality
- **NFR3.1**: Follow C# 12 best practices and conventions
- **NFR3.2**: Use XML documentation on all public APIs
- **NFR3.3**: No code duplication - extract shared logic to utilities
- **NFR3.4**: Maximum cyclomatic complexity of 10 per method
- **NFR3.5**: Follow SOLID principles

### NFR4: Cross-Platform Compatibility
- **NFR4.1**: Services MUST compile and run on .NET 8.0
- **NFR4.2**: Services MUST work on Windows, Linux, and macOS
- **NFR4.3**: No platform-specific dependencies in service layer
- **NFR4.4**: File paths and I/O must be platform-agnostic

### NFR5: Performance
- **NFR5.1**: GPS processing must maintain 10Hz update rate
- **NFR5.2**: Steering calculations must complete in <10ms
- **NFR5.3**: Section control updates must occur in <50ms
- **NFR5.4**: Memory usage must not exceed 500MB working set

## Extraction Patterns to Implement

### Pattern 1: Timer Tick Extraction
- Extract business logic from timer event handlers into background services
- Use events to notify UI of state changes

### Pattern 2: Complex Calculation Extraction
- Extract mathematical algorithms from UI event handlers into pure functions
- Return results via interfaces, let UI decide presentation

### Pattern 3: State Machine Extraction
- Replace scattered form field state with formal state machine service
- Use state transition events for UI synchronization

### Pattern 4: Cross-Form Communication Extraction
- Replace direct form references with message bus pattern
- Use pub/sub for loose coupling

### Pattern 5: OpenGL Rendering Calculation Extraction
- Pre-calculate all display values in services
- Rendering only consumes pre-calculated render state

### Pattern 6: Global Variable Elimination
- Replace static variables with dependency injection
- Configuration objects injected into services

### Pattern 7: Async Operation Extraction
- Convert blocking operations to async/await patterns
- Use progress reporting and cancellation tokens

### Pattern 8: Configuration Extraction
- Centralize all settings in configuration service
- JSON-based configuration with hot-reload support

### Pattern 9: Testable Service Extraction
- Pure functions with no side effects where possible
- Interface-based dependencies for easy mocking

## Constraints

### Technical Constraints
- Must target .NET 8.0 (no newer versions due to LTS)
- Must maintain compatibility with existing AgOpenGPS field files
- Must maintain compatibility with Teensy hardware modules (UDP protocol)
- Cannot break existing AgValoniaGPS POC code

### Process Constraints
- Must extract features in dependency order (cannot skip waves)
- Must complete each wave before starting next
- Must write tests before or during extraction (not after)
- Must validate against original behavior at each wave

### Resource Constraints
- Timeline: 10-12 weeks total
- Each wave: 1-2 weeks
- Must maintain current AgValoniaGPS functionality during extraction

## Success Criteria

### Definition of Done for Each Feature Extraction
1. Service interface defined
2. Service implementation complete
3. Unit tests written and passing (>80% coverage)
4. Integration test with UI working
5. Behavior verified against original AgOpenGPS
6. No UI framework references in service
7. Registered in DI container
8. Documentation complete

### Definition of Done for Overall Project
1. All 50+ features extracted across 8 waves
2. Test coverage >80% on service layer
3. All regression tests passing
4. Performance benchmarks met or exceeded
5. Zero WinForms/UI references in service layer
6. Complete service interface documentation
7. Migration guide for remaining features

## Out of Scope

### Explicitly NOT Included
- Rewriting the UI layer (that's separate work)
- Adding new features not in original AgOpenGPS
- Improving algorithms (extract as-is first)
- Adding AgShare integration (separate project)
- Mobile platform support (future work)
- Web interface development (future work)

### Future Phases (After Extraction)
- Algorithm improvements and optimizations
- New features not in AgOpenGPS
- AgShare cloud integration
- Mobile companion apps
- Web-based field management

## References

- **Source**: `agent-os/product/business-logic-extraction-plan.md`
- **Roadmap**: `agent-os/product/feature-extraction-roadmap.md`
- **Patterns**: `agent-os/product/extraction-patterns-guide.md`
- **Tech Stack**: `agent-os/product/tech-stack.md`
- **Original Code**: `SourceCode/GPS/Forms/FormGPS.cs` and related files

## Acceptance Criteria

The extraction project is complete when:
1. ✅ All 50+ features from roadmap extracted
2. ✅ All services have interfaces and are DI-registered
3. ✅ Test coverage >80% on service layer
4. ✅ Zero UI framework dependencies in services
5. ✅ All original functionality preserved
6. ✅ Performance meets or exceeds original
7. ✅ Cross-platform compatibility verified
8. ✅ Documentation complete
