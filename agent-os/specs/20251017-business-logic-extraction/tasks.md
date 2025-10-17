# Task Breakdown: Business Logic Extraction from AgOpenGPS to AgValoniaGPS

[Content before line 361 remains the same...]

- [x] WAVE1-008: Define IHeadingCalculatorService interface
  - [x] WAVE1-008.1: Create interface with all methods
  - [x] WAVE1-008.2: Define HeadingSource enum
  - [x] WAVE1-008.3: Define HeadingUpdate event args
  - [x] WAVE1-008.4: Add XML documentation

  **Effort:** 4 hours

  **Deliverables:**
  - IHeadingCalculatorService.cs interface
  - HeadingSource enum
  - Complete documentation

  **Acceptance Criteria:**
  - All heading modes represented
  - Source selection logic clear
  - Events properly defined

- [x] WAVE1-009: Implement HeadingCalculatorService
  - [x] WAVE1-009.1: Implement fix-to-fix heading calculation
  - [x] WAVE1-009.2: Implement VTG heading processing
  - [x] WAVE1-009.3: Implement dual antenna heading
  - [x] WAVE1-009.4: Implement IMU fusion algorithm
  - [x] WAVE1-009.5: Implement roll compensation
  - [x] WAVE1-009.6: Implement circular heading math utilities
  - [x] WAVE1-009.7: Implement optimal source determination

  **Effort:** 16 hours

  **Deliverables:**
  - HeadingCalculatorService.cs implementation
  - Heading calculation utilities
  - Source selection logic

  **Acceptance Criteria:**
  - All heading modes working
  - Heading accuracy within 0.1 degrees
  - Smooth transitions between sources
  - No heading jumps at 0/360 boundary
  - Calculation time <1ms

[... content continues from line 402 to line 243...]

- [ ] WAVE1-018: Register Wave 1 services in DI container
  - [x] WAVE1-018.1: Add IPositionUpdateService registration
  - [x] WAVE1-018.2: Add IHeadingCalculatorService registration
  - [ ] WAVE1-018.3: Add IVehicleKinematicsService registration
  - [ ] WAVE1-018.4: Configure service lifetimes
  - [ ] WAVE1-018.5: Test service resolution

[Content continues normally after this point...]
