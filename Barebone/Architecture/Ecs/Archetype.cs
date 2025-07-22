using System.Runtime.CompilerServices;

namespace Barebone.Architecture.Ecs;

/// <summary>
/// A full or partial archetype is a composition of component types.
/// </summary>
public readonly record struct Archetype
{
    public static int MaxComponentCount = 128;

    private readonly ulong _mask1, _mask2;

    private Archetype(ulong mask1, ulong mask2)
    {
        _mask1 = mask1;
        _mask2 = mask2;
    }

    public readonly static Archetype Empty = new(0ul, 0ul);

    /// <summary>
    /// Returns an archetype based on this one, but with the given flags set to 1.
    /// </summary>
    public Archetype Add(in ComponentDef componentDef)
    {
        return new Archetype(_mask1 | componentDef.Archetype._mask1, _mask2 | componentDef.Archetype._mask2);
    }

    /// <summary>
    /// Returns an archetype based on this one, but with the given flags set to 0.
    /// </summary>
    public Archetype Remove(in ComponentDef componentDef)
    {
        return new Archetype(_mask1 & ~componentDef.Archetype._mask1, _mask2 & ~componentDef.Archetype._mask2);
    }

    /// <summary>
    /// Returns true if this archetype includes all components in 'partialArchetype'.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IncludesAll(in Archetype partialArchetype)
    {
        return (_mask1 & partialArchetype._mask1) == partialArchetype._mask1 && (_mask2 & partialArchetype._mask2) == partialArchetype._mask2;
    }

    /// <summary>
    /// Returns true if this archetype includes all components in 'partialArchetype'.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IncludesAny(in Archetype partialArchetype)
    {
        return (_mask1 & partialArchetype._mask1) != 0 || (_mask2 & partialArchetype._mask2) != 0;
    }

    /// <summary>
    /// Returns true if this archetype includes the given component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Includes(in ComponentDef componentDef)
    {
        return (_mask1 & componentDef.Archetype._mask1) != 0 || (_mask2 & componentDef.Archetype._mask2) != 0;
    }

    public override string ToString()
    {
        var t = this;
        var componentIds = Enumerable.Range(0, Archetype.MaxComponentCount)
            .Where(id => t.Includes(id));
        return string.Join(", ", componentIds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Archetype Union(Archetype extraArchetype)
    {
        return new Archetype(_mask1 | extraArchetype._mask1, _mask2 | extraArchetype._mask2);
    }


    /// <summary>
    /// Checks if this archetype contains the given component (by its numerical id).
    /// </summary>
    public bool Includes(in int componentId)
    {
        if (componentId < 64)
        {
            var compoMask = 1ul << componentId;
            return ((_mask1 & compoMask) != 0);
        }
        else
        {
            var compoMask = 1ul << (componentId - 64);
            return ((_mask2 & compoMask) != 0);
        }
    }

    public static Archetype ForComponentId(in int componentId)
    {
        if (componentId < 64)
        {
            var mask1 = 1ul << componentId;
            return new Archetype(mask1, 0ul);
        }
        else
        {
            var mask2 = 1ul << (componentId - 64);
            return new Archetype(0ul, mask2);
        }
    }

    public static Archetype Combine(in Archetype a, in Archetype b)
    {
        return new Archetype(a._mask1 | b._mask1, a._mask2 | b._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1, a._mask2 | b._mask2 | c._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c, in Archetype d)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1 | d._mask1,
            a._mask2 | b._mask2 | c._mask2 | d._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c, in Archetype d, in Archetype e)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1 | d._mask1 | e._mask1,
            a._mask2 | b._mask2 | c._mask2 | d._mask2 | e._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c, in Archetype d, in Archetype e, in Archetype f)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1 | d._mask1 | e._mask1 | f._mask1,
            a._mask2 | b._mask2 | c._mask2 | d._mask2 | e._mask2 | f._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c, in Archetype d, in Archetype e, in Archetype f, in Archetype g)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1 | d._mask1 | e._mask1 | f._mask1 | g._mask1,
            a._mask2 | b._mask2 | c._mask2 | d._mask2 | e._mask2 | f._mask2 | g._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c, in Archetype d, in Archetype e, in Archetype f, in Archetype g, in Archetype h)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1 | d._mask1 | e._mask1 | f._mask1 | g._mask1 | h._mask1,
            a._mask2 | b._mask2 | c._mask2 | d._mask2 | e._mask2 | f._mask2 | g._mask2 | h._mask2);
    }

    public static Archetype Combine(in Archetype a, in Archetype b, in Archetype c, in Archetype d, in Archetype e, in Archetype f, in Archetype g, in Archetype h, in Archetype i)
    {
        return new Archetype(a._mask1 | b._mask1 | c._mask1 | d._mask1 | e._mask1 | f._mask1 | g._mask1 | h._mask1 | i._mask1,
            a._mask2 | b._mask2 | c._mask2 | d._mask2 | e._mask2 | f._mask2 | g._mask2 | h._mask2 | i._mask2);
    }
}
