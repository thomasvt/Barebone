using System.Collections;

namespace Barebone.Game
{
    public interface IComponentCollection
    {
        Component Parent { get; }
        T1 Add<T1>(T1 component) where T1: Component;

        /// <summary>
        /// Schedules to remove the entity upon next UpdateAll() call. Note that this will call its Dispose or will return it to the Pool.
        /// </summary>
        void Remove(Component component);

        /// <summary>
        /// Clears (not deferred) the collection. Calls Dispose on items that implement IDisposable, and returns IPoolable items to the pool.
        /// </summary>
        void ClearImmediate();

        /// <summary>
        /// Applies all deferred mutations and triggers Update() on all entities.
        /// </summary>
        void UpdateAll();

        /// <summary>
        /// Triggers Draw() on all entities.
        /// </summary>
        void DrawAll();

        T? Find<T>() where T : Component;
        ReadOnlySpan<Component> AsSpan();
    }
}
