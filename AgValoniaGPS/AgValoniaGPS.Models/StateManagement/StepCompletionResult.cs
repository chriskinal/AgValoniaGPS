namespace AgValoniaGPS.Models.StateManagement;

/// <summary>
/// Represents the result of completing a wizard step.
/// </summary>
public class StepCompletionResult
{
    /// <summary>
    /// Gets or sets whether the step was completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if step completion failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
