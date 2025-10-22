using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Analyzes dynamic visibility changes in code
/// </summary>
public class DynamicVisibilityAnalyzer
{
    public List<VisibilityRule> AnalyzeFile(string filePath)
    {
        var rules = new List<VisibilityRule>();

        if (!File.Exists(filePath))
            return rules;

        try
        {
            var code = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // Find all assignment expressions
            var assignments = root.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(a => a.Left.ToString().Contains(".Visible"));

            foreach (var assignment in assignments)
            {
                var rule = ExtractVisibilityRule(assignment, filePath);
                if (rule != null)
                    rules.Add(rule);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to analyze {filePath}: {ex.Message}");
        }

        return rules;
    }

    private VisibilityRule? ExtractVisibilityRule(AssignmentExpressionSyntax assignment, string filePath)
    {
        try
        {
            var rule = new VisibilityRule
            {
                SourceFile = Path.GetFileName(filePath),
                LineNumber = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };

            // Extract control name
            var leftSide = assignment.Left.ToString();
            rule.ControlName = ExtractControlName(leftSide);

            // Extract target visibility
            var rightSide = assignment.Right.ToString();
            rule.TargetVisibility = !rightSide.Contains("false");

            // Find the containing method for context
            var method = assignment.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            rule.Context = method?.Identifier.Text ?? "Unknown";

            // Extract condition from surrounding if statement
            var ifStatement = assignment.Ancestors().OfType<IfStatementSyntax>().FirstOrDefault();
            if (ifStatement != null)
            {
                rule.Condition = ifStatement.Condition.ToString();
                rule.DependsOnVariables = ExtractVariables(ifStatement.Condition.ToString());
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
                        rule.Condition = $"{switchStatement.Expression} == {caseLabel}";
                        rule.DependsOnVariables = ExtractVariables(switchStatement.Expression.ToString());
                    }
                }
                else
                {
                    rule.Condition = "Unconditional";
                }
            }

            return rule;
        }
        catch
        {
            return null;
        }
    }

    private string ExtractControlName(string memberAccess)
    {
        // Handle patterns like:
        // btn.Visible
        // this.btn.Visible
        // panel1.btn.Visible
        var parts = memberAccess.Split('.');
        if (parts.Length >= 2)
        {
            return parts[^2]; // Second to last part before .Visible
        }
        return memberAccess;
    }

    private List<string> ExtractVariables(string expression)
    {
        var variables = new List<string>();

        // Simple heuristic: find identifiers that look like variables
        // Match patterns like: variable, this.variable, variable.property
        var identifierPattern = new System.Text.RegularExpressions.Regex(@"\b([a-z][a-zA-Z0-9_]*)\b");
        var matches = identifierPattern.Matches(expression);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var identifier = match.Groups[1].Value;
            // Skip common keywords and method names
            if (!IsKeyword(identifier) && !identifier.StartsWith("is") && !identifier.StartsWith("has"))
            {
                if (!variables.Contains(identifier))
                    variables.Add(identifier);
            }
            else if (identifier.StartsWith("is") || identifier.StartsWith("has"))
            {
                // These are likely boolean state variables
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
            "var", "int", "bool", "string", "double", "float", "void",
            "public", "private", "protected", "static", "const", "readonly"
        };
        return keywords.Contains(identifier);
    }

    public Dictionary<string, List<string>> BuildVariableUsageMap(List<VisibilityRule> rules)
    {
        var map = new Dictionary<string, List<string>>();

        foreach (var rule in rules)
        {
            foreach (var variable in rule.DependsOnVariables)
            {
                if (!map.ContainsKey(variable))
                    map[variable] = new List<string>();

                if (!map[variable].Contains(rule.ControlName))
                    map[variable].Add(rule.ControlName);
            }
        }

        return map;
    }
}
