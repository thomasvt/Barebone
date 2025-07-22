namespace Barebone.AI.BehaviorTree
{
    /// <summary>
    /// Any iterating node that steps over its children in sequence until one returns 'breakState', else it returns the state of the last child.
    /// If a child returns Running, it Returns running itself to continue where it left off at the next Tick().
    /// </summary>
    internal abstract class IteratingNode : CompositeNode
    {
        private readonly NodeState _breakState;
        private int? _previousRunningChildIdx;
        private readonly NodeState _invBreakState;

        protected IteratingNode(NodeState breakState, params BehaviorNode[] children) : base(children)
        {
            _breakState = breakState;
            _invBreakState = NotNode.Invert(breakState);
        }

        protected override NodeState Tick()
        {
            var idx = _previousRunningChildIdx ?? 0;

            while (idx < Children.Length)
            {
                var state = Children[idx].DoTick();
                _previousRunningChildIdx = idx;
                if (state is NodeState.Running or NodeState.RunningDontInterrupt)
                    return state;
                if (state == _breakState)
                {
                    _previousRunningChildIdx = null;
                    return state;
                }

                idx++;
            }

            return _invBreakState;
        }

        protected override void Reset()
        {
            base.Reset();
            _previousRunningChildIdx = null;
        }
    }
}
