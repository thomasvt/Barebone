namespace Barebone.Architecture.Ecs
{
    public partial class EcsScene
    {
        /// <summary>
        /// Creates a <see cref="IQuerySystem"/> with an action to perform on one or more components of each entity that matches the query filter.
        /// </summary>
        public IQuerySystem CreateQuerySystem(ComponentAction action, bool excludeDisabled = true)
        {
            return new QuerySystem(this, action, excludeDisabled);
        }

        /// <summary>
        /// Creates a <see cref="IQuerySystem"/> with an action to perform on one or more components of each entity that matches the query filter.
        /// </summary>
        public IQuerySystem CreateQuerySystem<TC1>(ComponentAction<TC1> action, bool excludeDisabled = true)
            where TC1 : struct
        {
            return new QuerySystem<TC1>(this, action, excludeDisabled);
        }

        /// <summary>
        /// Creates a <see cref="IQuerySystem"/> with an action to perform on one or more components of each entity that matches the query filter.
        /// </summary>
        public IQuerySystem CreateQuerySystem<TC1, TC2>(ComponentAction<TC1, TC2> action, bool excludeDisabled = true)
            where TC1 : struct
            where TC2 : struct
        {
            return new QuerySystem<TC1, TC2>(this, action, excludeDisabled);
        }

        /// <summary>
        /// Creates a <see cref="IQuerySystem"/> with an action to perform on one or more components of each entity that matches the query filter.
        /// </summary>
        public IQuerySystem CreateQuerySystem<TC1, TC2, TC3>(ComponentAction<TC1, TC2, TC3> action, bool excludeDisabled = true)
            where TC1 : struct
            where TC2 : struct
            where TC3 : struct
        {
            return new QuerySystem<TC1, TC2, TC3>(this, action, excludeDisabled);
        }

        /// <summary>
        /// Creates a <see cref="IQuerySystem"/> with an action to perform on one or more components of each entity that matches the query filter.
        /// </summary>
        public IQuerySystem CreateQuerySystem<TC1, TC2, TC3, TC4>(ComponentAction<TC1, TC2, TC3, TC4> action, bool excludeDisabled = true)
            where TC1 : struct
            where TC2 : struct
            where TC3 : struct
            where TC4 : struct
        {
            return new QuerySystem<TC1, TC2, TC3, TC4>(this, action, excludeDisabled);
        }

        /// <summary>
        /// Creates a <see cref="IQuerySystem"/> with an action to perform on one or more components of each entity that matches the query filter.
        /// </summary>
        public IQuerySystem CreateQuerySystem<TC1, TC2, TC3, TC4, TC5>(ComponentAction<TC1, TC2, TC3, TC4, TC5> action, bool excludeDisabled = true)
            where TC1 : struct
            where TC2 : struct
            where TC3 : struct
            where TC4 : struct
            where TC5 : struct
        {
            return new QuerySystem<TC1, TC2, TC3, TC4, TC5>(this, action, excludeDisabled);
        }
    }
}
