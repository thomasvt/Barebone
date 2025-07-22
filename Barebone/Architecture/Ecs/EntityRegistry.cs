using System.Collections;

namespace Barebone.Architecture.Ecs;

internal class EntityRegistry : IEnumerable<EntityRegistry.Entry>
{
    private long _nextEntityId = 0;
    private readonly Dictionary<EntityId, Entry> _entityRegistry = new();

    public Entry Get(in EntityId entityId)
    {
        return _entityRegistry[entityId];
    }

    public bool Contains(in EntityId entityId)
    {
        return _entityRegistry.ContainsKey(entityId);
    }

    public EntityId Alloc(in Archetype archetype)
    {
        if (_nextEntityId == long.MaxValue) 
            throw new Exception("Oops. You ran out of entity ids. Didn't think that would happen."); // If this ever happens, we must change what we use for passing UserData to Box2D, because now we cheat by not passing in a handle but the raw id as if it is a pointer.
        var entityId = new EntityId(++_nextEntityId);
        _entityRegistry[entityId] = new Entry(entityId, archetype);
        return entityId;
    }

    public void ChangeArchetype(in EntityId entityId, Archetype archetype)
    {
        var entry = _entityRegistry[entityId];
        entry.Archetype = archetype;
        _entityRegistry[entityId] = entry;
    }

    public void Free(EntityId entityId)
    {
        _entityRegistry.Remove(entityId);
    }

    public void Clear()
    {
        _entityRegistry.Clear();
        _nextEntityId = 0;
    }

    public record struct Entry(EntityId EntityId, Archetype Archetype);

    public IEnumerator<Entry> GetEnumerator()
    {
        return _entityRegistry.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}