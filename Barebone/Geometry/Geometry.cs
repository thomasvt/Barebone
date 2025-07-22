using System.Numerics;
using System.Runtime.CompilerServices;

namespace Barebone.Geometry
{
    public static class Geometry
    {
        /// <summary>
        /// Finds the point T on AB that is closest to P and returns a vector V from P to that point T.
        /// This result V lets you calculate T = P+V, but also the direction of V which is useful as surfacenormal (= -Normalize(V))
        /// </summary>
        public static Vector2 GetVectorFromPToAB(in Vector2 p, in Vector2 a, in Vector2 b)
        {
            var ab = b - a;
            var abLength = ab.Length();
            var abNorm = ab / abLength;

            var pOnAB = Vector2.Dot(p - a, abNorm);
            if (pOnAB <= 0)
                return a - p;
            if (pOnAB >= abLength)
                return b - p;
            var t = a + abNorm * pOnAB;
            return t - p;
        }


        /// <summary>
        /// Returns +1 if p lies to the right of ab, -1 if p lies to the left, 0 if p lies on ab.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHalfPlane(Vector2 p, Vector2 a, Vector2 b)
        {
            return MathF.Sign((p - a).Cross(b - a));
        }
    }
}
