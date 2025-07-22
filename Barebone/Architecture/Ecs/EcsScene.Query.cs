namespace Barebone.Architecture.Ecs;

public delegate void ComponentAction(in EntityId entityid);
public delegate void ComponentAction<TC1>(in EntityId entityid, ref TC1 c) where TC1 : struct;
public delegate void ComponentAction<TC1, TC2>(in EntityId entityId, ref TC1 c1, ref TC2 c2);
public delegate void ComponentAction<TC1, TC2, TC3>(in EntityId entityId, ref TC1 c1, ref TC2 c2, ref TC3 c3);
public delegate void ComponentAction<TC1, TC2, TC3, TC4>(in EntityId entityId, ref TC1 c1, ref TC2 c2, ref TC3 c3, ref TC4 c4);
public delegate void ComponentAction<TC1, TC2, TC3, TC4, TC5>(in EntityId entityId, ref TC1 c1, ref TC2 c2, ref TC3 c3, ref TC4 c4, ref TC5 c5);

public partial class EcsScene
{
    /// <summary>
    /// Executes the action on all entities that match the component filter.
    /// </summary>
    internal int ForEachMatch(ComponentAction action, Archetype exclude, Archetype alsoInclude)
    {
        var filter = alsoInclude;

        var count = 0;

        // invoke 'action' on all entities of all EntitySets that match the component filter.
        foreach (var entitySet in _entitySets)
        {
            if (!entitySet.Archetype.IncludesAll(filter) || entitySet.Archetype.IncludesAny(exclude)) continue;

            var idSpan = entitySet.GetEntityIdSpan();

            for (var i = 0; i < idSpan.Length; i++)
            {
                action(idSpan[i]);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Executes the action on all entities that match the component filter.
    /// </summary>
    internal int ForEachMatch<T1>(ComponentAction<T1> action, Archetype exclude, Archetype alsoInclude) where T1 : struct
    {
        var filter = ArchetypeRegistry.ComposeArchetype<T1>().Union(alsoInclude);
        var t1Def = ArchetypeRegistry.GetComponentDef<T1>();

        var count = 0;

        // invoke 'action' on all entities of all EntitySets that match the component filter
        foreach (var entitySet in _entitySets)
        {
            if (!entitySet.Archetype.IncludesAll(filter) || entitySet.Archetype.IncludesAny(exclude)) continue;
            
            var idSpan = entitySet.GetEntityIdSpan();
            var t1Span = entitySet.GetComponentSpan<T1>(t1Def);

            for (var i = 0; i < idSpan.Length; i++)
            {
                action(idSpan[i], ref t1Span[i]);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Executes the action on all entities that match the component filter
    /// </summary>
    internal int ForEachMatch<T1, T2>(ComponentAction<T1, T2> action, Archetype exclude, Archetype alsoInclude) where T1 : struct where T2 : struct
    {
        var filter = ArchetypeRegistry.ComposeArchetype<T1, T2>().Union(alsoInclude);
        var t1Def = ArchetypeRegistry.GetComponentDef<T1>();
        var t2Def = ArchetypeRegistry.GetComponentDef<T2>();

        var count = 0;

        foreach (var entitySet in _entitySets)
        {
            if (!entitySet.Archetype.IncludesAll(filter) || entitySet.Archetype.IncludesAny(exclude)) continue;

            var idSpan = entitySet.GetEntityIdSpan();
            var t1Span = entitySet.GetComponentSpan<T1>(t1Def);
            var t2Span = entitySet.GetComponentSpan<T2>(t2Def);

            for (var i = 0; i < idSpan.Length; i++)
            {
                action(idSpan[i], ref t1Span[i], ref t2Span[i]);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Executes the action on all entities that match the component filter
    /// </summary>
    internal int ForEachMatch<T1, T2, T3>(ComponentAction<T1, T2, T3> action, Archetype exclude, Archetype alsoInclude) where T1 : struct where T2 : struct where T3 : struct
    {
        var filter = ArchetypeRegistry.ComposeArchetype<T1, T2, T3>().Union(alsoInclude);
        var t1Def = ArchetypeRegistry.GetComponentDef<T1>();
        var t2Def = ArchetypeRegistry.GetComponentDef<T2>();
        var t3Def = ArchetypeRegistry.GetComponentDef<T3>();

        var count = 0;

        foreach (var entitySet in _entitySets)
        {
            if (!entitySet.Archetype.IncludesAll(filter) || entitySet.Archetype.IncludesAny(exclude)) continue;

            var idSpan = entitySet.GetEntityIdSpan();
            var t1Span = entitySet.GetComponentSpan<T1>(t1Def);
            var t2Span = entitySet.GetComponentSpan<T2>(t2Def);
            var t3Span = entitySet.GetComponentSpan<T3>(t3Def);

            for (var i = 0; i < idSpan.Length; i++)
            {
                action(idSpan[i], ref t1Span[i], ref t2Span[i], ref t3Span[i]);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Executes the action on all entities that match the component filter
    /// </summary>
    internal int ForEachMatch<T1, T2, T3, T4>(ComponentAction<T1, T2, T3, T4> action, Archetype exclude, Archetype alsoInclude) where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        var filter = ArchetypeRegistry.ComposeArchetype<T1, T2, T3, T4>().Union(alsoInclude);
        var t1Def = ArchetypeRegistry.GetComponentDef<T1>();
        var t2Def = ArchetypeRegistry.GetComponentDef<T2>();
        var t3Def = ArchetypeRegistry.GetComponentDef<T3>();
        var t4Def = ArchetypeRegistry.GetComponentDef<T4>();

        var count = 0;
        
        foreach (var entitySet in _entitySets)
        {
            if (!entitySet.Archetype.IncludesAll(filter) || entitySet.Archetype.IncludesAny(exclude)) continue;

            var idSpan = entitySet.GetEntityIdSpan();
            var t1Span = entitySet.GetComponentSpan<T1>(t1Def);
            var t2Span = entitySet.GetComponentSpan<T2>(t2Def);
            var t3Span = entitySet.GetComponentSpan<T3>(t3Def);
            var t4Span = entitySet.GetComponentSpan<T4>(t4Def);

            for (var i = 0; i < idSpan.Length; i++)
            {
                action(idSpan[i], ref t1Span[i], ref t2Span[i], ref t3Span[i], ref t4Span[i]);
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Executes the action on all entities that match the component filter
    /// </summary>
    internal int ForEachMatch<T1, T2, T3, T4, T5>(ComponentAction<T1, T2, T3, T4, T5> action, Archetype exclude, Archetype alsoInclude) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        var filter = ArchetypeRegistry.ComposeArchetype<T1, T2, T3, T4, T5>().Union(alsoInclude);
        var t1Def = ArchetypeRegistry.GetComponentDef<T1>();
        var t2Def = ArchetypeRegistry.GetComponentDef<T2>();
        var t3Def = ArchetypeRegistry.GetComponentDef<T3>();
        var t4Def = ArchetypeRegistry.GetComponentDef<T4>();
        var t5Def = ArchetypeRegistry.GetComponentDef<T5>();

        var count = 0;

        foreach (var entitySet in _entitySets)
        {
            if (!entitySet.Archetype.IncludesAll(filter) || entitySet.Archetype.IncludesAny(exclude)) continue;

            var idSpan = entitySet.GetEntityIdSpan();
            var t1Span = entitySet.GetComponentSpan<T1>(t1Def);
            var t2Span = entitySet.GetComponentSpan<T2>(t2Def);
            var t3Span = entitySet.GetComponentSpan<T3>(t3Def);
            var t4Span = entitySet.GetComponentSpan<T4>(t4Def);
            var t5Span = entitySet.GetComponentSpan<T5>(t5Def);

            for (var i = 0; i < idSpan.Length; i++)
            {
                action(idSpan[i], ref t1Span[i], ref t2Span[i], ref t3Span[i], ref t4Span[i], ref t5Span[i]);
                count++;
            }
        }
        return count;
    }
}

