namespace Barebone.Architecture.Ecs
{
    internal interface IComponentAdditionQueue
    {
        void Process();
    }

    internal class SetComponentQueue<T>(EcsScene scene) : IComponentAdditionQueue where T : struct
    {
        private readonly List<(EntityId, T)> _queue = new();

        public void Add(EntityId entityId, T value)
        {
            _queue.Add((entityId, value));
        }

        /// <summary>
        /// Writes all queued components and clears the queue.
        /// </summary>
        public void Process()
        {
            foreach (var (id, value) in _queue)
            {
                scene.SetComponent(id, value);
            }
            _queue.Clear();
        }
    }
    internal record struct ComponentRemove(EntityId EntityId, ComponentDef ComponentDef);

    public partial class EcsScene
    {

        private readonly HashSet<EntityId> _removeQueue = new();
        private readonly List<ComponentRemove> _componentRemoves = new();
        /// <summary>
        /// sparse array by ComponentId with strongly typed <see cref="SetComponentQueue{T}"/>. Pre-populated by EcsScene.RegisterComponent()
        /// </summary>
        private readonly IComponentAdditionQueue?[] _componentAdditionQueues = new IComponentAdditionQueue[Archetype.MaxComponentCount];

        /// <summary>
        /// Removes the given entity at the end of the current frame.
        /// </summary>
        /// <param name="entityId"></param>
        public void RemoveEntityAfterFrame(in EntityId entityId)
        {
            _removeQueue.Add(entityId); // enqueues only when not already there
        }

        /// <summary>
        /// Removes a component from the given entity if it exists, at the end of the current frame.
        /// </summary>
        public void RemoveComponentAfterFrame<TCompo>(in EntityId entityId) where TCompo : struct
        {
            var compoDef = ArchetypeRegistry.GetComponentDef<TCompo>();
            _componentRemoves.Add(new ComponentRemove(entityId, compoDef));
        }

        /// <summary>
        /// Adds or overwrites the value of a component of an entity, at the end of the current frame.
        /// </summary>
        public void SetComponentAfterFrame<TC>(in EntityId entityId, TC value = default) where TC : struct
        {
            var compoDef = ArchetypeRegistry.GetComponentDef<TC>();
            ((SetComponentQueue<TC>)_componentAdditionQueues[compoDef.Id]!).Add(entityId, value);
        }

        private void ProcessPostFrame()
        {
            foreach (var componentRemove in _componentRemoves)
            {
                if (HasComponent(componentRemove.EntityId, componentRemove.ComponentDef))
                    RemoveComponent(componentRemove.EntityId, componentRemove.ComponentDef);
            }
            _componentRemoves.Clear();

            for (var i = 0; i < Archetype.MaxComponentCount; i++)
            {
                _componentAdditionQueues[i]?.Process();
            }

            
            foreach (var entityId in _removeQueue)
            {
                // No exists check: the HashSet protects prevents duplicate enqueueing.
                RemoveEntity(entityId);
            }
            _removeQueue.Clear();
        }
    }
}
