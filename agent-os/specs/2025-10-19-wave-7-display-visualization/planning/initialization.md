# Initial Spec Idea

## User's Initial Description
Wave 7 focuses on extracting display and visualization business logic from the legacy AgOpenGPS WinForms application into clean, testable services for AgValoniaGPS.

### Key Features to Extract:

1. **Field Statistics Calculator** - Calculate and format field statistics (area, distance, time remaining)
2. **Display Formatters** - Format data for UI display (speed, heading, GPS quality, steering angle)

### Context

This is part of a systematic extraction roadmap for AgValoniaGPS. Previous waves (0-6) have completed:
- Wave 0: Foundation (already complete)
- Wave 1: Position & Kinematics
- Wave 2: Guidance Line Core
- Wave 3: Steering Algorithms
- Wave 4: Section Control
- Wave 5: Field Operations
- Wave 6: Hardware I/O & Communication

Wave 7 is LOW complexity focusing on data presentation rather than computation.

### Dependencies
- All previous waves (1-6) for data to format and display
- Existing FieldStatisticsService (to be expanded)

### Success Criteria
- ~1,000 lines of business logic extracted
- All formatters are testable and UI-agnostic
- Support for multiple unit systems (metric/imperial)
- Color coding for quality indicators
- Performance: <1ms for all formatting operations

## Metadata
- Date Created: 2025-10-19
- Spec Name: wave-7-display-visualization
- Spec Path: agent-os/specs/2025-10-19-wave-7-display-visualization
