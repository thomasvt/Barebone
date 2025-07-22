namespace Barebone.AI.BehaviorTree
{
    internal class FallbackNode : IteratingNode
    {
        public FallbackNode(params BehaviorNode[] children) : base(NodeState.Succeeded, children)
        {
        }
    }
}
