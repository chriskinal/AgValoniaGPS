namespace AgValoniaGPS.Models;

/// <summary>
/// Represents the result of a dialog interaction.
/// </summary>
public enum DialogResult
{
    /// <summary>
    /// No result has been set yet or the dialog was closed without a response.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user clicked OK or confirmed the action.
    /// </summary>
    OK = 1,

    /// <summary>
    /// The user clicked Cancel or dismissed the dialog.
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// The user clicked Yes.
    /// </summary>
    Yes = 3,

    /// <summary>
    /// The user clicked No.
    /// </summary>
    No = 4
}
