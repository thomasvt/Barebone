namespace Barebone.Architecture.Ecs;

public partial class EcsScene
{
    private EntityId CreateEntityInternal(in Archetype archetype)
    {
        var entityId = _entityRegistry.Alloc(in archetype);
        var pool = GetOrCreateEntitySet(archetype);
        pool.Add(entityId);
        return entityId;
    }
    
    public EntityId CreateEntity<TC1>(TC1 value) where TC1 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value, true);
        return entityId;
    }
    
    public EntityId CreateEntity<TC1, TC2>(TC1 value1, TC2 value2) where TC1 : struct where TC2 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        return entityId;
    }
    
    public EntityId CreateEntity<TC1, TC2, TC3>(TC1 value1, TC2 value2, TC3 value3) 
        where TC1 : struct 
        where TC2 : struct
        where TC3 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        return entityId;
    }

    public EntityId CreateEntity<TC1, TC2, TC3, TC4>(TC1 value1, TC2 value2, TC3 value3, TC4 value4)
        where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3, TC4>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        entityPool.SetUnsafe(entityId, value4, true);
        return entityId;
    }
    
    public EntityId CreateEntity<TC1, TC2, TC3, TC4, TC5>(TC1 value1, TC2 value2, TC3 value3, TC4 value4, TC5 value5)
        where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
        where TC5 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3, TC4, TC5>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        entityPool.SetUnsafe(entityId, value4, true);
        entityPool.SetUnsafe(entityId, value5, true);
        return entityId;
    }

    public EntityId CreateEntity<TC1, TC2, TC3, TC4, TC5, TC6>(TC1 value1, TC2 value2, TC3 value3, TC4 value4, TC5 value5, TC6 value6)
        where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
        where TC5 : struct
        where TC6 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3, TC4, TC5, TC6>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        entityPool.SetUnsafe(entityId, value4, true);
        entityPool.SetUnsafe(entityId, value5, true);
        entityPool.SetUnsafe(entityId, value6, true);
        return entityId;
    }

    public EntityId CreateEntity<TC1, TC2, TC3, TC4, TC5, TC6, TC7>(TC1 value1, TC2 value2, TC3 value3, TC4 value4, TC5 value5, TC6 value6, TC7 value7)
        where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
        where TC5 : struct
        where TC6 : struct
        where TC7 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3, TC4, TC5, TC6, TC7>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        entityPool.SetUnsafe(entityId, value4, true);
        entityPool.SetUnsafe(entityId, value5, true);
        entityPool.SetUnsafe(entityId, value6, true);
        entityPool.SetUnsafe(entityId, value7, true);
        return entityId;
    }

    public EntityId CreateEntity<TC1, TC2, TC3, TC4, TC5, TC6, TC7, TC8>(TC1 value1, TC2 value2, TC3 value3, TC4 value4, TC5 value5, TC6 value6, TC7 value7, TC8 value8)
        where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
        where TC5 : struct
        where TC6 : struct
        where TC7 : struct
        where TC8 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3, TC4, TC5, TC6, TC7, TC8>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        entityPool.SetUnsafe(entityId, value4, true);
        entityPool.SetUnsafe(entityId, value5, true);
        entityPool.SetUnsafe(entityId, value6, true);
        entityPool.SetUnsafe(entityId, value7, true);
        entityPool.SetUnsafe(entityId, value8, true);
        return entityId;
    }

    public EntityId CreateEntity<TC1, TC2, TC3, TC4, TC5, TC6, TC7, TC8, TC9>(TC1 value1, TC2 value2, TC3 value3, TC4 value4, TC5 value5, TC6 value6, TC7 value7, TC8 value8, TC9 value9)
        where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
        where TC5 : struct
        where TC6 : struct
        where TC7 : struct
        where TC8 : struct
        where TC9 : struct
    {
        var archetype = ArchetypeRegistry.ComposeArchetype<TC1, TC2, TC3, TC4, TC5, TC6, TC7, TC8, TC9>();
        var entityId = CreateEntityInternal(archetype);
        var entityPool = GetEntitySet(archetype);
        entityPool.SetUnsafe(entityId, value1, true);
        entityPool.SetUnsafe(entityId, value2, true);
        entityPool.SetUnsafe(entityId, value3, true);
        entityPool.SetUnsafe(entityId, value4, true);
        entityPool.SetUnsafe(entityId, value5, true);
        entityPool.SetUnsafe(entityId, value6, true);
        entityPool.SetUnsafe(entityId, value7, true);
        entityPool.SetUnsafe(entityId, value8, true);
        entityPool.SetUnsafe(entityId, value9, true);
        return entityId;
    }

    public void RemoveEntity(EntityId entityId)
    {
        var entry = _entityRegistry.Get(entityId);
        var entitySet = GetEntitySet(entry.Archetype);
        entitySet.SwapRemove(entityId);
        _entityRegistry.Free(entityId);
    }
}

