using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Maps dynamic behaviors to UI modes and states
/// </summary>
public class UIStateMapper
{
    public List<UIMode> InferModesFromRules(
        List<VisibilityRule> visibilityRules,
        List<PropertyChange> propertyChanges)
    {
        var modes = new List<UIMode>();

        // Group rules by their controlling conditions/variables
        var rulesByCondition = visibilityRules
            .Where(r => r.Condition != "Unconditional")
            .GroupBy(r => NormalizeCondition(r.Condition))
            .ToList();

        foreach (var group in rulesByCondition.Take(50)) // Limit to avoid over-complexity
        {
            var mode = new UIMode
            {
                ModeName = InferModeName(group.Key, group.ToList()),
                Description = $"Mode triggered by: {group.Key}"
            };

            // Collect visible and hidden controls
            foreach (var rule in group)
            {
                if (rule.TargetVisibility)
                    mode.VisibleControls.Add(rule.ControlName);
                else
                    mode.HiddenControls.Add(rule.ControlName);
            }

            // Add triggers from dependencies
            var allVariables = group.SelectMany(r => r.DependsOnVariables).Distinct().ToList();
            mode.Triggers.AddRange(allVariables);

            // Find related property changes
            var relatedChanges = propertyChanges
                .Where(c => mode.VisibleControls.Contains(c.ControlName) ||
                            mode.HiddenControls.Contains(c.ControlName))
                .ToList();

            foreach (var change in relatedChanges)
            {
                if (!mode.ControlTextChanges.ContainsKey(change.ControlName) &&
                    change.PropertyName == "Text")
                {
                    mode.ControlTextChanges[change.ControlName] = change.NewValue;
                }
            }

            if (mode.VisibleControls.Count > 0 || mode.HiddenControls.Count > 0)
                modes.Add(mode);
        }

        return modes;
    }

    public List<StateTransition> InferTransitions(List<UIMode> modes, List<VisibilityRule> rules)
    {
        var transitions = new List<StateTransition>();

        // Look for mode switches in the code
        // Heuristic: If multiple controls change visibility together, it's likely a mode transition
        var rulesByContext = rules
            .Where(r => r.Context != "Unknown")
            .GroupBy(r => r.Context)
            .ToList();

        foreach (var contextGroup in rulesByContext)
        {
            if (contextGroup.Count() < 2) continue; // Skip single changes

            // Try to match this to modes
            var affectedControls = contextGroup.Select(r => r.ControlName).ToHashSet();

            var fromMode = modes.FirstOrDefault(m =>
                m.VisibleControls.Concat(m.HiddenControls)
                    .Intersect(affectedControls).Any());

            var toMode = modes.FirstOrDefault(m =>
                m != fromMode &&
                m.VisibleControls.Concat(m.HiddenControls)
                    .Intersect(affectedControls).Any());

            if (fromMode != null && toMode != null)
            {
                var transition = new StateTransition
                {
                    FromMode = fromMode.ModeName,
                    ToMode = toMode.ModeName,
                    Trigger = contextGroup.Key,
                    Condition = contextGroup.First().Condition,
                    SourceFile = contextGroup.First().SourceFile,
                    LineNumber = contextGroup.First().LineNumber
                };

                transitions.Add(transition);
            }
        }

        return transitions;
    }

    private string NormalizeCondition(string condition)
    {
        // Simplify conditions to group similar ones
        condition = condition.Trim();

        // Handle common patterns
        if (condition.Contains("==") || condition.Contains("!="))
        {
            // Extract the variable being checked
            var parts = condition.Split(new[] { "==", "!=" }, StringSplitOptions.None);
            if (parts.Length > 0)
            {
                return parts[0].Trim();
            }
        }

        return condition;
    }

    private string InferModeName(string condition, List<VisibilityRule> rules)
    {
        // Try to infer a meaningful mode name from the condition and affected controls

        // Check for common state variable patterns
        if (condition.Contains("isJobStarted") || condition.Contains("jobStarted"))
            return "JobActive";
        if (condition.Contains("isAutoSteerEnabled") || condition.Contains("autoSteerEnabled"))
            return "AutoSteerActive";
        if (condition.Contains("isBoundarySet") || condition.Contains("boundarySet"))
            return "BoundaryMode";
        if (condition.Contains("isInGuideMode") || condition.Contains("guideMode"))
            return "GuidanceMode";
        if (condition.Contains("isFieldOpen") || condition.Contains("fieldOpen"))
            return "FieldOpen";

        // Fallback: use the first control name with "Mode" suffix
        var firstControl = rules.FirstOrDefault()?.ControlName;
        if (!string.IsNullOrEmpty(firstControl))
        {
            return $"{firstControl}Mode";
        }

        // Last resort: use condition itself
        return $"Mode_{condition.GetHashCode():X}";
    }

    public Dictionary<string, List<string>> AnalyzeStateVariableImpact(
        List<VisibilityRule> rules,
        Dictionary<string, StateVariable> stateVariables)
    {
        var impact = new Dictionary<string, List<string>>();

        foreach (var rule in rules)
        {
            foreach (var variable in rule.DependsOnVariables)
            {
                // Check if it's a known state variable
                if (stateVariables.ContainsKey(variable))
                {
                    if (!impact.ContainsKey(variable))
                        impact[variable] = new List<string>();

                    if (!impact[variable].Contains(rule.ControlName))
                        impact[variable].Add(rule.ControlName);
                }
            }
        }

        return impact;
    }

    public List<UIMode> MergeRelatedModes(List<UIMode> modes)
    {
        // Merge modes that have significant overlap in visible controls
        var merged = new List<UIMode>();
        var processed = new HashSet<string>();

        foreach (var mode in modes)
        {
            if (processed.Contains(mode.ModeName))
                continue;

            var relatedModes = modes
                .Where(m => m != mode && !processed.Contains(m.ModeName))
                .Where(m => CalculateOverlap(mode, m) > 0.7) // 70% overlap threshold
                .ToList();

            if (relatedModes.Any())
            {
                // Merge into one mode
                var mergedMode = new UIMode
                {
                    ModeName = mode.ModeName,
                    Description = $"Merged mode: {mode.Description}"
                };

                mergedMode.VisibleControls.AddRange(mode.VisibleControls);
                mergedMode.HiddenControls.AddRange(mode.HiddenControls);
                mergedMode.Triggers.AddRange(mode.Triggers);

                foreach (var related in relatedModes)
                {
                    mergedMode.VisibleControls.AddRange(related.VisibleControls);
                    mergedMode.HiddenControls.AddRange(related.HiddenControls);
                    mergedMode.Triggers.AddRange(related.Triggers);
                    processed.Add(related.ModeName);
                }

                // Remove duplicates
                mergedMode.VisibleControls = mergedMode.VisibleControls.Distinct().ToList();
                mergedMode.HiddenControls = mergedMode.HiddenControls.Distinct().ToList();
                mergedMode.Triggers = mergedMode.Triggers.Distinct().ToList();

                merged.Add(mergedMode);
                processed.Add(mode.ModeName);
            }
            else
            {
                merged.Add(mode);
                processed.Add(mode.ModeName);
            }
        }

        return merged;
    }

    private double CalculateOverlap(UIMode mode1, UIMode mode2)
    {
        var controls1 = mode1.VisibleControls.Concat(mode1.HiddenControls).ToHashSet();
        var controls2 = mode2.VisibleControls.Concat(mode2.HiddenControls).ToHashSet();

        if (controls1.Count == 0 && controls2.Count == 0)
            return 0;

        var intersection = controls1.Intersect(controls2).Count();
        var union = controls1.Union(controls2).Count();

        return union > 0 ? (double)intersection / union : 0;
    }
}
