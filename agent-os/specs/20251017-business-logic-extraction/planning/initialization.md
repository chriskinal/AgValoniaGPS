# Spec Initialization: Business Logic Extraction

**Date**: 2025-10-17
**Spec Name**: Business Logic Extraction from AgOpenGPS WinForms to AgValoniaGPS Clean Architecture
**Requested by**: User

## Overview

Create a comprehensive specification for systematically extracting ~20,000 lines of embedded business logic from the WinForms-based AgOpenGPS application into clean, testable services for the cross-platform AgValoniaGPS application.

## Source Documentation

- **Primary Reference**: `agent-os/product/business-logic-extraction-plan.md`
- **Supporting Documents**:
  - `agent-os/product/feature-extraction-roadmap.md` - 50+ features in 8 waves
  - `agent-os/product/extraction-patterns-guide.md` - 9 transformation patterns
  - `agent-os/product/mission.md` - Product principles
  - `agent-os/product/tech-stack.md` - Technology stack

## Key Requirements

### Extraction Scope
- Extract business logic from 65 WinForms files, primarily FormGPS and partials
- 8 waves of feature extraction organized by dependencies
- ~50+ features spanning GPS, guidance, field operations, hardware communication

### Core Principles
1. **No wholesale code copying** - Use AgOpenGPS as behavioral reference only
2. **Test-first approach** - Write tests based on expected behavior before extraction
3. **Dependency-ordered waves** - Extract foundation features before dependent ones
4. **Clean architecture** - Services must be UI-agnostic and fully testable
5. **Maintain functionality** - Preserve all original behavior during extraction

### Architecture Requirements
- Pure .NET 8.0 services with no UI framework dependencies
- Interface-based design with dependency injection
- Event-driven communication using standard .NET events
- Cross-platform compatibility (Windows, Linux, macOS)

## Extraction Waves

1. **Wave 0**: Already completed (GPS parsing, UDP, Field I/O, NTRIP)
2. **Wave 1**: Position & Kinematics (2 weeks)
3. **Wave 2**: Guidance Lines (2 weeks)
4. **Wave 3**: Steering Algorithms (1.5 weeks)
5. **Wave 4**: Section Control (2 weeks)
6. **Wave 5**: Field Operations (1.5 weeks)
7. **Wave 6**: Hardware Communication (1.5 weeks)
8. **Wave 7**: Display & Visualization (1 week)
9. **Wave 8**: State Management (1.5 weeks)

**Total Timeline**: 10-12 weeks

## Success Criteria

- All business logic removed from UI layer
- Service interfaces defined for all features
- Unit test coverage >80% for extracted services
- No WinForms dependencies in service layer
- Original functionality preserved
- Performance equal or better than original

## Next Steps

1. Create detailed specification document
2. Break down into actionable tasks
3. Verify against requirements
4. Begin Wave 1 implementation
