using Barebone.AI.BehaviorTree;

namespace Barebone.AI.BehaviorTree.Builder
{
    /// <summary>
    /// Builds a behavior tree with fluent API. Start from BehaviorTree.Create(). If you add multiple nodes on this branch they will be wrapped in a non reactive sequence node.
    /// </summary>
    public class BehaviorBranchBuilder
    {
        protected readonly List<BehaviorNode> Nodes = new();

        /// <summary>
        /// Creates a Reactive Fallback. Ticks all children in sequence until one returns Succeeded. Meant to be used as a chain-of-responsibility where children return Failed if they don't apply and just one returns Succeeded to restart checking from the top. A child that returns Running will be remembered and is the first to be ticked the next Tick.
        /// </summary>
        public BehaviorBranchBuilder Fallback(Action<BehaviorBranchBuilder> cases)
        {
            return Fallback(Reactive.Yes, cases);
        }

        /// <summary>
        /// Executes all children in sequence until one returns Succeeded. A child that returns Running will continue to be ticked until a child earlier in the list stops returning Failed.
        /// </summary>
        public BehaviorBranchBuilder Fallback(Reactive reactive, Action<BehaviorBranchBuilder> cases)
        {
            var builder = new BehaviorBranchBuilder();
            cases.Invoke(builder);
            Nodes.Add(reactive == Reactive.Yes 
                ? new ReactiveFallbackNode(builder.GetNodes())
                : new FallbackNode(builder.GetNodes())
                );
            return this;
        }

        /// <summary>
        /// Execute a custom Action without return value. This always returns Succeeded to the parent behavior node.
        /// </summary>
        public BehaviorBranchBuilder Do(Action action, Action? enterAction = null, Action? leaveAction = null)
        {
            return Do(() => { action(); return NodeState.Succeeded; }, enterAction, leaveAction);
        }

        /// <summary>
        /// Execute a custom boolean test that succeeds or fails based on the returned boolean.
        /// </summary>
        public BehaviorBranchBuilder Do(Func<bool> condition)
        {
            return Do(() => condition() ? NodeState.Succeeded : NodeState.Failed);
        }

        /// <summary>
        /// Execute a custom Func that must return the state of the node.
        /// </summary>
        public BehaviorBranchBuilder Do(Func<NodeState> action, Action? enterAction = null, Action? leaveAction = null)
        {
            Nodes.Add(new DoNode(action, enterAction, leaveAction));
            return this;
        }


        /// <summary>
        /// Inverts the state of the body: Succeeded to Failed and visa versa. Running stays Running.
        /// </summary>
        public BehaviorBranchBuilder Not(Action<BehaviorBranchBuilder> body)
        {
            var builder = new BehaviorBranchBuilder();
            body.Invoke(builder);
            Nodes.Add(new NotNode(new SequenceNode(builder.GetNodes())));
            return this;
        }

        /// <summary>
        /// Ticks all its children each tick. Returns InProgress if any of its children are, Succee
        /// </summary>
        public BehaviorBranchBuilder Parallel(Action<BehaviorBranchBuilder> build)
        {
            var builder = new BehaviorBranchBuilder();
            build.Invoke(builder);

            Nodes.Add(new ParallelNode(builder.GetNodes()));

            return this;
        }

        public BehaviorBranchBuilder Sequence(Reactive reactive, Action<BehaviorBranchBuilder> sequence)
        {
            var builder = new BehaviorBranchBuilder();
            sequence.Invoke(builder);

            Nodes.Add(reactive == Reactive.Yes
            ? new ReactiveSequenceNode(builder.GetNodes())
            : new SequenceNode(builder.GetNodes()));

            return this;
        }

        /// <summary>
        /// Delays for a set duration in seconds.
        /// </summary>
        public BehaviorBranchBuilder Delay(float duration)
        {
            Nodes.Add(new DelayNode(duration));
            return this;
        }

        /// <summary>
        /// Creates a Sequence that only gets executed if the condition returns true. The sequence is interrupted when the condition becomes false, even if it is Running (unless its RunningDontInterrupt)
        /// </summary>
        public BehaviorBranchBuilder If(Func<bool> condition, Action<BehaviorBranchBuilder> sequence)
        {
            return If(Reactive.Yes, condition, sequence);
        }

        /// <summary>
        /// Creates a sequence that only gets executed if the condition returns true.
        /// </summary>
        public BehaviorBranchBuilder If(Reactive reactive, Func<bool> condition, Action<BehaviorBranchBuilder> sequence)
        {
            return Sequence(reactive, b =>
            {
                b.Do(condition);
                sequence.Invoke(b);
            });
        }

        protected BehaviorNode[] GetNodes()
        {
            if (Nodes.Count == 0) throw new Exception("Cannot build a behavior branch from zero nodes.");
            return Nodes.ToArray();
        }

        public virtual BehaviorTree Build()
        {
            throw new Exception("Don't call Build on a sub branch, only on the root branch.");
        }
    }
}
