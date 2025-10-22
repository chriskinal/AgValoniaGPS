# AgOpenGPS UI Extraction Documentation

Automated extraction of UI structure from legacy AgOpenGPS Windows Forms application.

## Quick Links

📊 **[Migration Guide](AVALONIA-MIGRATION-GUIDE.md)** - Start here for migration planning (466-992 hour estimate)

📈 **[Extraction Summary](UI-EXTRACTION-SUMMARY.md)** - Statistics and overview of all 74 forms

🔀 **[Navigation Graph](navigation-graph.md)** - Mermaid diagram showing form relationships

🎨 **[Complexity Heatmap](complexity-heatmap.md)** - Visual ranking of forms by complexity

🌐 **[Interactive HTML Docs](ui-documentation.html)** - Download and open in browser for searchable interface

### Phase 6: Dynamic Behavior Analysis (NEW)

🎛️ **[UI State Machine](ui-state-machine.md)** - 11 UI modes with state transitions (Mermaid diagrams)

👁️ **[Visibility Rules](visibility-rules.md)** - 312 dynamic control visibility rules

🎨 **[Property Changes](property-changes.md)** - 2,033 dynamic property changes (Text, Color, etc.)

🔄 **[Avalonia MVVM Guide](avalonia-mvvm-guide.md)** - Convert dynamic behavior to reactive bindings

## What's Included

### Documentation Files

- **AVALONIA-MIGRATION-GUIDE.md** - Comprehensive migration guide
  - Executive summary with effort estimates
  - Control mapping reference (Windows Forms → Avalonia)
  - Form-by-form migration plan (ordered by complexity)
  - Architecture recommendations (MVVM, DI, project structure)
  - Event handler migration patterns
  - Known challenges and solutions

- **UI-EXTRACTION-SUMMARY.md** - Human-readable summary
  - Overall statistics (10,774 controls, 670 event handlers)
  - Navigation insights (16 navigations, 64 entry points)
  - Complex form analysis (6 forms with >500 controls)
  - Detailed breakdown of all 74 forms

### Phase 6: Dynamic Behavior Documentation

- **ui-state-machine.md** - UI state machine analysis
  - 11 UI modes with descriptions
  - Mermaid state transition diagrams
  - State variable impact analysis (which controls depend on which state variables)

- **visibility-rules.md** - Dynamic visibility patterns
  - 312 visibility rules extracted from code
  - Top 10 forms by visibility rules with tables
  - Conditional visibility logic

- **property-changes.md** - Dynamic property modifications
  - 2,033 property changes (Text, BackColor, Image, Enabled, etc.)
  - Changes grouped by property type
  - Frequency analysis

- **avalonia-mvvm-guide.md** - MVVM conversion guide
  - State Variables → ViewModel Properties
  - Visibility Rules → AXAML Bindings
  - Property Changes → Reactive Bindings
  - UI Modes → ViewModel States
  - Implementation checklist with counts

### Mermaid Diagrams

- **navigation-graph.md** - Form navigation relationships
- **complexity-heatmap.md** - Visual complexity ranking
- **form-structure-FormConfig.md** - 2,133 controls
- **form-structure-FormSteerWiz.md** - 1,299 controls
- **form-structure-FormAllSettings.md** - 1,080 controls
- **form-structure-FormSteer.md** - 900 controls
- **form-structure-FormGPS.md** - 720 controls (main window)
- **form-structure-FormBuildTracks.md** - 501 controls

### Data Files

- **ui-structure.json** (3.5MB) - Complete machine-readable extraction data
- **ui-documentation.html** (140KB) - Interactive searchable documentation

## Viewing the Documentation

### On GitHub (Recommended)
GitHub natively renders Mermaid diagrams! Just click any `.md` file above.

### Interactive HTML
Download `ui-documentation.html` and open in any browser for:
- Real-time search across all forms
- Expandable form details
- Color-coded complexity indicators
- Navigation relationship table
- Statistics dashboard

### JSON Data
`ui-structure.json` contains the raw extracted data for programmatic access:
- 74 forms with complete metadata
- 10,774 controls with properties
- 670 event handlers with descriptions
- Navigation graph and structure analysis

## Extraction Statistics

**Successfully extracted from 74 forms:**
- 52 Menu Items
- 8 Toolbar Buttons
- 10,774 Controls
- 670 Event Handlers
- 47 State Variables
- 4 Images
- 1 String Resource

**Navigation & Structure:**
- 16 Form Navigations
- 64 Entry Points (forms with no parents)
- 8 Forms with Children
- 6 Complex Forms Analyzed (>500 controls each)

**Dynamic Behavior (Phase 6):**
- 312 Visibility Rules (conditional control show/hide)
- 2,033 Property Changes (Text, BackColor, Image, Enabled, etc.)
- 11 UI Modes (inferred application states)
- 25 State Transitions (mode changes)
- 68 Code-Behind Files Analyzed

**Migration Effort Estimate:**
- Complex Forms (>500 controls): 6 forms × 40-80 hours = 240-480 hours
- Moderate Forms (100-500): 15 forms × 8-20 hours = 120-300 hours
- Simple Forms (<100): 53 forms × 2-4 hours = 106-212 hours
- **Total: 466-992 hours**

## Form Complexity Breakdown

| Complexity | Control Count | Forms | Percentage |
|------------|--------------|-------|------------|
| 🔴 Complex | >500 | 6 | 8% |
| 🟡 Moderate | 100-500 | 15 | 20% |
| 🟢 Simple | <100 | 53 | 72% |

## Top 5 Most Complex Forms

1. **FormConfig** - 2,133 controls (Configuration settings)
2. **FormSteerWiz** - 1,299 controls (Auto steer wizard)
3. **FormAllSettings** - 1,080 controls (Settings summary)
4. **FormSteer** - 900 controls (Auto steer configuration)
5. **FormGPS** - 720 controls (Main window with 52 menu items)

## How This Was Generated

This documentation was automatically generated by the **AgOpenGPS UI Extractor** tool:

**6-Phase Extraction Process:**
1. **Phase 1**: Static UI structure from `*.Designer.cs` files (Roslyn parsing)
2. **Phase 2**: Dynamic behavior from `*.cs` code-behind (event handlers, state variables)
3. **Phase 3**: Resources from `*.resx` files (images, strings)
4. **Phase 4**: Navigation analysis and form structure patterns
5. **Phase 5**: Documentation generation (Mermaid, migration guide, HTML)
6. **Phase 6**: Dynamic behavior analysis (visibility rules, property changes, UI modes, state transitions)

**Tool Location:** `/Tools/AgOpenGPS.UIExtractor/`

## Next Steps

1. Review the **[Migration Guide](AVALONIA-MIGRATION-GUIDE.md)** for planning
2. Check the **[Navigation Graph](navigation-graph.md)** to understand application flow
3. Review **[Complex Form Diagrams](form-structure-FormConfig.md)** for refactoring opportunities
4. Use the **Interactive HTML** for detailed exploration
5. Reference the migration guide's control mappings during implementation

---

*Generated: 2025-10-21*
*Source: AgOpenGPS v6.x Windows Forms Application*
*Extractor Version: 1.0 (All Phases Complete)*
