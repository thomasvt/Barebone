namespace Barebone.Architecture.Ecs
{
    public interface IQuerySystem : IEcsSystem
    {
        /// <summary>
        /// Adds a component type to the list of components that the entities must have to match the filter of this QuerySystem.
        /// </summary>
        IQuerySystem MustHave<T>() where T : struct;

        /// <summary>
        /// Adds a component type to the list of components that the entities must NOT have to match the filter of this QuerySystem.
        /// </summary>
        IQuerySystem MustNotHave<T>() where T : struct;
    }
}
