namespace Barebone.AI.BehaviorTree
{
    public abstract class BehaviorNode
    {
        public NodeState DoTick()
        {
            if (LastState == null) // node has not yet run or was finished previous tick.
                OnEnter();

            var result = Tick();

            LastState = result;
            if (result != NodeState.Running && result != NodeState.RunningDontInterrupt)
                DoReset();

            return result;
        }

        public void DoReset()
        {
            if (!LastState.HasValue) return; // it's never run yet
            LastState = null;
            Reset();
        }

        /// <summary>
        /// Calles when a node is run for the first time since it was reset.
        /// </summary>
        protected virtual void OnEnter()
        {
        }

        protected abstract NodeState Tick();

        protected virtual void Reset() {}

        public NodeState? LastState { get; private set; }

        protected abstract IEnumerable<BehaviorNode> GetChildren();
    }
}
