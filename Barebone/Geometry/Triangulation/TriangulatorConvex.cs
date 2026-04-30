using System.Numerics;

namespace Barebone.Geometry.Triangulation
{
    public static class TriangulatorConvex
    {
        /// <summary>
        /// Use GetTriangleCount() first to know the size of the span. 
        /// </summary>
        public static void Triangulate(ReadOnlySpan<Vector2> corners, Span<Triangle2> triangleBuffer)
        {
            triangleBuffer.Clear();

            var a = corners[0];
            var b = corners[1];
            for (var i = 0; i < corners.Length - 2; i++)
            {
                var c = corners[i + 2];
                triangleBuffer[i] = new Triangle2(a, b, c);
                b = c;
            }
        }

        public static int GetTriangleCount(int cornerCount)
        {
            return cornerCount - 2;
        }
    }
}
