using Barebone.Pools;

namespace Barebone.AI.Goap
{
    public abstract class GoapAction : Poolable
    {
        public abstract string Name { get; }
        public abstract void Start();

        public abstract GoapActionResult Update();
        public abstract WorldState Preconditions { get; }
        public abstract WorldState Effects { get; }
    }
}
