# Task 9: Dependency Injection Registration for Wave 2

## Overview
**Task Reference:** Task #9 from `agent-os/specs/2025-10-17-wave-2-guidance-line-core/tasks.md`
**Implemented By:** api-engineer
**Date:** 2025-10-17
**Status:** Complete

### Task Description
Register Wave 2 Guidance Line services in the dependency injection container with Scoped lifetime, following Wave 1 patterns.

## Implementation Summary
This implementation establishes the dependency injection infrastructure for Wave 2 Guidance Line Core services. Following the established Wave 1 pattern for service registration, I updated ServiceCollectionExtensions.cs to register three new guidance services with Scoped lifetime.

A comprehensive test suite was created to validate the registration, including tests for service resolution, lifetime configuration, scoped instance behavior, and circular dependency detection.

## Files Changed/Created

### New Files
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Services.Tests/DependencyInjection/Wave2ServiceRegistrationTests.cs` - Comprehensive DI registration tests

### Modified Files
- `C:/Users/chrisk/Documents/AgValoniaGPS/AgValoniaGPS/AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs` - Added Wave 2 service registrations
- `C:/Users/chrisk/Documents/AgValoniaGPS/agent-os/specs/2025-10-17-wave-2-guidance-line-core/tasks.md` - Marked Task Group 9 as complete

## Key Implementation Details

### ServiceCollectionExtensions.cs Updates
**Location:** ServiceCollectionExtensions.cs

Added Wave 2 service registrations:
- IABLineService -> ABLineService (Scoped)
- ICurveLineService -> CurveLineService (Scoped)
- IContourService -> ContourService (Scoped)

**Rationale:** Scoped lifetime chosen for per-operation isolation while maintaining state during field operations.

### Wave 2 Service Registration Tests
**Location:** Wave2ServiceRegistrationTests.cs

Created 9 comprehensive tests:
1. Service resolution tests (3 tests)
2. Lifetime validation test
3. Instantiation test
4. Scoped instance behavior tests (2 tests)
5. Registration verification test
6. Circular dependency test

## Testing

### Test Coverage
- Unit tests: Complete
- Integration tests: Complete
- Edge cases: Service resolution, lifetime validation, scoped behavior, circular dependencies

## User Standards & Preferences Compliance

### agent-os/standards/backend/api.md
**Complies:** Uses interface-based DI registration following service-oriented design.

### agent-os/standards/global/coding-style.md
**Complies:** Meaningful names, small focused functions, DRY principle, XML documentation.

### agent-os/standards/global/conventions.md
**Complies:** Standard C# extension method pattern, SOLID principles.

## Known Issues & Limitations

### Limitations
1. Dependency on Task Groups 3, 5, 7 - Service implementations must be created before tests will pass.

## Dependencies for Other Tasks
- Task Group 3 (AB Line Service Core)
- Task Group 5 (Curve Line Service Core)
- Task Group 7 (Contour Service)

## Notes
Scoped lifetime provides balance between testability and performance for guidance services.
