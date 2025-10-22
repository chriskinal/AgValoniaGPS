# UI Extraction Tool Proposal

## Executive Summary

Create an automated C# console application to extract and document the complete UI structure from the legacy AgOpenGPS Windows Forms application. The tool will parse 78+ Designer.cs files and generate structured JSON/YAML documentation of menus, toolbars, dialogs, and state-dependent UI behaviors.

**Goal**: Enable faithful recreation of AgOpenGPS UI in Avalonia with full understanding of complex, state-dependent controls and user-configurable layouts.

---

## Problem Statement

The legacy AgOpenGPS UI contains:
- 78+ forms and dialogs
- Dynamic menus and toolbars that change based on application state
- User-configurable button panels
- Context-sensitive controls
- Extensive localization (15+ languages)
- Complex state management for showing/hiding controls

**Manual extraction would be:**
- Time-consuming (weeks of work)
- Error-prone (easy to miss controls)
- Incomplete (hard to capture all state transitions)
- Difficult to maintain as reference

---

## Proposed Solution

### Tool Architecture

```
AgOpenGPS.UIExtractor/
├── Program.cs                    # Entry point, orchestration
├── Parsers/
│   ├── DesignerParser.cs        # Parses Designer.cs files
│   ├── CodeBehindParser.cs      # Parses event handlers & logic
│   ├── ResourceParser.cs        # Parses .resx for strings/icons
│   └── MenuParser.cs            # Specialized menu extraction
├── Models/
│   ├── UIElement.cs             # Base class for all controls
│   ├── MenuItem.cs              # Menu/submenu representation
│   ├── ToolbarButton.cs         # Toolbar button with state
│   ├── Panel.cs                 # Panel/container
│   └── StateCondition.cs        # Visibility/enabled conditions
├── Analyzers/
│   ├── StateAnalyzer.cs         # Tracks state-dependent behavior
│   ├── HierarchyBuilder.cs      # Builds control trees
│   └── IconExtractor.cs         # Extracts embedded images
└── Output/
    ├── JsonGenerator.cs         # Generates JSON documentation
    ├── MarkdownGenerator.cs     # Generates human-readable docs
    └── DiagramGenerator.cs      # Generates Mermaid diagrams
```

---

## Implementation Phases

### Phase 1: Core Parser (Week 1)
**Objective**: Extract static UI structure from Designer.cs files

**Tasks**:
1. Create console app project structure
2. Implement Designer.cs parser using Roslyn
3. Extract control hierarchies:
   - MenuStrip items
   - ToolStrip buttons
   - Panels and containers
   - Buttons, labels, textboxes
4. Capture properties:
   - Text/Name
   - Position/Size
   - Dock/Anchor
   - Tab order
5. Generate basic JSON output

**Deliverable**: JSON file with all static controls from FormGPS

**Success Criteria**:
- ✅ Parse FormGPS.Designer.cs without errors
- ✅ Extract all MenuStrip items in correct hierarchy
- ✅ Identify all ToolStrip buttons with names/text
- ✅ Generate valid JSON

### Phase 2: Event Handler Analysis (Week 2)
**Objective**: Map event handlers and dynamic behavior

**Tasks**:
1. Parse FormGPS.cs for event handler methods
2. Identify state-dependent visibility logic:
   ```csharp
   if (isFieldOpen)
       btnGuidance.Visible = true;
   ```
3. Track button enable/disable conditions
4. Map Click handlers to form/dialog opens
5. Extract keyboard shortcuts

**Deliverable**: Enhanced JSON with event mappings

**Success Criteria**:
- ✅ Map all button clicks to their handlers
- ✅ Identify state variables that control visibility
- ✅ Document which buttons open which forms
- ✅ Extract all keyboard shortcuts

### Phase 3: Resource Extraction (Week 2)
**Objective**: Extract icons, images, and localized strings

**Tasks**:
1. Parse .resx files for UI strings
2. Extract embedded images/icons
3. Convert images to modern formats (SVG/PNG)
4. Map strings to controls
5. Document localization keys

**Deliverable**: Resource catalog with all UI assets

**Success Criteria**:
- ✅ Extract all button icons
- ✅ Export images in usable formats
- ✅ Map every UI string to its control
- ✅ Document translation keys

### Phase 4: Specialized Extractors (Week 3)
**Objective**: Handle complex UI patterns

**Tasks**:
1. **Menu Configuration Parser**
   - Parse ConfigMenu logic
   - Extract user customization options

2. **Right Panel Analyzer**
   - Parse FormButtonsRightPanel
   - Document configurable slots
   - List all available button choices

3. **State Machine Builder**
   - Analyze state transitions
   - Map application states to UI changes
   - Create state diagram

4. **Settings Analyzer**
   - Parse all Config* forms
   - Document settings hierarchy
   - Map settings to UI changes

**Deliverable**: Complete UI documentation package

**Success Criteria**:
- ✅ Document all user customization options
- ✅ Generate state transition diagram
- ✅ Complete catalog of all 78 forms
- ✅ Full settings hierarchy documented

### Phase 5: Output Generation (Week 3)
**Objective**: Generate multiple output formats

**Tasks**:
1. **JSON Schema**
   - Complete UI structure
   - Indexed by form/control
   - Include all metadata

2. **Markdown Documentation**
   - Human-readable reference
   - Organized by functional area
   - Screenshots/diagrams

3. **Mermaid Diagrams**
   - Menu hierarchies
   - Form navigation flow
   - State transitions

4. **Avalonia Migration Guide**
   - Mapping Windows Forms → Avalonia controls
   - Code snippets for common patterns
   - MVVM conversion suggestions

**Deliverable**: Multi-format documentation package

**Success Criteria**:
- ✅ Valid JSON schema validated
- ✅ Complete markdown reference generated
- ✅ Interactive diagrams created
- ✅ Migration guide ready for developers

---

## Technical Approach

### 1. Roslyn Syntax Analysis
Use Roslyn to parse C# files as syntax trees:

```csharp
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var tree = CSharpSyntaxTree.ParseText(designerCode);
var root = tree.GetRoot();

// Find InitializeComponent method
var initMethod = root.DescendantNodes()
    .OfType<MethodDeclarationSyntax>()
    .FirstOrDefault(m => m.Identifier.Text == "InitializeComponent");

// Extract control declarations
var controls = initMethod.DescendantNodes()
    .OfType<AssignmentExpressionSyntax>();
```

### 2. Pattern Recognition
Identify common patterns:

```csharp
// Visibility control pattern
if (controlName.Visible = expression)
    → StateCondition: "controlName visible when expression"

// Menu item pattern
menuStrip.Items.Add(toolStripMenuItem)
    → MenuItem hierarchy

// Event handler pattern
button.Click += Button_Click
    → Event mapping
```

### 3. Smart Defaults
Handle incomplete information:
- Infer state variables from naming (isFieldOpen, hasGuidance)
- Group related controls automatically
- Suggest Avalonia equivalents for Windows Forms controls

---

## Output Format

### JSON Structure
```json
{
  "version": "1.0",
  "source": "AgOpenGPS v6.x",
  "extractedDate": "2025-10-21",
  "forms": {
    "FormGPS": {
      "type": "MainWindow",
      "title": "AgOpenGPS",
      "size": { "width": 1920, "height": 1080 },
      "menus": {
        "mainMenu": {
          "items": [
            {
              "id": "menuFile",
              "text": "File",
              "shortcut": "Alt+F",
              "items": [
                {
                  "id": "menuFieldNew",
                  "text": "New Field",
                  "shortcut": "Ctrl+N",
                  "icon": "NewField.png",
                  "action": "OpenForm",
                  "target": "FormFieldNew",
                  "enabledWhen": "!isFieldOpen"
                }
              ]
            }
          ]
        }
      },
      "toolbars": {
        "mainToolbar": {
          "dock": "Top",
          "buttons": [
            {
              "id": "btnABLine",
              "text": "AB Line",
              "icon": "ABLine.png",
              "tooltip": "Create or Edit AB Line",
              "action": "OpenDialog",
              "target": "FormABLine",
              "visibleWhen": "isFieldOpen && !isRecording",
              "enabledWhen": "hasGPS",
              "shortcut": "F1"
            }
          ]
        }
      },
      "panels": {
        "rightPanel": {
          "configurable": true,
          "dock": "Right",
          "slots": 8,
          "defaultButtons": ["btnABLine", "btnContour", "btnFlags"]
        }
      },
      "stateVariables": {
        "isFieldOpen": {
          "type": "bool",
          "description": "True when a field is currently open",
          "affects": ["btnABLine", "btnContour", "menuFieldClose"]
        },
        "hasGPS": {
          "type": "bool",
          "description": "True when GPS fix is valid",
          "affects": ["btnAutoSteer", "btnSection"]
        }
      }
    }
  },
  "dialogs": {
    "FormABLine": {
      "type": "Dialog",
      "modal": true,
      "controls": [...]
    }
  },
  "resources": {
    "icons": {
      "ABLine.png": {
        "path": "Resources/Icons/ABLine.png",
        "size": "32x32",
        "format": "PNG"
      }
    },
    "strings": {
      "en-US": {
        "btnABLine.Text": "AB Line",
        "btnABLine.Tooltip": "Create or Edit AB Line"
      }
    }
  }
}
```

### Markdown Documentation
```markdown
# AgOpenGPS UI Reference

## Main Window (FormGPS)

### Menu Bar

#### File Menu
- **New Field** (Ctrl+N) - Opens new field creation dialog
  - Enabled: When no field is open
  - Action: Opens `FormFieldNew`

- **Open Field** (Ctrl+O) - Opens field selection dialog
  - Enabled: Always
  - Action: Opens `FormFieldExisting`

### Main Toolbar

#### AB Line Button
- **Icon**: ABLine.png
- **Shortcut**: F1
- **Visible**: When field is open and not recording
- **Enabled**: When GPS fix is valid
- **Action**: Opens AB Line creation/edit dialog
```

---

## Technology Stack

**Parser**:
- .NET 8.0 Console Application
- Microsoft.CodeAnalysis (Roslyn) - C# syntax parsing
- System.Text.Json - JSON generation
- Markdig - Markdown generation

**Optional**:
- Mermaid.CLI - Diagram generation
- System.Drawing.Common - Image processing
- HtmlAgilityPack - .resx parsing

---

## Deliverables

1. **UIExtractor.exe** - Console application
   - Input: Path to SourceCode/GPS directory
   - Output: Complete UI documentation package

2. **UI Documentation Package**:
   - `ui-structure.json` - Complete machine-readable structure
   - `UI-REFERENCE.md` - Human-readable documentation
   - `diagrams/` - Mermaid diagrams (menus, navigation, states)
   - `icons/` - Extracted icons in PNG/SVG
   - `MIGRATION-GUIDE.md` - Avalonia conversion guide

3. **Source Code**:
   - Fully commented and documented
   - Unit tests for parsers
   - README with usage instructions

---

## Success Metrics

1. **Coverage**: Extract 100% of forms (78/78)
2. **Accuracy**: 95%+ accuracy on manual verification
3. **Completeness**: Document all menu items, toolbar buttons, panels
4. **Usability**: Non-technical users can understand markdown docs
5. **Maintainability**: Tool can be re-run as legacy code updates

---

## Timeline

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Phase 1: Core Parser | 1 week | JSON with static controls |
| Phase 2: Event Handlers | 1 week | Event mappings added |
| Phase 3: Resources | 3 days | Icon/string extraction |
| Phase 4: Specialized | 4 days | Complete UI catalog |
| Phase 5: Output | 3 days | Multi-format docs |
| **Total** | **3 weeks** | **Complete Package** |

---

## Future Enhancements

1. **Automated Avalonia Code Generation**
   - Generate .axaml files from JSON
   - Create ViewModels with proper bindings
   - Generate command handlers

2. **Live Documentation**
   - Web-based UI explorer
   - Interactive diagrams
   - Search functionality

3. **Diff Tool**
   - Compare different AgOpenGPS versions
   - Track UI changes over time
   - Migration impact analysis

4. **Validation**
   - Verify all forms are reachable
   - Check for unused controls
   - Identify dead code

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Complex dynamic logic hard to parse | Manual annotation for edge cases |
| Missing state transitions | Add manual state documentation |
| Localization complexity | Focus on en-US first, extend later |
| Large codebase takes time | Incremental extraction, prioritize FormGPS |
| Output too large to consume | Generate multiple targeted docs |

---

## Cost-Benefit Analysis

**Manual Approach**:
- Time: 4-6 weeks
- Risk: High error rate, inconsistencies
- Maintainability: Low (must redo for updates)

**Automated Tool**:
- Initial Development: 3 weeks
- Future Runs: Minutes
- Accuracy: 95%+
- Maintainability: High (reusable)
- **ROI**: Positive after first use, scales indefinitely

---

## Recommendation

**Proceed with automated tool development** using the phased approach. Start with Phase 1 (Core Parser) to validate feasibility, then continue based on results.

**Priority**: Focus on FormGPS main window first (80% of UI complexity), then expand to dialogs.

**Quick Win**: Phase 1 alone provides significant value - knowing all menus and toolbars enables UI design to start immediately.

---

*Created: 2025-10-21*
*For: AgValoniaGPS UI Migration Project*
*Author: Claude Code*
