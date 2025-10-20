using System.Threading.Tasks;

namespace AgValoniaGPS.Services.UndoRedo
{
    /// <summary>
    /// Represents a command that can be executed and undone.
    /// Implements the Command Pattern for supporting undo/redo functionality.
    /// </summary>
    /// <remarks>
    /// Commands encapsulate user-initiated actions on field boundaries, guidance lines, and field edits.
    /// Each command must be able to execute itself and reverse its effects when undone.
    /// Commands are managed by IUndoRedoService in undo/redo stacks.
    /// </remarks>
    public interface IUndoableCommand
    {
        /// <summary>
        /// Gets a human-readable description of the command for display in undo/redo history.
        /// </summary>
        /// <example>
        /// "Create AB Line", "Modify Boundary Point", "Delete Headland"
        /// </example>
        string Description { get; }

        /// <summary>
        /// Executes the command, performing the intended action.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// Execution must complete in less than 50ms per performance requirements.
        /// Should be idempotent - safe to call multiple times with same result.
        /// </remarks>
        Task ExecuteAsync();

        /// <summary>
        /// Undoes the command, reversing the effects of ExecuteAsync.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// Undo must complete in less than 50ms per performance requirements.
        /// Must restore the exact state prior to ExecuteAsync being called.
        /// Should be idempotent - safe to call multiple times with same result.
        /// </remarks>
        Task UndoAsync();
    }
}
