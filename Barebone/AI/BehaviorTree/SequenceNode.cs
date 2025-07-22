namespace Barebone.AI.BehaviorTree
{
    internal class SequenceNode : IteratingNode
    {
        public SequenceNode(params BehaviorNode[] children) : base(NodeState.Failed, children)
        {
        }
    }
}
