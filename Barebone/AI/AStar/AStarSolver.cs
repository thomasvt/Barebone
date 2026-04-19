using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    public class AStarSolver(Vector2I gridSize, AStarSolver.HeuristicDelegate heuristic)
    {
        public delegate float HeuristicDelegate(in Vector2I from, in Vector2I to);

        private record struct Node(float G, bool IsSeen);

        private readonly record struct Connection(int DeltaIndex, float Cost, int DeltaX, int DeltaY);

        private readonly int _width = gridSize.X;
        private readonly int _cellCount = gridSize.X * gridSize.Y;

        private readonly PriorityQueue<int, float> _openList = new(1000);
        private readonly bool[] _closedList = new bool[gridSize.X * gridSize.Y];
        private readonly Node[] _nodes = new Node[gridSize.X * gridSize.Y];
        private readonly int[] _parents = new int[gridSize.X * gridSize.Y];

        private const int NoParent = -1;

        private static readonly float Sqrt2 = MathF.Sqrt(2);

        public void FindPath(in ReadOnlySpan<bool> obstacles, in Vector2I start, in Vector2I goal, in BBList<Vector2I> solutionBuffer)
        {
            if (obstacles.Length != _cellCount)
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
            Array.Fill(_parents, NoParent);
            Array.Clear(_closedList);
            _openList.Clear();

            _nodes[startIdx] = new(0f, true);
            _openList.Enqueue(startIdx, heuristic(start, goal));

            var w = _width;
            ReadOnlySpan<Connection> connections =
            [
                new(w, 1, 0, 1),         // up
                new(1, 1, 1, 0),          // right
                new(-w, 1, 0, -1),        // down
                new(-1, 1, -1, 0),        // left
                new(w + 1, Sqrt2, 1, 1),  // up-right
                new(w - 1, Sqrt2, -1, 1), // up-left
                new(-w + 1, Sqrt2, 1, -1),// down-right
                new(-w - 1, Sqrt2, -1, -1)// down-left
            ];

            while (_openList.Count > 0)
            {
                var currentIdx = _openList.Dequeue();

                if (_closedList[currentIdx])
                    continue;

                if (currentIdx == goalIdx)
                {
                    ConstructPath(goalIdx, solutionBuffer);
                    return;
                }

                ref readonly var currentNode = ref _nodes[currentIdx];
                _closedList[currentIdx] = true;

                var cx = currentIdx % w;
                var cy = currentIdx / w;

                foreach (ref readonly var conn in connections)
                {
                    var nx = cx + conn.DeltaX;
                    var ny = cy + conn.DeltaY;

                    // bounds check ((uint)nx >= (uint)w checks both < 0 and >= w in a single comparison)
                    if ((uint)nx >= (uint)w || (uint)ny >= (uint)gridSize.Y)
                        continue;

                    var neighbourIdx = currentIdx + conn.DeltaIndex;

                    if (_closedList[neighbourIdx] || obstacles[neighbourIdx])
                        continue;

                    if (conn.DeltaX != 0 && conn.DeltaY != 0)
                    {
                        // prevent diagonal moves if the adjacent cells are obstacles
                        if (obstacles[currentIdx + conn.DeltaX] || obstacles[currentIdx + conn.DeltaY * w])
                            continue;
                    }

                    ref var neighbourNode = ref _nodes[neighbourIdx];
                    var g = currentNode.G + conn.Cost;

                    if (neighbourNode.IsSeen && g >= neighbourNode.G)
                        continue;

                    neighbourNode.G = g;
                    neighbourNode.IsSeen = true;
                    _parents[neighbourIdx] = currentIdx;
                    var f = g + heuristic(IdxToPos(neighbourIdx), goal);
                    _openList.Enqueue(neighbourIdx, f);
                }
            }
        }

        private void ConstructPath(int goalIdx, in BBList<Vector2I> solutionBuffer)
        {
            var idx = goalIdx;
            while (true)
            {
                solutionBuffer.Add(IdxToPos(idx));
                var parentIdx = _parents[idx];
                if (parentIdx == NoParent)
                    break;
                idx = parentIdx;
            }

            solutionBuffer.Reverse();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PosToIdx(in Vector2I position)
        {
            return position.Y * _width + position.X;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2I IdxToPos(int idx)
        {
            return new Vector2I(idx % _width, idx / _width);
        }
    }
}
