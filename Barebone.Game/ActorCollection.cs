using Barebone.Pools;

namespace Barebone.Game
{
    /// <summary>
    /// Speciialized collection for game objects with low GC pressure, deferred collection mutations and support for Update and Draw operations on the actors.
    /// Actors must implement <see cref="IUpdate"/>, <see cref="IDraw"/>, <see cref="IOnAdded"/> and/or <see cref="IOnRemoved"/> to subscribe to those hooks.
    /// </summary>
    public class ActorCollection : Poolable, IDisposable
    {
        private BBListDeferred<object> _entities = null!;

        public ActorCollection()
        {
            Construct();
        }

        protected override void Construct()
        {
            _entities?.Return(true);
            _entities = Pool.Rent<BBListDeferred<object>>();
        }

        protected override void Destruct()
        {
            _entities?.Return(true);
            _entities = null!;
        }

        public T1 Add<T1>(T1 entity) where T1: class
        {
            _entities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Schedules to remove the entity upon next UpdateAll() call. Note that this will call its Dispose or will return it to the Pool.
        /// </summary>
        public void Remove(object entity)
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
