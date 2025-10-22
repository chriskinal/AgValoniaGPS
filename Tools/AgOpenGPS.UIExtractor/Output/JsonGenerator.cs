using System.Text.Json;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Output;

/// <summary>
/// Generates JSON output from extracted UI data
/// </summary>
public class JsonGenerator
{
    public void GenerateJson(UIExtractionResult result, string outputPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(result, options);
        File.WriteAllText(outputPath, json);

        Console.WriteLine($"JSON output written to: {outputPath}");
    }

    public void GenerateSummaryReport(UIExtractionResult result, string outputPath)
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("# AgOpenGPS UI Extraction Summary");
        summary.AppendLine();
        summary.AppendLine($"**Extraction Date**: {result.ExtractedDate:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine($"**Source**: {result.Source}");
        summary.AppendLine();

        // Add navigation insights if available
        if (result.NavigationGraph != null)
        {
            summary.AppendLine("## Navigation Insights");
            summary.AppendLine();
            summary.AppendLine($"- Total Form Navigations: {result.NavigationGraph.Navigations.Count}");
            summary.AppendLine($"- Entry Points: {result.NavigationGraph.EntryPoints.Count} ({string.Join(", ", result.NavigationGraph.EntryPoints.Take(5))})");
            summary.AppendLine($"- Forms with Children: {result.NavigationGraph.FormChildren.Count}");
            summary.AppendLine();

            if (result.FormStructures.Count > 0)
            {
                summary.AppendLine("## Complex Forms Analysis");
                summary.AppendLine();
                foreach (var structure in result.FormStructures.Values.OrderByDescending(s => s.Groups.Count))
                {
                    summary.AppendLine($"### {structure.FormName}");
                    summary.AppendLine($"- Control Groups: {structure.Groups.Count}");
                    summary.AppendLine($"- Max Nesting Depth: {structure.MaxDepth}");
                    summary.AppendLine($"- Top Control Types:");
                    foreach (var type in structure.ControlTypeDistribution.OrderByDescending(kv => kv.Value).Take(5))
                    {
                        summary.AppendLine($"  - {type.Key}: {type.Value}");
                    }
                    summary.AppendLine();
                }
            }
        }
        summary.AppendLine();
        summary.AppendLine("## Statistics");
        summary.AppendLine();
        summary.AppendLine($"- Total Forms: {result.Statistics.TotalForms}");
        summary.AppendLine($"- Total Menu Items: {result.Statistics.TotalMenuItems}");
        summary.AppendLine($"- Total Toolbar Buttons: {result.Statistics.TotalToolbarButtons}");
        summary.AppendLine($"- Total Controls: {result.Statistics.TotalControls}");
        summary.AppendLine($"- Total Event Handlers: {result.Statistics.TotalEventHandlers}");
        summary.AppendLine($"- Total State Variables: {result.Statistics.TotalStateVariables}");
        summary.AppendLine($"- Total Keyboard Shortcuts: {result.Statistics.TotalKeyboardShortcuts}");
        summary.AppendLine($"- Total Images: {result.Statistics.TotalImages}");
        summary.AppendLine($"- Total String Resources: {result.Statistics.TotalStringResources}");
        summary.AppendLine();
        summary.AppendLine("## Forms");
        summary.AppendLine();

        foreach (var form in result.Forms.Values.OrderBy(f => f.FormName))
        {
            summary.AppendLine($"### {form.FormName}");
            if (!string.IsNullOrEmpty(form.Title))
            {
                summary.AppendLine($"**Title**: {form.Title}");
            }
            if (form.Size != null)
            {
                summary.AppendLine($"**Size**: {form.Size.Width} x {form.Size.Height}");
            }
            summary.AppendLine($"- Menu Items: {form.MenuItems.Count}");
            summary.AppendLine($"- Toolbar Buttons: {form.ToolbarButtons.Count}");
            summary.AppendLine($"- Controls: {form.Controls.Count}");
            summary.AppendLine($"- Event Handlers: {form.EventHandlers.Count}");
            summary.AppendLine($"- State Variables: {form.StateVariables.Count}");
            summary.AppendLine($"- Keyboard Shortcuts: {form.KeyboardShortcuts.Count}");
            summary.AppendLine($"- Images: {form.Resources?.Images.Count ?? 0}");
            summary.AppendLine($"- String Resources: {form.Resources?.Strings.Count ?? 0}");
            summary.AppendLine();
        }

        File.WriteAllText(outputPath, summary.ToString());
        Console.WriteLine($"Summary report written to: {outputPath}");
    }
}
