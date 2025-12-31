using Barebone.Pools;

namespace Barebone.AI.Goap
{
    internal class GoapNode : Poolable
    {
        public GoapAction? Action { get; private set; }
        public WorldState State { get; private set; }
        public GoapNode? ParentNode { get; private set; }
        public int PlanLength { get; private set; }
        public int Score { get; private set; }

        protected internal override void Construct()
        {
            Action = null;
            State = default;
            ParentNode = null;
            PlanLength = 0;
            Score = 0;
        }

        protected internal override void Destruct()
        {
        }

        public void Init(in GoapAction? action, in WorldState state, GoapNode? parent, int planLength, int score)
        {
            Action = action;
            State = state;
            ParentNode = parent;
            PlanLength = planLength;
            Score = score;
        }
    }
}
