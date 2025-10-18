# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Structure

This repository contains TWO separate projects:

1. **SourceCode/AgOpenGPS** - Legacy .NET Framework 4.8 Windows Forms application
2. **AgValoniaGPS** - Modern .NET 8 Avalonia cross-platform rewrite

**IMPORTANT**: When working on AgValoniaGPS project:
- **ALWAYS** check `NAMING_CONVENTIONS.md` before creating new directories or services
- **NEVER** name directories after class names in `AgValoniaGPS.Models`
- This prevents namespace collisions when running parallel task delegation

## Legacy Project: AgOpenGPS (.NET Framework 4.8)

AgOpenGPS is a precision agriculture guidance software written in C# (.NET Framework 4.8) that provides GPS guidance, field mapping, and section control for agricultural equipment. The project consists of two main applications: AgIO (communication hub) and AgOpenGPS (main application).

## Build Commands

```bash
# Restore dependencies
dotnet restore --runtime win-x64 ./SourceCode/AgOpenGPS.sln

# Build solution
dotnet build --no-restore ./SourceCode/AgOpenGPS.sln

# Run all tests
dotnet test --no-restore --no-build ./SourceCode/AgOpenGPS.sln

# Run specific test project
dotnet test ./SourceCode/AgLibrary.Tests/AgLibrary.Tests.csproj
dotnet test ./SourceCode/AgOpenGPS.Core.Tests/AgOpenGPS.Core.Tests.csproj

# Publish (creates AgOpenGPS folder with all applications)
dotnet publish ./SourceCode/AgOpenGPS.sln
```

## Architecture

### Core Structure
- **MVP Pattern**: AgOpenGPS.Core implements Model-View-Presenter pattern with dependency injection
- **ApplicationCore**: Main composition root at `AgOpenGPS.Core/ApplicationCore.cs`
- **Separation of Concerns**: Models, ViewModels, Presenters, and Interfaces are cleanly separated

### Key Components
- **GPS/**: Main Windows Forms application with OpenGL graphics rendering
- **AgIO/**: Communication hub for hardware interfaces
- **AgOpenGPS.Core/**: Business logic library using MVP pattern
- **AgLibrary/**: Shared utilities and settings management
- **AgOpenGPS.WpfApp/**: WPF version of the application

### Main Entry Points
- GPS Application: `GPS/Program.cs`
- Core Logic: `AgOpenGPS.Core/ApplicationCore.cs`
- Settings: `AgLibrary/Settings/` namespace

## Development Workflow

### Version Control
- **Main development branch**: `develop` (submit PRs here)
- **Stable branch**: `master`
- **Version management**: GitVersion handles semantic versioning automatically
- **Version file**: Manual patch increments in `./sys/version.h` when fixing bugs

### Testing
- **Framework**: NUnit 4.3.2
- **Test pattern**: AAA (Arrange, Act, Assert) with Assert.That syntax
- **Test projects**: AgLibrary.Tests, AgOpenGPS.Core.Tests

### Key Technologies
- .NET Framework 4.8 (Windows-only)
- Windows Forms (main UI) and WPF (newer components)
- OpenTK.GLControl for OpenGL graphics
- SQLite for data storage
- NMEA protocol for GPS communication

## Common Development Tasks

### Running the Application
1. Set GPS project as startup project in Visual Studio
2. Build and run (F5)

### Adding New Features
- Business logic goes in AgOpenGPS.Core
- Shared utilities in AgLibrary
- UI components in GPS (Windows Forms) or AgOpenGPS.WpfApp (WPF)

### Debugging Hardware Communication
- AgIO handles all hardware communication
- Check AgIO logs for connection issues
- ModSim project provides hardware simulation

### Working with Translations
- Uses Weblate for internationalization
- Resource files (.resx) contain UI strings
- Located in each project's Properties folder

---

## Modern Project: AgValoniaGPS (.NET 8 + Avalonia)

AgValoniaGPS is a complete rewrite using modern .NET 8 and Avalonia UI for cross-platform support.

### Build Commands (AgValoniaGPS)

```bash
# Build solution
dotnet build AgValoniaGPS/AgValoniaGPS.sln

# Run tests
dotnet test AgValoniaGPS/AgValoniaGPS.Services.Tests/

# Run application
dotnet run --project AgValoniaGPS/AgValoniaGPS.Desktop/
```

### Architecture (AgValoniaGPS)

**Project Structure:**
```
AgValoniaGPS/
├── AgValoniaGPS.Models/          # Domain models, events, enums
├── AgValoniaGPS.Services/        # Business logic services
│   ├── GPS/                      # Position and GPS services
│   ├── Guidance/                 # Guidance line services
│   └── Vehicle/                  # Vehicle kinematics
├── AgValoniaGPS.ViewModels/      # MVVM view models
├── AgValoniaGPS.Desktop/         # Avalonia desktop application
├── AgValoniaGPS.Services.Tests/  # Unit tests (xUnit + NUnit)
└── AgValoniaGPS.Core/            # Core application logic
```

**Design Patterns:**
- **MVVM**: Model-View-ViewModel pattern with Avalonia
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Service Layer**: Services registered in `ServiceCollectionExtensions.cs`
- **Event-Driven**: EventArgs pattern for service state changes

### Critical Development Rules (AgValoniaGPS)

⚠️ **BEFORE creating any new services or directories:**

1. **Read `NAMING_CONVENTIONS.md`** - Contains reserved names and patterns
2. **Check for namespace collisions** - Directory names must not match class names in Models
3. **Use functional area names** - Organize by purpose (GPS, Guidance), not by domain objects
4. **Follow service patterns** - All services end with "Service" suffix

**Common Issues to Avoid:**
- ❌ Creating `Position/` directory (conflicts with `Position` class)
- ❌ Using generic names like `PositionService` (use `PositionUpdateService`)
- ❌ Organizing by domain objects instead of functional areas

### Service Organization (AgValoniaGPS)

Services are organized by **functional area**:

- **GPS/** - Position updates, GPS filtering
- **Guidance/** - AB lines, curves, contours, file I/O
- **Vehicle/** - Kinematics, configuration, steering

**Adding New Services:**
1. Choose functional area (or create new one per conventions)
2. Implement service class with interface
3. Register in `ServiceCollectionExtensions.cs`
4. Add unit tests in corresponding test directory

### Testing (AgValoniaGPS)

- **Frameworks**: xUnit and NUnit (mixed - being standardized)
- **Pattern**: AAA (Arrange, Act, Assert)
- **Coverage**: All services must have comprehensive tests
- **Performance**: Guidance calculations must meet <5ms requirements

### Dependency Injection (AgValoniaGPS)

Services are registered in `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
public static IServiceCollection AddAgValoniaGpsServices(this IServiceCollection services)
{
    // Position Services
    services.AddSingleton<IPositionUpdateService, PositionUpdateService>();

    // Guidance Services
    services.AddSingleton<IABLineService, ABLineService>();
    services.AddSingleton<ICurveLineService, CurveLineService>();

    // Vehicle Services
    services.AddSingleton<IVehicleKinematicsService, VehicleKinematicsService>();

    return services;
}
```

### Spec-Driven Development

AgValoniaGPS follows a spec-driven development process:

1. Specs are created in `agent-os/specs/` directory
2. Each spec includes detailed requirements, test scenarios, and acceptance criteria
3. Implementation is delegated to specialized agents in parallel
4. Verification ensures 100% test pass rate before completion

**Wave Progress:**
- ✅ Wave 1: Position & Kinematics Services
- ✅ Wave 2: Guidance Line Core (ABLine, Curve, Contour)
- 🔄 Wave 3+: See `agent-os/specs/` for upcoming features