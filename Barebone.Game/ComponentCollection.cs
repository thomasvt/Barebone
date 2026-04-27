namespace Barebone.Game
{
    /// <summary>
    /// Speciialized collection for game objects with low GC pressure, deferred collection mutations and support for Update and Draw operations on the actors.
    /// Actors must implement <see cref="IUpdate"/>, <see cref="IDraw"/>, <see cref="IOnAdded"/> and/or <see cref="IOnRemoved"/> to subscribe to those hooks.
    /// </summary>
    public class ComponentCollection : IDisposable
    {
        private readonly BBListDeferred<Component> _components = new()!;

        public T1 Add<T1>(T1 component) where T1: Component
        {
            _components.Add(component);
            return component;
        }

        /// <summary>
        /// Schedules to remove the entity upon next UpdateAll() call. Note that this will call its Dispose or will return it to the Pool.
        /// </summary>
        public void Remove(Component component)
        {
            _components.Remove(component);
        }

        /// <summary>
        /// Clears (not deferred) the collection. Calls Dispose on items that implement IDisposable, and returns IPoolable items to the pool.
        /// </summary>
        public void ClearImmediate()
        {
            _components.DisposeItems();
            _components.ClearImmediate(true, false);
        }

        /// <summary>
        /// Applies all deferred mutations and triggers Update() on all entities.
        /// </summary>
        public void UpdateAll()
        {
            _components.ApplyChanges(true);
            foreach (var entity in _components.AsReadOnlySpan())
                entity.Update();
        }

        /// <summary>
        /// Triggers Draw() on all entities.
        /// </summary>
        public void DrawAll()
        {
            foreach (var entity in _components.AsReadOnlySpan())
                entity.Draw();
        }

        public void Dispose()
        {
            _components.DisposeItems();
            _components.Dispose();
        }
    }
}
