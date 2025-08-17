using System.Diagnostics.Contracts;
using System.Numerics;

namespace Barebone.Architecture.Ecs.Components
{
    public struct TransformCompo
    {
        public Vector2 Position;
        public float Angle;

        [Pure]
        public Matrix3x2 GetTransform()
        {
            var m = Matrix3x2.CreateRotation(Angle);
            m.Translation = Position; // translation doesn't impact rotation. This is faster than multiplying a rotation matrix with a translation matrix.
            return m;
        }
    }
}
