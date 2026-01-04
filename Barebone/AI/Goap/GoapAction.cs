namespace Barebone.AI.Goap
{
    public abstract class GoapAction(in string name, in int cost, in WorldState preconditions, in WorldState effects)
    {
        public string Name { get; private set; } = name;
        public WorldState Preconditions { get; private set; } = preconditions;
        public WorldState Effects { get; private set; } = effects;
        public int Cost { get; private set; } = cost;

        public abstract void Start();
        public abstract GoapActionResult Update();
    }
}
