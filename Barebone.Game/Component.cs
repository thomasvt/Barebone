using System.Diagnostics.CodeAnalysis;

namespace Barebone.Game
{
    public abstract class Component
    {
        private static ulong _nextId = 1;
        public readonly ulong Id = _nextId++;

        internal Actor? ParentInternal { get; set; }
        public Actor Parent => ParentInternal ?? throw new Exception($"Component {this} has no parent (yet).");

        public virtual void OnAdded()
        {
        }

        public virtual void Update()
        {}

        public virtual void Draw()
        {}

        public T FindAncestor<T>() where T : Component
        {
            return FindAncestorOrNull<T>() ?? throw new Exception($"Failed to find ancestor of type {typeof(T).Name}.");
        }

        public bool TryFindAncestor<T>([MaybeNullWhen(false)] out T ancestor) where T : Component
        {
            ancestor = FindAncestorOrNull<T>();
            return ancestor != null;
        }

        public T? FindAncestorOrNull<T>() where T : Component
        {
            return ParentInternal switch
            {
                null => null,
                T t => t,
                _ => ParentInternal.FindAncestorOrNull<T>()
            };
        }

        public T FindSibling<T>() where T : Component
        {
            return FindSiblingOrNull<T>() ?? throw new Exception($"Failed to find sibling component/actor of type {typeof(T).Name}.");
        }

        public bool TryFindSibling<T>([MaybeNullWhen(false)] out T sibling) where T : Component
        {
            sibling = FindSiblingOrNull<T>();
            return sibling != null;
        }

        public T? FindSiblingOrNull<T>() where T : Component
        {
            return ParentInternal switch
            {
                Actor a => a.GetChildOrNull<T>(),
                _ => null
            };
        }

        public override string ToString()
        {
            return $"{GetType().Name}:{Id}";
        }
    }
}
