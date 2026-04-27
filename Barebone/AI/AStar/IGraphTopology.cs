using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    public interface IGraphTopology
    {
        int NodeCount { get; }
        int MaxNeighbours { get; }
        /// <summary>
        /// Must fill the buffer with viable neighbours and return the count (up to MaxNeighbours).
        /// </summary>
        int GetNeighbours(int nodeIdx, Span<Connection> buffer);
        /// <summary>
        /// Returns the distance heuristic for the 2 given positions (as indices)
        /// </summary>
        float Heuristic(int fromIdx, int toIdx);

        Vector2I GridSize { get; }
    }
}
