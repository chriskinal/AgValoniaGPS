using AgOpenGPS.UIExtractor.Models;
using System.Text.RegularExpressions;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Analyzes form navigation patterns and builds navigation graph
/// </summary>
public class NavigationAnalyzer
{
    public NavigationGraph BuildNavigationGraph(Dictionary<string, FormInfo> forms)
    {
        var graph = new NavigationGraph();

        foreach (var form in forms.Values)
        {
            // Analyze event handlers for form opens
            foreach (var handler in form.EventHandlers)
            {
                var targetForms = ExtractTargetForms(handler.Value);

                foreach (var targetForm in targetForms)
                {
                    var navigation = new FormNavigation
                    {
                        SourceForm = form.FormName,
                        TargetForm = targetForm,
                        TriggerEvent = handler.Key,
                        Action = handler.Value
                    };

                    graph.Navigations.Add(navigation);

                    // Build parent-child relationships
                    if (!graph.FormChildren.ContainsKey(form.FormName))
                    {
                        graph.FormChildren[form.FormName] = new List<string>();
                    }
                    if (!graph.FormChildren[form.FormName].Contains(targetForm))
                    {
                        graph.FormChildren[form.FormName].Add(targetForm);
                    }

                    if (!graph.FormParents.ContainsKey(targetForm))
                    {
                        graph.FormParents[targetForm] = new List<string>();
                    }
                    if (!graph.FormParents[targetForm].Contains(form.FormName))
                    {
                        graph.FormParents[targetForm].Add(form.FormName);
                    }
                }
            }
        }

        // Find entry points (forms with no parents)
        foreach (var formName in forms.Keys)
        {
            if (!graph.FormParents.ContainsKey(formName) || graph.FormParents[formName].Count == 0)
            {
                graph.EntryPoints.Add(formName);
            }
        }

        return graph;
    }

    private List<string> ExtractTargetForms(string actionDescription)
    {
        var forms = new List<string>();

        // Look for "Opens FormName" or "new FormName"
        var formPattern = new Regex(@"(?:Opens|new)\s+(Form\w+)");
        var matches = formPattern.Matches(actionDescription);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                forms.Add(match.Groups[1].Value);
            }
        }

        return forms;
    }

    public FormStructure AnalyzeFormStructure(FormInfo form)
    {
        var structure = new FormStructure
        {
            FormName = form.FormName
        };

        // Analyze control groupings
        var groupingControls = form.Controls
            .Where(c => IsGroupingControl(c.Type))
            .ToList();

        foreach (var control in groupingControls)
        {
            var group = new ControlGroup
            {
                GroupType = GetGroupType(control.Type),
                Name = control.Name,
                ControlCount = CountChildControls(control),
                ChildControls = GetChildControlNames(control)
            };
            structure.Groups.Add(group);
        }

        // Calculate control type distribution
        var distribution = form.Controls
            .GroupBy(c => GetSimplifiedType(c.Type))
            .ToDictionary(g => g.Key, g => g.Count());

        structure.ControlTypeDistribution = distribution;

        // Calculate max depth by analyzing nested groups
        structure.MaxDepth = CalculateMaxDepth(form);

        return structure;
    }

    private bool IsGroupingControl(string type)
    {
        return type.Contains("TabControl") ||
               type.Contains("TabPage") ||
               type.Contains("Panel") ||
               type.Contains("GroupBox") ||
               type.Contains("SplitContainer");
    }

    private string GetGroupType(string type)
    {
        if (type.Contains("TabControl")) return "TabControl";
        if (type.Contains("TabPage")) return "TabPage";
        if (type.Contains("Panel")) return "Panel";
        if (type.Contains("GroupBox")) return "GroupBox";
        if (type.Contains("SplitContainer")) return "SplitContainer";
        return "Unknown";
    }

    private int CountChildControls(UIElement control)
    {
        return control.Children.Count + control.Children.Sum(c => CountChildControls(c));
    }

    private List<string> GetChildControlNames(UIElement control)
    {
        var names = new List<string>();
        foreach (var child in control.Children)
        {
            names.Add(child.Name);
            names.AddRange(GetChildControlNames(child));
        }
        return names;
    }

    private string GetSimplifiedType(string fullType)
    {
        // Extract just the class name from fully qualified type
        var parts = fullType.Split('.');
        return parts.Last().Replace("System.Windows.Forms.", "");
    }

    private int CalculateMaxDepth(FormInfo form)
    {
        int maxDepth = 0;
        foreach (var control in form.Controls)
        {
            int depth = CalculateControlDepth(control, 1);
            if (depth > maxDepth)
            {
                maxDepth = depth;
            }
        }
        return maxDepth;
    }

    private int CalculateControlDepth(UIElement control, int currentDepth)
    {
        if (control.Children.Count == 0)
        {
            return currentDepth;
        }

        int maxChildDepth = currentDepth;
        foreach (var child in control.Children)
        {
            int childDepth = CalculateControlDepth(child, currentDepth + 1);
            if (childDepth > maxChildDepth)
            {
                maxChildDepth = childDepth;
            }
        }

        return maxChildDepth;
    }
}
