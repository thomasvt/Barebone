using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Barebone.Geometry
{
    /// <summary>
    /// Axis Aligned Bounding Box. The positional properties (eg. Top, Bottom) all are in context of X+ is Right, and Y+ is Up.
    /// </summary>
    public record struct Aabb(Vector2 MinCorner, Vector2 MaxCorner)
    {
        public bool Contains(Vector2 v)
        {
            return v.X >= MinCorner.X && v.X < MaxCorner.X && v.Y >= MinCorner.Y && v.Y < MaxCorner.Y;
        }

        internal bool Contains(in Aabb aabb)
        {
            var result = true;
            result = result && MinCorner.X <= aabb.MinCorner.X;
            result = result && MinCorner.Y <= aabb.MinCorner.Y;
            result = result && aabb.MaxCorner.X <= MaxCorner.X;
            result = result && aabb.MaxCorner.Y <= MaxCorner.Y;
            return result;
        }

        public float? RayCast(in Ray2 ray, float tMax)
        {
            var tMin = 0f;

            // check X dimension slab:

            if (Math.Abs(ray.DirectionNorm.X) < float.Epsilon)
            {
                // Ray is parallel to slab in this dimension. No hit if origin not within slab
                if (ray.Origin.X < MinCorner.X || ray.Origin.X > MaxCorner.X)
                    return null;
            }
            else
            {
                // Compute intersection t value of ray with near and far plane of slab
                var directionInv = 1.0f / ray.DirectionNorm.X;
                var t1 = (MinCorner.X - ray.Origin.X) * directionInv;
                var t2 = (MaxCorner.X - ray.Origin.X) * directionInv;

                // Make t1 be intersection with near plane, t2 with far plane
                if (t1 > t2)
                    (t1, t2) = (t2, t1);

                // Compute the intersection of slab intersection intervals
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);

                // Exit with no collision as soon as slab intersection becomes empty
                if (tMin > tMax) return null;
            }

            // check Y dimension slab:

            if (Math.Abs(ray.DirectionNorm.Y) < float.Epsilon)
            {
                // Ray is parallel to slab in this dimension. No hit if origin not within slab
                if (ray.Origin.Y < MinCorner.Y || ray.Origin.Y > MaxCorner.Y)
                    return null;
            }
            else
            {
                // Compute intersection t value of ray with near and far plane of slab
                var directionInv = 1.0f / ray.DirectionNorm.Y;
                var t1 = (MinCorner.Y - ray.Origin.Y) * directionInv;
                var t2 = (MaxCorner.Y - ray.Origin.Y) * directionInv;

                // Make t1 be intersection with near plane, t2 with far plane
                if (t1 > t2)
                    (t1, t2) = (t2, t1);

                // Compute the intersection of slab intersection intervals
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);

                // Exit with no collision as soon as slab intersection becomes empty
                if (tMin > tMax) return null;
            }

            return tMin;
        }

        public float? RayCastWithSurfaceNormal(in Ray2 ray, in float maxFraction, out Vector2 surfaceNormal)
        {
            // from Box2d-net-standard in AABB.RayCast().

            surfaceNormal = default;
            var tmin = float.MinValue;
            var tmax = float.MaxValue;

            var p = ray.Origin;
            var d = ray.DirectionNorm;
            var absD = Vector2.Abs(d);

            var normal = Vector2.Zero;

            // X

            if (absD.X < float.Epsilon)
            {
                // Parallel.
                if (p.X < MinCorner.X || MaxCorner.X < p.X)
                    return null;
            }
            else
            {
                var inv_d = 1.0f / d.X;
                var t1 = (MinCorner.X - p.X) * inv_d;
                var t2 = (MaxCorner.X - p.X) * inv_d;

                // Sign of the normal vector.
                var s = -1.0f;

                if (t1 > t2)
                {
                    var temp = t1;
                    t1 = t2;
                    t2 = temp;
                    s = 1.0f;
                }

                // Push the min up
                if (t1 > tmin)
                {
                    normal = new Vector2(s, 0);
                    tmin = t1;
                }

                // Pull the max down
                tmax = MathF.Min(tmax, t2);

                if (tmin > tmax)
                    return null;
            }

            // Y

            if (absD.Y < float.Epsilon)
            {
                // Parallel.
                if (p.Y < MinCorner.Y || MaxCorner.Y < p.Y)
                    return null;
            }
            else
            {
                var inv_d = 1.0f / d.Y;
                var t1 = (MinCorner.Y - p.Y) * inv_d;
                var t2 = (MaxCorner.Y - p.Y) * inv_d;

                // Sign of the normal vector.
                var s = -1.0f;

                if (t1 > t2)
                {
                    var temp = t1;
                    t1 = t2;
                    t2 = temp;
                    s = 1.0f;
                }

                // Push the min up
                if (t1 > tmin)
                {
                    normal = new Vector2(0, s);
                    tmin = t1;
                }

                // Pull the max down
                tmax = MathF.Min(tmax, t2);

                if (tmin > tmax)
                    return null;
            }


            // Does the ray start inside the box?
            // Does the ray intersect beyond the max fraction?
            if (tmin < 0.0f || maxFraction < tmin)
            {
                return null;
            }

            // Intersection.
            surfaceNormal = normal;
            return tmin;
        }

        /// <summary>
        /// Pushes the bounds of the aabb in all 4 directions to the outside for the given distance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public Aabb Grow(float distance)
        {
            return new(new(MinCorner.X - distance, MinCorner.Y - distance), new(MaxCorner.X + distance, MaxCorner.Y + distance));
        }

        /// <summary>
        /// Grows an aabb by another aabb. This is only intuitive when `growth` has its origin on the inside (MinCorner is less or equal to Vector2.Zero).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Aabb Grow(Aabb growth)
        {
            return new(MinCorner + growth.MinCorner, MaxCorner + growth.MaxCorner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb operator -(Aabb a, Vector2 b)
        {
            return new(a.MinCorner - b, a.MaxCorner - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb operator +(Aabb a, Vector2 b)
        {
            return new(a.MinCorner + b, a.MaxCorner + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb operator +(Vector2 b, Aabb a)
        {
            return new(a.MinCorner + b, a.MaxCorner + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb operator *(Aabb a, float factor)
        {
            return new(a.MinCorner * factor, a.MaxCorner * factor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb operator /(Aabb a, float factor)
        {
            return new(a.MinCorner / factor, a.MaxCorner / factor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb operator /(Aabb a, Vector2 factor)
        {
            return new(a.MinCorner / factor.X, a.MaxCorner / factor.Y);
        }

        [JsonIgnore]
        public Vector2 Size => MaxCorner - MinCorner;

        [JsonIgnore]
        public Vector2 HalfSize => (MaxCorner - MinCorner) * 0.5f;

        [JsonIgnore]
        public Vector2 Center => (MinCorner + MaxCorner) * 0.5f;

        /// <summary>
        /// Topleft corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore]
        public Vector2 TopLeft => new (MinCorner.X, MaxCorner.Y);

        /// <summary>
        /// TopRight corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2 TopRight => MaxCorner;

        /// <summary>
        /// BottomLeft corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2 BottomLeft => MinCorner;

        /// <summary>
        /// BottomRight corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2 BottomRight => new(MaxCorner.X, MinCorner.Y);

        [JsonIgnore] public Vector2 CenterRight => new(MaxCorner.X, (MinCorner.Y + MaxCorner.Y) * 0.5f);
        [JsonIgnore] public Vector2 CenterLeft => new(MinCorner.X, (MinCorner.Y + MaxCorner.Y) * 0.5f);
        [JsonIgnore] public Vector2 CenterTop => new((MinCorner.X + MaxCorner.X) * 0.5f, MaxCorner.Y);
        [JsonIgnore] public Vector2 CenterBottom => new((MinCorner.X + MaxCorner.X) * 0.5f, MinCorner.Y);

        [JsonIgnore] public float Right => MaxCorner.X;
        [JsonIgnore] public float Left => MinCorner.X;
        [JsonIgnore] public float Top => MaxCorner.Y;
        [JsonIgnore] public float Bottom => MinCorner.Y;
        public float Width => MaxCorner.X - MinCorner.X;
        public float Height => MaxCorner.Y - MinCorner.Y;
        public float Perimeter => 2 * (MaxCorner.X - MinCorner.X + MaxCorner.Y - MinCorner.Y);

        public static Aabb One = new(Vector2.Zero, Vector2.One);

        public static Aabb Zero = new(Vector2.Zero, Vector2.Zero);

        public static Aabb FromSizeAroundCenter(Vector2 center, Vector2 size)
        {
            var halfSize = size * 0.5f;
            return new Aabb(center-halfSize, center + halfSize);
        }

        public static Aabb FromSizeAroundCenter(Vector2 size)
        {
            var halfSize = size * 0.5f;
            return new Aabb(-halfSize, halfSize);
        }

        /// <summary>
        /// Finds the axis aligned bounding box of a set of points.
        /// </summary>
        public static Aabb FromPoints(params Vector2[] points)
        {
            return FromPoints((IEnumerable<Vector2>)points);
        }

        /// <summary>
        /// Finds the axis aligned bounding box of a set of points.
        /// </summary>
        public static Aabb FromPoints(Vector2 a, Vector2 b)
        {
            return new Aabb(new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y)), new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y)));
        }

        /// <summary>
        /// Finds the axis aligned bounding box of a set of points.
        /// </summary>
        public static Aabb FromPoints(IEnumerable<Vector2> points)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var isEmpty = true;

            foreach (var p in points)
            {
                isEmpty = false;
                minX = Math.Min(p.X, minX);
                minY = Math.Min(p.Y, minY);
                maxX = Math.Max(p.X, maxX);
                maxY = Math.Max(p.Y, maxY);
            }

            if (isEmpty) throw new ArgumentException("Cannot get AABB of an empty point collection.", nameof(points));

            return new Aabb(new (minX, minY), new(maxX, maxY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Aabb Scale(Vector2 scale)
        {
            return new Aabb(MinCorner * scale, MaxCorner * scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(in Aabb b)
        {
            return MinCorner.X < b.MaxCorner.X && MaxCorner.X > b.MinCorner.X && MinCorner.Y < b.MaxCorner.Y && MaxCorner.Y > b.MinCorner.Y;
        }

        /// <summary>
        /// Enumerates all 4 corners of the <see cref="Aabb"/>.
        /// </summary>
        public IEnumerable<Vector2> GetCorners()
        {
            yield return TopLeft;
            yield return TopRight;
            yield return BottomRight;
            yield return BottomLeft;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Aabb Union(in Aabb a, in Aabb b)
        {
            return new(Vector2.Min(a.MinCorner, b.MinCorner), Vector2.Max(a.MaxCorner, b.MaxCorner));
        }

        public float GetArea()
        {
            return Size.X * Size.Y;
        }

        public override string ToString()
        {
            return $"<{MinCorner.X}.{MinCorner.Y} : {MaxCorner.X}.{MaxCorner.Y}>";
        }

        /// <summary>
        /// Grows this AABB by extending its sides with the opposite-side extents of 'other'. Typically used for Continuous Collision Detection.
        /// </summary>
        public Aabb MinkowskiSum(Aabb other)
        {
            return new(new(MinCorner.X - other.MaxCorner.X, MinCorner.Y - other.MaxCorner.Y),
                new(MaxCorner.X - other.MinCorner.X, MaxCorner.Y - other.MinCorner.Y));
        }

        /// <summary>
        /// Splits the aabb at X. Returns true and the split aabb's, or false if x lies outside.
        /// </summary>
        public bool SplitAtX(float x, out Aabb left, out Aabb right)
        {
            left = default;
            right = default;

            if (x <= Left || x >= Right) return false;

            left = new(new(Left, Bottom), new(x, Top));
            right = new(new(x, Bottom), new(Right, Top));
            return true;
        }
        
        /// <summary>
        /// Splits the aabb at Y. Returns true and the split aabb's, or false if y lies outside.
        /// </summary>
        public bool SplitAtY(float y, out Aabb bottom, out Aabb top)
        {
            bottom = default;
            top = default;

            if (y <= Bottom || y >= Top) return false;

            bottom = new(new(Left, Bottom), new(Right, y));
            top = new(new(Left, y), new(Right, Top));
            return true;
        }
    }
}
