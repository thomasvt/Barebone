namespace Barebone.Architecture.Ecs
{
    public partial class EcsScene
    {
        private IEcsSystem[] _systems = [];

        /// <summary>
        /// Sets all ECS systems in the order they should be executed each frame.
        /// </summary>
        public void SetSystems(params IEcsSystem[] systems)
        {
            _systems = systems;
        }

        private void UpdateSystems()
        {
            foreach (var system in _systems)
            {
                system.Execute();
            }
        }
    }
}
