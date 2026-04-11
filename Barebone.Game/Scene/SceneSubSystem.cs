using Barebone.Game.Physics;

namespace Barebone.Game.Scene
{
    internal enum SceneCommandType
    {
        AddActor,
        RemoveActor,
        Clear
    }

    internal record struct SceneCommand(SceneCommandType Type, Actor? Actor);

    internal class SceneSubSystem(PhysicsSubSystem physics) : IScene
    {
        private uint _nextActorId;

        private readonly List<Actor> _actors = new();
        private readonly Queue<SceneCommand> _commandQueue = new();

        public void Update(in IBBApi bb)
        {
            while (_commandQueue.Any())
            {
                {
                    var command = _commandQueue.Dequeue();
                    switch (command.Type)
                    {
                        case SceneCommandType.AddActor:
                            _actors.Add(command.Actor!);
                            SpawnInternal(bb, command);
                            break;
                        case SceneCommandType.RemoveActor:
                            DespawnInternal(bb, command.Actor!);
                            _actors.Remove(command.Actor!);
                            break;
                        case SceneCommandType.Clear:
                            foreach (var actor in _actors) DespawnInternal(bb, actor);
                            _actors.Clear();
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }

            foreach (var actor in _actors)
                actor.Update(bb);
        }

        public void Draw(BBApi bb)
        {
            foreach (var actor in _actors)
                actor.Draw(bb);
        }

        public void Add(Actor actor)
        {
            _commandQueue.Enqueue(new SceneCommand(SceneCommandType.AddActor, actor));
        }

        public void Remove(Actor actor)
        {
            _commandQueue.Enqueue(new SceneCommand(SceneCommandType.RemoveActor, actor));
        }

        public void Clear()
        {
            _commandQueue.Enqueue(new SceneCommand(SceneCommandType.Clear, null));
        }

        private void SpawnInternal(IBBApi bb, SceneCommand command)
        {
            command.Actor!.ActorId = new(++_nextActorId);
            command.Actor!.OnSpawn(bb);
        }

        private void DespawnInternal(IBBApi bb, Actor actor)
        {
            actor.OnDespawn(bb);
            physics.DestroyIfExists(actor.ActorId);
        }
    }
}
