namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Validation result for guidance lines
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation passed
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets the list of error messages
    /// </summary>
    public List<string> ErrorMessages { get; } = new();

    /// <summary>
    /// Gets the list of warning messages
    /// </summary>
    public List<string> Warnings { get; } = new();

    /// <summary>
    /// Add an error message and mark validation as failed
    /// </summary>
    /// <param name="message">Error message</param>
    public void AddError(string message)
    {
        ErrorMessages.Add(message);
        IsValid = false;
    }

    /// <summary>
    /// Add a warning message without failing validation
    /// </summary>
    /// <param name="message">Warning message</param>
    public void AddWarning(string message)
    {
        Warnings.Add(message);
    }

    /// <summary>
    /// Merge another validation result into this one
    /// </summary>
    /// <param name="other">Other validation result to merge</param>
    public void Merge(ValidationResult other)
    {
        if (!other.IsValid) IsValid = false;
        ErrorMessages.AddRange(other.ErrorMessages);
        Warnings.AddRange(other.Warnings);
    }
}
