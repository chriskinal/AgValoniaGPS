using System;

namespace AgValoniaGPS.Models.StateManagement
{
    /// <summary>
    /// Event arguments for undo/redo state changes.
    /// Raised when the undo or redo stack state changes.
    /// </summary>
    public class UndoRedoStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether undo is available (undo stack has commands).
        /// </summary>
        public bool CanUndo { get; }

        /// <summary>
        /// Gets whether redo is available (redo stack has commands).
        /// </summary>
        public bool CanRedo { get; }

        /// <summary>
        /// Gets the number of commands available to undo.
        /// </summary>
        public int UndoCount { get; }

        /// <summary>
        /// Gets the number of commands available to redo.
        /// </summary>
        public int RedoCount { get; }

        /// <summary>
        /// Gets the timestamp when the state changed.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the description of the most recent command that was executed, undone, or redone.
        /// Null if stacks are empty.
        /// </summary>
        public string? LastCommandDescription { get; }

        /// <summary>
        /// Creates a new UndoRedoStateChangedEventArgs.
        /// </summary>
        /// <param name="canUndo">Whether undo is available</param>
        /// <param name="canRedo">Whether redo is available</param>
        /// <param name="undoCount">Number of commands in undo stack</param>
        /// <param name="redoCount">Number of commands in redo stack</param>
        /// <param name="lastCommandDescription">Description of the most recent command</param>
        public UndoRedoStateChangedEventArgs(
            bool canUndo,
            bool canRedo,
            int undoCount,
            int redoCount,
            string? lastCommandDescription = null)
        {
            CanUndo = canUndo;
            CanRedo = canRedo;
            UndoCount = undoCount;
            RedoCount = redoCount;
            Timestamp = DateTime.UtcNow;
            LastCommandDescription = lastCommandDescription;
        }
    }
}
