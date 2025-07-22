using System.Numerics;

namespace Barebone.Geometry
{
    public readonly struct Triangle2
    {
        public readonly Vector2 A, B, C;

        public Triangle2(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Triangle2(Span<Vector2> corners)
        {
            if (corners.Length != 3)
                throw new Exception("A triangle must have 3 corners.");

            A = corners[0];
            B = corners[1];
            C = corners[2];
        }

        /// <summary>
        /// Checks if a point p is inside this triangle.
        /// </summary>
        public bool Contains(Vector2 p)
        {
            // this is not ideal when you want the bottom and right edges of triangles to be exclusive.
            return Edge2.IsInRightHalfSpace(p, A, B)
                   && Edge2.IsInRightHalfSpace(p, B, C)
                   && Edge2.IsInRightHalfSpace(p, C, A);
        }

        /// <summary>
        /// Checks if a point p is inside triangle abc.
        /// </summary>
        public static bool Contains(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            // this is not ideal when you want the bottom and right edges of triangles to be exclusive.
            return Edge2.IsInRightHalfSpace(p, a, b)
                   && Edge2.IsInRightHalfSpace(p, b, c)
                   && Edge2.IsInRightHalfSpace(p, c, a);
        }

        /// <summary>
        /// Returns 2x the size of the area enclosed by the polygon. Don't add the closing vertex twice.
        /// With X+ pointing right and Y+ pointing up: positive result means CW, negative means CCW
        /// </summary>
        public float GetSignedAreaDoubled()
        {
            // altered version of the shoelace algorithm
            // we need to cast to double because needle-like corners will cause float precision errors, double is 10-based.

            var sum = 0.0;

            var a = A;
            var b = B;
            sum += (b.X - (double)a.X) * (b.Y + (double)a.Y);

            a = B;
            b = C;
            sum += (b.X - (double)a.X) * (b.Y + (double)a.Y);

            a = C;
            b = A;
            sum += (b.X - (double)a.X) * (b.Y + (double)a.Y);

            return (float)sum;
        }
    }
}
