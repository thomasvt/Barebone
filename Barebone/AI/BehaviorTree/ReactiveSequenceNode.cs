namespace Barebone.AI.BehaviorTree
{
    
    internal class ReactiveSequenceNode : ReactiveIteratingNode
    {
        public ReactiveSequenceNode(params BehaviorNode[] children) : base(NodeState.Failed, children)
        {
        }
    }
}
