using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    /// <summary>
    /// 8-directional grid topology with diagonal corner-cutting prevention.
    /// </summary>
    public readonly struct Grid8Topology(int width, int height, ReadOnlyMemory<bool> obstacles) : IGraphTopology
    {
        private readonly static float Sqrt2 = MathF.Sqrt(2);

        public int NodeCount => width * height;
        public int MaxNeighbours => 8;

        public int GetNeighbours(int nodeIdx, Span<Connection> buffer)
        {
            var obs = obstacles.Span;
            var cx = nodeIdx % width;
            var cy = nodeIdx / width;
            var count = 0;

            bool canUp = cy + 1 < height;
            bool canDown = cy - 1 >= 0;
            bool canRight = cx + 1 < width;
            bool canLeft = cx - 1 >= 0;

            bool upFree = canUp && !obs[nodeIdx + width];
            bool downFree = canDown && !obs[nodeIdx - width];
            bool rightFree = canRight && !obs[nodeIdx + 1];
            bool leftFree = canLeft && !obs[nodeIdx - 1];

            if (upFree)    buffer[count++] = new(nodeIdx + width, 1f);
            if (downFree)  buffer[count++] = new(nodeIdx - width, 1f);
            if (rightFree) buffer[count++] = new(nodeIdx + 1, 1f);
            if (leftFree)  buffer[count++] = new(nodeIdx - 1, 1f);

            if (upFree && rightFree && !obs[nodeIdx + width + 1])
                buffer[count++] = new(nodeIdx + width + 1, Sqrt2);
            if (upFree && leftFree && !obs[nodeIdx + width - 1])
                buffer[count++] = new(nodeIdx + width - 1, Sqrt2);
            if (downFree && rightFree && !obs[nodeIdx - width + 1])
                buffer[count++] = new(nodeIdx - width + 1, Sqrt2);
            if (downFree && leftFree && !obs[nodeIdx - width - 1])
                buffer[count++] = new(nodeIdx - width - 1, Sqrt2);

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Heuristic(int fromIdx, int toIdx)
        {
            var fx = fromIdx % width;
            var fy = fromIdx / width;
            var tx = toIdx % width;
            var ty = toIdx / width;
            var dx = Math.Abs(fx - tx);
            var dy = Math.Abs(fy - ty);
            return dx + dy + (Sqrt2 - 2f) * Math.Min(dx, dy);
        }

        public Vector2I GridSize { get; } = new(width, height);
    }
}
