using System;
using Barebone.Pools;

namespace Barebone.AI.Goap
{
    public class GoapAgent : Poolable
    {
        private readonly List<GoapNode> _openList = new();
        private readonly Dictionary<ulong, GoapNode> _existingNodes = new();
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
            // We use AStar algorithm to find the cheapest path from the pawns current WorldState to the goal's WorldState.
            // So each node on the graph is uniquely identified by the WorldState's Flags combination.
            // The possible connections from one node to other nodes are the pawn's Actions for which the Preconditions are satisfies on that given node(=WorldState).
            // The Score of each node is how many flags of the WorldState have the same value as the goal's Worldstate's flags.
            // This score is therefore also used as heuristic to guide the path finding.
            // If no perfect goal match is found, the highest Scoring one is chosen.
            
            _openList.Clear();
            _planBuffer.Clear(); 
            _existingNodes.Clear();

            var actions = _actions.AsReadOnlySpan();
            var worldState = pawn.GetWorldState();
            var goal = pawn.GetGoal();

            if (goal.FlagCount > 0 && actions.Length > 0)
            {
                GoapNode? winner = null;

                {
                    // initialize open list with current world state
                    var newNode = Pool.Rent<GoapNode>();
                    newNode.Init(null, worldState, null, 0, 0, worldState.GetUnsatisfiedFlagCount(goal));
                    _existingNodes.Add(newNode.State.Flags, newNode);
                    _openList.Add(newNode);
                }

                while (_openList.Count > 0)
                {
                    // take last one (highest score)
                    var currentNode = _openList[^1];
                    _openList.RemoveAt(_openList.Count - 1);

                    if (currentNode.State.Satisfies(goal))
                    {
                        // todo we want to collect all plans instead of just the first one so we can pick the one with lowest cost.
                        // and then choose a random one among them.
                        winner = currentNode;
                        break;
                    }

                    // add all possible actions from here:
                    if (currentNode.PlanDepth < maxPlanLength)
                    {
                        foreach (var action in actions)
                        {
                            if (!action.Preconditions.Satisfies(currentNode.State)) continue;

                            var newState = currentNode.State;
                            newState.ApplyEffects(action.Effects);

                            var planDepth = currentNode.PlanDepth + 1;
                            var planCost = currentNode.PlanCost + action.Cost;
                            var score = newState.GetUnsatisfiedFlagCount(goal);

                            if (_existingNodes.TryGetValue(newState.Flags, out var existingNode))
                            {
                                // this node already visited.
                                if (planCost < existingNode.PlanCost) // current path to this node is better?
                                    existingNode.ChangeParent(currentNode, planDepth, planCost); // yes: adjust node data

                                continue; // don't create a node that already exists.
                            }
                            
                            var newNode = Pool.Rent<GoapNode>();
                            newNode.Init(action, newState, currentNode, planDepth, planCost, score);
                            _existingNodes.Add(newNode.State.Flags, newNode);

                            _openList.Add(newNode);
                        }

                        
                        _openList.Sort(ByScoreDescending);
                    }
                }

                winner ??= _openList.LastOrDefault(); // winner or best-score
                if (winner != null)
                {
                    // assemble winning plan: all actions in reverse order.
                    var planLength = winner.PlanDepth;
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
            foreach (var node in _existingNodes.Values)
                node.Return();
            _existingNodes.Clear();
            _openList.Clear();
        }

        private static int ByScoreDescending(GoapNode a, GoapNode b)
        {
            return -a.CostPlusHeuristic.CompareTo(b.CostPlusHeuristic);
        }
        
        public void Init(GoapAction idleAction, params GoapAction[] actions)
        {
            _idleAction = idleAction;
            _actions.AddArray(actions);
        }
    }
}