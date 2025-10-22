using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Parses code-behind .cs files to extract event handlers and dynamic behavior
/// </summary>
public class CodeBehindParser
{
    public void EnhanceFormWithEventHandlers(string codeBehindPath, FormInfo formInfo)
    {
        if (!File.Exists(codeBehindPath))
        {
            return;
        }

        var sourceCode = File.ReadAllText(codeBehindPath);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot();

        // Find the class declaration
        var classDeclaration = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration == null) return;

        // Extract event handlers
        ExtractEventHandlers(classDeclaration, formInfo);

        // Extract state variables
        ExtractStateVariables(classDeclaration, formInfo);

        // Extract visibility logic
        ExtractVisibilityLogic(classDeclaration, formInfo);

        // Extract keyboard shortcuts
        ExtractKeyboardShortcuts(classDeclaration, formInfo);
    }

    private void ExtractEventHandlers(ClassDeclarationSyntax classDeclaration, FormInfo formInfo)
    {
        // Find all methods that look like event handlers
        var methods = classDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            var methodName = method.Identifier.Text;

            // Check if it's an event handler pattern (e.g., Button_Click, OnSomethingChanged)
            if (IsEventHandlerMethod(methodName, method))
            {
                var handlerInfo = AnalyzeEventHandler(method);
                formInfo.EventHandlers[methodName] = handlerInfo;
            }
        }
    }

    private bool IsEventHandlerMethod(string methodName, MethodDeclarationSyntax method)
    {
        // Common event handler patterns
        if (methodName.EndsWith("_Click") ||
            methodName.EndsWith("_Changed") ||
            methodName.EndsWith("_KeyDown") ||
            methodName.EndsWith("_KeyPress") ||
            methodName.StartsWith("On") ||
            methodName.Contains("Event"))
        {
            return true;
        }

        // Check parameters - typical event handler signature
        if (method.ParameterList.Parameters.Count == 2)
        {
            var param1 = method.ParameterList.Parameters[0].Type?.ToString();
            var param2 = method.ParameterList.Parameters[1].Type?.ToString();

            if (param1 == "object" && param2?.Contains("EventArgs") == true)
            {
                return true;
            }
        }

        return false;
    }

    private string AnalyzeEventHandler(MethodDeclarationSyntax method)
    {
        var actions = new List<string>();

        // Look for common patterns in the method body
        if (method.Body != null)
        {
            // Form opens: new FormName() or ShowDialog()
            var objectCreations = method.Body.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>();

            foreach (var creation in objectCreations)
            {
                var typeName = creation.Type.ToString();
                if (typeName.StartsWith("Form"))
                {
                    actions.Add($"Opens {typeName}");
                }
            }

            // ShowDialog calls
            var invocations = method.Body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                var expr = invocation.Expression.ToString();
                if (expr.Contains("ShowDialog"))
                {
                    actions.Add("Shows dialog");
                }
                else if (expr.Contains("Close"))
                {
                    actions.Add("Closes window");
                }
                else if (expr.Contains("Save"))
                {
                    actions.Add("Saves data");
                }
                else if (expr.Contains("Load"))
                {
                    actions.Add("Loads data");
                }
            }

            // Property changes that affect visibility
            var assignments = method.Body.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>();

            foreach (var assignment in assignments)
            {
                var left = assignment.Left.ToString();
                if (left.Contains(".Visible"))
                {
                    var controlName = left.Split('.')[0];
                    var value = assignment.Right.ToString();
                    actions.Add($"Sets {controlName}.Visible = {value}");
                }
                else if (left.Contains(".Enabled"))
                {
                    var controlName = left.Split('.')[0];
                    actions.Add($"Changes {controlName}.Enabled");
                }
            }
        }

        return actions.Count > 0 ? string.Join("; ", actions) : "No specific actions identified";
    }

    private void ExtractStateVariables(ClassDeclarationSyntax classDeclaration, FormInfo formInfo)
    {
        // Find field declarations that look like state variables
        var fields = classDeclaration.DescendantNodes()
            .OfType<FieldDeclarationSyntax>();

        foreach (var field in fields)
        {
            var variable = field.Declaration.Variables.FirstOrDefault();
            if (variable == null) continue;

            var fieldName = variable.Identifier.Text;
            var fieldType = field.Declaration.Type.ToString();

            // Common state variable patterns
            if (fieldName.StartsWith("is") ||
                fieldName.StartsWith("has") ||
                fieldName.StartsWith("can") ||
                fieldType == "bool")
            {
                if (!formInfo.StateVariables.ContainsKey(fieldName))
                {
                    formInfo.StateVariables[fieldName] = new StateVariable
                    {
                        Name = fieldName,
                        Type = fieldType,
                        AffectsControls = new List<string>()
                    };
                }
            }
        }
    }

    private void ExtractVisibilityLogic(ClassDeclarationSyntax classDeclaration, FormInfo formInfo)
    {
        // Find if statements that control visibility
        var ifStatements = classDeclaration.DescendantNodes()
            .OfType<IfStatementSyntax>();

        foreach (var ifStatement in ifStatements)
        {
            var condition = ifStatement.Condition.ToString();

            // Look for visibility assignments in the if block
            var assignments = ifStatement.Statement.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>();

            foreach (var assignment in assignments)
            {
                var left = assignment.Left.ToString();
                if (left.Contains(".Visible"))
                {
                    var parts = left.Split('.');
                    if (parts.Length >= 2)
                    {
                        var controlName = parts[0];
                        var control = FindControlInForm(formInfo, controlName);
                        if (control != null)
                        {
                            control.Properties["VisibilityCondition"] = condition;
                        }

                        // Track which state variables affect which controls
                        foreach (var stateVar in formInfo.StateVariables.Values)
                        {
                            if (condition.Contains(stateVar.Name))
                            {
                                if (!stateVar.AffectsControls.Contains(controlName))
                                {
                                    stateVar.AffectsControls.Add(controlName);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ExtractKeyboardShortcuts(ClassDeclarationSyntax classDeclaration, FormInfo formInfo)
    {
        // Look for KeyDown event handlers
        var methods = classDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text.Contains("KeyDown"));

        foreach (var method in methods)
        {
            if (method.Body == null) continue;

            // Find switch statements or if statements checking e.KeyCode
            var switchStatements = method.Body.DescendantNodes()
                .OfType<SwitchStatementSyntax>();

            foreach (var switchStmt in switchStatements)
            {
                var switchExpr = switchStmt.Expression.ToString();
                if (switchExpr.Contains("KeyCode") || switchExpr.Contains("Key"))
                {
                    foreach (var section in switchStmt.Sections)
                    {
                        var keyLabel = section.Labels.FirstOrDefault()?.ToString().Replace("case Keys.", "").Replace(":", "");
                        if (keyLabel != null)
                        {
                            var action = AnalyzeKeyboardAction(section);
                            formInfo.KeyboardShortcuts[keyLabel] = action;
                        }
                    }
                }
            }

            // Also look for if statements checking keys
            var ifStatements = method.Body.DescendantNodes()
                .OfType<IfStatementSyntax>();

            foreach (var ifStatement in ifStatements)
            {
                var condition = ifStatement.Condition.ToString();
                if (condition.Contains("KeyCode") || condition.Contains("Key"))
                {
                    // Try to extract the key name
                    var keyMatch = System.Text.RegularExpressions.Regex.Match(condition, @"Keys\.(\w+)");
                    if (keyMatch.Success)
                    {
                        var key = keyMatch.Groups[1].Value;
                        var action = AnalyzeKeyboardActionFromBlock(ifStatement.Statement);
                        if (!formInfo.KeyboardShortcuts.ContainsKey(key))
                        {
                            formInfo.KeyboardShortcuts[key] = action;
                        }
                    }
                }
            }
        }
    }

    private string AnalyzeKeyboardAction(SwitchSectionSyntax section)
    {
        var statements = section.Statements;
        var actions = new List<string>();

        foreach (var statement in statements)
        {
            var text = statement.ToString();
            if (text.Contains("ShowDialog") || text.Contains("new Form"))
            {
                actions.Add("Opens dialog");
            }
            else if (text.Contains("Close"))
            {
                actions.Add("Closes window");
            }
            else if (text.Contains("Save"))
            {
                actions.Add("Saves");
            }
            else if (text.Contains("="))
            {
                actions.Add("Sets property");
            }
        }

        return actions.Count > 0 ? string.Join("; ", actions) : "Unknown action";
    }

    private string AnalyzeKeyboardActionFromBlock(StatementSyntax block)
    {
        var text = block.ToString();
        if (text.Contains("ShowDialog"))
            return "Opens dialog";
        if (text.Contains("Close"))
            return "Closes window";
        if (text.Contains("Save"))
            return "Saves";

        return "Unknown action";
    }

    private UIElement? FindControlInForm(FormInfo formInfo, string controlName)
    {
        return formInfo.MenuItems.FirstOrDefault(c => c.Name == controlName)
            ?? formInfo.ToolbarButtons.FirstOrDefault(c => c.Name == controlName)
            ?? formInfo.Controls.FirstOrDefault(c => c.Name == controlName);
    }
}
