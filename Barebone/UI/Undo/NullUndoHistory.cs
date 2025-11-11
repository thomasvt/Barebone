using System.Diagnostics.CodeAnalysis;

namespace Barebone.UI.Undo
{
    internal class NullUndoHistory<TSnapshot> : IUndoHistory<TSnapshot>
    {
        public void Record()
        {
        }

        public bool TryUndo([MaybeNullWhen(false)] out TSnapshot snapshot)
        {
            snapshot = default;
            return false;
        }

        public bool TryRedo([MaybeNullWhen(false)] out TSnapshot snapshot)
        {
            snapshot = default;
            return false;
        }

        public void Clear()
        {
        }
    }
}
