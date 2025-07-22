namespace Barebone.Architecture.Ecs;

public partial class ArchetypeRegistry
{
    /// <summary>
    /// Holds a unique power of two for each compotype's ID, so we can combine multiple compotypes into bit-fields as archetype for entities.
    /// </summary>
    private readonly Dictionary<Type, ComponentDef> _componentDefsByType = new(Archetype.MaxComponentCount);

    /// <summary>
    /// Sparse array holding all ComponentDefs with the array-idx == ComponentDef.Id.
    /// </summary>
    private readonly ComponentDef?[] _componentDefs = new ComponentDef[Archetype.MaxComponentCount];
    public int ComponentCount => _componentDefsByType.Count;

    /// <summary>
    /// Enumerates the COmponentDefs that apply to the given archetype.
    /// </summary>
    public IEnumerable<ComponentDef> GetComponentDefs(Archetype archetype)
    {
        for (var i = 0; i < Archetype.MaxComponentCount; i++)
        {
            if (archetype.Includes(i)) yield return _componentDefs[i]!;
        }
    }

    /// <summary>
    /// Defines a component. You must do this with all components at startup.
    /// </summary>
    public byte RegisterComponent<TCompo>() where TCompo : struct
    {
        var compoType = typeof(TCompo);
        if (_componentDefsByType.ContainsKey(compoType)) throw new Exception($"Component '{typeof(TCompo).Name}' is already registered.");

        var count = _componentDefsByType.Count;
        if (count == Archetype.MaxComponentCount) throw new Exception($"Max component types reached ({Archetype.MaxComponentCount}).");
        var archetype = Archetype.ForComponentId(count);
        var def = new ComponentDef<TCompo>((byte)count, archetype);
        _componentDefsByType.Add(compoType, def);
        _componentDefs[def.Id] = def;
        return def.Id;
    }

    /// <summary>
    /// Checks if a certain component is registered in the registry.
    /// </summary>
    public bool IsComponentRegistered<TCompo>() where TCompo : struct
    {
        return _componentDefsByType.ContainsKey(typeof(TCompo));
    }

    /// <summary>
    /// Gets the Def for the given component type. The same instance is always returned for the same component type.
    /// Assumes the ComponentDef already exists. 
    /// </summary>
    public ComponentDef<TCompo> GetComponentDef<TCompo>() where TCompo : struct
    {
#if DEBUG
        if (!IsComponentRegistered<TCompo>()) throw new Exception($"Component '{typeof(TCompo).Name}' is not registered.");
#endif
        return (ComponentDef<TCompo>)_componentDefsByType[typeof(TCompo)];
    }

    public void Clear()
    {
        _componentDefsByType.Clear();
        Array.Clear(_componentDefs);
    }

    public string ArchetypeToString(in Archetype archetype)
    {
        return string.Join("|", GetComponentDefs(archetype).Select(cd => cd.Name));
    }
}

