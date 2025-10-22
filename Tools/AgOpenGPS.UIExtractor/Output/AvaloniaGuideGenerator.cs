using System.Text;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Output;

/// <summary>
/// Generates Avalonia migration guide from Windows Forms UI structure
/// </summary>
public class AvaloniaGuideGenerator
{
    private static readonly Dictionary<string, string> ControlMappings = new()
    {
        // Common controls
        { "Button", "Button" },
        { "Label", "TextBlock" },
        { "TextBox", "TextBox" },
        { "CheckBox", "CheckBox" },
        { "RadioButton", "RadioButton" },
        { "ComboBox", "ComboBox" },
        { "ListBox", "ListBox" },
        { "NumericUpDown", "NumericUpDown" },
        { "ProgressBar", "ProgressBar" },
        { "TrackBar", "Slider" },

        // Layout controls
        { "Panel", "Panel or StackPanel or Grid" },
        { "GroupBox", "Border with HeaderedContentControl" },
        { "TabControl", "TabControl" },
        { "TabPage", "TabItem" },
        { "SplitContainer", "Grid with GridSplitter" },
        { "FlowLayoutPanel", "WrapPanel" },
        { "TableLayoutPanel", "Grid" },

        // Menus
        { "MenuStrip", "Menu" },
        { "ToolStripMenuItem", "MenuItem" },
        { "ContextMenuStrip", "ContextMenu" },
        { "ToolStrip", "ToolBar" },
        { "ToolStripButton", "Button in ToolBar" },
        { "StatusStrip", "StatusBar" },

        // Data
        { "DataGridView", "DataGrid" },
        { "ListView", "ListBox or DataGrid" },
        { "TreeView", "TreeView" },
        { "PictureBox", "Image" },

        // OpenGL
        { "GLControl", "OpenGlControlBase (Avalonia.OpenGL)" },

        // Dialogs
        { "OpenFileDialog", "StorageProvider.OpenFilePickerAsync" },
        { "SaveFileDialog", "StorageProvider.SaveFilePickerAsync" },
        { "FolderBrowserDialog", "StorageProvider.OpenFolderPickerAsync" },
        { "ColorDialog", "Custom color picker (no built-in)" },
        { "MessageBox", "Window with custom content or ContentDialog" }
    };

    public void GenerateMigrationGuide(UIExtractionResult result, string outputPath)
    {
        var guide = new StringBuilder();
        guide.AppendLine("# AgOpenGPS to Avalonia Migration Guide");
        guide.AppendLine();
        guide.AppendLine("**Generated**: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        guide.AppendLine();
        guide.AppendLine("## Overview");
        guide.AppendLine();
        guide.AppendLine($"This guide covers the migration of {result.Forms.Count} forms from Windows Forms to Avalonia UI.");
        guide.AppendLine();

        // Executive Summary
        GenerateExecutiveSummary(result, guide);

        // Control Mapping Reference
        GenerateControlMappingReference(result, guide);

        // Form-by-Form Migration Plan
        GenerateFormMigrationPlan(result, guide);

        // Architecture Recommendations
        GenerateArchitectureRecommendations(result, guide);

        // Event Handler Migration
        GenerateEventHandlerGuidance(result, guide);

        // Known Challenges
        GenerateKnownChallenges(result, guide);

        File.WriteAllText(outputPath, guide.ToString());
        Console.WriteLine($"Migration guide written to: {outputPath}");
    }

    private void GenerateExecutiveSummary(UIExtractionResult result, StringBuilder guide)
    {
        guide.AppendLine("## Executive Summary");
        guide.AppendLine();

        var totalControls = result.Statistics.TotalControls;
        var complexForms = result.Forms.Values.Count(f => f.Controls.Count > 500);
        var moderateForms = result.Forms.Values.Count(f => f.Controls.Count >= 100 && f.Controls.Count <= 500);
        var simpleForms = result.Forms.Values.Count(f => f.Controls.Count < 100);

        guide.AppendLine($"- **Total Forms**: {result.Forms.Count}");
        guide.AppendLine($"- **Total Controls**: {totalControls:N0}");
        guide.AppendLine($"- **Complex Forms (>500 controls)**: {complexForms}");
        guide.AppendLine($"- **Moderate Forms (100-500 controls)**: {moderateForms}");
        guide.AppendLine($"- **Simple Forms (<100 controls)**: {simpleForms}");
        guide.AppendLine($"- **Total Event Handlers**: {result.Statistics.TotalEventHandlers}");
        guide.AppendLine();

        guide.AppendLine("### Migration Effort Estimate");
        guide.AppendLine();
        guide.AppendLine("Based on control complexity:");
        guide.AppendLine($"- **High Effort**: {complexForms} forms (estimated 40-80 hours each)");
        guide.AppendLine($"- **Medium Effort**: {moderateForms} forms (estimated 8-20 hours each)");
        guide.AppendLine($"- **Low Effort**: {simpleForms} forms (estimated 2-4 hours each)");
        guide.AppendLine();

        var totalHoursLow = (complexForms * 40) + (moderateForms * 8) + (simpleForms * 2);
        var totalHoursHigh = (complexForms * 80) + (moderateForms * 20) + (simpleForms * 4);
        guide.AppendLine($"**Total Estimated Effort**: {totalHoursLow}-{totalHoursHigh} hours");
        guide.AppendLine();
    }

    private void GenerateControlMappingReference(UIExtractionResult result, StringBuilder guide)
    {
        guide.AppendLine("## Control Mapping Reference");
        guide.AppendLine();
        guide.AppendLine("### Windows Forms → Avalonia");
        guide.AppendLine();

        // Find which controls are actually used
        var usedControls = new HashSet<string>();
        foreach (var form in result.Forms.Values)
        {
            foreach (var control in form.Controls)
            {
                var simpleType = GetSimpleTypeName(control.Type);
                usedControls.Add(simpleType);
            }
        }

        guide.AppendLine("| Windows Forms Control | Avalonia Equivalent | Notes |");
        guide.AppendLine("|----------------------|---------------------|-------|");

        foreach (var mapping in ControlMappings.OrderBy(kv => kv.Key))
        {
            var inUse = usedControls.Contains(mapping.Key) ? "✓" : "";
            var notes = GetMigrationNotes(mapping.Key);
            guide.AppendLine($"| {mapping.Key} {inUse} | {mapping.Value} | {notes} |");
        }

        guide.AppendLine();
        guide.AppendLine("✓ = Used in extracted forms");
        guide.AppendLine();
    }

    private void GenerateFormMigrationPlan(UIExtractionResult result, StringBuilder guide)
    {
        guide.AppendLine("## Form-by-Form Migration Plan");
        guide.AppendLine();
        guide.AppendLine("Forms ordered by migration priority (simplest to most complex):");
        guide.AppendLine();

        var sortedForms = result.Forms.Values
            .OrderBy(f => f.Controls.Count + f.MenuItems.Count + f.ToolbarButtons.Count)
            .ToList();

        int priority = 1;
        foreach (var form in sortedForms)
        {
            var totalControls = form.Controls.Count + form.MenuItems.Count + form.ToolbarButtons.Count;
            var complexity = GetComplexityLevel(totalControls);
            var effort = GetEffortEstimate(totalControls);

            guide.AppendLine($"### {priority}. {form.FormName}");
            guide.AppendLine();
            guide.AppendLine($"- **Complexity**: {complexity}");
            guide.AppendLine($"- **Total Controls**: {totalControls}");
            guide.AppendLine($"  - Menu Items: {form.MenuItems.Count}");
            guide.AppendLine($"  - Toolbar Buttons: {form.ToolbarButtons.Count}");
            guide.AppendLine($"  - Other Controls: {form.Controls.Count}");
            guide.AppendLine($"- **Event Handlers**: {form.EventHandlers.Count}");
            guide.AppendLine($"- **Estimated Effort**: {effort}");

            if (!string.IsNullOrEmpty(form.Title))
            {
                guide.AppendLine($"- **Title**: {form.Title}");
            }

            // Migration recommendations
            if (totalControls > 500)
            {
                guide.AppendLine();
                guide.AppendLine("**Recommendations**:");
                guide.AppendLine("- Break into smaller ViewModels and UserControls");
                guide.AppendLine("- Use TabControl or split into separate windows");
                guide.AppendLine("- Consider refactoring before migration");
            }

            guide.AppendLine();
            priority++;
        }
    }

    private void GenerateArchitectureRecommendations(UIExtractionResult result, StringBuilder guide)
    {
        guide.AppendLine("## Architecture Recommendations");
        guide.AppendLine();
        guide.AppendLine("### MVVM Pattern");
        guide.AppendLine();
        guide.AppendLine("Avalonia strongly encourages MVVM. For each form:");
        guide.AppendLine();
        guide.AppendLine("1. **Create a ViewModel**");
        guide.AppendLine("   - Inherit from `ReactiveObject` or `ViewModelBase`");
        guide.AppendLine("   - Implement `INotifyPropertyChanged`");
        guide.AppendLine("   - Move all business logic from code-behind");
        guide.AppendLine();
        guide.AppendLine("2. **Create AXAML View**");
        guide.AppendLine("   - Replace Designer.cs with AXAML markup");
        guide.AppendLine("   - Use data binding instead of direct control access");
        guide.AppendLine("   - Use commands instead of event handlers");
        guide.AppendLine();
        guide.AppendLine("3. **Dependency Injection**");
        guide.AppendLine("   - Register ViewModels in DI container");
        guide.AppendLine("   - Inject services into ViewModels");
        guide.AppendLine("   - Use `ServiceCollectionExtensions` pattern");
        guide.AppendLine();

        guide.AppendLine("### Project Structure");
        guide.AppendLine();
        guide.AppendLine("```");
        guide.AppendLine("AgValoniaGPS/");
        guide.AppendLine("├── AgValoniaGPS.Models/          # Domain models");
        guide.AppendLine("├── AgValoniaGPS.ViewModels/      # ViewModels");
        guide.AppendLine("├── AgValoniaGPS.Services/        # Business logic services");
        guide.AppendLine("└── AgValoniaGPS.Desktop/         # Avalonia UI");
        guide.AppendLine("    ├── Views/                    # AXAML views");
        guide.AppendLine("    ├── Controls/                 # Custom controls");
        guide.AppendLine("    └── Converters/               # Value converters");
        guide.AppendLine("```");
        guide.AppendLine();
    }

    private void GenerateEventHandlerGuidance(UIExtractionResult result, StringBuilder guide)
    {
        guide.AppendLine("## Event Handler Migration");
        guide.AppendLine();
        guide.AppendLine("### Common Event Patterns");
        guide.AppendLine();
        guide.AppendLine("| Windows Forms Event | Avalonia Equivalent |");
        guide.AppendLine("|---------------------|---------------------|");
        guide.AppendLine("| Click | Command binding or Tapped event |");
        guide.AppendLine("| TextChanged | PropertyChanged notification |");
        guide.AppendLine("| SelectedIndexChanged | PropertyChanged on SelectedItem |");
        guide.AppendLine("| CheckedChanged | PropertyChanged on IsChecked |");
        guide.AppendLine("| Load | OnInitialized or OnLoaded |");
        guide.AppendLine("| FormClosing | Window.Closing event |");
        guide.AppendLine("| KeyDown | KeyDown event or KeyBindings |");
        guide.AppendLine("| MouseClick | PointerPressed/PointerReleased |");
        guide.AppendLine("| Paint | Custom control with OnRender |");
        guide.AppendLine();

        var totalHandlers = result.Statistics.TotalEventHandlers;
        guide.AppendLine($"**Total Event Handlers to Migrate**: {totalHandlers}");
        guide.AppendLine();
        guide.AppendLine("Most event handlers should be converted to:");
        guide.AppendLine("1. **Commands** (ICommand) for user actions");
        guide.AppendLine("2. **Property bindings** for state changes");
        guide.AppendLine("3. **ReactiveCommand** for async operations");
        guide.AppendLine();
    }

    private void GenerateKnownChallenges(UIExtractionResult result, StringBuilder guide)
    {
        guide.AppendLine("## Known Migration Challenges");
        guide.AppendLine();
        guide.AppendLine("### 1. OpenGL Rendering");
        guide.AppendLine();
        guide.AppendLine("**Challenge**: GLControl is Windows Forms specific");
        guide.AppendLine();
        guide.AppendLine("**Solution**: Use Avalonia.OpenGL");
        guide.AppendLine("- Inherit from `OpenGlControlBase`");
        guide.AppendLine("- Implement `OnOpenGlInit`, `OnOpenGlDeinit`, `OnOpenGlRender`");
        guide.AppendLine("- Same OpenTK API available");
        guide.AppendLine();

        guide.AppendLine("### 2. Complex Forms (>500 controls)");
        guide.AppendLine();
        var complexFormNames = result.Forms.Values
            .Where(f => f.Controls.Count > 500)
            .Select(f => f.FormName)
            .ToList();

        guide.AppendLine($"Affected forms: {string.Join(", ", complexFormNames)}");
        guide.AppendLine();
        guide.AppendLine("**Challenge**: Large forms cause slow startup and poor UX");
        guide.AppendLine();
        guide.AppendLine("**Solution**:");
        guide.AppendLine("- Break into multiple UserControls");
        guide.AppendLine("- Use lazy loading for tab content");
        guide.AppendLine("- Consider data virtualization");
        guide.AppendLine();

        guide.AppendLine("### 3. Custom Dialogs");
        guide.AppendLine();
        guide.AppendLine("**Challenge**: No MessageBox.Show() equivalent");
        guide.AppendLine();
        guide.AppendLine("**Solution**:");
        guide.AppendLine("- Create custom dialog windows");
        guide.AppendLine("- Use ShowDialog() for modal behavior");
        guide.AppendLine("- Consider MessageBox.Avalonia NuGet package");
        guide.AppendLine();

        guide.AppendLine("### 4. Designer Migration");
        guide.AppendLine();
        guide.AppendLine("**Challenge**: No visual designer for Avalonia (yet)");
        guide.AppendLine();
        guide.AppendLine("**Solution**:");
        guide.AppendLine("- Use Avalonia XAML Intellisense in VS/Rider");
        guide.AppendLine("- Use Avalonia Previewer for live preview");
        guide.AppendLine("- Hand-code AXAML based on extracted structure");
        guide.AppendLine();
    }

    private string GetSimpleTypeName(string fullType)
    {
        var parts = fullType.Split('.');
        return parts.Last();
    }

    private string GetComplexityLevel(int controlCount)
    {
        if (controlCount >= 2000) return "⚠️ Extreme";
        if (controlCount >= 1000) return "🔴 Very High";
        if (controlCount >= 500) return "🟡 High";
        if (controlCount >= 100) return "🟢 Moderate";
        return "✅ Low";
    }

    private string GetEffortEstimate(int controlCount)
    {
        if (controlCount >= 2000) return "60-80 hours";
        if (controlCount >= 1000) return "40-60 hours";
        if (controlCount >= 500) return "20-40 hours";
        if (controlCount >= 100) return "8-20 hours";
        return "2-4 hours";
    }

    private string GetMigrationNotes(string controlType)
    {
        return controlType switch
        {
            "GLControl" => "Requires Avalonia.OpenGL package",
            "DataGridView" => "Feature-rich DataGrid available",
            "Panel" => "Choose based on layout needs",
            "GroupBox" => "Combine Border + TextBlock for header",
            "SplitContainer" => "Use Grid with GridSplitter for resizable panels",
            "ColorDialog" => "No built-in, use community package",
            "NumericUpDown" => "Available via NuGet",
            _ => "Direct equivalent available"
        };
    }
}
