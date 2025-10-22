using System.Text;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Output;

/// <summary>
/// Generates documentation for dynamic UI behavior
/// </summary>
public class DynamicBehaviorDocGenerator
{
    public void GenerateUIStateMachineDoc(DynamicBehaviorResult behavior, string outputPath)
    {
        var doc = new StringBuilder();

        doc.AppendLine("# AgOpenGPS UI State Machine");
        doc.AppendLine();
        doc.AppendLine("**Dynamic UI Behavior Analysis**");
        doc.AppendLine();

        // Overview
        doc.AppendLine("## Overview");
        doc.AppendLine();
        doc.AppendLine($"- **Visibility Rules**: {behavior.VisibilityRules.Count}");
        doc.AppendLine($"- **Property Changes**: {behavior.PropertyChanges.Count}");
        doc.AppendLine($"- **UI Modes**: {behavior.Modes.Count}");
        doc.AppendLine($"- **State Transitions**: {behavior.StateTransitions.Count}");
        doc.AppendLine();

        // UI Modes
        if (behavior.Modes.Count > 0)
        {
            doc.AppendLine("## UI Modes");
            doc.AppendLine();
            doc.AppendLine("The application operates in different UI modes, each with specific visible/hidden controls:");
            doc.AppendLine();

            foreach (var mode in behavior.Modes.Take(20)) // Limit to top 20
            {
                doc.AppendLine($"### {mode.ModeName}");
                doc.AppendLine();
                doc.AppendLine($"**Description**: {mode.Description}");
                doc.AppendLine();

                if (mode.Triggers.Any())
                {
                    doc.AppendLine($"**Triggered by**: {string.Join(", ", mode.Triggers.Take(5))}");
                    doc.AppendLine();
                }

                if (mode.VisibleControls.Any())
                {
                    doc.AppendLine($"**Visible Controls** ({mode.VisibleControls.Count}):");
                    foreach (var control in mode.VisibleControls.Take(10))
                    {
                        doc.AppendLine($"- {control}");
                    }
                    if (mode.VisibleControls.Count > 10)
                        doc.AppendLine($"- *... and {mode.VisibleControls.Count - 10} more*");
                    doc.AppendLine();
                }

                if (mode.HiddenControls.Any())
                {
                    doc.AppendLine($"**Hidden Controls** ({mode.HiddenControls.Count}):");
                    foreach (var control in mode.HiddenControls.Take(10))
                    {
                        doc.AppendLine($"- {control}");
                    }
                    if (mode.HiddenControls.Count > 10)
                        doc.AppendLine($"- *... and {mode.HiddenControls.Count - 10} more*");
                    doc.AppendLine();
                }

                if (mode.ControlTextChanges.Any())
                {
                    doc.AppendLine($"**Text Changes**:");
                    foreach (var change in mode.ControlTextChanges.Take(5))
                    {
                        doc.AppendLine($"- `{change.Key}` → \"{change.Value}\"");
                    }
                    doc.AppendLine();
                }
            }
        }

        // State Transitions
        if (behavior.StateTransitions.Count > 0)
        {
            doc.AppendLine("## State Transitions");
            doc.AppendLine();
            doc.AppendLine("Transitions between UI modes:");
            doc.AppendLine();
            doc.AppendLine("```mermaid");
            doc.AppendLine("stateDiagram-v2");
            doc.AppendLine();

            foreach (var transition in behavior.StateTransitions.Take(30))
            {
                var label = SanitizeForMermaid(transition.Trigger);
                doc.AppendLine($"    {SanitizeForMermaid(transition.FromMode)} --> {SanitizeForMermaid(transition.ToMode)}: {label}");
            }

            doc.AppendLine("```");
            doc.AppendLine();
        }

        // State Variable Impact
        if (behavior.StateVariableUsage.Any())
        {
            doc.AppendLine("## State Variable Impact");
            doc.AppendLine();
            doc.AppendLine("State variables and the controls they affect:");
            doc.AppendLine();

            var topVariables = behavior.StateVariableUsage
                .OrderByDescending(kv => kv.Value.Count)
                .Take(20);

            foreach (var variable in topVariables)
            {
                doc.AppendLine($"### `{variable.Key}` (affects {variable.Value.Count} controls)");
                doc.AppendLine();
                foreach (var control in variable.Value.Take(10))
                {
                    doc.AppendLine($"- {control}");
                }
                if (variable.Value.Count > 10)
                    doc.AppendLine($"- *... and {variable.Value.Count - 10} more*");
                doc.AppendLine();
            }
        }

        File.WriteAllText(outputPath, doc.ToString());
        Console.WriteLine($"UI state machine documentation written to: {outputPath}");
    }

    public void GenerateVisibilityRulesDoc(DynamicBehaviorResult behavior, string outputPath)
    {
        var doc = new StringBuilder();

        doc.AppendLine("# Dynamic Visibility Rules");
        doc.AppendLine();
        doc.AppendLine($"**Total Rules**: {behavior.VisibilityRules.Count}");
        doc.AppendLine();

        // Group by form
        var rulesByForm = behavior.VisibilityRules
            .GroupBy(r => r.SourceFile)
            .OrderByDescending(g => g.Count())
            .Take(10);

        doc.AppendLine("## Top 10 Forms by Visibility Rules");
        doc.AppendLine();

        foreach (var group in rulesByForm)
        {
            doc.AppendLine($"### {group.Key} ({group.Count()} rules)");
            doc.AppendLine();

            var rules = group.OrderBy(r => r.ControlName).Take(20);
            doc.AppendLine("| Control | Condition | Visible | Context |");
            doc.AppendLine("|---------|-----------|---------|---------|");

            foreach (var rule in rules)
            {
                var condition = rule.Condition.Length > 40 ? rule.Condition.Substring(0, 37) + "..." : rule.Condition;
                doc.AppendLine($"| {rule.ControlName} | `{condition}` | {rule.TargetVisibility} | {rule.Context} |");
            }

            if (group.Count() > 20)
                doc.AppendLine($"\n*... and {group.Count() - 20} more rules*\n");

            doc.AppendLine();
        }

        File.WriteAllText(outputPath, doc.ToString());
        Console.WriteLine($"Visibility rules documentation written to: {outputPath}");
    }

    public void GeneratePropertyChangesDoc(DynamicBehaviorResult behavior, string outputPath)
    {
        var doc = new StringBuilder();

        doc.AppendLine("# Dynamic Property Changes");
        doc.AppendLine();
        doc.AppendLine($"**Total Changes**: {behavior.PropertyChanges.Count}");
        doc.AppendLine();

        // Group by property type
        var changesByProperty = behavior.PropertyChanges
            .GroupBy(c => c.PropertyName)
            .OrderByDescending(g => g.Count());

        doc.AppendLine("## Changes by Property Type");
        doc.AppendLine();

        foreach (var group in changesByProperty)
        {
            doc.AppendLine($"### {group.Key} ({group.Count()} changes)");
            doc.AppendLine();

            var changes = group.Take(15);
            doc.AppendLine("| Control | New Value | Condition | Context |");
            doc.AppendLine("|---------|-----------|-----------|---------|");

            foreach (var change in changes)
            {
                var newValue = change.NewValue.Length > 30 ? change.NewValue.Substring(0, 27) + "..." : change.NewValue;
                var condition = change.Condition.Length > 30 ? change.Condition.Substring(0, 27) + "..." : change.Condition;
                doc.AppendLine($"| {change.ControlName} | `{newValue}` | `{condition}` | {change.Context} |");
            }

            if (group.Count() > 15)
                doc.AppendLine($"\n*... and {group.Count() - 15} more changes*\n");

            doc.AppendLine();
        }

        File.WriteAllText(outputPath, doc.ToString());
        Console.WriteLine($"Property changes documentation written to: {outputPath}");
    }

    public void GenerateAvaloniaMVVMGuide(DynamicBehaviorResult behavior, string outputPath)
    {
        var doc = new StringBuilder();

        doc.AppendLine("# Converting Dynamic UI to Avalonia MVVM");
        doc.AppendLine();
        doc.AppendLine("Guide for implementing dynamic UI behavior in Avalonia using MVVM pattern.");
        doc.AppendLine();

        doc.AppendLine("## Overview");
        doc.AppendLine();
        doc.AppendLine("The legacy Windows Forms application uses direct control manipulation with");
        doc.AppendLine("visibility toggles and property changes throughout the code. In Avalonia MVVM,");
        doc.AppendLine("this should be replaced with property bindings and commands.");
        doc.AppendLine();

        doc.AppendLine("## Conversion Strategy");
        doc.AppendLine();
        doc.AppendLine("### 1. State Variables → ViewModel Properties");
        doc.AppendLine();
        doc.AppendLine("Convert boolean state variables to observable properties:");
        doc.AppendLine();
        doc.AppendLine("```csharp");
        doc.AppendLine("// Legacy Windows Forms");
        doc.AppendLine("private bool isJobStarted = false;");
        doc.AppendLine();
        doc.AppendLine("// Avalonia MVVM");
        doc.AppendLine("private bool _isJobStarted;");
        doc.AppendLine("public bool IsJobStarted");
        doc.AppendLine("{");
        doc.AppendLine("    get => _isJobStarted;");
        doc.AppendLine("    set => this.RaiseAndSetIfChanged(ref _isJobStarted, value);");
        doc.AppendLine("}");
        doc.AppendLine("```");
        doc.AppendLine();

        doc.AppendLine("### 2. Visibility Rules → AXAML Bindings");
        doc.AppendLine();
        doc.AppendLine("Replace visibility assignments with bindings:");
        doc.AppendLine();
        doc.AppendLine("```xml");
        doc.AppendLine("<!-- Legacy: button.Visible = isJobStarted; -->");
        doc.AppendLine("<Button IsVisible=\"{Binding IsJobStarted}\" />");
        doc.AppendLine();
        doc.AppendLine("<!-- With converter for inverse -->");
        doc.AppendLine("<Button IsVisible=\"{Binding IsJobStarted,");
        doc.AppendLine("    Converter={StaticResource InverseBoolConverter}}\" />");
        doc.AppendLine("```");
        doc.AppendLine();

        doc.AppendLine("### 3. Property Changes → Reactive Bindings");
        doc.AppendLine();
        doc.AppendLine("Replace dynamic text/color changes with computed properties:");
        doc.AppendLine();
        doc.AppendLine("```csharp");
        doc.AppendLine("// Legacy: btnStart.Text = isJobStarted ? \"Stop\" : \"Start\";");
        doc.AppendLine();
        doc.AppendLine("// Avalonia MVVM");
        doc.AppendLine("public string StartButtonText => IsJobStarted ? \"Stop\" : \"Start\";");
        doc.AppendLine();
        doc.AppendLine("// Notify property changed when dependency changes");
        doc.AppendLine("public bool IsJobStarted");
        doc.AppendLine("{");
        doc.AppendLine("    get => _isJobStarted;");
        doc.AppendLine("    set");
        doc.AppendLine("    {");
        doc.AppendLine("        this.RaiseAndSetIfChanged(ref _isJobStarted, value);");
        doc.AppendLine("        this.RaisePropertyChanged(nameof(StartButtonText));");
        doc.AppendLine("    }");
        doc.AppendLine("}");
        doc.AppendLine("```");
        doc.AppendLine();

        doc.AppendLine("### 4. UI Modes → ViewModel States");
        doc.AppendLine();
        doc.AppendLine("Implement mode management in ViewModel:");
        doc.AppendLine();
        doc.AppendLine("```csharp");
        doc.AppendLine("public enum UIMode { Idle, GuidanceActive, BoundaryMode, SettingsMode }");
        doc.AppendLine();
        doc.AppendLine("private UIMode _currentMode;");
        doc.AppendLine("public UIMode CurrentMode");
        doc.AppendLine("{");
        doc.AppendLine("    get => _currentMode;");
        doc.AppendLine("    set");
        doc.AppendLine("    {");
        doc.AppendLine("        this.RaiseAndSetIfChanged(ref _currentMode, value);");
        doc.AppendLine("        // Notify all mode-dependent properties");
        doc.AppendLine("        this.RaisePropertyChanged(nameof(IsGuidancePanelVisible));");
        doc.AppendLine("        this.RaisePropertyChanged(nameof(IsBoundaryPanelVisible));");
        doc.AppendLine("    }");
        doc.AppendLine("}");
        doc.AppendLine();
        doc.AppendLine("public bool IsGuidancePanelVisible => CurrentMode == UIMode.GuidanceActive;");
        doc.AppendLine("public bool IsBoundaryPanelVisible => CurrentMode == UIMode.BoundaryMode;");
        doc.AppendLine("```");
        doc.AppendLine();

        doc.AppendLine("## Implementation Checklist");
        doc.AppendLine();
        doc.AppendLine($"- [ ] Convert {behavior.StateVariableUsage.Count} state variables to ViewModel properties");
        doc.AppendLine($"- [ ] Create bindings for {behavior.VisibilityRules.Count} visibility rules");
        doc.AppendLine($"- [ ] Implement {behavior.PropertyChanges.Count} property change bindings");
        doc.AppendLine($"- [ ] Create {behavior.Modes.Count} UI mode enums/states");
        doc.AppendLine($"- [ ] Implement {behavior.StateTransitions.Count} state transition methods");
        doc.AppendLine();

        File.WriteAllText(outputPath, doc.ToString());
        Console.WriteLine($"Avalonia MVVM guide written to: {outputPath}");
    }

    private string SanitizeForMermaid(string text)
    {
        return text.Replace(" ", "_").Replace(".", "_").Replace("-", "_").Replace("(", "").Replace(")", "");
    }
}
