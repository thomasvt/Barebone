namespace Barebone.Architecture.Ecs;

public partial class EcsScene
{
    /// <summary>
    /// Sets the value of a single component of a single entity.
    /// If the entity did not have the component yet, it is added (and therefore changes Archetype, which is quite costly)
    /// </summary>
    public void SetComponent<TCompo>(in EntityId entityId, in TCompo compo) where TCompo : struct
    {
        var archetype = _entityRegistry.Get(entityId).Archetype;
        var compoDef = ArchetypeRegistry.GetComponentDef<TCompo>();
        if (!archetype.Includes(compoDef))
        {
            // compo is not part of entity's archetype. We must change archetype.
            var destinationEntitySet = ChangeArchetype(in entityId, in archetype, archetype.Add(compoDef));
            destinationEntitySet.SetUnsafe(in entityId, in compo, true);
        }
        else
        {
            // overwrite the already existing component
            var entitySet = GetEntitySet(archetype);
            entitySet.SetUnsafe(in entityId, in compo, false);
        }
    }

    /// <summary>
    /// Removes a single component from a single entity.
    /// This relocates the entity to another EntitySet because its Archetype changes.
    /// </summary>
    public void RemoveComponent<TCompo>(in EntityId entityId) where TCompo : struct
    {
        var compoDef = ArchetypeRegistry.GetComponentDef<TCompo>();
        RemoveComponent(entityId, compoDef);
    }

    /// <summary>
    /// Removes a single component from a single entity.
    /// This relocates the entity to another EntitySet because its Archetype changes.
    /// </summary>
    public void RemoveComponent(in EntityId entityId, in ComponentDef compoDef)
    {
        var archetype = _entityRegistry.Get(entityId).Archetype;
#if DEBUG
        if (!archetype.Includes(compoDef))
            throw new Exception($"Cannot remove component '{compoDef.Name}' from Entity {entityId} because it does not have such a component.");
#endif
        var newArchetype = archetype.Remove(compoDef);
#if DEBUG
        if (newArchetype == Archetype.Empty)
            throw new Exception($"Don't remove the last component of Entity {entityId}. Remove the entity itself instead.");
#endif

        if (compoDef.HasOnRemove())
            compoDef.InvokeOnRemoveCallback(in entityId, GetEntitySet(archetype));
        ChangeArchetype(entityId, archetype, newArchetype);
    }

    /// <summary>
    /// Changes the archetype of an entity by moving it to the correct EntitySet. Returns the destination entityset.
    /// </summary>
    private EntitySet ChangeArchetype(in EntityId entityId, in Archetype currentArchetype, in Archetype newArchetype)
    {
        var sourceEntitySet = GetEntitySet(currentArchetype);
        var destinationEntitySet = GetOrCreateEntitySet(newArchetype);
        sourceEntitySet.Move(entityId, destinationEntitySet);
        _entityRegistry.ChangeArchetype(entityId, newArchetype);
        return destinationEntitySet;
    }
}