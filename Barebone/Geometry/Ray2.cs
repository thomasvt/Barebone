using System.Numerics;

namespace Barebone.Geometry
{
    public readonly struct Ray2
    {
        private Ray2(Vector2 origin, Vector2 directionNorm)
        {
            Origin = origin;
            DirectionNorm = directionNorm;
        }

        /// <summary>
        /// Creates a ray from origin and direction that is not guaranteed to be normalized.
        /// </summary>
        public static Ray2 From(Vector2 origin, Vector2 direction)
        {
            return new Ray2(origin, Vector2.Normalize(direction));
        }

        /// <summary>
        /// Creates a ray from origin and normalized direction. This is faster than `From()` but assumes you're sure that `directionNorm` is normalized.
        /// </summary>
        public static Ray2 FromNormalized(Vector2 origin, Vector2 directionNorm)
        {
            return new Ray2(origin, directionNorm);
        }

        public Ray2 Transform(Matrix3x2 transform)
        {
            return new Ray2(Vector2.Transform(Origin, transform), Vector2.TransformNormal(DirectionNorm, transform));
        }

        public Vector2 GetPosition(in float distance)
        {
            return Origin + DirectionNorm * distance;
        }

        public override string ToString()
        {
            return $"<{Origin.X}.{Origin.Y}:{DirectionNorm.X}.{DirectionNorm.Y}>";
        }

        /// <summary>
        /// Normalized direction of the ray.
        /// </summary>
        public readonly Vector2 DirectionNorm;

        public readonly Vector2 Origin;
    }
}
