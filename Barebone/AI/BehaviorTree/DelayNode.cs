namespace Barebone.AI.BehaviorTree
{
    public class DelayNode : BehaviorNode
    {
        private readonly float _duration;
        private float? _startTime;

        public DelayNode(float duration)
        {
            _duration = duration;
        }

        protected override NodeState Tick()
        {
            if (!_startTime.HasValue)
            {
                _startTime = BehaviorTree.GetGameTime();
            }

            if (_startTime + _duration <= BehaviorTree.GetGameTime())
            {
                return NodeState.Succeeded;
            }

            return NodeState.Running;
        }

        protected override void Reset()
        {
            _startTime = null;
        }

        protected override IEnumerable<BehaviorNode> GetChildren()
        {
            yield break;
        }
    }
}
