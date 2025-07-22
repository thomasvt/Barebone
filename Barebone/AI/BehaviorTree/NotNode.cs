namespace Barebone.AI.BehaviorTree
{
    internal class NotNode : BehaviorNode
    {
        private readonly BehaviorNode _body;

        public NotNode(BehaviorNode body)
        {
            _body = body;
        }

        protected override NodeState Tick()
        {
            var result = _body.DoTick();
            return Invert(result);
        }

        public static NodeState Invert(NodeState s)
        {
            if (s == NodeState.Failed) return NodeState.Succeeded;
            if (s == NodeState.Succeeded) return NodeState.Failed;
            return s;
        }

        protected override IEnumerable<BehaviorNode> GetChildren()
        {
            yield return _body;
        }
    }
}
