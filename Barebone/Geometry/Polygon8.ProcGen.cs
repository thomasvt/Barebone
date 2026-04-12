using System.Numerics;

namespace Barebone.Geometry
{
    public unsafe partial struct Polygon8
    {
        public static Polygon8 Square(float size)
        {
            var hs = size * 0.5f;
            return new Polygon8(new(-hs, -hs), new(-hs, hs), new(hs, hs), new(hs, -hs));
        }

        public static Polygon8 Rectangle(in Vector2 minCorner, in Vector2 size)
        {
            var minX = minCorner.X;
            var minY = minCorner.Y;
            var maxX = minX + size.X;
            var maxY = minY + size.Y;
            return new Polygon8(new(minX, minY), new(minX, maxY), new(maxX, maxY), new(maxX, minY));
        }

        public static Polygon8 Rectangle(in Aabb aabb)
        {
            var minX = aabb.MinCorner.X;
            var minY = aabb.MinCorner.Y;
            var maxX = aabb.MaxCorner.X;
            var maxY = aabb.MaxCorner.Y;
            return new Polygon8(new(minX, minY), new(minX, maxY), new(maxX, maxY), new(maxX, minY));
        }

        /// <summary>
        /// Generates a polygon representing a line with given width and caps.
        /// </summary>
        public static Polygon8 Line(Vector2 a, Vector2 b, float width, LineCap lineCap)
        {
            var ab = b - a;

            var longi = (a == b ? new Vector2(1, 0) : Vector2.Normalize(ab)) * (width * 0.5f);
            var latti = longi.CrossLeft();

            switch (lineCap)
            {
                case LineCap.Square: return new(a - longi - latti, a - longi + latti, b + longi + latti, b + longi - latti);
                case LineCap.Butt: return new(a - latti, a + latti, b + latti, b - latti);
                case LineCap.Round:
                {
                    // this adds a few more points to have somewhat of a rounded cap.
                    var lattiHalf = latti * 0.5f;
                    var longiHalf = longi * 0.5f;
                    return new(a - latti - longiHalf, a - longi - lattiHalf, a - longi + lattiHalf, a + latti - longiHalf, b + latti + longiHalf, b + longi + lattiHalf, b + longi - lattiHalf,
                        b - latti + longiHalf);
                }
                default: throw new ArgumentOutOfRangeException(nameof(lineCap), lineCap, null);
            }
        }
    }
}
