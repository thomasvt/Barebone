namespace Barebone.Game
{
    public abstract class Component : IDisposable
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

        public T? FindAncestor<T>() where T : Component
        {
            return Parent switch
            {
                null => null,
                T t => t,
                _ => Parent.FindAncestor<T>()
            };
        }

        public virtual void Dispose()
        {
        }
    }
}
