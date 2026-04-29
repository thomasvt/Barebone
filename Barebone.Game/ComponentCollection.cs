namespace Barebone.Game
{
    /// <summary>
    /// Specialized collection for game objects with low GC pressure, deferred collection mutations and support for Update and Draw operations on the actors.
    /// </summary>
    internal class ComponentCollection : IDisposable, IComponentCollection
    {
        private readonly BBListDeferred<Component> _components;

        public ComponentCollection(Actor parent)
        {
            Parent = parent;
            _components = new()
            {
                OnAdded = OnComponentAdded,
                OnRemoving = OnComponentRemoving
            };
        }

        private void OnComponentRemoving(Component c)
        {
            (c as IDisposable)?.Dispose();
            c.Parent = null;
        }

        private void OnComponentAdded(Component c)
        {
            c.Parent = Parent;
            c.OnAdded();
        }

        public Component Parent { get; }

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

        public T? Find<T>() where T : Component
        {
            foreach (var c in _components.AsReadOnlySpan())
                if (c is T t) return t;

            return null;
        }

        public ReadOnlySpan<Component> AsSpan()
        {
            return _components.AsReadOnlySpan();
        }

        public void Dispose()
        {
            _components.DisposeItems();
            _components.Dispose();
        }
    }
}
