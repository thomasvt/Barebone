using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    public class AStarSolver(Vector2I gridSize, AStarSolver.HeuristicDelegate heuristic)
    {
        public delegate float HeuristicDelegate(in Vector2I from, in Vector2I to);
        
        private record struct Node(float G, Vector2I ParentPos, bool IsVisited);

        private record struct Connection(Vector2I Direction, float Cost);

        private readonly PriorityQueue<Vector2I, float> _openList = new(1000);
        private readonly bool[] _closedList = new bool[gridSize.X * gridSize.Y];
        private readonly Node[] _nodes = new Node[gridSize.X * gridSize.Y];

        public void FindPath(in bool[] obstacle, in Vector2I start, in Vector2I goal, in BBList<Vector2I> solutionBuffer)
        {
            if (obstacle.Length != gridSize.X * gridSize.Y)
                throw new ArgumentException("Obstacle array size does not match grid size.");

            Array.Clear(_nodes);
            Array.Clear(_closedList);
            _openList.Clear();

            _nodes[PosToIdx(start)] = new(0f, start, true);
            _openList.Enqueue(start, heuristic(start, goal));

            ReadOnlySpan<Connection> connections = stackalloc Connection[]
            {
                new(new(0, 1), 1),
                new(new(1, 0), 1),
                new(new(0, -1), 1),
                new(new(-1, 0), 1),
                new(new(1,1), MathF.Sqrt(2)),
                new(new(-1,1), MathF.Sqrt(2)),
                new(new(1,-1), MathF.Sqrt(2)),
                new(new(-1,-1), MathF.Sqrt(2)),
            };

            while (_openList.Count > 0)
            {
                var currentPos = _openList.Dequeue();
                var currentLinearIdx = PosToIdx(currentPos);
                ref readonly var currentNode = ref _nodes[currentLinearIdx];

                if (currentPos.Equals(goal))
                {
                    ConstructPath(currentPos, solutionBuffer);
                    return;
                }

                _closedList[currentLinearIdx] = true;

                foreach (ref readonly var connection in connections)
                {
                    var neighbourPos = currentPos + connection.Direction;
                    var neighbourIdx = PosToIdx(neighbourPos);

                    if (_closedList[neighbourIdx] || obstacle[neighbourIdx])
                        continue;

                    ref var neighbourNode = ref _nodes[neighbourIdx];
                    var g = currentNode.G + connection.Cost;

                    if (neighbourNode.IsVisited && g >= neighbourNode.G)
                        continue; // neighbour already visited with a better path

                    neighbourNode.G = g;
                    neighbourNode.ParentPos = currentPos;
                    neighbourNode.IsVisited = true;
                    var f = g + heuristic(neighbourPos, goal);
                    _openList.Enqueue(neighbourPos, f);
                }
            }
        }

        private void ConstructPath(in Vector2I goalPosition, in BBList<Vector2I> solutionBuffer)
        {
            solutionBuffer.Clear();
            var currentPos = goalPosition;
            while (true)
            {
                solutionBuffer.Add(currentPos);
                var currentNode = _nodes[PosToIdx(currentPos)];
                if (currentNode.ParentPos.Equals(currentPos))
                    break;
                currentPos = currentNode.ParentPos;
            }
            
            solutionBuffer.Reverse();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PosToIdx(in Vector2I position)
        {
            return position.Y * gridSize.X + position.X;
        }
    }
}
