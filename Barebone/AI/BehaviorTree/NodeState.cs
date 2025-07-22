namespace Barebone.AI.BehaviorTree
{
    public enum NodeState
    {
        Succeeded = 0,
        Failed = 1,
        Running = 2,
        /// <summary>
        /// Like Running, but disallows a Reactive parent to switch to another child until this one stops returning RunningDontInterrupt. Use this to ensure a task is completed before doing something else, like an animation.
        /// </summary>
        RunningDontInterrupt = 3,
    }
}
