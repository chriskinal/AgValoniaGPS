using System.Text;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Output;

/// <summary>
/// Generates Mermaid diagrams from extracted UI data
/// </summary>
public class MermaidGenerator
{
    public void GenerateNavigationDiagram(UIExtractionResult result, string outputPath)
    {
        if (result.NavigationGraph == null)
        {
            Console.WriteLine("Warning: No navigation graph available");
            return;
        }

        var mermaid = new StringBuilder();
        mermaid.AppendLine("# AgOpenGPS Form Navigation Graph");
        mermaid.AppendLine();
        mermaid.AppendLine("```mermaid");
        mermaid.AppendLine("graph TD");
        mermaid.AppendLine();

        // Add entry points with special styling
        foreach (var entryPoint in result.NavigationGraph.EntryPoints.Take(10)) // Limit to top 10 for readability
        {
            var displayName = SanitizeNodeName(entryPoint);
            mermaid.AppendLine($"    {displayName}[[\"{entryPoint}\"]]");
            mermaid.AppendLine($"    style {displayName} fill:#90EE90,stroke:#333,stroke-width:2px");
        }
        mermaid.AppendLine();

        // Add navigation relationships
        var processedNavigations = new HashSet<string>();
        foreach (var nav in result.NavigationGraph.Navigations)
        {
            var key = $"{nav.SourceForm}->{nav.TargetForm}";
            if (processedNavigations.Contains(key))
                continue;

            var sourceName = SanitizeNodeName(nav.SourceForm);
            var targetName = SanitizeNodeName(nav.TargetForm);
            var label = GetEventLabel(nav.TriggerEvent);

            mermaid.AppendLine($"    {sourceName} -->|{label}| {targetName}");
            processedNavigations.Add(key);
        }

        mermaid.AppendLine("```");
        mermaid.AppendLine();
        mermaid.AppendLine("## Legend");
        mermaid.AppendLine("- **Green boxes with double border**: Entry points (no parent forms)");
        mermaid.AppendLine("- **Arrows**: Navigation triggered by events (Click, etc.)");
        mermaid.AppendLine();
        mermaid.AppendLine($"**Total Forms**: {result.Forms.Count}");
        mermaid.AppendLine($"**Entry Points**: {result.NavigationGraph.EntryPoints.Count}");
        mermaid.AppendLine($"**Navigations**: {result.NavigationGraph.Navigations.Count}");

        File.WriteAllText(outputPath, mermaid.ToString());
        Console.WriteLine($"Navigation diagram written to: {outputPath}");
    }

    public void GenerateFormStructureDiagram(FormStructure structure, string outputPath)
    {
        var mermaid = new StringBuilder();
        mermaid.AppendLine($"# {structure.FormName} - Structure Diagram");
        mermaid.AppendLine();
        mermaid.AppendLine("```mermaid");
        mermaid.AppendLine("graph TD");
        mermaid.AppendLine();

        // Root form node
        var rootName = SanitizeNodeName(structure.FormName);
        mermaid.AppendLine($"    {rootName}[\"{structure.FormName}\"]");
        mermaid.AppendLine($"    style {rootName} fill:#4682B4,stroke:#333,stroke-width:3px,color:#fff");
        mermaid.AppendLine();

        // Add control groups
        int groupIndex = 0;
        foreach (var group in structure.Groups.Take(20)) // Limit to avoid diagram clutter
        {
            var groupName = SanitizeNodeName($"{group.GroupType}_{groupIndex}");
            var displayText = $"{group.GroupType}: {group.Name}";
            if (displayText.Length > 30)
                displayText = displayText.Substring(0, 27) + "...";

            mermaid.AppendLine($"    {groupName}[\"{displayText}\\n{group.ControlCount} controls\"]");
            mermaid.AppendLine($"    {rootName} --> {groupName}");

            // Style by group type
            var fillColor = GetGroupColor(group.GroupType);
            mermaid.AppendLine($"    style {groupName} fill:{fillColor},stroke:#333");
            mermaid.AppendLine();

            groupIndex++;
        }

        mermaid.AppendLine("```");
        mermaid.AppendLine();
        mermaid.AppendLine("## Structure Statistics");
        mermaid.AppendLine($"- **Control Groups**: {structure.Groups.Count}");
        mermaid.AppendLine($"- **Max Nesting Depth**: {structure.MaxDepth}");
        mermaid.AppendLine();
        mermaid.AppendLine("### Control Type Distribution");
        foreach (var type in structure.ControlTypeDistribution.OrderByDescending(kv => kv.Value).Take(10))
        {
            mermaid.AppendLine($"- **{type.Key}**: {type.Value}");
        }

        File.WriteAllText(outputPath, mermaid.ToString());
        Console.WriteLine($"Structure diagram written to: {outputPath}");
    }

    public void GenerateComplexityHeatmap(UIExtractionResult result, string outputPath)
    {
        var mermaid = new StringBuilder();
        mermaid.AppendLine("# Form Complexity Heatmap");
        mermaid.AppendLine();
        mermaid.AppendLine("```mermaid");
        mermaid.AppendLine("%%{init: {'theme':'base', 'themeVariables': { 'fontSize':'16px'}}}%%");
        mermaid.AppendLine("graph LR");
        mermaid.AppendLine();

        var sortedForms = result.Forms.Values
            .OrderByDescending(f => f.Controls.Count + f.MenuItems.Count + f.ToolbarButtons.Count)
            .Take(15)
            .ToList();

        foreach (var form in sortedForms)
        {
            var totalControls = form.Controls.Count + form.MenuItems.Count + form.ToolbarButtons.Count;
            var nodeName = SanitizeNodeName(form.FormName);
            var color = GetComplexityColor(totalControls);

            mermaid.AppendLine($"    {nodeName}[\"{form.FormName}\\n{totalControls} controls\"]");
            mermaid.AppendLine($"    style {nodeName} fill:{color},stroke:#333,stroke-width:2px");
        }

        mermaid.AppendLine("```");
        mermaid.AppendLine();
        mermaid.AppendLine("## Complexity Legend");
        mermaid.AppendLine("- **Red (2000+)**: Extremely complex - requires major refactoring");
        mermaid.AppendLine("- **Orange (1000-1999)**: Very complex - needs careful planning");
        mermaid.AppendLine("- **Yellow (500-999)**: Complex - moderate effort");
        mermaid.AppendLine("- **Light Green (100-499)**: Moderate - straightforward migration");
        mermaid.AppendLine("- **Green (<100)**: Simple - quick migration");

        File.WriteAllText(outputPath, mermaid.ToString());
        Console.WriteLine($"Complexity heatmap written to: {outputPath}");
    }

    private string SanitizeNodeName(string name)
    {
        // Mermaid node names must be valid identifiers
        return name.Replace(".", "_").Replace("-", "_").Replace(" ", "_");
    }

    private string GetEventLabel(string eventName)
    {
        // Shorten common event names for diagram readability
        if (eventName.Contains("Click")) return "Click";
        if (eventName.Contains("Changed")) return "Change";
        if (eventName.Contains("Load")) return "Load";
        if (eventName.Contains("Closing")) return "Close";
        return "Event";
    }

    private string GetGroupColor(string groupType)
    {
        return groupType switch
        {
            "TabControl" => "#FFD700",    // Gold
            "TabPage" => "#FFA500",       // Orange
            "Panel" => "#87CEEB",         // Sky Blue
            "GroupBox" => "#98FB98",      // Pale Green
            "SplitContainer" => "#DDA0DD", // Plum
            _ => "#D3D3D3"                // Light Gray
        };
    }

    private string GetComplexityColor(int controlCount)
    {
        if (controlCount >= 2000) return "#FF4444"; // Red
        if (controlCount >= 1000) return "#FF8C00"; // Dark Orange
        if (controlCount >= 500) return "#FFD700";  // Gold
        if (controlCount >= 100) return "#90EE90";  // Light Green
        return "#00FF00";                            // Green
    }
}
