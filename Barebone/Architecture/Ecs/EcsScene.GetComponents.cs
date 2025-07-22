namespace Barebone.Architecture.Ecs;

public partial class EcsScene
{

    public bool Exists(in EntityId entityId)
    {
        return _entityRegistry.Contains(entityId);
    }

    /// <summary>
    /// Returns a ref to a single component of a single entity. Assumes that entity has that component.
    /// </summary>
    public ref TCompo GetComponentRef<TCompo>(in EntityId entityId) where TCompo : struct
    {
        var archetype = _entityRegistry.Get(entityId).Archetype;
        var entityPool = _entitySetsByArchetype[archetype];
        return ref entityPool.GetRef<TCompo>(in entityId);
    }

    public bool HasComponent<TC1>(in EntityId entityId) where TC1 : struct
    {
        if (!_entityRegistry.Contains(entityId)) return false;
        var archetype = _entityRegistry.Get(entityId).Archetype;
        return archetype.IncludesAll(ArchetypeRegistry.ComposeArchetype<TC1>());
    }

    public bool HasComponent(in EntityId entityId, ComponentDef compoDef)
    {
        var archetype = _entityRegistry.Get(entityId).Archetype;
        return archetype.Includes(compoDef);
    }
}