using System.Text;
using System.Web;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Output;

/// <summary>
/// Generates interactive HTML documentation
/// </summary>
public class HtmlDocGenerator
{
    public void GenerateInteractiveDoc(UIExtractionResult result, string outputPath)
    {
        var html = new StringBuilder();

        // HTML head with styling and JavaScript
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>AgOpenGPS UI Documentation</title>");
        html.AppendLine("    <style>");
        html.AppendLine(GetCssStyles());
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Header
        html.AppendLine("    <header>");
        html.AppendLine("        <h1>AgOpenGPS UI Documentation</h1>");
        html.AppendLine($"        <p>Extracted: {result.ExtractedDate:yyyy-MM-dd HH:mm:ss} | {result.Forms.Count} Forms | {result.Statistics.TotalControls:N0} Controls</p>");
        html.AppendLine("    </header>");

        // Navigation
        html.AppendLine("    <nav>");
        html.AppendLine("        <a href=\"#overview\">Overview</a>");
        html.AppendLine("        <a href=\"#forms\">Forms</a>");
        html.AppendLine("        <a href=\"#navigation\">Navigation</a>");
        html.AppendLine("        <a href=\"#statistics\">Statistics</a>");
        html.AppendLine("    </nav>");

        // Main content
        html.AppendLine("    <div class=\"container\">");

        // Overview section
        GenerateOverviewSection(result, html);

        // Forms list with search
        GenerateFormsSection(result, html);

        // Navigation graph section
        GenerateNavigationSection(result, html);

        // Statistics section
        GenerateStatisticsSection(result, html);

        html.AppendLine("    </div>");

        // JavaScript for interactivity
        html.AppendLine("    <script>");
        html.AppendLine(GetJavaScript());
        html.AppendLine("    </script>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        File.WriteAllText(outputPath, html.ToString());
        Console.WriteLine($"Interactive HTML documentation written to: {outputPath}");
    }

    private void GenerateOverviewSection(UIExtractionResult result, StringBuilder html)
    {
        html.AppendLine("        <section id=\"overview\" class=\"card\">");
        html.AppendLine("            <h2>Overview</h2>");
        html.AppendLine("            <div class=\"stats-grid\">");
        html.AppendLine($"                <div class=\"stat-card\"><div class=\"stat-value\">{result.Forms.Count}</div><div class=\"stat-label\">Forms</div></div>");
        html.AppendLine($"                <div class=\"stat-card\"><div class=\"stat-value\">{result.Statistics.TotalControls:N0}</div><div class=\"stat-label\">Controls</div></div>");
        html.AppendLine($"                <div class=\"stat-card\"><div class=\"stat-value\">{result.Statistics.TotalEventHandlers}</div><div class=\"stat-label\">Event Handlers</div></div>");
        html.AppendLine($"                <div class=\"stat-card\"><div class=\"stat-value\">{result.Statistics.TotalMenuItems}</div><div class=\"stat-label\">Menu Items</div></div>");
        html.AppendLine("            </div>");

        // Complexity breakdown
        var complexForms = result.Forms.Values.Count(f => f.Controls.Count > 500);
        var moderateForms = result.Forms.Values.Count(f => f.Controls.Count >= 100 && f.Controls.Count <= 500);
        var simpleForms = result.Forms.Values.Count(f => f.Controls.Count < 100);

        html.AppendLine("            <h3>Complexity Breakdown</h3>");
        html.AppendLine("            <div class=\"complexity-bar\">");
        var complexPct = (double)complexForms / result.Forms.Count * 100;
        var moderatePct = (double)moderateForms / result.Forms.Count * 100;
        var simplePct = (double)simpleForms / result.Forms.Count * 100;
        html.AppendLine($"                <div class=\"complexity-segment complex\" style=\"width: {complexPct:F1}%\" title=\"Complex: {complexForms} forms\"></div>");
        html.AppendLine($"                <div class=\"complexity-segment moderate\" style=\"width: {moderatePct:F1}%\" title=\"Moderate: {moderateForms} forms\"></div>");
        html.AppendLine($"                <div class=\"complexity-segment simple\" style=\"width: {simplePct:F1}%\" title=\"Simple: {simpleForms} forms\"></div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div class=\"complexity-legend\">");
        html.AppendLine($"                <span class=\"legend-item\"><span class=\"legend-color complex\"></span> Complex (>500): {complexForms}</span>");
        html.AppendLine($"                <span class=\"legend-item\"><span class=\"legend-color moderate\"></span> Moderate (100-500): {moderateForms}</span>");
        html.AppendLine($"                <span class=\"legend-item\"><span class=\"legend-color simple\"></span> Simple (<100): {simpleForms}</span>");
        html.AppendLine("            </div>");
        html.AppendLine("        </section>");
    }

    private void GenerateFormsSection(UIExtractionResult result, StringBuilder html)
    {
        html.AppendLine("        <section id=\"forms\" class=\"card\">");
        html.AppendLine("            <h2>Forms</h2>");
        html.AppendLine("            <input type=\"text\" id=\"searchBox\" placeholder=\"Search forms...\" onkeyup=\"filterForms()\">");

        html.AppendLine("            <div class=\"forms-list\" id=\"formsList\">");

        var sortedForms = result.Forms.Values
            .OrderBy(f => f.FormName)
            .ToList();

        foreach (var form in sortedForms)
        {
            var totalControls = form.Controls.Count + form.MenuItems.Count + form.ToolbarButtons.Count;
            var complexity = GetComplexityClass(totalControls);

            html.AppendLine($"                <div class=\"form-item {complexity}\" data-name=\"{HttpUtility.HtmlEncode(form.FormName.ToLower())}\">");
            html.AppendLine($"                    <div class=\"form-header\" onclick=\"toggleForm('{form.FormName}')\">");
            html.AppendLine($"                        <h3>{HttpUtility.HtmlEncode(form.FormName)} <span class=\"expand-icon\">▼</span></h3>");
            html.AppendLine($"                        <div class=\"form-meta\">");

            if (!string.IsNullOrEmpty(form.Title))
            {
                html.AppendLine($"                            <span class=\"form-title\">Title: {HttpUtility.HtmlEncode(form.Title)}</span>");
            }

            html.AppendLine($"                            <span class=\"badge\">{totalControls} controls</span>");
            html.AppendLine($"                            <span class=\"badge\">{form.EventHandlers.Count} handlers</span>");

            if (form.Size != null)
            {
                html.AppendLine($"                            <span class=\"badge\">{form.Size.Width}×{form.Size.Height}</span>");
            }

            html.AppendLine($"                        </div>");
            html.AppendLine("                    </div>");

            html.AppendLine($"                    <div class=\"form-details\" id=\"form-{form.FormName}\" style=\"display: none;\">");

            // Controls breakdown
            if (form.MenuItems.Count > 0 || form.ToolbarButtons.Count > 0 || form.Controls.Count > 0)
            {
                html.AppendLine("                        <h4>Controls</h4>");
                html.AppendLine("                        <ul>");
                if (form.MenuItems.Count > 0)
                    html.AppendLine($"                            <li>Menu Items: {form.MenuItems.Count}</li>");
                if (form.ToolbarButtons.Count > 0)
                    html.AppendLine($"                            <li>Toolbar Buttons: {form.ToolbarButtons.Count}</li>");
                if (form.Controls.Count > 0)
                    html.AppendLine($"                            <li>Other Controls: {form.Controls.Count}</li>");
                html.AppendLine("                        </ul>");
            }

            // Event handlers
            if (form.EventHandlers.Count > 0)
            {
                html.AppendLine("                        <h4>Event Handlers</h4>");
                html.AppendLine("                        <ul class=\"event-list\">");
                foreach (var handler in form.EventHandlers.Take(10))
                {
                    html.AppendLine($"                            <li><code>{HttpUtility.HtmlEncode(handler.Key)}</code>: {HttpUtility.HtmlEncode(handler.Value)}</li>");
                }
                if (form.EventHandlers.Count > 10)
                {
                    html.AppendLine($"                            <li><em>... and {form.EventHandlers.Count - 10} more</em></li>");
                }
                html.AppendLine("                        </ul>");
            }

            // State variables
            if (form.StateVariables.Count > 0)
            {
                html.AppendLine("                        <h4>State Variables</h4>");
                html.AppendLine("                        <ul class=\"state-list\">");
                foreach (var state in form.StateVariables.Take(10))
                {
                    html.AppendLine($"                            <li><code>{HttpUtility.HtmlEncode(state.Key)}</code> ({state.Value.Type})</li>");
                }
                if (form.StateVariables.Count > 10)
                {
                    html.AppendLine($"                            <li><em>... and {form.StateVariables.Count - 10} more</em></li>");
                }
                html.AppendLine("                        </ul>");
            }

            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
        }

        html.AppendLine("            </div>");
        html.AppendLine("        </section>");
    }

    private void GenerateNavigationSection(UIExtractionResult result, StringBuilder html)
    {
        if (result.NavigationGraph == null)
            return;

        html.AppendLine("        <section id=\"navigation\" class=\"card\">");
        html.AppendLine("            <h2>Form Navigation</h2>");
        html.AppendLine($"            <p>{result.NavigationGraph.Navigations.Count} navigation relationships found</p>");

        html.AppendLine("            <h3>Entry Points</h3>");
        html.AppendLine("            <div class=\"tag-cloud\">");
        foreach (var entry in result.NavigationGraph.EntryPoints.Take(20))
        {
            html.AppendLine($"                <span class=\"tag entry-point\">{HttpUtility.HtmlEncode(entry)}</span>");
        }
        html.AppendLine("            </div>");

        html.AppendLine("            <h3>Navigation Relationships</h3>");
        html.AppendLine("            <table class=\"nav-table\">");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr><th>From</th><th>To</th><th>Trigger</th></tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var nav in result.NavigationGraph.Navigations.Take(50))
        {
            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td>{HttpUtility.HtmlEncode(nav.SourceForm)}</td>");
            html.AppendLine($"                        <td>{HttpUtility.HtmlEncode(nav.TargetForm)}</td>");
            html.AppendLine($"                        <td><code>{HttpUtility.HtmlEncode(nav.TriggerEvent)}</code></td>");
            html.AppendLine("                    </tr>");
        }

        if (result.NavigationGraph.Navigations.Count > 50)
        {
            html.AppendLine($"                    <tr><td colspan=\"3\"><em>... and {result.NavigationGraph.Navigations.Count - 50} more</em></td></tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </section>");
    }

    private void GenerateStatisticsSection(UIExtractionResult result, StringBuilder html)
    {
        html.AppendLine("        <section id=\"statistics\" class=\"card\">");
        html.AppendLine("            <h2>Statistics</h2>");

        // Top 10 forms by control count
        html.AppendLine("            <h3>Most Complex Forms</h3>");
        html.AppendLine("            <table class=\"stats-table\">");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr><th>Rank</th><th>Form</th><th>Total Controls</th><th>Event Handlers</th></tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        var topForms = result.Forms.Values
            .OrderByDescending(f => f.Controls.Count + f.MenuItems.Count + f.ToolbarButtons.Count)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var form in topForms)
        {
            var total = form.Controls.Count + form.MenuItems.Count + form.ToolbarButtons.Count;
            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td>{rank}</td>");
            html.AppendLine($"                        <td>{HttpUtility.HtmlEncode(form.FormName)}</td>");
            html.AppendLine($"                        <td>{total:N0}</td>");
            html.AppendLine($"                        <td>{form.EventHandlers.Count}</td>");
            html.AppendLine("                    </tr>");
            rank++;
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </section>");
    }

    private string GetComplexityClass(int controlCount)
    {
        if (controlCount >= 500) return "complex";
        if (controlCount >= 100) return "moderate";
        return "simple";
    }

    private string GetCssStyles()
    {
        return @"
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f5f5f5; }
        header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 2rem; text-align: center; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        header h1 { margin-bottom: 0.5rem; }
        header p { opacity: 0.9; }
        nav { background: white; box-shadow: 0 2px 5px rgba(0,0,0,0.1); padding: 1rem; text-align: center; position: sticky; top: 0; z-index: 100; }
        nav a { margin: 0 1rem; text-decoration: none; color: #667eea; font-weight: 500; transition: color 0.3s; }
        nav a:hover { color: #764ba2; }
        .container { max-width: 1200px; margin: 2rem auto; padding: 0 1rem; }
        .card { background: white; border-radius: 8px; padding: 2rem; margin-bottom: 2rem; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h2 { color: #333; margin-bottom: 1.5rem; border-bottom: 2px solid #667eea; padding-bottom: 0.5rem; }
        h3 { color: #555; margin: 1.5rem 0 1rem; }
        h4 { color: #666; margin: 1rem 0 0.5rem; }
        .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin: 1rem 0; }
        .stat-card { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 1.5rem; border-radius: 8px; text-align: center; }
        .stat-value { font-size: 2.5rem; font-weight: bold; margin-bottom: 0.5rem; }
        .stat-label { opacity: 0.9; text-transform: uppercase; font-size: 0.85rem; letter-spacing: 1px; }
        .complexity-bar { display: flex; height: 30px; border-radius: 15px; overflow: hidden; margin: 1rem 0; }
        .complexity-segment { transition: opacity 0.3s; }
        .complexity-segment:hover { opacity: 0.8; cursor: help; }
        .complexity-segment.complex { background: #ff4444; }
        .complexity-segment.moderate { background: #ffa500; }
        .complexity-segment.simple { background: #4caf50; }
        .complexity-legend { display: flex; gap: 1.5rem; margin-top: 0.5rem; flex-wrap: wrap; }
        .legend-item { display: flex; align-items: center; gap: 0.5rem; }
        .legend-color { width: 20px; height: 20px; border-radius: 4px; }
        #searchBox { width: 100%; padding: 0.75rem; margin-bottom: 1rem; border: 2px solid #ddd; border-radius: 4px; font-size: 1rem; }
        #searchBox:focus { outline: none; border-color: #667eea; }
        .forms-list { max-height: 600px; overflow-y: auto; }
        .form-item { border: 1px solid #ddd; border-radius: 4px; margin-bottom: 0.5rem; transition: all 0.3s; }
        .form-item.complex { border-left: 4px solid #ff4444; }
        .form-item.moderate { border-left: 4px solid #ffa500; }
        .form-item.simple { border-left: 4px solid #4caf50; }
        .form-header { padding: 1rem; cursor: pointer; user-select: none; }
        .form-header:hover { background: #f9f9f9; }
        .form-header h3 { margin: 0; display: flex; justify-content: space-between; align-items: center; color: #333; }
        .expand-icon { font-size: 0.8rem; transition: transform 0.3s; }
        .form-header.expanded .expand-icon { transform: rotate(180deg); }
        .form-meta { display: flex; gap: 0.75rem; margin-top: 0.5rem; flex-wrap: wrap; }
        .form-title { color: #666; font-style: italic; }
        .badge { background: #667eea; color: white; padding: 0.25rem 0.75rem; border-radius: 12px; font-size: 0.85rem; }
        .form-details { padding: 0 1rem 1rem 1rem; background: #f9f9f9; }
        .form-details ul { list-style-position: inside; margin-left: 1rem; }
        .form-details li { margin: 0.25rem 0; }
        code { background: #f4f4f4; padding: 0.2rem 0.4rem; border-radius: 3px; font-family: 'Courier New', monospace; font-size: 0.9rem; }
        .tag-cloud { display: flex; flex-wrap: wrap; gap: 0.5rem; }
        .tag { background: #e0e0e0; padding: 0.5rem 1rem; border-radius: 20px; font-size: 0.9rem; }
        .tag.entry-point { background: #4caf50; color: white; }
        table { width: 100%; border-collapse: collapse; margin-top: 1rem; }
        th, td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #f4f4f4; font-weight: 600; color: #333; }
        tr:hover { background: #f9f9f9; }
        ";
    }

    private string GetJavaScript()
    {
        return @"
        function filterForms() {
            const input = document.getElementById('searchBox');
            const filter = input.value.toLowerCase();
            const forms = document.querySelectorAll('.form-item');

            forms.forEach(form => {
                const name = form.getAttribute('data-name');
                form.style.display = name.includes(filter) ? '' : 'none';
            });
        }

        function toggleForm(formName) {
            const details = document.getElementById('form-' + formName);
            const header = details.previousElementSibling;

            if (details.style.display === 'none') {
                details.style.display = 'block';
                header.classList.add('expanded');
            } else {
                details.style.display = 'none';
                header.classList.remove('expanded');
            }
        }
        ";
    }
}
