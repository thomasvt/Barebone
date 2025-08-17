namespace Barebone.Architecture.Ecs
{
    internal interface IAddComponentQueue
    {
        void Process();
    }

    internal class AddAddComponentQueue<T>(EcsScene scene) : IAddComponentQueue where T : struct
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
                scene.AddComponent(id, value);
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
        /// sparse array by ComponentId with strongly typed <see cref="AddAddComponentQueue{T}"/>. Pre-populated by EcsScene.RegisterComponent()
        /// </summary>
        private readonly IAddComponentQueue?[] _addComponentQueues = new IAddComponentQueue[Archetype.MaxComponentCount];

        /// <summary>
        /// Schedules to remove the given entity at the end of the current frame.
        /// </summary>
        /// <param name="entityId"></param>
        public void RemoveEntityAfterFrame(in EntityId entityId)
        {
            _removeQueue.Add(entityId); // enqueues only when not already there
        }

        /// <summary>
        /// Schedules to remove a component from the given entity (if it exists) at the end of the current frame.
        /// </summary>
        public void RemoveComponentAfterFrame<TCompo>(in EntityId entityId) where TCompo : struct
        {
            var compoDef = ArchetypeRegistry.GetComponentDef<TCompo>();
            _componentRemoves.Add(new ComponentRemove(entityId, compoDef));
        }

        /// <summary>
        /// Schedules to add the new component to an entity at the end of the current frame.
        /// </summary>
        public void AddComponentAfterFrame<TC>(in EntityId entityId, TC value = default) where TC : struct
        {
            var compoDef = ArchetypeRegistry.GetComponentDef<TC>();
            ((AddAddComponentQueue<TC>)_addComponentQueues[compoDef.Id]!).Add(entityId, value);
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
                _addComponentQueues[i]?.Process();
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
