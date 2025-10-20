using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.UndoRedo
{
    /// <summary>
    /// Implements undo/redo functionality using the Command Pattern with separate undo and redo stacks.
    /// Thread-safe implementation for concurrent access.
    /// </summary>
    /// <remarks>
    /// Maintains two stacks:
    /// - Undo Stack: Commands that have been executed and can be undone
    /// - Redo Stack: Commands that have been undone and can be redone
    ///
    /// ExecuteAsync: Executes command, adds to undo stack, clears redo stack
    /// UndoAsync: Pops from undo stack, calls UndoAsync, adds to redo stack
    /// RedoAsync: Pops from redo stack, calls ExecuteAsync, adds to undo stack
    ///
    /// All operations are thread-safe using lock synchronization.
    /// Performance target: All operations complete in less than 50ms.
    /// </remarks>
    public class UndoRedoService : IUndoRedoService
    {
        private readonly object _lockObject = new object();
        private readonly List<IUndoableCommand> _undoStack = new List<IUndoableCommand>();
        private readonly List<IUndoableCommand> _redoStack = new List<IUndoableCommand>();

        /// <summary>
        /// Raised when the undo/redo state changes.
        /// </summary>
        public event EventHandler<UndoRedoStateChangedEventArgs>? UndoRedoStateChanged;

        /// <summary>
        /// Executes a command and adds it to the undo stack.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
        public async Task ExecuteAsync(IUndoableCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            // Execute the command
            await command.ExecuteAsync();

            lock (_lockObject)
            {
                // Add to undo stack
                _undoStack.Add(command);

                // Clear redo stack (new action invalidates redo history)
                _redoStack.Clear();

                // Raise state changed event
                RaiseStateChanged(command.Description);
            }
        }

        /// <summary>
        /// Undoes the most recent command from the undo stack.
        /// </summary>
        /// <returns>Result indicating success/failure and command description</returns>
        public async Task<UndoResult> UndoAsync()
        {
            IUndoableCommand? command;
            string commandDescription;

            lock (_lockObject)
            {
                // Check if undo stack is empty
                if (_undoStack.Count == 0)
                {
                    return UndoResult.CreateFailure("Nothing to undo");
                }

                // Pop command from undo stack
                command = _undoStack[_undoStack.Count - 1];
                commandDescription = command.Description;
                _undoStack.RemoveAt(_undoStack.Count - 1);
            }

            try
            {
                // Call undo on the command (outside lock to avoid blocking)
                await command.UndoAsync();

                lock (_lockObject)
                {
                    // Add to redo stack
                    _redoStack.Add(command);

                    // Raise state changed event
                    RaiseStateChanged(commandDescription);
                }

                return UndoResult.CreateSuccess(commandDescription);
            }
            catch (Exception ex)
            {
                // If undo fails, put command back on undo stack
                lock (_lockObject)
                {
                    _undoStack.Add(command);
                }

                return UndoResult.CreateFailure($"Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Redoes the most recently undone command from the redo stack.
        /// </summary>
        /// <returns>Result indicating success/failure and command description</returns>
        public async Task<RedoResult> RedoAsync()
        {
            IUndoableCommand? command;
            string commandDescription;

            lock (_lockObject)
            {
                // Check if redo stack is empty
                if (_redoStack.Count == 0)
                {
                    return RedoResult.CreateFailure("Nothing to redo");
                }

                // Pop command from redo stack
                command = _redoStack[_redoStack.Count - 1];
                commandDescription = command.Description;
                _redoStack.RemoveAt(_redoStack.Count - 1);
            }

            try
            {
                // Call execute on the command (outside lock to avoid blocking)
                await command.ExecuteAsync();

                lock (_lockObject)
                {
                    // Add to undo stack
                    _undoStack.Add(command);

                    // Raise state changed event
                    RaiseStateChanged(commandDescription);
                }

                return RedoResult.CreateSuccess(commandDescription);
            }
            catch (Exception ex)
            {
                // If redo fails, put command back on redo stack
                lock (_lockObject)
                {
                    _redoStack.Add(command);
                }

                return RedoResult.CreateFailure($"Redo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if undo is available.
        /// </summary>
        /// <returns>True if undo stack is not empty</returns>
        public bool CanUndo()
        {
            lock (_lockObject)
            {
                return _undoStack.Count > 0;
            }
        }

        /// <summary>
        /// Checks if redo is available.
        /// </summary>
        /// <returns>True if redo stack is not empty</returns>
        public bool CanRedo()
        {
            lock (_lockObject)
            {
                return _redoStack.Count > 0;
            }
        }

        /// <summary>
        /// Gets descriptions of all commands in the undo stack.
        /// </summary>
        /// <returns>Array of command descriptions, newest first</returns>
        public string[] GetUndoStackDescriptions()
        {
            lock (_lockObject)
            {
                // Return in reverse order (newest first)
                return _undoStack
                    .Select(cmd => cmd.Description)
                    .Reverse()
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets descriptions of all commands in the redo stack.
        /// </summary>
        /// <returns>Array of command descriptions, most recently undone first</returns>
        public string[] GetRedoStackDescriptions()
        {
            lock (_lockObject)
            {
                // Return in reverse order (most recently undone first)
                return _redoStack
                    .Select(cmd => cmd.Description)
                    .Reverse()
                    .ToArray();
            }
        }

        /// <summary>
        /// Clears all commands from the undo stack.
        /// </summary>
        public void ClearUndoStack()
        {
            lock (_lockObject)
            {
                if (_undoStack.Count > 0)
                {
                    _undoStack.Clear();
                    RaiseStateChanged(null);
                }
            }
        }

        /// <summary>
        /// Clears all commands from the redo stack.
        /// </summary>
        public void ClearRedoStack()
        {
            lock (_lockObject)
            {
                if (_redoStack.Count > 0)
                {
                    _redoStack.Clear();
                    RaiseStateChanged(null);
                }
            }
        }

        /// <summary>
        /// Clears all commands from both undo and redo stacks.
        /// </summary>
        public void ClearAllStacks()
        {
            lock (_lockObject)
            {
                bool hadCommands = _undoStack.Count > 0 || _redoStack.Count > 0;
                _undoStack.Clear();
                _redoStack.Clear();

                if (hadCommands)
                {
                    RaiseStateChanged(null);
                }
            }
        }

        /// <summary>
        /// Raises the UndoRedoStateChanged event with current stack state.
        /// Must be called inside lock.
        /// </summary>
        /// <param name="lastCommandDescription">Description of the most recent command</param>
        private void RaiseStateChanged(string? lastCommandDescription)
        {
            var args = new UndoRedoStateChangedEventArgs(
                canUndo: _undoStack.Count > 0,
                canRedo: _redoStack.Count > 0,
                undoCount: _undoStack.Count,
                redoCount: _redoStack.Count,
                lastCommandDescription: lastCommandDescription
            );

            UndoRedoStateChanged?.Invoke(this, args);
        }
    }
}
