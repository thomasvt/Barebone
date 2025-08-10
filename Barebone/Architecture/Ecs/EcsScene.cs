using System.Runtime.CompilerServices;
using System.Text;
using Barebone.Architecture.Ecs.Components;

namespace Barebone.Architecture.Ecs
{
    public partial class EcsScene : IDisposable
    {
        /// <summary>
        /// Max number of component types this ECS system supports.
        /// </summary>
        public int MaxComponentCount => Archetype.MaxComponentCount;
        
        private readonly EntityRegistry _entityRegistry = new();
        public ArchetypeRegistry ArchetypeRegistry { get; } = new();

        private readonly Dictionary<Archetype, EntitySet> _entitySetsByArchetype = new();

        private readonly List<EntitySet> _entitySets = new();

        public EcsScene()
        {
            RegisterComponent<DisabledCompo>();
            // Clear();
        }
        
        public void Dispose()
        {
            foreach (var entitySet in _entitySets) 
            {
                entitySet.Dispose();
            }
        }

        public void Update()
        {
            UpdateSystems();
            ProcessPostFrame();
        }

        public void SubscribeOnRemove<TCompo>(ComponentAction<TCompo> callback) where TCompo : struct
        {
            ArchetypeRegistry.GetComponentDef<TCompo>().OnRemoveCallback = callback;
        }

        public void SubscribeOnAdd<TCompo>(ComponentAction<TCompo> callback) where TCompo : struct
        {
            ArchetypeRegistry.GetComponentDef<TCompo>().OnAddCallback = callback;
        }

        /// <summary>
        /// Removes all entities but not registered components.
        /// </summary>
        public void Clear()
        {
            foreach (var entry in _entityRegistry.ToList()) // Clear calls are low frequency, so this alloc is fine.
            {
                RemoveEntity(entry.EntityId);
            }
            //ArchetypeRegistry.Clear();
            //_entitySets.Clear();
            //_entitySetsByArchetype.Clear();

            // RegisterComponent<DisabledCompo>();
        }

        private EntitySet GetOrCreateEntitySet(in Archetype archetype)
        {
            if (!_entitySetsByArchetype.TryGetValue(archetype, out var pool))
            {
                pool = new EntitySet(ArchetypeRegistry, archetype);
                _entitySetsByArchetype[archetype] = pool;
                _entitySets.Add(pool);
            }

            return pool;
        }

        /// <summary>
        /// Assumes the entity set already exists. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EntitySet GetEntitySet(in Archetype archetype)
        {
            return _entitySetsByArchetype[archetype];
        }

        /// <summary>
        /// Logs some info about all entities that match the archetypemask.
        /// </summary>
        public string LogMatches(Archetype archetypeFilter)
        {
            var sb = new StringBuilder();
            foreach (var entityEntry in _entityRegistry)
            {
                if (entityEntry.Archetype.IncludesAll(archetypeFilter))
                    sb.AppendLine($"{entityEntry.EntityId} ({ArchetypeRegistry.ArchetypeToString(entityEntry.Archetype)})");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Informs the ECS framework of a component type. You must do this at startup for all components you're going to use.
        /// </summary>
        public void RegisterComponent<TCompo>() where TCompo : struct
        {
            var id = ArchetypeRegistry.RegisterComponent<TCompo>();
            _componentAdditionQueues[id] = new SetComponentQueue<TCompo>(this);
        }

        public Archetype GetArchetype(EntityId entityId)
        {
            return _entityRegistry.Get(entityId).Archetype;
        }

        public int ComponentCount => ArchetypeRegistry.ComponentCount;

        public string GetEntityListing()
        {
            var sb = new StringBuilder();
            foreach (var entry in _entityRegistry._entitiesById.Values)
            {
                sb.AppendLine($"{entry.EntityId.Value} [{string.Join(", ", ArchetypeRegistry.GetComponentDefs(entry.Archetype).Select(cd => cd.Name))}]");
            }
            return sb.ToString();
        }
    }
}
