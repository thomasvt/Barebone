namespace Barebone.Architecture.Ecs;

public abstract class ComponentDef
{
    public byte Id { get; }
    public Archetype Archetype { get; }
    public string Name { get; }

    internal ComponentDef(byte id, Archetype archetype, string name)
    {
        Id = id;
        Archetype = archetype;
        Name = name;
    }

    /// <summary>
    /// Returns an instance of the strongly typed generic version of ComponentSet
    /// </summary>
    internal abstract IComponentSet CreateGenericComponentSet();

    internal abstract bool HasOnRemove();
    internal abstract bool HasOnAdd();

    internal abstract void InvokeOnRemoveCallback(in EntityId entityId, in EntitySet entitySet);
    internal abstract void InvokeOnAddCallback(in EntityId entityId, in EntitySet entitySet);
}

public class ComponentDef<TCompo>(byte id, Archetype archetype) : ComponentDef(id, archetype, typeof(TCompo).Name) where TCompo : struct
{
    internal override IComponentSet CreateGenericComponentSet()
    {
        return new ComponentSet<TCompo>();
    }

    internal override bool HasOnRemove() => OnRemoveCallback != null;
    internal override bool HasOnAdd() => OnAddCallback != null;
    
    internal override void InvokeOnRemoveCallback(in EntityId entityId, in EntitySet entitySet)
    {
        OnRemoveCallback!.Invoke(entityId, ref entitySet.GetRef<TCompo>(entityId));
    }

    internal override void InvokeOnAddCallback(in EntityId entityId, in EntitySet entitySet)
    {
        OnAddCallback!.Invoke(entityId, ref entitySet.GetRef<TCompo>(entityId));
    }

    public ComponentAction<TCompo>? OnRemoveCallback { get; set; }
    
    public ComponentAction<TCompo>? OnAddCallback { get; set; }
}
