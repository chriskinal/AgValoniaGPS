# AgValoniaGPS Technology Stack

## Implementation Status
This document reflects both the current implementation and planned technologies for AgValoniaGPS.

## Core Platform (✅ Implemented)

### Runtime & Language
- **Language**: C# 12
- **Runtime**: .NET 8.0 (LTS)
  - *Status*: ✅ Fully implemented
  - *Rationale*: Cross-platform support, modern C# features, long-term support, excellent performance

### UI Framework
- **Primary**: Avalonia UI 11.3.6
  - *Status*: ✅ Fully implemented
  - *Rationale*: True cross-platform (Windows, Linux, macOS), XAML-based, hardware acceleration
  - *Current Usage*: Main window, dialogs, reactive bindings

### Graphics Rendering
- **3D Graphics**: Silk.NET.OpenGL 2.22.0
  - *Status*: ✅ Fully implemented
  - *Features*: 2D/3D map rendering, grid display, vehicle sprites, boundary visualization
  - *Shaders*: OpenGL ES 3.0 compatible vertex/fragment shaders
  - *Performance*: 60 FPS render loop achieved

## Architecture Patterns (✅ Implemented)

### Application Architecture
- **Pattern**: MVVM with ReactiveUI
  - *Status*: ✅ Fully implemented
  - *Implementation*: ReactiveUI 20.1.1 for reactive properties and commands
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection 8.0.0
  - *Status*: ✅ Fully implemented
  - *Usage*: Service registration in ServiceCollectionExtensions.cs
- **Hosting**: Microsoft.Extensions.Hosting 8.0.0
  - *Status*: ✅ Fully implemented

### Code Organization
- **Structure**: Multi-project solution with clear separation
  - *Status*: ✅ Fully implemented
  - AgValoniaGPS.Desktop (UI layer)
  - AgValoniaGPS.ViewModels (Presentation logic)
  - AgValoniaGPS.Services (Business logic)
  - AgValoniaGPS.Models (Data structures)
  - AgValoniaGPS.Core (Core framework - placeholder)
- **Principles**: SOLID, DRY, clean architecture
  - *Status*: ✅ Following in current implementation

## Data Management

### Local Storage
- **Field Files**: Text-based AgOpenGPS format
  - *Status*: ✅ Fully implemented
  - *Files*: Field.txt, Boundary.txt, Headland.Txt, BackPic.Txt
  - *Services*: FieldService, BoundaryFileService, FieldPlaneFileService
- **Database**: SQLite planned
  - *Status*: ⏳ Not yet implemented
  - *Future*: For settings persistence and data logging

### Configuration
- **Settings**: In-memory configuration
  - *Status*: ⚠️ Partially implemented
  - *Current*: Vehicle configuration in DI container
  - *Planned*: JSON configuration files with Microsoft.Extensions.Configuration

### Cloud Integration
- **AgShare API**: RESTful HTTP client
  - *Status*: ⏳ Planned for Phase 1
  - **HTTP Client**: HttpClient with resilience patterns
  - **Authentication**: API key and JWT tokens
  - *Integration Point*: FieldService will coordinate uploads

## Communication & Hardware (✅ Implemented)

### AgIO Integration (UDP-based)
- **Status**: ✅ Fully implemented (Serial excluded by design)
- **Port**: 9999 (receive), broadcasts to 192.168.5.255:8888
- **Modules Supported**:
  - AutoSteer module
  - Machine control module
  - IMU module
  - GPS data
- **Protocol**: PGN binary messages with CRC
- **Monitoring**: Hello packets (2s timeout), Data flow (100-300ms timeout)

### GPS Communication
- **NMEA Parsing**: ✅ Implemented
  - *Formats*: $PANDA, $PAOGI with IMU data
  - *Service*: NmeaParserService.cs
- **Network**: UDP via System.Net.Sockets
  - *Status*: ✅ Fully implemented

### NTRIP/RTK Support
- **Status**: ✅ Fully implemented
- **Service**: NtripClientService.cs
- **Features**:
  - ICY and HTTP/1.0 protocol support
  - RTCM3 forwarding to GPS (UDP port 2233)
  - GGA sentence generation
  - Queue management with overflow protection

### Hardware Abstraction
- **Pattern**: Interface-based services
  - *Status*: ✅ Implemented
  - *Examples*: IFieldService, IGuidanceService, IGpsService
- **Testing**: UDP-based modules eliminate USB issues
  - *Status*: ✅ Working with Teensy-based hardware

## Development Tools

### Version Control
- **VCS**: Git with GitHub
- **Branching**: GitFlow (main, develop, feature/*, release/*)
- **Versioning**: GitVersion for semantic versioning
- *Rationale*: Industry standard, good CI/CD integration

### Build & CI/CD
- **Build**: MSBuild via `dotnet` CLI
- **CI/CD**: GitHub Actions
  - Build matrix for Windows, Linux, macOS
  - Automated testing on all platforms
  - Release automation
- *Rationale*: Native GitHub integration, free for open source

### Testing
- **Unit Testing**: xUnit.net
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- **Coverage**: Coverlet with ReportGenerator
- **Integration**: TestContainers for database tests
- *Rationale*: Modern testing stack, good IDE support, expressive assertions

### Code Quality
- **Linting**: .editorconfig with Roslyn analyzers
- **Formatting**: dotnet format
- **Security**: GitHub Dependabot
- **Code Analysis**: SonarCloud (free for open source)
- *Rationale*: Automated quality gates, security scanning

## Deployment

### Desktop Packaging
- **Windows**:
  - MSIX installer via Windows App SDK
  - Portable ZIP archive
- **Linux**:
  - AppImage (universal)
  - Snap package (Ubuntu)
  - Flatpak (Fedora/others)
  - .deb/.rpm packages
- **macOS**:
  - .app bundle
  - DMG installer
  - Homebrew cask
- *Rationale*: Native packaging for each platform, user choice

### Updates
- **Mechanism**: In-app update notifications
- **Delivery**: GitHub Releases API
- **Auto-update**: Optional, user-controlled
- *Rationale*: Simple, reliable, no additional infrastructure

## Third-Party Libraries

### Core Dependencies (✅ Implemented)
- **Avalonia UI**: 11.3.6 - UI framework ✅
- **ReactiveUI**: 20.1.1 - MVVM reactive framework ✅
- **Silk.NET.OpenGL**: 2.22.0 - OpenGL graphics ✅
- **StbImageSharp**: 2.30.15 - Image loading ✅
- **System.IO.Ports**: 8.0.0 - Serial port support (available but unused) ✅

### Planned Dependencies
- **Serilog**: Structured logging ⏳
- **Polly**: Resilience patterns for AgShare ⏳
- **System.Text.Json**: JSON serialization ⏳
- **NetTopologySuite**: Advanced geometry operations ⏳

### Development Dependencies
- **Testing**: No test projects yet implemented ⏳
- **Planned**: xUnit, NSubstitute, FluentAssertions

## Platform-Specific Considerations

### Windows
- Maintain compatibility with Windows 10 1809+
- Support high-DPI displays
- Windows-specific GPS drivers via COM ports

### Linux
- Target Ubuntu 20.04 LTS as baseline
- Support Wayland and X11
- Consider ARM builds for Raspberry Pi

### macOS
- Support macOS 11 Big Sur+
- Apple Silicon (M1/M2) native builds
- Notarization for distribution

## Future Considerations

### Mobile Support
- **Framework**: .NET MAUI for iOS/Android companion apps
- **Sharing**: Code sharing with core library
- *Timeline*: Post v1.0

### Web Technologies
- **API**: ASP.NET Core for potential web services
- **Real-time**: SignalR for live field updates
- *Timeline*: Version 2.0

### Machine Learning
- **Framework**: ML.NET for on-device inference
- **Use cases**: Path optimization, yield prediction
- *Timeline*: Version 2.0

## Development Environment

### Recommended IDE
- **Primary**: JetBrains Rider (cross-platform)
- **Alternative**: Visual Studio 2022 (Windows)
- **Lightweight**: VS Code with C# Dev Kit

### Minimum System Requirements
- **Development**:
  - 8GB RAM (16GB recommended)
  - .NET 8 SDK
  - 10GB disk space
  - GPU with OpenGL 3.3+ support

- **Runtime**:
  - 4GB RAM
  - .NET 8 runtime
  - 500MB disk space
  - OpenGL 3.3+ compatible graphics

## Security Considerations

### Code Security
- No hardcoded credentials
- Secure storage for AgShare API keys
- Input validation on all external data
- Regular dependency updates

### Data Privacy
- Local-first data storage
- Optional cloud sync (user controlled)
- No telemetry without consent
- GDPR compliance for EU users

## Documentation Stack

### Code Documentation
- **XML Documentation**: Built into C# source
- **API Docs**: DocFX for API reference generation

### User Documentation
- **Format**: Markdown in `/docs` folder
- **Site Generator**: MkDocs with Material theme
- **Hosting**: GitHub Pages

## Monitoring & Analytics

### Application Monitoring
- **Logging**: Serilog with file and console sinks
- **Crash Reporting**: Optional, opt-in only
- **Performance**: Built-in diagnostics with EventCounters

### Usage Analytics
- **Policy**: Opt-in only
- **Framework**: Application Insights (if user consents)
- **Data**: Anonymous usage patterns only

---

## Current Implementation Details

### Services Architecture (✅ Implemented)
| Service | Purpose | Status |
|---------|---------|--------|
| UdpCommunicationService | AgIO module communication | ✅ Complete |
| GpsService | GPS data management | ✅ Complete |
| NtripClientService | RTK corrections | ✅ Complete |
| NmeaParserService | NMEA sentence parsing | ✅ Complete |
| FieldService | Field orchestration | ✅ Complete |
| FieldPlaneFileService | Field.txt I/O | ✅ Complete |
| BoundaryFileService | Boundary.txt I/O | ✅ Complete |
| BackgroundImageFileService | Satellite imagery | ✅ Complete |
| FieldStatisticsService | Area calculations | ✅ Complete |
| GuidanceService | Steering algorithms | ✅ Complete |

### Models (✅ Implemented)
- Position (WGS84, UTM coordinates)
- Field (metadata, boundaries, AB lines)
- Boundary (polygons with collision detection)
- GpsData (fix quality, satellites, HDOP)
- Vehicle & VehicleConfiguration
- PgnMessage (AgOpenGPS protocol)
- ABLine (guidance lines)

## Migration Strategy from AgOpenGPS

### Code Migration Principles (Being Followed)
1. **No Direct Code Copying**: ✅ Services ported with clean rewrites
2. **Clean Implementation**: ✅ Using MVVM, DI, interfaces
3. **Behavioral Compatibility**: ✅ File formats and protocols maintained
4. **Incremental Migration**: ✅ Phase-by-phase implementation

### Compatibility Achievements
1. **Field Files**: ✅ Reads AgOpenGPS formats (Field.txt, Boundary.txt)
2. **AgIO Protocol**: ✅ UDP-based PGN messaging implemented
3. **Hardware**: ✅ Works with Teensy-based modules
4. **NMEA**: ✅ $PANDA/$PAOGI support

### AgShare Integration Status
- **Current**: ⏳ Planned for Phase 1
- **Architecture**: Ready (FieldService will coordinate)
- **API Client**: To be implemented

---

*This tech stack document reflects the actual implementation status of AgValoniaGPS, which has successfully established the core architecture and integrated AgIO functionality (minus serial features) as a functional proof of concept.*