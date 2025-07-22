using System.Buffers;
using System.Runtime.CompilerServices;

namespace Barebone.Architecture.Ecs;

/// <summary>
/// Includes all component data of all entities of 1 certain Archetype. 
/// </summary>
internal class EntitySet : IDisposable
{
    public const int InitialCapacity = 4;

    private readonly ArchetypeRegistry _archetypeRegistry;

    private readonly Dictionary<EntityId, int> _idxByEntityId = new();

    // cached componentDef listing for the Archetype of this EntitySet, because they are often needed and are fixed
    private readonly ComponentDef[] _componentDefs;
    private readonly bool _hasOnRemoveCallback;
    private int _entityCount = 0;
    private EntityId[] _entityIndex;

    /// <summary>
    /// Sparse array of ComponentSets but only for the components that the Archetype of this EntityPool contains.
    /// The array idx == ComponentId.Value.
    /// </summary>
    private readonly IComponentSet?[] _componentSets = new IComponentSet[Archetype.MaxComponentCount];

    public Archetype Archetype { get; }

    public EntitySet(ArchetypeRegistry archetypeRegistry, in Archetype archetype)
    {
        _archetypeRegistry = archetypeRegistry;
        Archetype = archetype;
        _componentDefs = _archetypeRegistry.GetComponentDefs(archetype).ToArray();
        _hasOnRemoveCallback = _componentDefs.Any(cd => cd.HasOnRemove());

        // init arrays:
        _entityIndex = ArrayPool<EntityId>.Shared.Rent(InitialCapacity);
        foreach (var componentDef in _componentDefs)
        {
            var componentPool = componentDef.CreateGenericComponentSet();
            _componentSets[componentDef.Id] = componentPool;
        }
    }

    /// <summary>
    /// Allocates space for the new entity. Component data for the entity is still uninitialized after this. 
    /// </summary>
    public int Add(EntityId entityId)
    {
        if (_entityIndex.Length == _entityCount)
            GrowEntityCapacity();

        var idx = _entityCount;
        _entityIndex[idx] = entityId;
        _entityCount++;
        _idxByEntityId[entityId] = idx;
        return idx;
    }

    /// <summary>
    /// Doubles the capacity on the entity index and component arrays.
    /// </summary>
    private void GrowEntityCapacity()
    {
        var newCapacity = _entityIndex.Length * 2;
        ArrayExt.GrowPoolArray(ref _entityIndex, newCapacity, _entityCount);
        foreach (var componentDef in _componentDefs)
            _componentSets[componentDef.Id]!.GrowCapacity(newCapacity, _entityCount);
    }

    /// <summary>
    /// Moves the entity to another EntitySet:
    /// Adds it to the destination EntitySet with the components both Archetypes have in common and removes the Entity in this one.
    /// </summary>
    public void Move(in EntityId entityId, EntitySet destinationSet)
    {
        var srcIdx = _idxByEntityId[entityId];
        var destIdx = destinationSet.Add(entityId);

        var destinationArchetype = destinationSet.Archetype;

        foreach (var componentDef in _componentDefs)
        {
            if (!destinationArchetype.Includes(componentDef))
                continue;

            var sourceComponentArray = GetComponentSetUnsafe(componentDef);
            var destinationComponentArray = destinationSet.GetComponentSetUnsafe(componentDef);

            sourceComponentArray.Copy(srcIdx, destinationComponentArray, destIdx);
        }

        // remove without triggering callbacks, because the entity is not removed, only moved.
        SwapRemoveInternal(entityId);
    }

    /// <summary>
    /// Removes an entity and its component data by swapping the last entity in the collection to the place of the entity to be deleted. 
    /// </summary>
    public void SwapRemove(in EntityId entityId)
    {
        if (_hasOnRemoveCallback)
            InvokeOnRemoveCallbacks(entityId);

        SwapRemoveInternal(entityId);
    }

    private void SwapRemoveInternal(EntityId entityId)
    {
        var idx = _idxByEntityId[entityId];
        var lastIdx = _entityCount - 1;
        if (idx < lastIdx)
        {
            // swapremove = move the last entity over the one we want to delete.
            var lastEntityId = _entityIndex[lastIdx];
            _idxByEntityId[lastEntityId] = idx;
            // do the copying of data from last to 'idx'
            _entityIndex[idx] = lastEntityId;
            foreach (var componentDef in _componentDefs)
                GetComponentSetUnsafe(componentDef).Copy(lastIdx, idx);
        }

        _entityCount--; // because the item to delete is now guaranteed to be the last in the arrays, decreasing the count effectively removes it.
        _idxByEntityId.Remove(entityId); // ... and not forgetting the idx lookup
    }

    private void InvokeOnRemoveCallbacks(EntityId entityId)
    {
        foreach (var componentDef in _componentDefs)
            if (componentDef.HasOnRemove())
                componentDef.InvokeOnRemoveCallback(in entityId, this);
    }

    /// <summary>
    /// Sets the value of a component for an entity to zeros. Assumes the entity has that component.
    /// </summary>
    public void ClearComponentUnsafe(in EntityId entityId, ComponentDef compoDef, bool triggerOnAddCallback)
    {
        var idx = _idxByEntityId[entityId];
        var componentSet = _componentSets[compoDef.Id];
#if DEBUG
        if (componentSet == null)
            throw new Exception($"Entity {entityId} does not have component '{compoDef.Name}'.");
#endif
        componentSet.ClearItem(idx);
        if (triggerOnAddCallback && compoDef.HasOnAdd())
            compoDef.InvokeOnAddCallback(in entityId, this);
    }

    /// <summary>
    /// Sets the value of a component for an entity. Assumes the entity has that component.
    /// </summary>
    public void SetUnsafe<TCompo>(in EntityId entityId, in TCompo value, bool triggerOnAddCallback) where TCompo : struct
    {
        var compoDef = _archetypeRegistry.GetComponentDef<TCompo>();
        var idx = _idxByEntityId[entityId];
        var componentSet = _componentSets[compoDef.Id];
#if DEBUG
        if (componentSet == null)
            throw new Exception($"Entity {entityId} does not have component '{typeof(TCompo).Name}'.");
#endif
        ((ComponentSet<TCompo>)componentSet!).Set(idx, in value);
        if (triggerOnAddCallback && compoDef.HasOnAdd())
            compoDef.InvokeOnAddCallback(in entityId, this);
    }
    
    public void Clear()
    {
        if (_hasOnRemoveCallback)
            foreach (var entityId in GetEntityIdSpan())
                InvokeOnRemoveCallbacks(entityId);

        _idxByEntityId.Clear();
        Array.Clear(_entityIndex);
        _entityCount = 0;
        foreach (var componentPool in _componentSets)
            componentPool?.Clear();
    }

    /// <summary>
    /// Returns a Span to the components. ComponentDef is passed in for performance because calling code already has that. Could be extended with an overload that does the lookup itself from TCompo.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TCompo> GetComponentSpan<TCompo>(ComponentDef componentDef) where TCompo : struct
    {
        return ((ComponentSet<TCompo>)_componentSets[componentDef.Id]!).AsSpan(_entityCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<EntityId> GetEntityIdSpan()
    {
        return _entityIndex.AsSpan(0, _entityCount);
    }

    /// <summary>
    /// Returns the componentArray for the componentDef. There is no checking if that component belongs to this Archetype. If so, you will receive null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IComponentSet GetComponentSetUnsafe(in ComponentDef componentDef)
    {
        return _componentSets[componentDef.Id]!;
    }
    
    /// <summary>
    /// Returns the componentArray for the componentDef. There is no checking if that component belongs to this Archetype. If so, you will receive null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IComponentSet GetComponentSetUnsafe<TCompo>(in ComponentDef componentDef) where TCompo : struct
    {
        return (ComponentSet<TCompo>)_componentSets[componentDef.Id]!;
    }

    /// <summary>
    /// Returns a specific component of an entity. Assumes the component exists in this EntitySet.
    /// </summary>
    public ref TCompo GetRef<TCompo>(in EntityId entityId) where TCompo : struct
    {
        var compoDef = _archetypeRegistry.GetComponentDef<TCompo>();
#if DEBUG
        if (!Archetype.Includes(compoDef))
            throw new Exception($"Entity {entityId} does not have a component '{typeof(TCompo).Name}'.");
#endif
        var idx = _idxByEntityId[entityId];
        return ref ((ComponentSet<TCompo>)_componentSets[compoDef.Id]!).Get(idx);
    }

    public void Dispose()
    {
        foreach (var componentSet in _componentSets)
        {
            componentSet?.Dispose();
        }
    }
}