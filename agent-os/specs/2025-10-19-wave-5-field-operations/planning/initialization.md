# Initial Spec Idea

## User's Initial Description
Initialize a new spec for "Wave 5: Field Operations".

**Description:**
Wave 5 focuses on field boundary management, headland processing, and automated turn generation for the AgValoniaGPS precision agriculture application.

**Context:**
This is Wave 5 of 8 in the AgValoniaGPS business logic extraction from AgOpenGPS. Previous waves completed:
- Wave 1: Position & Kinematics Services ✅
- Wave 2: Guidance Line Core (ABLine, Curve, Contour) ✅
- Wave 3: Steering Algorithms (Pure Pursuit, Stanley, Look-ahead) ✅
- Wave 4: Section Control ✅

**Wave 5 Scope:**
According to the feature extraction roadmap, Wave 5 should include:

1. **Boundary Management Service** - Creating, editing, and validating field boundaries
   - Boundary recording from GPS positions
   - Point-in-polygon detection
   - Distance to boundary calculations
   - Area calculations (Shoelace formula)
   - Boundary simplification (Douglas-Peucker)

2. **Headland Service** - Headland creation and management
   - Generate headlands from field boundaries
   - Multi-pass headland support
   - Headland width configuration
   - Entry/exit point calculation

3. **U-Turn Service** - Automated turn pattern generation
   - Multiple turn styles (Question Mark, Semi-Circle, Keyhole)
   - Dubins path algorithm for minimum radius paths
   - Turn trigger detection
   - Path following during turns

**Technology Stack:**
- .NET 8, Avalonia UI, MVVM architecture
- Service-based architecture with dependency injection
- Event-driven patterns for state changes

Please initialize the spec folder and save the raw idea.

Return the spec folder path when complete.

## Metadata
- Date Created: 2025-10-19
- Spec Name: wave-5-field-operations
- Spec Path: agent-os/specs/2025-10-19-wave-5-field-operations
