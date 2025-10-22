using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Parses Designer.cs files to extract UI structure using Roslyn
/// </summary>
public class DesignerParser
{
    public FormInfo ParseDesignerFile(string filePath)
    {
        var formInfo = new FormInfo
        {
            FilePath = filePath,
            FormName = Path.GetFileNameWithoutExtension(filePath).Replace(".Designer", "")
        };

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Warning: File not found: {filePath}");
            return formInfo;
        }

        var sourceCode = File.ReadAllText(filePath);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot();

        // Find the InitializeComponent method
        var initMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "InitializeComponent");

        if (initMethod == null)
        {
            Console.WriteLine($"Warning: InitializeComponent not found in {filePath}");
            return formInfo;
        }

        // Extract control declarations
        ExtractControls(initMethod, formInfo);

        // Extract form properties
        ExtractFormProperties(initMethod, formInfo);

        return formInfo;
    }

    private void ExtractControls(MethodDeclarationSyntax initMethod, FormInfo formInfo)
    {
        // Find all "this.controlName = new ControlType()" patterns
        var objectCreations = initMethod.DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>();

        foreach (var creation in objectCreations)
        {
            var controlType = creation.Type.ToString();

            // Get the variable name by finding the assignment
            var assignment = creation.Parent as AssignmentExpressionSyntax;
            if (assignment == null) continue;

            var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var controlName = memberAccess.Name.ToString();

            var element = new UIElement
            {
                Name = controlName,
                Type = controlType
            };

            // Categorize by control type
            if (controlType.Contains("MenuStrip") || controlType.Contains("ToolStripMenuItem"))
            {
                formInfo.MenuItems.Add(element);
            }
            else if (controlType.Contains("ToolStrip") || controlType.Contains("ToolStripButton"))
            {
                formInfo.ToolbarButtons.Add(element);
            }
            else
            {
                formInfo.Controls.Add(element);
            }
        }

        // Extract properties for each control
        ExtractControlProperties(initMethod, formInfo);
    }

    private void ExtractControlProperties(MethodDeclarationSyntax initMethod, FormInfo formInfo)
    {
        // Find all "this.controlName.PropertyName = value" patterns
        var assignments = initMethod.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>();

        foreach (var assignment in assignments)
        {
            var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            // Check if it's a control property (this.controlName.Property)
            var innerMemberAccess = memberAccess.Expression as MemberAccessExpressionSyntax;
            if (innerMemberAccess == null) continue;

            var controlName = innerMemberAccess.Name.ToString();
            var propertyName = memberAccess.Name.ToString();
            var value = assignment.Right.ToString().Trim('"');

            // Find the control and add the property
            var control = FindControl(formInfo, controlName);
            if (control != null)
            {
                control.Properties[propertyName] = value;

                // Special handling for Text property
                if (propertyName == "Text")
                {
                    control.Text = value;
                }
            }
        }
    }

    private void ExtractFormProperties(MethodDeclarationSyntax initMethod, FormInfo formInfo)
    {
        // Extract form title and size
        var assignments = initMethod.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>();

        foreach (var assignment in assignments)
        {
            var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var thisExpr = memberAccess.Expression as ThisExpressionSyntax;
            if (thisExpr == null) continue;

            var propertyName = memberAccess.Name.ToString();
            var value = assignment.Right.ToString().Trim('"');

            if (propertyName == "Text")
            {
                formInfo.Title = value;
            }
            else if (propertyName == "Size" || propertyName == "ClientSize")
            {
                // Try to extract width and height from Size(width, height)
                var match = System.Text.RegularExpressions.Regex.Match(value, @"Size\((\d+),\s*(\d+)\)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int width) && int.TryParse(match.Groups[2].Value, out int height))
                {
                    formInfo.Size = new Models.Size { Width = width, Height = height };
                }
            }
        }
    }

    private UIElement? FindControl(FormInfo formInfo, string controlName)
    {
        var allControls = formInfo.MenuItems
            .Concat(formInfo.ToolbarButtons)
            .Concat(formInfo.Controls);

        return allControls.FirstOrDefault(c => c.Name == controlName);
    }
}
