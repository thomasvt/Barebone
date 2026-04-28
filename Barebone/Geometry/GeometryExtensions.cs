using System.Numerics;
using System.Runtime.CompilerServices;

namespace Barebone.Geometry;

public static class GeometryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Aabb ToAabb(this AabbI aabb)
    {
        return new Aabb(aabb.MinCorner, aabb.MaxCornerIncl);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Triangle3 ToTriangle3(this Triangle2 t, float z = 0)
    {
        return new Triangle3(new(t.A, z), new(t.B, z), new(t.C, z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 XY(this Vector3 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static Matrix4x4 To4x4(this Matrix3x2 m, float z = 0f)
    {
        return new Matrix4x4(
            m.M11, m.M12, 0, 0, 
            m.M21, m.M22, 0, 0, 
            0, 0, 1, 0, 
            m.M31, m.M32, z, 1);
    }

    extension(Vector2 vector)
    {
        /// <summary>
        /// Returns a <see cref="Vector2I"/> by rounding X and Y down to the next integer.
        /// </summary>
        public Vector2I Floor()
        {
            return new ((int)MathF.Floor(vector.X), (int)MathF.Floor(vector.Y));
        }

        /// <summary>
        /// Returns a <see cref="Vector2I"/> by rounding X and Y to the closest integer.
        /// </summary>
        public Vector2I Round()
        {
            return new((int)MathF.Round(vector.X, MidpointRounding.AwayFromZero), (int)MathF.Round(vector.Y, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Returns a <see cref="Vector2I"/> by rounding X and Y up to the next integer.
        /// </summary>
        public Vector2I Ceiling()
        {
            return new ((int)MathF.Ceiling(vector.X), (int)MathF.Ceiling(vector.Y));
        }

        /// <summary>
        /// Gets angle of the vector within (-180, +180)
        /// </summary>
        public float GetAngle()
        {
            return MathF.Atan2(vector.Y, vector.X);
        }

        /// <summary>
        /// Gets angle of the vector within (-180, +180), or the given default if the vector has no length.
        /// </summary>
        public float GetAngleOrDefault(float @default = 0f)
        {
            return vector is { X: 0, Y: 0 } ? @default : MathF.Atan2(vector.Y, vector.X);
        }

        /// <summary>
        /// Similar to interpolating from 'vector' to 'target' but with a fixed step-distance, instead of a percentage.
        /// Adds to 'vector' a step-vector pointing from 'vector' to 'target'. Does not overshoot.
        /// </summary>
        /// <param name="isTargetReached">True if 'step' was large enough to reach 'target', or false if not.</param>
        public Vector2 StepTowards(Vector2 target, float stepDistance, out bool isTargetReached)
        {
            var inc2 = stepDistance * stepDistance;
            var toTarget = target - vector;
            if (toTarget.LengthSquared() <= inc2)
            {
                isTargetReached = true;
                return target;
            }

            isTargetReached = false;
            return vector + Vector2.Normalize(toTarget) * stepDistance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanLength()
        {
            return MathF.Abs(vector.X) + MathF.Abs(vector.Y);
        }

        /// <summary>
        /// Returns the velocity vector bounced off of a flat surface.
        /// </summary>
        public Vector2 Bounce(Vector2 surfaceNormal)
        {
            var tangent = surfaceNormal.CrossRight();
            var x = Vector2.Dot(vector, tangent);
            var y = Vector2.Dot(vector, surfaceNormal);
            return x * tangent - y * surfaceNormal;
        }

        /// <summary>
        /// Gets a perpendicular vector pointing to the left from the original vector and with the same length. (when X+ is right, Y+ is up)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 CrossLeft()
        {
            return new Vector2(-vector.Y, vector.X);
        }

        /// <summary>
        /// Gets a perpendicular vector pointing to the right from the original vector and with the same length. (when X+ is right, Y+ is up)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 CrossRight()
        {
            return new Vector2(vector.Y, -vector.X);
        }

        /// <summary>
        /// Adds y to the vector's Y value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 TranslateY(float y)
        {
            return vector with { Y = vector.Y + y };
        }

        /// <summary>
        /// Adds x to the vector's X value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 TranslateX(float x)
        {
            return vector with { X = vector.X + x };
        }

        /// <summary>
        /// Calculates the cross product ("a×b"). Interpretable as the surface area of the parallelogram formed by the two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cross(Vector2 b)
        {
            return vector.X * b.Y - vector.Y * b.X;
        }

        /// <summary>
        /// Returns the vector capped to maxLength. If it's shorter, the original vector is returned.
        /// </summary>
        public Vector2 CapVectorLength(float maxLength)
        {
            var length = vector.Length();
            if (length > maxLength)
                return vector / length * maxLength;
            return vector;
        }

        /// <summary>
        /// Normalises the vector, or returns Vector2.Zero if the vector has no length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 NormalizeOrZero()
        {
            return vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReverseY()
        {
            return vector with { Y = -vector.Y };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3(float z = 0)
        {
            return new Vector3(vector.X, vector.Y, z);
        }

        /// <summary>
        /// Normalises the vector but also outputs its Length., or returns Vector2.Zero if the vector has no length. And also outputs its Length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 NormalizeOrZero(out float length)
        {
            if (vector == Vector2.Zero)
            {
                length = 0f;
                return Vector2.Zero;
            }

            length = vector.Length();
            return vector / length;
        }

        /// <summary>
        /// Normalises the vector but also outputs its Length., or returns Vector2.Zero if the vector has no length. And also outputs its Length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Normalize(out float length)
        {
            length = vector.Length();
            var norm = vector / length;
#if DEBUG
            if (float.IsNaN(norm.X) || float.IsNaN(norm.Y)) throw new Exception("Normalized vector contains NaN");
#endif
            return norm;
        }
    }
    

    extension(float angle)
    {
        /// <summary>
        /// Rotates the angle one step towards the targetAngle along the shortest side of the circle. Guarantees the final step returns 'targetAngle' exactly.
        /// </summary>
        public float StepAngleTowards(float targetAngle, float absoluteStep, out bool isTargetReached)
        {
            var shortestAngleChange = angle.GetShortestAngleTo(targetAngle);

            if (MathF.Abs(shortestAngleChange) <= absoluteStep)
            {
                isTargetReached = true;
                return targetAngle;
            }

            isTargetReached = false;
            return angle + MathF.Sign(shortestAngleChange) * absoluteStep;
        }

        /// <summary>
        /// Steps the value one step towards 'target'. If the step is larger than the remaining distance to 'target', the result is set to the exact target and true is returned. Else false is returned.
        /// </summary>
        /// <param name="isTargetReached">True if 'step' was large enough to reach 'target', or false if not.</param>
        public float StepTowards(float target, float absoluteStep, out bool isTargetReached)
        {
            var toTarget = target - angle;
            if (MathF.Abs(toTarget) <= absoluteStep)
            {
                isTargetReached = true;
                return target;
            }

            isTargetReached = false;
            return angle + MathF.Sign(toTarget) * absoluteStep;
        }

        /// <summary>
        /// Returns the signed difference between two angles in radians within (-180, 180). It always returns the shortest path.
        /// </summary>
        public float GetShortestAngleTo(float toAngle)
        {
            angle = NormalizeAngle(angle);
            toAngle = NormalizeAngle(toAngle);

            var diff = toAngle - angle;
            var diffAbs = MathF.Abs(diff);
            if (diffAbs > MathF.PI)
            {
                // pick the other, shorter way around the circle:
                diff = -MathF.Sign(diff) * (MathF.Tau - diff);
            }

            return diff;
        }

        /// <summary>
        /// Normalizes an angle in radians to [0, tau) (=2*pi)
        /// </summary>
        public float NormalizeAngle()
        {
            angle %= MathF.Tau;
            if (angle < 0)
                return angle + MathF.Tau;
            return angle;
        }

        public Vector2 AngleToVector2(float length = 1f)
        {
            var (s, c) = MathF.SinCos(angle);
            return new Vector2(c * length, s * length);
        }
    }
}