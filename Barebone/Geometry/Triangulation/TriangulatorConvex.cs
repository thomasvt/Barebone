using BareBone.Geometry.Triangulation;

namespace Barebone.Geometry.Triangulation
{
    public static class TriangulatorConvex
    {
        /// <summary>
        /// Use GetTriangleCount() first to know the size of the span. 
        /// </summary>
        public static void Triangulate(int cornerCount, Span<IndexTriangle> triangleBuffer)
        {
            triangleBuffer.Clear();

            var a = 0;
            var b = 1;
            for (var i = 0; i < cornerCount - 2; i++)
            {
                var c = i + 2;
                triangleBuffer[i] = new IndexTriangle(a, b, c);
                b = c;
            }
        }

        public static int GetTriangleCount(int cornerCount)
        {
            return cornerCount - 2;
        }
    }
}
