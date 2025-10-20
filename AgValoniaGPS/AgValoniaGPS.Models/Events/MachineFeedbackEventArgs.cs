using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for Machine feedback events.
/// Raised when feedback data is received from the Machine module.
/// </summary>
public class MachineFeedbackEventArgs : EventArgs
{
    /// <summary>
    /// Gets the Machine feedback data.
    /// </summary>
    public readonly MachineFeedback Feedback;

    /// <summary>
    /// Creates a new instance of MachineFeedbackEventArgs.
    /// </summary>
    /// <param name="feedback">Machine feedback data</param>
    public MachineFeedbackEventArgs(MachineFeedback feedback)
    {
        Feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
    }
}
