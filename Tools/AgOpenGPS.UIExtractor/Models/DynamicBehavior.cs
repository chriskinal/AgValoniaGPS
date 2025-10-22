namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Represents a visibility change rule for a UI element
/// </summary>
public class VisibilityRule
{
    public string ControlName { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Condition { get; set; } = string.Empty;
    public bool TargetVisibility { get; set; }
    public List<string> DependsOnVariables { get; set; } = new();
    public string Context { get; set; } = string.Empty; // Method name where it occurs
}

/// <summary>
/// Represents a dynamic property change
/// </summary>
public class PropertyChange
{
    public string ControlName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty; // Text, BackColor, Image, etc.
    public string SourceFile { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public List<string> DependsOnVariables { get; set; } = new();
    public string Context { get; set; } = string.Empty;
}

/// <summary>
/// Represents a button panel configuration
/// </summary>
public class ButtonPanelConfig
{
    public string PanelName { get; set; } = string.Empty;
    public List<ConfigurableButton> Buttons { get; set; } = new();
    public string ConfigurationSource { get; set; } = string.Empty;
    public bool IsUserCustomizable { get; set; }
}

/// <summary>
/// Represents a button that can be configured
/// </summary>
public class ConfigurableButton
{
    public string Name { get; set; } = string.Empty;
    public string DefaultText { get; set; } = string.Empty;
    public List<string> PossiblePositions { get; set; } = new();
    public List<string> Modes { get; set; } = new(); // Which modes this button appears in
    public string Action { get; set; } = string.Empty;
}

/// <summary>
/// Represents a UI mode/state
/// </summary>
public class UIMode
{
    public string ModeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> VisibleControls { get; set; } = new();
    public List<string> HiddenControls { get; set; } = new();
    public List<string> EnabledMenuItems { get; set; } = new();
    public List<string> DisabledMenuItems { get; set; } = new();
    public Dictionary<string, string> ControlTextChanges { get; set; } = new();
    public List<string> Triggers { get; set; } = new(); // What causes transition to this mode
}

/// <summary>
/// Represents a state transition
/// </summary>
public class StateTransition
{
    public string FromMode { get; set; } = string.Empty;
    public string ToMode { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

/// <summary>
/// Represents menu item dynamic behavior
/// </summary>
public class MenuDynamicBehavior
{
    public string MenuItemName { get; set; } = string.Empty;
    public List<string> EnableConditions { get; set; } = new();
    public List<string> DisableConditions { get; set; } = new();
    public List<string> VisibilityConditions { get; set; } = new();
    public bool IsContextSensitive { get; set; }
    public List<string> DependsOnState { get; set; } = new();
}

/// <summary>
/// Complete dynamic behavior analysis result
/// </summary>
public class DynamicBehaviorResult
{
    public List<VisibilityRule> VisibilityRules { get; set; } = new();
    public List<PropertyChange> PropertyChanges { get; set; } = new();
    public List<ButtonPanelConfig> ButtonPanels { get; set; } = new();
    public List<UIMode> Modes { get; set; } = new();
    public List<StateTransition> StateTransitions { get; set; } = new();
    public List<MenuDynamicBehavior> MenuBehaviors { get; set; } = new();
    public Dictionary<string, List<string>> StateVariableUsage { get; set; } = new(); // Variable -> List of controls it affects
}
