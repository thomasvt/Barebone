using System.Diagnostics.CodeAnalysis;

namespace Barebone.UI.Undo
{
    internal interface IUndoHistory<TSnapshot>
    {
        /// <summary>
        /// Adds a snapshot to the Undo history. Call it before changing the state of your document so we undo back to just-before that change. This resets Redo memory.
        /// </summary>
        void Record();

        bool TryUndo([MaybeNullWhen(false)] out TSnapshot snapshot);
        bool TryRedo([MaybeNullWhen(false)] out TSnapshot snapshot);
        void Clear();
    }
}
