namespace Barebone.Game.Scene
{
    internal class SceneSubSystem : IScene
    {
        private readonly List<IActor> _actors = new();
        private readonly List<IActor> _actorsToAdd = new();
        private readonly List<IActor> _actorsToRemove = new();

        public void Update(in IBBApi bb)
        {
            foreach (var actor in _actorsToRemove)
            {
                actor.OnDespawn(bb);
                _actors.Remove(actor);
            }
            _actorsToRemove.Clear();

            foreach (var actor in _actorsToAdd)
            {
                _actors.Add(actor);
                actor.OnSpawn(bb);
            }
            _actorsToAdd.Clear();

            foreach (var actor in _actors)
                actor.Update(bb);
        }

        public void Add(IActor actor)
        {
            _actorsToAdd.Add(actor);
        }

        public void Remove(IActor actor)
        {
            _actorsToRemove.Add(actor);
        }
    }
}
