namespace Barebone.AI.BehaviorTree
{
    /// <summary>
    /// Ticks all its children each tick, so they appear to run in parallel.
    /// Returns the leftmost state found among its children from this list: InProgressDontInterrupt, InProgress, Failed, Succeeded
    /// </summary>
    internal class ParallelNode : CompositeNode
    {
        public ParallelNode(BehaviorNode[] children) : base(children)
        {
        }

        protected override NodeState Tick()
        {
            var state = (int)NodeState.Running;
            foreach (var child in Children)
            {
                var childState = child.DoTick();
                state = Math.Max(state, (int)childState);
            }

            return (NodeState)state;
        }
    }
}
