# VM to PC Handoff Document - AgValoniaGPS Development

**Date:** 2025-10-18
**Current Branch:** `develop`
**Latest Commit:** `1e7dde3e` - Complete Wave 2: Guidance Line Core
**Purpose:** Moving from VM to real PC for Android test environment

---

## 🎯 Project Status Overview

### ✅ Completed Waves

**Wave 0: Foundation** (Already existed)
- NMEA parsing
- UDP communication
- Field file I/O
- NTRIP client
- Basic UI framework

**Wave 1: Position & Kinematics** ✅ COMPLETE
- `PositionUpdateService` - GPS position processing and filtering
- `VehicleKinematicsService` - Pivot, steer axle, tool position calculations
- Test coverage: Comprehensive
- Location: `AgValoniaGPS.Services/GPS/` and `AgValoniaGPS.Services/Vehicle/`

**Wave 2: Guidance Line Core** ✅ COMPLETE (Just finished!)
- `ABLineService` - Create AB lines, calculate XTE, nudge, parallel lines (13 tests)
- `CurveLineService` - Record curves, smooth paths, guidance calculations (7 tests)
- `ContourService` - Record contours, lock for guidance, dual-point algorithm (8 tests)
- File I/O services with JSON + AgOpenGPS backward compatibility (6 tests)
- **Test Results:** 34/34 passing (100%)
- Location: `AgValoniaGPS.Services/Guidance/`

### 🔄 Current Wave

**Wave 3: Steering Algorithms** 🎯 READY TO IMPLEMENT
- Spec created and verified
- Tasks broken down (21 tasks, 5 groups, ~12-14 hours)
- Location: `agent-os/specs/2025-10-18-wave-3-steering-algorithms/`

---

## 📁 Repository Structure

```
AgValoniaGPS/
├── SourceCode/AgOpenGPS/          # Legacy .NET Framework 4.8 (reference only)
├── AgValoniaGPS/                  # Modern .NET 8 + Avalonia rewrite
│   ├── AgValoniaGPS.Models/       # Domain models
│   │   ├── Guidance/              # ABLine, CurveLine, ContourLine models
│   │   └── Events/                # EventArgs for service events
│   ├── AgValoniaGPS.Services/     # Business logic services
│   │   ├── GPS/                   # PositionUpdateService (renamed from Position/)
│   │   ├── Guidance/              # ABLine, Curve, Contour services + file I/O
│   │   └── Vehicle/               # VehicleKinematicsService
│   ├── AgValoniaGPS.Services.Tests/ # Unit tests (xUnit + NUnit)
│   ├── AgValoniaGPS.ViewModels/   # MVVM ViewModels
│   ├── AgValoniaGPS.Desktop/      # Avalonia desktop app
│   └── AgValoniaGPS.Core/         # Core application logic
├── NAMING_CONVENTIONS.md          # ⚠️ READ BEFORE CREATING DIRECTORIES
├── CLAUDE.md                      # Project guide for Claude Code
└── agent-os/                      # Spec-driven development
    ├── product/                   # Product roadmaps
    └── specs/                     # Implementation specs
        ├── 2025-10-17-wave-2-guidance-line-core/  ✅ Complete
        └── 2025-10-18-wave-3-steering-algorithms/ 📋 Ready
```

---

## 🔧 Development Environment Setup

### Prerequisites
- .NET 8 SDK
- Git
- Visual Studio Code or Visual Studio 2022
- Android SDK (for Android testing on real PC)

### Initial Setup Commands

```bash
# Clone repository
git clone https://github.com/chriskinal/AgValoniaGPS.git
cd AgValoniaGPS

# Checkout develop branch
git checkout develop

# Verify you're on the right commit
git log --oneline -1
# Should show: 1e7dde3e Complete Wave 2: Guidance Line Core...

# Restore dependencies
dotnet restore AgValoniaGPS/AgValoniaGPS.sln

# Build solution
dotnet build AgValoniaGPS/AgValoniaGPS.sln

# Run tests to verify everything works
dotnet test AgValoniaGPS/AgValoniaGPS.Services.Tests/
# Expected: 34/34 tests passing
```

### Verify Installation

```bash
# Should build with 0 errors, 0 warnings
dotnet build AgValoniaGPS/AgValoniaGPS.sln --no-restore

# Should launch the app (will timeout after 10 seconds)
timeout 10 dotnet run --project AgValoniaGPS/AgValoniaGPS.Desktop/ --no-build
```

---

## 🚨 Critical Issues Fixed (Don't Repeat!)

### Issue #1: Namespace Collision (Wave 2)
**Problem:** Created `AgValoniaGPS.Services/Position/` directory which conflicted with `Position` class from `AgValoniaGPS.Models.Position`

**Solution:**
- Renamed directory from `Position/` to `GPS/`
- Updated all namespaces and using directives
- Created `NAMING_CONVENTIONS.md` to prevent future collisions

**Prevention:** ⚠️ **ALWAYS check `NAMING_CONVENTIONS.md` before creating new directories!**

### Issue #2: JSON Serialization (Wave 2)
**Problem:** `Name` property became empty after save/load round-trip

**Solution:**
- Save methods used `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- Load methods forgot to specify the same policy
- Added `JsonSerializerOptions` to all Load methods

**Prevention:** Always use consistent serialization options for save AND load

### Issue #3: Cross-Track Error Sign Convention (Wave 2)
**Problem:** XTE calculation had inverted sign (right/left convention)

**Solution:**
- Fixed cross product formula in `ABLineService.cs` line 338
- Changed from `(lineE * deltaN - lineN * deltaE)` to `(lineN * deltaE - lineE * deltaN)`

**Prevention:** Write tests with known expected values first

---

## 📚 Essential Documentation

### Must-Read Before Development

1. **`NAMING_CONVENTIONS.md`** ⚠️ CRITICAL
   - Prevents namespace collisions
   - Lists reserved class names that cannot be used as directory names
   - Shows approved directory structure
   - Read this BEFORE creating any new services or directories!

2. **`CLAUDE.md`**
   - Project overview and architecture
   - Build commands
   - Testing guidelines
   - AgValoniaGPS-specific development rules

3. **`agent-os/product/feature-extraction-roadmap.md`**
   - Complete wave breakdown (Waves 0-8)
   - Extraction principles
   - Dependencies between waves

### Wave 3 Specification Files

- **Spec:** `agent-os/specs/2025-10-18-wave-3-steering-algorithms/spec.md`
- **Tasks:** `agent-os/specs/2025-10-18-wave-3-steering-algorithms/tasks.md`
- **Verification:** `agent-os/specs/2025-10-18-wave-3-steering-algorithms/verification/spec-verification.md`

---

## 🎯 Wave 3: Next Steps

### What You're Implementing

**Four Services for Steering Algorithms:**

1. **LookAheadDistanceService** (2-3 hours)
   - Adaptive look-ahead distance based on speed, XTE, curvature, vehicle type
   - Independent, can start first

2. **StanleySteeringService** (3-4 hours)
   - Stanley algorithm: `steerAngle = headingError * k1 + atan(k2 * XTE / speed)`
   - Can develop in parallel with Pure Pursuit

3. **PurePursuitService** (3-4 hours)
   - Pure Pursuit algorithm with goal point calculation
   - Can develop in parallel with Stanley

4. **SteeringCoordinatorService** (3-4 hours)
   - Algorithm selection and switching
   - PGN message output via UDP
   - Depends on services 1-3

5. **Integration Testing** (3-4 hours)
   - Performance validation (100Hz capability)
   - Edge cases: tight curves, U-turns, GPS loss
   - Multi-vehicle testing

### Performance Requirements

- **100Hz target** (10ms max per iteration)
- LookAheadDistanceService: <1ms
- StanleySteeringService: <2ms
- PurePursuitService: <3ms
- SteeringCoordinatorService: <5ms total

### Key Constraints

✅ Services in flat `AgValoniaGPS.Services/Guidance/` directory (no `Steering/` subdirectory)
✅ Real-time algorithm switching supported
✅ All tuning parameters exposed for UI configuration
✅ Send steering commands via PGN over UDP

---

## 🧪 Testing Strategy

### Current Test Status (Wave 2)

```bash
# Run all guidance tests
dotnet test AgValoniaGPS/AgValoniaGPS.Services.Tests/ --filter "FullyQualifiedName~Guidance"

# Expected output:
# Total tests: 34
# Passed: 34 (100%)
# Time: ~2-3 seconds
```

### Test Organization

```
AgValoniaGPS.Services.Tests/
├── GPS/
│   └── PositionUpdateServiceTests.cs
├── Vehicle/
│   └── VehicleKinematicsServiceTests.cs
├── Guidance/
│   ├── ABLineServiceTests.cs           (8 tests)
│   ├── ABLineAdvancedTests.cs          (5 tests)
│   ├── CurveLineServiceTests.cs        (7 tests)
│   └── ContourServiceTests.cs          (8 tests)
└── Field/
    └── FieldServiceGuidanceLineTests.cs (6 tests)
```

### Wave 3 Test Plan

- **Task Groups 1-4:** 4-7 focused tests each (~28-36 total)
- **Task Group 5:** Max 10 integration tests
- **Performance Tests:** Benchmark 100Hz capability
- **Edge Cases:** Tight curves, U-turns, vehicle variations

---

## 🔗 Git Workflow

### Current State

```bash
# Branch: develop
# Latest commit: 1e7dde3e
# Status: Clean working directory
# Remote: origin/develop (pushed)
```

### After Cloning on New PC

```bash
# Verify branch
git branch
# Should show: * develop

# Verify commit
git log --oneline -1
# Should show: 1e7dde3e Complete Wave 2: Guidance Line Core...

# Check status
git status
# Should show: On branch develop, Your branch is up to date with 'origin/develop'
```

### Development Workflow

```bash
# Before starting work
git pull origin develop

# After completing a task group
git add AgValoniaGPS/
git commit -m "Implement [TaskGroupName]: [Description]"

# Push frequently
git push origin develop
```

---

## 🏗️ Architecture Patterns

### Service Pattern (All Waves)

```csharp
// 1. Define interface in same file/directory as implementation
public interface IMyService
{
    event EventHandler<MyEventArgs>? MyEvent;
    void DoSomething();
}

// 2. Implement service
public class MyService : IMyService
{
    public event EventHandler<MyEventArgs>? MyEvent;

    public void DoSomething()
    {
        // Implementation
        MyEvent?.Invoke(this, new MyEventArgs(...));
    }
}

// 3. Register in ServiceCollectionExtensions.cs
services.AddSingleton<IMyService, MyService>();
```

### Event Pattern (Wave 2)

```csharp
// EventArgs in AgValoniaGPS.Models/Events/
public class MyEventArgs : EventArgs
{
    public MyEventArgs(string data) => Data = data;
    public string Data { get; }
}

// Raise events from services
MyEvent?.Invoke(this, new MyEventArgs("data"));
```

### File I/O Pattern (Wave 2)

```csharp
// Save with camelCase
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var json = JsonSerializer.Serialize(obj, options);

// Load with SAME options
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var obj = JsonSerializer.Deserialize<T>(json, options);
```

---

## 🐛 Common Pitfalls

### ❌ DON'T

1. **Create directories named after Models classes**
   - ❌ `Position/` (conflicts with Position class)
   - ❌ `Vehicle/` as new directory (already exists)
   - ❌ `Field/` (conflicts with Field class)
   - ✅ Check `NAMING_CONVENTIONS.md` first!

2. **Forget serialization options on Load**
   - ❌ `JsonSerializer.Deserialize<T>(json)` (missing options)
   - ✅ `JsonSerializer.Deserialize<T>(json, options)` (with camelCase)

3. **Use generic service names**
   - ❌ `PositionService` (too vague)
   - ✅ `PositionUpdateService` (specific purpose)

4. **Create subdirectories in Services**
   - ❌ `Services/Guidance/Steering/` (adds extra namespace level)
   - ✅ `Services/Guidance/` (flat structure)

### ✅ DO

1. **Read NAMING_CONVENTIONS.md before creating directories**
2. **Use consistent JsonSerializerOptions for save AND load**
3. **Write tests BEFORE implementation (TDD approach)**
4. **Follow existing patterns from Wave 1 and Wave 2**
5. **Run tests frequently during development**
6. **Check performance with benchmarks**

---

## 📊 Project Metrics

### Current Codebase Size

- **Services:** ~15 service classes
- **Models:** ~30 model classes
- **Tests:** 34 unit tests (Wave 2), additional tests from Wave 1
- **LOC Added (Wave 2):** +11,847 lines
- **Test Coverage:** >90% for guidance services

### Wave 3 Estimates

- **Effort:** 12-14 hours (with parallel execution)
- **Services to Add:** 4 new services
- **Tests to Add:** ~28-36 tests
- **LOC Estimate:** ~1,500 lines

---

## 🚀 Quick Start Guide for New PC

### 1. Clone and Verify

```bash
git clone https://github.com/chriskinal/AgValoniaGPS.git
cd AgValoniaGPS
git checkout develop
git log --oneline -1  # Verify: 1e7dde3e
```

### 2. Build and Test

```bash
dotnet restore AgValoniaGPS/AgValoniaGPS.sln
dotnet build AgValoniaGPS/AgValoniaGPS.sln
dotnet test AgValoniaGPS/AgValoniaGPS.Services.Tests/
```

### 3. Read Essential Docs

```bash
# Critical - prevents namespace collisions
cat NAMING_CONVENTIONS.md

# Project overview
cat CLAUDE.md

# Wave 3 spec
cat agent-os/specs/2025-10-18-wave-3-steering-algorithms/spec.md
cat agent-os/specs/2025-10-18-wave-3-steering-algorithms/tasks.md
```

### 4. Start Wave 3 Implementation

Follow the tasks in order from `tasks.md`:
1. Task Group 1: LookAheadDistanceService (independent)
2. Task Groups 2 & 3: Stanley and Pure Pursuit (parallel)
3. Task Group 4: SteeringCoordinatorService (depends on 1-3)
4. Task Group 5: Integration testing and performance validation

---

## 🎮 Android Testing (New PC)

### Why Moving to Real PC

- **VM Limitation:** Cannot run Android emulator or test on Android devices
- **Goal:** Test Avalonia cross-platform capabilities on Android
- **AgValoniaGPS:** Built with Avalonia for cross-platform (Windows, Linux, macOS, Android, iOS)

### Android Setup (To Do)

1. Install Android SDK
2. Configure Avalonia Android project (if not exists)
3. Test guidance services on Android device
4. Validate performance on mobile hardware

---

## 📝 Implementation Checklist for Wave 3

Before starting:
- [ ] Clone repo on new PC
- [ ] Verify develop branch (commit 1e7dde3e)
- [ ] Build solution successfully
- [ ] Run tests (34/34 passing)
- [ ] Read `NAMING_CONVENTIONS.md`
- [ ] Read Wave 3 spec and tasks
- [ ] Setup Android SDK (for testing)

During implementation:
- [ ] Follow flat directory structure (`Services/Guidance/`)
- [ ] Write tests BEFORE implementation
- [ ] Run tests after each task
- [ ] Check performance benchmarks
- [ ] Commit after each task group
- [ ] Push frequently to origin

After Wave 3:
- [ ] All 34 new tests passing
- [ ] Performance: <10ms per iteration (100Hz)
- [ ] Integration tests passing
- [ ] App runs on Windows (desktop)
- [ ] App runs on Android (mobile)

---

## 📞 Key Contacts & Resources

- **Repository:** https://github.com/chriskinal/AgValoniaGPS
- **Branch:** `develop`
- **Claude Code:** AI assistant for development
- **Original AgOpenGPS:** https://github.com/farmerbriantee/AgOpenGPS (reference)

---

## 🎯 Success Criteria

### Wave 3 Complete When:

✅ All 4 steering services implemented
✅ ~28-36 tests passing (100%)
✅ Performance: <10ms per iteration (100Hz capable)
✅ Real-time algorithm switching works
✅ All tuning parameters exposed
✅ PGN messages sent via UDP
✅ Edge cases tested (tight curves, U-turns, etc.)
✅ Application runs on desktop
✅ Application runs on Android

---

## 💡 Final Notes

**From VM Development:**
- Wave 1 and Wave 2 completed successfully
- Namespace collision issue identified and fixed
- Comprehensive testing strategy established
- Spec-driven development process validated

**For PC Development:**
- Continue spec-driven approach
- Use parallel task execution where possible
- Test on Android to validate cross-platform
- Maintain 100% test pass rate

**Remember:**
- Check `NAMING_CONVENTIONS.md` BEFORE creating directories
- Write tests BEFORE implementation
- Run tests FREQUENTLY during development
- Commit and push OFTEN

---

**Good luck with Wave 3 implementation on the real PC! 🚀**

Last VM session: 2025-10-18
Next session: Real PC with Android testing
Current status: Wave 2 complete, Wave 3 spec ready
