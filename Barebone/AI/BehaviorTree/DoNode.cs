namespace Barebone.AI.BehaviorTree
{
    /// <summary>
    /// Invokes a custom Func.
    /// </summary>
    internal class DoNode : BehaviorNode
    {
        private readonly Func<NodeState> _action;
        private readonly Action? _enterAction;
        private readonly Action? _leaveAction;
        private bool _isFirst = true;

        public DoNode(Func<NodeState> action, Action? enterAction, Action? leaveAction) 
        {
            _action = action;
            _enterAction = enterAction;
            _leaveAction = leaveAction;
        }

        protected override NodeState Tick()
        {
            if (_isFirst)
            {
                _isFirst = false;
                _enterAction?.Invoke();
            }
            return _action();
        }

        protected override void Reset()
        {
            _leaveAction?.Invoke();
            _isFirst = true;
        }

        protected override IEnumerable<BehaviorNode> GetChildren()
        {
            yield break;
        }
    }
}
