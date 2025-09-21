using System.Numerics;

namespace Barebone.Geometry
{
    /// <summary>
    /// Geometric intersection calculations.
    /// </summary>
    public static class Intersections
    {
        /// <summary>
        /// Returns the factor [0,1] along segment a where it intersects segment b. Returns null if there is no intersection.
        /// </summary>
        public static float? LineLine(in Vector2 a0, in Vector2 a1, in Vector2 b0, in Vector2 b1)
        {
            // source: https://gamedev.stackexchange.com/questions/44720/line-intersection-from-parametric-equation

            var a = a0;
            var b = a1 - a0;
            var c = b0;
            var d = b1 - b0;

            var crossDB = d.Cross(b);
            if (crossDB < float.Epsilon)
                return null;

            var crossDbInv = 1f / crossDB;

            var u = (b.X * (c.Y - a.Y) + b.Y * (a.X - c.X)) * crossDbInv;
            var t = (d.X * (a.Y - c.Y) + d.Y * (c.X - a.X)) * -crossDbInv;

            // Check if the intersection point lies on both segments
            if (t is >= 0 and <= 1 && u is >= 0 and <= 1)
                return t;

            return null;
        }

        /// <summary>
        /// Returns the percentage [0,1] along segment a of the closest intersection with the given circle. Returns null if there is no intersection.
        /// </summary>
        public static float? IntersectLineCircle(in Vector2 a0, in Vector2 a1, in Vector2 center, float radius)
        {
            // Direction vector of the segment
            var d = a1 - a0;

            var circleToA0 = a0 - center;

            var a = Vector2.Dot(d, d);
            var b = 2 * Vector2.Dot(circleToA0, d);
            var c = Vector2.Dot(circleToA0, circleToA0) - radius * radius;

            var discriminant = b * b - 4 * a * c;

            // No intersection if the discriminant is negative
            if (discriminant < 0)
                return null;

            // Compute the two possible values of t (parameter on the ray)
            var sqrtDiscriminant = MathF.Sqrt(discriminant);
            var a2Inv = 1f / (2 * a);
            var t1 = (-b - sqrtDiscriminant) * a2Inv;
            var t2 = (-b + sqrtDiscriminant) * a2Inv;

            var isT1OnA = t1 is >= 0 and <= 1;
            var isT2OnA = t2 is >= 0 and <= 1;

            if (isT1OnA)
                return isT2OnA ? MathF.Min(t1, t2) : t1;

            if (isT2OnA)
                return t2;

            return null;
        }

        /// <summary>
        /// Untested, supposedly very fast...
        /// </summary>
        public static float? RayAabb2(in Ray2 r, in Aabb aabb)
        {
            var rDirInv = new Vector2(1f / r.DirectionNorm.X, 1f / r.DirectionNorm.Y);

            // unrolled version of: https://tavianator.com/2015/ray_box_nan.html

            var tmin = float.MinValue;
            var tmax = float.MaxValue;

            // X
            {
                var t1 = (aabb.MinCorner.X - r.Origin.X) * rDirInv.X;
                var t2 = (aabb.MaxCorner.X - r.Origin.X) * rDirInv.X;

                tmin = MathF.Max(tmin, MathF.Min(t1, t2));
                tmax = MathF.Min(tmax, MathF.Max(t1, t2));
            }
            // Y
            {
                var t1 = (aabb.MinCorner.Y - r.Origin.Y) * rDirInv.Y;
                var t2 = (aabb.MaxCorner.Y - r.Origin.Y) * rDirInv.Y;

                tmin = MathF.Max(tmin, MathF.Min(t1, t2));
                tmax = MathF.Min(tmax, MathF.Max(t1, t2));
            }

            if (tmax > tmin && tmin >= 0f)
                return tmin;
            return null;
        }

        /// <summary>
        /// Returns the distance (in raylength units) from the ray's origin to the first intersection with the given aabb.
        /// Returns null if there is no intersection. Origin INSIDE the aabb is considered NO intersection.
        /// </summary>
        public static float? RayAabb(in Ray2 r, in Aabb aabb)
        {
            // better solution here: https://people.csail.mit.edu/amy/papers/box-jgt.pdf

            // Only 2 edges can face the ray's origin. So we find which ones and test the ray against them.

            // first vertical edges:
            if (r.DirectionNorm.X != 0)
            {
                if (r.Origin.X < aabb.MinCorner.X)
                {
                    // the left edge is facing the ray origin
                    var boundX = aabb.MinCorner.X;
                    var t = (boundX - r.Origin.X) / r.DirectionNorm.X;
                    if (t > 0)
                    {
                        var y = r.Origin.Y + t * r.DirectionNorm.Y;
                        if (y >= aabb.MinCorner.Y && y <= aabb.MaxCorner.Y)
                            return t;
                    }
                }
                else if (r.Origin.X > aabb.MaxCorner.X)
                {
                    // the right edge is facing the ray origin
                    var boundX = aabb.MaxCorner.X;
                    var t = (boundX - r.Origin.X) / r.DirectionNorm.X;
                    if (t > 0)
                    {
                        var y = r.Origin.Y + t * r.DirectionNorm.Y;
                        if (y >= aabb.MinCorner.Y && y <= aabb.MaxCorner.Y)
                            return t;
                    }
                }
                // else, the ray's origin is between the vertical edges, so we can skip to the horizontal edge test
            }

            // now the horizontal edges:
            if (r.DirectionNorm.Y != 0)
            {
                if (r.Origin.Y < aabb.MinCorner.Y)
                {
                    // the bottom edge is facing the ray origin
                    var boundY = aabb.MinCorner.Y;
                    var t = (boundY - r.Origin.Y) / r.DirectionNorm.Y;
                    if (t > 0)
                    {
                        var x = r.Origin.X + t * r.DirectionNorm.X;
                        if (x >= aabb.MinCorner.X && x <= aabb.MaxCorner.X)
                            return t;
                    }
                }
                else if (r.Origin.Y > aabb.MaxCorner.Y)
                {
                    // the top edge is facing the ray origin
                    var boundY = aabb.MaxCorner.Y;
                    var t = (boundY - r.Origin.Y) / r.DirectionNorm.Y;
                    if (t > 0)
                    {
                        var x = r.Origin.X + t * r.DirectionNorm.X;
                        if (x >= aabb.MinCorner.X && x <= aabb.MaxCorner.X)
                            return t;
                    }
                }
                // else, the ray's origin is INSIDE the aabb, which we count as NO intersection
            }

            return null;
        }
    }
}
