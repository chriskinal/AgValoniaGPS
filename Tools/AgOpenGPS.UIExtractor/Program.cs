using AgOpenGPS.UIExtractor.Models;
using AgOpenGPS.UIExtractor.Parsers;
using AgOpenGPS.UIExtractor.Output;

namespace AgOpenGPS.UIExtractor;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("AgOpenGPS UI Extractor - Phase 1");
        Console.WriteLine("=================================");
        Console.WriteLine();

        // Get the path to the GPS Forms directory
        string formsPath;
        if (args.Length > 0)
        {
            formsPath = args[0];
        }
        else
        {
            // Default path relative to the tool
            formsPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../SourceCode/GPS/Forms"));
        }

        if (!Directory.Exists(formsPath))
        {
            Console.WriteLine($"Error: Forms directory not found: {formsPath}");
            Console.WriteLine();
            Console.WriteLine("Usage: AgOpenGPS.UIExtractor [path-to-GPS-Forms-directory]");
            return;
        }

        Console.WriteLine($"Scanning directory: {formsPath}");
        Console.WriteLine();

        // Find all Designer.cs files
        var designerFiles = Directory.GetFiles(formsPath, "*.Designer.cs", SearchOption.AllDirectories);
        Console.WriteLine($"Found {designerFiles.Length} Designer.cs files");
        Console.WriteLine();

        // Create output directories
        var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
        var imageDir = Path.Combine(outputDir, "images");
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(imageDir);

        // Parse each file
        var designerParser = new DesignerParser();
        var codeBehindParser = new CodeBehindParser();
        var resourceParser = new ResourceParser(imageDir);
        var result = new UIExtractionResult();

        int successCount = 0;
        int errorCount = 0;

        foreach (var file in designerFiles)
        {
            try
            {
                Console.Write($"Parsing {Path.GetFileName(file)}...");
                var formInfo = designerParser.ParseDesignerFile(file);

                // Try to find and parse corresponding .cs file for event handlers
                var codeBehindPath = file.Replace(".Designer.cs", ".cs");
                if (File.Exists(codeBehindPath))
                {
                    codeBehindParser.EnhanceFormWithEventHandlers(codeBehindPath, formInfo);
                }

                // Parse .resx files for resources
                var resxFiles = resourceParser.FindResxFiles(file);
                if (resxFiles.Count > 0)
                {
                    // Parse the main .resx file (first one)
                    formInfo.Resources = resourceParser.ParseResourceFile(resxFiles[0], formInfo.FormName);
                }

                result.Forms[formInfo.FormName] = formInfo;
                Console.WriteLine(" ✓");
                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" ✗ Error: {ex.Message}");
                errorCount++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Successfully parsed: {successCount}/{designerFiles.Length}");
        if (errorCount > 0)
        {
            Console.WriteLine($"Errors: {errorCount}");
        }
        Console.WriteLine();

        // Calculate statistics
        result.Statistics.TotalForms = result.Forms.Count;
        result.Statistics.TotalMenuItems = result.Forms.Values.Sum(f => f.MenuItems.Count);
        result.Statistics.TotalToolbarButtons = result.Forms.Values.Sum(f => f.ToolbarButtons.Count);
        result.Statistics.TotalControls = result.Forms.Values.Sum(f => f.Controls.Count);
        result.Statistics.TotalEventHandlers = result.Forms.Values.Sum(f => f.EventHandlers.Count);
        result.Statistics.TotalStateVariables = result.Forms.Values.Sum(f => f.StateVariables.Count);
        result.Statistics.TotalKeyboardShortcuts = result.Forms.Values.Sum(f => f.KeyboardShortcuts.Count);
        result.Statistics.TotalImages = result.Forms.Values.Sum(f => f.Resources?.Images.Count ?? 0);
        result.Statistics.TotalStringResources = result.Forms.Values.Sum(f => f.Resources?.Strings.Count ?? 0);

        // Phase 4: Analyze navigation and form structures
        Console.WriteLine("Analyzing form navigation and structures...");
        var navigationAnalyzer = new NavigationAnalyzer();

        result.NavigationGraph = navigationAnalyzer.BuildNavigationGraph(result.Forms);

        // Analyze structure of complex forms (>500 controls)
        foreach (var form in result.Forms.Values.Where(f => f.Controls.Count > 500))
        {
            result.FormStructures[form.FormName] = navigationAnalyzer.AnalyzeFormStructure(form);
        }

        Console.WriteLine($"  - Found {result.NavigationGraph.Navigations.Count} form navigations");
        Console.WriteLine($"  - Identified {result.NavigationGraph.EntryPoints.Count} entry point(s)");
        Console.WriteLine($"  - Analyzed {result.FormStructures.Count} complex form(s)");
        Console.WriteLine();

        // Phase 5: Generate output files
        Console.WriteLine("Generating output files...");

        var jsonGenerator = new JsonGenerator();
        var jsonPath = Path.Combine(outputDir, "ui-structure.json");
        var summaryPath = Path.Combine(outputDir, "UI-EXTRACTION-SUMMARY.md");

        jsonGenerator.GenerateJson(result, jsonPath);
        jsonGenerator.GenerateSummaryReport(result, summaryPath);

        // Mermaid diagrams
        var mermaidGenerator = new MermaidGenerator();
        var navDiagramPath = Path.Combine(outputDir, "navigation-graph.md");
        var complexityHeatmapPath = Path.Combine(outputDir, "complexity-heatmap.md");

        mermaidGenerator.GenerateNavigationDiagram(result, navDiagramPath);
        mermaidGenerator.GenerateComplexityHeatmap(result, complexityHeatmapPath);

        // Complex form structure diagrams
        int diagramCount = 0;
        foreach (var structure in result.FormStructures.Values)
        {
            var structurePath = Path.Combine(outputDir, $"form-structure-{structure.FormName}.md");
            mermaidGenerator.GenerateFormStructureDiagram(structure, structurePath);
            diagramCount++;
        }

        // Avalonia migration guide
        var avaloniaGenerator = new AvaloniaGuideGenerator();
        var migrationGuidePath = Path.Combine(outputDir, "AVALONIA-MIGRATION-GUIDE.md");
        avaloniaGenerator.GenerateMigrationGuide(result, migrationGuidePath);

        // Interactive HTML documentation
        var htmlGenerator = new HtmlDocGenerator();
        var htmlPath = Path.Combine(outputDir, "ui-documentation.html");
        htmlGenerator.GenerateInteractiveDoc(result, htmlPath);

        Console.WriteLine();
        Console.WriteLine("Extraction complete!");
        Console.WriteLine();
        Console.WriteLine("Output files:");
        Console.WriteLine($"  - JSON: {jsonPath}");
        Console.WriteLine($"  - Summary: {summaryPath}");
        Console.WriteLine($"  - Navigation Diagram: {navDiagramPath}");
        Console.WriteLine($"  - Complexity Heatmap: {complexityHeatmapPath}");
        Console.WriteLine($"  - Form Structure Diagrams: {diagramCount} files");
        Console.WriteLine($"  - Migration Guide: {migrationGuidePath}");
        Console.WriteLine($"  - Interactive HTML: {htmlPath}");
        Console.WriteLine();

        // Show top-level statistics
        Console.WriteLine("Top 10 Forms by Control Count:");
        foreach (var form in result.Forms.Values
            .OrderByDescending(f => f.Controls.Count + f.MenuItems.Count + f.ToolbarButtons.Count)
            .Take(10))
        {
            var totalControls = form.Controls.Count + form.MenuItems.Count + form.ToolbarButtons.Count;
            Console.WriteLine($"  {form.FormName}: {totalControls} total controls " +
                             $"({form.MenuItems.Count} menu, {form.ToolbarButtons.Count} toolbar, {form.Controls.Count} other)");
        }
    }
}
