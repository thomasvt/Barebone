using System.Collections;

namespace Barebone.Architecture.Ecs;

internal class EntityRegistry : IEnumerable<EntityRegistry.Entry>
{
    private long _nextEntityId = 0;
    internal readonly Dictionary<EntityId, Entry> _entitiesById = new();

    public Entry Get(in EntityId entityId)
    {
        return _entitiesById[entityId];
    }

    public bool Contains(in EntityId entityId)
    {
        return _entitiesById.ContainsKey(entityId);
    }

    public EntityId Alloc(in Archetype archetype)
    {
        if (_nextEntityId == long.MaxValue) 
            throw new Exception("Oops. You ran out of entity ids. Didn't think that would happen."); // If this ever happens, we must change what we use for passing UserData to Box2D, because now we cheat by not passing in a handle but the raw id as if it is a pointer.
        var entityId = new EntityId(++_nextEntityId);
        _entitiesById[entityId] = new Entry(entityId, archetype);
        return entityId;
    }

    public void ChangeArchetype(in EntityId entityId, Archetype archetype)
    {
        var entry = _entitiesById[entityId];
        entry.Archetype = archetype;
        _entitiesById[entityId] = entry;
    }

    public void Free(EntityId entityId)
    {
        _entitiesById.Remove(entityId);
    }

    public void Clear()
    {
        _entitiesById.Clear();
        _nextEntityId = 0;
    }

    public record struct Entry(EntityId EntityId, Archetype Archetype);

    public IEnumerator<Entry> GetEnumerator()
    {
        return _entitiesById.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}