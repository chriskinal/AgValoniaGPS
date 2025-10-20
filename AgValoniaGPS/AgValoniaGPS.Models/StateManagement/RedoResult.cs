namespace AgValoniaGPS.Models.StateManagement
{
    /// <summary>
    /// Result of a redo operation.
    /// </summary>
    public class RedoResult
    {
        /// <summary>
        /// Gets whether the redo operation was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the error message if the redo operation failed.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Gets the description of the command that was redone.
        /// </summary>
        public string? CommandDescription { get; }

        /// <summary>
        /// Creates a successful RedoResult.
        /// </summary>
        /// <param name="commandDescription">Description of the redone command</param>
        /// <returns>A successful RedoResult</returns>
        public static RedoResult CreateSuccess(string commandDescription)
        {
            return new RedoResult(true, null, commandDescription);
        }

        /// <summary>
        /// Creates a failed RedoResult.
        /// </summary>
        /// <param name="errorMessage">Error message describing the failure</param>
        /// <returns>A failed RedoResult</returns>
        public static RedoResult CreateFailure(string errorMessage)
        {
            return new RedoResult(false, errorMessage, null);
        }

        /// <summary>
        /// Creates a new RedoResult.
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="errorMessage">Error message if failed</param>
        /// <param name="commandDescription">Description of redone command if successful</param>
        private RedoResult(bool success, string? errorMessage, string? commandDescription)
        {
            Success = success;
            ErrorMessage = errorMessage;
            CommandDescription = commandDescription;
        }
    }
}
