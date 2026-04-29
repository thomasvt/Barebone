using System.Diagnostics.CodeAnalysis;

namespace Barebone.Game
{
    public abstract class Component
    {
        private static ulong _nextId = 1;
        public readonly ulong Id = _nextId++;

        public Component? Parent { get; internal set; }

        public virtual void Update()
        {}

        public virtual void Draw()
        {}

        public T FindAncestorOrThrow<T>() where T : Component
        {
            return FindAncestor<T>() ?? throw new Exception($"Failed to find ancestor of type {typeof(T).Name}.");
        }

        public bool TryFindAncestor<T>([MaybeNullWhen(false)] out T ancestor) where T : Component
        {
            ancestor = FindAncestor<T>();
            return ancestor != null;
        }

        public T? FindAncestor<T>() where T : Component
        {
            return Parent switch
            {
                null => null,
                T t => t,
                _ => Parent.FindAncestor<T>()
            };
        }

        public T FindSiblingOrThrow<T>() where T : Component
        {
            return FindSibling<T>() ?? throw new Exception($"Failed to find sibling component/actor of type {typeof(T).Name}.");
        }

        public bool TryFindSibling<T>([MaybeNullWhen(false)] out T sibling) where T : Component
        {
            sibling = FindSibling<T>();
            return sibling != null;
        }

        public T? FindSibling<T>() where T : Component
        {
            return Parent switch
            {
                Actor a => a.FindChild<T>(),
                _ => null
            };
        }

        public virtual void OnAdded()
        {
        }
    }
}
