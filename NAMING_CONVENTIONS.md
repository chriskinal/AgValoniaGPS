# Naming Conventions for AgValoniaGPS

This document establishes naming conventions to prevent namespace collisions and maintain consistency across the codebase, especially when delegating parallel tasks to multiple agents.

## Critical Rules to Prevent Namespace Collisions

### 1. Directory Names vs Class Names
**DO NOT** name directories after existing class names in `AgValoniaGPS.Models`.

**Reserved Class Names (Cannot be used as directory names):**
- `Position` - Use `GPS` instead for position-related services
- `Vehicle` - Already exists as directory, contains vehicle kinematics services
- `Field` - Do not create `Field/` directory
- `Guidance` - Already exists as directory for guidance services
- `Boundary` - Do not create `Boundary/` directory
- `GpsData` - Use `GPS` instead
- `ImuData` - Use `Sensors` or `GPS` instead

### 2. Service Directory Organization

Services should be organized by **functional area**, not by the domain objects they manipulate.

**Existing Service Directories:**
```
AgValoniaGPS.Services/
├── GPS/                    # GPS and positioning services
├── Guidance/              # Guidance line services (ABLine, Curve, Contour)
├── Vehicle/               # Vehicle kinematics and configuration
└── Interfaces/            # Legacy interfaces (avoid adding new files here)
```

**Approved Directory Names for Future Use:**
- `Field/` - Field management services
- `Mapping/` - Mapping and boundary services
- `Sensors/` - Sensor data processing (IMU, etc.)
- `Communication/` - Network and UDP communication
- `FileIO/` - File I/O services
- `Display/` - Display and visualization services
- `Section/` - Section control services
- `Navigation/` - Route and path planning

### 3. Namespace Naming Pattern

All services follow this pattern:
```
AgValoniaGPS.Services.{FunctionalArea}
```

Examples:
- ✅ `AgValoniaGPS.Services.GPS.PositionUpdateService`
- ✅ `AgValoniaGPS.Services.Guidance.ABLineService`
- ✅ `AgValoniaGPS.Services.Vehicle.VehicleKinematicsService`
- ❌ `AgValoniaGPS.Services.Position.PositionService` (conflicts with `Position` class)

### 4. Service Class Naming

Service classes should be descriptive and end with `Service`:
```
{Functionality}Service
```

Examples:
- ✅ `PositionUpdateService` (provides position updates)
- ✅ `VehicleKinematicsService` (calculates vehicle kinematics)
- ✅ `ABLineService` (manages AB line guidance)
- ❌ `PositionService` (too generic)
- ❌ `VehicleService` (too generic)

### 5. Interface Naming

Interfaces should mirror their implementation:
```
I{ServiceName}
```

Examples:
- `IPositionUpdateService`
- `IVehicleKinematicsService`
- `IABLineService`

### 6. File Service Naming

File I/O services should clearly indicate their purpose:
```
{EntityType}FileService
```

Examples:
- `ABLineFileService`
- `CurveLineFileService`
- `FieldFileService`

## Common Patterns

### Service Registration Pattern
All services register in `ServiceCollectionExtensions.cs` using:
```csharp
services.AddSingleton<IServiceName, ServiceName>();
```

### Event Naming Pattern
Events should follow the pattern:
```csharp
public event EventHandler<{Entity}ChangedEventArgs>? {Entity}Changed;
```

Examples:
- `ABLineChanged` with `ABLineChangedEventArgs`
- `CurveChanged` with `CurveLineChangedEventArgs`

### Change Type Enum Naming
Change type enums should be specific:
```csharp
{Entity}ChangeType
```

Examples:
- ✅ `ABLineChangeType`
- ✅ `CurveLineChangeType`
- ❌ `ChangeType` (too generic)

## Quick Reference: What NOT to Name Directories

Based on existing Models classes, **NEVER** create directories with these names:
- Position
- Vehicle (already exists)
- Field
- Boundary
- GpsData
- ImuData
- ABLine
- CurveLine
- ContourLine
- GeoCoord
- ValidationResult

## Pre-Implementation Checklist

Before creating new services in parallel tasks:

1. ✅ Check this document for reserved names
2. ✅ Choose functional area name (GPS, Guidance, Vehicle, etc.)
3. ✅ Verify directory name doesn't conflict with Models classes
4. ✅ Use descriptive service names ending in "Service"
5. ✅ Follow namespace pattern: `AgValoniaGPS.Services.{FunctionalArea}`

## Examples of Correct Organization

### GPS Services
```
AgValoniaGPS.Services/GPS/
├── PositionUpdateService.cs      (namespace: AgValoniaGPS.Services.GPS)
├── IPositionUpdateService.cs
└── GpsFilterService.cs
```

### Guidance Services
```
AgValoniaGPS.Services/Guidance/
├── ABLineService.cs              (namespace: AgValoniaGPS.Services.Guidance)
├── CurveLineService.cs
├── ContourService.cs
├── ABLineFileService.cs
└── Interfaces/
    ├── IABLineService.cs
    ├── ICurveLineService.cs
    └── IContourService.cs
```

### Vehicle Services
```
AgValoniaGPS.Services/Vehicle/
├── VehicleKinematicsService.cs   (namespace: AgValoniaGPS.Services.Vehicle)
├── IVehicleKinematicsService.cs
└── VehicleConfigurationService.cs
```

## Conflict Resolution

If a namespace collision is discovered:

1. Rename the directory to a functional area name
2. Update all namespace declarations in that directory
3. Update all `using` directives referencing the old namespace
4. Update dependency injection registrations

Example from Wave 2:
- **Problem**: `AgValoniaGPS.Services.Position/` conflicted with `Position` class
- **Solution**: Renamed to `AgValoniaGPS.Services.GPS/`
- **Files Updated**: 6 files (namespace declarations, using directives, DI registration)

---

Last Updated: 2025-10-18
Wave: Post-Wave 2 Cleanup
