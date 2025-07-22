using Barebone.AI.BehaviorTree.Builder;

namespace Barebone.AI.BehaviorTree
{
    public class BehaviorTree
    {
        public readonly BehaviorNode RootNode;
        

        internal BehaviorTree(BehaviorNode rootNode)
        {
            RootNode = rootNode;
        }

        public void Tick()
        {
            RootNode.DoTick();
        }
        

        private static Func<float>? _getGameTimeFunc;

        public static float GetGameTime()
        {
            if (_getGameTimeFunc == null)
                throw new Exception("Cannot use time in behavior trees unless you register a time service by calling the static BehaviorTree.SetTimeService() ");

            return _getGameTimeFunc();
        }

        /// <summary>
        /// Injects a game-time service for all behaviortrees to use. This enables eg. Delay.
        /// </summary>
        public static void SetTimeService(Func<float> getGameTime)
        {
            _getGameTimeFunc = getGameTime;
        }

        public static BehaviorTreeBuilder Create()
        {
            return new BehaviorTreeBuilder();
        }
    }
}
