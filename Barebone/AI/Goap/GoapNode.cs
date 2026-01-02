using Barebone.Pools;

namespace Barebone.AI.Goap
{
    internal class GoapNode : Poolable
    {
        public GoapAction? Action { get; private set; }
        public WorldState State { get; private set; }
        public GoapNode? ParentNode { get; private set; }
        /// <summary>
        /// Count of nodes this plan touches on the graph.
        /// </summary>
        public int PlanDepth { get; private set; }

        /// <summary>
        /// Sum of costs of all Actions along the path that arrives at this node.
        /// </summary>
        public int PlanCost { get; private set; }

        /// <summary>
        /// Goodness of this node while finding the best path. Lower is better.
        /// </summary>
        public int Heuristic { get; private set; }

        public int GoalCostEstimate { get; set; }

        protected internal override void Construct()
        {
            Action = null;
            State = default;
            ParentNode = null;
            PlanDepth = 0;
            PlanCost = 0;
            Heuristic = 0;
            GoalCostEstimate = 0;
        }

        protected internal override void Destruct()
        {
        }

        public void Init(in GoapAction? action, in WorldState state, in GoapNode? parent, in int planDepth, in int planCost, in int goalCostEstimate)
        {
            Action = action;
            State = state;
            ParentNode = parent;
            PlanDepth = planDepth;
            PlanCost = planCost;
            GoalCostEstimate = goalCostEstimate;
            Heuristic = PlanCost + goalCostEstimate;
        }

        public void ChangeParent(in GoapNode node, in int planDepth, in int planCost)
        {
            ParentNode = node;
            PlanDepth = planDepth;
            PlanCost = planCost;
            Heuristic = planCost + GoalCostEstimate;
        }
    }
}
