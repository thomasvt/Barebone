using System.Diagnostics.Contracts;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Barebone.Geometry
{
    /// <summary>
    /// Axis Aligned Bounding Box. The positional properties (eg. Top, Bottom) all are in context of X+ is Right, and Y+ is Up.
    /// </summary>
    public struct AabbI(Vector2I minCorner, Vector2I maxCorner) : IEquatable<AabbI>
    {
        public AabbI(int minX, int minY, int maxXIncl, int maxYIncl) : this(new(minX, minY), new(maxXIncl, maxYIncl))
        {
        }

        public Vector2I MinCorner = minCorner;
        public Vector2I MaxCorner = maxCorner;

        [JsonIgnore] public Vector2I Size => MaxCorner - MinCorner + Vector2I.One;
        [JsonIgnore] public int Width => MaxCorner.X - MinCorner.X + 1;
        [JsonIgnore] public int Height => MaxCorner.Y - MinCorner.Y + 1;
        [JsonIgnore] public Vector2 Center => (MaxCorner + MinCorner) * 0.5f;
        [JsonIgnore] public Vector2I CenterI => MinCorner + Size / 2;
        /// <summary>
        /// Topleft corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I TopLeft => new(MinCorner.X, MaxCorner.Y);

        /// <summary>
        /// TopRight corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I TopRight => MaxCorner;

        /// <summary>
        /// BottomLeft corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I BottomLeft => MinCorner;

        /// <summary>
        /// BottomRight corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I BottomRight => new(MaxCorner.X, MinCorner.Y);
        [JsonIgnore] public int Top => MaxCorner.Y;
        [JsonIgnore] public int Right => MaxCorner.X;
        [JsonIgnore] public int Left => MinCorner.X;
        [JsonIgnore] public int Bottom => MinCorner.Y;

        public static AabbI Zero = new(Vector2I.Zero, Vector2I.Zero);

        public void ForEach(Action<Vector2I> action)
        {
            for (var y = MinCorner.Y; y < MaxCorner.Y; y++)
                for (var x = MinCorner.X; x < MaxCorner.X; x++)
                    action(new Vector2I(x, y));
        }

        public AabbI GetIntersection(in AabbI b)
        {
            var minX = Math.Max(MinCorner.X, b.MinCorner.X);
            var minY = Math.Max(MinCorner.Y, b.MinCorner.Y);
            var maxX = Math.Min(MaxCorner.X, b.MaxCorner.X);
            var maxY = Math.Min(MaxCorner.Y, b.MaxCorner.Y);

            if (minX >= maxX || minY >= maxY) 
                return AabbI.Zero;
            return new(new(minX, minY), new(maxX, maxY));
        }

        public readonly bool Contains(in Vector2I position)
        {
            return position.X >= MinCorner.X && position.Y >= MinCorner.Y && position.X < MaxCorner.X && position.Y < MaxCorner.Y;
        }

        public static AabbI FromSizeAroundCenter(in Vector2I size)
        {
            var halfSize = size / 2;
            return new AabbI(-halfSize, size - halfSize);
        }

        public static AabbI FromSizeAroundCenter(in Vector2I center, in Vector2I size)
        {
            var halfSize = size / 2;
            return new AabbI(center - halfSize, center + size - halfSize);
        }

        /// <summary>
        /// Returns an <see cref="AabbI"/> guaranteed to fit the original <see cref="Aabb"/> by rounding MinCorner down and MaxCorner up to the nearest integer values.
        /// </summary>
        public static AabbI FromAabb(in Aabb aabb)
        {
            return new AabbI(aabb.MinCorner.Floor(), aabb.MaxCorner.Ceiling());
        }

        /// <summary>
        /// Returns an <see cref="AabbI"/> with MinCorner (0,0) and the given size as MaxCornerExcl.
        /// </summary>
        public static AabbI FromSize(in Vector2I size)
        {
            return new AabbI(Vector2I.Zero, size - Vector2I.One);
        }

        /// <summary>
        /// Returns an <see cref="AabbI"/> with given minCorner and size.
        /// </summary>
        public static AabbI FromSize(in Vector2I minCorner, in Vector2I size)
        {
            return new AabbI(minCorner, minCorner + size - Vector2I.One);
        }

        /// <summary>
        /// Returns the AAbbI that includes both a and b
        /// </summary>
        public static AabbI FromPoints(in Vector2I a, in Vector2I b)
        {
            return new AabbI(new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
        }

        public static AabbI FromPoints(IEnumerable<Vector2I> points)
        {
            int? minX = null;
            int? minY = null;
            int? maxX = null;
            int? maxY = null;
            foreach (var p in points)
            {
                if (!minX.HasValue || p.X < minX) minX = p.X;
                if (!minY.HasValue || p.Y < minY) minY = p.Y;
                if (!maxX.HasValue || p.X > maxX) maxX = p.X;
                if (!maxY.HasValue || p.Y > maxY) maxY = p.Y;
            }

            if (minX == null) throw new ArgumentException("Cannot create AabbI from 0 points.");

            return new AabbI(new(minX.Value, minY!.Value), new(maxX!.Value, maxY!.Value));
        }

        public static AabbI operator +(AabbI a, Vector2I b)
        {
            return new(a.MinCorner + b, a.MaxCorner + b);
        }

        public static AabbI operator +(Vector2I b, AabbI a)
        {
            return new(a.MinCorner + b, a.MaxCorner + b);
        }

        public static bool operator ==(AabbI a, AabbI b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(AabbI a, AabbI b)
        {
            return !a.Equals(b);
        }

        public bool Equals(AabbI other)
        {
            return MinCorner.Equals(other.MinCorner) && MaxCorner.Equals(other.MaxCorner);
        }

        public override bool Equals(object? obj)
        {
            return obj is AabbI other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MinCorner, MaxCorner);
        }

        [Pure]
        public AabbI Grow(int amount)
        {
            var minX = MinCorner.X - amount;
            var minY = MinCorner.Y - amount;
            var maxX = Math.Max(MaxCorner.X + amount, minX);
            var maxY = Math.Max(MaxCorner.Y + amount, minY);
            return new(minX, minY, maxX, maxY);
        }

        public override string ToString()
        {
            return $"[{MinCorner.X}.{MinCorner.Y} : {MaxCorner.X}.{MaxCorner.Y})"; ;
        }
    }
}
