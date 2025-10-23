namespace AgValoniaGPS.Models;

/// <summary>
/// Represents the type of message being displayed to the user.
/// Used to determine icon and styling for message dialogs.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Informational message (default).
    /// </summary>
    Information = 0,

    /// <summary>
    /// Warning message that requires user attention.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error message indicating a problem occurred.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Success message indicating an operation completed successfully.
    /// </summary>
    Success = 3
}
