namespace Barebone.AI.AStar
{
    public class AStarSolver<TGraph>(TGraph graph) where TGraph : struct, IGraphTopology // when TGraph is a struct, compiler will build a non-generic result that can even get its methods inlined.
    {
        private record struct Node(float G, bool IsSeen);

        private const int NoParent = -1;

        private readonly PriorityQueue<int, float> _openList = new(1000);
        private readonly bool[] _closedList = new bool[graph.NodeCount];
        private readonly Node[] _nodes = new Node[graph.NodeCount];
        private readonly int[] _parents = new int[graph.NodeCount];

        public bool FindPath(int startIdx, int goalIdx, in BBList<int> solutionBuffer)
        {
            solutionBuffer.Clear();

            if (startIdx == goalIdx)
            {
                solutionBuffer.Add(startIdx);
                return true;
            }

            Array.Clear(_nodes);
            Array.Fill(_parents, NoParent);
            Array.Clear(_closedList);
            _openList.Clear();

            _nodes[startIdx] = new(0f, true);
            _openList.Enqueue(startIdx, graph.Heuristic(startIdx, goalIdx));

            Span<Connection> neighbourBuffer = stackalloc Connection[graph.MaxNeighbours];

            while (_openList.Count > 0)
            {
                var currentIdx = _openList.Dequeue();

                if (_closedList[currentIdx])
                    continue;

                if (currentIdx == goalIdx)
                {
                    ConstructPath(goalIdx, solutionBuffer);
                    return true;
                }

                ref readonly var currentNode = ref _nodes[currentIdx];
                _closedList[currentIdx] = true;

                var count = graph.GetNeighbours(currentIdx, neighbourBuffer);

                for (var i = 0; i < count; i++)
                {
                    ref readonly var edge = ref neighbourBuffer[i];
                    var neighbourIdx = edge.NeighbourIdx;

                    if (_closedList[neighbourIdx])
                        continue;

                    ref var neighbourNode = ref _nodes[neighbourIdx];
                    var g = currentNode.G + edge.Cost;

                    if (neighbourNode.IsSeen && g >= neighbourNode.G)
                        continue;

                    neighbourNode.G = g;
                    neighbourNode.IsSeen = true;
                    _parents[neighbourIdx] = currentIdx;
                    _openList.Enqueue(neighbourIdx, g + graph.Heuristic(neighbourIdx, goalIdx));
                }
            }

            return false;
        }

        private void ConstructPath(int goalIdx, in BBList<int> solutionBuffer)
        {
            var idx = goalIdx;
            while (true)
            {
                solutionBuffer.Add(idx);
                var parentIdx = _parents[idx];
                if (parentIdx == NoParent)
                    break;
                idx = parentIdx;
            }

            solutionBuffer.Reverse();
        }
    }
}
