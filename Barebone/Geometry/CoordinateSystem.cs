using System.Numerics;
using System.Runtime.CompilerServices;

namespace Barebone.Geometry
{
    /// <summary>
    /// Defines a custom coordinate system with axes defined by vectors in the global coordinate system.
    /// </summary>
    public record struct CoordinateSystem(Vector2 XAxis, Vector2 YAxis)
    {
        /// <summary>
        /// Projects a vector expressed in global coordinates onto this custom CoordinateSytem.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Project(in Vector2 globalVector)
        {
            return new(Vector2.Dot(globalVector, XAxis), Vector2.Dot(globalVector, YAxis));
        }

        /// <summary>
        /// Unprojects a vector expressed in this CoordinateSystem into global coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Unproject(in Vector2 v)
        {
            return v.X * XAxis + v.Y * YAxis;
        }
    }
}
