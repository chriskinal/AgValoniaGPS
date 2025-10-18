# Initial Spec Idea

## User's Initial Description
**Feature Name**: Wave 2 - Guidance Line Core

**Description**: Implementation of the core guidance line services for agricultural precision guidance. This includes three main services:

1. **ABLineService** - Straight-line guidance system
   - Create AB lines from two points or heading
   - Calculate cross-track error
   - Generate parallel lines
   - Line nudging/offset operations

2. **CurveLineService** - Curved path guidance system
   - Record and follow curved paths
   - Curve smoothing (cubic spline interpolation)
   - Cross-track error for curves
   - Parallel curve generation

3. **ContourService** - Contour following system
   - Real-time contour recording
   - Offset calculation from recorded contours
   - Contour locking and guidance updates

**Dependencies**: Wave 1 (PositionUpdateService, HeadingCalculatorService, VehicleKinematicsService)

**Complexity**: HIGH

**Estimated LOC**: ~2,000

Please initialize the spec folder structure and save this initial description.

## Metadata
- Date Created: 2025-10-17
- Spec Name: wave-2-guidance-line-core
- Spec Path: agent-os/specs/2025-10-17-wave-2-guidance-line-core
