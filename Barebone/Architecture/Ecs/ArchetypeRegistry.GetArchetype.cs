namespace Barebone.Architecture.Ecs;

public partial class ArchetypeRegistry
{
    /// <summary>
    /// Composes an Archetype with the given component type.
    /// </summary>
    public Archetype ComposeArchetype<TCompo>() where TCompo : struct
    {
        return GetComponentDef<TCompo>().Archetype;
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2>() where TCompo1 : struct where TCompo2 : struct
    {
        return Archetype.Combine(GetComponentDef<TCompo1>().Archetype, GetComponentDef<TCompo2>().Archetype);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3>() where TCompo1 : struct where TCompo2 : struct where TCompo3 : struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3, TCompo4>() where TCompo1 : struct where TCompo2 : struct where TCompo3 : struct where TCompo4 : struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        var compoTypeMask4 = GetComponentDef<TCompo4>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3, compoTypeMask4);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3, TCompo4, TCompo5>() 
        where TCompo1 : struct 
        where TCompo2 : struct
        where TCompo3 : struct
        where TCompo4 : struct
        where TCompo5 : struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        var compoTypeMask4 = GetComponentDef<TCompo4>().Archetype;
        var compoTypeMask5 = GetComponentDef<TCompo5>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3, compoTypeMask4, compoTypeMask5);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3, TCompo4, TCompo5, TCompo6>()
        where TCompo1 : struct
        where TCompo2 : struct
        where TCompo3 : struct
        where TCompo4 : struct
        where TCompo5 : struct
        where TCompo6 : struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        var compoTypeMask4 = GetComponentDef<TCompo4>().Archetype;
        var compoTypeMask5 = GetComponentDef<TCompo5>().Archetype;
        var compoTypeMask6 = GetComponentDef<TCompo6>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3, compoTypeMask4, compoTypeMask5, compoTypeMask6);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3, TCompo4, TCompo5, TCompo6, TCompo7>()
        where TCompo1 : struct
        where TCompo2 : struct
        where TCompo3 : struct
        where TCompo4 : struct
        where TCompo5 : struct
        where TCompo6 : struct
        where TCompo7 : struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        var compoTypeMask4 = GetComponentDef<TCompo4>().Archetype;
        var compoTypeMask5 = GetComponentDef<TCompo5>().Archetype;
        var compoTypeMask6 = GetComponentDef<TCompo6>().Archetype;
        var compoTypeMask7 = GetComponentDef<TCompo7>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3, compoTypeMask4, compoTypeMask5, compoTypeMask6, compoTypeMask7);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3, TCompo4, TCompo5, TCompo6, TCompo7, TCompo8>()
        where TCompo1 : struct
        where TCompo2 : struct
        where TCompo3 : struct
        where TCompo4 : struct
        where TCompo5 : struct
        where TCompo6 : struct
        where TCompo7 : struct
        where TCompo8 : struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        var compoTypeMask4 = GetComponentDef<TCompo4>().Archetype;
        var compoTypeMask5 = GetComponentDef<TCompo5>().Archetype;
        var compoTypeMask6 = GetComponentDef<TCompo6>().Archetype;
        var compoTypeMask7 = GetComponentDef<TCompo7>().Archetype;
        var compoTypeMask8 = GetComponentDef<TCompo8>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3 , compoTypeMask4, compoTypeMask5, compoTypeMask6, compoTypeMask7, compoTypeMask8);
    }

    /// <summary>
    /// Composes an Archetype with the given component types.
    /// </summary>
    public Archetype ComposeArchetype<TCompo1, TCompo2, TCompo3, TCompo4, TCompo5, TCompo6, TCompo7, TCompo8, TCompo9>()
        where TCompo1 : struct
        where TCompo2 : struct
        where TCompo3 : struct
        where TCompo4 : struct
        where TCompo5 : struct
        where TCompo6 : struct
        where TCompo7 : struct
        where TCompo8 : struct
        where TCompo9: struct
    {
        var compoTypeMask1 = GetComponentDef<TCompo1>().Archetype;
        var compoTypeMask2 = GetComponentDef<TCompo2>().Archetype;
        var compoTypeMask3 = GetComponentDef<TCompo3>().Archetype;
        var compoTypeMask4 = GetComponentDef<TCompo4>().Archetype;
        var compoTypeMask5 = GetComponentDef<TCompo5>().Archetype;
        var compoTypeMask6 = GetComponentDef<TCompo6>().Archetype;
        var compoTypeMask7 = GetComponentDef<TCompo7>().Archetype;
        var compoTypeMask8 = GetComponentDef<TCompo8>().Archetype;
        var compoTypeMask9 = GetComponentDef<TCompo9>().Archetype;
        return Archetype.Combine(compoTypeMask1, compoTypeMask2, compoTypeMask3, compoTypeMask4, compoTypeMask5, compoTypeMask6, compoTypeMask7, compoTypeMask8, compoTypeMask9);
    }
}