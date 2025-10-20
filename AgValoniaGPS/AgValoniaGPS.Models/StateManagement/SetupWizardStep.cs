namespace AgValoniaGPS.Models.StateManagement;

/// <summary>
/// Represents a single step in the setup wizard.
/// </summary>
public class SetupWizardStep
{
    /// <summary>
    /// Gets or sets the index of this step in the wizard sequence.
    /// </summary>
    public int StepIndex { get; set; }

    /// <summary>
    /// Gets or sets the title of this step.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of this step.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this step is required to complete the wizard.
    /// </summary>
    public bool IsRequired { get; set; }
}
