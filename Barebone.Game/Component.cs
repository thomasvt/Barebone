namespace Barebone.Game
{
    public abstract class Component : IDisposable
    {
        private static ulong _nextId = 1;
        public readonly ulong Id = _nextId++;

        public virtual void Update()
        {}

        public virtual void Draw()
        {}

        public virtual void Dispose()
        {
        }
    }
}
