namespace AgValoniaGPS.Models.StateManagement
{
    /// <summary>
    /// Result of an undo operation.
    /// </summary>
    public class UndoResult
    {
        /// <summary>
        /// Gets whether the undo operation was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the error message if the undo operation failed.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Gets the description of the command that was undone.
        /// </summary>
        public string? CommandDescription { get; }

        /// <summary>
        /// Creates a successful UndoResult.
        /// </summary>
        /// <param name="commandDescription">Description of the undone command</param>
        /// <returns>A successful UndoResult</returns>
        public static UndoResult CreateSuccess(string commandDescription)
        {
            return new UndoResult(true, null, commandDescription);
        }

        /// <summary>
        /// Creates a failed UndoResult.
        /// </summary>
        /// <param name="errorMessage">Error message describing the failure</param>
        /// <returns>A failed UndoResult</returns>
        public static UndoResult CreateFailure(string errorMessage)
        {
            return new UndoResult(false, errorMessage, null);
        }

        /// <summary>
        /// Creates a new UndoResult.
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="errorMessage">Error message if failed</param>
        /// <param name="commandDescription">Description of undone command if successful</param>
        private UndoResult(bool success, string? errorMessage, string? commandDescription)
        {
            Success = success;
            ErrorMessage = errorMessage;
            CommandDescription = commandDescription;
        }
    }
}
