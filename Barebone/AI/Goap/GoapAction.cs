using Barebone.Pools;

namespace Barebone.AI.Goap
{
    public abstract class GoapAction : Poolable
    {
        protected internal override void Construct()
        {
            Name = null!;
            Preconditions = default;
            Effects = default;
            Cost = 0;
        }

        protected void Init(in string name, in int cost, in WorldState preconditions, in WorldState effects)
        {
            Name = name;
            Cost = cost;
            Preconditions = preconditions;
            Effects = effects;
        }

        public string Name { get; private set; } = null!;
        public WorldState Preconditions { get; private set; }
        public WorldState Effects { get; private set; }
        public int Cost { get; private set; }

        public abstract void Start();
        public abstract GoapActionResult Update();
    }
}
