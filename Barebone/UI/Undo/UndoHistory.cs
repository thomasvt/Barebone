using System.Diagnostics.CodeAnalysis;

namespace Barebone.UI.Undo
{
    internal class UndoHistory<TSnapshot>(int maxRetention, Func<TSnapshot> snapshotFactory) : IUndoHistory<TSnapshot>
    {
        private readonly List<TSnapshot> _undoStack = new();
        private readonly List<TSnapshot> _redoStack = new();

        /// <summary>
        /// Adds a snapshot to the Undo history. Call it before changing the state of your document so we undo back to just-before that change. This resets Redo memory.
        /// </summary>
        public void Record()
        {
            _redoStack.Clear();
            _undoStack.Add(snapshotFactory());

            while (_undoStack.Count > maxRetention)
                _undoStack.RemoveAt(0);
        }

        public bool TryUndo([MaybeNullWhen(false)] out TSnapshot snapshot)
        {
            if (_undoStack.Any())
            {
                _redoStack.Add(snapshotFactory());
                snapshot = _undoStack.Last();
                _undoStack.RemoveAt(_undoStack.Count-1);
                return true;
            }
            snapshot = default;
            return false;
        }

        public bool TryRedo([MaybeNullWhen(false)] out TSnapshot snapshot)
        {
            if (_redoStack.Any())
            {
                _undoStack.Add(snapshotFactory());
                snapshot = _redoStack.Last();
                _redoStack.RemoveAt(_redoStack.Count - 1);
                return true;
            }
            snapshot = default;
            return false;
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
