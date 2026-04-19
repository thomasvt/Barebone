using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    public class AStarSolver(Vector2I gridSize, AStarSolver.HeuristicDelegate heuristic)
    {
        public delegate float HeuristicDelegate(in Vector2I from, in Vector2I to);
        
        private record struct Node(float G, bool IsSeen);

        private record struct Connection(Vector2I Direction, float Cost, bool IsDiagonal);

        private readonly PriorityQueue<Vector2I, float> _openList = new(1000);
        private readonly bool[] _closedList = new bool[gridSize.X * gridSize.Y];
        private readonly Node[] _nodes = new Node[gridSize.X * gridSize.Y];
        private readonly Vector2I[] _parents = new Vector2I[gridSize.X * gridSize.Y];

        private readonly Vector2I _startParent = new(-1, -1);

        private readonly static float Sqrt2 = MathF.Sqrt(2);

        public void FindPath(in ReadOnlySpan<bool> obstacles, in Vector2I start, in Vector2I goal, in BBList<Vector2I> solutionBuffer)
        {
            if (obstacles.Length != gridSize.X * gridSize.Y)
                throw new ArgumentException("Obstacle array size does not match grid size.");

            solutionBuffer.Clear();

            if (start.Equals(goal))
            {
                solutionBuffer.Add(start);
                return;
            }

            var startIdx = PosToIdx(start);
            var goalIdx = PosToIdx(goal);

            if (obstacles[startIdx] || obstacles[goalIdx])
                return;

            Array.Clear(_nodes);
            Array.Clear(_parents);
            Array.Clear(_closedList);
            _openList.Clear();

            _nodes[startIdx] = new(0f, true);
            _parents[startIdx] = _startParent;
            _openList.Enqueue(start, heuristic(start, goal));

            ReadOnlySpan<Connection> connections = stackalloc Connection[]
            {
                new(new(0, 1), 1, false),
                new(new(1, 0), 1, false),
                new(new(0, -1), 1, false),
                new(new(-1, 0), 1, false),
                new(new(1,1), Sqrt2, true),
                new(new(-1,1), Sqrt2, true),
                new(new(1,-1), Sqrt2, true),
                new(new(-1,-1), Sqrt2, true),
            };

            while (_openList.Count > 0)
            {
                var currentPos = _openList.Dequeue();
                var currentIdx = PosToIdx(currentPos);

                if (_closedList[currentIdx]) // queue might have closed it due to finding and processing a better path through it
                    continue;

                ref readonly var currentNode = ref _nodes[currentIdx];

                if (currentPos.Equals(goal))
                {
                    ConstructPath(currentPos, solutionBuffer);
                    return;
                }

                _closedList[currentIdx] = true;

                foreach (ref readonly var connection in connections)
                {
                    var neighbourPos = currentPos + connection.Direction;
                    if (neighbourPos.X < 0 || neighbourPos.X >= gridSize.X || neighbourPos.Y < 0 || neighbourPos.Y >= gridSize.Y)
                        continue;

                    var neighbourIdx = PosToIdx(neighbourPos);

                    if (_closedList[neighbourIdx] || obstacles[neighbourIdx])
                        continue;

                    if (connection.IsDiagonal)
                    {
                        // disallow moving through diagonally adjacent obstacles.
                        var corner1 = new Vector2I(currentPos.X + connection.Direction.X, currentPos.Y);
                        var corner2 = new Vector2I(currentPos.X, currentPos.Y + connection.Direction.Y);

                        if (obstacles[PosToIdx(corner1)] || obstacles[PosToIdx(corner2)])
                            continue;
                    }

                    ref var neighbourNode = ref _nodes[neighbourIdx];
                    var g = currentNode.G + connection.Cost;

                    if (neighbourNode.IsSeen && g >= neighbourNode.G)
                        continue; // neighbour already visited with a better path

                    neighbourNode.G = g;
                    neighbourNode.IsSeen = true;
                    _parents[neighbourIdx] = currentPos;
                    var f = g + heuristic(neighbourPos, goal);
                    _openList.Enqueue(neighbourPos, f);
                }
            }
        }

        private void ConstructPath(in Vector2I goalPosition, in BBList<Vector2I> solutionBuffer)
        {
            var currentPos = goalPosition;
            while (true)
            {
                solutionBuffer.Add(currentPos);
                var parentPos = _parents[PosToIdx(currentPos)];
                if (parentPos == _startParent)
                    break; // start reached
                currentPos = parentPos;
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
