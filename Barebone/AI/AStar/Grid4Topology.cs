using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    /// <summary>
    /// 4-directional (cardinal-only) grid topology.
    /// </summary>
    public readonly struct Grid4Topology(int width, int height, ReadOnlyMemory<bool> obstacles) : IGraphTopology
    {
        public int NodeCount => width * height;
        public int MaxNeighbours => 4;

        public int GetNeighbours(int nodeIdx, Span<Connection> buffer)
        {
            var obs = obstacles.Span;
            var cx = nodeIdx % width;
            var cy = nodeIdx / width;
            var count = 0;

            if (cy + 1 < height && !obs[nodeIdx + width]) 
                buffer[count++] = new(nodeIdx + width, 1f);
            if (cy - 1 >= 0    && !obs[nodeIdx - width])  
                buffer[count++] = new(nodeIdx - width, 1f);
            if (cx + 1 < width && !obs[nodeIdx + 1])      
                buffer[count++] = new(nodeIdx + 1, 1f);
            if (cx - 1 >= 0   && !obs[nodeIdx - 1])       
                buffer[count++] = new(nodeIdx - 1, 1f);

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Heuristic(int fromIdx, int toIdx)
        {
            var dx = Math.Abs(fromIdx % width - toIdx % width);
            var dy = Math.Abs(fromIdx / width - toIdx / width);
            return dx + dy;
        }

        public Vector2I GridSize { get; } = new (width, height);
    }
}
