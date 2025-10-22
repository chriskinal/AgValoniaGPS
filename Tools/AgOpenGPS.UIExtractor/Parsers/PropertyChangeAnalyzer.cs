using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Analyzes dynamic property changes (Text, BackColor, Image, etc.)
/// </summary>
public class PropertyChangeAnalyzer
{
    private static readonly HashSet<string> TrackedProperties = new()
    {
        "Text", "BackColor", "ForeColor", "Image", "Enabled",
        "BackgroundImage", "Font", "Size", "Location", "Checked"
    };

    public List<PropertyChange> AnalyzeFile(string filePath)
    {
        var changes = new List<PropertyChange>();

        if (!File.Exists(filePath))
            return changes;

        try
        {
            var code = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // Find all assignment expressions
            var assignments = root.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>();

            foreach (var assignment in assignments)
            {
                var propertyChange = ExtractPropertyChange(assignment, filePath);
                if (propertyChange != null)
                    changes.Add(propertyChange);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to analyze {filePath}: {ex.Message}");
        }

        return changes;
    }

    private PropertyChange? ExtractPropertyChange(AssignmentExpressionSyntax assignment, string filePath)
    {
        try
        {
            var leftSide = assignment.Left.ToString();

            // Check if it's a tracked property
            var propertyName = GetPropertyName(leftSide);
            if (string.IsNullOrEmpty(propertyName) || !TrackedProperties.Contains(propertyName))
                return null;

            var change = new PropertyChange
            {
                PropertyName = propertyName,
                SourceFile = Path.GetFileName(filePath),
                LineNumber = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };

            // Extract control name
            change.ControlName = ExtractControlName(leftSide);

            // Extract new value
            change.NewValue = assignment.Right.ToString();

            // Simplify string literals
            if (change.NewValue.StartsWith("\"") && change.NewValue.EndsWith("\""))
            {
                change.NewValue = change.NewValue.Trim('"');
            }

            // Find the containing method for context
            var method = assignment.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            change.Context = method?.Identifier.Text ?? "Unknown";

            // Extract condition from surrounding if statement
            var ifStatement = assignment.Ancestors().OfType<IfStatementSyntax>().FirstOrDefault();
            if (ifStatement != null)
            {
                change.Condition = ifStatement.Condition.ToString();
                change.DependsOnVariables = ExtractVariables(ifStatement.Condition.ToString());
            }
            else
            {
                // Check for switch statement
                var switchSection = assignment.Ancestors().OfType<SwitchSectionSyntax>().FirstOrDefault();
                if (switchSection != null)
                {
                    var switchStatement = assignment.Ancestors().OfType<SwitchStatementSyntax>().FirstOrDefault();
                    if (switchStatement != null)
                    {
                        var caseLabel = switchSection.Labels.FirstOrDefault()?.ToString() ?? "";
                        change.Condition = $"{switchStatement.Expression} == {caseLabel}";
                        change.DependsOnVariables = ExtractVariables(switchStatement.Expression.ToString());
                    }
                }
                else
                {
                    change.Condition = "Unconditional";
                }
            }

            return change;
        }
        catch
        {
            return null;
        }
    }

    private string GetPropertyName(string memberAccess)
    {
        var parts = memberAccess.Split('.');
        if (parts.Length > 0)
        {
            var lastPart = parts[^1];
            if (TrackedProperties.Contains(lastPart))
                return lastPart;
        }
        return string.Empty;
    }

    private string ExtractControlName(string memberAccess)
    {
        // Handle patterns like:
        // btn.Text
        // this.btn.Text
        // panel1.btn.Text
        var parts = memberAccess.Split('.');
        if (parts.Length >= 2)
        {
            return parts[^2]; // Second to last part before .PropertyName
        }
        return memberAccess;
    }

    private List<string> ExtractVariables(string expression)
    {
        var variables = new List<string>();

        // Simple heuristic: find identifiers that look like variables
        var identifierPattern = new System.Text.RegularExpressions.Regex(@"\b([a-z][a-zA-Z0-9_]*)\b");
        var matches = identifierPattern.Matches(expression);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var identifier = match.Groups[1].Value;
            if (!IsKeyword(identifier))
            {
                if (!variables.Contains(identifier))
                    variables.Add(identifier);
            }
        }

        return variables;
    }

    private bool IsKeyword(string identifier)
    {
        var keywords = new HashSet<string>
        {
            "if", "else", "true", "false", "null", "this", "new", "return",
            "var", "int", "bool", "string", "double", "float", "void"
        };
        return keywords.Contains(identifier);
    }

    public Dictionary<string, List<PropertyChange>> GroupByControl(List<PropertyChange> changes)
    {
        return changes
            .GroupBy(c => c.ControlName)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public Dictionary<string, int> GetPropertyChangeFrequency(List<PropertyChange> changes)
    {
        return changes
            .GroupBy(c => c.PropertyName)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
