namespace Barebone.Architecture.Ecs
{
    public partial class EcsScene
    {
        private ISystem[] _systems = [];

        /// <summary>
        /// Sets all ECS systems in the order they should be executed each frame.
        /// </summary>
        public void SetSystems(params ISystem[] systems)
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
