using System.Numerics;
using System.Runtime.CompilerServices;

namespace Barebone.Geometry
{
    internal record struct Edge2(Vector2 A, Vector2 B)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Edge2 Reverse()
        {
            return new Edge2(B, A);
        }

        /// <summary>
        /// Checks if p is on the right side of ab when standing on a, looking towards b.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInRightHalfSpace(in Vector2 p)
        {
            var ab = B - A;
            var ap = p - A;
            return ab.X * ap.Y - ab.Y * ap.X <= 0f;
        }

        /// <summary>
        /// Checks if p is on the right side (or exactly on top) of ab when standing on a, looking towards b.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRightHalfSpace(in Vector2 p, in Vector2 a, in Vector2 b)
        {
            var ab = b - a;
            var ap = p - a;
            return ab.X * ap.Y - ab.Y * ap.X <= 0f;
        }

        /// <summary>
        /// Returns an edge at a certain offset in an edgeloop. An edgeloop is a list of points forming a chain where the last point is connected to the first: aka edgeloop, aka polygon.
        /// </summary>
        public static Edge2 FromEdgeLoop(Span<Vector2> edgeLoop, int offet)
        {
            return new Edge2(edgeLoop[offet], edgeLoop[(offet + 1) % edgeLoop.Length]);
        }
    }
}
