using Barebone.Pools;

namespace Barebone.Game
{
    public sealed class ActorCollection : ActorCollection<object>;

    /// <summary>
    /// Speciialized collection for game objects with low GC pressure, deferred collection mutations and support for Update and Draw operations on the actors.
    /// Actors must implement <see cref="IUpdate"/>, <see cref="IDraw"/>, <see cref="IOnAdded"/> and/or <see cref="IOnRemoved"/> to subscribe to those hooks.
    /// </summary>
    public class ActorCollection<T> : Poolable, IDisposable where T : class
    {
        private BBListDeferred<T> _entities = null!;

        public ActorCollection()
        {
            Construct();
        }

        protected override void Construct()
        {
            _entities?.Return(true);
            _entities = Pool.Rent<BBListDeferred<T>>();
        }

        protected override void Destruct()
        {
            _entities?.Return(true);
            _entities = null!;
        }

        public void Add(T entity)
        {
            _entities.Add(entity);
        }

        public void Remove(T entity)
        {
            _entities.Remove(entity);
        }

        /// <summary>
        /// Clears (not deferred) the collection. Calls Dispose on items that implement IDisposable, and returns IPoolable items to the pool.
        /// </summary>
        public void ClearImmediate()
        {
            _entities.DisposeItems();
            _entities.ClearImmediate(true, false);
        }

        /// <summary>
        /// Applies all deferred mutations and triggers Update() on all entities.
        /// </summary>
        public void UpdateAll()
        {
            _entities.ApplyChanges(true);
            foreach (var entity in _entities.AsReadOnlySpan())
                (entity as IUpdate)?.Update();
        }

        /// <summary>
        /// Triggers Draw() on all entities.
        /// </summary>
        public void DrawAll()
        {
            foreach (var entity in _entities.AsReadOnlySpan())
                (entity as IDraw)?.Draw();
        }

        public void Dispose()
        {
            _entities.DisposeItems();
            _entities.Dispose();
        }

        
    }
}
