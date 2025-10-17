using System.Numerics;
using System.Text.Json.Serialization;

namespace Barebone.Geometry
{
    /// <summary>
    /// Axis Aligned Bounding Box. The positional properties (eg. Top, Bottom) all are in context of X+ is Right, and Y+ is Up.
    /// </summary>
    public struct AabbI(Vector2I minCorner, Vector2I maxCornerExcl) : IEquatable<AabbI>
    {
        public AabbI(int minX, int minY, int maxX, int maxY) : this(new(minX, minY), new(maxX, maxY))
        {
        }

        public Vector2I MinCorner = minCorner;
        public Vector2I MaxCornerExcl = maxCornerExcl;

        [JsonIgnore] public Vector2I Size => MaxCornerExcl - MinCorner;
        [JsonIgnore] public int Width => MaxCornerExcl.X - MinCorner.X;
        [JsonIgnore] public int Height => MaxCornerExcl.Y - MinCorner.Y;
        [JsonIgnore] public Vector2 Center => (MaxCornerExcl + MinCorner) * 0.5f;
        [JsonIgnore] public Vector2I CenterI => MinCorner + Size / 2;
        /// <summary>
        /// Topleft corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I TopLeftExcl => new(MinCorner.X, MaxCornerExcl.Y);

        /// <summary>
        /// TopRight corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I TopRightExcl => MaxCornerExcl;

        /// <summary>
        /// BottomLeft corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I BottomLeft => MinCorner;

        /// <summary>
        /// BottomRight corner of the aabb when X+ is right and Y+ is up.
        /// </summary>
        [JsonIgnore] public Vector2I BottomRightExcl => new(MaxCornerExcl.X, MinCorner.Y);
        [JsonIgnore] public int Top => MaxCornerExcl.Y - 1;
        [JsonIgnore] public int Right => MaxCornerExcl.X - 1;
        [JsonIgnore] public int Left => MinCorner.X;
        [JsonIgnore] public int Bottom => MinCorner.Y;
        [JsonIgnore] public Vector2I MaxCorner => MaxCornerExcl - Vector2I.One;

        public static AabbI Zero = new(Vector2I.Zero, Vector2I.Zero);

        public void ForEach(Action<Vector2I> action)
        {
            for (var y = MinCorner.Y; y < MaxCornerExcl.Y; y++)
                for (var x = MinCorner.X; x < MaxCornerExcl.X; x++)
                    action(new Vector2I(x, y));
        }

        public AabbI GetIntersection(AabbI b)
        {
            var minX = Math.Max(MinCorner.X, b.MinCorner.X);
            var minY = Math.Max(MinCorner.Y, b.MinCorner.Y);
            var maxX = Math.Min(MaxCornerExcl.X, b.MaxCornerExcl.X);
            var maxY = Math.Min(MaxCornerExcl.Y, b.MaxCornerExcl.Y);

            if (minX >= maxX || minY >= maxY) 
                return AabbI.Zero;
            return new(new(minX, minY), new(maxX, maxY));
        }

        public readonly bool Contains(Vector2I position)
        {
            return position.X >= MinCorner.X && position.Y >= MinCorner.Y && position.X < MaxCornerExcl.X && position.Y < MaxCornerExcl.Y;
        }

        public static AabbI FromSizeAroundCenter(Vector2I size)
        {
            var halfSize = size / 2;
            return new AabbI(-halfSize, size - halfSize);
        }

        public static AabbI FromSizeAroundCenter(Vector2I center, Vector2I size)
        {
            var halfSize = size / 2;
            return new AabbI(center - halfSize, center + size - halfSize);
        }

        /// <summary>
        /// Returns an <see cref="AabbI"/> guaranteed to fit the original <see cref="Aabb"/> by rounding MinCorner down and MaxCorner up to the nearest integer values.
        /// </summary>
        public static AabbI FromAabb(Aabb aabb)
        {
            return new AabbI(aabb.MinCorner.Floor(), aabb.MaxCorner.Ceiling());
        }

        /// <summary>
        /// Returns an <see cref="AabbI"/> with MinCorner (0,0) and the given size as MaxCornerExcl.
        /// </summary>
        public static AabbI FromSize(Vector2I size)
        {
            return new AabbI(Vector2I.Zero, size);
        }

        /// <summary>
        /// Returns an <see cref="AabbI"/> with given minCorner and size.
        /// </summary>
        public static AabbI FromSize(Vector2I minCorner, Vector2I size)
        {
            return new AabbI(minCorner, minCorner + size);
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

            return new AabbI(new(minX.Value, minY!.Value), new(maxX!.Value + 1, maxY!.Value + 1));
        }

        public static AabbI operator +(AabbI a, Vector2I b)
        {
            return new(a.MinCorner + b, a.MaxCornerExcl + b);
        }

        public static AabbI operator +(Vector2I b, AabbI a)
        {
            return new(a.MinCorner + b, a.MaxCornerExcl + b);
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
            return MinCorner.Equals(other.MinCorner) && MaxCornerExcl.Equals(other.MaxCornerExcl);
        }

        public override bool Equals(object? obj)
        {
            return obj is AabbI other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MinCorner, MaxCornerExcl);
        }
    }
}
