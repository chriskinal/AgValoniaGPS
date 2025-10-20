using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.UndoRedo
{
    /// <summary>
    /// Provides undo/redo functionality for user-initiated actions using the Command Pattern.
    /// Manages undo and redo stacks for operations on field boundaries, guidance lines, and field edits.
    /// </summary>
    /// <remarks>
    /// Thread-safe service that maintains separate undo and redo stacks.
    /// ExecuteAsync adds commands to undo stack and clears redo stack.
    /// UndoAsync moves commands from undo stack to redo stack.
    /// RedoAsync moves commands from redo stack to undo stack.
    /// All operations must complete in less than 50ms per performance requirements.
    /// </remarks>
    public interface IUndoRedoService
    {
        /// <summary>
        /// Raised when the undo/redo state changes (stack additions, removals, or clears).
        /// </summary>
        event EventHandler<UndoRedoStateChangedEventArgs>? UndoRedoStateChanged;

        /// <summary>
        /// Executes a command and adds it to the undo stack.
        /// Clears the redo stack as new actions invalidate redo history.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
        /// <remarks>
        /// Must complete in less than 50ms per performance requirements.
        /// Raises UndoRedoStateChanged event after execution.
        /// </remarks>
        Task ExecuteAsync(IUndoableCommand command);

        /// <summary>
        /// Undoes the most recent command from the undo stack.
        /// Moves the undone command to the redo stack.
        /// </summary>
        /// <returns>Result indicating success/failure and command description</returns>
        /// <remarks>
        /// Must complete in less than 50ms per performance requirements.
        /// Raises UndoRedoStateChanged event after undo.
        /// Returns failure result if undo stack is empty.
        /// </remarks>
        Task<UndoResult> UndoAsync();

        /// <summary>
        /// Redoes the most recently undone command from the redo stack.
        /// Moves the redone command back to the undo stack.
        /// </summary>
        /// <returns>Result indicating success/failure and command description</returns>
        /// <remarks>
        /// Must complete in less than 50ms per performance requirements.
        /// Raises UndoRedoStateChanged event after redo.
        /// Returns failure result if redo stack is empty.
        /// </remarks>
        Task<RedoResult> RedoAsync();

        /// <summary>
        /// Checks if undo is available (undo stack has commands).
        /// </summary>
        /// <returns>True if undo stack is not empty, false otherwise</returns>
        bool CanUndo();

        /// <summary>
        /// Checks if redo is available (redo stack has commands).
        /// </summary>
        /// <returns>True if redo stack is not empty, false otherwise</returns>
        bool CanRedo();

        /// <summary>
        /// Gets descriptions of all commands in the undo stack.
        /// Used for displaying undo history in UI.
        /// </summary>
        /// <returns>Array of command descriptions, newest first (top of stack)</returns>
        string[] GetUndoStackDescriptions();

        /// <summary>
        /// Gets descriptions of all commands in the redo stack.
        /// Used for displaying redo history in UI.
        /// </summary>
        /// <returns>Array of command descriptions, most recently undone first (top of stack)</returns>
        string[] GetRedoStackDescriptions();

        /// <summary>
        /// Clears all commands from the undo stack.
        /// </summary>
        /// <remarks>
        /// Raises UndoRedoStateChanged event if stack was not empty.
        /// </remarks>
        void ClearUndoStack();

        /// <summary>
        /// Clears all commands from the redo stack.
        /// </summary>
        /// <remarks>
        /// Raises UndoRedoStateChanged event if stack was not empty.
        /// </remarks>
        void ClearRedoStack();

        /// <summary>
        /// Clears all commands from both undo and redo stacks.
        /// </summary>
        /// <remarks>
        /// Raises UndoRedoStateChanged event if either stack was not empty.
        /// </remarks>
        void ClearAllStacks();
    }
}
