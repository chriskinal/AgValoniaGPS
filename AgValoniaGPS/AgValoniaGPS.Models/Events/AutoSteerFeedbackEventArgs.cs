using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for AutoSteer feedback events.
/// Raised when feedback data is received from the AutoSteer module.
/// </summary>
public class AutoSteerFeedbackEventArgs : EventArgs
{
    /// <summary>
    /// Gets the AutoSteer feedback data.
    /// </summary>
    public readonly AutoSteerFeedback Feedback;

    /// <summary>
    /// Creates a new instance of AutoSteerFeedbackEventArgs.
    /// </summary>
    /// <param name="feedback">AutoSteer feedback data</param>
    public AutoSteerFeedbackEventArgs(AutoSteerFeedback feedback)
    {
        Feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
    }
}
