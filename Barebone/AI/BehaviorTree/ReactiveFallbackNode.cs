namespace Barebone.AI.BehaviorTree
{
    internal class ReactiveFallbackNode : ReactiveIteratingNode
    {
        
        public ReactiveFallbackNode(params BehaviorNode[] children) : base(NodeState.Succeeded, children)
        {
        }
    }
}
