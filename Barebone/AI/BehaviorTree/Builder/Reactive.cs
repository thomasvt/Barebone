namespace Barebone.AI.BehaviorTree.Builder
{
    public enum Reactive
    {
        No,
        /// <summary>
        /// Higher prio items or conditions are rechecked each Tick, so a Running child may be abandoned in which case it gets a Reset() call.
        /// </summary>
        Yes
    }
}
