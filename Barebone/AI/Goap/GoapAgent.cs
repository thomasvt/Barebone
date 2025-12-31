using Barebone.Pools;

namespace Barebone.AI.Goap
{
    public class GoapAgent : Poolable
    {
        private readonly List<GoapNode> _openList = new();
        private readonly List<GoapNode> _nodeGarbage = new();
        private BBList<GoapAction> _planBuffer = null!;
        private BBList<GoapAction> _actions = null!;

        private int _planStepIdx;
        private GoapActionResult _planStepResult;
        private GoapAction _idleAction = null!;
        private GoapAction? _previousAction;

        protected internal override void Construct()
        {
            _planStepIdx = 0;
            _planStepResult = GoapActionResult.Success;
            _planBuffer = Pool.Rent<BBList<GoapAction>>();
            _idleAction = null!;
            _previousAction = null;
            _actions = Pool.Rent<BBList<GoapAction>>();
        }

        protected internal override void Destruct()
        {
            _actions.Return();
            _planBuffer.Return();
        }

        public void Update(in IGoapPawn pawn)
        {
            var currentState = pawn.GetWorldState();

            if (_planStepIdx >= _planBuffer.Count)
            {
                _planStepIdx = 0;
                _previousAction = null;
                Plan(pawn, 3);
            }

            if (_planStepIdx < _planBuffer.Count && currentState.Satisfies(_planBuffer.InternalArray[_planStepIdx].Preconditions))
            {
                var currentAction = _planBuffer.InternalArray[_planStepIdx];
                if (currentAction != _previousAction)
                {
                    Console.WriteLine("Start: " + currentAction.Name);
                    currentAction.Start();
                }
                _planStepResult = currentAction.Update();
                _previousAction = currentAction;

                switch (_planStepResult)
                {
                    case GoapActionResult.Success: 
                        _planStepIdx++; 
                        _previousAction = null; // a plan with same action multiple times should trigger it's Start() each time.
                        break;
                    case GoapActionResult.Failure:
                        StartIdlePlan();
                        break;
                }

            }
            else
            {
                StartIdlePlan();
            }
        }

        /// <summary>
        /// Executes a plan with a single occurrence of the IdleAction. After that completes successfully, a replan will happen.
        /// </summary>
        private void StartIdlePlan()
        {
            _planBuffer.Clear();
            _planBuffer.Add(_idleAction);
            _planStepResult = GoapActionResult.InProgress;
            _planStepIdx = 0;
            _previousAction = null;
        }

        /// <summary>
        /// Fills _planBuffer with the best plan found for the given pawn, up to maxPlanLength actions.
        /// </summary>
        private void Plan(in IGoapPawn pawn, in int maxPlanLength)
        {
            
            _nodeGarbage.Clear();
            _openList.Clear();
            _planBuffer.Clear();

            var actions = _actions.AsReadOnlySpan();
            var worldState = pawn.GetWorldState();
            var goal = pawn.GetGoal();

            if (goal.FlagCount > 0 && actions.Length > 0)
            {
                GoapNode? winner = null;

                {
                    // initialize open list with current world state
                    var newNode = Pool.Rent<GoapNode>();
                    _nodeGarbage.Add(newNode);
                    newNode.Init(null, worldState, null, 0, worldState.GetScore(goal));
                    _openList.Add(newNode);
                }

                while (_openList.Count > 0)
                {
                    // take last one (highest score)
                    var currentNode = _openList[^1];
                    _openList.RemoveAt(_openList.Count - 1);

                    if (currentNode.State.Satisfies(goal))
                    {
                        // todo we may want to collect all optimal plans here instead of just the first one found.
                        // and then choose a random one among them.
                        winner = currentNode;
                        break;
                    }

                    // add all possible actions from here:
                    if (currentNode.PlanLength < maxPlanLength)
                    {
                        foreach (var action in actions)
                        {
                            if (!action.Preconditions.Satisfies(currentNode.State)) continue;
                            var newState = currentNode.State;
                            newState.ApplyEffects(action.Effects);

                            var newNode = Pool.Rent<GoapNode>();
                            _nodeGarbage.Add(newNode);
                            newNode.Init(action, newState, currentNode, currentNode.PlanLength + 1,
                                newState.GetScore(goal));
                            _openList.Add(newNode);
                        }

                        _openList.Sort(CompareScoreAscending);
                    }
                }

                winner ??= _openList.LastOrDefault(); // winner or best-score
                if (winner != null)
                {
                    // assemble winning plan: all actions in reverse order.
                    var planLength = winner.PlanLength;
                    _planBuffer.SetFixedCount(planLength, false);
                    var node = winner;
                    var i = planLength - 1;
                    while (node.ParentNode != null)
                    {
                        _planBuffer.InternalArray[i] = node.Action!;
                        node = node.ParentNode;
                        i--;
                    }
                } // else: no winner -> planBuffer stays empty
            }

            // cleanup:
            foreach (var node in _nodeGarbage)
                node.Return();
            _nodeGarbage.Clear();
            _openList.Clear();

            Console.WriteLine($"New plan: [{string.Join(", ", _planBuffer.AsArraySegment().Select(a => a.Name))}]");
        }

        private static int CompareScoreAscending(GoapNode a, GoapNode b)
        {
            return a.Score.CompareTo(b.Score);
        }

        public void RegisterIdleAction(GoapAction action)
        {
            _idleAction = action;
        }

        public void RegisterAction(GoapAction action)
        {
            _actions.Add(action);
        }
    }
}