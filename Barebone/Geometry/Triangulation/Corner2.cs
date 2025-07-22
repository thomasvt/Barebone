using System.Numerics;
using Barebone.Geometry;

namespace BareBone.Geometry.Triangulation
{
    internal static class Corner2
    {
        /// <summary>
        /// Gets the type of corner that is formed by abc. (b is the corner point)
        /// </summary>
        public static CornerType GetCornerType(in Vector2 a, in Vector2 b, in Vector2 c)
        {
            var cross = (a - b).Cross(c - b);
            if (cross < 0d)
                return CornerType.Reflex;
            return cross > 0d
                ? CornerType.Inflex
                : CornerType.StraightOrZero;
        }
    }
}
