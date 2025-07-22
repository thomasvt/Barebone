namespace Barebone.AI.BehaviorTree
{
    /// <summary>
    /// Similar to <see cref="IteratingNode"/> but does not remember Running child across Tick()s: all children are always retried each Tick so that they become a chain-of-responsibility that changes Child as soon as conditions change.
    /// A Running child that is interrupted because a higher one takes over, or the item itself decides to fail, is Reset().
    /// </summary>
    internal abstract class ReactiveIteratingNode : CompositeNode
    {
        private readonly NodeState _breakState;
        private int? _previousRunningChildIdx;
        private readonly NodeState _invBreakState;

        protected ReactiveIteratingNode(NodeState breakState, params BehaviorNode[] children) : base(children)
        {
            _breakState = breakState;
            _invBreakState = NotNode.Invert(breakState);
        }

        protected override NodeState Tick()
        {
            var idx = 0;

            var previousRunningChild = _previousRunningChildIdx.HasValue
                ? Children[_previousRunningChildIdx.Value]
                : null;

            // must start loop from previous running child that doesn't want to be interrupted?
            if (previousRunningChild is { LastState: NodeState.RunningDontInterrupt })
                idx = _previousRunningChildIdx!.Value;

            while (idx < Children.Length)
            {
                var state = Children[idx].DoTick();
                if (state == NodeState.Running || state == NodeState.RunningDontInterrupt || state == _breakState)
                {
                    if (_previousRunningChildIdx != idx)
                    {
                        // we interrupted the previously running child -> reset that one.
                        previousRunningChild?.DoReset();
                    }

                    _previousRunningChildIdx = state is NodeState.Running or NodeState.RunningDontInterrupt
                        ? idx
                        : null;

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
