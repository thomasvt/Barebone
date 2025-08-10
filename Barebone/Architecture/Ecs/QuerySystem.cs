using Barebone.Architecture.Ecs.Components;

namespace Barebone.Architecture.Ecs
{
    internal abstract class QuerySystemBase : IQuerySystem
    {
        protected readonly EcsScene Scene;
        protected Archetype AlsoInclude;
        protected Archetype Exclude;

        protected QuerySystemBase(EcsScene scene, bool excludeDisabled = true)
        {
            Scene = scene;
            if (excludeDisabled)
                MustExclude<DisabledCompo>();
        }

        public abstract void Execute();

        /// <summary>
        /// Adds a component type to the list of components that the entities must have to match with this query. If the component was put in the Exclude list earlier, that is undone.
        /// </summary>
        public IQuerySystem MustInclude<T>() where T : struct
        {
            var componentDef = Scene.ArchetypeRegistry.GetComponentDef<T>();
            Exclude.Remove(componentDef);
            AlsoInclude = AlsoInclude.Add(componentDef);
            return this;
        }

        /// <summary>
        /// Adds a component type to the list of components that the entities must NOT have to match with this query. Note that excluding a component that is in the generic type listing will break this QuerySystem.
        /// If the component was put in the Include list earlier, that is undone.
        /// </summary>
        public IQuerySystem MustExclude<T>() where T : struct
        {
            var componentDef = Scene.ArchetypeRegistry.GetComponentDef<T>();
            AlsoInclude.Remove(componentDef);
            Exclude = Exclude.Add(componentDef);
            return this;
        }
    }

    internal class QuerySystem(EcsScene scene, ComponentAction action, bool excludeDisabled = true) : QuerySystemBase(scene, excludeDisabled)
    {
        public override void Execute()
        {
            Scene.ForEachMatch(action, Exclude, AlsoInclude);
        }
    }

    internal class QuerySystem<TC1>(EcsScene scene, ComponentAction<TC1> action, bool excludeDisabled = true) : QuerySystemBase(scene, excludeDisabled) where TC1 : struct
    {
        public override void Execute()
        {
            Scene.ForEachMatch(action, Exclude, AlsoInclude);
        }
    }

    internal class QuerySystem<TC1, TC2>(EcsScene scene, ComponentAction<TC1, TC2> action, bool excludeDisabled = true) : QuerySystemBase(scene, excludeDisabled) where TC1 : struct
        where TC2 : struct
    {
        public override void Execute()
        {
            Scene.ForEachMatch(action, Exclude, AlsoInclude);
        }
    }

    internal class QuerySystem<TC1, TC2, TC3>(EcsScene scene, ComponentAction<TC1, TC2, TC3> action, bool excludeDisabled = true) : QuerySystemBase(scene, excludeDisabled) where TC1 : struct
        where TC2 : struct
        where TC3 : struct

    {
        public override void Execute()
        {
            Scene.ForEachMatch(action, Exclude, AlsoInclude);
        }
    }

    internal class QuerySystem<TC1, TC2, TC3, TC4>(EcsScene scene, ComponentAction<TC1, TC2, TC3, TC4> action, bool excludeDisabled = true) : QuerySystemBase(scene, excludeDisabled) where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
    {
        public override void Execute()
        {
            Scene.ForEachMatch(action, Exclude, AlsoInclude);
        }
    }

    internal class QuerySystem<TC1, TC2, TC3, TC4, TC5>(EcsScene scene, ComponentAction<TC1, TC2, TC3, TC4, TC5> action, bool excludeDisabled = true) : QuerySystemBase(scene, excludeDisabled) where TC1 : struct
        where TC2 : struct
        where TC3 : struct
        where TC4 : struct
        where TC5 : struct
    {
        public override void Execute()
        {
            Scene.ForEachMatch(action, Exclude, AlsoInclude);
        }
    }
}
