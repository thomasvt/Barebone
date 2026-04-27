using Barebone.Geometry;

namespace Barebone.AI.AStar
{
    public interface IPathSolver
    {
        bool FindPath(in Vector2I start, in Vector2I goal, in BBList<Vector2I> solutionBuffer);
    }
}
